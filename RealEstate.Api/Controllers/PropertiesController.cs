using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using RealEstate.Application.Abstractions.Messaging;
using RealEstate.Application.Contracts.Commons;
using RealEstate.Application.Contracts.Files;
using RealEstate.Application.Contracts.Properties;
using RealEstate.Application.UseCases.AddPropertyImage;
using RealEstate.Application.UseCases.ChangePrice;
using RealEstate.Application.UseCases.CreatePropertyBuilding;
using RealEstate.Application.UseCases.GetPropertyDetails;
using RealEstate.Application.UseCases.GetPropertyList;
using RealEstate.Application.UseCases.UpdateProperty;

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RealEstate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertiesController : ControllerBase
    {
        private readonly IQueryHandler<GetPropertyDetailsQuery, PropertyDetailDto?> _getPropertyDetailQueryHandler;
        private readonly IQueryHandler<GetPropertyListQuery, PagedDtos<PropertySummaryDto>> _getPropertiesListQueryHandler;
        private readonly ICommandHandler<ChangePriceCommand, bool> _changePriceCommandHandler;
        private readonly ICommandHandler<CreatePropertyBuildingCommand, PropertyDetailDto> _createPropertyBuildingHandler;
        ICommandHandler<UpdatePropertyCommand, bool> _updatePropertyHandler;
        private readonly ICommandHandler<AddPropertyImageCommand, PropertyImageDto> _addPropertyImageHandler;
        private readonly ILogger<PropertiesController> _logger;


        public PropertiesController(
            IQueryHandler<GetPropertyDetailsQuery, PropertyDetailDto> getPropertyDetailQueryHandler,
            IQueryHandler<GetPropertyListQuery, PagedDtos<PropertySummaryDto>> getPropertiesListQueryHandler,
            ICommandHandler<ChangePriceCommand, bool> changePriceCommandHandler,
            ICommandHandler<CreatePropertyBuildingCommand, PropertyDetailDto> createPropertyBuildingHandler,
            ICommandHandler<UpdatePropertyCommand, bool> updatePropertyHandler,
            ICommandHandler<AddPropertyImageCommand, PropertyImageDto> addPropertyImageHandler,
            ILogger<PropertiesController> logger)
        {
            _getPropertyDetailQueryHandler = getPropertyDetailQueryHandler;
            _getPropertiesListQueryHandler = getPropertiesListQueryHandler;
            _changePriceCommandHandler = changePriceCommandHandler;
            _createPropertyBuildingHandler = createPropertyBuildingHandler;
            _updatePropertyHandler = updatePropertyHandler;
            _addPropertyImageHandler = addPropertyImageHandler;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene el detalle de una propiedad por su identificador.
        /// </summary>
        /// <param name="id">Identificador de la propiedad.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>200 con el detalle de la propiedad, o 404 si no existe.</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PropertyDetailDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var query = new GetPropertyDetailsQuery { PropertyId = id };

            var dto = await _getPropertyDetailQueryHandler.Handle(query, cancellationToken);
            return dto is null ? NotFound() : Ok(dto);
        }

        /// <summary>
        /// Obtiene una lista paginada de propiedades según los filtros proporcionados.
        /// </summary>
        /// <param name="filters">Filtros de búsqueda y paginación.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>200 con la lista paginada de propiedades.</returns>
        [HttpGet("")]
        public async Task<IActionResult> Get(
            [FromQuery] GetPropertyListQuery filters,
            CancellationToken cancellationToken)
        {
            var page = await _getPropertiesListQueryHandler.Handle(filters, cancellationToken);
            return Ok(page);
        }

        /// <summary>
        /// Cambia el precio de una propiedad existente.
        /// </summary>
        /// <param name="id">Identificador de la propiedad.</param>
        /// <param name="command">Comando con el nuevo precio.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>204 si se actualizó, 400 si hay error de validación, 404 si no existe.</returns>
        [HttpPatch("{id:int}/price")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ChangePrice(
            int id,
            [FromBody] ChangePriceCommand command,
            CancellationToken cancellationToken)
        {
            if (id != command.IdProperty)
                return BadRequest(new { error = "Id in URL and payload must be the same" });

            if (command.NewPrice <= 0)
                return BadRequest(new { error = "NewPrice must be > 0" });

            var ok = await _changePriceCommandHandler.Handle(
                command,
                cancellationToken);

            return ok ? NoContent() : NotFound();
        }

        /// <summary>
        /// Crea una nueva propiedad.
        /// </summary>
        /// <param name="command">Comando con los datos de la propiedad.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>201 con el detalle de la propiedad creada, 400 si hay error de validación, 404 si el propietario no existe.</returns>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Create(
                   [FromBody] CreatePropertyBuildingCommand command,
                   CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.Name)) return BadRequest("Name required");
            if (command.Address == null) return BadRequest("Address required");
            if (command.Price <= 0) return BadRequest(nameof(command.Price));
            if (command.IdOwner <= 0) return BadRequest(nameof(command.IdOwner));

            try
            {
                var dto = await _createPropertyBuildingHandler.Handle(command, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id = dto.IdProperty }, dto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza los datos de una propiedad existente.
        /// </summary>
        /// <param name="id">Identificador de la propiedad.</param>
        /// <param name="command">Comando con los datos actualizados.</param>
        /// <param name="cancelationToken">Token de cancelación.</param>
        /// <returns>204 si se actualizó, 400 si hay error de validación, 404 si no existe.</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdatePropertyCommand command,
            CancellationToken cancelationToken)
        {
            if (id != command.IdProperty)
                return BadRequest(new { error = "Id in URL and payload must be the same" });

            if (string.IsNullOrWhiteSpace(command.Name)) return BadRequest("Name required");
            if (command.Price <= 0) return BadRequest(nameof(command.Price));
            if (command.IdOwner <= 0) return BadRequest(nameof(command.IdOwner));

            try
            {
                var ok = await _updatePropertyHandler.Handle(command, cancelationToken);
                return ok ? NoContent() : NotFound();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Agrega una imagen a una propiedad.
        /// </summary>
        /// <param name="id">Identificador de la propiedad.</param>
        /// <param name="image">Archivo de imagen a subir.</param>
        /// <param name="cancelationToken">Token de cancelación.</param>
        /// <returns>201 con los datos de la imagen, 400 si no se envía imagen, 404 si la propiedad no existe.</returns>
        [HttpPost("{id:int}/images")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(PropertyImageDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AddImageForm(
            int id,
            IFormFile image,
            CancellationToken cancelationToken)
        {
            if (image is null || image.Length == 0)
                return BadRequest("Image required");

            using var ms = new MemoryStream();
            await image.CopyToAsync(ms, cancelationToken);
            var upload = new ImageUpload { FileName = image.FileName, ContentType = image.ContentType, Content = ms.ToArray() };

            var command = new AddPropertyImageCommand
            {
                IdProperty = id,
                Image = upload,
                Enabled = true
            };


            var dto = await _addPropertyImageHandler.Handle(command, cancelationToken);
            return CreatedAtAction(nameof(GetById), new { id = dto.IdProperty }, dto);
        }
    }
}

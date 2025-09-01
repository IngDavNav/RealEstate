# RealEstate API (.NET 8)

API para una empresa inmobiliaria de gran tamaño que consulta información de propiedades en Estados Unidos a partir de una base de datos existente. Expone servicios para **listar con filtros y paginación**, **consultar detalle con dueño/imagenes/trazas**, **actualizar precio con historial** y **gestionar imágenes vía URL**.

## Tabla de contenido

* [Arquitectura](#arquitectura)
* [Stack técnico](#stack-técnico)
* [Estructura de la solución](#estructura-de-la-solución)
* [Arranque rápido](#arranque-rápido)
* [Configuración de base de datos](#configuración-de-base-de-datos)
* [Seed de datos](#seed-de-datos)
* [Endpoints](#endpoints)
* [Manejo de imágenes](#manejo-de-imágenes)
* [Pruebas](#pruebas)
* [Rendimiento](#rendimiento)
* [ADRs (resumen)](#adrs-resumen)
* [Roadmap](#roadmap)
* [Licencia](#licencia)

---

## Arquitectura

* **DDD + CQRS ligero**: dominio rico (entidades y *Value Objects* para direcciones), capa de aplicación con *handlers* (commands/queries), infraestructura con EF Core y repositorios, API delgada.
* **Unit of Work** sobre `DbContextTransaction` para operaciones consistentes (p. ej., actualizar propiedad + registrar traza de precio).
* **EF Core** con *Value Objects* de dirección mapeados en la **misma tabla** (owned types) y `DateOnly` para fechas sin tiempo.
* **Imágenes**: el detalle devuelve **URLs públicas** (no bytes). Los bytes se sirven como archivos estáticos (o endpoint dedicado).

## Stack técnico

* .NET 8, ASP.NET Core Web API
* EF Core (SqlServer), AutoMapper
* Swagger/OpenAPI
* xUnit + FluentAssertions + Moq
* SQL Server (contenedor Docker)

## Estructura de la solución

```
RealEstate.sln
 ├─ RealEstate.Api                // ASP.NET Core Web API
 ├─ RealEstate.Application        // Casos de uso, CQRS, DTOs, perfiles AutoMapper
 ├─ RealEstate.Domain             // Entidades y Value Objects
 ├─ RealEstate.Infrastructure     // EF Core, Repositorios, DbContext, Unit of Work, Storage
 └─ RealEstate.Tests              // Tests de controladores, handlers y repositorios
```

---

## Arranque rápido

### Requisitos

* .NET 8 SDK
* Docker (para SQL Server local)
* PowerShell/CMD o bash

### 1) Levanta SQL Server en Docker

```bash
docker run -d --name realestate-sql -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=PassParaDev123*" \
  -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
```

Crea usuario de app (opcional si ya existe):

```bash
docker exec -i realestate-sql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "PassParaDev123*" -Q "
IF DB_ID('RealEstateDb') IS NULL CREATE DATABASE RealEstateDb;
IF NOT EXISTS (SELECT 1 FROM sys.sql_logins WHERE name='RealStateApp')
    CREATE LOGIN [RealStateApp] WITH PASSWORD='PassParaDev123*', CHECK_POLICY=OFF, CHECK_EXPIRATION=OFF;
USE RealEstateDb;
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name='RealStateApp')
    CREATE USER [RealStateApp] FOR LOGIN [RealStateApp];
EXEC sp_addrolemember 'db_owner','RealStateApp';
"
```

### 2) Configura `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=RealEstateDb;User Id=RealStateApp;Password=PassParaDev123*;Encrypt=false;TrustServerCertificate=true;MultipleActiveResultSets=true"
  }
}
```

### 3) Restaura, compila y ejecuta

```bash
dotnet restore
dotnet build
dotnet run --project RealEstate.Api
```

Swagger: `https://localhost:63023/swagger`

---

## Configuración de base de datos

El proyecto usa **EnsureCreated()** para crear el esquema rápidamente en entornos de desarrollo.

* Si el usuario no tiene permisos para crear la base, crea `RealEstateDb` con `sa` y otorga `db_owner` al usuario de la app.
* Índices clave:

  * `Property(Price)` para rangos `MinPrice/MaxPrice`.
  * `Property(Year)` para filtros puntuales y ordenaciones.
  * Compuesto `Property(Address.City, Address.State)` para búsquedas por ubicación.
  * `PropertyImage(IdProperty, Enabled)` para listar imágenes activas por propiedad.
  * `PropertyTrace(IdProperty, DateSale)` para leer histórico ordenado por fecha.

---

## Seed de datos

Hay un *seeder* con datos de Miami.

En `Program.cs`:

```csharp
await app.Services.CreateAndSeedAsync(reset: false); // pon true la 1ª vez para recrear todo
```

* `reset: true` → borra y crea la DB desde cero y carga datos (solo local/primera vez).
* `reset: false` → asegura esquema y evita duplicados (idempotente).

---

## Endpoints

### Propiedades

* `GET /api/properties`

  * **Query**: `page`, `pageSize`, `address.street|city|state|zipCode`, `minPrice`, `maxPrice`, `year`
  * **200** `PagedDtos<PropertySummaryDto>`

* `GET /api/properties/{id}`

  * **200** `PropertyDetailDto` (incluye `Owner`, `Address`, `Images` con **Url**, `Traces`)
  * **404** si no existe

* `POST /api/properties`

  * **Body (JSON)**:

    ```json
    {
      "name": "Brickell High-Rise 2BR",
      "address": { "street": "68 SE 6th St", "city": "Miami", "state": "FL", "zipCode": "33131" },
      "price": 850000,
      "codeInternal": "BRK-2BR-6806",
      "year": 2019,
      "idOwner": 1,
      "createInitialTrace": true,
      "initialTraceName": "Property created",
      "initialTax": 0
    }
    ```
  * **201** `Location: /api/properties/{id}` + DTO creado

* `PUT /api/properties/{id}`

  * **Body**: igual al create, con `idProperty`
  * **204** (No Content), o **404** si no existe

* `PATCH /api/properties/{id}/price`

  * **Body**:

    ```json
    { "idProperty": 1, "newPrice": 860000 }
    ```
  * **204** o **404**

### Imágenes

* `POST /api/properties/{id}/images` (**multipart/form-data**)

  * Campo: `image` como archivo
  * **201** `PropertyImageDto` (con `Url`)

* `POST /api/properties/{id}/images` (**application/json**)

  * **Body**:

    ```json
    {
      "fileName": "front.jpg",
      "contentType": "image/jpeg",
      "contentBase64": "/9j/4AAQSkZJRgABAQAAAQABAAD...BASE64...",
      "enabled": true
    }
    ```

* `GET /api/properties/{id}/images`

  * **200** `IEnumerable<PropertyImageDto>` (cada item con `Url`)

* `GET /api/properties/{id}/images/{imageId}/content` *(opcional, bytes)*

  * **200** `image/*` (sirve archivo físico con cache cliente)

**cURL de ejemplo (crear):**

```bash
curl -k -X POST "https://localhost:63023/api/properties" \
 -H "Content-Type: application/json" \
 -d '{ "name":"Wynwood Loft 1BR", "address":{"street":"2400 NW 2nd Ave","city":"Miami","state":"FL","zipCode":"33127"}, "price":520000, "codeInternal":"WYN-LOFT-2400", "year":2016, "idOwner":1, "createInitialTrace":true, "initialTraceName":"Property created", "initialTax":0 }'
```

**cURL (subir imagen multipart):**

```bash
curl -k -X POST "https://localhost:63023/api/properties/1/images" \
 -H "Accept: application/json" \
 -F "image=@/ruta/front.jpg;type=image/jpeg"
```

---

## Manejo de imágenes

* Se guardan en el sistema de archivos local bajo `wwwroot/uploads/properties/{id}/...`.
* El DTO devuelve **URL pública** construida por `IImageUrlBuilder`.
* `app.UseStaticFiles()` habilita la entrega estática.
* Fácil de migrar a S3/Azure: cambia la implementación de `IImageStorage`/`IImageUrlBuilder`.

---

## Pruebas

* **Framework**: xUnit + FluentAssertions + Moq + `Microsoft.NET.Test.Sdk` + `xunit.runner.visualstudio`.
* **Cobertura**:

  * *Handlers*: validaciones, flujos felices, efectos colaterales (trazas).
  * *Repositorios*: filtros, paginación y orden estable (SQLite in-memory).
  * *Controladores*: rutas, códigos 201/404/400, `CreatedAtAction`, *model binding*.

Instalación de paquetes (si hiciera falta):

```bash
dotnet add RealEstate.Tests package Microsoft.NET.Test.Sdk
dotnet add RealEstate.Tests package xunit
dotnet add RealEstate.Tests package xunit.runner.visualstudio
dotnet add RealEstate.Tests package FluentAssertions
dotnet add RealEstate.Tests package Moq
```

Ejecutar:

```bash
dotnet test RealEstate.Tests -v n
```

> Nota: evita adjuntar dos instancias con la misma PK en el mismo `DbContext` en seeds/tests. Usa solo FK o reutiliza la instancia (`ChangeTracker.Clear()` ayuda entre etapas).

---

## Rendimiento

* Lecturas con `AsNoTracking()`.
* Paginación en servidor (`Skip/Take`) y filtros que aprovechan índices.
* Detalle en un solo viaje (`AsSingleQuery()`); considerar `AsSplitQuery()` si las colecciones crecen mucho.
* No enviar bytes de imágenes en JSON; usar URLs cacheables.
* Logging de latencias en los handlers para detectar regresiones.

---

## ADRs (resumen)

* **ADR-001**: DDD + CQRS ligero para claridad y testeabilidad.
* **ADR-002**: EF Core (SqlServer); direcciones como Value Objects en la misma tabla; `DateOnly`.
* **ADR-003**: Imágenes por URL en JSON; bytes por endpoint/estático.
* **ADR-004**: Unit of Work con transacciones explícitas (o `CreateExecutionStrategy` si hay reintentos).
* **ADR-005**: Sin migraciones en la prueba; `EnsureCreated()` + seed.
* **ADR-006**: Orden estable: trazas por fecha desc; imágenes por id (o `ImageOrder` futura).

---

## Roadmap

* Autenticación JWT + autorización por rol.
* Ordenamiento configurable en listados.
* `ImageOrder`, miniaturas y CDN/S3 con URLs firmadas.
* Migraciones EF + versionado de esquema; FluentValidation.
* Observabilidad: OpenTelemetry (traces/métricas).

---

## Licencia

MIT (o la que prefieras).

# EventMarketplace API

EventMarketplace is a .NET 9 Clean Architecture Web API for listing and managing events.

## Architecture

- EventMarketplace.Domain
- EventMarketplace.Application
- EventMarketplace.Infrastructure
- EventMarketplace.API

## Tech Stack

- ASP.NET Core 9
- EF Core 9 + Pomelo MySQL
- ASP.NET Identity
- JWT Access + Refresh Token
- MediatR (CQRS)
- FluentValidation
- Serilog
- Swagger/OpenAPI

## Custom API Response Format

All API endpoints return a unified response envelope.

```json
{
  "data": {},
  "statusCode": 200,
  "error": null,
  "isSuccessful": true
}
```

Validation/Error example:

```json
{
  "data": null,
  "statusCode": 400,
  "error": {
    "errors": [
      "Title: 'Title' must not be empty."
    ],
    "isShow": true
  },
  "isSuccessful": false
}
```

## Main Endpoints

- POST /api/auth/register
- POST /api/auth/login
- POST /api/auth/refresh
- GET /api/events
- GET /api/events/{id}
- POST /api/events (Organizer role required)

## Authentication

Use JWT Bearer token in Authorization header:

```text
Authorization: Bearer <access_token>
```

Swagger is configured with Bearer security definition.

## Run

1. Restore and build

```powershell
dotnet restore
dotnet build
```

2. Update database

```powershell
dotnet ef database update -p EventMarketplace.Infrastructure -s EventMarketplace.API
```

3. Start API

```powershell
dotnet run --project EventMarketplace.API
```

## Seed Data

- Default Admin: admin@eventmarketplace.com / Admin123!
- Default roles: Admin, Organizer
- Default categories: Concert, Theatre, Festival
- Development seeder adds fake events when needed.

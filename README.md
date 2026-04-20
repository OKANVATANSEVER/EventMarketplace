# EventMarketplace API

EventMarketplace is a .NET 9 Clean Architecture Web API for event discovery and management.

## Architecture

- EventMarketplace.Domain
- EventMarketplace.Application
- EventMarketplace.Infrastructure
- EventMarketplace.API

## Tech Stack

- ASP.NET Core 9
- EF Core 9 + Pomelo MySQL
- ASP.NET Identity + JWT Access/Refresh Token
- MediatR (CQRS)
- FluentValidation
- Serilog
- OpenTelemetry (ASP.NET Core, HttpClient, EF Core)
- Swagger/OpenAPI

## Standard API Response

All endpoints return a unified envelope:

```json
{
  "data": {},
  "statusCode": 200,
  "error": null,
  "isSuccessful": true
}
```

Error response example:

```json
{
  "data": null,
  "statusCode": 400,
  "error": {
    "errors": ["Title: 'Title' must not be empty."],
    "isShow": true
  },
  "isSuccessful": false
}
```

## Main Endpoints

- POST /api/auth/register
- POST /api/auth/login
- POST /api/auth/refresh
- POST /api/auth/revoke
- GET /api/events
- GET /api/events/{id}
- POST /api/events (Organizer role required)

## Example API Calls

Swagger UI: http://localhost:5105/swagger

Sample login request:

```http
POST /api/auth/login HTTP/1.1
Content-Type: application/json

{
  "email": "your-admin-or-user-email",
  "password": "your-password"
}
```

Sample filtered event query:

```http
GET /api/events?categoryId=<guid>&city=Istanbul&isFeatured=true&page=1&pageSize=10 HTTP/1.1
```

Postman collection:

- docs/postman/EventMarketplace.postman_collection.json

## Authentication

Use JWT bearer tokens:

```text
Authorization: Bearer <access_token>
```

Swagger is configured with Bearer security definition and requirement.

## Local Run

1. Restore and build

```powershell
dotnet restore
dotnet build
```

2. Apply migrations

```powershell
dotnet ef database update -p EventMarketplace.Infrastructure -s EventMarketplace.API
```

3. Run API

```powershell
dotnet run --project EventMarketplace.API
```

## Deployment

### Docker Compose

1. Copy `.env.example` to `.env` and set `JWT_SECRET_KEY`
2. Start stack:

```powershell
docker compose up -d --build
```

3. API will be available at http://localhost:8080

### Cloud Targets

- Azure: App Service + Azure Database for MySQL
- AWS: ECS/Fargate + RDS MySQL
- GCP: Cloud Run + Cloud SQL MySQL

Use environment variables for all secrets and connection strings.

## Testing

- Unit tests: `EventMarketplace.Application.Tests`
- Integration tests: `EventMarketplace.API.IntegrationTests`

Run all tests:

```powershell
dotnet test EventMarketplace.sln
```

## CI/CD and Code Quality

- CI workflow: `.github/workflows/ci.yml`
- CD workflow: `.github/workflows/cd.yml`
- CodeQL workflow: `.github/workflows/codeql.yml`

## Security Notes

- Do not keep default credentials in production.
- Use secret providers (`dotnet user-secrets`, environment variables, cloud key vaults).
- Auth endpoints are rate-limited.
- Refresh token rotation and explicit revoke endpoint are enabled.

## Project Management

- Roadmap: `ROADMAP.md`
- Release notes: `CHANGELOG.md`
- Issue templates: `.github/ISSUE_TEMPLATE/`
- Pull request template: `.github/pull_request_template.md`

## Versioning

This project follows Semantic Versioning (`vMAJOR.MINOR.PATCH`).

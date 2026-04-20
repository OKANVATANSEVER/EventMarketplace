# Changelog

All notable changes to this project are documented here.

## [0.2.1] - 2026-04-20
### Added
- API endpoint: `GET /api/admin/members`.
- Application query/handler pair for member listing in admin workflows.
- Remember Me support in Web login flow.

### Changed
- Web app root route now points to event listing (`/`).
- Web authentication/navigation flow stabilized for admin redirects.
- Blazor pages updated to interactive server render mode where needed.
- API token model mapping in Web aligned with API auth response contract.

### Fixed
- `HttpClient` base address resolution issue caused by duplicate `ApiClient` registration.
- Premature unauthorized redirect behavior before token initialization.
- Redirect-to-login full reload behavior causing auth state loss.

## [0.2.0] - 2026-04-20
### Added
- Unified custom API response envelope.
- Refresh token rotation and revoke endpoint.
- Rate limiting for auth endpoints.
- OpenTelemetry tracing instrumentation.
- Unit and integration test projects.
- CI/CD workflows (CI, CD, CodeQL).
- Dockerfile and docker-compose deployment artifacts.
- Issue/PR templates and roadmap.

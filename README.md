# loaniq-integration

ASP.NET Core 10 Web API for integrating with the LoanIQ loan-management platform.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 10 (LTS) |
| Framework | ASP.NET Core Web API |
| Logging | Serilog (console + rolling file) |
| OpenAPI | `Microsoft.AspNetCore.OpenApi` + `Microsoft.Extensions.ApiDescription.Server` |
| Testing | xUnit + `Microsoft.AspNetCore.Mvc.Testing` (integration tests) |
| Code style | `.editorconfig` + Roslyn analyzers (`AnalysisLevel=latest`, `EnforceCodeStyleInBuild=true`) |

## Prerequisites

- [.NET 10 SDK](https://dot.net) — install without root via:
  ```bash
  curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel LTS
  export PATH="$HOME/.dotnet:$PATH"
  ```

## Getting started

```bash
# Build
make build

# Run all tests
make test

# Start the dev server (http://localhost:5193)
make run

# Regenerate the OpenAPI document
make openapi
```

## Project structure

```
.
├── src/
│   └── LoanIQ.Integration.Api/
│       ├── Controllers/          # HTTP endpoints
│       │   ├── HealthController.cs   GET /health
│       │   └── LoansController.cs    CRUD /api/v1/loans
│       ├── Extensions/           # IServiceCollection helpers
│       ├── Middleware/           # Exception → RFC 9110 problem+json
│       ├── Models/
│       │   ├── Requests/         # Validated request DTOs
│       │   └── Responses/        # Response DTOs + PagedResponse<T>
│       ├── LoanStatus.cs         # Pending | Active | Closed | Defaulted
│       └── Program.cs            # Startup wiring
├── tests/
│   └── LoanIQ.Integration.Api.Tests/
│       └── HealthControllerTests.cs  # WebApplicationFactory integration tests
├── .polaira/
│   ├── emit-openapi.sh           # Regenerates openapi.json (not committed)
│   └── openapi.json              # Generated OpenAPI 3.1 document (not committed)
├── Directory.Build.props         # Solution-wide MSBuild settings
├── LoanIQ.Integration.slnx       # Solution file
└── Makefile                      # Build shortcuts
```

## API routes

| Method | Path | Description |
|--------|------|-------------|
| GET | `/health` | Liveness / health-check |
| GET | `/api/v1/loans` | List loans (paginated) |
| GET | `/api/v1/loans/{id}` | Get loan by GUID |
| POST | `/api/v1/loans` | Create a loan |
| PUT | `/api/v1/loans/{id}` | Update a loan |
| DELETE | `/api/v1/loans/{id}` | Delete a loan |

The full machine-readable description lives in `.polaira/openapi.json` (regenerated via `make openapi`). In development mode the document is also served live at `/openapi/v1.json`.

## Configuration

| Key | Default | Description |
|-----|---------|-------------|
| `Serilog:MinimumLevel:Default` | `Information` | Root log level |
| `ASPNETCORE_ENVIRONMENT` | _(unset)_ | Set to `Development` for verbose logs and the live OpenAPI endpoint |

## Running tests

```bash
make test
# or with coverage:
dotnet test LoanIQ.Integration.slnx --collect:"XPlat Code Coverage"
```

## Generating the OpenAPI document

```bash
bash .polaira/emit-openapi.sh
# → .polaira/openapi.json
```

The script builds the project and uses `Microsoft.Extensions.ApiDescription.Server` to extract the document at build time — no running server required.

## Vulnerability audit

Run at any time:

```bash
make audit
```

**Last audit result (2026-08-31):** No vulnerable packages found in either project against the NuGet default feed.

## Code style

Style rules are enforced at compile time via `.editorconfig` and `EnforceCodeStyleInBuild=true` in `Directory.Build.props`. Warnings are treated as errors (`TreatWarningsAsErrors=true`). Run `dotnet format` to auto-fix formatting.

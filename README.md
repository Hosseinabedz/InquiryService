# Inquiry Service

A small ASP.NET Core Web API for processing bill inquiries through multiple payment providers.

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- MediatR
- xUnit + Moq + FluentAssertions

## Architecture

The project is split into four layers:

- **API** – HTTP endpoints
- **Application** – inquiry processing, provider execution and application logic
- **Domain** – inquiry entities and business rules
- **Infrastructure** – EF Core, SQL Server and provider implementations

Provider priority and timeout are configured in `appsettings.json`.

## How it works

When an inquiry is received:

1. The cache is checked first.
2. If there is no cached result, the configured providers are tried based on their priority.
3. A successful response is returned immediately.
4. A business error is treated as a valid provider response, so no failover happens.
5. A technical error or timeout causes the next provider to be tried.
6. The inquiry and all provider attempts are stored in SQL Server.
7. The final result is cached for 5 minutes.

Identical concurrent requests for the same bill ID are also serialized to avoid processing the same inquiry multiple times.

`IgnoreCache = true` can be used to bypass the cache.

## Configuration

Update the SQL Server connection string in `appsettings.json` if needed.

Provider order and timeout can be changed without changing the code:

```json
"PaymentProviders": {
  "Providers": [
    {
      "Name": "Mellat",
      "Priority": 1,
      "TimeoutSeconds": 3
    },
    {
      "Name": "Saman",
      "Priority": 2,
      "TimeoutSeconds": 3
    }
  ]
}
```

The included Mellat and Saman providers are simulated implementations for this assignment.

## Run

Create the database using the included EF Core migration, then run the API:

```bash
dotnet ef database update
dotnet run --project InquiryService
```

The API exposes:

```http
POST /api/inquiries
```

Example request:

```json
{
  "billId": "123456",
  "ignoreCache": false
}
```

## Tests

Run the tests with:

```bash
dotnet test
```

The tests cover provider success, business errors, technical errors, timeouts, failover, provider priority, caching and concurrent requests.

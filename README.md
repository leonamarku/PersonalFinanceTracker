# Personal Finance Tracker API

A full-featured RESTful API built with **ASP.NET Core (.NET 10)** for managing personal finances —
accounts, transactions, budgets, and analytics reports.

---

## Architecture

```
Client (React / Swagger)
       ↓  HTTP/JSON
   Controllers          ← validate input, delegate to services
       ↓
    Services            ← business logic, orchestration
       ↓
  Repositories          ← data access abstraction (interfaces)
       ↓
 Entity Framework Core  ← ORM
       ↓
   SQL Server           ← database
```

**Design patterns used:** Repository Pattern · Dependency Injection · DTO Pattern · AutoMapper ·
Clean Architecture (Core / Infrastructure / API layers)

---

## Project Structure

```
PersonalFinanceTracker/
├── src/
│   ├── PersonalFinanceTracker.Core/           # Domain layer (no external dependencies)
│   │   ├── Entities/                          # EF Core entity classes
│   │   ├── Enums/                             # AccountType, TransactionType
│   │   ├── DTOs/                              # Request/response data transfer objects
│   │   ├── Interfaces/                        # Repository + Service interfaces
│   │   ├── Mappings/                          # AutoMapper profile
│   │   └── Common/                            # Result<T>, ServiceException
│   │
│   ├── PersonalFinanceTracker.Infrastructure/ # Data + service implementations
│   │   ├── Data/ApplicationDbContext.cs       # EF Core DbContext + seed data
│   │   ├── Repositories/                      # Concrete repository implementations
│   │   ├── Services/                          # Business logic implementations
│   │   └── Extensions/ServiceCollectionExtensions.cs
│   │
│   └── PersonalFinanceTracker.API/            # Presentation layer
│       ├── Controllers/                       # HTTP endpoints
│       ├── Middleware/ExceptionMiddleware.cs  # Global error handling
│       ├── Program.cs                         # App startup, DI registration
│       └── appsettings.json
│
├── tests/
│   └── PersonalFinanceTracker.Tests/
│       ├── Services/                          # Unit tests for service layer
│       ├── Controllers/                       # Unit tests for controllers
│       └── Repositories/                     # Repository tests (EF InMemory)
│
└── .github/workflows/ci-cd.yml               # GitHub Actions pipeline
```

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB is fine for dev)
- [Git](https://git-scm.com/)

---

## Setup & Run (Local)

### 1. Clone the repository

```bash
git clone https://github.com/YOUR_USERNAME/personal-finance-tracker.git
cd personal-finance-tracker/code/PersonalFinanceTracker
```

### 2. Configure the connection string

Edit `src/PersonalFinanceTracker.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PersonalFinanceTrackerDb;Trusted_Connection=True;"
  },
  "Jwt": {
    "Key": "your-strong-secret-key-at-least-32-characters",
    "Issuer": "PersonalFinanceTracker",
    "ExpiresHours": "24"
  }
}
```

### 3. Apply database migrations

```bash
cd src/PersonalFinanceTracker.API
dotnet ef database update
```

> The app also auto-migrates on startup, so you can skip this step and just run.

### 4. Run the API

```bash
dotnet run --project src/PersonalFinanceTracker.API
```

The API starts at `https://localhost:7xxx` and Swagger UI opens at the root `/`.

### 5. Run the tests

```bash
dotnet test tests/PersonalFinanceTracker.Tests
```

---

## API Endpoints

### Authentication (no token required)

| Method | Route               | Description                       |
|--------|---------------------|-----------------------------------|
| POST   | /api/auth/register  | Register a new user               |
| POST   | /api/auth/login     | Login and receive a JWT token     |

### Accounts (requires JWT)

| Method | Route              | Description                        |
|--------|--------------------|------------------------------------|
| GET    | /api/accounts      | List all accounts                  |
| GET    | /api/accounts/{id} | Get account by ID                  |
| POST   | /api/accounts      | Create a new account               |
| PUT    | /api/accounts/{id} | Update account name/type/currency  |
| DELETE | /api/accounts/{id} | Delete account + all transactions  |

### Transactions (requires JWT)

| Method | Route                              | Description                             |
|--------|------------------------------------|-----------------------------------------|
| GET    | /api/transactions                  | List all user transactions              |
| GET    | /api/transactions/{id}             | Get transaction by ID                   |
| GET    | /api/transactions/account/{id}     | List transactions for one account       |
| POST   | /api/transactions                  | Create transaction (balance auto-updates)|
| PUT    | /api/transactions/{id}             | Update transaction                      |
| DELETE | /api/transactions/{id}             | Delete transaction (balance reversed)   |
| POST   | /api/transactions/transfer         | Transfer between two accounts           |

### Categories (requires JWT)

| Method | Route                 | Description                                  |
|--------|-----------------------|----------------------------------------------|
| GET    | /api/categories       | List all visible categories                  |
| GET    | /api/categories/{id}  | Get category by ID                           |
| POST   | /api/categories       | Create category (Admin: global, User: personal)|
| PUT    | /api/categories/{id}  | Update category                              |
| DELETE | /api/categories/{id}  | Delete category                              |
| GET    | /api/categories/all   | **[Admin only]** List all categories         |

### Budgets (requires JWT)

| Method | Route                            | Description                    |
|--------|----------------------------------|--------------------------------|
| GET    | /api/budgets                     | List all user budgets          |
| GET    | /api/budgets/{id}                | Get budget by ID               |
| GET    | /api/budgets/month/{year}/{month}| List budgets for a month       |
| POST   | /api/budgets                     | Create monthly budget          |
| PUT    | /api/budgets/{id}                | Update budget limit            |
| DELETE | /api/budgets/{id}                | Delete budget                  |

### Reports (requires JWT)

| Method | Route                               | Description                          |
|--------|-------------------------------------|--------------------------------------|
| GET    | /api/reports/summary/{year}/{month} | Monthly income/expense/net summary   |
| GET    | /api/reports/categories/{year}/{month}| Category breakdown with %          |
| GET    | /api/reports/budgets/{year}/{month} | Budget status + over-budget alerts   |

---

## Authentication Flow

1. **Register**: `POST /api/auth/register` → receives `{ token, email, role, expires }`
2. **Login**: `POST /api/auth/login` → same response
3. **Use token**: Add header `Authorization: Bearer <token>` to all protected requests
4. **Roles**: `Admin` can manage global categories; `User` manages own data only

---

## Business Logic Highlights

- **Balance auto-update**: Every transaction create/update/delete recalculates the account balance automatically.
- **Insufficient funds guard**: An expense cannot be posted if it exceeds the current account balance.
- **Inter-account transfer**: Creates two linked transactions (expense on source, income on destination).
- **Budget over-limit alert**: The `/api/reports/budgets` endpoint flags any category where spending ≥ budget limit.
- **Category scoping**: Global categories (Admin-created) are visible to all; personal categories are user-private.

---

## Technologies Used

| Category        | Technology                          |
|-----------------|-------------------------------------|
| Backend         | ASP.NET Core Web API (.NET 10)      |
| Database        | SQL Server 2022 / LocalDB           |
| ORM             | Entity Framework Core 10            |
| Authentication  | JWT Bearer Tokens                   |
| Mapping         | AutoMapper 13                       |
| Password hashing| BCrypt.Net                          |
| Testing         | xUnit + Moq + FluentAssertions      |
| CI/CD           | GitHub Actions                      |
| Cloud           | Azure App Service + Azure SQL       |
| API Docs        | Swagger / OpenAPI                   |

---

## Developer

**Leona Marku** — South East European University, SOA Course 2025/2026

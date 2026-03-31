# Product API - Quick Reference

## 📌 Quick Start (5 Minutes)

### 1. Database Setup
```sql
-- Execute: database/CreateProductDB.sql in MSSQL Server Management Studio
```

### 2. Update Connection String
```json
// File: src/ProductAPI/appsettings.json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=ProductDB;Trusted_Connection=true;Encrypt=false;"
}
```

### 3. Run Application
```bash
cd src/ProductAPI
dotnet run
# API at: https://localhost:5001
# Swagger at: https://localhost:5001/swagger
```

### 4. Run Tests
```bash
dotnet test
```

---

## 🔗 API Quick Reference

### Create Product
```bash
curl -X POST "https://localhost:5001/api/products" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Laptop",
    "description": "Gaming laptop",
    "price": 999.99,
    "category": "Electronics",
    "stock": 10
  }'
```

**Response (201 Created):**
```json
{
  "id": 1,
  "name": "Laptop",
  "description": "Gaming laptop",
  "price": 999.99,
  "category": "Electronics",
  "stock": 10
}
```

**Error (400 Bad Request):**
```json
{
  "errors": {
    "Price": ["Price must be greater than 0"],
    "Stock": ["Stock cannot be negative"]
  }
}
```

### Get All Products
```bash
curl -X GET "https://localhost:5001/api/products"
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "name": "Laptop",
    "description": "Gaming laptop",
    "price": 999.99,
    "category": "Electronics",
    "stock": 10
  }
]
```

---

## 📁 File Structure

```
ProductAPI/
├── Program.cs                    - Startup configuration
├── appsettings.json              - Configuration & connection string
├── ProductAPI.csproj             - Project file
├── Models/
│   └── Product.cs                - Product entity model
├── Controllers/
│   └── ProductsController.cs      - API endpoints (POST/GET)
├── Data/
│   └── AppDbContext.cs           - Entity Framework Core context
└── Services/
    └── ProductService.cs         - Business logic

Tests/
├── ProductServiceTests.cs             - Unit tests
├── ProductControllerIntegrationTests.cs - Integration tests
├── TestDataFixtures.cs                 - Test data
└── ProductAPI.Tests.csproj             - Test project

Database/
└── CreateProductDB.sql            - MSSQL schema
```

---

## ✅ Validation Rules

| Field | Required | Min/Max | Rules |
|-------|----------|---------|-------|
| name | Yes | 1-200 chars | Required |
| description | No | 0-1000 chars | Optional |
| price | Yes | > 0 | Must be positive |
| category | No | 0-100 chars | Optional |
| stock | Yes | >= 0 | Non-negative |

---

## 🧪 Test Execution

```bash
# All tests
dotnet test

# Specific test class
dotnet test --filter "ClassName=ProductServiceTests"

# Verbose output
dotnet test --logger "console;verbosity=detailed"

# Watch mode (auto-run on changes)
dotnet watch test
```

### Test Count
- Unit Tests: 8 (ProductServiceTests)
- Integration Tests: 11 (ProductControllerIntegrationTests)
- **Total: 19+ tests**

---

## 🔧 Useful Commands

```bash
# Restore NuGet packages
dotnet restore

# Build project
dotnet build

# Run in development
dotnet run

# Run in watch mode (auto-rebuild on changes)
dotnet watch run

# Publish for production
dotnet publish -c Release

# Create migration (if adding new models)
dotnet ef migrations add MigrationName
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

---

## ⚙️ Configuration Files

### appsettings.json
Located: `src/ProductAPI/appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=ProductDB;..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Program.cs Setup
- DbContext registration
- Dependency injection
- CORS policy
- Swagger/OpenAPI
- Database initialization

---

## 🚨 Common Issues

| Issue | Solution |
|-------|----------|
| "Failed to connect to database" | Check connection string, verify MSSQL is running |
| "Port 5000 is already in use" | Kill process using port or use `dotnet run --urls "https://localhost:5002"` |
| "Tests fail with DB error" | In-memory DB used for tests, clear and rebuild |
| "Swagger not loading" | Check that app is running, navigate to `/swagger` |

---

## 📋 Implementation Checklist

✅ Project structure created
✅ Product model with validation
✅ DbContext configured for MSSQL
✅ POST /products endpoint (201/400)
✅ GET /products endpoint (200/empty array)
✅ Price validation (> 0)
✅ Stock validation (>= 0)
✅ JSON responses
✅ Error handling
✅ 8 unit tests
✅ 11 integration tests
✅ Test fixtures
✅ Database script
✅ Documentation
✅ Configuration guide
✅ Setup instructions

---

## 📚 Documentation Files

| File | Purpose |
|------|---------|
| README.md | Complete project documentation |
| SETUP.md | Setup and configuration guide |
| IMPLEMENTATION_SUMMARY.md | Implementation details |
| QUICK_REFERENCE.md | This file |

---

## 🎯 Key Technologies

- ASP.NET Core 8.0
- Entity Framework Core 8.0
- MSSQL Server
- NUnit 4.0
- C# 12
- JSON/REST API

---

## 📞 Support Resources

- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core/)
- [EF Core Docs](https://docs.microsoft.com/ef/core/)
- [NUnit Docs](https://docs.nunit.org/)
- Check SETUP.md for troubleshooting

---

## 🚀 Next Steps

1. Execute `CreateProductDB.sql` to create database
2. Update connection string in `appsettings.json`
3. Run `dotnet run` in `src/ProductAPI/`
4. Test with `dotnet test` from root
5. Access API at `https://localhost:5001`
6. View Swagger at `https://localhost:5001/swagger`

---

**Last Updated**: March 31, 2026
**Status**: Ready for Development ✅

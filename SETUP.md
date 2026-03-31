# Product API - Setup and Configuration Guide

## Prerequisites

- .NET 8.0 SDK or later ([Download](https://dotnet.microsoft.com/download))
- MSSQL Server or SQL Server Express ([Download](https://www.microsoft.com/en-us/sql-server/sql-server-downloads))
- Visual Studio 2022, VS Code, or other .NET IDE

## Quick Start

### 1. Clone and Restore
```bash
cd c:\GitRepo\muti-agent-demo
dotnet restore
```

### 2. Configure Database Connection
Edit `src/ProductAPI/appsettings.json` and update the connection string:

**For Local MSSQL Server:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=ProductDB;Trusted_Connection=true;Encrypt=false;"
}
```

**For MSSQL Express:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=ProductDB;Trusted_Connection=true;Encrypt=false;"
}
```

**For Remote Server:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=your-server-name;Database=ProductDB;User Id=sa;Password=your-password;Encrypt=false;"
}
```

### 3. Create Database Schema

**Option A: Using the SQL Script**
1. Open SQL Server Management Studio (SSMS)
2. Connect to your MSSQL Server
3. Open `database/CreateProductDB.sql`
4. Execute the script

**Option B: Using Entity Framework Migrations**
```bash
# Create a migration
cd src/ProductAPI
dotnet ef migrations add InitialCreate

# Apply the migration
dotnet ef database update
```

### 4. Run the Application
```bash
cd src/ProductAPI
dotnet run
```

The API will be available at:
- Swagger UI: https://localhost:5001/swagger
- API Base URL: https://localhost:5001/api/

### 5. Run Tests
```bash
# From root directory
dotnet test

# Or with detailed output
dotnet test --logger "console;verbosity=detailed"
```

## Application Structure

### Main API Project: `ProductAPI.csproj`
- **Program.cs**: Application startup configuration
- **appsettings.json**: Configuration settings
- **Models/Product.cs**: Product data model with validation
- **Controllers/ProductsController.cs**: HTTP endpoints
- **Data/AppDbContext.cs**: Entity Framework DbContext
- **Services/ProductService.cs**: Business logic service

### Test Project: `ProductAPI.Tests.csproj`
- **ProductServiceTests.cs**: Unit tests for ProductService
- **ProductControllerIntegrationTests.cs**: Integration tests for API endpoints
- **TestDataFixtures.cs**: Test data and fixtures

## Configuration Details

### Database Connection String Options

**Windows Authentication (Recommended for Local Development):**
```
Server=localhost;Database=ProductDB;Trusted_Connection=true;Encrypt=false;
```

**SQL Authentication:**
```
Server=localhost;Database=ProductDB;User Id=sa;Password=YourPassword;Encrypt=false;
```

**Connection String Parameters:**
- `Server`: Your MSSQL server instance
- `Database`: Database name (ProductDB)
- `Trusted_Connection`: Use Windows authentication (true/false)
- `User Id`: SQL login (if not using Windows auth)
- `Password`: SQL password (if not using Windows auth)
- `Encrypt`: Use encryption (true/false)

### appsettings.json Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ProductDB;Trusted_Connection=true;Encrypt=false;"
  }
}
```

## Troubleshooting

### Issue: "Failed to connect to database"
**Solution:**
1. Verify MSSQL Server is running and accessible
2. Check connection string in `appsettings.json`
3. Confirm database `ProductDB` exists
4. Test connection with SQL Server Management Studio

### Issue: "Port 5000 is already in use"
**Solution:**
```bash
# Find and kill the process using port 5000
# On Windows:
netstat -ano | findstr :5000
taskkill /PID <PID> /F

# Then restart the application
dotnet run
```

### Issue: "Entity Framework migration errors"
**Solution:**
```bash
# Remove the last migration
dotnet ef migrations remove

# Try again from scratch
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Issue: "Tests fail with database errors"
**Solution:**
- Tests use in-memory database (no external DB needed)
- Clear any pending test processes
- Run: `dotnet clean && dotnet build && dotnet test`

## Development Workflow

### Adding a New Feature
1. Create model class in `Models/`
2. Add DbSet to `AppDbContext.cs`
3. Create/update migration: `dotnet ef migrations add <MigrationName>`
4. Update database: `dotnet ef database update`
5. Create service/repository in `Services/`
6. Add controller endpoint in `Controllers/`
7. Write tests in `Tests/`
8. Run all tests: `dotnet test`

### Creating a Migration
```bash
cd src/ProductAPI

# Add a new migration
dotnet ef migrations add AddNewTable

# Apply migration to database
dotnet ef database update

# Revert to previous migration
dotnet ef database update <PreviousMigrationName>

# Remove last migration (before applying)
dotnet ef migrations remove
```

## Running the API

### Development Mode
```bash
cd src/ProductAPI
dotnet run
```

### Production Build
```bash
dotnet publish -c Release -o ./publish
cd publish
dotnet ProductAPI.dll
```

### With Environment Variable
```bash
# Windows
set ASPNETCORE_ENVIRONMENT=Production
dotnet run

# Linux/macOS
export ASPNETCORE_ENVIRONMENT=Production
dotnet run
```

## Testing

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Class
```bash
dotnet test --filter ClassName=ProductServiceTests
```

### Run Tests with Coverage
```bash
# Install coverage tool
dotnet tool install -g OpenCover

# Run tests with coverage
opencover.console.exe -target:"dotnet.exe" -targetargs:"test" -output:"coverage.xml"
```

### Watch Mode (Auto-run tests on file changes)
```bash
dotnet watch test
```

## API Endpoints

### Create Product
```
POST /api/products
Content-Type: application/json

{
  "name": "Product Name",
  "description": "Product Description",
  "price": 99.99,
  "category": "Category",
  "stock": 10
}
```

### Get All Products
```
GET /api/products
```

## Validation Rules

| Field | Rule | Example |
|-------|------|---------|
| name | Required, max 200 chars | "Laptop" |
| description | Optional, max 1000 chars | "High-performance laptop" |
| price | Required, must be > 0 | 999.99 |
| category | Optional, max 100 chars | "Electronics" |
| stock | Required, must be >= 0 | 15 |

## Performance Tips

1. **Database Indexes**: Indexes on Category and Name columns improve query performance
2. **Connection Pooling**: Connection strings use default pooling (max 100 connections)
3. **Async Operations**: All database operations use async/await
4. **Caching**: Consider adding caching layer for GET /api/products

## Security Considerations

1. **Connection String**: Keep in environment variables for production
2. **Validation**: All inputs are validated server-side
3. **HTTPS**: Enabled by default in ASP.NET Core
4. **CORS**: Currently allows all origins (restrict in production)
5. **Authentication**: Add authentication as needed for your use case

## Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [NUnit Documentation](https://docs.nunit.org/)
- [MSSQL Server Documentation](https://docs.microsoft.com/sql/)

## Support

For issues or questions:
1. Check the troubleshooting section
2. Review test cases for usage examples
3. Check ASP.NET Core logs in console output
4. Verify database connection and schema

# Product API - .NET Core REST API Implementation

A complete .NET Core REST API for product management with MSSQL database, built with Entity Framework Core and tested with NUnit.

## Project Structure

```
muti-agent-demo/
├── src/
│   └── ProductAPI/
│       ├── Models/
│       │   └── Product.cs
│       ├── Controllers/
│       │   └── ProductsController.cs
│       ├── Data/
│       │   └── AppDbContext.cs
│       ├── Services/
│       │   └── ProductService.cs
│       ├── Program.cs
│       └── appsettings.json
├── tests/
│   └── ProductAPI.Tests/
│       ├── ProductServiceTests.cs
│       └── ProductControllerIntegrationTests.cs
├── database/
│   └── CreateProductDB.sql
├── ProductAPI.csproj
└── README.md
```

## Features

- **REST API Endpoints**
  - POST /products - Create a new product
  - GET /products - Retrieve all products

- **Validation**
  - Price must be greater than 0
  - Stock must be >= 0
  - Product name is required
  - Description and Category are optional

- **Database**
  - MSSQL Server database
  - Products table with proper constraints
  - Entity Framework Core for data access
  - In-memory database for testing

- **Testing**
  - Unit tests for ProductService
  - Integration tests for ProductsController
  - NUnit framework
  - Test data fixtures

- **Error Handling**
  - HTTP 400 Bad Request for validation errors
  - HTTP 201 Created for successful product creation
  - HTTP 200 OK for product retrieval
  - Comprehensive error messages in JSON responses

## Project Setup

### Prerequisites
- .NET 8.0 SDK or later
- MSSQL Server (for production)
- Visual Studio 2022 or VS Code

### Installation

1. Clone the repository
   ```bash
   git clone <repository-url>
   cd muti-agent-demo
   ```

2. Restore NuGet packages
   ```bash
   dotnet restore
   ```

3. Set up the database
   - Execute the SQL script: `database/CreateProductDB.sql` on your MSSQL server
   - Or use EF Core migrations:
     ```bash
     dotnet ef database update
     ```

### Configuration

Update `src/ProductAPI/appsettings.json` with your database connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=ProductDB;Trusted_Connection=true;Encrypt=false;"
  }
}
```

## Running the API

### Development
```bash
cd src/ProductAPI
dotnet run
```

The API will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger UI: https://localhost:5001/swagger

### Production
```bash
dotnet publish -c Release
dotnet ProductAPI.dll
```

## Running Tests

### All Tests
```bash
dotnet test
```

### Specific Test Project
```bash
cd tests/ProductAPI.Tests
dotnet test
```

### NUnit Test Runner
```bash
dotnet test --logger "console;verbosity=detailed"
```

## API Endpoints

### Create Product
**POST** `/api/products`

Request Body:
```json
{
  "name": "Laptop",
  "description": "High-performance laptop",
  "price": 999.99,
  "category": "Electronics",
  "stock": 15
}
```

Success Response (201 Created):
```json
{
  "id": 1,
  "name": "Laptop",
  "description": "High-performance laptop",
  "price": 999.99,
  "category": "Electronics",
  "stock": 15
}
```

Error Response (400 Bad Request):
```json
{
  "errors": {
    "Price": ["Price must be greater than 0"],
    "Stock": ["Stock cannot be negative"]
  }
}
```

### Get All Products
**GET** `/api/products`

Success Response (200 OK):
```json
[
  {
    "id": 1,
    "name": "Laptop",
    "description": "High-performance laptop",
    "price": 999.99,
    "category": "Electronics",
    "stock": 15
  },
  {
    "id": 2,
    "name": "Mouse",
    "description": "Wireless mouse",
    "price": 29.99,
    "category": "Accessories",
    "stock": 50
  }
]
```

Empty Response (200 OK):
```json
[]
```

## Product Model

The Product entity includes the following fields:

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| Id | int | Yes (auto-generated) | Primary Key |
| Name | string | Yes | Max 200 characters |
| Description | string | No | Max 1000 characters |
| Price | decimal | Yes | Must be > 0 |
| Category | string | No | Max 100 characters |
| Stock | int | Yes | Must be >= 0 |

## Testing

The project includes comprehensive test suites:

### Unit Tests (ProductServiceTests.cs)
- Product creation with valid data
- Price validation (must be > 0)
- Stock validation (must be >= 0)
- Retrieving all products
- Empty product list handling
- Required field validation

### Integration Tests (ProductControllerIntegrationTests.cs)
- POST /products with valid data
- POST /products with invalid price
- POST /products with invalid stock
- POST /products with missing required fields
- GET /products returns products
- GET /products returns empty array
- HTTP status code validation
- JSON response validation

## Dependencies

- **Microsoft.EntityFrameworkCore** (8.0.0)
- **Microsoft.EntityFrameworkCore.SqlServer** (8.0.0)
- **Microsoft.EntityFrameworkCore.InMemory** (8.0.0) - for testing
- **NUnit** (4.0.1)
- **NUnit3TestAdapter** (4.5.0)
- **Moq** (4.20.70)
- **Microsoft.AspNetCore.Mvc.Testing** (8.0.0)

## Implementation Notes

1. **Data Access**: Uses Entity Framework Core with DbContext pattern for MSSQL
2. **Service Layer**: ProductService implements IProductService for dependency injection
3. **Validation**: Data annotations on model properties + method-level validation
4. **Testing**: In-memory database for unit and integration tests
5. **Error Handling**: Structured error responses with detailed validation messages
6. **CORS**: Enabled for all origins in development

## Database Schema

```sql
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    Price DECIMAL(18,2) NOT NULL,
    Category NVARCHAR(100) NULL,
    Stock INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    CONSTRAINT CK_Products_Price CHECK (Price > 0),
    CONSTRAINT CK_Products_Stock CHECK (Stock >= 0)
);
```

## Troubleshooting

### Database Connection Issues
- Verify MSSQL Server is running
- Check connection string in `appsettings.json`
- Ensure database `ProductDB` exists
- Run `CreateProductDB.sql` script to initialize schema

### NUnit Tests Not Running
- Ensure NUnit framework is installed: `dotnet add package NUnit`
- Check that test project has correct project reference
- Run `dotnet restore` to update all dependencies

### API Not Responding
- Check that port 5000/5001 is available
- Verify firewall settings allow local connections
- Check application logs in Program.cs output

## Contributing

1. Follow C# naming conventions
2. Write unit tests for new features
3. Ensure all existing tests pass
4. Update documentation for API changes

## License

This project is part of the multi-agent demo application.

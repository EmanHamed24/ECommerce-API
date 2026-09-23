E-Commerce API

A backend-focused E-Commerce Order Management REST API built with ASP.NET Core and designed using a layered Clean Architecture approach.

The project focuses on secure API development, business logic, authentication and authorization, data access, validation, caching, background processing, automated testing, and containerization.

🚀 Technologies & Tools

- C#
- ASP.NET Core Web API
- .NET 10
- Entity Framework Core
- SQL Server
- JWT Authentication & Authorization
- Role-Based Authorization
- Clean Architecture
- Repository & Service Patterns
- FluentValidation
- Global Exception Handling
- Pagination & Filtering
- Redis Caching
- Background Jobs
- Swagger / OpenAPI
- xUnit
- Unit & Integration Testing
- Docker & Docker Compose
- Git & GitHub

🏗️ Architecture

The solution is organized into separate layers to maintain separation of concerns and make the application easier to maintain and extend.

ECommerce
│
├── ECommerce.API
│   ├── Controllers
│   ├── Middleware
│   ├── Program.cs
│   └── Dockerfile
│
├── ECommerce.Application
│   ├── DTOs
│   ├── Interfaces
│   ├── Services
│   ├── Validators
│   └── Business Logic
│
├── ECommerce.Domain
│   ├── Entities
│   ├── Constants
│   └── Domain Models
│
├── ECommerce.Infrastructure
│   ├── Persistence
│   ├── Repositories
│   ├── Services
│   └── External Implementations
│
└── ECommerce.Tests
    ├── Unit Tests
    └── Integration Tests

🔐 Authentication & Authorization

The API uses JWT Bearer Authentication with role-based authorization.

Supported roles include:

- Customer
- Admin

Administrative access is restricted to protected endpoints and cannot be selected during normal customer registration.

Authorization is applied to sensitive operations such as:

- Product management
- Order management
- Coupon management
- Administrative operations

JWT secrets and database credentials are stored outside the source code using environment configuration and User Secrets.

🛒 Main Features

Authentication

- Customer registration
- Login
- JWT token generation
- Role-based authorization
- Protected API endpoints

Products

- Create products
- Update products
- Retrieve products
- Product status management
- Stock management
- SKU validation
- Category relationships

Categories

- Category management
- Product/category relationships
- Validation of category operations

Shopping Cart

- Add products to cart
- Update cart items
- Remove cart items
- Retrieve current cart
- Product availability validation

Orders

- Create orders
- Order item management
- Stock validation
- Order status management
- Customer order history
- Administrative order management

Payments

- Payment processing workflow
- Payment status management
- Order/payment relationship

Coupons

- Coupon management
- Validation
- Discount handling
- Administrative coupon operations

⚡ Performance & Scalability

The project includes several backend techniques intended to improve performance and maintainability:

- Pagination
- Filtering
- Redis caching
- Background processing
- Efficient Entity Framework Core queries
- Separation of business logic from controllers

🛡️ Validation & Error Handling

The API uses centralized error handling and validation to provide consistent API responses.

Implemented concepts include:

- Global Exception Handling
- Business Exceptions
- FluentValidation
- Request validation
- Duplicate SKU validation
- Product availability validation
- Stock validation
- Authorization checks

🧪 Testing

The project includes both Unit Tests and Integration Tests.

Unit Testing

Unit tests cover application business logic and service behavior.

Examples include:

- Product operations
- Cart operations
- Order creation
- Stock validation
- Business rule validation

Integration Testing

Integration tests verify API endpoints and application behavior using an ASP.NET Core test server.

Examples include:

- Authentication
- Products API
- Cart API
- Orders API
- Payments API
- Admin Orders API

🐳 Docker

The application can be containerized using Docker and Docker Compose.

The Docker setup includes:

- ASP.NET Core API container
- SQL Server container
- Persistent SQL Server volume
- Environment-based configuration
- Secure secret management

Run with Docker Compose

Create a ".env" file locally containing the required secrets:

SA_PASSWORD=your_secure_sql_password
JWT_KEY=your_secure_jwt_key

Then run:

docker compose -f ECommerce.API/docker-compose.yml up --build

The API will be available at:

http://localhost:8080


📖 API Documentation

The project uses Swagger / OpenAPI for API documentation and testing.

When running the API locally, Swagger can be used to explore and test the available endpoints.

🗄️ Database

The project uses SQL Server with Entity Framework Core.

The database contains entities for:

- Users
- Products
- Categories
- Cart
- Cart Items
- Orders
- Order Items
- Payments
- Coupons

Database schema changes are managed using EF Core Migrations.

🔒 Security

Security-related configuration is intentionally kept outside the source code.

Sensitive values such as:

- Database passwords
- JWT signing keys
- Environment-specific secrets

are provided through environment variables or development User Secrets.

The repository excludes sensitive local configuration through ".gitignore" and ".dockerignore".

📂 Project Structure

Project| Responsibility
"ECommerce.API"| HTTP API, controllers, middleware, application startup
"ECommerce.Application"| DTOs, interfaces, services, validation, business logic
"ECommerce.Domain"| Entities and domain models
"ECommerce.Infrastructure"| Database, repositories, infrastructure services
"ECommerce.Tests"| Unit and integration testing

🎯 Project Goals

This project was developed to demonstrate practical backend development skills with the .NET ecosystem, including:

- RESTful API development
- Layered architecture
- Secure authentication and authorization
- Database design and Entity Framework Core
- Business logic implementation
- Automated testing
- Performance considerations
- Docker containerization
- Production-oriented configuration

👩‍💻 Author

Eman Hamed

Electrical Engineering — Computer & Systems

Aspiring .NET Backend Developer
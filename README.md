## Project Overview

Ecomify API is a conceptual e-commerce solution developed as a personal project to explore modern software architecture and .NET development practices. While not intended for production use, it showcases key features of scalable, secure, and maintainable web applications.

## Technology Stack

- **Framework**: .NET 8, ASP.NET Core
- **Architecture**: Clean Architecture with Domain-Driven Design (DDD)
- **Security**: Keycloak integration for authentication and role-based authorization using JWT tokens.
- **Data Access**: Dapper
- **Documentation**: Swagger/OpenAPI
- **Testing**: Unit and Integration Tests

## Architecture Overview

```mermaid
flowchart TD
    A["EcomifyAPI.Api
    ------------
    API Controllers
    Middleware
    API Documentation"] --> |"Request DTOs"| B["EcomifyAPI.Application
    ---------------
    Application Logic
    Service Implementations
    Validation Logic"]
    B --> C["EcomifyAPI.Domain
    ------------
    Domain Entities
    Business Rules
    Value Objects"]
    A --> D["EcomifyAPI.Contracts
    -------------
    DTOs / Request Models
    Response Models
    Read Models"]
    B --> D
    D --> C
    C --> E["EcomifyAPI.Infrastructure
    -----------------
    Database Access
    External Services
    Security Implementation
    Email Services"]
    %% Write/update cycle
    B -->|Confirmation| A
    %% Read cycle (using Read Models)
    E -->|Data| D
    D -->|Response DTOs| B
    B -->|Response| A
    %% Add labels for flows
    linkStyle 0 stroke:#FF5722,stroke-width:2,stroke-dasharray: 5 5;
    linkStyle 1 stroke:#FF5722,stroke-width:2,stroke-dasharray: 5 5;
    linkStyle 2 stroke:#FF5722,stroke-width:2,stroke-dasharray: 5 5;
    linkStyle 3 stroke:#FF5722,stroke-width:2,stroke-dasharray: 5 5;
    linkStyle 4 stroke:#FF5722,stroke-width:2,stroke-dasharray: 5 5;
    linkStyle 5 stroke:#2196F3,stroke-width:2;
    linkStyle 6 stroke:#4CAF50,stroke-width:2;
    linkStyle 7 stroke:#9C27B0,stroke-width:2;
    linkStyle 8 stroke:#9C27B0,stroke-width:2;
    linkStyle 9 stroke:#9C27B0,stroke-width:2;
    classDef domain fill:#c62828,stroke:#b71c1c,color:white;
    classDef infra fill:#0d47a1,stroke:#002171,color:white;
    classDef contracts fill:#827717,stroke:#524c00,color:white;
    classDef api fill:#00695c,stroke:#004d40,color:white;
    classDef app fill:#4527a0,stroke:#311b92,color:white;
    class A api;
    class B app;
    class C domain;
    class D contracts;
    class E infra;
    %% Legend
    subgraph Legend
    R[Request] --->|dashed orange| S[Inquiry]
    T[Transformation/Write] --->|blue| U[Storage]
    V[Confirmation] --->|green| W[Response]
    X[Read] --->|purple| Y[Data]
    end
    classDef legendBox fill:transparent,stroke:#333,stroke-width:1px;
    class Legend legendBox;
    %% Legend style
    linkStyle 10 stroke:#FF5722,stroke-width:2,stroke-dasharray: 5 5;
    linkStyle 11 stroke:#2196F3,stroke-width:2;
    linkStyle 12 stroke:#4CAF50,stroke-width:2;
    linkStyle 13 stroke:#9C27B0,stroke-width:2;
```

## Key Features

- **User Management**: Registration, authentication, and authorization
- **Product Catalog**: Product management with categories and search functionality
- **Shopping Cart**: Cart management with item addition, removal, and checkout
- **Order Processing**: Order creation, payment processing, and shipping options
- **Discount System**: Flexible discount and promotion engine
- **Payment Integration**: Simulated secure payment processing system for demonstration purposes.

## Domain Model

The application is centered around these core entities:

- **User**: Customer accounts with authentication and profile management
- **Product**: Items available for purchase with detailed information
- **Cart**: Shopping cart for collecting items before checkout
- **Order**: Purchase records with status tracking and history
- **Payment**: Transaction records and payment processing
- **Discount**: Special offers and promotional pricing

## Code Quality and Best Practices

- Clean Architecture separation of concerns
- SOLID principles implementation
- Comprehensive error handling
- Automated testing with high coverage

## Development Process

The application follows modern development methodologies including:

- Continuous Integration/Continuous Deployment pipelines
- Code reviews and quality gates
- Test-driven development
- Documentation as code

## Getting Started

### Prerequisites

- .NET 8 SDK
- Docker and Docker Compose (recommended)
- PostgreSQL 13+ (if not using Docker)
- Keycloak (if not using Docker)

### Docker Setup (Recommended)

The easiest way to get started is using Docker Compose, which will set up PostgreSQL and Keycloak for you.

1. Ensure Docker and Docker Compose are installed on your system

2. Start the containers:

````

docker-compose up -d

```

3. The following services will be available:

- PostgreSQL: localhost:5432 (DB: EcomifyAPI, User: keycloak_user)
- Keycloak: http://localhost:8080 (Admin user: admin_user, Admin password: Adm1n_K3ycl0ak_2025!)

4. Apply database migrations:

```

# Using Docker's psql

docker exec -i postgres psql -U keycloak_user -d EcomifyAPI < migration.sql

````

5. Update connection strings in `src/EcomifyAPI.Api/appsettings.json` to use the Docker setup:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=EcomifyAPI;Username=keycloak_user;Password=K3ycl0ak_P0stgr3s_2025!"
  }
  // Other settings...
}
```

### Keycloak Configuration

When running `docker-compose up -d`, Keycloak is automatically configured with the `base-realm` realm and all necessary roles and groups pre-configured from the `realm-export.json` file. However, you need to get the correct client secret for your application:

1. Access the Keycloak Admin Console:
   - URL: http://localhost:8080/admin
   - Username: admin_user
   - Password: Adm1n_K3ycl0ak_2025!
2. Select the "base-realm" realm from the dropdown in the top-left corner

3. Go to "Clients" in the left sidebar and click on the "base-realm" client

4. Navigate to the "Credentials" tab to view the client secret

5. Copy the displayed client secret and update your `appsettings.json` with this value in multiple places:
   ```json
   "UserKeycloakAdmin": {
     // other settings...
     "client_secret": "YOUR_NEW_CLIENT_SECRET",
     // other settings...
   },
   "UserKeycloakClient": {
     // other settings...
     "client_secret": "YOUR_NEW_CLIENT_SECRET",
     // other settings...
   },
   "Keycloak": {
     // other settings...
     "Credentials": {
       "Secret": "YOUR_NEW_CLIENT_SECRET"
     },
     // other settings...
   }
   ```

This step is critical because the client secret is regenerated each time you start Keycloak, and the API will not authenticate properly without the correct secret.

### Manual Database Setup

If you prefer to set up your database manually:

1. Create a new PostgreSQL database for the application:

   ```
   CREATE DATABASE ecomify;
   ```

2. Run the migration script to create all required tables:

   ```
   psql -U postgres -d ecomify -f migration.sql
   ```

### Application Setup

1. Clone the repository

   ```
   git clone https://github.com/yourusername/EcomifyAPI.git
   cd EcomifyAPI
   ```

2. Update connection strings in `src/EcomifyAPI.Api/appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=ecomify;Username=postgres;Password=your_password"
     }
     // Other settings...
   }
   ```

3. Build and run the application:

   ```
   dotnet build
   cd src/EcomifyAPI.Api
   dotnet run
   ```

4. Access the Swagger documentation at:
   ```
   https://localhost:5001/swagger
   ```

## Testing

Run the unit tests:

```
dotnet test test/EcomifyAPI.UnitTests
```

Run the integration tests:

```
dotnet test test/EcomifyAPI.IntegrationTests
```

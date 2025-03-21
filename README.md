# Basket Application

A simple basket application built with C# and Angular as part of a technical assessment.

## Table of Contents
- [Overview](#overview)
- [Technology Stack](#technology-stack)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Running with Docker (Recommended)](#running-with-docker-recommended)
  - [Running Locally](#running-locally)
- [Testing](#testing)
  - [Backend Tests](#backend-tests)
  - [Frontend Tests](#frontend-tests)
- [Project Structure](#project-structure)
- [API Documentation](#api-documentation)

## Overview

This application allows users to:
- Register and login to their accounts
- Add items to their basket
- View information about items and pricing
- Submit orders and receive a detailed receipt with costs and applied discounts

The application implements a flexible discount strategy pattern that allows for easy addition of new promotional rules. Currently, the system supports the following discount types:

- **Product-Specific Discount**: 10% off the normal price of apples
- **Multi-buy Discount**: Buy 2 tins of soup and get a loaf of bread for half price

The discount system is designed to handle multiple simultaneous promotions and correctly calculate the final price based on the items in the basket.

## Technology Stack

### Backend
- C# 
- xUnit for unit/integration testing

### Frontend
- Angular
- Cypress for integration testing

### Development and Testing Environment
- Developed on Arch Linux using JetBrains Rider
- Tested on Linux and Windows 11

## Getting Started

### Prerequisites
- [Docker](https://www.docker.com/products/docker-desktop) and [Docker Compose](https://docs.docker.com/compose/install/) (for Docker deployment)

OR

- [.NET Core SDK](https://dotnet.microsoft.com/download) (version 9)
- [Node.js](https://nodejs.org/) (version 20+)
- [Angular CLI](https://angular.io/cli)
- [MySQL](https://www.mysql.com/downloads/) or Docker (for running MySQL in a container)

### Running with Docker (Recommended)

The easiest way to get the application running is with Docker:

1. Clone the repository
```bash
git clone https://github.com/ruxsilva/basket_app.git
cd basket_app
```

2. Start the application using Docker Compose
```bash
docker-compose up -d --build
```

3. Navigate to `http://localhost:8081` in your browser

### Running Locally

If you prefer to run the application without Docker:

1. Clone the repository
```bash
git clone https://github.com/ruxsilva/basket_app.git
cd basket_app
```

2. Start MySQL
   - Option 1: Using Docker
   ```bash
   docker-compose up mysql -d
   ```
   - Option 2: Using local MySQL installation (ensure it's running)

3. Backend setup
```bash
dotnet restore
dotnet build
```

4. Frontend setup
```bash
cd BasketProject/Web/ClientApp
npm install && npm run build
```

5. Start the application
```bash
cd backend
dotnet run
```

6. Navigate to `http://localhost:8081` in your browser

This application should be running properly if you followed these steps.

## Testing

### Backend Tests
```bash
# On the root of the project
dotnet test 
```

### Frontend Tests
```bash
cd BasketProject/Web/ClientApp
npm run test:integration
```

## Project Structure

The project follows a clean architecture approach with clear separation of concerns:

```
BasketProject/
├── Application/     # Application services, DTOs, and business logic
├── Domain/          # Domain entities and business rules
├── Infrastructure/  # Data access, external services integration
└── Web/             # API controllers and Angular web application

BasketProjects.Tests/
├── Integration/     # Integration tests
└── Unit/            # Unit tests
```

While enterprise applications typically separate architectural layers into distinct projects, I made a deliberate choice to consolidate all components within a single project for this assessment. This approach simplifies the solution structure while still maintaining clean separation of concerns through folders, making the codebase more approachable without sacrificing architectural principles.

### Design Patterns

Several design patterns have been implemented to ensure maintainable and extensible code:

- **Repository Pattern**: Abstracts data access logic and provides a clean separation between the domain model and data access layers
- **Strategy Pattern**: Used for implementing different discount calculation strategies that can be selected at runtime
- **Singleton Pattern**: Applied for services that need to maintain a single instance throughout the application lifecycle

### Technical Features

The application includes several technical implementations to enhance security, performance, and scalability:

- **DTOs (Data Transfer Objects)**: Used throughout the application to decouple the domain model from the presentation layer and to optimize data transfer
- **JWT Authentication**: JSON Web Tokens are used for secure user authentication and authorization
- **Pagination**: Implemented for basket history to efficiently handle large datasets by loading data in manageable chunks
- **Transaction Management**: Ensures data consistency when multiple database operations need to succeed or fail as a unit

### Technology Decisions

#### Dapper vs Entity Framework

I chose Dapper over Entity Framework for this project as a learning opportunity, despite having more experience with EF. Dapper offers better performance and more direct control over SQL queries, though it requires writing more manual code for data operations.

While this decision created some additional challenges during development, it provided valuable insights into the trade-offs between different ORM approaches and the importance of selecting the right tool based on project requirements.

#### Angular

For the frontend framework, I selected Angular for its comprehensive feature set and structured approach, which aligned well with the project requirements. Angular provided built-in solutions for routing, form validation, and state management that were beneficial for implementing the basket functionality.

#### Docker Implementation

I integrated Docker into this project from the start as a practical solution for developing on Linux while ensuring the application would work across different environments. Docker containers helped me keep my development environment clean by isolating all the dependencies inside containers rather than installing them directly on my machine.

This approach made it easier to develop, debug, and test the application locally while also providing a straightforward path to deployment. Using Docker Compose simplified the process of running the application with its database, making the project more accessible for anyone who wants to run it regardless of their operating system.

#### Configuration Management

For this assessment, I implemented a simplified configuration approach with settings stored within the project itself. In production environments, sensitive configuration data (connection strings, API keys, JWT secrets) would typically be externalized using:

- Environment variables
- Secret management services
- Configuration servers
- CI/CD pipeline variables

This architectural decision acknowledges the trade-off between development simplicity and security best practices. A production implementation would leverage infrastructure-appropriate secret management while maintaining the application's configuration structure.

#### API Documentation & Code Style

While tools like Swagger/OpenAPI are valuable for documenting APIs in production applications, I chose not to implement them for this assessment to focus on core functionality. In a production environment, proper API documentation would be essential for both internal and external consumers.

Regarding code style, I follow the principle that well-structured code should be largely self-documenting. I've minimized comments throughout the codebase, focusing instead on clear naming conventions, proper abstraction, and intuitive organization. Comments are reserved for cases where the "why" behind a decision isn't immediately obvious from the code itself.
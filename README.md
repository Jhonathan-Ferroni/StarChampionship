# SalesWebMvc

A complete **ASP.NET Core web application** focused on sales domain management, built with **Microsoft technologies**, **MVC architecture**, **Razor view templates**, and **MySQL persistence**.

This project demonstrates a practical, structured CRUD system for managing business entities such as sellers and departments, while also modeling sales records and their relationships.

---

## Table of Contents

- [Overview](#overview)
- [Core Architecture](#core-architecture)
- [Main Technologies](#main-technologies)
- [CRUD Coverage](#crud-coverage)
- [Domain Model](#domain-model)
- [Project Structure](#project-structure)
- [Database and MySQL Connection](#database-and-mysql-connection)
- [Migrations](#migrations)
- [Validation System](#validation-system)
- [Async Operations and Delete Exception Handling](#async-operations-and-delete-exception-handling)
- [Sales Search Navigation Views](#sales-search-navigation-views)
- [How to Run the Project](#how-to-run-the-project)
- [HTTP Routing](#http-routing)
- [Seed Data](#seed-data)
- [Future Implementations](#future-implementations)

---

## Overview

**SalesWebMvc** is a server-rendered web system that follows the Model-View-Controller pattern to separate responsibilities clearly:

- **Model**: domain entities and business structure.
- **View**: `.cshtml` pages rendered with the Razor view engine.
- **Controller**: HTTP endpoints and flow coordination.

The application uses **Entity Framework Core** for ORM access and is configured to run with a **MySQL** database through **Pomelo + MySqlConnector**.

---

## Core Architecture

### MVC (Model-View-Controller)

The project follows classic MVC composition:

- `Controllers/` handles incoming requests and action methods.
- `Models/` contains entities, enums, and view models.
- `Views/` contains Razor templates used to render server-side HTML.

This structure supports maintainability and explicit separation between:

- business/domain data,
- request logic,
- and UI rendering.

### Razor Views (`.cshtml`)

The UI layer is built using Razor syntax in view files under `Views/`.

Examples:

- `Views/Sellers/*.cshtml`
- `Views/Departments/*.cshtml`
- shared layout in `Views/Shared/_Layout.cshtml`

### Service Layer

In addition to MVC, the project includes application services:

- `SellerService`
- `DepartmentService`
- `SeedingService`

These classes centralize domain operations and data access orchestration for cleaner controller code.

---

## Main Technologies

### Microsoft Framework Stack

- **ASP.NET Core** (Web SDK)
- **ASP.NET Core MVC** (`AddControllersWithViews`)
- **Razor view engine** for server-side rendered pages
- **Dependency Injection** (built-in container)
- **Request Localization** (`en-US` configured)

### Data and Persistence

- **Entity Framework Core 9**
- **Pomelo.EntityFrameworkCore.MySql** (MySQL provider for EF Core)
- **MySqlConnector** (MySQL driver)
- **EF Core Migrations** for schema evolution

### Front-end

- **Bootstrap** (UI styling and layout)
- **jQuery**
- **jQuery Validation + Unobtrusive Validation**

### Runtime

- **.NET 10.0** target framework (`net10.0`)

---

## CRUD Coverage

The project implements CRUD workflows for key entities.

### Sellers

`SellersController` supports:

- **Create** seller
- **Read** seller list and details
- **Update** seller information
- **Delete** seller

The seller flow includes service-based operations and integration with departments.

### Departments

`DepartmentsController` supports:

- **Create** department
- **Read** department list and details
- **Update** department
- **Delete** department

This controller demonstrates a full scaffolded CRUD path with async EF Core operations.

---

## Domain Model

### Entities

- **Department**
  - Has many `Seller`
- **Seller**
  - Belongs to one `Department`
  - Has many `SalesRecord`
- **SalesRecord**
  - Linked to one `Seller`
  - Uses `SaleStatus` enum for business state

### Enumerations

- `SaleStatus`
  - `Pending`
  - `Billed`
  - `Canceled`

---

## Project Structure

```text
SalesWebMvc/
├── Controllers/
├── Data/
│   ├── SalesWebMvcContext.cs
│   └── SeedingService.cs
├── Migrations/
├── Models/
│   ├── Enums/
│   └── ViewModels/
├── Services/
│   └── Exceptions/
├── Views/
│   ├── Departments/
│   ├── Sellers/
│   ├── Shared/
│   └── Home/
├── wwwroot/
└── Program.cs
```

---

## Database and MySQL Connection

The application uses a connection string entry named:

- `SalesWebMvcContext`

Default local example in `appsettings.json`:

```json
"ConnectionStrings": {
  "SalesWebMvcContext": "server=localhost;userid=YOUR_USER;password=YOUR_PASSWORD;database=saleswebmvcappdb"
}
```

EF Core setup is registered in `Program.cs` with:

- `UseMySql(...)`
- `ServerVersion.AutoDetect(...)`
- `MigrationsAssembly("SalesWebMvc")`

---

## Migrations

The repository already includes migration history in `SalesWebMvc/Migrations/`.

To apply migrations to the configured MySQL database:

```bash
dotnet ef database update --project SalesWebMvc
```

To create a new migration:

```bash
dotnet ef migrations add <MigrationName> --project SalesWebMvc
```

---
## Validation System

The project includes a layered validation approach:

- **Server-side validation** through model validation attributes and MVC model binding checks.
- **Client-side validation** using **jQuery Validation** + **Unobtrusive Validation**, providing immediate feedback in forms.

This helps keep invalid data from being submitted and improves UX by showing validation messages early.

---

## Async Operations and Delete Exception Handling

The project also applies recent reliability improvements:

- **Asynchronous operations (`async` / `await`)** are used across service/controller flows to keep I/O-bound operations non-blocking and improve scalability under concurrent requests.
- **Delete exception handling** was added to protect delete flows and return controlled application behavior when integrity constraints or related-record conflicts occur.

---

## Sales Search Navigation Views

The project now includes dedicated sales search navigation views to improve how users explore sales data:

- **Simple Search** for quick filtering with a streamlined navigation path.
- **Grouping Search** for aggregated and grouped visualization of sales results.

These views make sales consultation more intuitive while keeping navigation aligned with the MVC flow.

---

## How to Run the Project

### Prerequisites

- .NET SDK compatible with `net10.0`
- MySQL server available
- Connection string configured in `SalesWebMvc/appsettings.json`

### Steps

1. Restore dependencies:

```bash
dotnet restore SalesWebMvc/SalesWebMvc.csproj
```

2. Apply migrations:

```bash
dotnet ef database update --project SalesWebMvc
```

3. Run the app:

```bash
dotnet run --project SalesWebMvc/SalesWebMvc.csproj
```

4. Open the URL shown in the terminal (for example, `https://localhost:<port>`).

---

## HTTP Routing

The default route is configured as:

```text
{controller=Home}/{action=Index}/{id?}
```

This means:

- Home page: `/`
- Sellers page: `/Sellers`
- Departments page: `/Departments`

---

## Seed Data

On startup, `SeedingService` checks whether the database already has records.
If not, it inserts:

- Departments
- Sellers
- Sales records

This enables immediate local testing with realistic sample data.

---

## Future Implementations

> Reserved space for upcoming project evolution.



# Ordering Service - Database Migrations

This document explains how to manage Entity Framework Core migrations for the Ordering service.

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) installed
- [EF Core CLI tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) installed (`dotnet tool install --global dotnet-ef`)
- Update your connection string in `appsettings.Development.json` if needed

## Common Commands

**Checkout to directory:**

```bash
cd Services/Ordering/Ordering.API
```

**Add a new migration:**
```bash
dotnet ef migrations add <MigrationName> -p ../Ordering.Infrastructure/ -s ../Ordering.API -c OrderContext
```

**Add a second migration:**
```bash
dotnet ef migrations add add-second-migration -p ../Ordering.Infrastructure/ -s ../Ordering.API -c OrderContext
```

**Update the database after migration:**
```bash
dotnet ef database update -p ../Ordering.Infrastructure/ -s ../Ordering.API -c OrderContext
```
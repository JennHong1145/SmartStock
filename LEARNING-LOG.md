# SmartStock Learning Log

## Day 1 - Project Setup & First Entity

### What I learned
- **`Up()` vs `Down()`:** Up() is used to apply changes to the database schema, while Down() is used to revert those changes
- **`__EFMigrationsHistory`:** It is a table that keeps track of the migrations that have been applied to the database.
- **Scoped `DbContext`:** DbContext is registered as a scoped service, meaning a new instance is created for each request.
- **`DeleteBehavior.Restrict`:** The 'Delete' behavior restricts is because the parent entity has related child entitites.

## Day 2 - Product Entity & Relationships

### What I learned
- **`HasPrecision(18, 2)` for `decimal`:** `int` and `bool` have a fixed size in SQL Server, so there's nothing to set. `decimal` needs a precision and a scale, and EF has no safe default for money. Without it EF warns and the value can get cut off. After the migration, `Price` showed up as `decimal(18,2)`.
- **Fluent API vs annotations:** Annotations like `[MaxLength]` are quick, but Fluent API can do more (indexes, precision, delete behavior). I moved my rules into `CategoryConfiguration` and `ProductConfiguration`, and `ApplyConfigurationsFromAssembly` picks them up so `OnModelCreating` stays clean.
- **Unique index vs primary key:** Primary key is what identifies a row, and there's only one per table. The unique index on `SKU` just stops duplicate SKUs. I tried inserting the same SKU twice in SSMS and it got rejected.
- **`HasData` and `Restrict`:** `HasData` needs fixed `Id`s and a fixed date, because `DateTime.UtcNow` would make every migration think the seed data changed. I used `Restrict` from Category to Products so deleting a category can't wipe out its products.
# SmartStock Learning Log

## Day 1 - Project Setup & First Entity

### What I learned
- **`Up()` vs `Down()`:** Up() is used to apply changes to the database schema, while Down() is used to revert those changes
- **`__EFMigrationsHistory`:** It is a table that keeps track of the migrations that have been applied to the database.
- **Scoped `DbContext`:** DbContext is registered as a scoped service, meaning a new instance is created for each request.
- **`DeleteBehavior.Restrict`:** The 'Delete' behavior restricts is because the parent entity has related child entitites.
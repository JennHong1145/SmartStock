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

## Day 3 - Partners Identity & Roles

### What I learned
- **Roles vs users:** `RoleSeeder` only creates the role names (`Admin`, `Staff`). I only supply the name, and Identity generates the `Id` (a GUID), `NormalizedName` and `ConcurrencyStamp`. Users and role assignments are separate steps (`CreateAsync` and `AddToRoleAsync`).
- **`AspNetUserRoles`:** It's the join table for the many-to-many between users and roles. Its primary key is `UserId` + `RoleId`, and EF built it for me.
- **`AddIdentityCore` and scopes:** I used `AddIdentityCore` because the front end is Blazor WASM, so no Razor login UI is needed. The seeder runs in a manual scope because `DbContext` is scoped and there's no HTTP request at startup.
- - **`IdentityDbContext` vs `DbContext`:** A plain `DbContext` has no tables until I define them. `IdentityDbContext<ApplicationUser>` already comes with the user and role tables (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles` and more), so I didn't write any of them. They still get created through my own migration.
- **Where the `AspNet` tables come from:** The base class declares them and sets them up in its own `OnModelCreating`, which is why `base.OnModelCreating(modelBuilder)` has to stay in my override. My `FullName` column shows up in `AspNetUsers` because `ApplicationUser` is the user type I passed in.

## Day 4 - Purchase Orders (Stock In)

### What I learned
- **Enum as string:** By default EF stores an enum as an `int`, so the column shows `0, 1, 2`. With `HasConversion<string>()` it stores `Draft`, `Ordered` and so on, which is readable in SSMS and safe if I reorder the enum later. I also set `HasMaxLength(20)` so it isn't `nvarchar(max)`.
- **Cascade vs Restrict:** Deleting a purchase order cascades to its lines, because the lines mean nothing without the header. Deleting a product or supplier is restricted, because it would erase order history. I tested both in SSMS: the product and supplier deletes failed with a REFERENCE constraint error, and deleting the order removed its lines.
- **Header + lines:** `PurchaseOrder` has many `PurchaseOrderItem`. The line stores its own `UnitCost`, so a later change to the product's price doesn't change what I paid on this order.

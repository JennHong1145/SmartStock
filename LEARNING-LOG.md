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
- **`IdentityDbContext` vs `DbContext`:** A plain `DbContext` has no tables until I define them. `IdentityDbContext<ApplicationUser>` already comes with the user and role tables (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles` and more), so I didn't write any of them. They still get created through my own migration.
- **Where the `AspNet` tables come from:** The base class declares them and sets them up in its own `OnModelCreating`, which is why `base.OnModelCreating(modelBuilder)` has to stay in my override. My `FullName` column shows up in `AspNetUsers` because `ApplicationUser` is the user type I passed in.

## Day 4 - Purchase Orders (Stock In)

### What I learned
- **Enum as string:** By default EF stores an enum as an `int`, so the column shows `0, 1, 2`. With `HasConversion<string>()` it stores `Draft`, `Ordered` and so on, which is readable in SSMS and safe if I reorder the enum later. I also set `HasMaxLength(20)` so it isn't `nvarchar(max)`.
- **Cascade vs Restrict:** Deleting a purchase order cascades to its lines, because the lines mean nothing without the header. Deleting a product or supplier is restricted, because it would erase order history. I tested both in SSMS: the product and supplier deletes failed with a REFERENCE constraint error, and deleting the order removed its lines.
- **Header + lines:** `PurchaseOrder` has many `PurchaseOrderItem`. The line stores its own `UnitCost`, so a later change to the product's price doesn't change what I paid on this order.

## Day 5 - Sales Orders & Invoices

### What I learned
- **FK on one side:** A foreign key is a column, and a column can only sit in one table. So I put `SalesOrderId` in `Invoices`. An order can exist without an invoice, but an invoice can't exist without an order.
- **Why EF can't guess:** Both `SalesOrder` and `Invoice` point to each other, and both look optional. EF can't tell which one is the parent, so I tell it with `HasForeignKey<Invoice>()`.
- **What makes it one-to-one:** The FK alone isn't enough. It's the unique index on `Invoices.SalesOrderId` that stops a second invoice for the same order. I tried adding two in SSMS and the second one failed.
- **Restrict on invoices:** I used `Restrict`, so an order with an invoice can't be deleted. Invoices are financial records.

## Day 6 - Stock Ledger (Audit Trail)

### What I learned
- **Ledger pattern:** I never edit a "current stock" number. Every change is a new row in `StockMovements`, and the balance is the sum of the rows. My test product came out at 67 (100 in, 30 and 10 out, 5 returned, 2 adjusted), both through SQL and through EF.
- **Composite index `(ProductId, MovementDate)`:** Most stock queries filter by product first and then by date, so the index lets SQL Server jump straight to that product's rows. The column order matters, and EF skipped the separate `ProductId` index because this one already covers it.
- **Quantity stays positive:** The movement type decides whether it adds or subtracts. That also fits the `Quantity >= 0` check constraint coming on Day 8. The downside is `Adjustment` can only add for now.
### Event-sourced vs cached balance
- **Event-sourced (sum of movements):** Every unit is explained by a row, I can work out the balance at any past date, and nothing gets overwritten so there are no lost updates. The downside is that reads slow down as the ledger grows, since every balance query adds up many rows.
- **Cached (a `StockQty` column):** Reads are instant, but there's no history to explain why the number is wrong. Two sales can read 10 and both write 9, and a bug can corrupt the number without anyone noticing.
- **Hybrid for later:** Keep the ledger as the source of truth and update a cached `StockQty` in the same transaction as each movement. That gives fast reads and an audit trail, and the cache can be rebuilt from the ledger.

## Day 7 - Concurrency, Soft Delete & Auditing

### What I learned
- **`RowVersion` vs "last one wins":** Without it, if two users load the same product and both change the price, the second save silently overwrites the first and the first change is lost. With `RowVersion`, SQL Server changes the version on every update. EF saves with `WHERE Id = 1 AND RowVersion = <old value>`, so the second save matches 0 rows and EF throws `DbUpdateConcurrencyException`. In my test, user A saved 1300, user B tried 1301 and got stopped, and the database kept 1300.
- **Two contexts needed to test it:** Two variables in the same `DbContext` give the same tracked object, so there's no conflict. I used two scopes so each had its own `AppDbContext`, like two separate users.
- **Soft delete with a query filter:** `HasQueryFilter(x => !x.IsDeleted)` hides deleted rows from normal queries automatically. `IgnoreQueryFilters()` shows them again. When I removed a customer, the row stayed in the table with `IsDeleted = 1`.
- **Audit fields in `SaveChangesAsync`:** I override it so `CreatedAt` and `UpdatedAt` are filled automatically for entities that implement `IAuditable`. I also had to make sure `CreatedAt` isn't overwritten on updates. It only worked after `Product` actually declared `: IAuditable, ISoftDelete`. Before that, `UpdatedAt` stayed `NULL`.

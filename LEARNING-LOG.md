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


## Day 8 – Constraints, Indexes & Computed Columns

- **Check constraints:** I added rules in the database itself, so a negative price, quantity or cost gets rejected even if my C# code forgets to check. I tested it by setting a product price to -5 and SQL Server blocked it with `CK_Products_Price`.
- **Computed column (LineTotal):** SQL Server works out `Quantity * UnitPrice` (or `UnitCost`) by itself, so I never set it in code. That means it can never be out of sync with the quantity and price.
- **Indexes:** EF already indexes every foreign key. I only had to add indexes for the columns people search by: `Customer.Email` and `Supplier.ContactEmail`. I made them non-unique because two people can share an email.

**3 lines from Script-Migration that surprised me:**
1. `ALTER TABLE [SalesOrderItems] ADD [LineTotal] AS [Quantity] * [UnitPrice] PERSISTED;` – a computed column has no type or default, it's just a formula. `PERSISTED` means SQL Server saves the result instead of recalculating it on every read.
2. `ALTER TABLE [Products] ADD CONSTRAINT [CK_Products_Price] CHECK ([Price] >= 0);` – a check constraint is only one line of SQL, but it protects the data from every app and every query.
3. `INSERT INTO [__EFMigrationsHistory] ... VALUES (N'20261123034938_AddConstraints', ...)` – this is how EF remembers which migrations are already applied, and it's the last step inside the transaction.

**Also noticed:** the whole script is wrapped in `BEGIN TRANSACTION` and `COMMIT`, so if one step fails, nothing is half-applied.

**Test:** the negative price update failed with error 547, which is what I wanted. The foreign key also blocked an insert with a missing parent order, so that rule works too.

## Day 9 - Repository, Unit of Work & Service Layer

### What I learned
- **Repository:** It sits between my code and `AppDbContext`, so controllers and services don't touch the DbContext directly. It only adds, updates and removes in memory. It never saves.
- **Unit of Work:** It holds all the repositories and does the one `SaveChangesAsync`. They all share the same `AppDbContext`, so one save covers everything, and my audit and soft delete code still runs. I registered it as scoped because the DbContext is scoped.
- **Transactions:** One `SaveChangesAsync` is already a transaction. I only need `BeginTransactionAsync` when a service saves more than once, like a sale that also writes stock movements.
- **Service layer:** The business rules live here, like unique SKU, parent category must exist, and a category can't be deleted if it's still in use. This way every caller gets the same rules.
- **DTOs:** The API only returns DTOs, never entities. The controller stays thin. It calls the service and turns the result into a status code.
- **Enum result:** Category delete returns an enum, because a plain `bool` can't tell "not found" apart from "still in use".

### What I tested
- `POST /api/Products` with a negative price returned `400` ("The field Price must be between 0 and 1000000"). The `[Range]` rule on the request stopped it before it reached the service.
- I created product 9 (`FUR-001`) and then deleted it. In SSMS the row is still there with `IsDeleted = 1`, and `CreatedAt` and `UpdatedAt` are filled in. So the Unit of Work still goes through my audit and soft delete logic.
- Posting the same SKU a second time returned `409`, so the unique SKU rule in `ProductService` works.
- `GET /api/Products/9` returned `404` after the soft delete, so the query filter also applies when loading by id.

### Learning exercise: is Repository + UoW redundant over EF Core?
The criticism is fair. `DbContext` already tracks changes and saves them together, so it is already a Unit of Work, and each `DbSet` is already a kind of repository. A generic repository on top mostly repeats Add, Remove and Find. It also hides things like `Include` and projections unless I write a new method for each one.

I would use it when the app has real business rules and I want to test the services without a real database. I would also use it when many services need the same data access rules.

I wouldn't use it for a small CRUD app or one with lots of custom queries, because it's extra code for no real gain. I'm keeping it in SmartStock because I'm learning the pattern, but I know the downsides: more layers, more code, and it can limit what I can do with EF.

### Definition of Done
- ProductService and CategoryService go through the repository and Unit of Work. Create, delete and the business rules work in Swagger and SSMS.
- In my own words: the repository hides how data is loaded and saved, and the Unit of Work saves it all in one go. The downside is extra layers on top of something EF already does.

## Day 10 - Transactional Business Logic: Placing a Sales Order

### What I learned
- **Transaction:** Placing an order touches four tables (order, order lines, stock movement, invoice). They must all be saved or none of them. I put them in one transaction, so if any step fails everything is rolled back.
- **Execution strategy:** I turned on `EnableRetryOnFailure()`, which retries after temporary SQL errors. Once that's on, I can't use a bare `BeginTransaction`. A retry has to run the whole block again from the start, so the transaction goes inside `CreateExecutionStrategy().ExecuteAsync(...)`. I also clear the change tracker on failure so a retry starts clean.
- **Stock guard:** Before saving anything, each line checks the stock from the ledger. If I ask for more than there is, a clear `InsufficientStockException` is thrown and the API returns `400`.
- **Race condition:** Two requests can both read "65 in stock" and both sell it. `RowVersion` doesn't help, because they insert different rows. I lock the product row with `UPDLOCK, HOLDLOCK` inside the transaction, so the second request waits and then sees the new stock. I also sort the lines by product Id so two orders can't wait on each other (deadlock).
- **Why two saves:** The stock movements and invoice need the order's Id, which only exists after the first save. Both saves are inside the same transaction.

### What I tested
- **Not enough stock:** 1000 of product 1 returned `400` ("requested 1000, available 67"). SalesOrders, SalesOrderItems and Invoices stayed at 0, and StockMovements stayed at my 5 old test rows.
- **Success:** 2 of product 1 created 1 order, 1 line (LineTotal 2598.00), 1 `SaleOut` movement and 1 invoice (`INV-20261123-000002`).
- **Rollback:** I forced an error after the order was saved. The counts stayed the same, so the transaction rolled back. The order Id also skipped a number, because SQL Server doesn't reuse identity values after a rollback.
- **Race:** I sent 5 orders of 20 at the same time with 65 in stock. The result was three `201` and two `400`.
- **Database after the race:** product 1 stock was exactly 5 (65 - 3 x 20), never negative. There were 4 orders and 4 invoices, so every successful order got its invoice.

### Transaction boundary (learning exercise)
1. Check the order has at least one line, and group duplicate products. This is outside the transaction because it doesn't touch the database.
2. Open the transaction inside the retry strategy.
3. Load the customer and check it exists.
4. For each product: load it, lock the row, read the stock, check there's enough.
5. Save the order and its lines, which gives the order an Id.
6. Add one `SaleOut` stock movement per line.
7. Add the invoice and save.
8. Commit, which releases the locks.

Steps 3 to 7 must succeed or fail together. If the order saved but the invoice failed, I'd have an order with no invoice and stock that never moved.

### Definition of Done
- A successful order creates rows in 4 tables at once: SalesOrders, SalesOrderItems, StockMovements and Invoices.
- A failed or over-quantity order leaves the database unchanged.

## Day 11 - Advanced Querying & Performance

### What I learned
- **Projection:** `ProductListDto` is built with `Select`, so SQL only returns the 6 columns I need (Id, SKU, Name, category name, price, stock). No `Product` or `Category` entities are loaded, and the stock is worked out as a subquery inside the same query.
- **Pagination, filter and sort:** All of it happens in SQL (`Where`, `OrderBy`, `Skip`, `Take`). I sort by `Id` as a tie-breaker, so the same row can't show up on two pages. The total count is a second query, which is normal.
- **N+1:** I wrote the bad version on purpose. It ran 1 query for the products, 1 for the categories, and then 1 stock query for every product, so 10 queries for 8 products. The single projected query needs only 2 (the count and the page). With 1,000 products the bad version would fire 1,002 queries. In C# it looks harmless, but in the SQL log it's obvious.
- **AsNoTracking:** A read-only query doesn't need change tracking. I added `Query()` and `GetByIdNoTrackingAsync` to the repository and used them in the list and detail reads, and for the customer and product lookups when placing an order. I kept tracking where I change the entity, like the deletes and the order I'm saving.
- **IQueryable in the repository:** Exposing `Query()` made projections possible, but it leaks EF into the services. That's the downside I wrote about on Day 9. I only use it inside services.
- **Include vs AsSplitQuery:** For category 1 I included two collections (4 sub-categories and 2 products). One query repeats every sub-category for every product (4 x 2 = 8 rows). `AsSplitQuery` ran 3 small queries instead (1 + 4 + 2 = 7 rows). The gap is small here, but it grows by multiplication: 100 and 100 would be 10,000 rows against about 200. I'd use split queries when I include two or more collections. The cost is more round trips, and the data could change a little between the queries.

### Learning exercise: reading the SQL log
- `Include` loads all 13 columns of `Product`, including `RowVersion` and the audit fields, which a list never needs. My projection only selects 6.
- The soft-delete filter (`IsDeleted = 0`) showed up inside the products subquery, so it also applies through `Include`.
- Each split query repeats the `SELECT TOP(1)` parent subquery instead of reusing the first result.
- The N+1 log was 8 identical `SUM` queries that only differed by the parameter.

### Definition of Done
- The N+1 stock query was found in the SQL log and fixed with one projected query.
- The list endpoint uses `AsNoTracking` and a projection, not tracked entities.
- I can explain when to use `AsSplitQuery`: when I include two or more collections, because a single query multiplies the rows.

## Day 12 - Views & Raw SQL Reporting

### What I learned
- **SQL view from a migration:** EF can't generate a view, so I wrote the SQL myself inside `migrationBuilder.Sql(...)`. `Up()` creates `vw_SalesSummaryByProduct` and `Down()` drops it, so the migration can still be rolled back.
- **Keyless entity:** `SalesSummaryByProduct` has no primary key because a view row isn't a table row. `HasNoKey().ToView("vw_SalesSummaryByProduct")` tells EF to read from the view and not create a table. The migration came out with empty `Up()`/`Down()` for it, which is how I knew `ToView` was working.
- **Querying it:** I query the view like any other `DbSet`. EF just runs `SELECT ... FROM [vw_SalesSummaryByProduct]`. The API returns a `SalesSummaryDto`, not the keyless entity.
- **What the view counts:** Only `Confirmed` and `Shipped` orders, so Draft and Cancelled orders aren't counted as sales. Revenue uses the `LineTotal` computed column from Day 8, so the line math isn't repeated. Only products that have sold show up (inner join).
- **FromSqlInterpolated:** For the date range the view has no date column, so I used `FromSqlInterpolated`. Each `{value}` becomes a SQL parameter. In the log the SQL text had `@p0` and `@p1` and the dates were sent separately as `DateTime2` parameters.
- **SQL injection:** `FromSqlRaw` with string concatenation puts the user's text right into the SQL, so something like `'; DROP TABLE Products; --` would run. Interpolation sends the value as a parameter, so it's only ever data. The trap is `FromSqlRaw($"...{x}")`: the `$` builds the string first, so it's just as unsafe.
- **No ORDER BY inside raw SQL:** EF wraps the raw query in a subquery to add its own `OrderBy`, and SQL Server doesn't allow `ORDER BY` inside that.

### What I tested
- The view returned product 1 with 62 units and 80538.00 revenue. My cross-check query on the raw tables gave the same numbers.
- `GET /api/Reports/sales-summary` ran a single `SELECT` on the view, ordered by revenue.
- The date-range endpoint logged `@p0` and `@p1` as `DateTime2` parameters, not the dates in the SQL text.

### Learning exercise: view + keyless entity vs LINQ
I'd use a view when the report is a big join with grouping that lots of places need, because the logic lives in one spot and EF just reads it. It's also good when someone else, like a BI tool or a DBA, needs the same numbers. This report was a good fit because it joins three tables and groups them, and the result isn't an entity I ever save. I'd stay with LINQ for small or one-off queries, or when the filters change a lot, because a view is another database object I have to keep in sync with a migration.

### Definition of Done
- `vw_SalesSummaryByProduct` exists and returns correct totals (checked against the raw tables).
- I can read it through EF with strongly typed results.
- I can explain why string concatenation in `FromSqlRaw` is a SQL injection risk and `FromSqlInterpolated` isn't.

## Day 13 - Seeding & Migration Lifecycle

### What I learned
- **Two kinds of seed data:** Small fixed lookups (the categories, the roles) stay in `HasData`, because they belong to the schema and need fixed `Id`s. Bigger sample data goes in a `DataSeeder` class, because `HasData` puts every row into a migration. Dozens of fake products would make the migrations noisy and hard to read.
- **Development only:** `DataSeeder` runs at startup inside the existing scope, but only when `app.Environment.IsDevelopment()` is true. So realistic sample data never shows up in production.
- **Guard must match my own data:** My first version skipped seeding if any customer existed. My test database already had one customer from Day 10, so nothing was added. I changed the check to look for my own `SMP-` SKUs. After that I got 21 customers, 5 suppliers, 39 products and 39 stock movements, and a second run changed nothing.
- **Seeder goes through `SaveChangesAsync`:** That way the audit override fills in `CreatedAt`. Opening stock is added as `StockMovements` rows, not a stock column, which matches the ledger pattern.
- **User-secrets:** The connection string now lives in `secrets.json` outside the repo, and I removed it from `appsettings.Development.json`. The app still runs because secrets load automatically in Development. A search for `ConnectionStrings` in the appsettings files comes back empty. Old commits still contain the old string. It's only LocalDB with Windows login, but a real password would need to be changed.
- **`Remove-Migration`:** It deletes the migration files and puts the model snapshot back, but only for a migration that hasn't been applied. I added a scratch `ScratchNote` column, created `ScratchTest`, read its `Up()` and `Down()`, then removed the migration and the property. `git status` showed no migration changes.
- **Roll back and forward:** `Update-Database AddConstraints` ran `Down()` of `AddReportingView`, so the view was gone and the history table ended at `AddConstraints`. `Update-Database` then brought it back.
- **Migration bundle:** `dotnet ef migrations bundle` makes one `efbundle.exe` that applies the migrations without needing the SDK or source code. It has no connection string inside, so you pass `--connection`. I ran it against a separate test database and it applied all the migrations. It only applies migrations, so the `DataSeeder` rows aren't there.

### What I tested
- Ran the seeder twice and the row counts stayed the same.
- Started the app after removing the connection string from `appsettings.Development.json`, and Swagger and the API still worked.
- Rolled back one migration and re-applied it, checking the view and `__EFMigrationsHistory` in SSMS both times.
- Ran the bundle against a throwaway database, checked the history table, then dropped the database.

### Learning exercise: safe migrations on a team project
1. Pull the latest `main` first, so my model and the `ModelSnapshot` start from what everyone else has.
2. Make my model change, then run `Add-Migration` with a clear name.
3. Open the generated file and read `Up()` and `Down()`. Check it only does what I meant, with no surprise drops or renames.
4. Run `Update-Database` on my local database and test the app. Also try rolling back once to check `Down()` works.
5. Commit the migration, its `.Designer.cs` and the updated `AppDbContextModelSnapshot.cs` together. If two people add migrations at the same time, the second one pulls, removes their own migration, and adds it again.

### Definition of Done
- [x] Realistic sample data loads only in Development
- [x] Connection string no longer committed to source control
- [x] Rolled a migration back and forward successfully

## Day 14 - Blazor WASM Client

### What I learned
- **CORS:** The Blazor client runs on its own port (`7075`) and the API on another (`7067`). The browser blocks calls between them unless the API allows it. I added a CORS policy in `Program.cs` and only turn it on in Development.
- **Typed `HttpClient`:** `ProductApiClient` and `OrderApiClient` hold the URLs and JSON handling, so the pages only call methods like `GetPagedAsync`. The template's plain `HttpClient` pointed at the client's own address, so I replaced it with `AddHttpClient<T>` using the API's address.
- **Shared DTOs:** The client uses the same `ProductListDto`, `PagedResult<T>` and `PlaceSalesOrderRequest` classes as the server through `SmartStock.Shared`, so the JSON shape can't drift apart.
- **Paged list:** The Products page sends page, sort, search and direction to `api/Products/paged`. The server does the paging, so the browser only gets 10 rows at a time. It showed 38 products, not 39, because the soft-deleted product is hidden by the query filter.
- **400 vs 409:** A 400 means the request broke a business rule (not enough stock, unknown customer), and the user can fix it and retry. A 409 means someone else changed the same data, so the right message is "try again". The page shows 400 in red and 409 in yellow.
- **Why my order endpoint never gives a real 409:** `PlaceSalesOrderAsync` only inserts rows and never updates a `Product`, so the `RowVersion` check never runs. The `UPDLOCK, HOLDLOCK` lock from Day 10 already stops the race. The 409 catch in `OrdersController` is a safety net. The one place my API returns a real 409 is a duplicate SKU on `POST api/Products`.
- **Turning errors into messages:** `OrderApiClient` reads `{ message }` from my own 400s, validation errors, or plain text, and gives the page one error string.
- **Client checks don't replace server checks:** The form stops an empty product or a quantity under 1 before sending, but the server checks everything again.
- **Empty template:** My project had no `NavMenu.razor` or Bootstrap, so I added a simple nav in `MainLayout.razor` and the Bootstrap link in `index.html`.
- **Two startup projects:** With both set to Start, Microsoft Edge only opened the Swagger tab, but Chrome opened both. So it was the browser, not my code.

### What I tested
- Products page: sorting, search, and Next and Previous.
- Place Order page: a successful order, quantity above stock, an unknown customer, and no product chosen. All behaved as expected.
- 409: I temporarily threw `DbUpdateConcurrencyException` for customer `777` and checked the yellow message and the 409 in the Network tab. Then I removed the throw.

### Definition of Done
- [x] Client calls the API through typed `HttpClient`s
- [x] Paged product list page works
- [x] Place-order form works and shows 400 and 409 clearly
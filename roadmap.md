# SmartStock Self-Review and v2 Roadmap

## Self-review

### What's solid
- **`PlaceSalesOrderAsync`:** a transaction with an execution strategy, a stock guard, sorted lines, and tests that fail when the guard is weakened.
- **Layering:** thin controllers, services, a unit of work, and DTOs only.
- **Reads:** `GetPagedAsync` is one projected query. Sorting and paging happen in SQL with an `Id` tie-breaker.
- **Money rules:** check constraints and computed `LineTotal` columns in the database.
- **Tests:** 8 tests for `PlaceSalesOrderAsync` on in-memory SQLite, each checked by breaking the code on purpose.

### Weak spots (highest risk first)
1. **No authorization.** Anyone can place an order or delete a product. Roles exist but no endpoint uses them.
2. **SKU check-then-insert race.** `CreateAsync` checks for an existing SKU and then inserts. Two requests at once could both pass the check. The unique index protects the data, but the losing request would return a 500 instead of a 409.
3. **Unpaged list endpoint.** `GET api/Products` returns every product.
4. **`StockMovement.ReferenceId` has no foreign key.** This is by design, but nothing stops a bad id.
5. **Locking is SQL Server only.** The SQLite tests cannot prove the race protection. That was verified by hand on SQL Server (5 simultaneous orders against 5 units gave 3 successes and 2 rejections).
6. **Order 409 only tested with a forced exception.** The row locking prevents the real race.
7. **Test gaps.** No tests yet for a soft-deleted product or a zero quantity.

## v2 roadmap

### Must have
- Authentication (JWT) and `[Authorize(Roles = ...)]` on the write endpoints.
- Purchase order receiving: `Received` status plus a `PurchaseIn` movement per line, in one transaction.
- Handle the SKU race so a duplicate returns 409.

### Should have
- Paging on `GET api/Products`, and global exception handling that returns consistent `ProblemDetails`.
- Cached stock on `Product`, updated in the same transaction as each movement, with a rebuild from the ledger.
- Cancel and return flows for sales orders (`Return` movements).
- CI that runs `dotnet test` on every push.
- **Low-stock alerts:** a reorder level per product. A background service checks the ledger totals and sends an email or notification when stock drops below it. Builds on: the stock ledger.

### Nice to have
- **Reporting dashboard:** a Blazor page with sales per product, revenue over time and low-stock items. Builds on: `vw_SalesSummaryByProduct` and the report endpoints from Day 12. It would need a date-range chart and more views, for example monthly revenue.
- **Multi-tenant (multiple stores):** add a `StoreId` to products, customers, orders and stock movements, and scope every query with a global query filter, the same way soft delete works today. The stock ledger would be per store, and a transfer between stores would be two movements. Builds on: global query filters, plus a tenant id taken from the signed-in user. This depends on authentication, so it comes after it.
- **.NET MAUI Blazor Hybrid companion app:** a phone app for warehouse staff to check stock, scan a SKU and place orders. It would reuse the same API, the same `SmartStock.Shared` DTOs and the Razor components from the Blazor client, moved into a shared Razor class library. Builds on: the typed `HttpClient`s and DTOs from Day 14.
- SQL Server tests in a container (Testcontainers) to prove the row locking.
- Category and customer management pages in Blazor.
- Structured logging and health checks.

### Suggested order
1. Authentication and roles
2. Purchase order receiving
3. Paging, error handling and CI
4. Cached stock
5. Dashboard
6. Multi-tenant
7. MAUI companion app
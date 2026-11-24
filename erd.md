# SmartStock ERD

```mermaid
erDiagram
    Category ||--o{ Category : "parent of (Restrict)"
    Category ||--o{ Product : "has (Restrict)"

    Product ||--o{ StockMovement : "ledger rows"
    Product ||--o{ SalesOrderItem : "sold in (Restrict)"
    Product ||--o{ PurchaseOrderItem : "bought in (Restrict)"

    Supplier ||--o{ PurchaseOrder : "supplies (Restrict)"
    PurchaseOrder ||--|{ PurchaseOrderItem : "lines (Cascade)"

    Customer ||--o{ SalesOrder : "places"
    SalesOrder ||--|{ SalesOrderItem : "lines (Cascade)"
    SalesOrder ||--o| Invoice : "billed by (Restrict, unique)"

    SalesOrder ||..o{ StockMovement : "ReferenceId, no FK"

    Category {
        int Id PK
        string Name
        int ParentCategoryId FK
    }
    Product {
        int Id PK
        string SKU UK
        string Name
        decimal Price
        bool IsActive
        int CategoryId FK
        bool IsDeleted
        byte[] RowVersion
    }
    StockMovement {
        int Id PK
        int ProductId FK
        string MovementType
        int Quantity
        datetime MovementDate
        int ReferenceId
    }
    Supplier {
        int Id PK
        string Name
        bool IsDeleted
    }
    Customer {
        int Id PK
        string Name
        bool IsDeleted
    }
    PurchaseOrder {
        int Id PK
        int SupplierId FK
        string Status
        decimal TotalAmount
        byte[] RowVersion
    }
    PurchaseOrderItem {
        int Id PK
        int PurchaseOrderId FK
        int ProductId FK
        int Quantity
        decimal UnitCost
        decimal LineTotal "computed"
    }
    SalesOrder {
        int Id PK
        int CustomerId FK
        string Status
        decimal TotalAmount
        byte[] RowVersion
    }
    SalesOrderItem {
        int Id PK
        int SalesOrderId FK
        int ProductId FK
        int Quantity
        decimal UnitPrice
        decimal LineTotal "computed"
    }
    Invoice {
        int Id PK
        int SalesOrderId FK, UK
        string InvoiceNumber
        bool IsPaid
    }
```

## Notes
- Stock is never stored on `Product`. It is the sum of `StockMovement` rows (`SaleOut` subtracts, everything else adds).
- `Product`, `Customer` and `Supplier` are soft-deleted and hidden by global query filters.
- `Invoice.SalesOrderId` has a unique index, which makes the relationship one-to-one.
- `ReferenceId` points at the sales or purchase order that caused the movement, but it has no foreign key.
- `vw_SalesSummaryByProduct` is a keyless view for reporting and isn't drawn.
- ASP.NET Identity tables (`AspNetUsers`, `AspNetRoles` and so on) are left out.
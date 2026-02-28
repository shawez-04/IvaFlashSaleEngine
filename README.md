# 🚀 IvaFlashSale Engine

> High-Concurrency Flash Sale Backend built with **.NET 8 LTS + PostgreSQL**
> Designed to prevent **Overselling**, **Duplicate Transactions**, and **Race Conditions** during high-traffic product drops.

---

## 🔥 Live Demo

🌍 **Production URL:**
`[https://ivaflashsaleengine.onrender.com](https://ivaflashsaleengine.onrender.com/swagger/index.html)`

📦 **Deployment:** Dockerized & hosted on Render
🗄 **Database:** PostgreSQL (Neon - Serverless)

---

# 🧠 Problem Statement

In flash-sale scenarios (e.g., limited sneaker or clothing drops), thousands of users attempt to purchase limited inventory simultaneously.

Without proper safeguards, this leads to:

* ❌ Overselling (Negative stock)
* ❌ Duplicate orders (Double-click / retry)
* ❌ Race conditions
* ❌ Data inconsistency
* ❌ Unauthorized inventory manipulation

---

# 🏗 Architecture Overview

```text
Client → Controller → Service Layer → EF Core → PostgreSQL
              ↑
        JWT Authentication
              ↑
     Global Exception Middleware
```

### Layer Responsibilities

| Layer       | Responsibility                                        |
| ----------- | ----------------------------------------------------- |
| Controllers | HTTP routing, header validation, claim extraction     |
| Services    | Business logic, transactions, idempotency enforcement |
| DTOs        | Public API contract isolation                         |
| DbContext   | Database mapping & concurrency control                |
| Middleware  | Centralized error handling                            |

---

# ⚙️ Tech Stack

| Layer             | Technology           |
| ----------------- | -------------------- |
| Framework         | .NET 8 LTS           |
| Language          | C#                   |
| Database          | PostgreSQL (Neon)    |
| ORM               | EF Core 8            |
| Authentication    | JWT                  |
| Password Security | BCrypt               |
| Testing           | xUnit + Moq          |
| Logging           | Serilog              |
| Hosting           | Render               |
| Containerization  | Docker (Multi-stage) |

---

# 🚨 Core Engineering Features

---

## ⚡ 1. High-Concurrency Protection (No Overselling)

### The Problem

If two users attempt to buy the last item at the same time, both may see `Stock = 1`.

### The Solution

Implemented **Optimistic Concurrency Control** using PostgreSQL’s hidden `xmin` column.

* Mapped `RowVersion` to `xmin`
* EF Core checks row version during update
* On conflict → `DbUpdateConcurrencyException`
* Middleware converts it to **HTTP 409 Conflict**

### Result

✔ Zero negative stock
✔ No double-sells under concurrent requests

---

## 🔁 2. Distributed Idempotency (Double-Click Protection)

Flash sale users may retry or double-click due to latency.

### Implementation

* Required header: `X-Idempotency-Key`
* GUID per purchase request
* Persisted in `Orders` table
* UNIQUE database index
* Duplicate key → return original success response

### Result

✔ Safe retries
✔ No duplicate orders
✔ No double stock decrement

---

## 🔐 3. JWT Authentication & RBAC

* Stateless JWT authentication
* Claim-bound `NameIdentifier` (User ID)
* Role-Based Access Control

| Role   | Access           |
| ------ | ---------------- |
| Public | View products    |
| User   | Purchase         |
| Admin  | Manage inventory |

Security enhancements:

* BCrypt password hashing
* Duplicate username prevention
* Claim mapping fix (`JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear()`)

---

## 💰 4. Financial Precision

Configured:

```
numeric(18,2)
```

via Fluent API:

```csharp
HasPrecision(18,2)
```

Prevents floating-point rounding errors in high-volume transactions.

---

## 🗑 5. Soft Deletion Strategy

Products are not permanently deleted.

Instead:

```csharp
IsActive = false;
```

Preserves:

* Order history
* Analytics integrity
* Audit trail

---

# 🧪 Testing & Verification

Implemented a comprehensive **xUnit test suite**.

### Coverage Includes:

* Purchase success flow
* Out-of-stock rejection
* Idempotency duplicate handling
* Password hashing validation
* Soft-delete state verification

✔ 100% passing test suite
✔ Verified transaction integrity
✔ Verified business rule enforcement

---

# 🐳 Docker & Deployment

### Multi-Stage Docker Build

1. SDK stage → Restore & Publish
2. Runtime stage → Lightweight ASP.NET Alpine image

Benefits:

* Smaller container size
* Faster deployment
* Reduced attack surface
* Cloud-ready dynamic port binding

---

# 📊 Concurrency Stress Testing

Scenario:

* 5 simultaneous requests
* 1 item in stock

Results:

* 1 → `200 OK`
* 4 → `409 Conflict`
* Final stock = 0

✔ No negative values
✔ No duplicate transactions

---

# 📂 Project Structure

```text
IvaFlashSale/
├── Controllers/
│   ├── AuthController.cs
│   ├── ProductsController.cs
│   └── PurchaseController.cs
├── Services/
│   ├── AuthService.cs
│   ├── ProductService.cs
│   └── PurchaseService.cs
├── Models/
│   ├── User.cs
│   ├── Product.cs
│   └── Order.cs
├── Data/
│   ├── AppDbContext.cs
│   └── DbInitializer.cs
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs
├── Tests/
│   └── ServiceTests.cs
└── Dockerfile
```

---

# 🚀 Scalability Roadmap

Future SDE-2 enhancements:

* Redis caching for product lookups
* Redis-backed idempotency key store
* Async order queue (RabbitMQ / Azure Service Bus)
* Rate limiting middleware
* Load testing automation

---

# 🎯 Resume Summary

> Architected and deployed a high-concurrency Flash Sale backend in .NET 8 LTS using PostgreSQL optimistic concurrency and attribute-driven idempotency to prevent overselling and duplicate transactions. Verified core business logic with automated xUnit tests and deployed via Docker to a cloud-hosted environment.

---

# 🏁 Final Status

✔ Production-ready
✔ Concurrency-safe
✔ Idempotent
✔ Secure
✔ Tested
✔ Dockerized
✔ Cloud-deployed


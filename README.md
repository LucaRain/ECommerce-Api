# ECommerce API

A production-ready RESTful e-commerce API built with .NET 10, Clean Architecture, PostgreSQL, and Redis.

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Environment Variables](#environment-variables)
- [API Endpoints](#api-endpoints)
- [Authentication](#authentication)
- [Project Structure](#project-structure)
- [Docker](#docker)

---

## Features

- JWT Authentication with Refresh Tokens
- Role-based Authorization (Admin / Customer)
- Products — CRUD, multiple images, pagination, search, filter, sort
- Categories — CRUD
- Cart — Redis-powered, persists across devices
- Orders — stock management, status transitions, cancellation with stock restore
- Reviews — verified purchase only, one per product per customer
- Admin Dashboard — revenue, top products, recent orders
- Redis Caching — product listings and dashboard cached automatically
- FluentValidation — request validation on all endpoints
- Global Error Handling — consistent error responses across all endpoints
- Rate Limiting — brute force protection on auth endpoints
- Docker — full containerization with auto migrations on startup

---

## Tech Stack

| Technology              | Purpose                |
| ----------------------- | ---------------------- |
| .NET 10                 | Web API framework      |
| PostgreSQL 16           | Primary database       |
| Entity Framework Core   | ORM + migrations       |
| Redis 7                 | Cart storage + caching |
| JWT Bearer              | Authentication         |
| BCrypt                  | Password hashing       |
| FluentValidation        | Request validation     |
| Docker + Docker Compose | Containerization       |
| Scalar                  | API documentation      |

---

## Architecture

This project follows Clean Architecture with 3 layers:

```
ECommerce/
├── src/
│   ├── ECommerce.API              # HTTP layer — controllers, middleware, Program.cs
│   ├── ECommerce.Application      # Business layer — entities, DTOs, interfaces
│   └── ECommerce.Infrastructure   # Data layer — DB, Redis, services, file storage
├── Dockerfile
└── docker-compose.yml
```

Dependency rule: API -> Application <- Infrastructure

- API knows about Application
- Infrastructure knows about Application
- Application knows nothing about either — pure business logic, no external dependencies

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Configuration

1. Copy `appsettings.example.json` to `appsettings.Development.json`
2. Fill in your values
3. Never commit `appsettings.Development.json` — it is git-ignored

### Option 1 — Docker (recommended)

```bash
# clone the repo
git clone https://github.com/yourusername/ecommerce-api.git
cd ecommerce-api

# start everything (PostgreSQL + Redis + API)
docker compose up --build
```

API available at `http://localhost:5000`
API docs available at `http://localhost:5000/scalar/v1`

### Option 2 — Local development

```bash
# start only PostgreSQL and Redis
docker compose up -d postgres redis

# run the API
dotnet run --project src/ECommerce.API
```

Migrations run automatically on startup — no need to run `dotnet ef database update` manually.

---

## Environment Variables

| Variable                               | Description                            |
| -------------------------------------- | -------------------------------------- |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string           |
| `ConnectionStrings__Redis`             | Redis connection string                |
| `Jwt__Secret`                          | JWT signing secret (min 32 characters) |
| `Jwt__Issuer`                          | JWT issuer                             |
| `Jwt__Audience`                        | JWT audience                           |
| `App__BaseUrl`                         | Base URL used for image URLs           |

In development these live in `appsettings.Development.json`. In production always use environment variables — never commit secrets.

---

## API Endpoints

### Auth

| Method | Endpoint                  | Auth     | Description                        |
| ------ | ------------------------- | -------- | ---------------------------------- |
| POST   | `/api/auth/register`      | —        | Register new customer              |
| POST   | `/api/auth/login`         | —        | Login, returns JWT + refresh token |
| GET    | `/api/auth/me`            | Required | Get current user info              |
| POST   | `/api/auth/refresh-token` | —        | Get new JWT using refresh token    |
| POST   | `/api/auth/revoke-token`  | —        | Logout / revoke refresh token      |

### Categories

| Method | Endpoint               | Auth  | Description        |
| ------ | ---------------------- | ----- | ------------------ |
| GET    | `/api/categories`      | —     | Get all categories |
| GET    | `/api/categories/{id}` | —     | Get category by ID |
| POST   | `/api/categories`      | Admin | Create category    |
| DELETE | `/api/categories/{id}` | Admin | Delete category    |

### Products

| Method | Endpoint                              | Auth  | Description                          |
| ------ | ------------------------------------- | ----- | ------------------------------------ |
| GET    | `/api/products`                       | —     | Get products (paginated, filterable) |
| GET    | `/api/products/{id}`                  | —     | Get product by ID                    |
| POST   | `/api/products`                       | Admin | Create product                       |
| PUT    | `/api/products/{id}`                  | Admin | Update product                       |
| DELETE | `/api/products/{id}`                  | Admin | Delete product                       |
| POST   | `/api/products/{id}/images`           | Admin | Upload product image                 |
| DELETE | `/api/products/{id}/images/{imageId}` | Admin | Delete product image                 |

#### Product query parameters

```
GET /api/products?page=1&limit=10&search=iphone&categoryId=xxx&minPrice=100&maxPrice=999&sortBy=price&sortOrder=asc
```

### Cart

| Method | Endpoint                 | Auth     | Description           |
| ------ | ------------------------ | -------- | --------------------- |
| GET    | `/api/cart`              | Required | Get current cart      |
| POST   | `/api/cart`              | Required | Add item to cart      |
| PUT    | `/api/cart/{cartItemId}` | Required | Update item quantity  |
| DELETE | `/api/cart/{cartItemId}` | Required | Remove item from cart |
| DELETE | `/api/cart`              | Required | Clear entire cart     |
| POST   | `/api/cart/checkout`     | Required | Convert cart to order |

### Orders

| Method | Endpoint                  | Auth     | Description         |
| ------ | ------------------------- | -------- | ------------------- |
| POST   | `/api/orders`             | Required | Place a new order   |
| GET    | `/api/orders/my-orders`   | Required | Get my orders       |
| GET    | `/api/orders/{id}`        | Required | Get order by ID     |
| GET    | `/api/orders`             | Admin    | Get all orders      |
| PATCH  | `/api/orders/{id}/status` | Admin    | Update order status |

#### Order status transitions

```
Pending -> Shipped -> Delivered
Pending -> Cancelled
Shipped -> Cancelled
```

Delivered and Cancelled orders cannot be modified.

### Reviews

| Method | Endpoint                                       | Auth     | Description                         |
| ------ | ---------------------------------------------- | -------- | ----------------------------------- |
| GET    | `/api/products/{productId}/reviews`            | —        | Get product reviews                 |
| POST   | `/api/products/{productId}/reviews`            | Required | Add review (verified purchase only) |
| DELETE | `/api/products/{productId}/reviews/{reviewId}` | Required | Delete review                       |

### Dashboard

| Method | Endpoint         | Auth  | Description     |
| ------ | ---------------- | ----- | --------------- |
| GET    | `/api/dashboard` | Admin | Get admin stats |

---

## Authentication

This API uses JWT Bearer tokens.

### Login flow

```bash
# 1. Login to get tokens
POST /api/auth/login
{
  "email": "user@example.com",
  "password": "Password1"
}

# Response
{
  "token": "eyJ...",            # use for API calls, expires in 2 hours
  "refreshToken": "abc123...",  # use to get new JWT, expires in 30 days
  "tokenExpiry": "2026-03-04T...",
  "fullName": "John Doe",
  "email": "user@example.com",
  "role": "Customer"
}

# 2. Use JWT in requests
Authorization: Bearer eyJ...

# 3. When JWT expires, get a new one
POST /api/auth/refresh-token
{ "refreshToken": "abc123..." }
```

### Roles

- Customer — browse, cart, order, review
- Admin — full access including product management and dashboard

### Make yourself Admin

```bash
docker exec -it ecommerce_db psql -U postgres -d ecommerce -c "UPDATE \"Users\" SET \"Role\" = 'Admin' WHERE \"Email\" = 'your@email.com';"
```

---

## Docker

```bash
# start everything
docker compose up --build -d

# view logs
docker compose logs api
docker compose logs postgres
docker compose logs redis

# stop everything
docker compose down

# stop and wipe all data
docker compose down -v
```

### Services

| Service    | Port | Description          |
| ---------- | ---- | -------------------- |
| API        | 5000 | .NET application     |
| PostgreSQL | 5432 | Primary database     |
| Redis      | 6379 | Cache + cart storage |

---

## Project Structure

```
ECommerce/
├── src/
│   ├── ECommerce.API/
│   │   ├── Controllers/          # HTTP endpoints
│   │   ├── Middleware/           # Global error handling
│   │   ├── Validators/           # FluentValidation rules
│   │   ├── wwwroot/images/       # Uploaded product images
│   │   └── Program.cs            # App configuration and DI
│   │
│   ├── ECommerce.Application/
│   │   ├── DTOs/                 # Request and response models
│   │   ├── Entities/             # Database entities
│   │   ├── Exceptions/           # Custom exception types
│   │   └── Interfaces/           # Service contracts
│   │
│   └── ECommerce.Infrastructure/
│       ├── Data/                 # DbContext and migrations
│       └── Services/             # Service implementations
│
├── Dockerfile
├── docker-compose.yml
├── appsettings.example.json
└── README.md
```

---

## License

MIT License — free to use as a template or for learning purposes.

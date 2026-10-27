# TheCart24 — E-Commerce Platform

## 🔐 Login Credentials

> **Testing accounts only.** Do not use real production passwords in this file.

### Super Admin

```json
{
  "email": "buyer1@example.com",
  "password": "Cart24@Test123"
}
```

### Admin

```json
{
  "email": "seller2@example.com",
  "password": "Cart24@Test123"
}
```

### User 1

```json
{
  "email": "user1@example.com",
  "password": "Cart24@Test123"
}
```

### User 2

```json
{
  "email": "user2@example.com",
  "password": "Cart24@Test123"
}
```

### User 3

```json
{
  "email": "user3@example.com",
  "password": "Cart24@Test123"
}
```

**Note:** These are the account details supplied for project testing. Actual permissions depend on the roles assigned to each account in the backend.

### For Swagger Testing

```json
{
  https://localhost:YOUR_PORT/swagger
}
```

---

## 📌 Project Overview

**TheCart24** is an e-commerce web application built to provide an online shopping experience with product browsing, cart management, checkout, order tracking, discount schemes, and administrative functionality.

The project uses an Angular frontend and a .NET 10 Web API backend with SQL Server for data persistence.

## 🚀 Features

### Customer Features
- User registration and login
- Browse available products
- View product details and prices
- Add products to the shopping cart
- Checkout with delivery information
- Apply available discounts during checkout
- View personal orders
- Track order status
- Cancel orders where permitted

### Admin Features
- Access the administrative dashboard
- Manage seller accounts
- View dashboard statistics
- Manage discount schemes
- Control authorized administrative operations

### Seller Dashboard
- View total products
- View total orders
- Monitor pending, shipped, and delivered orders
- View sales summary
- Access dashboard data through the backend API

### Discount System
- Create product-specific discounts
- Configure discount percentages and validity dates
- Activate or deactivate discounts
- Retrieve active discount announcements
- Apply eligible discounts during checkout

## 🛠️ Technology Stack

### Frontend
- Angular
- TypeScript
- HTML5
- CSS3
- Angular Router
- Angular HttpClient

### Backend
- C#
- .NET 10 Web API
- Entity Framework Core
- JWT Authentication
- Role-based authorization
- Swagger / OpenAPI

### Database
- Microsoft SQL Server
- Entity Framework Core Code First migrations

### Version Control
- Git
- GitHub

## 📂 Project Structure

```text
TheCart24/
├── TheCart24.Api/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Models/
│   ├── Services/
│   ├── Repositories/
│   └── Program.cs
│
├── TheCart24.Frontend/
│   └── Angular application
│
├── TheCart24.slnx
└── README.md
```

## ⚙️ Prerequisites

Install the following tools before running the project:

- Visual Studio with the required .NET development workload
- .NET 10 SDK
- Node.js and npm
- Angular CLI compatible with the project
- Microsoft SQL Server
- Git

## ▶️ Running the Backend

1. Open `TheCart24.slnx` in Visual Studio.
2. Configure the SQL Server connection string securely.
3. Configure JWT authentication settings.
4. Apply the required Entity Framework Core migrations.
5. Run the `TheCart24.Api` project.

Default development API URL:

```text
https://localhost:7187
```

Swagger endpoint, if enabled:

```text
https://localhost:7187/swagger
```

## ▶️ Running the Frontend

Open Git Bash or a terminal in the frontend directory:

```bash
cd TheCart24.Frontend
npm install
ng serve
```

Open the local URL shown in the terminal. The current development frontend has been configured to run at:

```text
http://localhost:57513
```

Ensure the backend is running and the API URL matches the frontend configuration.

## 🔑 Authentication and Authorization

The backend uses JWT bearer authentication to protect authorized endpoints.

- Users must log in to access protected customer functionality.
- Administrative endpoints require the appropriate role.
- API requests requiring authentication must include a valid bearer token.
- Role assignments and permissions are determined by the backend configuration.

Never commit production passwords, JWT signing keys, or database credentials to source control.

## 🌿 Git Workflow

Pull the latest changes:

```bash
git pull
```

Stage modified files:

```bash
git add .
```

Commit changes:

```bash
git commit -m "Describe your changes"
```

Push changes to GitHub:

```bash
git push
```

## 🔒 Security Notes

- Use test accounts only for development.
- Keep database credentials and JWT signing keys outside committed source files.
- Use environment-specific configuration for local development and production.
- Enforce authorization and resource ownership on the backend.
- Never rely on frontend route guards alone to secure API endpoints.
- Change or rotate any credentials that have been exposed publicly.

## 📈 Future Enhancements

- Complete seller-specific product management
- Seller order management and fulfillment
- Product search, filtering, and sorting
- Improved inventory management
- Payment gateway integration
- Order and sales reporting
- Enhanced responsive dashboard UI

## 👨‍💻 Project Status

TheCart24 is under active development. Features and API endpoints may change as the application evolves.

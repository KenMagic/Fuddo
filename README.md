# Fuddo – Online Sales Management System

## Overview
Fuddo is an **ASP.NET Core MVC** web application for online sales and order management.
It supports **customers** in browsing products, placing and tracking orders, and **administrators** in managing inventory, categories, and order statuses.

---

## Features

### Customer
- **Account Management**
  - Register and log in.
  - Update personal information.

- **Product Browsing**
  - View product categories and detailed product pages.
  - Search products by name or category.

- **Order Management**
  - Place orders with shipping address and notes.
  - Track order status (Pending → Processing → Shipped → Delivered).
  - Receive **email notifications** when the status changes.

---

### Admin
- **Category Management**
  - Add, edit, delete categories with validation.

- **Product Management**
  - CRUD operations for products.
  - Upload and preview multiple product images.

- **Order Management**
  - View detailed order information.
  - Update order status or cancel orders.
  - Stock validation before confirming orders.

---

## Highlights
- **Multiple product images** with preview before uploading.
- **Order status workflow** with controlled transitions.
- **Email notifications** for order status changes.
- **Bootstrap 5 UI** with breadcrumbs, badges, and dynamic alerts.
- **Validation** for stock, categories, and duplicate names.

---

## Technologies

- **Backend:** ASP.NET Core MVC 8, Entity Framework Core
- **Database:** SQL Server
- **Frontend:** Razor Pages, Bootstrap 5
- **Authentication:** ASP.NET Identity
- **Email:** SMTP (for notifications)

---

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/KenMagic/Fuddo

# 🏨 Hotel Booking System

![.NET](https://img.shields.io/badge/.NET-ASP.NET%20Core-blue)
![Next.js](https://img.shields.io/badge/Next.js-Frontend-black)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Database-blue)
![Docker](https://img.shields.io/badge/Docker-Containerized-blue)
![License](https://img.shields.io/badge/license-MIT-green)

A modern, full-stack hotel management platform built with **ASP.NET Core**, **PostgreSQL**, and **Next.js**.  
The system provides role-based access for administrators, staff, and guests, covering the core workflows of a real-world hotel operation.

---

## 🚀 Overview

This application is designed to simulate a production-grade hotel management system. It handles the full lifecycle of hotel operations, including room management, reservations, check-in/check-out, housekeeping, payments, and analytics.

The architecture follows industry best practices with clear separation of concerns and scalable design patterns.

---

## 🧩 Features

### 🛏️ Room Management

- Manage hotel rooms and availability
- Track room status:
  - Available
  - Occupied
  - Maintenance
- Update pricing and room details

### 📅 Reservations & Bookings

- Create, update, and cancel reservations
- Prevent overlapping bookings
- Track booking lifecycle (upcoming, active, completed)

### 🔑 Check-In / Check-Out

- Process guest check-ins and check-outs
- Automatically update room status
- Maintain booking history

### 🧹 Housekeeping

- Create and assign housekeeping tasks
- Track task progress and completion
- Support room readiness workflows

### 💳 Payments & Revenue

- Record booking payments
- Track paid vs pending amounts
- Monitor revenue metrics

### 📊 Dashboard Analytics

- Occupancy overview
- Booking statistics
- Revenue tracking
- Operational insights

---

## 🛠️ Tech Stack

### Backend

- ASP.NET Core Web API
- Clean Architecture
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- Role-based Authorization

### Frontend

- Next.js (App Router)
- TypeScript
- React
- Component-based UI

### DevOps

- Docker
- Docker Compose
- Environment-based configuration

---

## 🏗️ Architecture

The backend follows **Clean Architecture principles**, ensuring maintainability and scalability.

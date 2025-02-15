# 🚀 CleanArchitecture .NET 8 WebAPI - Enterprise-Grade Solution

[![.NET](https://img.shields.io/badge/.NET-8.0-blueviolet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![Architecture](https://img.shields.io/badge/architecture-Clean%20Architecture-brightgreen)](https://)
[![DDD](https://img.shields.io/badge/pattern-DDD%20%7C%20CQRS-blue)](https://)

**A production-ready WebAPI template** implementing modern software practices with robust security, scalability, and maintainability at its core. Built for .NET 8 with ❤️

![Clean Architecture Diagram](https://via.placeholder.com/800x400.png?text=Clean+Architecture+Diagram) 
*(Consider adding your architecture diagram here)*

## 🌟 Key Features

### 🏗️ Foundation
- **Clean Architecture** with Domain/Application/Infrastructure/Web layers
- **Domain-Driven Design** with modular business capabilities
- **CQRS Pattern** (Dapper + EF Core)
- Transactional Outbox with Hangfire

### 🛡️ Security First
- JWT Authentication with Microsoft Identity
- Refresh Tokens in HttpOnly cookies
- Dual Context Strategy (IdentityContext + ApplicationDbContext)
- Role & Permission-based Authorization

### ⚡ Modern Tooling
- Minimal API Endpoints with MapGroup
- MediatR Pipeline Behaviors:
  - Caching (Memory + Redis)
  - Validation & Error Handling (RFC Standards)
  - Performance Tracking
- Real-time Notifications via SignalR
- NSwag Client Generation

### 📈 Observability
- Seq Integration for centralized logging
- Structured logging throughout
- Monitoring-ready architecture

## 🛠️ Getting Started

### Prerequisites
- .NET 8 SDK
- Docker (for containerized dependencies)
- PostgreSQL/Redis

### Installation
```bash
git clone https://github.com/salmanshafiq00/cleanarchitecture.git
cd cleanarchitecture
docker-compose up -d
dotnet restore
dotnet run --project src/WebApi

🏭 Solution Structure
src/
├── Domain/          # Core business models
├── Application/     # Use cases & business logic
├── Infrastructure/  # External implementations
├── WebApi/             # API endpoints & DI
tests/               # Unit & integration tests

🚦 Quality Assurance
RFC-compliant error responses
FluentValidation integration
Transactional consistency guarantees
Centralized package management
CI/CD-ready configuration

🚧 Roadmap
Hybrid Caching Implementation
Comprehensive Test Suite
ASP.NET Core 8 Performance Optimizations
Kubernetes Deployment Samples
GRPC Endpoints

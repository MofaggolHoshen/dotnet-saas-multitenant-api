# Core Concepts - Domain-Driven Design Fundamentals

> **A comprehensive guide to the foundational patterns and principles used in this .NET SaaS Multi-Tenant API**

Welcome to the core concepts documentation! This guide explains the Domain-Driven Design (DDD) and Clean Architecture patterns that form the foundation of this application.

---

## 📚 Table of Contents

- [What is Domain-Driven Design?](#what-is-domain-driven-design)
- [Core Building Blocks](#core-building-blocks)
- [Domain Layer Components](#domain-layer-components)
- [Visual Class Diagrams](#-visual-class-diagrams)
- [Learning Paths](#learning-paths)
- [Documentation Index](#documentation-index)
- [Quick Reference](#quick-reference)

---

## What is Domain-Driven Design?

**Domain-Driven Design (DDD)** is an approach to software development that focuses on:

1. **Understanding the Business Domain** - Deep collaboration with domain experts
2. **Modeling the Core Domain** - Creating a rich domain model with business logic
3. **Ubiquitous Language** - Shared vocabulary between developers and business
4. **Strategic Design** - Bounded contexts and context mapping
5. **Tactical Patterns** - Entities, Value Objects, Aggregates, Domain Events, etc.

### Why DDD for This Project?

This SaaS multi-tenant API uses DDD because:

- ✅ **Complex Business Rules** - User management, roles, permissions, multi-tenancy
- ✅ **Rich Domain Model** - Entities with behavior, not just data containers
- ✅ **Clear Boundaries** - Aggregates protect business invariants
- ✅ **Maintainability** - Domain logic separated from infrastructure
- ✅ **Testability** - Pure domain logic without dependencies

---

## Core Building Blocks

### 🏗️ Foundational Patterns

These are the essential building blocks that everything else depends on:

#### 1. **[BaseEntity](./BaseEntity.md)** - Entity Identity & Lifecycle

**What:** Base class for all entities providing common identity and audit fields.

**Key Concepts:**
- Unique identity (Id)
- Audit timestamps (CreatedAtUtc, UpdatedAtUtc)
- Soft delete support (IsDeleted)
- Lifecycle methods (MarkUpdated())

**When to Use:** Every entity in your domain should inherit from BaseEntity.

---

#### 2. **[AggregateRoot](./AggregateRoot.md)** - Consistency Boundaries

**What:** The main entity that controls access to a cluster of related objects (aggregate).

**Key Concepts:**
- Transaction boundaries
- Domain event collection
- Encapsulates child entities
- Enforces business invariants

**When to Use:** Top-level entities that own other entities (User, Tenant, Role).

---

#### 3. **[ValueObject](./ValueObject.md)** - Immutable Domain Primitives

**What:** Immutable objects identified by their values, not identity.

**Key Concepts:**
- No identity (Id)
- Immutable (cannot change after creation)
- Equality by value (not reference)
- Self-validating

**When to Use:** Email, Password, Money, Address, TenantId - concepts defined by their attributes.

---

#### 4. **[Result Pattern](./Result.md)** - Railway-Oriented Programming

**What:** A pattern for handling success/failure without exceptions.

**Key Concepts:**
- Result.Success() or Result.Failure(error)
- Avoid exceptions for expected failures
- Clear error handling
- Composable operations

**When to Use:** All operations that can fail due to business rules.

---

#### 5. **[Error](./Error.md)** - Structured Error Handling

**What:** Standardized error representation with codes and messages.

**Key Concepts:**
- Error codes and types
- Descriptive messages
- Error metadata
- Consistent error handling

**When to Use:** Whenever you return Result.Failure().

---

## Domain Layer Components

### 🎯 Business Logic & Models

#### **[Domain Entities](./DomainEntities.md)** - Core Business Objects

**The 4 Core Entities:**

1. **👤 User** - Manages user authentication, roles, and lifecycle
2. **🏢 Tenant** - Represents a customer organization (multi-tenancy)
3. **🎭 Role** - Aggregates permissions for authorization
4. **🔐 Permission** - Defines access rights (resource:action pattern)

**Key Patterns:**
- Rich domain model (behavior + data)
- Private setters (encapsulation)
- Factory methods for creation
- Domain events for state changes

---

#### **[Value Objects](./ValueObjects.md)** - Type-Safe Primitives

**The 4 Value Objects:**

1. **📧 Email** - Validated email addresses
2. **🔒 Password** - Enforced password strength
3. **🏢 TenantId** - Strongly-typed tenant identifier
4. **💳 SubscriptionTier** - Subscription plan enumeration

**Benefits:**
- Compile-time type safety
- Centralized validation
- Self-documenting code
- Prevents primitive obsession

---

#### **[Domain Events](./DomainEvents.md)** - Capturing State Changes

**The 6 Domain Events:**

1. UserCreatedEvent - New user registered
2. UserDeactivatedEvent - User deactivated
3. RoleAssignedEvent - Role granted to user
4. PasswordChangedEvent - Password updated
5. TenantProvisionedEvent - New tenant created
6. Additional extensibility events

**Benefits:**
- Loose coupling between aggregates
- Audit trail of changes
- Integration with external systems
- Event-driven architecture support

---

#### **[Domain Exceptions & Repositories](./DomainExceptionsAndRepositories.md)** - Infrastructure Abstractions

**5 Domain Exceptions:**
- DomainException (base)
- TenantNotFoundException
- UserNotFoundException
- UserAlreadyExistsException
- DomainInvalidOperationException

**4 Repository Interfaces:**
- IUserRepository - User data access
- ITenantRepository - Tenant data access
- IRoleRepository - Role data access
- IUnitOfWork - Transaction coordination

**2 Domain Services:**
- ITenantIsolationService - Multi-tenancy enforcement
- IPasswordHashingService - Password security

---

## 📊 Visual Class Diagrams

### **[DDD Class Diagrams](./DDD-ClassDiagrams.md)** - Complete Visual Reference

**Comprehensive Mermaid class diagrams showing the entire domain architecture:**

#### **📐 12 Detailed Diagrams**

1. **Complete Domain Layer Overview** - High-level architecture
2. **Foundational Building Blocks** - BaseEntity, AggregateRoot, ValueObject, Result, Error
3. **Entity Hierarchy** - All 4 entities with full member details
4. **Entity Relationships (ERD)** - Database-style relationship diagram
5. **Value Object System** - All 4 value objects with validation logic
6. **Domain Events Architecture** - Event system with IDomainEvent
7. **Repository Interfaces** - Data access contracts
8. **Domain Services** - Cross-cutting concerns
9. **Exception Hierarchy** - Domain exception tree
10. **Complete Integration** - How everything connects
11. **Entity Creation Flow** - Sequence diagram
12. **Repository Flow** - Transaction and event dispatch

#### **✨ Features**

- ✅ **Complete Member Details** - All properties, methods, and visibility
- ✅ **Rich Relationships** - Inheritance, composition, aggregation, dependencies
- ✅ **Theme Compatible** - Works in both light and dark modes
- ✅ **Interactive** - Renders in GitHub, VS Code, and Mermaid viewers
- ✅ **Symbol Legend** - Complete reference for all diagram symbols
- ✅ **Usage Examples** - Sequence diagrams showing real-world flows

#### **🎯 Quick Navigation**

| Concept | Diagram Section | Key Insights |
|---------|----------------|--------------|
| Foundation | Foundational Building Blocks | Base classes, Result pattern |
| Entities | Entity Hierarchy | User, Tenant, Role, Permission |
| Values | Value Object System | Email, Password, TenantId |
| Events | Domain Events | Event-driven architecture |
| Data Access | Repository Interfaces | Persistence abstractions |
| Errors | Exception Hierarchy | Domain exception types |
| **Data Flow** | **Request Lifecycle** | **End-to-end flow diagrams** |

**👉 [View All Class Diagrams](./DDD-ClassDiagrams.md)**  
**👉 [View All Data Flow Diagrams](./DDD-DataFlow.md)** ⭐ New!

---

## Learning Paths

### 🎓 For Beginners (New to DDD)

**Recommended Path:**

1. **[Concrete Example: Create User](./ConcreteExample-CreateUser.md)** (25 min) - **Real code walkthrough** 💻
2. **[DDD Class Diagrams](./DDD-ClassDiagrams.md)** (15 min) - **Visual overview** 🎨
3. **[DDD Data Flow](./DDD-DataFlow.md)** (20 min) - **Request lifecycle** 🔄
4. [Result Pattern](./Result.md) (10 min) - Understand error handling
5. [Error](./Error.md) (5 min) - Structured errors
6. [BaseEntity](./BaseEntity.md) (10 min) - Entity basics
7. [ValueObject](./ValueObject.md) (10 min) - Value vs Entity
8. [AggregateRoot](./AggregateRoot.md) (15 min) - Aggregates
9. [Value Objects Guide](./ValueObjects.md) (30 min) - Deep dive
10. [Domain Entities Guide](./DomainEntities.md) (30 min) - Deep dive
11. [Domain Events](./DomainEvents.md) (30 min) - Event-driven design

**⏱️ Total Time:** ~3.25 hours

---

### 🚀 For Experienced Developers (Know DDD)

**Fast Track:**

1. **[Concrete Example: Create User](./ConcreteExample-CreateUser.md)** (20 min) - **See actual code** 💻
2. **[DDD Class Diagrams](./DDD-ClassDiagrams.md)** (10 min) - **Visual architecture** 🎨
3. **[DDD Data Flow](./DDD-DataFlow.md)** (15 min) - **Request flows** 🔄
4. [Domain Entities](./DomainEntities.md) (20 min) - See the 4 entities
5. [Value Objects](./ValueObjects.md) (20 min) - See the 4 value objects
6. [Domain Events](./DomainEvents.md) (15 min) - Event patterns
7. [Exceptions & Repositories](./DomainExceptionsAndRepositories.md) (15 min) - Infrastructure contracts

**⏱️ Total Time:** ~1.75 hours

---

### 📖 For Quick Reference

**Looking for something specific?**

| Concept | File | Quick Summary |
|---------|------|---------------|
| Identity & Audit | [BaseEntity.md](./BaseEntity.md) | Id, timestamps, soft delete |
| Event Collection | [AggregateRoot.md](./AggregateRoot.md) | Domain events, boundaries |
| Immutability | [ValueObject.md](./ValueObject.md) | Equality by value |
| Error Handling | [Result.md](./Result.md) | Success/Failure pattern |
| Error Types | [Error.md](./Error.md) | Validation, NotFound, etc. |
| User Entity | [DomainEntities.md](./DomainEntities.md) | Authentication, roles |
| Tenant Entity | [DomainEntities.md](./DomainEntities.md) | Multi-tenancy |
| Email VO | [ValueObjects.md](./ValueObjects.md) | Email validation |
| Events | [DomainEvents.md](./DomainEvents.md) | State change notifications |
| Repositories | [DomainExceptionsAndRepositories.md](./DomainExceptionsAndRepositories.md) | Data access contracts |
| **Visual Diagrams** | **[DDD-ClassDiagrams.md](./DDD-ClassDiagrams.md)** | **12 comprehensive class diagrams** |
| **Data Flow** | **[DDD-DataFlow.md](./DDD-DataFlow.md)** | **15 end-to-end flow diagrams** ⭐ |

---

## Documentation Index

### 📘 Foundational Patterns (5 docs)

| Document | Size | Topics | Diagrams |
|----------|------|--------|----------|
| [BaseEntity.md](./BaseEntity.md) | ~5 KB | Identity, Audit, Lifecycle | Yes |
| [AggregateRoot.md](./AggregateRoot.md) | ~7 KB | Boundaries, Events | Yes |
| [ValueObject.md](./ValueObject.md) | ~5 KB | Equality, Immutability | Yes |
| [Result.md](./Result.md) | ~4 KB | Error Handling | Yes |
| [Error.md](./Error.md) | ~3 KB | Error Types | Yes |

### 📗 Comprehensive Guides (4 docs)

| Document | Size | Topics | Diagrams |
|----------|------|--------|----------|
| [DomainEntities.md](./DomainEntities.md) | ~15 KB | 4 Entities | 30+ |
| [ValueObjects.md](./ValueObjects.md) | ~17 KB | 4 Value Objects | 34+ |
| [DomainEvents.md](./DomainEvents.md) | ~17 KB | 6 Events | 35+ |
| [DomainExceptionsAndRepositories.md](./DomainExceptionsAndRepositories.md) | ~21 KB | Exceptions, Repos | 42+ |

### 📊 Visual Reference (2 docs)

| Document | Size | Topics | Diagrams |
|----------|------|--------|----------|
| **[DDD-ClassDiagrams.md](./DDD-ClassDiagrams.md)** | **~23 KB** | **Complete Visual Architecture** | **12** |
| **[DDD-DataFlow.md](./DDD-DataFlow.md)** | **~32 KB** | **Data Flow & Request Lifecycle** | **20** |

### 📝 Concrete Code Examples (1 doc)

| Document | Size | Topics | Code Examples |
|----------|------|--------|---------------|
| **[ConcreteExample-CreateUser.md](./ConcreteExample-CreateUser.md)** | **~22 KB** | **Complete Create User Flow** | **12+ files** |

**ConcreteExample-CreateUser - What's Inside:**
- Complete step-by-step code walkthrough
- How IDomainEvent works in practice
- Actual code from all 9 execution steps
- Memory state at each step
- Visual timeline with millisecond breakdown
- File structure mapping
- Common questions answered

**DDD-ClassDiagrams - What's Inside:**
- Complete domain layer class diagram
- Entity hierarchy with all members
- Value object system visualization
- Domain events architecture
- Repository and service contracts
- Exception hierarchy
- Integration diagrams
- Sequence flows

**DDD-DataFlow - What's Inside:**
- Complete request lifecycle diagrams
- Command flow (write operations)
- Query flow (read operations)
- Domain event flow and side effects
- **Event handler registration & execution** ⭐ New!
- Authentication & authorization flow
- Multi-tenant data isolation
- Error handling patterns
- Performance optimizations

---

## Quick Reference

### 📊 Domain Layer Structure

```
Domain/
├── Common/                    ← Foundational patterns
│   ├── BaseEntity.cs
│   ├── AggregateRoot.cs
│   ├── ValueObject.cs
│   ├── Result.cs
│   └── Error.cs
│
├── Entities/                  ← Business objects (4)
│   ├── User.cs
│   ├── Tenant.cs
│   ├── Role.cs
│   └── Permission.cs
│
├── ValueObjects/              ← Type-safe primitives (4)
│   ├── Email.cs
│   ├── Password.cs
│   ├── TenantId.cs
│   └── SubscriptionTier.cs
│
├── Events/                    ← State changes (6)
│   ├── IDomainEvent.cs
│   ├── UserCreatedEvent.cs
│   ├── UserDeactivatedEvent.cs
│   ├── RoleAssignedEvent.cs
│   ├── PasswordChangedEvent.cs
│   └── TenantProvisionedEvent.cs
│
├── Exceptions/                ← Domain errors (5)
│   ├── DomainException.cs
│   ├── TenantNotFoundException.cs
│   ├── UserNotFoundException.cs
│   ├── UserAlreadyExistsException.cs
│   └── DomainInvalidOperationException.cs
│
├── Repositories/              ← Data access contracts (4)
│   ├── IUserRepository.cs
│   ├── ITenantRepository.cs
│   ├── IRoleRepository.cs
│   └── IUnitOfWork.cs
│
└── Services/                  ← Domain services (2)
    ├── ITenantIsolationService.cs
    └── IPasswordHashingService.cs
```

### 📈 Statistics

| Category | Count | Status |
|----------|-------|--------|
| **Common Classes** | 5 | ✅ Complete |
| **Entities** | 4 | ✅ Complete |
| **Value Objects** | 4 | ✅ Complete |
| **Domain Events** | 6 | ✅ Complete |
| **Exceptions** | 5 | ✅ Complete |
| **Repository Interfaces** | 4 | ✅ Complete |
| **Domain Services** | 2 | ✅ Complete |
| **📁 Total Domain Files** | **30+** | ✅ Complete |
| **📄 Documentation Files** | **12** | ✅ Complete |
| **📊 Total Diagrams** | **180+** | ✅ Complete |
| **📄 Documentation Files** | **11** | ✅ Complete |
| **📊 Total Diagrams** | **170+** | ✅ Complete |

---

## 🚀 Next Steps

After mastering these concepts, you're ready for:

**Phase 3: Application Layer Setup (MediatR)**
- MediatR configuration for CQRS
- FluentValidation infrastructure
- AutoMapper profiles
- Command and Query handlers

---

## 📝 Document Metadata

| Property | Value |
|----------|-------|
| **Phase** | 2 - Domain Layer |
| **Status** | ✅ Completed |
| **Last Updated** | April 02, 2026 |
| **Total Documentation** | 11 comprehensive guides |
| **Visual Elements** | 170+ diagrams |
| **Code Examples** | 150+ snippets |

---

**Made with ❤️ for Clean Architecture and Domain-Driven Design**

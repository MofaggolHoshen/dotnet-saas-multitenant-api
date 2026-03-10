# Domain Layer

The Domain layer is the core of the application, containing all business logic, entities, and domain rules. This layer has **no external dependencies** and remains completely pure.

## Purpose

The Domain layer represents the business domain and encapsulates all business rules. It is framework-agnostic and should not reference any infrastructure or application concerns.

## Structure

```
Domain/
├── Common/           # Base classes and shared abstractions
├── Entities/         # Domain entities (aggregate roots and child entities)
├── ValueObjects/     # Immutable value objects
├── Events/           # Domain events for event-driven architecture
├── Repositories/     # Repository interfaces (implementations in Infrastructure)
├── Services/         # Domain services for complex business logic
└── Exceptions/       # Domain-specific exceptions
```

## Guidelines

### Entities
- Entities have identity and lifecycle
- Use private setters to protect invariants
- Include factory methods for creation
- Encapsulate business logic within the entity

### Value Objects
- Immutable and compared by value
- Self-validating
- No identity (equality based on properties)

### Domain Events
- Represent something significant that happened
- Named in past tense (e.g., `UserCreatedEvent`)
- Immutable

### Repository Interfaces
- Define contracts for data access
- No implementation details
- Aggregate root focused

## Dependencies

**None** - This layer must remain pure with no external package dependencies.

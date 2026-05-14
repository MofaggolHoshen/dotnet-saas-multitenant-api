# 2) Entity Configurations

## Definition

Entity configuration defines how domain types are persisted: table names, columns, constraints, indexes, and value conversions.

## Reason

Fluent API configuration was used to avoid relying only on conventions and to keep mapping rules explicit, versioned, and reviewable.

## What was configured

- `UserConfiguration`
- `TenantConfiguration`
- `RoleConfiguration`
- `PermissionConfiguration`

Key mapping choices:

- explicit table/column mapping
- value object conversions:
  - `TenantId` <-> `Guid`
  - `Email` <-> `string`
  - `SubscriptionTier` <-> `string`
- indexes and uniqueness constraints
- ignored non-persisted members (domain events/in-memory collections)

## Pros

- Predictable schema output from migrations
- Better readability of persistence rules
- Improved performance potential via intentional indexes

## Cons

- More verbose than convention-only mapping
- Requires upkeep when entity models evolve

## Notes

Ignoring non-persisted domain members prevents accidental column generation and keeps persistence model aligned with aggregate design.

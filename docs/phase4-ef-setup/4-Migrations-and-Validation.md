# 4) Migrations and Validation

## Definition

A migration is a versioned description of schema changes generated from EF model metadata.

## Reason

Migrations provide a repeatable and auditable way to evolve the database schema across environments.

## What was completed

- Generated initial migration:
  - name: `InitialCreate`
  - output: `source/Infrastructure/Persistence/Migrations`
- Installed `dotnet-ef` tooling required for migration commands
- Added startup connectivity check:
  - `dbContext.Database.CanConnectAsync()`
- Verified solution compiles after all Phase 4 changes

## Pros

- Controlled schema evolution in source control
- Safer deployment pipeline for DB changes
- Early runtime detection of invalid DB configuration

## Cons

- Migration history can become noisy if not reviewed carefully
- Startup connectivity checks can fail fast in environments where DB is intentionally unavailable

## Notes

Migration review remains important to catch unintended columns, indexes, or constraints before deployment.

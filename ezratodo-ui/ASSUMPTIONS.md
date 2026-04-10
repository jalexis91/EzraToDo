# Design Decisions & Assumptions - EzraToDo

## Overview
This document outlines the architectural decisions, trade-offs, and assumptions made during the development of the EzraToDo task management application.

## 1. Architecture Decisions

### 1.1 Backend: Clean Architecture & CQRS
- **Decision**: Implemented a layered architecture (Api, Application, Domain, Infrastructure) using the CQRS (Command Query Responsibility Segregation) pattern with MediatR.
- **Rationale**: 
    - **Separation of Concerns**: Each layer has a distinct responsibility.
    - **Maintainability**: CQRS makes it easy to locate and modify specific business logic.
    - **Testability**: Logic is decoupled from infrastructure and delivery mechanisms.
- **Trade-off**: Slightly more boilerplate for a simple todo app, but demonstrates production-ready thinking and scalability.

### 1.2 Frontend: React with Custom Hooks and Service Layer
- **Decision**: Built a React (TypeScript) application using a service layer for API calls and custom hooks for state management.
- **Rationale**:
    - **Clean UI Components**: Components focus on presentation; logic is extracted into hooks.
    - **Reusability**: `useTodos` hook can be used across different parts of the application.
    - **Type Safety**: TypeScript ensures consistency between the frontend models and backend DTOs.
- **Styling**: Used inline styles and standard CSS to match the requirements without adding the complexity of external styling libraries for this MVP, though Tailwind CSS is noted as a future enhancement.

## 2. API Enhancements

### 2.1 Filtering, Searching, and Sorting
- **Decision**: Added `IsCompleted`, `SearchTerm`, `SortBy`, and `SortOrder` parameters to the `GetAllTodos` endpoint.
- **Rationale**: Provides a better user experience by allowing users to manage large lists of todos efficiently.
- **Implementation**: Filtering and searching are performed at the database level (via EF Core) for optimal performance.

### 2.2 Patch Endpoints
- **Decision**: Utilized the existing `PATCH` endpoints for completion/reopening instead of a generic `PUT`.
- **Rationale**: More semantic and reduces the payload size for simple state transitions.

## 3. Assumptions

- **Single User Context**: As per the MVP scope, authentication and authorization are omitted. All todos are accessible to anyone with the API URL.
- **Soft Deletion**: Assumed that "deleting" a todo should be reversible or trackable, so implemented soft deletion using an `IsDeleted` flag.
- **Database**: Used SQLite for portability and ease of setup, as recommended in the objectives.

## 4. Future Work & Scalability

### 4.1 Authentication & Multi-Tenancy
- Implement JWT-based authentication.
- Add a `UserId` to the `Todo` entity to support multiple users.

### 4.2 Performance
- **Pagination**: Add `Skip` and `Take` parameters to the API to handle very large lists.
- **Caching**: Implement Redis for frequently accessed lists.

### 4.3 UI/UX
- **Tailwind CSS**: Refactor styling to use Tailwind for better consistency and responsive design.
- **Real-time Updates**: Use SignalR to sync changes across multiple tabs/users.
- **Detailed Validations**: Add more robust client-side validation and rich error feedback.

## 5. Testing Strategy
- **Backend**: Unit tests for Query and Command handlers using xUnit, Moq, and FluentAssertions.
- **Frontend**: Component tests using React Testing Library and Jest, focusing on user interactions and rendering.

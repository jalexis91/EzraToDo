# Copilot Skills for EzraToDo

This directory contains Copilot skills that define standards, best practices, and guidelines for the EzraToDo project. These skills are specifically designed to support **successful completion of the take-home test for Ezra's Full Stack Developer position**.

## 🎯 Quick Start: Starting the Take-Home Test?

**Read this first**: [`take-home-test-objectives.md`](./take-home-test-objectives.md)

This is your primary reference. It explains:
- ✅ Why Ezra uses the take-home as their highest-signal evaluation tool
- ✅ What they're looking for and what they're NOT looking for
- ✅ Two common pitfalls to avoid
- ✅ The 9 evaluation criteria with code examples
- ✅ How to approach the assessment
- ✅ Complete submission checklist

**Then reference these skills as you build**:
1. [`csharp-dotnet10-best-practices.md`](./csharp-dotnet10-best-practices.md) - Backend architecture and patterns
2. [`frontend-architecture.md`](./frontend-architecture.md) - Frontend component design
3. [`database-design.md`](./database-design.md) - Schema design (MVP scope)
4. [`testing-strategy.md`](./testing-strategy.md) - Testing patterns and examples

---

## Available Skills

### [Take-Home Test Objectives & Evaluation Criteria](./take-home-test-objectives.md)

**Primary Focus**: Complete guide for the EzraToDo take-home assessment requirements

**Key Areas**:
- Backend API design with .NET Core (REST endpoints, error handling, validation)
- Data structure and persistence (EF Core, SQLite, repository pattern)
- Frontend component design (React/Vue, state management, user experience)
- Frontend-backend communication (API client services, error handling, async states)
- Clean code and architecture principles (SOLID, layered design, naming)
- Trade-offs and assumptions documentation
- Production MVP features and enhancements
- Scalability and future work considerations
- Testing examples and patterns
- Complete submission checklist and evaluation criteria

**Use This Skill When**:
- Starting the take-home test project
- Designing backend endpoints and data models
- Building frontend components
- Deciding on MVP vs. optional features
- Documenting design decisions and assumptions
- Preparing to submit for evaluation
- Reviewing your work against evaluation criteria

### [Testing Strategy & Best Practices](./testing-strategy.md)

**Primary Focus**: Comprehensive testing patterns and best practices for both backend and frontend

**Key Areas**:
- Testing pyramid (unit, integration, E2E)
- Backend unit testing with xUnit and Moq
- Backend integration testing with WebApplicationFactory
- Frontend component testing with React Testing Library
- Hook testing patterns
- Test data and fixtures
- Mocking best practices
- Coverage targets and measurements
- Common testing mistakes
- CI/CD integration
- Testing checklist

**Use This Skill When**:
- Writing unit tests for services
- Writing integration tests for API endpoints
- Testing React components
- Deciding what to test and how
- Setting up test infrastructure
- Reviewing test coverage
- Implementing CI/CD pipelines

### [Frontend Architecture & Component Design](./frontend-architecture.md)

**Primary Focus**: React/Vue component patterns, state management, and API integration

**Key Areas**:
- Project structure and organization
- API service layer pattern
- State management with hooks
- Component patterns (list, item, form, error alert)
- Environment configuration
- Error handling patterns
- Styling approach (Tailwind CSS)
- Component testing examples
- Performance best practices
- Accessibility standards
- Vue.js alternative patterns

**Use This Skill When**:
- Designing the frontend component structure
- Setting up API communication
- Building reusable components
- Managing form state
- Handling errors from API
- Testing components
- Optimizing performance

### [Database Design & Architecture](./database-design.md)

**Primary Focus**: Schema design, data modeling, and database patterns for scalability

**Key Areas**:
- MVP schema design (simple todos table)
- Multi-user enhancement roadmap
- Advanced features (templates, instances, approvals)
- Indexing strategy and query optimization
- Soft delete and audit trail patterns
- Normalization vs. denormalization trade-offs
- Migration strategy and scalability planning
- Design decisions with rationale and alternatives
- **IMPORTANT**: MVP scope clarification for take-home test

**Use This Skill When**:
- Designing the database schema for new features
- Optimizing database queries
- Planning database migrations
- Deciding on indexing strategy
- Implementing audit trails
- Scaling the database
- Choosing between soft and hard deletes

### [C#/.NET 10 Best Practices & Standards](./csharp-dotnet10-best-practices.md)

**Primary Focus**: Production-ready, maintainable C#/.NET 10 code with clear architecture and security

**Key Areas**:
- Clear, straightforward architecture with layered design
- Production-ready MVP definition and acceptance criteria
- Code quality and modern C# 10 features
- Comprehensive testing and validation strategies
- Security considerations (authentication, authorization, input validation, secrets management)
- Structured logging and diagnostics
- RESTful API design and backend-frontend communication
- Complete setup instructions and documentation
- Development workflow and best practices
- Program.cs and appsettings.json configuration examples

**Use This Skill When**:
- Writing new services or controllers
- Implementing API endpoints
- Setting up the data layer or domain models
- Configuring authentication/authorization
- Designing error handling strategies
- Writing tests
- Setting up logging
- Reviewing pull requests for code quality and standards compliance

## How to Use Skills in Copilot CLI

### Option 1: Reference Directly in Prompts
```
@.github/skills/csharp-dotnet10-best-practices.md Follow these best practices when implementing the todo service
```

### Option 2: Use the /skills Command
```
/skills
```

This command allows you to browse and manage available skills in the interactive CLI.

## Maintaining Skills

- Keep skills up-to-date as project standards evolve
- Add new skills for specialized areas (e.g., frontend, DevOps, database migrations)
- Reference skills in code review comments: "See .github/skills/csharp-dotnet10-best-practices.md for logging standards"
- Update skills when decisions are made about new patterns or tools

## Adding New Skills

Create a new markdown file following the pattern:
- Filename: `{domain}-{focus-area}.md` (e.g., `frontend-react-patterns.md`)
- Include clear sections with actionable guidance
- Provide code examples where relevant
- Link to external resources (official docs, style guides)




## DO NOT MODFIY BELOW THIS LINE
Thoughts on design
Initially will focus on a single user application focused on one user todo task management.
For the future and for production use, authentication and authorization would be required to ensure that users can only access their own tasks. For the MVP, we will assume a single user context and omit authentication for simplicity.
In the future, a graph db approach could be useful if wanting to add on the ability to assign tasks to others or to addd an approval process to tasks.




Original prompt:
Ezra Take-Home Instructions

Ezra thinks you have a great background and is excited to move you forward in their interview process!

As a next step, they’d like you to complete their take-home test (on the last page of this doc) for the Full Stack Developer position. This technical assessment is to help Ezra better understand your coding abilities, architectural thinking, and problem-solving approach. Why Ezra starts their interview process with the take-home: Ezra uses a take-home project as the first formal step in their interview process. This is very intentional. Because resumes and short interviews don’t reliably reflect technical depth, the take-home is their highest-signal evaluation tool. Rather than multiple early-stage interviews, Ezra prefers to evaluate real work first. Ezra is not using the takehome as a “volume screen.” It’s a serious evaluation tool that directly correlates with hiring decisions. If you pass the take-home, you are very likely a strong technical fit and your odds of moving forward successfully through the rest of the interview process are strong. If you’re excited about the role and confident in your ability to build thoughtful, production-minded systems, this is a high-leverage first step. What the take-home involves: The assignment is intentionally simple at a high level: you’ll be asked to build a task management system. The description is concise by design. They want to see how you interpret a lightly scoped prompt and make thoughtful decisions. There is no strict time requirement. You’re encouraged to take the time you need to do your best work. That said, they care much more about clarity of thinking and quality than raw hours spent. What they’re looking for: They’re not looking for something flashy or over-engineered. They’re evaluating: Clear, straightforward architecture Thoughtful decisions about what “production-ready MVP” means Appropriate tests, logging, and security considerations Clean, readable code Sensible tradeoffs Documentation of your thinking Two common pitfalls to avoid: Doing minimal scaffolding without real thought Over-architecting or overcomplicating a simple prompt

Test Details: The test involves building a to-do task management application with both API and frontend components. As you’ll see below, the take-home should be completed in .NET Core. That said, prior .NET experience is not required for this role. Ezra has hired many strong engineers who did not come from a .NET background. What matters most is your engineering judgment, architecture decisions, and problem-solving approach, not prior experience in a specific language. If you are not familiar with .NET you are welcome (and expected) to use modern AI tools to help translate from a language you’re more comfortable in into .NET. The goal of this exercise is not to test memorized framework knowledge, it’s to evaluate how you think about building clean, production-minded systems. The key expectation is that you deeply understand any code you submit as there will be follow-up questions about your implementation, tradeoffs, architecture, and production-readiness decisions. You should be fully comfortable explaining and defending every part of your solution.

What They’re Looking For: Clean, well-structured code Thoughtful architectural decisions Good communication between frontend and backend Production-ready features and considerations Clear documentation and setup instructions

Submission Instructions: Once completed, please share a GitHub repository link containing your completed project and README to the Leopard team via your Slack channel Feel free to tag @channel with your submission so we can make sure to get your take-home over to Ezra ASAP :) Important: please include a comprehensive README with setup steps and your thought process Once we (Team Leopard) receive your submission, we’ll share it with Ezra’s engineering team who will let us know within 5 business days if you have made it through to the next round.

If you have any questions about the requirements or need clarification on anything, please don't hesitate to reach out to the Leopard team. We're here to help ensure you have everything you need to showcase your skills and we can ask Ezra’s engineering team any questions regarding the take-home we are unable to answer ourselves.

From Team Ezra: “Thank you for your interest in joining our team. We look forward to seeing your solution!”

Ezra Full Stack Developer Take-Home Test

Objective

Build a small to-do task management API and frontend. This test evaluates:

● Backend API design. Please use .NET Core. ● Data structure design. Please use SQL Lite or EF Core in memory. ● Frontend component design. Please use React or Vue. ● Communication between frontend and backend. ● Clean code, architecture structure, and thought process. ● Trade-offs and assumptions. ● Include a short README.md with setup steps and your explanation notes. ● Comments or a README.md explaining assumptions, scalability, and what you would implement in the future. ● Submit a GitHub repo link. ● Please add any features you feel are required for a Production MVP.
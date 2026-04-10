# Database Design & Architecture

This skill file covers database design patterns, schema design, and data modeling for the EzraToDo project. It addresses both the MVP phase and provides a roadmap for scalability.

## Design Philosophy

- **Normalization**: Aim for 3NF to reduce redundancy while maintaining query efficiency
- **Indexing Strategy**: Add indexes on frequently queried columns; balance read performance with write performance
- **Soft Deletes**: Use `IsDeleted` flag for historical integrity instead of hard deletes
- **Audit Trail**: Include `CreatedAt`, `UpdatedAt`, `CreatedById`, `UpdatedById` for traceability
- **Relationships**: Foreign keys maintain referential integrity; be explicit about cascading behavior
- **Query Performance**: Consider access patterns early; denormalize strategically if needed

## Phase 1: MVP Database Design

### ⚠️ TAKE-HOME TEST SCOPE

**For the take-home assessment, implement ONLY Phase 1 (MVP schema).**

Phases 2 and 3 demonstrate production-thinking and scalability but are **NOT required** for submission. They show how the design would evolve post-MVP but should not be included in your take-home test submission.

**→ See take-home-test-objectives.md for MVP requirements**

### Overview
The MVP supports a single-user todo application with basic CRUD operations. This keeps the schema simple and enables rapid delivery.

### MVP Schema

#### Table: Todos
```sql
CREATE TABLE Todos (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(2000) NULL,
    IsCompleted BIT NOT NULL DEFAULT 0,
    DueDate DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    -- Indexes for common queries
    INDEX IX_Todos_IsCompleted ON IsCompleted,
    INDEX IX_Todos_DueDate ON DueDate
    -- Note: Avoid indexing IsDeleted (low cardinality boolean)
    -- Use filtered indexes if needed: CREATE INDEX IX_Todos_NotDeleted ON Todos(Id) WHERE IsDeleted = 0
);
```

#### Key Decisions
- **Single-user**: No UserId column; single application user
- **Soft delete**: Use `IsDeleted` flag instead of hard delete (supports recovery, maintains history)
- **Timestamp tracking**: `CreatedAt` and `UpdatedAt` for audit trail
- **Nullable DueDate**: Optional feature; NULL means no deadline
- **Indexes**: On commonly filtered fields (`IsCompleted`, `DueDate`)
  - Note: `IsDeleted` typically not indexed on its own due to low cardinality (only 2 values)
  - Instead, use filtered indexes in practice: `WHERE IsDeleted = 0` if needed

#### Entity Framework Core Mapping
```csharp
public class Todo
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}

// DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Todo>()
        .HasIndex(t => t.IsDeleted);
    
    modelBuilder.Entity<Todo>()
        .HasIndex(t => t.IsCompleted);
    
    modelBuilder.Entity<Todo>()
        .HasIndex(t => t.DueDate);
    
    modelBuilder.Entity<Todo>()
        .Property(t => t.CreatedAt)
        .HasDefaultValueSql("GETUTCDATE()");
    
    modelBuilder.Entity<Todo>()
        .Property(t => t.UpdatedAt)
        .HasDefaultValueSql("GETUTCDATE()");
}
```

### Migration Strategy
Use Entity Framework Core migrations to manage schema changes:

```bash
# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update

# For automated deployment, apply migrations on startup
app.Services.GetRequiredService<TodoDbContext>().Database.Migrate();
```

## Phase 2: Multi-User Enhancement

When expanding to multi-user, introduce user management and ownership:

### Additional Table: Users
```sql
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Email NVARCHAR(256) NOT NULL UNIQUE,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    INDEX IX_Users_Email ON Email,
    INDEX IX_Users_IsActive ON IsActive
);
```

### Updated Table: Todos (Multi-User)
```sql
ALTER TABLE Todos ADD
    UserId INT NOT NULL,
    CONSTRAINT FK_Todos_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE;

CREATE INDEX IX_Todos_UserId ON Todos(UserId);
```

#### Security Consideration
- Users can only see/modify their own todos
- Implement query filter: `WHERE UserId = @currentUserId`
- Use row-level security or filter in queries

## Phase 3: Task Templates & Advanced Features

For a task management system with templates, hierarchical assignments, and approvals:

### Table: Users (Enhanced)
```sql
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Email NVARCHAR(256) NOT NULL UNIQUE,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    ManagerId INT NULL,  -- Self-referential for org hierarchy
    DepartmentId INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Users_Manager FOREIGN KEY (ManagerId) REFERENCES Users(Id),
    INDEX IX_Users_Email ON Email,
    INDEX IX_Users_IsActive ON IsActive,
    INDEX IX_Users_ManagerId ON ManagerId
);
```

### Table: TaskTemplates
```sql
CREATE TABLE TaskTemplates (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(256) NOT NULL,
    Description NVARCHAR(2000) NULL,
    
    -- Assignment Configuration
    AssignmentType INT NOT NULL,  -- 0: Specific Person, 1: Direct Reports, 2: Department
    AssignmentTargetId INT NULL,  -- UserId or DepartmentId depending on AssignmentType
    
    -- Deadline Configuration
    DeadlineType INT NOT NULL,  -- 0: None, 1: Specific Date, 2: Dynamic (relative to event)
    SpecificDeadlineDate DATETIME2 NULL,
    DynamicDeadlineOffsetDays INT NULL,  -- For future: "30 days after hire date"
    
    -- Metadata
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedById INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_TaskTemplates_CreatedBy FOREIGN KEY (CreatedById) REFERENCES Users(Id),
    INDEX IX_TaskTemplates_CreatedById ON CreatedById,
    INDEX IX_TaskTemplates_IsActive ON IsActive,
    INDEX IX_TaskTemplates_IsDeleted ON IsDeleted,
    INDEX IX_TaskTemplates_Name ON Name
);
```

#### Design Decisions
- **AssignmentType**: Enum (0=Specific Person, 1=Direct Reports, 2=Department)
  - Allows flexible assignment patterns
  - In MVP: Use only AssignmentType=0 (Specific Person)
- **DeadlineType**: Enum (0=None, 1=Specific Date, 2=Dynamic)
  - MVP: Use only DeadlineType=0 (None) or DeadlineType=1 (Specific Date)
  - Future: Implement DeadlineType=2 for milestone-based deadlines
- **IsDeleted flag**: Can't hard delete if tasks using template exist
  - Check before hard delete: `SELECT COUNT(*) FROM TaskInstances WHERE TemplateId = @templateId AND IsDeleted = 0`
  - If count > 0, soft delete instead: `UPDATE TaskTemplates SET IsDeleted = 1`
- **Versioning consideration**: Templates could evolve; current design doesn't version

### Table: TaskInstances
```sql
CREATE TABLE TaskInstances (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TemplateId INT NOT NULL,
    
    -- Assignment
    AssignedToId INT NOT NULL,
    CreatedById INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    -- Deadline
    DeadlineDate DATETIME2 NULL,
    
    -- Completion
    CompletedDate DATETIME2 NULL,
    CompletedById INT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',  -- Pending, In Progress, Completed, Overdue
    
    -- Soft Delete
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedDate DATETIME2 NULL,
    DeletedById INT NULL,
    
    -- Audit
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_TaskInstances_Template FOREIGN KEY (TemplateId) REFERENCES TaskTemplates(Id),
    CONSTRAINT FK_TaskInstances_AssignedTo FOREIGN KEY (AssignedToId) REFERENCES Users(Id),
    CONSTRAINT FK_TaskInstances_CreatedBy FOREIGN KEY (CreatedById) REFERENCES Users(Id),
    CONSTRAINT FK_TaskInstances_CompletedBy FOREIGN KEY (CompletedById) REFERENCES Users(Id),
    CONSTRAINT FK_TaskInstances_DeletedBy FOREIGN KEY (DeletedById) REFERENCES Users(Id),
    
    INDEX IX_TaskInstances_TemplateId ON TemplateId,
    INDEX IX_TaskInstances_AssignedToId ON AssignedToId,
    INDEX IX_TaskInstances_Status ON Status,
    INDEX IX_TaskInstances_DeadlineDate ON DeadlineDate,
    INDEX IX_TaskInstances_IsDeleted ON IsDeleted
);
```

#### Design Decisions
- **Status column**: Denormalized from completion logic for query performance
  - Calculated field: Pending, In Progress, Completed, Overdue
  - Store explicitly for easier filtering and reporting
  - Keep in sync via triggers or application logic
- **CompletedById**: Track who completed, not who was assigned
- **Soft delete with metadata**: DeletedDate and DeletedById for audit trail
- **Null handling**: CompletedDate, CompletedById, DeletedDate, DeletedById are NULL until relevant

### Table: TaskApprovals (Future Enhancement)
```sql
CREATE TABLE TaskApprovals (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TaskInstanceId INT NOT NULL,
    ApproverId INT NOT NULL,
    ApprovalStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending',  -- Pending, Approved, Rejected
    ApprovedDate DATETIME2 NULL,
    RejectionReason NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_TaskApprovals_TaskInstance FOREIGN KEY (TaskInstanceId) REFERENCES TaskInstances(Id) ON DELETE CASCADE,
    CONSTRAINT FK_TaskApprovals_Approver FOREIGN KEY (ApproverId) REFERENCES Users(Id),
    
    INDEX IX_TaskApprovals_TaskInstanceId ON TaskInstanceId,
    INDEX IX_TaskApprovals_ApproverId ON ApproverId,
    INDEX IX_TaskApprovals_ApprovalStatus ON ApprovalStatus
);
```

## Indexing Strategy

### When to Index
- **Frequent Filters**: Columns used in WHERE clauses
- **Joins**: Foreign key columns
- **Sorting**: Columns used in ORDER BY (especially for pagination)
- **Aggregations**: Columns in GROUP BY

### When NOT to Index
- **Low cardinality** (boolean, status with few values): May not be selective enough
- **Write-heavy columns**: Index maintenance overhead
- **Large text columns**: Use filtered indexes or full-text search
- **Infrequently queried columns**: Index maintenance cost > query benefit

### Recommended Indexes for Phase 3

| Table | Column(s) | Reason | Type |
|-------|-----------|--------|------|
| TaskTemplates | (IsActive, CreatedById) | List user's active templates | Composite |
| TaskTemplates | Name | Search templates by name | Single |
| TaskInstances | (AssignedToId, Status, DeadlineDate) | User's pending tasks sorted by deadline | Composite |
| TaskInstances | (TemplateId, CreatedAt) | Tasks from specific template | Composite |
| TaskInstances | (CreatedById, IsDeleted) | Audit: tasks created by user | Composite |
| Users | (ManagerId, IsActive) | Org hierarchy queries | Composite |

### Example: Creating Composite Index
```sql
CREATE INDEX IX_TaskInstances_AssignedToStatus_DeadlineDate 
ON TaskInstances (AssignedToId, Status, DeadlineDate)
WHERE IsDeleted = 0;
```

## Query Patterns & Optimization

### MVP: List User's Todos (Filtered & Paginated)
```csharp
// EF Core LINQ query
var todos = _context.Todos
    .Where(t => !t.IsDeleted)
    .Where(t => filterByCompleted == null || t.IsCompleted == filterByCompleted)
    .OrderBy(t => t.DueDate ?? DateTime.MaxValue)
    .ThenBy(t => t.CreatedAt)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .Select(t => new TodoDto { ... })
    .ToListAsync();
```

### Phase 3: Get User's Pending Tasks with Template Info
```csharp
var tasks = _context.TaskInstances
    .Where(t => t.AssignedToId == userId)
    .Where(t => t.Status == "Pending" || t.Status == "In Progress")
    .Where(t => !t.IsDeleted)
    .Include(t => t.Template)
    .OrderBy(t => t.DeadlineDate)
    .ToListAsync();
```

### Phase 3: List All Templates Created by User with Active Count
```csharp
var templates = _context.TaskTemplates
    .Where(t => t.CreatedById == userId)
    .Where(t => t.IsActive)
    .Where(t => !t.IsDeleted)
    .Select(t => new TemplateDto
    {
        Id = t.Id,
        Name = t.Name,
        Description = t.Description,
        ActiveInstanceCount = t.TaskInstances.Count(ti => !ti.IsDeleted && ti.Status != "Completed")
    })
    .ToListAsync();
```

## Soft Delete Strategy

### Principle
Use `IsDeleted` flag instead of hard delete for:
- Maintaining historical integrity
- Supporting recovery/undo operations
- Preserving audit trails
- Preventing referential integrity issues

### Implementation

**Query Filter (Global)**:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Automatically filter deleted records
    modelBuilder.Entity<Todo>()
        .HasQueryFilter(t => !t.IsDeleted);
    
    modelBuilder.Entity<TaskTemplate>()
        .HasQueryFilter(t => !t.IsDeleted);
    
    modelBuilder.Entity<TaskInstance>()
        .HasQueryFilter(t => !t.IsDeleted);
}
```

**Hard Delete Constraints**:
```csharp
// Before hard deleting a TaskTemplate
public async Task DeleteTemplateAsync(int templateId, int deletedById)
{
    var template = await _context.TaskTemplates.FindAsync(templateId);
    
    // Check if any active instances exist
    var activeInstancesExist = await _context.TaskInstances
        .Where(ti => ti.TemplateId == templateId && !ti.IsDeleted)
        .AnyAsync();
    
    if (activeInstancesExist)
    {
        // Soft delete only
        template.IsDeleted = true;
        await _context.SaveChangesAsync();
    }
    else
    {
        // Hard delete safe
        _context.TaskTemplates.Remove(template);
        await _context.SaveChangesAsync();
    }
}
```

## Audit Trail Pattern

Track all changes for compliance and debugging:

```csharp
public class TodoAuditLog
{
    public int Id { get; set; }
    public int TodoId { get; set; }
    public string Operation { get; set; }  // Created, Updated, Deleted
    public string? OldValues { get; set; }  // JSON
    public string? NewValues { get; set; }  // JSON
    public int ChangedById { get; set; }
    public DateTime ChangedAt { get; set; }
}
```

Or use interceptors:
```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    var entries = ChangeTracker.Entries()
        .Where(e => e.Entity is ITenantEntity && e.State == EntityState.Modified)
        .ToList();
    
    foreach (var entry in entries)
    {
        var auditLog = CreateAuditLog(entry);
        AuditLogs.Add(auditLog);
    }
    
    return await base.SaveChangesAsync(cancellationToken);
}
```

## Data Validation Rules

### MVP (Phase 1)

| Field | Validation | Reason |
|-------|-----------|--------|
| Title | Required, 1-200 chars | User-facing; should be concise |
| Description | Optional, max 2000 chars | Allow long-form context |
| DueDate | Optional, must be future or today | No past deadlines |
| IsCompleted | Boolean | Simple state |

### Phase 3 Enhancement

| Field | Validation | Reason |
|-------|-----------|--------|
| TemplateName | Required, 1-256 chars | Identify template |
| AssignmentTargetId | Required if type requires it | Prevent orphaned assignments |
| DeadlineDate | Future if set, null if type=None | Consistency |
| Status | One of: Pending, In Progress, Completed, Overdue | Controlled vocabulary |

## Stored Procedures & Performance

### For Complex Queries (Phase 3+)

**Get User's Pending Tasks with Template Details**:
```sql
CREATE PROCEDURE sp_GetUserPendingTasks
    @UserId INT,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SELECT 
        ti.Id,
        ti.TemplateId,
        tt.Name AS TemplateName,
        ti.DeadlineDate,
        ti.Status,
        ti.CreatedAt
    FROM TaskInstances ti
    INNER JOIN TaskTemplates tt ON ti.TemplateId = tt.Id
    WHERE ti.AssignedToId = @UserId
        AND ti.Status IN ('Pending', 'In Progress')
        AND ti.IsDeleted = 0
    ORDER BY ti.DeadlineDate ASC, ti.CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
```

## Migration Path: MVP → Phase 3

### Step 1: Deploy MVP Schema
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Step 2: Add Users Table (when expanding)
```csharp
// Migration
migrationBuilder.CreateTable(
    name: "Users",
    columns: table => new { ... });
```

### Step 3: Add TaskTemplates & TaskInstances
```csharp
migrationBuilder.CreateTable(name: "TaskTemplates", ...);
migrationBuilder.CreateTable(name: "TaskInstances", ...);
migrationBuilder.AddColumn<int>(name: "UserId", table: "Todos", ...);
```

### Step 4: Backfill Data (if needed)
```sql
-- Add admin user
INSERT INTO Users (Email, FirstName, LastName, IsActive)
VALUES ('admin@company.com', 'Admin', 'User', 1);

-- Assign existing todos to admin user
UPDATE Todos SET UserId = 1 WHERE UserId IS NULL;
```

## Scalability Considerations

### Current Limitations (MVP)
- Single SQLite/in-memory database
- No sharding or partitioning
- Limited to single server

### Future Optimizations (Phase 4+)

**Caching Layer**:
- Cache task templates (rarely change)
- Cache user org hierarchy
- Invalidate on update

**Database Scaling**:
- Archive old completed tasks to separate table/database
- Implement read replicas for reporting
- Partition TaskInstances by date for large datasets

**Indexing for Scale**:
- Filtered indexes: `WHERE IsDeleted = 0`
- Covering indexes for common queries
- Monitor query performance with Execution Plans

## Design Decisions & Trade-offs

| Decision | MVP | Future | Rationale |
|----------|-----|--------|-----------|
| **User support** | Single user (no UserId) | Multi-user (add UserId) | Simplify MVP; add when needed |
| **Templates** | Not included | Phase 3 | Complexity; start simple |
| **Approvals** | Not included | Phase 3 | Can be bolted on later |
| **Status enum** | Implicit (boolean IsCompleted) | Explicit Status column | More flexible for complex workflows |
| **Deletion** | Soft delete with IsDeleted | Maintained | Preserve audit trail |
| **Indexing** | Essential indexes only | Composite indexes | Balance query/write performance |
| **Stored procedures** | EF Core LINQ only | Use for complex queries | Performance; maintainability |

## References & Resources

- [Entity Framework Core Best Practices](https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying)
- [SQL Server Indexing](https://learn.microsoft.com/en-us/sql/relational-databases/indexes/indexes)
- [Soft Delete Pattern](https://martinfowler.com/eaaDev/index.html)
- [Database Normalization](https://en.wikipedia.org/wiki/Database_normalization)

# Temporal constraints

> [!NOTE]
> Temporal constraints are only supported starting with version 11 of the EF provider, and require PostgreSQL 18.

PostgreSQL 18 introduced temporal constraints, which allow you to enforce data integrity rules over time periods. These features are particularly valuable for applications that need to track the validity periods of data, such as employee records, pricing information, equipment assignments, or any scenario where you need to maintain a complete historical timeline without gaps or overlaps.

Temporal constraints work with PostgreSQL's range types, such as `daterange`, `tstzrange` (timestamp with timezone range), and `tsrange` (timestamp range).

## WITHOUT OVERLAPS

The `WITHOUT OVERLAPS` clause can be added to primary and alternate keys to ensure that for any given set of scalar column values, the associated time ranges do not overlap.

A temporal key combines regular columns with a range column. This allows multiple rows for the same entity (e.g., same employee ID) as long as their time periods don't overlap, enabling you to maintain a complete history of changes:

```csharp
public class Employee
{
    public int EmployeeId { get; set; }
    public string Name { get; set; }
    public string Department { get; set; }
    public decimal Salary { get; set; }
    public NpgsqlRange<DateTime> ValidPeriod { get; set; }
}

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Employee>(b =>
    {
        // Configure the range property with a default value
        b.Property(e => e.ValidPeriod)
            .HasDefaultValueSql("tstzrange(now(), 'infinity', '[)')");

        // Configure the temporal primary key
        b.HasKey(e => new { e.EmployeeId, e.ValidPeriod })
            .HasWithoutOverlaps();
    });
}
```

This configuration creates the following table:

```sql
CREATE TABLE employees (
    employee_id INTEGER,
    name VARCHAR(100) NOT NULL,
    department VARCHAR(50) NOT NULL,
    salary DECIMAL(10,2) NOT NULL,
    valid_period tstzrange NOT NULL DEFAULT tstzrange(now(), 'infinity', '[)'),
    PRIMARY KEY (employee_id, valid_period WITHOUT OVERLAPS)
);
```

With this constraint, you can insert multiple records for the same employee as long as their time periods don't overlap:

```sql
-- Valid: Two records for the same employee with non-overlapping periods
INSERT INTO employees (employee_id, name, department, salary, valid_period)
VALUES
    (1, 'Alice Johnson', 'Engineering', 75000, tstzrange('2024-01-01', '2025-01-01', '[)')),
    (1, 'Alice Johnson', 'Engineering', 85000, tstzrange('2025-01-01', 'infinity', '[)'));

-- Invalid: This would fail because it overlaps with existing data
INSERT INTO employees (employee_id, name, department, salary, valid_period)
VALUES (1, 'Alice Johnson', 'Engineering', 95000, tstzrange('2024-06-01', '2025-06-01', '[)'));
```

> [!IMPORTANT]
> The range column with `WITHOUT OVERLAPS` must be the last column in the primary key definition.

## PERIOD for temporal foreign keys

PostgreSQL 18 also introduces temporal foreign keys using the `PERIOD` clause. These constraints ensure that foreign key relationships are maintained across time periods, checking for range containment rather than simple equality.

A temporal foreign key ensures that the referenced row exists during the entire time period of the referencing row. This is particularly useful when you need to enforce that related temporal data is valid for the same time periods.

```csharp
public class Employee
{
    public int EmployeeId { get; set; }
    public string Name { get; set; }
    public NpgsqlRange<DateTime> ValidPeriod { get; set; }
}

public class ProjectAssignment
{
    public int AssignmentId { get; set; }
    public int EmployeeId { get; set; }
    public string ProjectName { get; set; }
    public NpgsqlRange<DateTime> AssignmentPeriod { get; set; }
}

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Employee>(b =>
    {
        b.Property(e => e.ValidPeriod)
            .HasDefaultValueSql("tstzrange(now(), 'infinity', '[)')");

        b.HasKey(e => new { e.EmployeeId, e.ValidPeriod })
            .HasWithoutOverlaps();
    });

    modelBuilder.Entity<ProjectAssignment>(b =>
    {
        b.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(e => new { e.EmployeeId, e.AssignmentPeriod })
            .HasPrincipalKey(e => new { e.EmployeeId, e.ValidPeriod })
            .HasPeriod();
    });
}
```

This generates a foreign key constraint like:

```sql
ALTER TABLE project_assignments
ADD CONSTRAINT fk_emp_temporal
FOREIGN KEY (employee_id, PERIOD assignment_period)
REFERENCES employees (employee_id, PERIOD valid_period);
```

With this constraint:

```sql
-- Valid: Assignment period falls within the employee's validity period
INSERT INTO project_assignments (employee_id, project_name, assignment_period)
VALUES (1, 'Website Redesign', tstzrange('2024-03-01', '2024-06-01', '[)'));

-- Invalid: Assignment period extends beyond the employee's validity period
INSERT INTO project_assignments (employee_id, project_name, assignment_period)
VALUES (1, 'Legacy Project', tstzrange('2022-01-01', '2022-06-01', '[)'));
```

## Querying temporal data

When querying temporal data, PostgreSQL's range operators are particularly useful. The containment operator (`@>`) checks if a range contains a specific point in time:

```csharp
// Find employees who were active on a specific date
var activeEmployees = context.Employees
    .Where(e => e.ValidPeriod.Contains(new DateTime(2024, 6, 15)))
    .ToList();

// Find all historical records for a specific employee
var employeeHistory = context.Employees
    .Where(e => e.EmployeeId == 1)
    .OrderBy(e => e.ValidPeriod)
    .ToList();
```

These queries translate to efficient SQL that can leverage GiST indexes:

```sql
-- Active employees on a specific date
SELECT * FROM employees
WHERE valid_period @> '2024-06-15'::timestamptz;

-- Employee history
SELECT * FROM employees
WHERE employee_id = 1
ORDER BY valid_period;
```

> [!NOTE]
> Temporal constraints require the `btree_gist` extension to be installed in your database. The EF provider automatically installs `btree_gist` when it detects a key with `WITHOUT OVERLAPS`.

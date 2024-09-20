# Collations and Case Sensitivity

> [!NOTE]
> This feature is introduced in EF Core 5.0.
>
> It's recommended that you start by reading [the general Entity Framework Core docs on collations and case sensitivity](https://docs.microsoft.com/ef/core/miscellaneous/collations-and-case-sensitivity).

PostgreSQL is a case-sensitive database by default, but provides various possibilities for performing case-insensitive operations and working with collations. Unfortunately, full collation support is recent and somewhat incomplete, so you may need to carefully review your options below and pick the one which suits you.

## PostgreSQL collations

While PostgreSQL has supported collations for a long time, supported was limited to "deterministic" collations, which did not allow for case-insensitive or accent-insensitive operations. PostgreSQL 12 introduced non-deterministic ICU collations, so it is now possible to use collations in a more flexible way. Read more about PostgreSQL collation support [in the documentation](https://www.postgresql.org/docs/current/collation.html).

> [!NOTE]
> It is not yet possible to use pattern matching operators such as LIKE on columns with a non-deterministic collation.

### Creating a collation

In PostgreSQL, collations are first-class, named database objects which can be created and dropped, just like tables. To create a collation, place the following in your context's `OnModelCreating`:

```csharp
modelBuilder.HasCollation("my_collation", locale: "en-u-ks-primary", provider: "icu", deterministic: false);
```

This creates a collation with the name `my_collation`: this is an arbitrary name you can choose, which you will be specifying later when assigning the collation to columns. The rest of the parameters instruct PostgreSQL to create a non-deterministic, case-insensitive ICU collation. ICU collations are very powerful, and allow you to specify precise rules with regards to case, accents and other textual aspects. Consult [the ICU docs](https://unicode-org.github.io/icu/userguide/collation/) for more information on supported features and keywords.

### Column collation

Once a collation has been created in your database, you can specify it on columns:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasCollation("my_collation", locale: "en-u-ks-primary", provider: "icu", deterministic: false);

    modelBuilder.Entity<Customer>().Property(c => c.Name)
        .UseCollation("my_collation");
}
```

This will cause all textual operators on this column to be case-insensitive.

### Database collation

PostgreSQL also allows you to specify collations at the database level, when it is created:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.UseCollation("<collation_name>");
}
```

Unfortunately, the database collation is quite limited in PostgreSQL; it notably does not support non-deterministic collations (e.g. case-insensitive ones). To work around this limitation, you can use EF Core's [pre-convention model configuration](https://docs.microsoft.com/ef/core/modeling/bulk-configuration#pre-convention-configuration) feature:

```csharp
protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
{
    configurationBuilder.Properties<string>().UseCollation("my_collation");
}
```

All columns created with this configuration will automatically have their collation specified accordingly, and all existing columns will be altered. The end result of the above is very similar to specifying a database collation: instead of telling PostgreSQL to implicit apply a collation to all columns, EF Core will do the same for you in its migrations.

## The citext type

The older PostgreSQL method for performing case-insensitive text operations is the `citext` type; it is similar to the `text` type, but operators are functions between `citext` values are implicitly case-insensitive. [The PostgreSQL docs](https://www.postgresql.org/docs/current/citext.html) provide more information on this type.

`citext` is available in a PostgreSQL-bundled extension, so you'll first have to install it:

```csharp
modelBuilder.HasPostgresExtension("citext");
```

Specifying that a column should use `citext` is simply a matter of setting the column's type:

### [Data Annotations](#tab/data-annotations)

```csharp
public class Blog
{
    public int Id { get; set; }
    [Column(TypeName = "citext")]
    public string Name { get; set; }
}
```

### [Fluent API](#tab/fluent-api)

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Blog>().Property(b => b.Name)
        .HasColumnType("citext");
}
```

***

Some limitations (others are listed in [the PostgreSQL docs](https://www.postgresql.org/docs/current/citext.html)):

* While `citext` allows case-insensitive comparisons, it doesn't handle other aspects of collations, such as accents.
* Several PostgreSQL text functions are overloaded to work with `citext` as expected, but others aren't. Using a function that isn't overloaded will result in a regular, case-sensitive match.
* Unlike collations, `citext` does not allow the same column to be compared case-sensitively in some queries, and and insensitively in others.

## ILIKE

`ILIKE` is a PostgreSQL-specific operator that works just like `LIKE`, but is case-insensitive. If you only need to perform case-insensitive `LIKE` pattern matching, then this could be sufficient. The provider exposes this via `EF.Functions.ILike`:

```csharp
var results = ctx.Blogs
    .Where(b => EF.Functions.ILike(b.Name, "a%b"))
    .ToList();
```

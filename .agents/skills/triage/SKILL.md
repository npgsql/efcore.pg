---
name: triage
description: Use this skill to triage an incoming issue on the EF Core PostgreSQL provider repo (bug report or feature request). Sets the issue type (bug/feature), attempts to arrive at a minimal repro reproducing the bug, checks whether it represents a regression, finds possible duplicates, etc.
---

# EF Issue Triage

This skill covers triaging and reproducing incoming issues on the repository for the PostgreSQL provider of Entity Framework Core. To do so, read the issue in question (provided as input in the prompt), as well as any linked issues/code/resources, apply appropriate classifications and assignments, and for alleged bugs, try to arrive at a minimal repro. User-submitted bug reports frequently provide only fragmentary information and code snippets, forcing you to try to fill in the missing information in the effort to create a minimal repro; valuable information is frequently provided in free-form text, which you need to integrate into the repro as code.

## High-level steps

1. Read the issue in question and any linked issues/code/resources.
2. Assess whether the issue involves any sort of security concern. If it does, either because the reporting user claims so, or because you suspect there might be a security aspect that the reporting user hasn't mentioned, **exit immediately**. **Do not** continue processing or post anything on issues which may involve any sort of security aspect.
3. Produce a minimal repro
    1. If the issue was determined to be a feature request, skip the minimal repro in this step; continue with the remaining triage steps (duplicate search and final report).
    2. If, on the other hand, the issue was determined to be a bug report, attempt to produce a minimal repro as a console program which confirms that the bug is genuine. See "Creating a minimal repro" below for instructions.
    3. If you've managed to confirm a bug in your repro, test your repro on both the failing version and the previous working version. Provide clear feedback confirming or refuting the fact that the reported issue is a regression.
4. Try to find possible duplicate issues - opened or closed - in the EF Core repo (https://github.com/dotnet/efcore), and include the likely candidates in your final report.
5. Determine whether the issue is a feature, bug or task, and set the GitHub issue type accordingly.
    * An issue is only a bug if you could confirm an actual, unresolved bug in a released version of the provider. The fact that the user claims a bug does not mean it is one.
    * If you traced the problem to an upstream bug in EF, do not set the EFCore.PG type to a bug; issues should be classsified as bugs only if they represesnt a bug in our own product.
    * A bug that's only present in older - but supported - versions are still bugs, even if they no longer exist in the latest version. However, we will not fix bugs which exist only in unsupported versions (EFCore.PG's support policy aligns exactly with EF's in terms of which versions are supported).
    * If the user seems to be asking for a new feature, but it's already possible to perform what they want via some other means, then do not set the type to a feature request; such issues are candidates for closing instead. But if the issue is asking for - or proposing - a simpler, more 1st-class way to do something that's already possible, that's indeed a valid feature request.
    * If one of the above cases doesn't match, leave the issue type unset rather than setting bug/feature/task needlessly. For example, if the issue seems to be in user code, that is not a bug - leave the issue type unset for this case.
5. Post your final report as a comment on the issue being triaged.

## Creating a minimal repro

The minimal repro should be created as a completely separate console program, outside of the EF repo. Use the following as your starting point:

```csharp
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

await using var context = new TestContext();
await context.Database.EnsureDeletedAsync();
await context.Database.EnsureCreatedAsync();

public class TestContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .Npgsql(Environment.GetEnvironmentVariable("Test__Npgsql__DefaultConnection"))
            // Use the following to test the repro against SQL Server, to understand whether it's PG-specific or not
//            .UseSqlServer(Environment.GetEnvironmentVariable("Test__SqlServer__DefaultConnection"))
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}

public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

* Try to integrate the user's code into the minimal template, incorporating any textual instructions from the issue, or clues that you can glean.
* At the end of the process, the minimal repro should compile and execute, and reproduce the user's reported error.
* The program should use code that's as close as possible to the user-reported code, including type/property naming and things that seem irrelevant.

### Database providers used in the bug report and repro

This repro is for PostgreSQL (Npgsql) issues only, but we frequently get issues that are more general, describing issues in EF which are also reproducible with e.g. SQL Server. Once you have a repro with PostgreSQL, check whether that repro is specific to PostgreSQL by switching the database provider to SQL Server (or, as a fallback, to SQLite). Pay attention to the EF provider version being used, as the bug may be specific to the version reported by the user. Once you have a working repro, try other, newer versions to confirm where the bug still occurs, and whether it has already been fixed. 

If the issue is indeed PG-specific, say so clearly on the triage analysis and post the repro with PG. If it isn't, post the SQL Server repro instead, and clearly recommend closing the issue and reopening it on the EF repo.

### Make the repro as minimal as possible

Once you've managed to reproduce the bug, work to make the repro as minimal as possible, removing any code that isn't absolutely necessary to triggering the bug:

* If the repro includes a LINQ query, try to remove any irrelevant LINQ operators from that query, as long as the error continues to reproduce.
* If the repro makes use of AutoMapper, attempt to remove it, reproducing the raw LINQ query which Automapper produces.
* If the repro is a query translation issue and does not actually require seed data to reproduce, remove any seeding as well, keeping only the query.
* Do not include any non-necessary Console.WriteLine, banners, comments, summaries or other long-form text inside the code to explain what's going on. Add minimal one-line comments at most, and only where they're really necessary to follow a complicated flow or document results of calls; otherwise no comments are necessary.
* Do not encapsulate code in functions unless really necessary - prefer a simple, minimal function-less program with only top-level statements.
* Do not catch exceptions in order to convert them to a friendlier message; just allow them to bubble up and terminate the program.
* However, leave the LogTo code that ensures that SQL gets logged to the console for diagnostics.
* Do DbContext configuration within the OnConfiguring method of the DbContext type, rather than building the options externally and passing them to the constructor. Avoid any sort of DI unless it's necessary to reproducing the bug.
* In general, the less lines of code, the better.

## Posting your findings

* Post your findings on the triaged issue as a comment.
* The comment should begin with a first-level heading with the text "AI Triage", followed by the sentence "The below is an AI-generated analysis and may contain inaccuracies."
* If there's a minimal repro, its source code should be contained within the posted comment, wrapped inside a collapsible HTML `<details>` block, to not take up too much space (the summary should be "minimal repro").
* In your response, make sure that all links to issues, pull requests or source files are to the repo on github.com, and not local (e.g. `file://` or `vscode://`) links, as your answer will be posted online.

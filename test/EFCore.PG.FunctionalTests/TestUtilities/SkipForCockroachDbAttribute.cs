// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class SkipForCockroachDbAttribute : Attribute, ITestCondition
{
    private readonly string _reason;

    public SkipForCockroachDbAttribute(string reason = null) => _reason = reason;
    public ValueTask<bool> IsMetAsync() => new(!TestEnvironment.IsCockroachDB);

    public string SkipReason => string.IsNullOrWhiteSpace(_reason) ? "Skip for CockroachDB" : $"Skip for CockroachDB: {_reason}";
}

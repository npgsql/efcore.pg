// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class SkipForCockroachDbAttribute : Attribute, ITestCondition
{
    public ValueTask<bool> IsMetAsync() => new(!TestEnvironment.IsCockroachDB);

    public string SkipReason => $"Skip for CockroachDB";
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal
{
    public class NpgsqlOptionsExtension : RelationalOptionsExtension
    {
        public NpgsqlOptionsExtension()
        {
        }

        public NpgsqlOptionsExtension([NotNull] NpgsqlOptionsExtension copyFrom)
            : base(copyFrom)
        {
        }

        public override void ApplyServices(IServiceCollection services)
            => Check.NotNull(services, nameof(services)).AddEntityFrameworkNpgsql();
    }
}

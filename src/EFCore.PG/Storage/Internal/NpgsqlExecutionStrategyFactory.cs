using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class NpgsqlExecutionStrategyFactory : RelationalExecutionStrategyFactory
    {
        public NpgsqlExecutionStrategyFactory(
            [NotNull] ExecutionStrategyDependencies dependencies)
            : base(dependencies)
        {
        }

        protected override IExecutionStrategy CreateDefaultStrategy(ExecutionStrategyDependencies dependencies)
            => new NpgsqlExecutionStrategy(dependencies);
    }
}

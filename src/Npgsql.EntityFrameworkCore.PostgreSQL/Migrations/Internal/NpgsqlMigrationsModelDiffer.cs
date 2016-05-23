#region License
// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal
{
    public class NpgsqlMigrationsModelDiffer : MigrationsModelDiffer
    {
        public NpgsqlMigrationsModelDiffer([NotNull] IRelationalTypeMapper typeMapper, [NotNull] IRelationalAnnotationProvider annotations, [NotNull] IMigrationsAnnotationProvider migrationsAnnotations)
            : base(typeMapper, annotations, migrationsAnnotations)
        {}

        protected override IReadOnlyList<MigrationOperation> Sort(
            [NotNull] IEnumerable<MigrationOperation> operations,
            [NotNull] DiffContext diffContext)
        {
            var ops = base.Sort(operations, diffContext);

            // base.Sort will leave operations it doesn't recognize (Npgsql-specific)
            // at the end, go get these
            if (ops.Count > 0 && !ops[ops.Count - 1].IsNpgsqlSpecific())
                return ops;

            var newOps = new List<MigrationOperation>();
            newOps.AddRange(ops.SkipWhile(o => !o.IsNpgsqlSpecific()));
            newOps.AddRange(ops.TakeWhile(o => !o.IsNpgsqlSpecific()));
            return newOps;
        }

        protected override IEnumerable<MigrationOperation> Diff(
            [CanBeNull] IModel source,
            [CanBeNull] IModel target,
            [NotNull] DiffContext diffContext)
            => (
                source != null && target != null
                    ? Diff(source.Npgsql().PostgresExtensions, target.Npgsql().PostgresExtensions)
                    : source == null
                        ? target.Npgsql().PostgresExtensions.SelectMany(Add)
                        : source.Npgsql().PostgresExtensions.SelectMany(Remove)
               ).Concat(base.Diff(source, target, diffContext));

        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IEnumerable<IPostgresExtension> source,
            [NotNull] IEnumerable<IPostgresExtension> target)
            => DiffCollection(
                source, target,
                Diff, Add, Remove,
                (s, t) => s.Name == t.Name);

        protected virtual IEnumerable<MigrationOperation> Diff([NotNull] IPostgresExtension source, [NotNull] IPostgresExtension target)
            => Enumerable.Empty<MigrationOperation>();

        protected virtual IEnumerable<MigrationOperation> Add([NotNull] IPostgresExtension target)
        {
            yield return new NpgsqlCreatePostgresExtensionOperation
            {
                Schema = target.Schema,
                Name = target.Name,
                Version = target.Version
            };
        }

        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] IPostgresExtension source)
        {
            yield return new NpgsqlDropPostgresExtensionOperation { Name = source.Name };
        }
    }

    static class MigrationOperationExtensions
    {
        internal static bool IsNpgsqlSpecific(this MigrationOperation op)
            => op is NpgsqlCreatePostgresExtensionOperation ||
               op is NpgsqlDropPostgresExtensionOperation;
    }
}

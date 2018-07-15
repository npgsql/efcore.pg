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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal
{
    /// <remarks>
    /// The usual ModificationCommandBatch implementation is <see cref="AffectedCountModificationCommandBatch"/>,
    /// which selects the number of rows modified via a SQL query.
    ///
    /// PostgreSQL actually has no way of selecting the modified row count.
    /// SQL defines GET DIAGNOSTICS which should provide this, but in PostgreSQL it's only available
    /// in PL/pgSQL. See http://www.postgresql.org/docs/9.4/static/unsupported-features-sql-standard.html,
    /// identifier F121-01.
    ///
    /// Instead, the affected row count can be accessed in the PostgreSQL protocol itself, which seems
    /// cleaner and more efficient anyway (no additional query).
    /// </remarks>
    public class NpgsqlModificationCommandBatch : ReaderModificationCommandBatch
    {
        const int DefaultBatchSize = 1000;
        readonly int _maxBatchSize;
        long _parameterCount;

        public NpgsqlModificationCommandBatch(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IUpdateSqlGenerator updateSqlGenerator,
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory,
            [CanBeNull] int? maxBatchSize)
            : base(commandBuilderFactory, sqlGenerationHelper, updateSqlGenerator, valueBufferFactoryFactory)
        {
            if (maxBatchSize.HasValue && maxBatchSize.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxBatchSize), RelationalStrings.InvalidMaxBatchSize);

            _maxBatchSize = maxBatchSize ?? DefaultBatchSize;
        }

        protected override int GetParameterCount() => (int)_parameterCount;

        protected override bool CanAddCommand(ModificationCommand modificationCommand)
        {
            if (ModificationCommands.Count >= _maxBatchSize)
                return false;

            var newParamCount = _parameterCount + modificationCommand.ColumnModifications.Count;

            if (newParamCount > int.MaxValue)
                return false;

            _parameterCount = newParamCount;
            return true;
        }

        protected override bool IsCommandTextValid()
            => true;

        protected override void Consume(RelationalDataReader reader)
        {
            var npgsqlReader = (NpgsqlDataReader)reader.DbDataReader;
            Debug.Assert(npgsqlReader.Statements.Count == ModificationCommands.Count, $"Reader has {npgsqlReader.Statements.Count} statements, expected {ModificationCommands.Count}");
            var commandIndex = 0;

            try
            {
                while (true)
                {
                    // Find the next propagating command, if any
                    int nextPropagating;
                    for (nextPropagating = commandIndex;
                        nextPropagating < ModificationCommands.Count &&
                        !ModificationCommands[nextPropagating].RequiresResultPropagation;
                        nextPropagating++) ;

                    // Go over all non-propagating commands before the next propagating one,
                    // make sure they executed
                    for (; commandIndex < nextPropagating; commandIndex++)
                    {
                        if (npgsqlReader.Statements[commandIndex].Rows == 0)
                        {
                            throw new DbUpdateConcurrencyException(
                                RelationalStrings.UpdateConcurrencyException(1, 0),
                                ModificationCommands[commandIndex].Entries
                            );
                        }
                    }

                    if (nextPropagating == ModificationCommands.Count)
                    {
                        Debug.Assert(!npgsqlReader.NextResult(), "Expected less resultsets");
                        break;
                    }

                    // Propagate to results from the reader to the ModificationCommand

                    var modificationCommand = ModificationCommands[commandIndex++];

                    if (!reader.Read())
                    {
                        throw new DbUpdateConcurrencyException(
                            RelationalStrings.UpdateConcurrencyException(1, 0),
                            modificationCommand.Entries);
                    }

                    var valueBufferFactory = CreateValueBufferFactory(modificationCommand.ColumnModifications);
                    modificationCommand.PropagateResults(valueBufferFactory.Create(npgsqlReader));

                    npgsqlReader.NextResult();
                }
            }
            catch (DbUpdateException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DbUpdateException(
                    RelationalStrings.UpdateStoreException,
                    ex,
                    ModificationCommands[commandIndex].Entries);
            }
        }

        protected override async Task ConsumeAsync(
            RelationalDataReader reader,
            CancellationToken cancellationToken = default)
        {
            var npgsqlReader = (NpgsqlDataReader)reader.DbDataReader;
            Debug.Assert(npgsqlReader.Statements.Count == ModificationCommands.Count, $"Reader has {npgsqlReader.Statements.Count} statements, expected {ModificationCommands.Count}");
            var commandIndex = 0;

            try
            {
                while (true)
                {
                    // Find the next propagating command, if any
                    int nextPropagating;
                    for (nextPropagating = commandIndex;
                        nextPropagating < ModificationCommands.Count &&
                        !ModificationCommands[nextPropagating].RequiresResultPropagation;
                        nextPropagating++)
                        ;

                    // Go over all non-propagating commands before the next propagating one,
                    // make sure they executed
                    for (; commandIndex < nextPropagating; commandIndex++)
                    {
                        if (npgsqlReader.Statements[commandIndex].Rows == 0)
                        {
                            throw new DbUpdateConcurrencyException(
                                RelationalStrings.UpdateConcurrencyException(1, 0),
                                ModificationCommands[commandIndex].Entries
                            );
                        }
                    }

                    if (nextPropagating == ModificationCommands.Count)
                    {
                        Debug.Assert(!(await npgsqlReader.NextResultAsync(cancellationToken)), "Expected less resultsets");
                        break;
                    }

                    // Extract result from the command and propagate it

                    var modificationCommand = ModificationCommands[commandIndex++];

                    if (!(await reader.ReadAsync(cancellationToken)))
                    {
                        throw new DbUpdateConcurrencyException(
                            RelationalStrings.UpdateConcurrencyException(1, 0),
                            modificationCommand.Entries
                        );
                    }

                    var valueBufferFactory = CreateValueBufferFactory(modificationCommand.ColumnModifications);
                    modificationCommand.PropagateResults(valueBufferFactory.Create(npgsqlReader));

                    await npgsqlReader.NextResultAsync(cancellationToken);
                }
            }
            catch (DbUpdateException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DbUpdateException(
                    RelationalStrings.UpdateStoreException,
                    ex,
                    ModificationCommands[commandIndex].Entries);
            }
        }
    }
}

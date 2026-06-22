using System.Data;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlModificationCommand : ModificationCommand
{
    private readonly bool _detailedErrorsEnabled;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlModificationCommand(in ModificationCommandParameters modificationCommandParameters)
        : base(in modificationCommandParameters)
    {
        _detailedErrorsEnabled = modificationCommandParameters.DetailedErrorsEnabled;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlModificationCommand(in NonTrackedModificationCommandParameters modificationCommandParameters)
        : base(in modificationCommandParameters)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void ProcessSinglePropertyJsonUpdate(ref ColumnModificationParameters parameters)
    {
        // PG jsonb_set accepts a jsonb parameter for the value to be set - not an int, boolean or string like many other providers.
        // So we always pass the value through the mapping's ToJsonString() (except for null).
        var mapping = parameters.Property!.GetRelationalTypeMapping();
        var value = parameters.Value;

        value = value is null
            ? "null"
            : (mapping.JsonValueReaderWriter?.ToJsonString(value)
                ?? (mapping.Converter == null ? value : mapping.Converter.ConvertToProvider(value)));

        parameters = parameters with { Value = value };
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void PropagateResults(RelationalDataReader relationalReader)
    {
        // The default implementation of PropagateResults skips (output) parameters, since for e.g. SQL Server these aren't yet populated
        // when consuming the result set (propagating output columns is done later, after the reader is closed).
        // However, in PostgreSQL, output parameters actually get returned as the result set, so we override and take care of that here.
        var columnCount = ColumnModifications.Count;

        var readerIndex = -1;

        for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
        {
            var columnModification = ColumnModifications[columnIndex];

            switch (columnModification.Column)
            {
                case IColumn when columnModification.IsRead:
                case IStoreStoredProcedureParameter { Direction: ParameterDirection.Output or ParameterDirection.InputOutput }:
                    readerIndex++;
                    break;

                case IColumn:
                case IStoreStoredProcedureParameter:
                case null when columnModification.JsonPath is not null:
                    continue;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            // For regular result sets, results are always propagated back into entity properties.
            // But with stored procedures, there may be a rows affected result column (generated by an output parameter definition).
            // Skip these.
            if (columnModification.Property is null || !columnModification.IsRead)
            {
                continue;
            }

            columnModification.Value =
                columnModification.Property.GetReaderFieldValue(relationalReader, readerIndex, _detailedErrorsEnabled);
        }
    }
}

using System.Text.Json;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;

/// <summary>
///     This convention is a hack around https://github.com/dotnet/efcore/issues/32192. To support the EF owned entity JSON support, EF
///     requires
///     that a lookup of the CLR type <see cref="JsonElement" /> return the provider's special <see cref="JsonTypeMapping" />. But Npgsql has
///     its own JSON DOM support, where actually mapping <see cref="JsonElement" /> is allowed as a weakly-typed mapping strategy. The two
///     JSON type mappings are incompatible notably because EF's <see cref="JsonTypeMapping" /> is expected to return UTF8 byte data which is
///     then parsed via <see cref="Utf8JsonWriter" /> (and not a string). So for properties actually typed as <see cref="JsonElement" />, we
///     hack here and set the type mapping rather than going through the regular type mapping process, updating it when the column type
///     changes.
/// </summary>
public class NpgsqlJsonElementHackConvention : IPropertyAddedConvention, IPropertyAnnotationChangedConvention
{
    private NpgsqlJsonTypeMapping? _jsonbTypeMapping;
    private NpgsqlJsonTypeMapping? _jsonTypeMapping;

    /// <inheritdoc />
    public void ProcessPropertyAdded(IConventionPropertyBuilder propertyBuilder, IConventionContext<IConventionPropertyBuilder> context)
        => SetTypeMapping(propertyBuilder.Metadata);

    /// <inheritdoc />
    public void ProcessPropertyAnnotationChanged(
        IConventionPropertyBuilder propertyBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        if (name == RelationalAnnotationNames.ColumnType)
        {
            SetTypeMapping(propertyBuilder.Metadata);
        }
    }

    private void SetTypeMapping(IConventionProperty property)
    {
        if (property.ClrType.UnwrapNullableType() == typeof(JsonElement))
        {
            var columnType = (string?)property[RelationalAnnotationNames.ColumnType];

            if (columnType is null || columnType.Equals("jsonb", StringComparison.OrdinalIgnoreCase))
            {
                property.SetTypeMapping(_jsonbTypeMapping ??= new NpgsqlJsonTypeMapping("jsonb", typeof(JsonElement)));
            }
            else if (columnType.Equals("json", StringComparison.OrdinalIgnoreCase))
            {
                property.SetTypeMapping(_jsonTypeMapping ??= new NpgsqlJsonTypeMapping("json", typeof(JsonElement)));
            }
        }
    }
}

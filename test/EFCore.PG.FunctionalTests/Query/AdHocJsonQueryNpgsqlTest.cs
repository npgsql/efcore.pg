using System.Text.Json;

#pragma warning disable EF8001 // ToJson on owned entities is obsolete

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class AdHocJsonQueryNpgsqlTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string NonSharedStoreName
        => "AdHocJsonQueryTests";

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_predicate_on_bytea(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<TypesDbContext>(
            seed: async context =>
            {
                context.Entities.AddRange(
                    new TypesContainerEntity { JsonEntity = new TypesJsonEntity { Bytea = [1, 2, 3] } },
                    new TypesContainerEntity { JsonEntity = new TypesJsonEntity { Bytea = [1, 2, 4] } });
                await context.SaveChangesAsync();
            });

        using (var context = contextFactory.CreateDbContext())
        {
            var query = context.Entities.Where(x => x.JsonEntity.Bytea == new byte[] { 1, 2, 4 });

            var result = async
                ? await query.SingleAsync()
                : query.Single();

            Assert.Equal(2, result.Id);

            AssertSql(
                """
SELECT e."Id", e."JsonEntity"
FROM "Entities" AS e
WHERE (decode(e."JsonEntity" ->> 'Bytea', 'base64')) = BYTEA E'\\x010204'
LIMIT 2
""");
        }
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_predicate_on_interval(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<TypesDbContext>(
            seed: async context =>
            {
                context.Entities.AddRange(
                    new TypesContainerEntity { JsonEntity = new TypesJsonEntity { Interval = new TimeSpan(1, 2, 3, 4, 123, 456) } },
                    new TypesContainerEntity { JsonEntity = new TypesJsonEntity { Interval = new TimeSpan(2, 2, 3, 4, 123, 456) } });
                await context.SaveChangesAsync();
            });

        using (var context = contextFactory.CreateDbContext())
        {
            var query = context.Entities.Where(x => x.JsonEntity.Interval == new TimeSpan(2, 2, 3, 4, 123, 456));

            var result = async
                ? await query.SingleAsync()
                : query.Single();

            Assert.Equal(2, result.Id);

            AssertSql(
                """
SELECT e."Id", e."JsonEntity"
FROM "Entities" AS e
WHERE (CAST(e."JsonEntity" ->> 'Interval' AS interval)) = INTERVAL '2 02:03:04.123456'
LIMIT 2
""");
        }
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    public async Task Converted_json_composition(bool jsonb, bool coalesce, bool domProvider)
    {
        var contextFactory = await InitializeNonSharedTest<ConvertedJsonContext>(
            onModelCreating: modelBuilder =>
            {
                var property = modelBuilder.Entity<ConvertedJsonEntity>().Property(e => e.Payload)
                    .HasColumnType(jsonb ? "jsonb" : "json");
                if (domProvider)
                {
                    property.HasConversion(
                        value => JsonSerializer.Deserialize<JsonElement>(value.Json, (JsonSerializerOptions)null),
                        value => new JsonWrapper(value.GetRawText()));
                }
                else
                {
                    property.HasConversion(value => value.Json, value => new JsonWrapper(value));
                }
            },
            seed: async context =>
            {
                context.Entities.AddRange(
                    new ConvertedJsonEntity { Id = 1, Payload = new JsonWrapper("""{"Age":25}""") },
                    new ConvertedJsonEntity { Id = 2 },
                    new ConvertedJsonEntity { Id = 3, Payload = new JsonWrapper("null") });
                await context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var fallback = new JsonWrapper("""{"Age":99}""");
        var entities = context.Entities.OrderBy(e => e.Id);
        var query = coalesce
            ? entities.Select(e => e.Payload ?? EF.Parameter(fallback))
            : entities.Select(e => e.Id == 2 ? EF.Parameter(fallback) : e.Payload);

        var values = await query.ToArrayAsync();
        using var first = JsonDocument.Parse(values[0].Json);
        using var second = JsonDocument.Parse(values[1].Json);
        using var third = JsonDocument.Parse(values[2].Json);
        Assert.Equal(25, first.RootElement.GetProperty("Age").GetInt32());
        Assert.Equal(99, second.RootElement.GetProperty("Age").GetInt32());
        Assert.Equal(JsonValueKind.Null, third.RootElement.ValueKind);

        var nullIds = await context.Entities.Where(e => e.Payload == null).Select(e => e.Id).ToArrayAsync();
        Assert.Equal([2], nullIds);

        var projection = coalesce
            ? """COALESCE(e."Payload", @fallback)"""
            : """
CASE
    WHEN e."Id" = 2 THEN @fallback
    ELSE e."Payload"
END
""";
        AssertSql(
            $$"""
@fallback='{"Age":99}'{{(domProvider ? " (Nullable = true)" : "")}} (DbType = Object)

SELECT {{projection}}
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""",
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE e."Payload" IS NULL
""");
    }

    [Theory]
    [InlineData(false, "CaseParameter")]
    [InlineData(true, "CaseParameter")]
    [InlineData(false, "CaseColumn")]
    [InlineData(true, "CaseColumn")]
    [InlineData(false, "CoalesceColumn")]
    [InlineData(true, "CoalesceColumn")]
    public async Task Scalar_converted_json_composition(bool jsonb, string operation)
    {
        var contextFactory = await InitializeNonSharedTest<ScalarConvertedJsonContext>(
            onModelCreating: modelBuilder => modelBuilder.Entity<ScalarConvertedJsonEntity>().Property(e => e.Payload)
                .HasColumnType(jsonb ? "jsonb" : "json")
                .HasConversion(
                    value => JsonSerializer.Serialize(new[] { value }, (JsonSerializerOptions)null),
                    value => JsonSerializer.Deserialize<int[]>(value, (JsonSerializerOptions)null)[0]),
            seed: async context =>
            {
                context.Entities.AddRange(
                    new ScalarConvertedJsonEntity { Id = 1, Payload = 25 },
                    new ScalarConvertedJsonEntity { Id = 2, Payload = 35 });
                await context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var fallback = 99;
        var entities = context.Entities.OrderBy(e => e.Id);
        var query = operation switch
        {
            "CaseParameter" => entities.Select(e => e.Id == 1 ? e.Payload : EF.Parameter(fallback)),
            "CaseColumn" => entities.Select(e => e.Id == 1 ? e.Payload : e.Id),
            "CoalesceColumn" => from entity in context.Entities
                                join other in context.Entities.Where(e => e.Id == 1) on entity.Id equals other.Id into grouping
                                from other in grouping.DefaultIfEmpty()
                                orderby entity.Id
                                select (int?)other.Payload ?? entity.Id,
            _ => throw new UnreachableException()
        };
        var values = await query.ToArrayAsync();
        Assert.Equal([25, operation == "CaseParameter" ? 99 : 2], values);

        AssertSql(
            operation switch
            {
                "CaseColumn" => """
SELECT e."Id" = 1, e."Payload", e."Id"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""",
                "CoalesceColumn" => """
SELECT e1."Payload", e."Id"
FROM "Entities" AS e
LEFT JOIN (
    SELECT e0."Id", e0."Payload"
    FROM "Entities" AS e0
    WHERE e0."Id" = 1
) AS e1 ON e."Id" = e1."Id"
ORDER BY e."Id" NULLS FIRST
""",
                _ => """
@fallback='[99]' (Nullable = false) (DbType = Object)

SELECT CASE
    WHEN e."Id" = 1 THEN e."Payload"
    ELSE @fallback
END
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
"""
            });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Scalar_converted_json_literal_comparison(bool jsonb)
    {
        var contextFactory = await InitializeNonSharedTest<ScalarConvertedJsonContext>(
            onModelCreating: modelBuilder => modelBuilder.Entity<ScalarConvertedJsonEntity>().Property(e => e.Payload)
                .HasColumnType(jsonb ? "jsonb" : "json")
                .HasConversion(
                    value => JsonSerializer.SerializeToElement(new[] { value }, (JsonSerializerOptions)null),
                    value => value[0].GetInt32()),
            seed: async context =>
            {
                context.Entities.AddRange(
                    new ScalarConvertedJsonEntity { Id = 1, Payload = 25 },
                    new ScalarConvertedJsonEntity { Id = 2, Payload = 35 });
                await context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        Assert.Equal(1, await context.Entities.Where(e => e.Payload == 25).Select(e => e.Id).SingleAsync());

        var column = "e.\"Payload\"";
        var left = $"{column}{(jsonb ? "" : "::jsonb")}";
        var right = $"'[25]'{(jsonb ? "" : "::jsonb")}";
        AssertSql(
            $"""
SELECT e."Id"
FROM "Entities" AS e
WHERE {left} = {right}
LIMIT 2
""");
    }

    [Theory]
    [InlineData(false, "ObjectPredicate")]
    [InlineData(true, "ObjectPredicate")]
    [InlineData(false, "TypedPredicate")]
    [InlineData(true, "TypedPredicate")]
    [InlineData(false, "CastPredicate")]
    [InlineData(true, "CastPredicate")]
    [InlineData(false, "TypedProjection")]
    [InlineData(true, "TypedProjection")]
    [InlineData(false, "CastProjection")]
    [InlineData(true, "CastProjection")]
    [InlineData(false, "ColumnPredicate")]
    [InlineData(true, "ColumnPredicate")]
    [InlineData(false, "ColumnCase")]
    [InlineData(true, "ColumnCase")]
    [InlineData(false, "ColumnCoalesce")]
    [InlineData(true, "ColumnCoalesce")]
    [InlineData(false, "CollectionExpansion")]
    [InlineData(true, "CollectionExpansion")]
    [InlineData(false, "ArrayColumnPredicate")]
    [InlineData(true, "ArrayColumnPredicate")]
    [InlineData(false, "ArrayColumnCase")]
    [InlineData(true, "ArrayColumnCase")]
    [InlineData(false, "ArrayColumnCoalesce")]
    [InlineData(true, "ArrayColumnCoalesce")]
    [InlineData(false, "ArrayParameter")]
    [InlineData(true, "ArrayParameter")]
    [InlineData(false, "TypedHasFlag")]
    [InlineData(true, "TypedHasFlag")]
    [InlineData(false, "CastHasFlag")]
    [InlineData(true, "CastHasFlag")]
    [InlineData(false, "NullableHasValue")]
    [InlineData(true, "NullableHasValue")]
    [InlineData(false, "NullableValue")]
    [InlineData(true, "NullableValue")]
    public async Task Native_enum_json_translation(bool jsonb, string operation)
    {
        var contextFactory = await InitializeNonSharedTest<EnumJsonContext>(
            onModelCreating: modelBuilder =>
            {
                modelBuilder.Entity<EnumJsonEntity>().Property(e => e.Payload).HasColumnType(jsonb ? "jsonb" : "json");
                modelBuilder.Entity<EnumJsonEntity>().Property(e => e.TypedPayload).HasColumnType(jsonb ? "jsonb" : "json");
                modelBuilder.Entity<EnumJsonEntity>().Property(e => e.CollectionPayload).HasColumnType(jsonb ? "jsonb" : "json");
            },
            onConfiguring: options => options.UseNpgsql(
                ((NpgsqlTestStore)NonSharedTestStore).ConnectionString,
                o => o.MapEnum<JsonMood>("json_mood").ConfigureDataSource(b => b.EnableDynamicJson())),
            seed: async context =>
            {
                context.Entities.AddRange(
                    new EnumJsonEntity
                    {
                        Id = 1,
                        Mood = JsonMood.Sad,
                        Payload = new() { ["Mood"] = JsonMood.Sad, ["Moods"] = (JsonMood[])[JsonMood.Sad], ["NullMood"] = null },
                        TypedPayload = new() { ["Mood"] = JsonMood.Sad },
                        CollectionPayload = new() { ["Moods"] = [JsonMood.Sad] }
                    },
                    new EnumJsonEntity
                    {
                        Id = 2,
                        Mood = JsonMood.Happy,
                        Payload = new() { ["Mood"] = JsonMood.Happy, ["Moods"] = (JsonMood[])[JsonMood.Happy], ["NullMood"] = null },
                        TypedPayload = new() { ["Mood"] = JsonMood.Happy },
                        CollectionPayload = new() { ["Moods"] = [JsonMood.Happy] }
                    });
                await context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        if (operation == "CollectionExpansion")
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => context.Entities.SelectMany(e => e.CollectionPayload["Moods"]).ToArrayAsync());
            Assert.Contains("could not be translated", exception.Message);
            AssertSql();
            return;
        }

        if (operation is "ColumnPredicate" or "ColumnCase" or "ColumnCoalesce"
            or "ArrayColumnPredicate" or "ArrayColumnCase" or "ArrayColumnCoalesce")
        {
            var columnQuery = operation switch
            {
                "ColumnPredicate" => context.Entities.Where(e => e.Payload["Mood"] == (object)e.Mood).Select(e => e.Payload["Mood"]),
                "ColumnCase" => context.Entities.Select(e => e.Id == 1 ? e.Payload["Mood"] : (object)e.Mood),
                "ColumnCoalesce" => context.Entities.Select(e => e.Payload["Mood"] ?? (object)e.Mood),
                "ArrayColumnPredicate" => context.Entities.Where(e => e.Payload["Moods"] == (object)new[] { e.Mood })
                    .Select(e => e.Payload["Moods"]),
                "ArrayColumnCase" => context.Entities.Select(e => e.Id == 1 ? e.Payload["Moods"] : (object)new[] { e.Mood }),
                "ArrayColumnCoalesce" => context.Entities.Select(e => e.Payload["Moods"] ?? (object)new[] { e.Mood }),
                _ => throw new UnreachableException()
            };
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => columnQuery.ToArrayAsync());
            Assert.Equal("Converting PostgreSQL enum expressions to JSON is not supported.", exception.Message);
            AssertSql();
            return;
        }

        var column = operation.StartsWith("Typed", StringComparison.Ordinal) ? "TypedPayload" : "Payload";
        var traversal = $"""e."{column}" -> '{(operation == "ArrayParameter" ? "Moods" : "Mood")}'""";
        if (operation == "NullableHasValue")
        {
            var values = await context.Entities.OrderBy(e => e.Id).Select(e => new
            {
                Present = ((JsonMood?)e.Payload["Mood"]).HasValue,
                Null = ((JsonMood?)e.Payload["NullMood"]).HasValue,
                Missing = ((JsonMood?)e.Payload["MissingMood"]).HasValue
            }).ToArrayAsync();
            Assert.All(values, value =>
            {
                Assert.True(value.Present);
                Assert.False(value.Null);
                Assert.False(value.Missing);
            });

            AssertSql(
                """
SELECT e."Payload" ->> 'Mood' IS NOT NULL AS "Present", e."Payload" ->> 'NullMood' IS NOT NULL AS "Null", e."Payload" ->> 'MissingMood' IS NOT NULL AS "Missing"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
            return;
        }

        if (operation is "TypedHasFlag" or "CastHasFlag")
        {
            var projection = operation == "TypedHasFlag"
                ? context.Entities.OrderBy(e => e.Id).Select(e => e.TypedPayload["Mood"].HasFlag(JsonMood.Sad))
                : context.Entities.OrderBy(e => e.Id).Select(e => ((JsonMood)e.Payload["Mood"]).HasFlag(JsonMood.Sad));
            var values = await projection.ToArrayAsync();
            Assert.Equal([true, false], values);

            AssertSql(
                $"""
SELECT {traversal}, 'sad'::json_mood
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
            return;
        }

        if (operation is "TypedProjection" or "CastProjection" or "NullableValue")
        {
            var projection = operation switch
            {
                "TypedProjection" => context.Entities.OrderBy(e => e.Id).Select(e => e.TypedPayload["Mood"]),
                "CastProjection" => context.Entities.OrderBy(e => e.Id).Select(e => (JsonMood)e.Payload["Mood"]),
                "NullableValue" => context.Entities.OrderBy(e => e.Id).Select(e => ((JsonMood?)e.Payload["Mood"]).Value),
                _ => throw new UnreachableException()
            };
            var values = await projection.ToArrayAsync();
            Assert.Equal([JsonMood.Sad, JsonMood.Happy], values);

            AssertSql(
                $"""
SELECT {traversal}
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
            return;
        }

        var mood = JsonMood.Sad;
        JsonMood[] moods = [JsonMood.Sad];
        var query = operation switch
        {
            "ObjectPredicate" => context.Entities.Where(e => e.Payload["Mood"] == (object)EF.Parameter(mood)),
            "TypedPredicate" => context.Entities.Where(e => e.TypedPayload["Mood"] == EF.Parameter(mood)),
            "CastPredicate" => context.Entities.Where(e => (JsonMood)e.Payload["Mood"] == EF.Parameter(mood)),
            "ArrayParameter" => context.Entities.Where(e => e.Payload["Moods"] == (object)EF.Parameter(moods)),
            _ => throw new UnreachableException()
        };
        Assert.Equal(1, await query.Select(e => e.Id).SingleAsync());

        var left = $"NULLIF({(jsonb ? traversal : $"CAST({traversal} AS jsonb)")}, 'null'::jsonb)";
        var parameter = operation == "ArrayParameter" ? "moods" : "mood";
        var right = $"NULLIF(@{parameter}{(jsonb || operation is "ObjectPredicate" or "ArrayParameter" ? "" : "::jsonb")}, 'null'::jsonb)";
        var value = operation switch
        {
            "ObjectPredicate" => "1",
            "ArrayParameter" => "[1]",
            _ => "Sad"
        };
        AssertSql(
            $"""
@{parameter}='{value}'{(operation == "ArrayParameter" ? " (Nullable = true)" : "")} (DbType = Object)

SELECT e."Id"
FROM "Entities" AS e
WHERE {left} = {right} OR ({left} IS NULL AND {right} IS NULL)
LIMIT 2
""");
    }

    protected class EnumJsonContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<EnumJsonEntity> Entities { get; set; }
    }

    public class EnumJsonEntity
    {
        public int Id { get; set; }
        public JsonMood Mood { get; set; }
        public Dictionary<string, object> Payload { get; set; }
        public Dictionary<string, JsonMood> TypedPayload { get; set; }
        public Dictionary<string, JsonMood[]> CollectionPayload { get; set; }
    }

    public enum JsonMood
    {
        Happy,
        Sad
    }

    [Theory]
    [InlineData(false, "Typed")]
    [InlineData(true, "Typed")]
    [InlineData(false, "Cast")]
    [InlineData(true, "Cast")]
    [InlineData(false, "Days")]
    [InlineData(true, "Days")]
    [InlineData(false, "Add")]
    [InlineData(true, "Add")]
    [InlineData(false, "Negate")]
    [InlineData(true, "Negate")]
    [InlineData(false, "TypedToString")]
    [InlineData(true, "TypedToString")]
    [InlineData(false, "CastToString")]
    [InlineData(true, "CastToString")]
    public async Task TimeSpan_json_dictionary_projection(bool jsonb, string operation)
    {
        var first = TimeSpan.FromDays(1).Add(TimeSpan.FromTicks(1234567));
        var second = -TimeSpan.FromDays(2).Add(TimeSpan.FromTicks(1234567));
        var contextFactory = await InitializeNonSharedTest<TimeSpanJsonContext>(
            onModelCreating: modelBuilder =>
            {
                modelBuilder.Entity<TimeSpanJsonEntity>().Property(e => e.Payload).HasColumnType(jsonb ? "jsonb" : "json");
                modelBuilder.Entity<TimeSpanJsonEntity>().Property(e => e.TypedPayload).HasColumnType(jsonb ? "jsonb" : "json");
            },
            onConfiguring: options => options.UseNpgsql(
                ((NpgsqlTestStore)NonSharedTestStore).ConnectionString,
                o => o.ConfigureDataSource(b => b.EnableDynamicJson())),
            seed: async context =>
            {
                context.Entities.AddRange(
                    new TimeSpanJsonEntity { Id = 1, Payload = new() { ["Duration"] = first }, TypedPayload = new() { ["Duration"] = first } },
                    new TimeSpanJsonEntity { Id = 2, Payload = new() { ["Duration"] = second }, TypedPayload = new() { ["Duration"] = second } });
                await context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var entities = context.Entities.OrderBy(e => e.Id);
        if (operation is "TypedToString" or "CastToString")
        {
            var query = operation == "TypedToString"
                ? entities.Select(e => e.TypedPayload["Duration"].ToString())
                : entities.Select(e => ((TimeSpan)e.Payload["Duration"]).ToString());
            var values = await query.ToArrayAsync();
            Assert.Equal([first.ToString(), second.ToString()], values);
        }
        else if (operation == "Days")
        {
            var days = await entities.Select(e => ((TimeSpan)e.Payload["Duration"]).Days).ToArrayAsync();
            Assert.Equal([1, -2], days);
        }
        else
        {
            var query = operation switch
            {
                "Typed" => entities.Select(e => e.TypedPayload["Duration"]),
                "Cast" => entities.Select(e => (TimeSpan)e.Payload["Duration"]),
                "Add" => entities.Select(e => (TimeSpan)e.Payload["Duration"] + TimeSpan.FromHours(1)),
                "Negate" => entities.Select(e => -(TimeSpan)e.Payload["Duration"]),
                _ => throw new UnreachableException()
            };
            var values = await query.ToArrayAsync();
            TimeSpan[] expected = operation switch
            {
                "Add" => [first + TimeSpan.FromHours(1), second + TimeSpan.FromHours(1)],
                "Negate" => [-first, -second],
                _ => [first, second]
            };
            Assert.Equal(expected, values);
        }

        AssertSql(
            $"""
SELECT e."{(operation is "Typed" or "TypedToString" ? "TypedPayload" : "Payload")}" -> 'Duration'
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task TimeSpan_json_dictionary_has_value(bool jsonb, bool negated)
    {
        var contextFactory = await InitializeNonSharedTest<TimeSpanJsonContext>(
            onModelCreating: modelBuilder =>
            {
                modelBuilder.Entity<TimeSpanJsonEntity>().Property(e => e.Payload).HasColumnType(jsonb ? "jsonb" : "json");
                modelBuilder.Entity<TimeSpanJsonEntity>().Property(e => e.TypedPayload).HasColumnType(jsonb ? "jsonb" : "json");
            },
            onConfiguring: options => options.UseNpgsql(
                ((NpgsqlTestStore)NonSharedTestStore).ConnectionString,
                o => o.ConfigureDataSource(b => b.EnableDynamicJson())),
            seed: async context =>
            {
                context.Entities.AddRange(
                    new TimeSpanJsonEntity { Id = 1, Payload = new() { ["Duration"] = TimeSpan.FromDays(1) }, TypedPayload = [] },
                    new TimeSpanJsonEntity { Id = 2, Payload = new() { ["Duration"] = null }, TypedPayload = [] },
                    new TimeSpanJsonEntity { Id = 3, Payload = [], TypedPayload = [] });
                await context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var query = negated
            ? context.Entities.Where(e => !((TimeSpan?)e.Payload["Duration"]).HasValue)
            : context.Entities.Where(e => ((TimeSpan?)e.Payload["Duration"]).HasValue);
        var ids = await query.OrderBy(e => e.Id).Select(e => e.Id).ToArrayAsync();
        Assert.Equal(negated ? [2, 3] : [1], ids);

        AssertSql(
            $"""
SELECT e."Id"
FROM "Entities" AS e
WHERE e."Payload" ->> 'Duration' IS {(negated ? "" : "NOT ")}NULL
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false, "Compare")]
    [InlineData(true, "Compare")]
    [InlineData(false, "CompareTo")]
    [InlineData(true, "CompareTo")]
    [InlineData(false, "ReverseCompare")]
    [InlineData(true, "ReverseCompare")]
    [InlineData(false, "ReverseCompareTo")]
    [InlineData(true, "ReverseCompareTo")]
    public async Task TimeSpan_json_dictionary_comparison(bool jsonb, string operation)
    {
        var contextFactory = await InitializeNonSharedTest<TimeSpanJsonContext>(
            onModelCreating: modelBuilder =>
            {
                modelBuilder.Entity<TimeSpanJsonEntity>().Property(e => e.Payload).HasColumnType(jsonb ? "jsonb" : "json");
                modelBuilder.Entity<TimeSpanJsonEntity>().Property(e => e.TypedPayload).HasColumnType(jsonb ? "jsonb" : "json");
            },
            onConfiguring: options => options.UseNpgsql(
                ((NpgsqlTestStore)NonSharedTestStore).ConnectionString,
                o => o.ConfigureDataSource(b => b.EnableDynamicJson())),
            seed: async context =>
            {
                context.Entities.AddRange(
                    new TimeSpanJsonEntity
                    {
                        Id = 1,
                        Payload = new() { ["Duration"] = TimeSpan.FromDays(2) },
                        TypedPayload = new() { ["Duration"] = TimeSpan.FromDays(2) }
                    },
                    new TimeSpanJsonEntity
                    {
                        Id = 2,
                        Payload = new() { ["Duration"] = TimeSpan.FromDays(10) },
                        TypedPayload = new() { ["Duration"] = TimeSpan.FromDays(10) }
                    });
                await context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var threshold = TimeSpan.FromDays(5);
        var entities = context.Entities.OrderBy(e => e.Id);
        var query = operation switch
        {
            "Compare" => entities.Select(e => TimeSpan.Compare(e.TypedPayload["Duration"], threshold)),
            "CompareTo" => entities.Select(e => e.TypedPayload["Duration"].CompareTo(threshold)),
            "ReverseCompare" => entities.Select(e => TimeSpan.Compare(threshold, (TimeSpan)e.Payload["Duration"])),
            "ReverseCompareTo" => entities.Select(e => threshold.CompareTo((TimeSpan)e.Payload["Duration"])),
            _ => throw new UnreachableException()
        };
        var values = await query.ToArrayAsync();
        var reverse = operation.StartsWith("Reverse", StringComparison.Ordinal);
        Assert.Equal(reverse ? [1, -1] : [-1, 1], values);

        AssertSql(
            $"""
SELECT e."{(reverse ? "Payload" : "TypedPayload")}" -> 'Duration'
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    protected class TimeSpanJsonContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<TimeSpanJsonEntity> Entities { get; set; }
    }

    public class TimeSpanJsonEntity
    {
        public int Id { get; set; }
        public Dictionary<string, object> Payload { get; set; }
        public Dictionary<string, TimeSpan> TypedPayload { get; set; }
    }

    protected class ScalarConvertedJsonContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<ScalarConvertedJsonEntity> Entities { get; set; }
    }

    public class ScalarConvertedJsonEntity
    {
        public int Id { get; set; }
        public int Payload { get; set; }
    }

    protected class ConvertedJsonContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<ConvertedJsonEntity> Entities { get; set; }
    }

    public class ConvertedJsonEntity
    {
        public int Id { get; set; }
        public JsonWrapper Payload { get; set; }
    }

    public record JsonWrapper(string Json);

    protected class TypesDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<TypesContainerEntity> Entities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<TypesContainerEntity>().OwnsOne(b => b.JsonEntity).ToJson();
    }

    public class TypesContainerEntity
    {
        public int Id { get; set; }
        public TypesJsonEntity JsonEntity { get; set; }
    }

    public class TypesJsonEntity
    {
        public byte[] Bytea { get; set; }
        public TimeSpan Interval { get; set; }
    }

    protected void AssertSql(params string[] expected)
        => ((TestSqlLoggerFactory)ListLoggerFactory).AssertBaseline(expected);
}

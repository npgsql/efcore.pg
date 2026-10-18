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
    [InlineData(false, "TypedIntCast")]
    [InlineData(true, "TypedIntCast")]
    [InlineData(false, "BitwiseAnd")]
    [InlineData(true, "BitwiseAnd")]
    [InlineData(false, "BitwiseOr")]
    [InlineData(true, "BitwiseOr")]
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
        if (operation == "TypedIntCast")
        {
            var values = await context.Entities.OrderBy(e => e.Id).Select(e => (int)e.TypedPayload["Mood"]).ToArrayAsync();
            Assert.Equal([1, 0], values);

            AssertSql(
                """
SELECT e."TypedPayload" -> 'Mood'
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
            return;
        }

        if (operation is "BitwiseAnd" or "BitwiseOr")
        {
            var projection = operation == "BitwiseAnd"
                ? context.Entities.OrderBy(e => e.Id).Select(e => e.TypedPayload["Mood"] & JsonMood.Sad)
                : context.Entities.OrderBy(e => e.Id).Select(e => e.TypedPayload["Mood"] | JsonMood.Sad);
            var values = await projection.ToArrayAsync();
            Assert.Equal(operation == "BitwiseAnd" ? [JsonMood.Sad, JsonMood.Happy] : [JsonMood.Sad, JsonMood.Sad], values);

            AssertSql(
                """
SELECT e."TypedPayload" -> 'Mood'
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
            return;
        }

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
    [InlineData(false, "TypedConvert")]
    [InlineData(true, "TypedConvert")]
    [InlineData(false, "CastConvert")]
    [InlineData(true, "CastConvert")]
    [InlineData(false, "TypedDateTrunc")]
    [InlineData(true, "TypedDateTrunc")]
    [InlineData(false, "CastDateTrunc")]
    [InlineData(true, "CastDateTrunc")]
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
        if (operation is "TypedDateTrunc" or "CastDateTrunc")
        {
            var query = operation == "TypedDateTrunc"
                ? entities.Where(e => EF.Functions.DateTrunc("hour", e.TypedPayload["Duration"]) == TimeSpan.Zero)
                : entities.Where(e => EF.Functions.DateTrunc("hour", (TimeSpan)e.Payload["Duration"]) == TimeSpan.Zero);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToArrayAsync());
            Assert.Contains("could not be translated", exception.Message);
            AssertSql();
            return;
        }

        if (operation is "TypedToString" or "CastToString" or "TypedConvert" or "CastConvert")
        {
            var query = operation switch
            {
                "TypedToString" => entities.Select(e => e.TypedPayload["Duration"].ToString()),
                "CastToString" => entities.Select(e => ((TimeSpan)e.Payload["Duration"]).ToString()),
                "TypedConvert" => entities.Select(e => Convert.ToString(e.TypedPayload["Duration"])),
                "CastConvert" => entities.Select(e => Convert.ToString((TimeSpan)e.Payload["Duration"])),
                _ => throw new UnreachableException()
            };
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
SELECT e."{(operation is "Typed" or "TypedToString" or "TypedConvert" ? "TypedPayload" : "Payload")}" -> 'Duration'
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

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task TimeSpan_json_dictionary_time_only(bool jsonb, bool add)
    {
        var first = TimeSpan.FromHours(1).Add(TimeSpan.FromTicks(1234567));
        var second = TimeSpan.FromHours(10).Add(TimeSpan.FromTicks(1234567));
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
                    new TimeSpanJsonEntity { Id = 1, Payload = new() { ["Duration"] = first }, TypedPayload = [] },
                    new TimeSpanJsonEntity { Id = 2, Payload = new() { ["Duration"] = second }, TypedPayload = [] });
                await context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var entities = context.Entities.OrderBy(e => e.Id);
        var query = add
            ? entities.Select(e => TimeOnly.MinValue.Add((TimeSpan)e.Payload["Duration"]))
            : entities.Select(e => TimeOnly.FromTimeSpan((TimeSpan)e.Payload["Duration"]));
        var values = await query.ToArrayAsync();
        Assert.Equal([TimeOnly.FromTimeSpan(first), TimeOnly.FromTimeSpan(second)], values);

        AssertSql(
            """
SELECT e."Payload" -> 'Duration'
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [Theory]
    [InlineData(false, "Timestamp", "Predicate")]
    [InlineData(true, "Timestamp", "Predicate")]
    [InlineData(false, "TimestampOffset", "Predicate")]
    [InlineData(true, "TimestampOffset", "Predicate")]
    [InlineData(false, "Date", "Predicate")]
    [InlineData(true, "Date", "Predicate")]
    [InlineData(false, "LocalTimestamp", "Predicate")]
    [InlineData(true, "LocalTimestamp", "Predicate")]
    [InlineData(false, "Timestamp", "Case")]
    [InlineData(true, "Timestamp", "Case")]
    [InlineData(false, "TimestampOffset", "Case")]
    [InlineData(true, "TimestampOffset", "Case")]
    [InlineData(false, "Date", "Case")]
    [InlineData(true, "Date", "Case")]
    [InlineData(false, "LocalTimestamp", "Case")]
    [InlineData(true, "LocalTimestamp", "Case")]
    [InlineData(false, "Timestamp", "Coalesce")]
    [InlineData(true, "Timestamp", "Coalesce")]
    [InlineData(false, "TimestampOffset", "Coalesce")]
    [InlineData(true, "TimestampOffset", "Coalesce")]
    [InlineData(false, "Date", "Coalesce")]
    [InlineData(true, "Date", "Coalesce")]
    [InlineData(false, "LocalTimestamp", "Coalesce")]
    [InlineData(true, "LocalTimestamp", "Coalesce")]
    public async Task Timestamp_json_dictionary_extrema(bool jsonb, string type, string operation)
    {
        object minimum = type switch
        {
            "TimestampOffset" => DateTimeOffset.MinValue,
            "Date" => DateOnly.MinValue,
            _ => DateTime.MinValue
        };
        object maximum = type switch
        {
            "TimestampOffset" => DateTimeOffset.MaxValue,
            "Date" => DateOnly.MaxValue,
            _ => DateTime.MaxValue
        };
        var contextFactory = await InitializeNonSharedTest<TimestampJsonContext>(
            onModelCreating: modelBuilder => modelBuilder.Entity<TimestampJsonEntity>().Property(e => e.Payload)
                .HasColumnType(jsonb ? "jsonb" : "json"),
            onConfiguring: options => options.UseNpgsql(
                ((NpgsqlTestStore)NonSharedTestStore).ConnectionString,
                o => o.ConfigureDataSource(b => b.EnableDynamicJson())),
            seed: async context =>
            {
                context.Entities.AddRange(
                    new TimestampJsonEntity
                    {
                        Id = 1,
                        Timestamp = DateTime.MinValue,
                        TimestampOffset = DateTimeOffset.MinValue,
                        LocalTimestamp = DateTime.MinValue,
                        Date = DateOnly.MinValue,
                        Payload = new() { ["Timestamp"] = minimum }
                    },
                    new TimestampJsonEntity
                    {
                        Id = 2,
                        Timestamp = DateTime.MaxValue,
                        TimestampOffset = DateTimeOffset.MaxValue,
                        LocalTimestamp = DateTime.MaxValue,
                        Date = DateOnly.MaxValue,
                        Payload = new() { ["Timestamp"] = maximum }
                    });
                await context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var entities = context.Entities.OrderBy(e => e.Id);
        var query = (operation, type) switch
        {
            ("Predicate", "Timestamp") => entities.Where(e => e.Payload["Timestamp"] == (object)e.Timestamp).Select(e => e.Payload["Timestamp"]),
            ("Predicate", "TimestampOffset") => entities.Where(e => e.Payload["Timestamp"] == (object)e.TimestampOffset).Select(e => e.Payload["Timestamp"]),
            ("Predicate", "Date") => entities.Where(e => e.Payload["Timestamp"] == (object)e.Date).Select(e => e.Payload["Timestamp"]),
            ("Predicate", "LocalTimestamp") => entities.Where(e => e.Payload["Timestamp"] == (object)e.LocalTimestamp).Select(e => e.Payload["Timestamp"]),
            ("Case", "Timestamp") => entities.Select(e => e.Id == 1 ? e.Payload["Timestamp"] : (object)e.Timestamp),
            ("Case", "TimestampOffset") => entities.Select(e => e.Id == 1 ? e.Payload["Timestamp"] : (object)e.TimestampOffset),
            ("Case", "Date") => entities.Select(e => e.Id == 1 ? e.Payload["Timestamp"] : (object)e.Date),
            ("Case", "LocalTimestamp") => entities.Select(e => e.Id == 1 ? e.Payload["Timestamp"] : (object)e.LocalTimestamp),
            ("Coalesce", "Timestamp") => entities.Select(e => e.Payload["Missing"] ?? (object)e.Timestamp),
            ("Coalesce", "TimestampOffset") => entities.Select(e => e.Payload["Missing"] ?? (object)e.TimestampOffset),
            ("Coalesce", "Date") => entities.Select(e => e.Payload["Missing"] ?? (object)e.Date),
            ("Coalesce", "LocalTimestamp") => entities.Select(e => e.Payload["Missing"] ?? (object)e.LocalTimestamp),
            _ => throw new UnreachableException()
        };
        var values = await query.ToArrayAsync();
        Assert.Equal(
            [JsonSerializer.SerializeToElement(minimum).GetString(), JsonSerializer.SerializeToElement(maximum).GetString()],
            values.Select(value => Assert.IsType<JsonElement>(value).GetString()));

        var text = $$"""to_jsonb(e."{{type}}"{{(type is "Timestamp" or "TimestampOffset" ? " AT TIME ZONE 'UTC'" : "")}}) #>> '{}'""";
        var finite = type is "Timestamp" or "TimestampOffset"
            ? $"regexp_replace({text}, '$', '{(type == "TimestampOffset" ? "+00:00" : "Z")}')"
            : text;
        var serialized = $"""
{(jsonb || operation == "Predicate" ? "to_jsonb" : "to_json")}(CASE
    WHEN {text} = '-infinity' THEN '{JsonSerializer.SerializeToElement(minimum).GetString()}'
    WHEN {text} = 'infinity' THEN '{JsonSerializer.SerializeToElement(maximum).GetString()}'
    ELSE {finite}
END)
""";
        var traversal = """e."Payload" -> 'Timestamp'""";
        var left = $"NULLIF({(jsonb ? traversal : $"CAST({traversal} AS jsonb)")}, 'null'::jsonb)";
        var right = $"NULLIF({serialized}, 'null'::jsonb)";
        var sql = operation switch
        {
            "Predicate" => $"""
SELECT {traversal}
FROM "Entities" AS e
WHERE {left} = {right} OR ({left} IS NULL AND {right} IS NULL)
ORDER BY e."Id" NULLS FIRST
""",
            "Case" => $"""
SELECT CASE
    WHEN e."Id" = 1 THEN {traversal}
    ELSE {serialized.Replace("\n", "\n    ")}
END
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""",
            "Coalesce" => $"""
SELECT COALESCE(CASE
    WHEN e."Payload" ->> 'Missing' IS NULL THEN NULL
    ELSE e."Payload" -> 'Missing'
END, {serialized})
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""",
            _ => throw new UnreachableException()
        };
        AssertSql(sql);
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public async Task DateOnly_json_dictionary_parameter_extrema(bool jsonb, bool maximum, bool constant)
    {
        var date = maximum ? DateOnly.MaxValue : DateOnly.MinValue;
        var contextFactory = await InitializeNonSharedTest<TimestampJsonContext>(
            onModelCreating: modelBuilder => modelBuilder.Entity<TimestampJsonEntity>().Property(e => e.Payload)
                .HasColumnType(jsonb ? "jsonb" : "json"),
            onConfiguring: options => options.UseNpgsql(
                ((NpgsqlTestStore)NonSharedTestStore).ConnectionString,
                o => o.ConfigureDataSource(b => b.EnableDynamicJson())),
            seed: async context =>
            {
                context.Entities.Add(new TimestampJsonEntity { Id = 1, Payload = new() { ["Date"] = date } });
                await context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var query = constant
            ? context.Entities.Where(e => e.Payload["Date"] == (object)EF.Constant(date))
            : context.Entities.Where(e => e.Payload["Date"] == (object)EF.Parameter(date));
        Assert.Equal(1, await query.Select(e => e.Id).SingleAsync());

        var traversal = """e."Payload" -> 'Date'""";
        var left = $"NULLIF({(jsonb ? traversal : $"CAST({traversal} AS jsonb)")}, 'null'::jsonb)";
        var serialized = JsonSerializer.SerializeToElement(date).GetString();
        var right = $"NULLIF({(constant ? $"'\"{serialized}\"'" : "@date")}, 'null'::jsonb)";
        AssertSql(
            (constant ? "" : $"@date='{serialized}' (DbType = Object)\n\n") + $"""
SELECT e."Id"
FROM "Entities" AS e
WHERE {left} = {right} OR ({left} IS NULL AND {right} IS NULL)
LIMIT 2
""");
    }

    [Theory]
    [InlineData(false, "Predicate")]
    [InlineData(true, "Predicate")]
    [InlineData(false, "Case")]
    [InlineData(true, "Case")]
    [InlineData(false, "Coalesce")]
    [InlineData(true, "Coalesce")]
    public async Task Native_converted_column_json_composition(bool jsonb, string operation)
    {
        var contextFactory = await InitializeNonSharedTest<NativeConvertedJsonContext>(
            onModelCreating: modelBuilder =>
            {
                modelBuilder.Entity<NativeConvertedJsonEntity>().Property(e => e.Payload).HasColumnType(jsonb ? "jsonb" : "json");
                modelBuilder.Entity<NativeConvertedJsonEntity>().Property(e => e.Number).HasConversion(value => -value, value => -value);
            },
            onConfiguring: options => options.UseNpgsql(
                ((NpgsqlTestStore)NonSharedTestStore).ConnectionString,
                o => o.ConfigureDataSource(b => b.EnableDynamicJson())),
            seed: async context =>
            {
                context.Entities.AddRange(
                    new NativeConvertedJsonEntity { Id = 1, Number = 25, Payload = new() { ["Number"] = 25 } },
                    new NativeConvertedJsonEntity { Id = 2, Number = 35, Payload = new() { ["Number"] = 35 } });
                await context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var entities = context.Entities.OrderBy(e => e.Id);
        if (operation == "Predicate")
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => entities.Where(e => e.Payload["Number"] == (object)e.Number).Select(e => e.Id).ToArrayAsync());
            Assert.Equal("Converting value-converted PostgreSQL expressions to JSON is not supported.", exception.Message);
            AssertSql();
            return;
        }

        var query = operation == "Case"
            ? entities.Select(e => e.Id == 1 ? e.Payload["Number"] : (object)e.Number)
            : entities.Select(e => e.Payload["Missing"] ?? (object)e.Number);
        var values = await query.ToArrayAsync();
        Assert.Equal(35, Assert.IsType<int>(values[1]));
        if (operation == "Case")
        {
            Assert.Equal(25, Assert.IsType<JsonElement>(values[0]).GetInt32());
        }
        else
        {
            Assert.Equal(25, Assert.IsType<int>(values[0]));
        }

        AssertSql(
            operation == "Case"
                ? """
SELECT e."Id" = 1, e."Payload" -> 'Number', e."Number"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
"""
                : """
SELECT e."Payload" -> 'Missing', e."Number"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    protected class NativeConvertedJsonContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<NativeConvertedJsonEntity> Entities { get; set; }
    }

    public class NativeConvertedJsonEntity
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public Dictionary<string, object> Payload { get; set; }
    }

    protected class TimestampJsonContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<TimestampJsonEntity> Entities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<TimestampJsonEntity>().Property(e => e.LocalTimestamp).HasColumnType("timestamp without time zone");
    }

    public class TimestampJsonEntity
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTimeOffset TimestampOffset { get; set; }
        public DateOnly Date { get; set; }
        public DateTime LocalTimestamp { get; set; }
        public Dictionary<string, object> Payload { get; set; }
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

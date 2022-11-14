# Translations

Entity Framework Core allows providers to translate query expressions to SQL for database evaluation. For example, PostgreSQL supports [regular expression operations](http://www.postgresql.org/docs/current/static/functions-matching.html#FUNCTIONS-POSIX-REGEXP), and the Npgsql EF Core provider automatically translates .NET's [`Regex.IsMatch`](https://docs.microsoft.com/dotnet/api/system.text.regularexpressions.regex.ismatch) to use this feature. Since evaluation happens at the server, table data doesn't need to be transferred to the client (saving bandwidth), and in some cases indexes can be used to speed things up. The same C# code on other providers will trigger client evaluation.

The Npgsql-specific translations are listed below. Some areas, such as [full-text search](full-text-search.md), have their own pages in this section which list additional translations.

## String functions

.NET                                                          | SQL                                                                                                                     | Notes
------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------- | --------
EF.Functions.Collate(operand, collation)                      | operand COLLATE collation                                                                                               | Added in 5.0
EF.Functions.Like(matchExpression, pattern)                   | matchExpression LIKE pattern
EF.Functions.Like(matchExpression, pattern, escapeCharacter)  | matchExpression LIKE pattern ESCAPE escapeCharacter
EF.Functions.ILike(matchExpression, pattern)                  | [matchExpression ILIKE pattern](../misc/collations-and-case-sensitivity.md)
EF.Functions.ILike(matchExpression, pattern, escapeCharacter) | [matchExpression ILIKE pattern ESCAPE escapeCharacter](../misc/collations-and-case-sensitivity.md)
string.Compare(strA, strB)                                    | CASE WHEN strA = strB THEN 0 ... END
string.Concat(str0, str1)                                     | str0 \|\| str1
string.IsNullOrEmpty(value)                                   | value IS NULL OR value = ''
string.IsNullOrWhiteSpace(value)                              | value IS NULL OR btrim(value, E' \t\n\r') = ''
stringValue.CompareTo(strB)                                   | CASE WHEN stringValue = strB THEN 0 ... END
stringValue.Contains(value)                                   | strpos(stringValue, value) > 0
stringValue.EndsWith(value)                                   | stringValue LIKE '%' \|\| value
stringValue.FirstOrDefault()                                  | substr(stringValue, 1, 1)                                                                                               | Added in 5.0
stringValue.IndexOf(value)                                    | strpos(stringValue, value) - 1
stringValue.LastOrDefault()                                   | substr(stringValue, length(stringValue), 1)                                                                             | Added in 5.0
stringValue.Length                                            | length(stringValue)
stringValue.PadLeft(length)                                   | lpad(stringValue, length)
stringValue.PadLeft(length, char)                             | lpad(stringValue, length, char)
stringValue.PadRight(length)                                  | rpad(stringValue, length)
stringValue.PadRight(length, char)                            | rpad(stringValue, length, char)
stringValue.Replace(oldValue, newValue)                       | replace(stringValue, oldValue, newValue)
stringValue.StartsWith(value)                                 | stringValue LIKE value \|\| '%'
stringValue.Substring(startIndex, length)                     | substr(stringValue, startIndex + 1, @length)
stringValue.ToLower()                                         | lower(stringValue)
stringValue.ToUpper()                                         | upper(stringValue)
stringValue.Trim()                                            | btrim(stringValue)
stringValue.Trim(trimChar)                                    | btrim(stringValue, trimChar)
stringValue.TrimEnd()                                         | rtrim(stringValue)
stringValue.TrimEnd(trimChar)                                 | rtrim(stringValue, trimChar)
stringValue.TrimStart()                                       | ltrim(stringValue)
stringValue.TrimStart(trimChar)                               | ltrim(stringValue, trimChar)
EF.Functions.Reverse(value)                                   | reverse(value)
Regex.IsMatch(stringValue, "^A+")                             | [stringValue ~ '^A+'](http://www.postgresql.org/docs/current/static/functions-matching.html#FUNCTIONS-POSIX-REGEXP) (with options)
Regex.IsMatch(stringValue, "^A+", regexOptions)               | [stringValue ~ '^A+'](http://www.postgresql.org/docs/current/static/functions-matching.html#FUNCTIONS-POSIX-REGEXP) (with options)
string.Join(", ", a, b)                                       | [concat_ws(', ', a, b)](https://www.postgresql.org/docs/current/functions-string.html#FUNCTIONS-STRING-OTHER) | Added in 7.0 (previously array_to_string)
string.Join(", ", array)                                      | [array_to_string(array, ', ', '')](https://www.postgresql.org/docs/current/functions-array.html#ARRAY-FUNCTIONS-TABLE)
string.Join(", ", agg_strings)                                | [string_agg(agg_strings, ', ')](https://www.postgresql.org/docs/current/functions-aggregate.html#FUNCTIONS-AGGREGATE-TABLE) | Added in 7.0, see [Aggregate functions](#aggregate-functions).

## Date and time functions

> [!NOTE]
> Some of the operations below depend on the concept of a "local time zone" (e.g. `DateTime.Today`). While in .NET this is the machine time zone where .NET is running, the corresponding PostgreSQL translations use the [`TimeZone`](https://www.postgresql.org/docs/current/runtime-config-client.html#GUC-TIMEZONE) connection parameter as the local time zone.
>
> Since version 6.0, many of the below DateTime translations are also supported on DateTimeOffset.
>
> See also Npgsql's [NodaTime support](/efcore/mapping/nodatime.html), which is a better and safer way of interacting with date/time data.

.NET                                                              | SQL                                                                                                                                    | Notes
----------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------- | --------
DateTime.UtcNow (6.0+)                                            | [now()](https://www.postgresql.org/docs/current/functions-datetime.html)                                                               | See 6.0 release notes
DateTime.Now (6.0+)                                               | [now()::timestamp](https://www.postgresql.org/docs/current/functions-datetime.html)                                                    | See 6.0 release notes
DateTime.Today (6.0+)                                             | [date_trunc('day', now()::timestamp)](https://www.postgresql.org/docs/current/functions-datetime.html)                                 | See 6.0 release notes
DateTime.UtcNow (legacy)                                          | [now() AT TIME ZONE 'UTC'](https://www.postgresql.org/docs/current/functions-datetime.html)                                            | See 6.0 release notes
DateTime.Now (legacy)                                             | [now()](https://www.postgresql.org/docs/current/functions-datetime.html)                                                               | See 6.0 release notes
DateTime.Today (legacy)                                           | [date_trunc('day', now())](https://www.postgresql.org/docs/current/functions-datetime.html)                                            | See 6.0 release notes
dateTime.AddDays(1)                                               | [dateTime + INTERVAL '1 days'](https://www.postgresql.org/docs/current/functions-datetime.html#OPERATORS-DATETIME-TABLE)               |
dateTime.AddHours(value)                                          | [dateTime + INTERVAL '1 hours'](https://www.postgresql.org/docs/current/functions-datetime.html#OPERATORS-DATETIME-TABLE)              |
dateTime.AddMinutes(1)                                            | [dateTime + INTERVAL '1 minutes'](https://www.postgresql.org/docs/current/functions-datetime.html#OPERATORS-DATETIME-TABLE)            |
dateTime.AddMonths(1)                                             | [dateTime + INTERVAL '1 months'](https://www.postgresql.org/docs/current/functions-datetime.html#OPERATORS-DATETIME-TABLE)             |
dateTime.AddSeconds(1)                                            | [dateTime + INTERVAL '1 seconds'](https://www.postgresql.org/docs/current/functions-datetime.html#OPERATORS-DATETIME-TABLE)            |
dateTime.AddYears(1)                                              | [dateTime + INTERVAL '1 years'](https://www.postgresql.org/docs/current/functions-datetime.html#OPERATORS-DATETIME-TABLE)              |
dateTime.Date                                                     | [date_trunc('day', dateTime)](https://www.postgresql.org/docs/current/functions-datetime.html)                                         |
dateTime.Day                                                      | [date_part('day', dateTime)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                                     |
dateTime.DayOfWeek                                                | [floor(date_part('dow', dateTime))::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                              |
dateTime.DayOfYear                                                | [date_part('doy', dateTime)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                                     |
dateTime.Hour                                                     | [date_part('hour', dateTime)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                                    |
dateTime.Minute                                                   | [date_part('minute', dateTime)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                                  |
dateTime.Month                                                    | [date_part('month', dateTime)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                                   |
dateTime.Second                                                   | [date_part('second', dateTime)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                                  |
dateTime.Year                                                     | [date_part('year', dateTime)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                                    |
dateTime.ToUniversalTime                                          | [dateTime::timestamptz](https://www.postgresql.org/docs/current/datatype-datetime.html#id-1.5.7.13.18.7)                               | Added in 6.0
dateTime.ToLocalTime                                              | [dateTime::timestamp](https://www.postgresql.org/docs/current/datatype-datetime.html#id-1.5.7.13.18.7)                                 | Added in 6.0
dateTimeOffset.DateTime                                           | [dateTimeOffset AT TIME ZONE 'UTC'](https://www.postgresql.org/docs/current/functions-datetime.html#FUNCTIONS-DATETIME-ZONECONVERT)    | Added in 6.0
dateTimeOffset.UtcDateTime                                        | No PG operation (.NET-side conversion from DateTimeOffset to DateTime only)                                                            | Added in 6.0
dateTimeOffset.LocalDateTime                                      | [dateTimeOffset::timestamp](https://www.postgresql.org/docs/current/datatype-datetime.html#id-1.5.7.13.18.7)                           | Added in 6.0
timeSpan.Days                                                     | [floor(date_part('day', timeSpan))::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                              |
timeSpan.Hours                                                    | [floor(date_part('hour', timeSpan))::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                             |
timeSpan.Minutes                                                  | [floor(date_part('minute', timeSpan))::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                           |
timeSpan.Seconds                                                  | [floor(date_part('second', timeSpan))::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                           |
timeSpan.Milliseconds                                             | [floor(date_part('millisecond', timeSpan))::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                      |
timeSpan.Milliseconds                                             | [floor(date_part('millisecond', timeSpan))::INT](https://www.postgresql.org/docs/current/functions-datetime.html)                      |
timeSpan.TotalMilliseconds                                        | [date_part('epoch', interval) / 0.001](https://www.postgresql.org/docs/current/functions-datetime.html)                                | Added in 6.0
timeSpan.TotalSeconds                                             | [date_part('epoch', interval)](https://www.postgresql.org/docs/current/functions-datetime.html)                                        | Added in 6.0
timeSpan.TotalMinutes                                             | [date_part('epoch', interval) / 60.0](https://www.postgresql.org/docs/current/functions-datetime.html)                                 | Added in 6.0
timeSpan.TotalDays                                                | [date_part('epoch', interval) / 86400.0](https://www.postgresql.org/docs/current/functions-datetime.html)                              | Added in 6.0
timeSpan.TotalHours                                               | [date_part('epoch', interval) / 3600.0](https://www.postgresql.org/docs/current/functions-datetime.html)                               | Added in 6.0
dateTime1 - dateTime2                                             | [dateTime1 - dateTime2](https://www.postgresql.org/docs/current/functions-datetime.html#OPERATORS-DATETIME-TABLE)                      |
TimeZoneInfo.ConvertTimeBySystemTimeZoneId(utcDateTime, timezone) | [utcDateTime AT TIME ZONE timezone](https://www.postgresql.org/docs/current/functions-datetime.html#FUNCTIONS-DATETIME-ZONECONVERT)    | Added in 6.0, only for timestamptz columns
TimeZoneInfo.ConvertTimeToUtc(nonUtcDateTime)                     | [nonUtcDateTime::timestamptz](https://www.postgresql.org/docs/current/functions-datetime.html)                                         | Added in 6.0, only for timestamp columns
DateTime.SpecifyKind(utcDateTime, DateTimeKind.Unspecified)       | [utcDateTime AT TIME ZONE 'UTC'](https://www.postgresql.org/docs/current/functions-datetime.html#FUNCTIONS-DATETIME-ZONECONVERT)       | Added in 6.0, only for timestamptz columns
DateTime.SpecifyKind(nonUtcDateTime, DateTimeKind.Utc)            | [nonUtcDateTime AT TIME ZONE 'UTC'](https://www.postgresql.org/docs/current/functions-datetime.html#FUNCTIONS-DATETIME-ZONECONVERT)    | Added in 6.0, only for timestamp columns
new DateTime(year, month, day)                                    | [make_date(year, month, day)](https://www.postgresql.org/docs/current/functions-datetime.html)                                         |
new DateTime(y, m, d, h, m, s)                                    | [make_timestamp(y, m, d, h, m, s)](https://www.postgresql.org/docs/current/functions-datetime.html)                                    |
new DateTime(y, m, d, h, m, s, kind)                              | [make_timestamp or make_timestamptz](https://www.postgresql.org/docs/current/functions-datetime.html), based on `kind`                 | Added in 6.0
EF.Functions.Sum(timespans)                                       | [sum(timespans)](https://www.postgresql.org/docs/current/functions-aggregate.html#FUNCTIONS-AGGREGATE-TABLE)                           | Added in 7.0, see [Aggregate functions](#aggregate-functions).
EF.Functions.Average(timespans)                                   | [avg(timespans)](https://www.postgresql.org/docs/current/functions-aggregate.html#FUNCTIONS-AGGREGATE-TABLE)                           | Added in 7.0, see [Aggregate functions](#aggregate-functions).

## Miscellaneous functions

.NET                                     | SQL
---------------------------------------- | --------------------
collection.Contains(item)                | item IN collection
enumValue.HasFlag(flag)                  | enumValue & flag = flag
Guid.NewGuid()                           | [uuid_generate_v4()](https://www.postgresql.org/docs/current/uuid-ossp.html), or [gen_random_uuid()](https://www.postgresql.org/docs/current/functions-uuid.html) on PostgreSQL 13 with EF Core 5 and above.
nullable.GetValueOrDefault()             | coalesce(nullable, 0)
nullable.GetValueOrDefault(defaultValue) | coalesce(nullable, defaultValue)

## Binary functions

.NET                         | SQL                             | Notes
---------------------------- | ------------------------------- | --------
bytes[i]                     | get_byte(bytes, i)              | Added in 5.0
bytes.Contains(value)        | position(value IN bytes) > 0    | Added in 5.0
bytes.Length                 | length(@bytes)                  | Added in 5.0
bytes1.SequenceEqual(bytes2) | @bytes = @second                | Added in 5.0

## Math functions

.NET                    | SQL                | Notes
----------------------- | ------------------ | -----
Math.Abs(value)         | abs(value)         |
Math.Acos(d)            | acos(d)            |
Math.Asin(d)            | asin(d)            |
Math.Atan(d)            | atan(d)            |
Math.Atan2(y, x)        | atan2(y, x)        |
Math.Ceiling(d)         | ceiling(d)         |
Math.Cos(d)             | cos(d)             |
Math.Exp(d)             | exp(d)             |
Math.Floor(d)           | floor(d)           |
Math.Log(d)             | ln(d)              |
Math.Log10(d)           | log(d)             |
Math.Max(x, y)          | greatest(x, y)     |
Math.Min(x, y)          | least(x, y)        |
Math.Pow(x, y)          | power(x, y)        |
Math.Round(d)           | round(d)           |
Math.Round(d, decimals) | round(d, decimals) |
Math.Sin(a)             | sin(a)             |
Math.Sign(value)        | sign(value)::int   |
Math.Sqrt(d)            | sqrt(d)            |
Math.Tan(a)             | tan(a)             |
Math.Truncate(d)        | trunc(d)           |
EF.Functions.Random()   | random()           | Added in 6.0

See also [Aggregate statistics functions](#aggregate-functions).

## Row value comparisons

The following allow expressing [comparisons over SQL row values](https://www.postgresql.org/docs/current/functions-comparisons.html#ROW-WISE-COMPARISON). This are particularly useful for implementing efficient pagination, see [the EF Core docs](https://docs.microsoft.com/ef/core/querying/pagination) for more information.

> [!NOTE]
> All of the below were introduced in version 7.0 of the provider.

.NET                                                                              | SQL
--------------------------------------------------------------------------------- | ----------------
EF.Functions.GreaterThan(ValueTuple.Create(a, b), ValueTuple.Create(c, d))        | (a, b) > (c, d)
EF.Functions.LessThan(ValueTuple.Create(a, b), ValueTuple.Create(c, d))           | (a, b) < (c, d)
EF.Functions.GreaterThanOrEqual(ValueTuple.Create(a, b), ValueTuple.Create(c, d)) | (a, b) >= (c, d)
EF.Functions.LessThanOrEqual(ValueTuple.Create(a, b), ValueTuple.Create(c, d))    | (a, b) <= (c, d)
ValueTuple.Create(a, b).Equals(ValueTuple.Create(c, d))                           | (a, b) = (c, d)
!ValueTuple.Create(a, b).Equals(ValueTuple.Create(c, d))                          | (a, b) <> (c, d)

## Network functions

.NET                                             | SQL
------------------------------------------------ | ---
IPAddress.Parse(string)                          | [CAST(string AS inet)](https://www.postgresql.org/docs/current/datatype-net-types.html#DATATYPE-INET)
PhysicalAddress.Parse(string)                    | [CAST(string AS macaddr)](https://www.postgresql.org/docs/current/datatype-net-types.html#DATATYPE-MACADDR)
EF.Functions.LessThan(net1, net2)                | [net1 < net2](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-OPERATORS-TABLE)
EF.Functions.LessThanOrEqual(net1, net2)         | [net1 <= net2](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-OPERATORS-TABLE)
EF.Functions.GreaterThan(net1, net2)             | [net1 > net2](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-OPERATORS-TABLE)
EF.Functions.GreaterThanOrEqual(net1, net2)      | [net1 >= net2](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-OPERATORS-TABLE)
EF.Functions.ContainedBy(inet1, inet2)           | [inet1 << inet2](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-OPERATORS-TABLE)
EF.Functions.ContainedByOrEqual(inet1, inet2)    | [inet1 <<= inet2](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-OPERATORS-TABLE)
EF.Functions.Contains(inet1, inet2)              | [inet1 >> inet2](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-OPERATORS-TABLE)
EF.Functions.ContainsOrEqual(inet1, inet2)       | [inet1 >>= inet2](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-OPERATORS-TABLE)
EF.Functions.ContainsOrContainedBy(inet1, inet2) | [inet1 && inet2](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-OPERATORS-TABLE)
EF.Functions.BitwiseNot(net)                     | [~net1](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-OPERATORS-TABLE)
EF.Functions.BitwiseAnd(net1, net2)              | [net1 & net2](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-OPERATORS-TABLE)
EF.Functions.BitwiseOr(net1, net2)               | [net1 \| net2](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-OPERATORS-TABLE)
EF.Functions.Add(inet, int)                      | [inet + int](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-OPERATORS-TABLE)
EF.Functions.Subtract(inet, int)                 | [inet - int](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-OPERATORS-TABLE)
EF.Functions.Subtract(inet1, inet2)              | [inet1 - inet2](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-OPERATORS-TABLE)
EF.Functions.Abbreviate(inet)                    | [abbrev(inet)](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-FUNCTIONS-TABLE)
EF.Functions.Broadcast(inet)                     | [broadcast(inet)](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-FUNCTIONS-TABLE)
EF.Functions.Family(inet)                        | [family(inet)](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-FUNCTIONS-TABLE)
EF.Functions.Host(inet)                          | [host(inet)](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-FUNCTIONS-TABLE)
EF.Functions.HostMark(inet)                      | [hostmask(inet)](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-FUNCTIONS-TABLE)
EF.Functions.MaskLength(inet)                    | [masklen(inet)](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-FUNCTIONS-TABLE)
EF.Functions.Netmask(inet)                       | [netmask(inet)](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-FUNCTIONS-TABLE)
EF.Functions.Network(inet)                       | [network(inet)](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-FUNCTIONS-TABLE)
EF.Functions.SetMaskLength(inet)                 | [set_masklen(inet)](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-FUNCTIONS-TABLE)
EF.Functions.Text(inet)                          | [text(inet)](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-FUNCTIONS-TABLE)
EF.Functions.SameFamily(inet1, inet2)            | [inet_same_family(inet1, inet2)](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-FUNCTIONS-TABLE)
EF.Functions.Merge(inet1, inet2)                 | [inet_merge(inet1, inet2)](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-FUNCTIONS-TABLE)
EF.Functions.Truncate(macaddr)                   | [trunc(macaddr)](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-FUNCTIONS-TABLE)
EF.Functions.Set7BitMac8(macaddr8)               | [macaddr8_set7bit(macaddr8)](https://www.postgresql.org/docs/current/functions-net.html#CIDR-INET-FUNCTIONS-TABLE)

## Trigram functions

The below translations provide functionality for determining the similarity of alphanumeric text based on trigram matching, using the [`pg_trgm`](https://www.postgresql.org/docs/current/pgtrgm.html) extension which is bundled with standard PostgreSQL distributions. All the below parameters are strings.

> [!NOTE]
> Prior to version 6.0, to use these translations, your project must depend on the [Npgsql.EntityFrameworkCore.PostgreSQL.Trigrams](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL.Trigrams/) package, and call `UseTrigrams()` in your `OnModelConfiguring`.

.NET                                                              | SQL
----------------------------------------------------------------- | --------------------
EF.Functions.TrigramsShow(s)                                      | show_trgm(s)
EF.Functions.TrigramsSimilarity(s1, s2)                           | similarity(s1, s2)
EF.Functions.TrigramsWordSimilarity(s1, s2)                       | word_similarity(s1, s2)
EF.Functions.TrigramsStrictWordSimilarity(s1, s2)                 | strict_word_similarity(s1, s2)
EF.Functions.TrigramsAreSimilar(s1, s2)                           | s1 % s2
EF.Functions.TrigramsAreWordSimilar(s1, s2)                       | s1 &lt;% s2
EF.Functions.TrigramsAreNotWordSimilar(s1, s2)                    | s1 %&gt; s2
EF.Functions.TrigramsAreStrictWordSimilar(s1, s2)                 | s1 &lt;&lt;% s2
EF.Functions.TrigramsAreNotStrictWordSimilar(s1, s2)              | s1 %&gt;&gt; s2
EF.Functions.TrigramsSimilarityDistance(s1, s2)                   | s1 &lt;-&gt; s2
EF.Functions.TrigramsWordSimilarityDistance(s1, s2)               | s1 &lt;&lt;-&gt; s2
EF.Functions.TrigramsWordSimilarityDistanceInverted(s1, s2)       | s1 &lt;-&gt;&gt; s2
EF.Functions.TrigramsStrictWordSimilarityDistance(s1, s2)         | s1 &lt;&lt;&lt;-&gt; s2
EF.Functions.TrigramsStrictWordSimilarityDistanceInverted(s1, s2) | s1 &lt;-&gt;&gt;&gt; s2

## LTree functions

The below translations are for working with label trees from the PostgreSQL [`ltree`](https://www.postgresql.org/docs/current/ltree.html) extension. Use the <xref:Microsoft.EntityFrameworkCore.LTree> type to represent ltree and invoke methods on it in EF Core LINQ queries.

> [!NOTE]
> LTree support was introduced in version 6.0 of the provider, and requires PostgreSQL 13 or later.

.NET                                                              | SQL
----------------------------------------------------------------- | --------------------
ltree1.IsAncestorOf(ltree2)                                       | ltree1 @&gt; ltree2
ltree1.IsDescendantOf(ltree2)                                     | ltree1 &lt;@ ltree2
ltree.MatchesLQuery(lquery)                                       | ltree ~ lquery
ltree.MatchesLTxtQuery(ltxtquery)                                 | ltree @ ltxtquery
lqueries.Any(q => ltree.MatchesLQuery(q))                         | ltree ? lqueries
ltrees.Any(t => t.IsAncestorOf(ltree))                            | ltrees @&gt; ltree
ltrees.Any(t => t.IsDescendantOf(ltree))                          | ltrees &lt;@ ltree
ltrees.Any(t => t.MatchesLQuery(lquery))                          | ltrees ~ ltree
ltrees.Any(t => t.MatchesLTxtQuery(ltxtquery))                    | ltrees @ ltxtquery
ltrees.Any(t => lqueries.Any(q => t.MatchesLQuery(q)))            | ltrees ? lqueries
ltrees.FirstOrDefault(l => l.IsAncestorOf(ltree))                 | ltrees ?@&gt; ltree
ltrees.FirstOrDefault(l => l.IsDescendantOf(ltree))               | ltrees ?&lt;@ ltree
ltrees.FirstOrDefault(l => l.MatchesLQuery(lquery))               | ltrees ?~ ltree
ltrees.FirstOrDefault(l => l.MatchesLTxtQuery(ltxtquery))         | ltrees ?@ ltree
ltree.Subtree(0, 1)                                               | subltree(ltree, 0, 1)
ltree.Subpath(0, 1)                                               | sublpath(ltree, 0, 1)
ltree.Subpath(2)                                                  | sublpath(ltree, 2)
ltree.NLevel                                                      | nlevel(ltree)
ltree.Index(subpath)                                              | index(ltree, subpath)
ltree.Index(subpath, 2)                                           | index(ltree, subpath, 2)
LTree.LongestCommonAncestor(ltree1, ltree2)                       | lca(index(ltree1, ltree2)

## Aggregate functions

The PostgreSQL aggregate functions are documented [here](https://www.postgresql.org/docs/current/functions-aggregate.html).

> [!NOTE]
> All the below aggregate functions were added in version 7.0.

| .NET                                                                               | SQL
| ---------------------------------------------------------------------------------- | --------------------
| string.Join(", ", agg_strings)                                                     | string_agg(agg_strings, ', ')
| EF.Functions.ArrayAgg(values)                                                      | array_agg(values)
| EF.Functions.JsonbAgg(values)                                                      | jsonb_agg(values)
| EF.Functions.JsonAgg(values)                                                       | json_agg(values)
| EF.Functions.Sum(timespans)                                                        | sum(timespans)
| EF.Functions.Average(timespans)                                                    | avg(timespans)
| EF.Functions.JsonObjectAgg(tuple_of_2)                                             | json_object_agg(tuple_of_2.first, tuple_of_2.second)
| ranges.RangeAgg()                                                                  | range_agg(ranges)
| ranges.RangeIntersectAgg()                                                         | range_intersect_agg(ranges)
| multiranges.RangeIntersectAgg()                                                    | range_intersect_agg(multiranges)
|                                                                                    |
| EF.Functions.StandardDeviationSample(values)                                       | stddev_samp(values)
| EF.Functions.StandardDeviationPopulation(values)                                   | stddev_pop(values)
| EF.Functions.VarianceSample(values)                                                | var_samp(values)
| EF.Functions.VariancePopulation(values)                                            | var_pop(values)
|                                                                                    |
| EF.Functions.Correlation(tuple)                                                    | corr(tuple_of_2.first, tuple_of_2.second)
| EF.Functions.CovariancePopulation(tuple)                                           | covar_pop(tuple_of_2.first, tuple_of_2.second)
| EF.Functions.CovarianceSample(tuple)                                               | covar_samp(tuple_of_2.first, tuple_of_2.second)
| EF.Functions.RegrAverageX(tuple)                                                   | regr_avgx(tuple_of_2.first, tuple_of_2.second)
| EF.Functions.RegrAverageY(tuple)                                                   | regr_avgy(tuple_of_2.first, tuple_of_2.second)
| EF.Functions.RegrCount(tuple)                                                      | regr_count(tuple_of_2.first, tuple_of_2.second)
| EF.Functions.RegrIntercept(tuple)                                                  | regr_intercept(tuple_of_2.first, tuple_of_2.second)
| EF.Functions.RegrR2(tuple)                                                         | regr_r2(tuple_of_2.first, tuple_of_2.second)
| EF.Functions.RegrSlope(tuple)                                                      | regr_slope(tuple_of_2.first, tuple_of_2.second)
| EF.Functions.RegrSXX(tuple)                                                        | regr_sxx(tuple_of_2.first, tuple_of_2.second)
| EF.Functions.RegrSXY(tuple)                                                        | regr_sxy(tuple_of_2.first, tuple_of_2.second)

Aggregate functions can be used as follows:

```c#
var query = ctx.Set<Customer>()
    .GroupBy(c => c.City)
    .Select(
        g => new
        {
            City = g.Key,
            Companies = EF.Functions.ArrayAgg(g.Select(c => c.ContactName))
        });
```

To use functions accepting a tuple_of_2, project out from the group as follows:

```c#
var query = ctx.Set<Customer>()
    .GroupBy(c => c.City)
    .Select(
        g => new
        {
            City = g.Key,
            Companies = EF.Functions.JsonObjectAgg(g.Select(c => ValueTuple.Create(c.CompanyName, c.ContactName)))
        });
```

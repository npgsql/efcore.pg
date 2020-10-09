# Translations

Entity Framework Core allows providers to translate query expressions to SQL for database evaluation. For example, PostgreSQL supports [regular expression operations](http://www.postgresql.org/docs/current/static/functions-matching.html#FUNCTIONS-POSIX-REGEXP), and the Npgsql EF Core provider automatically translates .NET's [`Regex.IsMatch`](https://docs.microsoft.com/dotnet/api/system.text.regularexpressions.regex.ismatch) to use this feature. Since evaluation happens at the server, table data doesn't need to be transferred to the client (saving bandwidth), and in some cases indexes can be used to speed things up. The same C# code on other providers will trigger client evaluation.

The Npgsql-specific translations are listed below. Some areas, such as [full-text search](full-text-search.md), have their own pages in this section which list additional translations.

## Binary functions

.NET                         | SQL                             | Added in
---------------------------- | ------------------------------- | --------
bytes[i]                     | get_byte(bytes, i)              | EF Core 5.0
bytes.Contains(value)        | position(value IN bytes) > 0    | EF Core 5.0
bytes.Length                 | length(@bytes)                  | EF Core 5.0
bytes1.SequenceEqual(bytes2) | @bytes = @second                | EF Core 5.0

## Date and time functions

.NET                           | SQL
------------------------------ | ---
DateTime.Now                   | [now()](https://www.postgresql.org/docs/current/functions-datetime.html)
DateTime.Today                 | [date_trunc('day', now())](https://www.postgresql.org/docs/current/functions-datetime.html)
DateTime.UtcNow                | [now() AT TIME ZONE 'UTC'](https://www.postgresql.org/docs/current/functions-datetime.html)
dateTime.AddDays(1)            | [dateTime + INTERVAL '1 days'](https://www.postgresql.org/docs/current/functions-datetime.html#OPERATORS-DATETIME-TABLE)
dateTime.AddHours(value)       | [dateTime + INTERVAL '1 hours'](https://www.postgresql.org/docs/current/functions-datetime.html#OPERATORS-DATETIME-TABLE)
dateTime.AddMinutes(1)         | [dateTime + INTERVAL '1 minutes'](https://www.postgresql.org/docs/current/functions-datetime.html#OPERATORS-DATETIME-TABLE)
dateTime.AddMonths(1)          | [dateTime + INTERVAL '1 months'](https://www.postgresql.org/docs/current/functions-datetime.html#OPERATORS-DATETIME-TABLE)
dateTime.AddSeconds(1)         | [dateTime + INTERVAL '1 seconds'](https://www.postgresql.org/docs/current/functions-datetime.html#OPERATORS-DATETIME-TABLE)
dateTime.AddYears(1)           | [dateTime + INTERVAL '1 years'](https://www.postgresql.org/docs/current/functions-datetime.html#OPERATORS-DATETIME-TABLE)
dateTime.Date                  | [date_trunc('day', dateTime)](https://www.postgresql.org/docs/current/functions-datetime.html)
dateTime.Day                   | [date_part('day', dateTime)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
dateTime.DayOfWeek             | [floor(date_part('dow', o.""OrderDate""))::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
dateTime.DayOfYear             | [date_part('doy', dateTime)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
dateTime.Hour                  | [date_part('hour', dateTime)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
dateTime.Minute                | [date_part('minute', dateTime)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
dateTime.Month                 | [date_part('month', dateTime)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
dateTime.Second                | [date_part('second', dateTime)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
dateTime.Year                  | [date_part('year', dateTime)::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
timeSpan.Days                  | [floor(date_part('day', timeSpan))::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
timeSpan.Hours                 | [floor(date_part('hour', timeSpan))::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
timeSpan.Minutes               | [floor(date_part('minute', timeSpan))::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
timeSpan.Seconds               | [floor(date_part('second', timeSpan))::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
timeSpan.Milliseconds          | [floor(date_part('millisecond', timeSpan))::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
timeSpan.Milliseconds          | [floor(date_part('millisecond', timeSpan))::INT](https://www.postgresql.org/docs/current/functions-datetime.html)
dateTime1 - dateTime2          | [dateTime1 - dateTime2](https://www.postgresql.org/docs/current/functions-datetime.html#OPERATORS-DATETIME-TABLE)
new DateTime(year, month, day) | [make_date(year, month, day)](https://www.postgresql.org/docs/current/functions-datetime.html)
new DateTime(y, m, d, h, m, s) | [make_timestamp(y, m, d, h, m, s)](https://www.postgresql.org/docs/current/functions-datetime.html)

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

## Numeric functions

.NET                    | SQL
----------------------- | --------------------
Math.Abs(value)         | abs(value)
Math.Acos(d)            | acos(d)
Math.Asin(d)            | asin(d)
Math.Atan(d)            | atan(d)
Math.Atan2(y, x)        | atan2(y, x)
Math.Ceiling(d)         | ceiling(d)
Math.Cos(d)             | cos(d)
Math.Exp(d)             | exp(d)
Math.Floor(d)           | floor(d)
Math.Log(d)             | ln(d)
Math.Log10(d)           | log(d)
Math.Max(x, y)          | greatest(x, y)
Math.Min(x, y)          | least(x, y)
Math.Pow(x, y)          | power(x, y)
Math.Round(d)           | round(d)
Math.Round(d, decimals) | round(d, decimals)
Math.Sin(a)             | sin(a)
Math.Sign(value)        | sign(value)::int
Math.Sqrt(d)            | sqrt(d)
Math.Tan(a)             | tan(a)
Math.Truncate(d)        | trunc(d)

## String functions

.NET                                                          | SQL                                                    | Added in
------------------------------------------------------------- | ------------------------------------------------------ | --------
EF.Functions.Collate(operand, collation)                      | operand COLLATE collation                              | EF Core 5.0
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
stringValue.FirstOrDefault()                                  | substr(stringValue, 1, 1)                              | EF Core 5.0
stringValue.IndexOf(value)                                    | strpos(stringValue, value) - 1
stringValue.LastOrDefault()                                   | substr(stringValue, length(stringValue), 1)            | EF Core 5.0
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

## Miscellaneous functions

.NET                                     | SQL
---------------------------------------- | --------------------
collection.Contains(item)                | item IN collection
enumValue.HasFlag(flag)                  | enumValue & flag = flag
nullable.GetValueOrDefault()             | coalesce(nullable, 0)
nullable.GetValueOrDefault(defaultValue) | coalesce(nullable, defaultValue)
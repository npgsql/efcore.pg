Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_date_component() : 
            AssertSql(
                @"@__myDatetime_0='1998-05-04T00:00:00' (DbType = DateTime)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE DATE_TRUNC('day', ""o"".""OrderDate"") = @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsIgnoreCase() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?ip)' || '^a') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_minute_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('minute', ""o"".""OrderDate"")) AS int4) = 23");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsIgnorePatternWhitespace() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?px)' || '^ A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatch() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_StartsWith_Column() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE (""c"".""City"" || '%') AND (LEFT(""c"".""ContactName"", LENGTH(""c"".""City"")) = ""c"".""City"")");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.IsNullOrWhiteSpace_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""Region"" IS NULL OR (""c"".""Region"" ~ '^\s*$' = TRUE)");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_without_arguments_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*', '') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsSingleline() : line() na R:\RafaelNuvem\ProjetosGit\EFCorePostgreSQL\test\EFCore.PG.FunctionalTests\Query\SimpleQueryNpgsqlTest.Functions.cs:linha 238
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ '^A' = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_without_arguments_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '\s*$', '') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_StartsWith_Literal_with_escaping() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE '\_a\%b\\c%'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_not_found() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", '[') - 1) = -1");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_StartsWith_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE 'M%'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_Contains_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE STRPOS(""c"".""ContactName"", 'M') > 0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsMultiline() : line() na R:\RafaelNuvem\ProjetosGit\EFCorePostgreSQL\test\EFCore.PG.FunctionalTests\Query\SimpleQueryNpgsqlTest.Functions.cs:linha 229
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?n)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'Or') = 'wne'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'Ow') = 'ner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_now() : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() <> @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Query_expression_with_to_string_and_contains() : 
            AssertSql(
                @"SELECT ""o"".""CustomerID""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL AND (STRPOS(CAST(""o"".""EmployeeID"" AS text), '10') > 0)");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_EndsWith_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RIGHT(""c"".""ContactName"", LENGTH('b')) = 'b'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_without_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*(.*?)\s*$', '\1') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_month_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('month', ""o"".""OrderDate"")) AS int4) = 4");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'er') = 'Own'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_day_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('day', ""o"".""OrderDate"")) AS int4) = 4");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_utcnow() : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() AT TIME ZONE 'UTC' <> @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'r') = 'Owne'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_dayOfWeek_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('dow', ""o"".""OrderDate"")) AS int4) = 2");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsNone() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_hour_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('hour', ""o"".""OrderDate"")) AS int4) = 14");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_String() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", 'ar') - 1) > 5");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_Char() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", 'A') - 1) > 5");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_second_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('second', ""o"".""OrderDate"")) AS int4) = 44");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_dayOfYear_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('doy', ""o"".""OrderDate"")) AS int4) = 68");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_year_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('year', ""o"".""OrderDate"")) AS int4) = 1998");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_date_component() : 
            AssertSql(
                @"@__myDatetime_0='1998-05-04T00:00:00' (DbType = DateTime)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE DATE_TRUNC('day', ""o"".""OrderDate"") = @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsIgnoreCase() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?ip)' || '^a') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_minute_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('minute', ""o"".""OrderDate"")) AS int4) = 23");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsIgnorePatternWhitespace() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?px)' || '^ A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatch() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_StartsWith_Column() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE (""c"".""City"" || '%') AND (LEFT(""c"".""ContactName"", LENGTH(""c"".""City"")) = ""c"".""City"")");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.IsNullOrWhiteSpace_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""Region"" IS NULL OR (""c"".""Region"" ~ '^\s*$' = TRUE)");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_without_arguments_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*', '') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsSingleline() : line() na R:\RafaelNuvem\ProjetosGit\EFCorePostgreSQL\test\EFCore.PG.FunctionalTests\Query\SimpleQueryNpgsqlTest.Functions.cs:linha 238
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ '^A' = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_without_arguments_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '\s*$', '') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_StartsWith_Literal_with_escaping() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE '\_a\%b\\c%'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_not_found() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", '[') - 1) = -1");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_StartsWith_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE 'M%'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_Contains_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE STRPOS(""c"".""ContactName"", 'M') > 0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsMultiline() : line() na R:\RafaelNuvem\ProjetosGit\EFCorePostgreSQL\test\EFCore.PG.FunctionalTests\Query\SimpleQueryNpgsqlTest.Functions.cs:linha 229
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?n)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'Or') = 'wne'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'Ow') = 'ner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_now() : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() <> @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Query_expression_with_to_string_and_contains() : 
            AssertSql(
                @"SELECT ""o"".""CustomerID""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL AND (STRPOS(CAST(""o"".""EmployeeID"" AS text), '10') > 0)");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_EndsWith_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RIGHT(""c"".""ContactName"", LENGTH('b')) = 'b'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_without_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*(.*?)\s*$', '\1') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_month_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('month', ""o"".""OrderDate"")) AS int4) = 4");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'er') = 'Own'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_day_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('day', ""o"".""OrderDate"")) AS int4) = 4");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_utcnow() : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() AT TIME ZONE 'UTC' <> @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'r') = 'Owne'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_dayOfWeek_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('dow', ""o"".""OrderDate"")) AS int4) = 2");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsNone() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_hour_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('hour', ""o"".""OrderDate"")) AS int4) = 14");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_String() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", 'ar') - 1) > 5");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_Char() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", 'A') - 1) > 5");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_second_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('second', ""o"".""OrderDate"")) AS int4) = 44");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_dayOfYear_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('doy', ""o"".""OrderDate"")) AS int4) = 68");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_year_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('year', ""o"".""OrderDate"")) AS int4) = 1998");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_date_component() : 
            AssertSql(
                @"@__myDatetime_0='1998-05-04T00:00:00' (DbType = DateTime)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE DATE_TRUNC('day', ""o"".""OrderDate"") = @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsIgnoreCase() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?ip)' || '^a') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_minute_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('minute', ""o"".""OrderDate"")) AS int4) = 23");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsIgnorePatternWhitespace() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?px)' || '^ A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatch() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_StartsWith_Column() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE (""c"".""City"" || '%') AND (LEFT(""c"".""ContactName"", LENGTH(""c"".""City"")) = ""c"".""City"")");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.IsNullOrWhiteSpace_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""Region"" IS NULL OR (""c"".""Region"" ~ '^\s*$' = TRUE)");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_without_arguments_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*', '') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsSingleline() : line() na R:\RafaelNuvem\ProjetosGit\EFCorePostgreSQL\test\EFCore.PG.FunctionalTests\Query\SimpleQueryNpgsqlTest.Functions.cs:linha 238
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ '^A' = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_without_arguments_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '\s*$', '') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_StartsWith_Literal_with_escaping() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE '\_a\%b\\c%'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_not_found() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", '[') - 1) = -1");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_StartsWith_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE 'M%'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_Contains_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE STRPOS(""c"".""ContactName"", 'M') > 0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsMultiline() : line() na R:\RafaelNuvem\ProjetosGit\EFCorePostgreSQL\test\EFCore.PG.FunctionalTests\Query\SimpleQueryNpgsqlTest.Functions.cs:linha 229
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?n)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'Or') = 'wne'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'Ow') = 'ner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_now() : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() <> @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Query_expression_with_to_string_and_contains() : 
            AssertSql(
                @"SELECT ""o"".""CustomerID""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL AND (STRPOS(CAST(""o"".""EmployeeID"" AS text), '10') > 0)");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_EndsWith_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RIGHT(""c"".""ContactName"", LENGTH('b')) = 'b'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_without_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*(.*?)\s*$', '\1') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_month_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('month', ""o"".""OrderDate"")) AS int4) = 4");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'er') = 'Own'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_day_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('day', ""o"".""OrderDate"")) AS int4) = 4");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_utcnow() : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() AT TIME ZONE 'UTC' <> @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'r') = 'Owne'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_dayOfWeek_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('dow', ""o"".""OrderDate"")) AS int4) = 2");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsNone() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_hour_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('hour', ""o"".""OrderDate"")) AS int4) = 14");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_String() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", 'ar') - 1) > 5");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_Char() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", 'A') - 1) > 5");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_second_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('second', ""o"".""OrderDate"")) AS int4) = 44");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_dayOfYear_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('doy', ""o"".""OrderDate"")) AS int4) = 68");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_year_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('year', ""o"".""OrderDate"")) AS int4) = 1998");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_Contains_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE STRPOS(""c"".""ContactName"", 'M') > 0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_date_component() : 
            AssertSql(
                @"@__myDatetime_0='1998-05-04T00:00:00' (DbType = DateTime)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE DATE_TRUNC('day', ""o"".""OrderDate"") = @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsIgnoreCase() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?ip)' || '^a') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_minute_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('minute', ""o"".""OrderDate"")) AS int4) = 23");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsIgnorePatternWhitespace() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?px)' || '^ A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatch() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_StartsWith_Column() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE (""c"".""City"" || '%') AND (LEFT(""c"".""ContactName"", LENGTH(""c"".""City"")) = ""c"".""City"")");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.IsNullOrWhiteSpace_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""Region"" IS NULL OR (""c"".""Region"" ~ '^\s*$' = TRUE)");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_without_arguments_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*', '') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsSingleline() : line() na R:\RafaelNuvem\ProjetosGit\EFCorePostgreSQL\test\EFCore.PG.FunctionalTests\Query\SimpleQueryNpgsqlTest.Functions.cs:linha 238
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ '^A' = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_without_arguments_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '\s*$', '') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_StartsWith_Literal_with_escaping() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE '\_a\%b\\c%'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_not_found() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", '[') - 1) = -1");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_StartsWith_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE 'M%'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_Contains_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE STRPOS(""c"".""ContactName"", 'M') > 0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsMultiline() : line() na R:\RafaelNuvem\ProjetosGit\EFCorePostgreSQL\test\EFCore.PG.FunctionalTests\Query\SimpleQueryNpgsqlTest.Functions.cs:linha 229
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?n)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'Or') = 'wne'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'Ow') = 'ner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_now() : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() <> @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Query_expression_with_to_string_and_contains() : 
            AssertSql(
                @"SELECT ""o"".""CustomerID""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL AND (STRPOS(CAST(""o"".""EmployeeID"" AS text), '10') > 0)");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_EndsWith_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RIGHT(""c"".""ContactName"", LENGTH('b')) = 'b'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_without_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*(.*?)\s*$', '\1') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_month_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('month', ""o"".""OrderDate"")) AS int4) = 4");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'er') = 'Own'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_day_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('day', ""o"".""OrderDate"")) AS int4) = 4");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_utcnow() : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() AT TIME ZONE 'UTC' <> @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'r') = 'Owne'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_dayOfWeek_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('dow', ""o"".""OrderDate"")) AS int4) = 2");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsNone() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_hour_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('hour', ""o"".""OrderDate"")) AS int4) = 14");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_String() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", 'ar') - 1) > 5");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_Char() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", 'A') - 1) > 5");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_second_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('second', ""o"".""OrderDate"")) AS int4) = 44");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_dayOfYear_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('doy', ""o"".""OrderDate"")) AS int4) = 68");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_year_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('year', ""o"".""OrderDate"")) AS int4) = 1998");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_date_component() : 
            AssertSql(
                @"@__myDatetime_0='1998-05-04T00:00:00' (DbType = DateTime)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE DATE_TRUNC('day', ""o"".""OrderDate"") = @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsIgnoreCase() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?ip)' || '^a') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_minute_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('minute', ""o"".""OrderDate"")) AS int4) = 23");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsIgnorePatternWhitespace() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?px)' || '^ A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatch() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_StartsWith_Column() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE (""c"".""City"" || '%') AND (LEFT(""c"".""ContactName"", LENGTH(""c"".""City"")) = ""c"".""City"")");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.IsNullOrWhiteSpace_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""Region"" IS NULL OR (""c"".""Region"" ~ '^\s*$' = TRUE)");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_without_arguments_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*', '') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_without_arguments_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '\s*$', '') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_StartsWith_Literal_with_escaping() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE '\_a\%b\\c%'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_not_found() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", '[') - 1) = -1");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_Contains_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE STRPOS(""c"".""ContactName"", 'M') > 0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsMultiline() : line() na R:\RafaelNuvem\ProjetosGit\EFCorePostgreSQL\test\EFCore.PG.FunctionalTests\Query\SimpleQueryNpgsqlTest.Functions.cs:linha 229
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?n)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'Or') = 'wne'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'Ow') = 'ner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_now() : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() <> @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Query_expression_with_to_string_and_contains() : 
            AssertSql(
                @"SELECT ""o"".""CustomerID""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL AND (STRPOS(CAST(""o"".""EmployeeID"" AS text), '10') > 0)");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_EndsWith_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RIGHT(""c"".""ContactName"", LENGTH('b')) = 'b'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_without_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*(.*?)\s*$', '\1') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_month_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('month', ""o"".""OrderDate"")) AS int4) = 4");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'er') = 'Own'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_day_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('day', ""o"".""OrderDate"")) AS int4) = 4");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_utcnow() : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() AT TIME ZONE 'UTC' <> @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'r') = 'Owne'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_dayOfWeek_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('dow', ""o"".""OrderDate"")) AS int4) = 2");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsNone() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_hour_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('hour', ""o"".""OrderDate"")) AS int4) = 14");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_String() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", 'ar') - 1) > 5");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_Char() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", 'A') - 1) > 5");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_second_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('second', ""o"".""OrderDate"")) AS int4) = 44");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_dayOfYear_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('doy', ""o"".""OrderDate"")) AS int4) = 68");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_year_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('year', ""o"".""OrderDate"")) AS int4) = 1998");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_date_component() : 
            AssertSql(
                @"@__myDatetime_0='1998-05-04T00:00:00' (DbType = DateTime)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE DATE_TRUNC('day', ""o"".""OrderDate"") = @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsIgnoreCase() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?ip)' || '^a') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_minute_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('minute', ""o"".""OrderDate"")) AS int4) = 23");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsIgnorePatternWhitespace() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?px)' || '^ A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatch() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_StartsWith_Column() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE (""c"".""City"" || '%') AND (LEFT(""c"".""ContactName"", LENGTH(""c"".""City"")) = ""c"".""City"")");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.IsNullOrWhiteSpace_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""Region"" IS NULL OR (""c"".""Region"" ~ '^\s*$' = TRUE)");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_without_arguments_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*', '') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsSingleline() : line() na R:\RafaelNuvem\ProjetosGit\EFCorePostgreSQL\test\EFCore.PG.FunctionalTests\Query\SimpleQueryNpgsqlTest.Functions.cs:linha 238
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ '^A' = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_without_arguments_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '\s*$', '') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_StartsWith_Literal_with_escaping() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE '\_a\%b\\c%'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_not_found() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", '[') - 1) = -1");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_StartsWith_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE 'M%'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_Contains_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE STRPOS(""c"".""ContactName"", 'M') > 0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsMultiline() : line() na R:\RafaelNuvem\ProjetosGit\EFCorePostgreSQL\test\EFCore.PG.FunctionalTests\Query\SimpleQueryNpgsqlTest.Functions.cs:linha 229
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?n)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'Or') = 'wne'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'Ow') = 'ner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_now() : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() <> @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Query_expression_with_to_string_and_contains() : 
            AssertSql(
                @"SELECT ""o"".""CustomerID""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL AND (STRPOS(CAST(""o"".""EmployeeID"" AS text), '10') > 0)");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_EndsWith_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RIGHT(""c"".""ContactName"", LENGTH('b')) = 'b'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_without_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*(.*?)\s*$', '\1') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_month_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('month', ""o"".""OrderDate"")) AS int4) = 4");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'er') = 'Own'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_day_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('day', ""o"".""OrderDate"")) AS int4) = 4");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_utcnow() : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() AT TIME ZONE 'UTC' <> @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'r') = 'Owne'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_dayOfWeek_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('dow', ""o"".""OrderDate"")) AS int4) = 2");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsNone() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_hour_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('hour', ""o"".""OrderDate"")) AS int4) = 14");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_String() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", 'ar') - 1) > 5");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_Char() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", 'A') - 1) > 5");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_second_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('second', ""o"".""OrderDate"")) AS int4) = 44");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_dayOfYear_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('doy', ""o"".""OrderDate"")) AS int4) = 68");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_year_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('year', ""o"".""OrderDate"")) AS int4) = 1998");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_date_component() : 
            AssertSql(
                @"@__myDatetime_0='1998-05-04T00:00:00' (DbType = DateTime)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE DATE_TRUNC('day', ""o"".""OrderDate"") = @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsIgnoreCase() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?ip)' || '^a') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_minute_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('minute', ""o"".""OrderDate"")) AS int4) = 23");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsIgnorePatternWhitespace() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?px)' || '^ A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatch() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_StartsWith_Column() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE (""c"".""City"" || '%') AND (LEFT(""c"".""ContactName"", LENGTH(""c"".""City"")) = ""c"".""City"")");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.IsNullOrWhiteSpace_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""Region"" IS NULL OR (""c"".""Region"" ~ '^\s*$' = TRUE)");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_without_arguments_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*', '') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsSingleline() : line() na R:\RafaelNuvem\ProjetosGit\EFCorePostgreSQL\test\EFCore.PG.FunctionalTests\Query\SimpleQueryNpgsqlTest.Functions.cs:linha 238
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ '^A' = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_without_arguments_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '\s*$', '') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_StartsWith_Literal_with_escaping() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE '\_a\%b\\c%'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_not_found() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", '[') - 1) = -1");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_StartsWith_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE 'M%'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_Contains_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE STRPOS(""c"".""ContactName"", 'M') > 0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsMultiline() : line() na R:\RafaelNuvem\ProjetosGit\EFCorePostgreSQL\test\EFCore.PG.FunctionalTests\Query\SimpleQueryNpgsqlTest.Functions.cs:linha 229
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?n)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'Or') = 'wne'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'Ow') = 'ner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_now() : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() <> @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Query_expression_with_to_string_and_contains() : 
            AssertSql(
                @"SELECT ""o"".""CustomerID""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL AND (STRPOS(CAST(""o"".""EmployeeID"" AS text), '10') > 0)");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_EndsWith_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RIGHT(""c"".""ContactName"", LENGTH('b')) = 'b'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_without_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*(.*?)\s*$', '\1') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_month_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('month', ""o"".""OrderDate"")) AS int4) = 4");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'er') = 'Own'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_day_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('day', ""o"".""OrderDate"")) AS int4) = 4");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_utcnow() : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() AT TIME ZONE 'UTC' <> @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'r') = 'Owne'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_dayOfWeek_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('dow', ""o"".""OrderDate"")) AS int4) = 2");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsNone() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_hour_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('hour', ""o"".""OrderDate"")) AS int4) = 14");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_String() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", 'ar') - 1) > 5");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_Char() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", 'A') - 1) > 5");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_second_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('second', ""o"".""OrderDate"")) AS int4) = 44");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_dayOfYear_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('doy', ""o"".""OrderDate"")) AS int4) = 68");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_year_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('year', ""o"".""OrderDate"")) AS int4) = 1998");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_date_component() : 
            AssertSql(
                @"@__myDatetime_0='1998-05-04T00:00:00' (DbType = DateTime)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE DATE_TRUNC('day', ""o"".""OrderDate"") = @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsIgnoreCase() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?ip)' || '^a') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_minute_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('minute', ""o"".""OrderDate"")) AS int4) = 23");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsIgnorePatternWhitespace() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?px)' || '^ A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatch() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_StartsWith_Column() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE (""c"".""City"" || '%') AND (LEFT(""c"".""ContactName"", LENGTH(""c"".""City"")) = ""c"".""City"")");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.IsNullOrWhiteSpace_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""Region"" IS NULL OR (""c"".""Region"" ~ '^\s*$' = TRUE)");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_without_arguments_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*', '') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsSingleline() : line() na R:\RafaelNuvem\ProjetosGit\EFCorePostgreSQL\test\EFCore.PG.FunctionalTests\Query\SimpleQueryNpgsqlTest.Functions.cs:linha 238
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ '^A' = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_without_arguments_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '\s*$', '') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_StartsWith_Literal_with_escaping() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE '\_a\%b\\c%'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_not_found() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", '[') - 1) = -1");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_StartsWith_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE 'M%'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_Contains_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE STRPOS(""c"".""ContactName"", 'M') > 0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsMultiline() : line() na R:\RafaelNuvem\ProjetosGit\EFCorePostgreSQL\test\EFCore.PG.FunctionalTests\Query\SimpleQueryNpgsqlTest.Functions.cs:linha 229
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?n)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'Or') = 'wne'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimStart_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'Ow') = 'ner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_now() : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() <> @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Query_expression_with_to_string_and_contains() : 
            AssertSql(
                @"SELECT ""o"".""CustomerID""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL AND (STRPOS(CAST(""o"".""EmployeeID"" AS text), '10') > 0)");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_EndsWith_Literal() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RIGHT(""c"".""ContactName"", LENGTH('b')) = 'b'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Trim_without_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*(.*?)\s*$', '\1') = 'Owner'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_month_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('month', ""o"".""OrderDate"")) AS int4) = 4");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_with_char_array_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'er') = 'Own'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_day_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('day', ""o"".""OrderDate"")) AS int4) = 4");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_utcnow() : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() AT TIME ZONE 'UTC' <> @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.TrimEnd_with_char_argument_in_predicate() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'r') = 'Owne'");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_dayOfWeek_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('dow', ""o"".""OrderDate"")) AS int4) = 2");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Regex_IsMatchOptionsNone() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_hour_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('hour', ""o"".""OrderDate"")) AS int4) = 14");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_String() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", 'ar') - 1) > 5");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.String_IndexOf_Char() : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (STRPOS(""c"".""CompanyName"", 'A') - 1) > 5");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_second_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('second', ""o"".""OrderDate"")) AS int4) = 44");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_dayOfYear_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('doy', ""o"".""OrderDate"")) AS int4) = 68");



Microsoft.EntityFrameworkCore.Query.SimpleQueryNpgsqlTest.Where_datetime_year_component() : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('year', ""o"".""OrderDate"")) AS int4) = 1998");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"@__myDatetime_0='1998-05-04T00:00:00' (DbType = DateTime)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE DATE_TRUNC('day', ""o"".""OrderDate"") = @__myDatetime_0");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?ip)' || '^a') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('minute', ""o"".""OrderDate"")) AS int4) = 23");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?px)' || '^ A') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"@__myDatetime_0='1998-05-04T00:00:00' (DbType = DateTime)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE DATE_TRUNC('day', ""o"".""OrderDate"") = @__myDatetime_0");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?ip)' || '^a') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('minute', ""o"".""OrderDate"")) AS int4) = 23");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?px)' || '^ A') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE (""c"".""City"" || '%') AND (LEFT(""c"".""ContactName"", LENGTH(""c"".""City"")) = ""c"".""City"")");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""Region"" IS NULL OR (""c"".""Region"" ~ '^\s*$' = TRUE)");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*', '') = 'Owner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ '^A' = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '\s*$', '') = 'Owner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE '\_a\%b\\c%'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE 'M%'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE STRPOS(""c"".""ContactName"", 'M') > 0");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?n)' || '^A') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'Or') = 'wne'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'Ow') = 'ner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() <> @__myDatetime_0");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""CustomerID""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL AND (STRPOS(CAST(""o"".""EmployeeID"" AS text), '10') > 0)");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RIGHT(""c"".""ContactName"", LENGTH('b')) = 'b'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*(.*?)\s*$', '\1') = 'Owner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('month', ""o"".""OrderDate"")) AS int4) = 4");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'er') = 'Own'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('day', ""o"".""OrderDate"")) AS int4) = 4");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() AT TIME ZONE 'UTC' <> @__myDatetime_0");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'r') = 'Owne'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('dow', ""o"".""OrderDate"")) AS int4) = 2");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('hour', ""o"".""OrderDate"")) AS int4) = 14");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('second', ""o"".""OrderDate"")) AS int4) = 44");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('doy', ""o"".""OrderDate"")) AS int4) = 68");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('year', ""o"".""OrderDate"")) AS int4) = 1998");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"@__myDatetime_0='1998-05-04T00:00:00' (DbType = DateTime)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE DATE_TRUNC('day', ""o"".""OrderDate"") = @__myDatetime_0");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?ip)' || '^a') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('minute', ""o"".""OrderDate"")) AS int4) = 23");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?px)' || '^ A') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE (""c"".""City"" || '%') AND (LEFT(""c"".""ContactName"", LENGTH(""c"".""City"")) = ""c"".""City"")");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""Region"" IS NULL OR (""c"".""Region"" ~ '^\s*$' = TRUE)");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*', '') = 'Owner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ '^A' = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '\s*$', '') = 'Owner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE '\_a\%b\\c%'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE 'M%'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE STRPOS(""c"".""ContactName"", 'M') > 0");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?n)' || '^A') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'Or') = 'wne'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'Ow') = 'ner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() <> @__myDatetime_0");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""CustomerID""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL AND (STRPOS(CAST(""o"".""EmployeeID"" AS text), '10') > 0)");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RIGHT(""c"".""ContactName"", LENGTH('b')) = 'b'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*(.*?)\s*$', '\1') = 'Owner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('month', ""o"".""OrderDate"")) AS int4) = 4");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'er') = 'Own'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('day', ""o"".""OrderDate"")) AS int4) = 4");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() AT TIME ZONE 'UTC' <> @__myDatetime_0");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'r') = 'Owne'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('dow', ""o"".""OrderDate"")) AS int4) = 2");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('hour', ""o"".""OrderDate"")) AS int4) = 14");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('second', ""o"".""OrderDate"")) AS int4) = 44");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('doy', ""o"".""OrderDate"")) AS int4) = 68");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('year', ""o"".""OrderDate"")) AS int4) = 1998");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() <> @__myDatetime_0");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() AT TIME ZONE 'UTC' <> @__myDatetime_0");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"@__myDatetime_0='1998-05-04T00:00:00' (DbType = DateTime)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE DATE_TRUNC('day', ""o"".""OrderDate"") = @__myDatetime_0");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?ip)' || '^a') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('minute', ""o"".""OrderDate"")) AS int4) = 23");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?px)' || '^ A') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE (""c"".""City"" || '%') AND (LEFT(""c"".""ContactName"", LENGTH(""c"".""City"")) = ""c"".""City"")");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""Region"" IS NULL OR (""c"".""Region"" ~ '^\s*$' = TRUE)");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*', '') = 'Owner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ '^A' = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '\s*$', '') = 'Owner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE '\_a\%b\\c%'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE 'M%'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE STRPOS(""c"".""ContactName"", 'M') > 0");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?n)' || '^A') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'Or') = 'wne'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'Ow') = 'ner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() <> @__myDatetime_0");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""CustomerID""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL AND (STRPOS(CAST(""o"".""EmployeeID"" AS text), '10') > 0)");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RIGHT(""c"".""ContactName"", LENGTH('b')) = 'b'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*(.*?)\s*$', '\1') = 'Owner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('month', ""o"".""OrderDate"")) AS int4) = 4");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'er') = 'Own'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('day', ""o"".""OrderDate"")) AS int4) = 4");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() AT TIME ZONE 'UTC' <> @__myDatetime_0");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'r') = 'Owne'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('dow', ""o"".""OrderDate"")) AS int4) = 2");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('hour', ""o"".""OrderDate"")) AS int4) = 14");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('second', ""o"".""OrderDate"")) AS int4) = 44");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('doy', ""o"".""OrderDate"")) AS int4) = 68");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('year', ""o"".""OrderDate"")) AS int4) = 1998");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"@__myDatetime_0='1998-05-04T00:00:00' (DbType = DateTime)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE DATE_TRUNC('day', ""o"".""OrderDate"") = @__myDatetime_0");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?ip)' || '^a') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('minute', ""o"".""OrderDate"")) AS int4) = 23");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?px)' || '^ A') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE (""c"".""City"" || '%') AND (LEFT(""c"".""ContactName"", LENGTH(""c"".""City"")) = ""c"".""City"")");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""Region"" IS NULL OR (""c"".""Region"" ~ '^\s*$' = TRUE)");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*', '') = 'Owner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ '^A' = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '\s*$', '') = 'Owner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE '\_a\%b\\c%'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE 'M%'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE STRPOS(""c"".""ContactName"", 'M') > 0");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?n)' || '^A') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'Or') = 'wne'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'Ow') = 'ner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() <> @__myDatetime_0");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""CustomerID""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL AND (STRPOS(CAST(""o"".""EmployeeID"" AS text), '10') > 0)");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RIGHT(""c"".""ContactName"", LENGTH('b')) = 'b'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*(.*?)\s*$', '\1') = 'Owner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('month', ""o"".""OrderDate"")) AS int4) = 4");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'er') = 'Own'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('day', ""o"".""OrderDate"")) AS int4) = 4");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() AT TIME ZONE 'UTC' <> @__myDatetime_0");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'r') = 'Owne'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('dow', ""o"".""OrderDate"")) AS int4) = 2");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('hour', ""o"".""OrderDate"")) AS int4) = 14");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('second', ""o"".""OrderDate"")) AS int4) = 44");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('doy', ""o"".""OrderDate"")) AS int4) = 68");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('year', ""o"".""OrderDate"")) AS int4) = 1998");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"@__myDatetime_0='1998-05-04T00:00:00' (DbType = DateTime)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE DATE_TRUNC('day', ""o"".""OrderDate"") = @__myDatetime_0");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?ip)' || '^a') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('minute', ""o"".""OrderDate"")) AS int4) = 23");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?px)' || '^ A') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE (""c"".""City"" || '%') AND (LEFT(""c"".""ContactName"", LENGTH(""c"".""City"")) = ""c"".""City"")");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""Region"" IS NULL OR (""c"".""Region"" ~ '^\s*$' = TRUE)");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*', '') = 'Owner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ '^A' = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '\s*$', '') = 'Owner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE '\_a\%b\\c%'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE 'M%'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE STRPOS(""c"".""ContactName"", 'M') > 0");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?n)' || '^A') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'Or') = 'wne'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE BTRIM(""c"".""ContactTitle"", 'O') = 'wner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE LTRIM(""c"".""ContactTitle"", 'Ow') = 'ner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() <> @__myDatetime_0");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""CustomerID""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL AND (STRPOS(CAST(""o"".""EmployeeID"" AS text), '10') > 0)");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RIGHT(""c"".""ContactName"", LENGTH('b')) = 'b'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*(.*?)\s*$', '\1') = 'Owner'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('month', ""o"".""OrderDate"")) AS int4) = 4");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'er') = 'Own'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('day', ""o"".""OrderDate"")) AS int4) = 4");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() AT TIME ZONE 'UTC' <> @__myDatetime_0");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE RTRIM(""c"".""ContactTitle"", 'r') = 'Owne'");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('dow', ""o"".""OrderDate"")) AS int4) = 2");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CompanyName"" ~ ('(?p)' || '^A') = TRUE");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('hour', ""o"".""OrderDate"")) AS int4) = 14");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('second', ""o"".""OrderDate"")) AS int4) = 44");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('doy', ""o"".""OrderDate"")) AS int4) = 68");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE CAST(FLOOR(DATE_PART('year', ""o"".""OrderDate"")) AS int4) = 1998");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""Region"" IS NULL OR (""c"".""Region"" ~ '^\s*$' = TRUE)");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"SELECT ""o"".""CustomerID""
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL AND (STRPOS(CAST(""o"".""EmployeeID"" AS text), '10') > 0)");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor) : 
            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00' (DbType = DateTime)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE NOW() <> @__myDatetime_0");



Microsoft.EntityFrameworkCore.Query.AsyncGroupByQuerySqlServerTest.<GroupBy_Composite_Select_Average>d__1.MoveNext() : line 25
            AssertSql(
                @"SELECT AVG(CAST(""o"".""OrderID"" AS float8))
FROM ""Orders"" AS ""o""
GROUP BY ""o"".""CustomerID"", ""o"".""EmployeeID""");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor, Boolean wrapExceptions) : 
            AssertSql(
                @"SELECT (o.""OrderDate"" + MAKE_INTERVAL(secs => 1)) AS ""OrderDate""
FROM ""Orders"" AS o
WHERE o.""OrderDate"" IS NOT NULL");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor, Boolean wrapExceptions) : 
            AssertSql(
                @"SELECT (o.""OrderDate"" + MAKE_INTERVAL(years => 1)) AS ""OrderDate""
FROM ""Orders"" AS o
WHERE o.""OrderDate"" IS NOT NULL");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor, Boolean wrapExceptions) : 
            AssertSql(
                @"SELECT (o.""OrderDate"" + MAKE_INTERVAL(mins => 1)) AS ""OrderDate""
FROM ""Orders"" AS o
WHERE o.""OrderDate"" IS NOT NULL");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor, Boolean wrapExceptions) : 
            AssertSql(
                @"@__years_0='2'

SELECT (o.""OrderDate"" + MAKE_INTERVAL(years => @__years_0)) AS ""OrderDate""
FROM ""Orders"" AS o
WHERE o.""OrderDate"" IS NOT NULL");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor, Boolean wrapExceptions) : 
            AssertSql(
                @"SELECT (o.""OrderDate"" + MAKE_INTERVAL(months => 1)) AS ""OrderDate""
FROM ""Orders"" AS o
WHERE o.""OrderDate"" IS NOT NULL");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor, Boolean wrapExceptions) : 
            AssertSql(
                @"SELECT (o.""OrderDate"" + MAKE_INTERVAL(hours => 1)) AS ""OrderDate""
FROM ""Orders"" AS o
WHERE o.""OrderDate"" IS NOT NULL");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor, Boolean wrapExceptions) : 
            AssertSql(
                @"SELECT (to_tsquery('a & b') && to_tsquery('c & d'))
FROM ""Customers"" AS c
LIMIT 1");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor, Boolean wrapExceptions) : 
            AssertSql(
                @"SELECT (to_tsquery('a & b') || to_tsquery('c & d'))
FROM ""Customers"" AS c
LIMIT 1");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor, Boolean wrapExceptions) : 
            AssertSql(
                @"SELECT (to_tsvector('a') @@ 'b')
FROM ""Customers"" AS c
LIMIT 1");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor, Boolean wrapExceptions) : 
            AssertSql(
                @"SELECT (to_tsvector('a') @@ to_tsquery('b'))
FROM ""Customers"" AS c
LIMIT 1");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor, Boolean wrapExceptions) : 
            AssertSql(
                @"SELECT (to_tsquery('b') <@ to_tsquery('a & b'))
FROM ""Customers"" AS c
LIMIT 1");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor, Boolean wrapExceptions) : 
            AssertSql(
                @"SELECT (to_tsquery('a & b') @> to_tsquery('b'))
FROM ""Customers"" AS c
LIMIT 1");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor, Boolean wrapExceptions) : 
            AssertSql(
                @"SELECT (to_tsvector('b') || to_tsvector('c'))
FROM ""Customers"" AS c
LIMIT 1");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor, Boolean wrapExceptions) : 
            AssertSql(
                @"SELECT (to_tsquery('a & b') && to_tsquery('c & d'))
FROM ""Customers"" AS c
LIMIT 1");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor, Boolean wrapExceptions) : 
            AssertSql(
                @"SELECT (to_tsquery('a & b') || to_tsquery('c & d'))
FROM ""Customers"" AS c
LIMIT 1");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor, Boolean wrapExceptions) : 
            AssertSql(
                @"SELECT (to_tsvector('a') @@ 'b')
FROM ""Customers"" AS c
LIMIT 1");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor, Boolean wrapExceptions) : 
            AssertSql(
                @"SELECT (to_tsvector('a') @@ to_tsquery('b'))
FROM ""Customers"" AS c
LIMIT 1");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor, Boolean wrapExceptions) : 
            AssertSql(
                @"SELECT (to_tsquery('b') <@ to_tsquery('a & b'))
FROM ""Customers"" AS c
LIMIT 1");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor, Boolean wrapExceptions) : 
            AssertSql(
                @"SELECT (to_tsquery('a & b') @> to_tsquery('b'))
FROM ""Customers"" AS c
LIMIT 1");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor, Boolean wrapExceptions) : 
            AssertSql(
                @"SELECT (to_tsvector('b') || to_tsvector('c'))
FROM ""Customers"" AS c
LIMIT 1");



Npgsql.EntityFrameworkCore.PostgreSQL.Query.FullTextSearchDbFunctionsNpgsqlTest.TsQueryAnd() : 
            AssertSql(
                @"SELECT (to_tsquery('a & b') && to_tsquery('c & d'))
FROM ""Customers"" AS c
LIMIT 1");



Npgsql.EntityFrameworkCore.PostgreSQL.Query.FullTextSearchDbFunctionsNpgsqlTest.TsQueryOr() : 
            AssertSql(
                @"SELECT (to_tsquery('a & b') || to_tsquery('c & d'))
FROM ""Customers"" AS c
LIMIT 1");



Npgsql.EntityFrameworkCore.PostgreSQL.Query.FullTextSearchDbFunctionsNpgsqlTest.Matches_With_String() : 
            AssertSql(
                @"SELECT (to_tsvector('a') @@ 'b')
FROM ""Customers"" AS c
LIMIT 1");



Npgsql.EntityFrameworkCore.PostgreSQL.Query.FullTextSearchDbFunctionsNpgsqlTest.Matches_With_Tsquery() : 
            AssertSql(
                @"SELECT (to_tsvector('a') @@ to_tsquery('b'))
FROM ""Customers"" AS c
LIMIT 1");



Npgsql.EntityFrameworkCore.PostgreSQL.Query.FullTextSearchDbFunctionsNpgsqlTest.TsQueryIsContainedIn() : 
            AssertSql(
                @"SELECT (to_tsquery('b') <@ to_tsquery('a & b'))
FROM ""Customers"" AS c
LIMIT 1");



Npgsql.EntityFrameworkCore.PostgreSQL.Query.FullTextSearchDbFunctionsNpgsqlTest.TsQueryContains() : 
            AssertSql(
                @"SELECT (to_tsquery('a & b') @> to_tsquery('b'))
FROM ""Customers"" AS c
LIMIT 1");



Npgsql.EntityFrameworkCore.PostgreSQL.Query.FullTextSearchDbFunctionsNpgsqlTest.TsVectorConcat() : 
            AssertSql(
                @"SELECT (to_tsvector('b') || to_tsvector('c'))
FROM ""Customers"" AS c
LIMIT 1");



Npgsql.EntityFrameworkCore.PostgreSQL.Query.SpatialQueryNpgsqlGeographyTest.<IsWithinDistance>d__9.MoveNext() : line 84
            AssertSql(
                @"@__point_0='POINT (0 1)' (DbType = Object)

SELECT e.""Id"", ST_DWithin(e.""Point"", @__point_0, 1.0) AS ""IsWithinDistance""
FROM ""PointEntity"" AS e");



Npgsql.EntityFrameworkCore.PostgreSQL.Query.SpatialQueryNpgsqlGeographyTest.<IsWithinDistance>d__9.MoveNext() : line 84
            AssertSql(
                @"@__point_0='POINT (0 1)' (DbType = Object)

SELECT e.""Id"", ST_DWithin(e.""Point"", @__point_0, 1.0) AS ""IsWithinDistance""
FROM ""PointEntity"" AS e");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor, Boolean wrapExceptions) : 
            AssertSql(
                @"SELECT websearch_to_tsquery('a OR b')
FROM ""Customers"" AS c
LIMIT 1");



System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor, Boolean wrapExceptions) : 
AssertSql(
@"SELECT websearch_to_tsquery('english', 'a OR b')
FROM ""Customers"" AS c
LIMIT 1");



Npgsql.EntityFrameworkCore.PostgreSQL.Query.FullTextSearchDbFunctionsNpgsqlTest.WebSearchToTsQuery_With_Config_From_Variable() : 
            AssertSql(
                @"@__config_1='english'

SELECT websearch_to_tsquery(CAST(@__config_1 AS regconfig), 'a OR b')
FROM ""Customers"" AS c
LIMIT 1");



Npgsql.EntityFrameworkCore.PostgreSQL.Query.FullTextSearchDbFunctionsNpgsqlTest.WebSearchToTsQuery_With_Config() : 
            AssertSql(
                @"SELECT websearch_to_tsquery(CAST('english' AS regconfig), 'a OR b')
FROM ""Customers"" AS c
LIMIT 1");




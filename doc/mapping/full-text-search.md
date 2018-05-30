# Full Text Search

PostgreSQL has [built-in support for full-text search](https://www.postgresql.org/docs/current/static/textsearch.html), which allows you to conveniently and efficiently query natural language documents.

*TO BE COMPLETED*

<!--
You can now use the PostgreSQL Full Text Search functions and operators from LINQ queries (except for ```array_to_tsquery```, which is not possible to implement currently due to Entity Framework Core limitations). ```NpgsqlTsQuery``` and ```NpgsqlTsVector``` are now fully supported property types that will create ```tsquery``` and ```tsvector``` columns. Full text search functions are implemented as extensions on ```DbFunctions``` and both full text types. Raw SQL migrations are still needed to create and drop update triggers however.
-->


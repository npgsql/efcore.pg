# Array Type Mapping

PostgreSQL has the unique feature of supporting [*array data types*](https://www.postgresql.org/docs/current/static/arrays.html). This allow you to conveniently and efficiently store several values in a single column, where in other database you'd typically resort to concatenating the values in a string or defining another table with a one-to-many relationship.

> [!NOTE]
> Although PostgreSQL supports multidimensional arrays, these aren't yet supported by the EF Core provider.

## Mapping arrays

Simply define a regular .NET array or `List<>` property:

```c#
public class Post
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string[] Tags { get; set; }
    public List<string> AlternativeTags { get; set; }
}
```

The provider will create `text[]` columns for the above two properties, and will properly detect changes in them - if you load an array and change one of its elements, calling `SaveChanges` will automatically update the row in the database accordingly.

## Operation translation

The provider can also translate CLR array operations to the corresponding SQL operation; this allows you to efficiently work with arrays by evaluating operations in the database and avoids pulling all the data. The following table lists the range operations that currently get translated. If you run into a missing operation, please open an issue.

.NET                                          | SQL
--------------------------------------------- | ---
array[1]                                      | [array[1]](https://www.postgresql.org/docs/current/static/arrays.html#ARRAYS-ACCESSING)
array.Length                                  | [cardinality(array)](https://www.postgresql.org/docs/current/static/functions-array.html#ARRAY-FUNCTIONS-TABLE)
array1.SequenceEqual(array2)                  | [array1 = array2](https://www.postgresql.org/docs/current/static/arrays.html)
array1.Contains(element))                     | [element = ANY(array)](https://www.postgresql.org/docs/current/static/functions-comparisons.html#AEN21104)
array.Any()                                   | [cardinality(array) > 0](https://www.postgresql.org/docs/current/static/functions-array.html#ARRAY-FUNCTIONS-TABLE)
array1.Any(i => array2.Contains(i))           | [array1 && array2](https://www.postgresql.org/docs/current/functions-array.html#ARRAY-OPERATORS-TABLE)
array1.All(i => array2.Contains(i))           | [array1 <@ array2](https://www.postgresql.org/docs/current/functions-array.html#ARRAY-OPERATORS-TABLE)
array.Any(s => EF.Functions.Like(string, s))  | [string LIKE ANY (array)](https://www.postgresql.org/docs/current/functions-comparisons.html#id-1.5.8.30.16)
array.Any(s => EF.Functions.ILike(string, s)) | [string ILIKE ANY (array)](https://www.postgresql.org/docs/current/functions-comparisons.html#id-1.5.8.30.16)
array.All(s => EF.Functions.Like(string, s))  | [string LIKE ALL (array)](https://www.postgresql.org/docs/current/functions-comparisons.html#id-1.5.8.30.16)
array.All(s => EF.Functions.ILike(string, s)) | [string ILIKE ALL (array)](https://www.postgresql.org/docs/current/functions-comparisons.html#id-1.5.8.30.16)

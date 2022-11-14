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

The provider can also translate CLR array operations to the corresponding SQL operation; this allows you to efficiently work with arrays by evaluating operations in the database and avoids pulling all the data. The following table lists the range operations that currently get translated; all these translations work both for .NET arrays (`int[]`) and for generic Lists (`List<int>`). If you run into a missing operation, please open an issue.

.NET                                          | SQL                                                                                                                                      | Notes
--------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- | ----
array[1]                                      | [array[1]](https://www.postgresql.org/docs/current/static/arrays.html#ARRAYS-ACCESSING)                                                  |
array.Length / list.Count                     | [cardinality(array)](https://www.postgresql.org/docs/current/static/functions-array.html#ARRAY-FUNCTIONS-TABLE)                          |
array1 == array2                              | [array1 = array2](https://www.postgresql.org/docs/current/static/arrays.html)                                                            |
array1.SequenceEqual(array2)                  | [array1 = array2](https://www.postgresql.org/docs/current/static/arrays.html)                                                            |
arrayNonColumn.Contains(element))             | [element = ANY(arrayNonColumn)](https://www.postgresql.org/docs/current/static/functions-comparisons.html#AEN21104)                      | Can use regular index
arrayColumn.Contains(element)                 | [arrayColumn @> ARRAY\[element\]](https://www.postgresql.org/docs/current/functions-array.html#ARRAY-OPERATORS-TABLE)                    | Can use GIN index
array.Append(element)                         | [array_append(array, element)](https://www.postgresql.org/docs/current/functions-array.html#ARRAY-FUNCTIONS-TABLE)                       | Added in 6.0
array1.Concat(array2)                         | [array_cat(array1, array2)](https://www.postgresql.org/docs/current/functions-array.html#ARRAY-FUNCTIONS-TABLE)                          | Added in 6.0
array.IndexOf(element)                        | [array_position(array, element) - 1](https://www.postgresql.org/docs/current/functions-array.html#ARRAY-FUNCTIONS-TABLE)                 | Added in 6.0
array.IndexOf(element, startIndex)            | [array_position(array, element, startIndex + 1) - 1](https://www.postgresql.org/docs/current/functions-array.html#ARRAY-FUNCTIONS-TABLE) | Added in 6.0
String.Join(separator, array)                 | [array_to_string(array, separator, '')](https://www.postgresql.org/docs/current/functions-array.html#ARRAY-FUNCTIONS-TABLE)              | Added in 6.0
array.Any()                                   | [cardinality(array) > 0](https://www.postgresql.org/docs/current/static/functions-array.html#ARRAY-FUNCTIONS-TABLE)                      |
array1.Any(i => array2.Contains(i))           | [array1 && array2](https://www.postgresql.org/docs/current/functions-array.html#ARRAY-OPERATORS-TABLE)                                   |
array1.All(i => array2.Contains(i))           | [array1 <@ array2](https://www.postgresql.org/docs/current/functions-array.html#ARRAY-OPERATORS-TABLE)                                   |
array.Any(s => EF.Functions.Like(string, s))  | [string LIKE ANY (array)](https://www.postgresql.org/docs/current/functions-comparisons.html#id-1.5.8.30.16)                             |
array.Any(s => EF.Functions.ILike(string, s)) | [string ILIKE ANY (array)](https://www.postgresql.org/docs/current/functions-comparisons.html#id-1.5.8.30.16)                            |
array.All(s => EF.Functions.Like(string, s))  | [string LIKE ALL (array)](https://www.postgresql.org/docs/current/functions-comparisons.html#id-1.5.8.30.16)                             |
array.All(s => EF.Functions.ILike(string, s)) | [string ILIKE ALL (array)](https://www.postgresql.org/docs/current/functions-comparisons.html#id-1.5.8.30.16)                            |
EF.Functions.ArrayAgg(values)                 | [array_agg(values)](https://www.postgresql.org/docs/current/functions-aggregate.html#FUNCTIONS-AGGREGATE-TABLE)                          | Added in 7.0, See [Aggregate functions](translations.md#aggregate-functions).

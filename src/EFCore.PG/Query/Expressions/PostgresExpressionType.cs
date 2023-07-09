namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;

/// <summary>
/// PostgreSQL-specific expression node types.
/// </summary>
public enum PostgresExpressionType
{
    #region General operators

    /// <summary>
    ///     Represents a PostgreSQL contains operator.
    /// </summary>
    Contains,                     // >> (inet/cidr), @>
    
    /// <summary>
    ///     Represents a PostgreSQL contained-by operator.
    /// </summary>
    ContainedBy,                  // << (inet/cidr), <@

    /// <summary>
    ///     Represents a PostgreSQL overlap operator.
    /// </summary>
    Overlaps,                     // &&

    /// <summary>
    ///     Represents a PostgreSQL operator for finding the distance between two things (e.g. 2D distance between two geometries,
    ///     between timestamps...)
    /// </summary>
    Distance,                     // <->

    #endregion General operators

    #region Network

    /// <summary>
    ///     Represents a PostgreSQL network contained-by-or-equal operator.
    /// </summary>
    NetworkContainedByOrEqual,    // <<=
    
    /// <summary>
    ///     Represents a PostgreSQL network contains-or-equal operator.
    /// </summary>
    NetworkContainsOrEqual,       // >>=
    
    /// <summary>
    ///     Represents a PostgreSQL network contains-or-contained-by operator.
    /// </summary>
    NetworkContainsOrContainedBy, // &&

    #endregion Network

    #region Range

    /// <summary>
    ///     Represents a PostgreSQL operator for checking if a range is strictly to the left of another range.
    /// </summary>
    RangeIsStrictlyLeftOf,        // <<
    
    /// <summary>
    ///     Represents a PostgreSQL operator for checking if a range is strictly to the right of another range.
    /// </summary>
    RangeIsStrictlyRightOf,       // >>
    
    /// <summary>
    ///     Represents a PostgreSQL operator for checking if a range does not extend to the right of another range.
    /// </summary>
    RangeDoesNotExtendRightOf,    // &<

    /// <summary>
    ///     Represents a PostgreSQL operator for checking if a range does not extend to the left of another range.
    /// </summary>
    RangeDoesNotExtendLeftOf,     // &>

    /// <summary>
    ///     Represents a PostgreSQL operator for checking if a range is adjacent to another range.
    /// </summary>
    RangeIsAdjacentTo,            // -|-
    
    /// <summary>
    ///     Represents a PostgreSQL operator for performing a union between two ranges.
    /// </summary>
    RangeUnion,                   // +
    
    /// <summary>
    ///     Represents a PostgreSQL operator for performing an intersection between two ranges.
    /// </summary>
    RangeIntersect,               // *
    
    /// <summary>
    ///     Represents a PostgreSQL operator for performing an except operation between two ranges.
    /// </summary>
    RangeExcept,                  // -

    #endregion Range

    #region Text search

    /// <summary>
    ///     Represents a PostgreSQL operator for performing a full-text search match.
    /// </summary>
    TextSearchMatch,              // @@

    /// <summary>
    ///     Represents a PostgreSQL operator for logical AND within a full-text search match.
    /// </summary>
    TextSearchAnd,                // &&
    
    /// <summary>
    ///     Represents a PostgreSQL operator for logical OR within a full-text search match.
    /// </summary>
    TextSearchOr,                 // ||

    #endregion Text search

    #region JSON

    /// <summary>
    ///     Represents a PostgreSQL operator for checking whether a key exists in a JSON document.
    /// </summary>
    JsonExists,                   // ?

    /// <summary>
    ///     Represents a PostgreSQL operator for checking whether any of multiple keys exists in a JSON document.
    /// </summary>
    JsonExistsAny,                // ?@>
    
    /// <summary>
    ///     Represents a PostgreSQL operator for checking whether all the given keys exist in a JSON document.
    /// </summary>
    JsonExistsAll,                // ?<@

    #endregion JSON

    #region LTree
    
    /// <summary>
    ///     Represents a PostgreSQL operator for matching in an ltree type.
    /// </summary>
    LTreeMatches,                 // ~ or @

    /// <summary>
    ///     Represents a PostgreSQL operator for matching in an ltree type.
    /// </summary>
    LTreeMatchesAny,              // ?
    
    /// <summary>
    ///     Represents a PostgreSQL operator for finding the first ancestor in an ltree type.
    /// </summary>
    LTreeFirstAncestor,           // ?@>

    /// <summary>
    ///     Represents a PostgreSQL operator for finding the first descendent in an ltree type.
    /// </summary>
    LTreeFirstDescendent,         // ?<@
    
    /// <summary>
    ///     Represents a PostgreSQL operator for finding the first match in an ltree type.
    /// </summary>
    LTreeFirstMatches,            // ?~ or ?@

    #endregion LTree

    #region Cube

    /// <summary>
    ///     Represents a PostgreSQL operator for extracting the n-th coordinate of a cube (counting from 1).
    /// </summary>
    CubeNthCoordinate,            // ->

    /// <summary>
    ///     Represents a PostgreSQL operator for extracting the n-th coordinate of a cube (by n = 2 * k - 1).
    /// </summary>
    CubeNthCoordinate2,           // ~>

    /// <summary>
    ///     Represents a PostgreSQL operator for computing the taxicab (L-1 metric) distance between two cubes.
    /// </summary>
    CubeDistanceTaxicab,          // <#>

    /// <summary>
    ///     Represents a PostgreSQL operator for computing the Chebyshev (L-inf metric) distance between two cubes.
    /// </summary>
    CubeDistanceChebyshev,        // <=>

    #endregion
}

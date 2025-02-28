namespace Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

internal static class Statics
{
    internal static readonly bool[][] TrueArrays =
    [
        [],
        [true],
        [true, true],
        [true, true, true],
        [true, true, true, true],
        [true, true, true, true, true],
        [true, true, true, true, true, true],
        [true, true, true, true, true, true, true],
        [true, true, true, true, true, true, true, true]
    ];

    internal static readonly bool[][] FalseArrays = [
        [],
        [false],
        [false, false],
        [false, false, false]
    ];
}

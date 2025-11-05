using Microsoft.EntityFrameworkCore;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Types;

public class PgIndexesTest
{
    [Fact]
    public void Constructor_with_null_creates_empty()
    {
        var indexes = new PgIndexes(null);

        Assert.Equal(0, indexes.Count);
        Assert.Empty(indexes.GetZeroBasedIndexes());
    }

    [Fact]
    public void Constructor_with_empty_array()
    {
        var indexes = new PgIndexes([]);

        Assert.Equal(0, indexes.Count);
        Assert.Empty(indexes.GetZeroBasedIndexes());
    }

    [Fact]
    public void ToOneBased_converts_single_element()
    {
        var indexes = new PgIndexes([0]);

        Assert.Equal([1], indexes.ToOneBased());
    }

    [Fact]
    public void ToOneBased_converts_multiple_elements()
    {
        var indexes = new PgIndexes([0, 1, 2]);

        Assert.Equal([1, 2, 3], indexes.ToOneBased());
    }

    [Fact]
    public void ToOneBased_handles_negative_indices()
    {
        var indexes = new PgIndexes([-1, 0, 1]);

        Assert.Equal([0, 1, 2], indexes.ToOneBased());
    }

    [Fact]
    public void ToOneBased_handles_large_indices()
    {
        var indexes = new PgIndexes([99, 100, 1000]);

        Assert.Equal([100, 101, 1001], indexes.ToOneBased());
    }

    [Fact]
    public void ToOneBased_preserves_order()
    {
        var indexes = new PgIndexes([2, 1, 0]);

        Assert.Equal([3, 2, 1], indexes.ToOneBased());
    }

    [Fact]
    public void ToOneBased_handles_duplicates()
    {
        var indexes = new PgIndexes([0, 0, 1]);

        Assert.Equal([1, 1, 2], indexes.ToOneBased());
    }

    [Fact]
    public void Equals_same_values_returns_true()
    {
        var a = new PgIndexes([1, 2, 3]);
        var b = new PgIndexes([1, 2, 3]);

        Assert.True(a.Equals(b));
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void Equals_different_values_returns_false()
    {
        var a = new PgIndexes([1, 2, 3]);
        var b = new PgIndexes([1, 2, 4]);

        Assert.False(a.Equals(b));
        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void Equals_different_order_returns_false()
    {
        var a = new PgIndexes([1, 2, 3]);
        var b = new PgIndexes([3, 2, 1]);

        Assert.False(a.Equals(b));
    }

    [Fact]
    public void GetHashCode_same_values_same_hash()
    {
        var a = new PgIndexes([1, 2, 3]);
        var b = new PgIndexes([1, 2, 3]);

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void ToString_formats_correctly()
    {
        var indexes = new PgIndexes([0, 1, 2]);

        Assert.Equal("PgIndexes[0, 1, 2]", indexes.ToString());
    }

    [Fact]
    public void ToString_empty_array()
    {
        var indexes = new PgIndexes([]);

        Assert.Equal("PgIndexes[]", indexes.ToString());
    }
}

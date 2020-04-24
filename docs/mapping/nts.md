# Spatial Mapping with NetTopologySuite

> [!NOTE]
> It's recommended that you start by reading the general [Entity Framework Core docs on spatial support](https://docs.microsoft.com/ef/core/modeling/spatial).

PostgreSQL supports spatial data and operations via [the PostGIS extension](https://postgis.net/), which is a mature and feature-rich database spatial implementation. .NET doesn't provide a standard spatial library, but [NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite) is quite a good candidate. The Npgsql EF Core provider has a plugin which allows you to map NetTopologySuite's types PostGIS columns, and even translate many useful spatial operations to SQL. This is the recommended way to interact with spatial types in Npgsql.

Note that the EF Core NetTopologySuite plugin depends on [the Npgsql ADO.NET NetTopology plugin](http://www.npgsql.org/doc/types/nts.html), which provides NetTopologySuite support at the lower level. The EF Core plugin automatically arranged for the ADO.NET plugin to be set up.

## Setup

To set up the NetTopologySuite plugin, add the [Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite nuget](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite) to your project. Then, make the following modification to your `UseNpgsql()` line:

```c#
protected override void OnConfiguring(DbContextOptionsBuilder builder)
{
    builder.UseNpgsql("Host=localhost;Database=test;Username=npgsql_tests;Password=npgsql_tests",
        o => o.UseNetTopologySuite());
}
```

This will set up all the necessary mappings and operation translators. In addition, to make sure that the PostGIS extension is installed in your database, add the following to your DbContext:

```c#
protected override void OnModelCreating(ModelBuilder builder)
{
    builder.HasPostgresExtension("postgis");
}
```

At this point spatial support is set up. You can now use NetTopologySuite types as regular properties in your entities, and even perform some operations:

```c#
public class City
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Point Location { get; set; }
}

var nearbyCities = context.Cities.Where(c => c.Location.Distance(somePoint) < 100);
```

## Constraining your type names

With the code above, the provider will create a database column of type `geometry`. This is perfectly fine, but be aware that this type accepts any geometry type (point, polygon...), with any coordinate system (XY, XYZ...). It's good practice to constrain the column to the exact type of data you will be storing, but unfortunately the provider isn't aware of your required coordinate system and therefore can't do that for you. Consider explicitly specifying your column types on your properties as follows:

```c#
[Column(TypeName="geometry (point)")]
public Point Location { get; set; }
```

This will constrain your column to XY points only. The same can be done via the fluent API with `HasColumnType()`.

## Operation translation

The following table lists NetTopologySuite operations which are translated to PostGIS SQL operations. This allows you to use these NetTopologySuite methods and members efficiently - evaluation will happen on the server side. Since evaluation happens at the server, table data doesn't need to be transferred to the client (saving bandwidth), and in some cases indexes can be used to speed things up.

Note that the plugin is far from covering all spatial operations. If an operation you need is missing, please open an issue to request for it.

| This C# expression...                                    | ... gets translated to this SQL |
|----------------------------------------------------------|---------------------------------|
| .Where(c => c.Polygon.Area() > x)                        | [WHERE ST_Area(c."Polygon") > x](https://postgis.net/docs/manual-2.4/ST_Area.html)
| .Where(c => c.Polygon.AsText() = x                       | [WHERE ST_AsText(c."Polygon") = x](https://postgis.net/docs/manual-2.4/ST_AsText.html)
| .Where(c => c.Polygon.Boundary = x                       | [WHERE ST_AsCoundary(c."Polygon") = x](https://postgis.net/docs/manual-2.4/ST_Boundary.html)
| .Where(c => c.Polygon.Contains(x))                       | [WHERE ST_Contains(c."Polygon", x)](https://postgis.net/docs/manual-2.4/ST_Contains.html)
| .Where(c => c.Polygon.Covers(x))                         | [WHERE ST_Covers(c."Polygon", x)](https://postgis.net/docs/manual-2.4/ST_Covers.html)
| .Where(c => c.Polygon.CoveredBy(x))                      | [WHERE ST_CoveredBy(c."Polygon", x)](https://postgis.net/docs/manual-2.4/ST_CoveredBy.html)
| .Where(c => c.Polygon.Crosses(x))                        | [WHERE ST_Crosses(c."Polygon", x)](https://postgis.net/docs/manual-2.4/ST_Crosses.html)
| .Where(c => c.Polygon.Difference(x) = y)                 | [WHERE ST_Difference(c."Polygon", x) = y](https://postgis.net/docs/manual-2.4/ST_Difference.html)
| .Where(c => c.Polygon.Disjoint(x))                       | [WHERE ST_Disjoint(c."Polygon", x)](https://postgis.net/docs/manual-2.4/ST_Disjoint.html)
| .Where(c => c.Point.Distance(x) > y)                     | [WHERE ST_Distance(c."Polygon", x) > y](https://postgis.net/docs/manual-2.4/ST_Distance.html)
| .Where(c => c.Polygon.Equals(x))                         | [WHERE c."Polygon" = x](https://postgis.net/docs/manual-2.4/ST_Geometry_EQ.html)
| .Where(c => c.Polygon.EqualsExact(x))                    | [WHERE c."Polygon" = x](https://postgis.net/docs/manual-2.4/ST_Geometry_EQ.html)
| .Where(c => c.Polygon.EqualsTopologically(x))            | [WHERE ST_Equals(c."Polygon", x)](https://postgis.net/docs/manual-2.4/ST_Equals.html)
| .Where(c => c.Geometry.GeometryType() = x)               | [WHERE GeometryType(c."GeomCollection") = x](https://postgis.net/docs/manual-2.4/GeometryType.html)
| .Where(c => c.GeomCollection.GetGeometryN(2) = x)        | [WHERE ST_GeometryN(c."GeomCollection", 3) = x](https://postgis.net/docs/manual-2.4/ST_GeometryN.html)
| .Where(c => c.Polygon.Intersection(x) = y)               | [WHERE ST_Intersection(c."Polygon", x) = y](https://postgis.net/docs/manual-2.4/ST_Intersection.html)
| .Where(c => c.Polygon.Intersects(x))                     | [WHERE ST_Intersects(c."Polygon", x)](https://postgis.net/docs/manual-2.4/ST_Intersects.html)
| .Where(c => c.LineString.IsClosed())                     | [WHERE ST_IsClosed(c."LineString")](https://postgis.net/docs/manual-2.4/ST_IsClosed.html)
| .Where(c => c.GeomCollection.IsEmpty())                  | [WHERE ST_IsEmpty(c."GeomCollection")](https://postgis.net/docs/manual-2.4/ST_IsEmpty.html)
| .Where(c => c.Polygon.IsSimple())                        | [WHERE ST_IsSimple(c."Polygon")](https://postgis.net/docs/manual-2.4/ST_IsSimple.html)
| .Where(c => c.Polygon.IsValid())                         | [WHERE ST_IsValid(c."Polygon")](https://postgis.net/docs/manual-2.4/ST_IsValid.html)
| .Where(c => c.LineString.Length > x)                     | [WHERE ST_Length(c."LineString") > x](https://postgis.net/docs/manual-2.4/ST_Length.html)
| .Where(c => c.GeomCollection.NumGeometries > x)          | [WHERE ST_NumGeometries(c."GeomCollection") > x](https://postgis.net/docs/manual-2.4/ST_NumGeometries.html)
| .Where(c => c.LineString.NumPoints > x)                  | [WHERE ST_NumPoints(c."LineString") > x](https://postgis.net/docs/manual-2.4/ST_NumPoints.html)
| .Where(c => c.Polygon.Overlaps(x))                       | [WHERE ST_Overlaps(c."Polygon", x)](https://postgis.net/docs/manual-2.4/ST_Overlaps.html)
| .Where(c => c.Polygon.Relate(x) == y)                    | [WHERE ST_Relate(c."Polygon", x) = y](https://postgis.net/docs/manual-2.4/ST_Relate.html)
| .Where(c => c.LineString.Reverse() == x)                 | [WHERE ST_Reverse(c."Polygon") = x](https://postgis.net/docs/manual-2.4/ST_Reverse.html)
| .Where(c => c.Polygon.SymmetricDifference(x) == y)       | [WHERE ST_SymDifference(c."Polygon", x) = y](https://postgis.net/docs/manual-2.4/ST_SymDifference.html)
| .Where(c => c.Polygon.Touches(x))                        | [WHERE ST_Touches(c."Polygon", x)](https://postgis.net/docs/manual-2.4/ST_Touches.html)
| .Where(c => c.Polygon.ToText() = x)                      | [WHERE ST_AsText(c."Polygon") = x](https://postgis.net/docs/manual-2.4/ST_AsText.html)
| .Where(c => c.Polygon.Union(x) = y)                      | [WHERE ST_Union(c."Polygon", x) = y](https://postgis.net/docs/manual-2.4/ST_Union.html)
| .Where(c => c.Polygon.Within(x))                         | [WHERE ST_Within(c."Polygon", x)](https://postgis.net/docs/manual-2.4/ST_Within.html)
| .Where(c => c.Point.X == 3)                              | [WHERE ST_X(c."Point") = 3](https://postgis.net/docs/manual-2.4/ST_X.html)
| .Where(c => c.Point.Y == 3)                              | [WHERE ST_Y(c."Point") = 3](https://postgis.net/docs/manual-2.4/ST_Y.html)
| .Where(c => c.Point.Z == 3)                              | [WHERE ST_Z(c."Point") = 3](https://postgis.net/docs/manual-2.4/ST_Z.html)

## Geography (geodetic) support

PostGIS has two types: `geometry` (for Cartesian coordinates) and `geography` (for geodetic or spherical coordinates). You can read about the geometry/geography distinction [in the PostGIS docs](https://postgis.net/docs/manual-2.4/using_postgis_dbmanagement.html#PostGIS_Geography) or in [this blog post](http://workshops.boundlessgeo.com/postgis-intro/geography.html). In a nutshell, `geography` is much more accurate when doing calculations over long distances, but is more expensive computationally and supports only a small subset of the spatial operations supported by `geometry`.

The Npgsql provider will be default map all NetTopologySuite types to PostGIS `geometry`. However, you can instruct it to map certain properties to `geography` instead:

```c#
protected override void OnModelCreating(ModelBuilder builder)
{
    builder.Entity<City>().Property(b => b.Location).HasColumnType("geography (point)");
}
```

or via an attribute:

```c#
public class City
{
    public int Id { get; set; }
    public string Name { get; set; }
    [Column(TypeName="geography")]
    public Point Location { get; set; }
}
```

Once you do this, your column will be created as `geography`, and spatial operations will behave as expected.

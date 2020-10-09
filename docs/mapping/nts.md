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

## Operation translation

The following table lists NetTopologySuite operations which are translated to PostGIS SQL operations. This allows you to use these NetTopologySuite methods and members efficiently - evaluation will happen on the server side. Since evaluation happens at the server, table data doesn't need to be transferred to the client (saving bandwidth), and in some cases indexes can be used to speed things up.

Note that the plugin is far from covering all spatial operations. If an operation you need is missing, please open an issue to request for it.

C#                               | .NET
---------------------------------|-----
geom.Area()                      | [ST_Area(geom)](https://postgis.net/docs/manual-3.0/ST_Area.html)
geom.AsText()                    | [ST_AsText(geom)](https://postgis.net/docs/manual-3.0/ST_AsText.html)
geom.Boundary                    | [ST_Boundary(geom)](https://postgis.net/docs/manual-3.0/ST_Boundary.html)
geom1.Contains(geom2))           | [ST_Contains(geom1, geom2)](https://postgis.net/docs/manual-3.0/ST_Contains.html)
geom1.Covers(geom2))             | [ST_Covers(geom1, geom2)](https://postgis.net/docs/manual-3.0/ST_Covers.html)
geom1.CoveredBy(geom2))          | [ST_CoveredBy(geom1, geom2)](https://postgis.net/docs/manual-3.0/ST_CoveredBy.html)
geom1.Crosses(geom2)             | [ST_Crosses(geom1, geom2)](https://postgis.net/docs/manual-3.0/ST_Crosses.html)
geom1.Difference(geom2)          | [ST_Difference(geom1, geom2)](https://postgis.net/docs/manual-3.0/ST_Difference.html)
geom1.Disjoint(geom2))           | [ST_Disjoint(geom1, geom2)](https://postgis.net/docs/manual-3.0/ST_Disjoint.html)
geom1.Distance(geom2)            | [ST_Distance(geom1, geom2)](https://postgis.net/docs/manual-3.0/ST_Distance.html)
geom1.Equals(geom2)              | [geom1 = geom2](https://postgis.net/docs/manual-3.0/ST_Geometry_EQ.html)
geom1.Polygon.EqualsExact(geom2) | [geom1 = geom2](https://postgis.net/docs/manual-3.0/ST_Geometry_EQ.html)
geom1.EqualsTopologically(geom2) | [ST_Equals(geom1, geom2)](https://postgis.net/docs/manual-3.0/ST_Equals.html)
geom.GeometryType()              | [GeometryType(geom)](https://postgis.net/docs/manual-3.0/GeometryType.html)
geomCollection.GetGeometryN(i)   | [ST_GeometryN(geomCollection, i)](https://postgis.net/docs/manual-3.0/ST_GeometryN.html)
geom1.Intersection(geom2)        | [ST_Intersection(geom1, geom2)](https://postgis.net/docs/manual-3.0/ST_Intersection.html)
geom2.Intersects(geom2)          | [ST_Intersects(geom1, geom2)](https://postgis.net/docs/manual-3.0/ST_Intersects.html)
lineString.IsClosed()            | [ST_IsClosed(lineString)](https://postgis.net/docs/manual-3.0/ST_IsClosed.html)
geomCollection.IsEmpty()         | [ST_IsEmpty(geomCollection)](https://postgis.net/docs/manual-3.0/ST_IsEmpty.html)
geom.IsSimple()                  | [ST_IsSimple(geom)](https://postgis.net/docs/manual-3.0/ST_IsSimple.html)
geom.IsValid()                   | [ST_IsValid(geom)](https://postgis.net/docs/manual-3.0/ST_IsValid.html)
lineString.Length                | [ST_Length(lineString)](https://postgis.net/docs/manual-3.0/ST_Length.html)
geomCollection.NumGeometries     | [ST_NumGeometries(geomCollection)](https://postgis.net/docs/manual-3.0/ST_NumGeometries.html)
lineString.NumPoints             | [ST_NumPoints(lineString)](https://postgis.net/docs/manual-3.0/ST_NumPoints.html)
geom1.Overlaps(geom2))           | [ST_Overlaps(geom1, geom2)](https://postgis.net/docs/manual-3.0/ST_Overlaps.html)
geom1.Relate(geom2)              | [ST_Relate(geom1, geom2)](https://postgis.net/docs/manual-3.0/ST_Relate.html)
geom.Reverse()                   | [ST_Reverse(geom)](https://postgis.net/docs/manual-3.0/ST_Reverse.html)
geom1.SymmetricDifference(geom2) | [ST_SymDifference(geom1, geom2)](https://postgis.net/docs/manual-3.0/ST_SymDifference.html)
geom1.Touches(geom2))            | [ST_Touches(geom1, geom2)](https://postgis.net/docs/manual-3.0/ST_Touches.html)
geom.ToText()                    | [ST_AsText(geom)](https://postgis.net/docs/manual-3.0/ST_AsText.html)
geom1.Union(geom2)               | [ST_Union(geom1, geom2)](https://postgis.net/docs/manual-3.0/ST_Union.html)
geom1.Within(geom2)              | [ST_Within(geom1, geom2)](https://postgis.net/docs/manual-3.0/ST_Within.html)
point.X                          | [ST_X(point)](https://postgis.net/docs/manual-3.0/ST_X.html)
point.Y                          | [ST_Y(point)](https://postgis.net/docs/manual-3.0/ST_Y.html)
point.Z                          | [ST_Z(point)](https://postgis.net/docs/manual-3.0/ST_Z.html)

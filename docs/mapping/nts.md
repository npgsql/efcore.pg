# Spatial Mapping with NetTopologySuite

> [!NOTE]
> It's recommended that you start by reading the general [Entity Framework Core docs on spatial support](https://docs.microsoft.com/ef/core/modeling/spatial).

PostgreSQL supports spatial data and operations via [the PostGIS extension](https://postgis.net/), which is a mature and feature-rich database spatial implementation. .NET doesn't provide a standard spatial library, but [NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite) is a leading spatial library. The Npgsql EF Core provider has a plugin which allows you to map the NTS types to PostGIS columns, allowing seamless reading and writing. This is the recommended way to interact with spatial types in Npgsql.

Note that the EF Core NetTopologySuite plugin depends on [the Npgsql ADO.NET NetTopology plugin](http://www.npgsql.org/doc/types/nts.html), which provides NetTopologySuite support at the lower level.

## Setup

To use the NetTopologySuite plugin, add the [Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite nuget](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite) to your project. Then, configure the NetTopologySuite plugin as follows:

### [EF 9.0, with a connection string](#tab/ef9-with-connection-string)

If you're passing a connection string to `UseNpgsql`, simply add the `UseNetTopologySuite` call as follows:

```csharp
builder.Services.AddDbContext<MyContext>(options => options.UseNpgsql(
    "<connection string>",
    o => o.UseNetTopologySuite()));
```

This configures all aspects of Npgsql to use the NetTopologySuite plugin - both at the EF and the lower-level Npgsql layer.

### [With an external NpgsqlDataSource](#tab/with-datasource)

If you're creating an external NpgsqlDataSource and passing it to `UseNpgsql`, you must call `UseNetTopologySuite` on your NpgsqlDataSourceBuilder independently of the EF-level setup:

```csharp
var dataSourceBuilder = new NpgsqlDataSourceBuilder("<connection string>");
dataSourceBuilder.UseNetTopologySuite();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<MyContext>(options => options.UseNpgsql(
    dataSource,
    o => o.UseNetTopologySuite()));
```

### [Older EF versions, with a connection string](#tab/legacy-with-connection-string)

```csharp
// Configure NetTopologySuite at the ADO.NET level.
// This code must be placed at the beginning of your application, before any other Npgsql API is called; an appropriate place for this is in the static constructor on your DbContext class:
static MyDbContext()
    => NpgsqlConnection.GlobalTypeMapper.UseNetTopologySuite();

// Then, when configuring EF Core with UseNpgsql(), call UseNetTopologySuite():
builder.Services.AddDbContext<MyContext>(options =>
    options.UseNpgsql("<connection string>", o => o.UseNetTopologySuite()));
```

***

The above sets up all the necessary EF mappings and operation translators. If you're using EF 6.0, you also need to make sure that the PostGIS extension is installed in your database (later versions do this automatically). Add the following to your DbContext:

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    builder.HasPostgresExtension("postgis");
}
```

At this point spatial support is set up. You can now use NetTopologySuite types as regular properties in your entities, and even perform some operations:

```csharp
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

```csharp
[Column(TypeName="geometry (point)")]
public Point Location { get; set; }
```

This will constrain your column to XY points only. The same can be done via the fluent API with `HasColumnType()`.

## Geography (geodetic) support

PostGIS has two types: `geometry` (for Cartesian coordinates) and `geography` (for geodetic or spherical coordinates). You can read about the geometry/geography distinction [in the PostGIS docs](https://postgis.net/docs/manual-2.4/using_postgis_dbmanagement.html#PostGIS_Geography) or in [this blog post](http://workshops.boundlessgeo.com/postgis-intro/geography.html). In a nutshell, `geography` is much more accurate when doing calculations over long distances, but is more expensive computationally and supports only a small subset of the spatial operations supported by `geometry`.

The Npgsql provider will be default map all NetTopologySuite types to PostGIS `geometry`. However, you can instruct it to map certain properties to `geography` instead:

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    builder.Entity<City>().Property(b => b.Location).HasColumnType("geography (point)");
}
```

or via an attribute:

```csharp
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

.NET                                                             | SQL                                                                                  | Notes
---------------------------------------------------------------- |------------------------------------------------------------------------------------- | -----
geom.Area()                                                      | [ST_Area(geom)](https://postgis.net/docs/ST_Area.html)
geom.AsBinary()                                                  | [ST_AsBinary(geom)](https://postgis.net/docs/ST_AsBinary.html)
geom.AsText()                                                    | [ST_AsText(geom)](https://postgis.net/docs/ST_AsText.html)
geom.Boundary                                                    | [ST_Boundary(geom)](https://postgis.net/docs/ST_Boundary.html)
geom.Buffer(d)                                                   | [ST_Buffer(geom,d)](https://postgis.net/docs/ST_Buffer.html)
geom.Centroid                                                    | [ST_Centroid(geom)](https://postgis.net/docs/ST_Centroid.html)
geom1.Contains(geom2)                                            | [ST_Contains(geom1, geom2)](https://postgis.net/docs/ST_Contains.html)
geomCollection.Count                                             | [ST_NumGeometries(geom1)](https://postgis.net/docs/ST_NumGeometries.html)
linestring.Count                                                 | [ST_NumPoints(linestring)](https://postgis.net/docs/ST_NumPoints.html)
geom1.ConvexHull()                                               | [ST_ConvexHull(geom1)](https://postgis.net/docs/ST_ConvexHull.html)
geom1.Covers(geom2)                                              | [ST_Covers(geom1, geom2)](https://postgis.net/docs/ST_Covers.html)
geom1.CoveredBy(geom2)                                           | [ST_CoveredBy(geom1, geom2)](https://postgis.net/docs/ST_CoveredBy.html)
geom1.Crosses(geom2)                                             | [ST_Crosses(geom1, geom2)](https://postgis.net/docs/ST_Crosses.html)
geom1.Difference(geom2)                                          | [ST_Difference(geom1, geom2)](https://postgis.net/docs/ST_Difference.html)
geom1.Dimension                                                  | [ST_Dimension(geom1)](https://postgis.net/docs/ST_Dimension.html)
geom1.Disjoint(geom2)                                            | [ST_Disjoint(geom1, geom2)](https://postgis.net/docs/ST_Disjoint.html)
geom1.Distance(geom2)                                            | [ST_Distance(geom1, geom2)](https://postgis.net/docs/ST_Distance.html)
EF.Functions.DistanceKnn(geom1, geom2)                           | [geom1 <-> geom2](https://postgis.net/docs/geometry_distance_knn.html)
EF.Functions.Distance(geom1, geom2, useSpheriod)                 | [ST_Distance(geom1, geom2, useSpheriod)](https://postgis.net/docs/ST_Distance.html)
geom1.Envelope                                                   | [ST_Envelope(geom1)](https://postgis.net/docs/ST_Envelope.html)
geom1.ExactEquals(geom2)                                         | [ST_OrderingEquals(geom1, geom2)](https://postgis.net/docs/ST_OrderingEquals.html)
lineString.EndPoint                                              | [ST_EndPoint(lineString)](https://postgis.net/docs/ST_EndPoint.html)
polygon.ExteriorRing                                             | [ST_ExteriorRing(polygon)](https://postgis.net/docs/ST_ExteriorRing.html)
geom1.Equals(geom2)                                              | [geom1 = geom2](https://postgis.net/docs/ST_Geometry_EQ.html)
geom1.Polygon.EqualsExact(geom2)                                 | [geom1 = geom2](https://postgis.net/docs/ST_Geometry_EQ.html)
geom1.EqualsTopologically(geom2)                                 | [ST_Equals(geom1, geom2)](https://postgis.net/docs/ST_Equals.html)
EF.Functions.Force2D                                             | [ST_Force2D(geom)](https://postgis.net/docs/ST_Force2D.html)
geom.GeometryType                                                | [GeometryType(geom)](https://postgis.net/docs/GeometryType.html)
geomCollection.GetGeometryN(i)                                   | [ST_GeometryN(geomCollection, i)](https://postgis.net/docs/ST_GeometryN.html)
linestring.GetPointN(i)                                          | [ST_PointN(linestring, i)](https://postgis.net/docs/ST_PointN.html)
geom1.Intersection(geom2)                                        | [ST_Intersection(geom1, geom2)](https://postgis.net/docs/ST_Intersection.html)
geom1.Intersects(geom2)                                          | [ST_Intersects(geom1, geom2)](https://postgis.net/docs/ST_Intersects.html)
EF.Functions.IntersectsBbox(geom1, geom2)                        | [geom1 && geom2](https://postgis.net/docs/geometry_overlaps.html)                    | Added in 11.0
geom.InteriorPoint                                               | [ST_PointOnSurface(geom)](https://postgis.net/docs/ST_PointOnSurface.html)
lineString.IsClosed()                                            | [ST_IsClosed(lineString)](https://postgis.net/docs/ST_IsClosed.html)
geomCollection.IsEmpty()                                         | [ST_IsEmpty(geomCollection)](https://postgis.net/docs/ST_IsEmpty.html)
linestring.IsRing                                                | [ST_IsRing(linestring)](https://postgis.net/docs/ST_IsRing.html)
geom.IsWithinDistance(geom2,d)                                   | [ST_DWithin(geom1, geom2, d)](https://postgis.net/docs/ST_DWithin.html)
EF.Functions.IsWithinDistance(geom1, geom2, d, useSpheriod)      | [ST_DWithin(geom1, geom2, d, useSpheriod)](https://postgis.net/docs/ST_DWithin.html)
geom.IsSimple()                                                  | [ST_IsSimple(geom)](https://postgis.net/docs/ST_IsSimple.html)
geom.IsValid()                                                   | [ST_IsValid(geom)](https://postgis.net/docs/ST_IsValid.html)
lineString.Length                                                | [ST_Length(lineString)](https://postgis.net/docs/ST_Length.html)
geom.Normalized                                                  | [ST_Normalize(geom)](https://postgis.net/docs/ST_Normalize.html)
geomCollection.NumGeometries                                     | [ST_NumGeometries(geomCollection)](https://postgis.net/docs/ST_NumGeometries.html)
polygon.NumInteriorRings                                         | [ST_NumInteriorRings(polygon)](https://postgis.net/docs/ST_NumInteriorRings.html)
lineString.NumPoints                                             | [ST_NumPoints(lineString)](https://postgis.net/docs/ST_NumPoints.html)
geom1.Overlaps(geom2)                                            | [ST_Overlaps(geom1, geom2)](https://postgis.net/docs/ST_Overlaps.html)
geom.PointOnSurface                                              | [ST_PointOnSurface(geom)](https://postgis.net/docs/ST_PointOnSurface.html)
geom1.Relate(geom2)                                              | [ST_Relate(geom1, geom2)](https://postgis.net/docs/ST_Relate.html)
geom.Reverse()                                                   | [ST_Reverse(geom)](https://postgis.net/docs/ST_Reverse.html)
geom1.SRID                                                       | [ST_SRID(geom1)](https://postgis.net/docs/ST_SRID.html)
lineString.StartPoint                                            | [ST_StartPoint(lineString)](https://postgis.net/docs/ST_StartPoint.html)
geom1.SymmetricDifference(geom2)                                 | [ST_SymDifference(geom1, geom2)](https://postgis.net/docs/ST_SymDifference.html)
geom.ToBinary()                                                  | [ST_AsBinary(geom)](https://postgis.net/docs/ST_AsBinary.html)
geom.ToText()                                                    | [ST_AsText(geom)](https://postgis.net/docs/ST_AsText.html)
geom1.Touches(geom2)                                             | [ST_Touches(geom1, geom2)](https://postgis.net/docs/ST_Touches.html)
EF.Functions.Transform(geom, srid)                               | [ST_Transform(geom, srid)](https://postgis.net/docs/ST_Transform.html)
geom1.Union(geom2)                                               | [ST_Union(geom1, geom2)](https://postgis.net/docs/ST_Union.html)
geom1.Within(geom2)                                              | [ST_Within(geom1, geom2)](https://postgis.net/docs/ST_Within.html)
point.M                                                          | [ST_M(point)](https://postgis.net/docs/ST_M.html)
point.X                                                          | [ST_X(point)](https://postgis.net/docs/ST_X.html)
point.Y                                                          | [ST_Y(point)](https://postgis.net/docs/ST_Y.html)
point.Z                                                          | [ST_Z(point)](https://postgis.net/docs/ST_Z.html)
UnaryUnionOp.Union(geometries)                                   | [ST_Union(geometries)](https://postgis.net/docs/ST_Union.html)                       | See [Aggregate functions](translations.md#aggregate-functions).
GeometryCombiner.Combine(geometries)                             | [ST_Collect(geometries)](https://postgis.net/docs/ST_Collect.html)                   | See [Aggregate functions](translations.md#aggregate-functions).
EnvelopeCombiner.CombineAsGeometry(geometries)                   | [ST_Extent(geometries)::geometry](https://postgis.net/docs/ST_Extent.html)           | See [Aggregate functions](translations.md#aggregate-functions).
ConvexHull.Create(geometries)                                    | [ST_ConvexHull(geometries)](https://postgis.net/docs/ST_ConvexHull.html)             | See [Aggregate functions](translations.md#aggregate-functions).

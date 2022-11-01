# MapDataReader
Super fast mapping DataReader to a strongly typed object. High performance, lighweight (12Kb dll), uses AOT source generation and no reflection, mapping code is generated at compile time.

[![.NET](https://github.com/jitbit/MapDataReader/actions/workflows/dotnet.yml/badge.svg)](https://github.com/jitbit/MapDataReader/actions/workflows/dotnet.yml)
[![Nuget](https://img.shields.io/nuget/v/MapDataReader)](https://www.nuget.org/packages/MapDataReader/)
![Net stanrdard 2.0](https://img.shields.io/badge/netstandard-2.0-brightgreen)

## Benchmarks

20X faster than using reflection, even with caching. Benchmark for a tiny class with 5 string properties:

| Method         |      Mean |     Error |   StdDev |   Gen0 | Allocated |
|--------------- |----------:|----------:|---------:|-------:|----------:|
|  Reflection    | 951.16 ns | 15.107 ns | 0.828 ns | 0.1459 |     920 B |
|  MapDataReader |  44.15 ns |  2.840 ns | 0.156 ns | 0.0089 |      56 B |

## Install via [Nuget](https://www.nuget.org/packages/MapDataReader/)

```
Install-Package MapDataReader
```

## Usage with `IDataReader`

```csharp
using MapDataReader;

[GenerateDataReaderMapper] // <-- mark your class with this attribute
public class MyClass
{
	public int ID { get; set; }
	public string Name { get; set; }
	public int Size { get; set; }
	public bool Enabled { get; set; }
}

//ToMyClass() method is generated at compile time
List<MyClass> result = dbconnection.ExecuteReader("SELECT * FROM MyTable").ToMyClass();

//"ExecuteReader" is just a helper method from Dapper ORM, you're free to use other ways to create a datareader
```

Some notes for the above

* The `ToMyClass()` method above - is an `IDataReader` extension method generated at compile time. You can even "go to definition" in Visual Studio and examine its code.
* The naming convention is `ToCLASSNAME()` we can't use generics here, since `<T>` is not part of method signatures in C# (considered in later versions of C#). If you find a prettier way - please contribute!
* The mapper compares property names with pre-calculated hashes, like this
	```csharp
	if (namehash == -1150196469) { target.Name = value as string; return; }
	```
	to prevent expensive string comparisons
* Maps properies with public setters only.
* The datareader is being closed after mapping, so don't reuse it.
* Supports `enum` properties based on `int` and other implicit casting (sometimes a DataReader may decide to return `byte` for small integer database value, and it maps to `int` perfectly via some unboxing magic)
* Properly maps `DBNull` to `null`.
* Complex-type properties may not work.

## Bonus API: `SetPropertyByName`

This package also adds a super fast `SetPropertyByName` extension method generated at compile time for your class.

Usage:

```csharp
var x = new MyClass();
x.SetPropertyByName("Size", 42); //20X faster than using reflection
```

|                  Method |      Mean |     Error |    StdDev | Allocated |
|------------------------ |----------:|----------:|----------:|----------:|
|       SetPropReflection | 98.294 ns | 5.7443 ns | 0.3149 ns |         - |
| SetPropReflectionCached | 71.137 ns | 1.9736 ns | 0.1082 ns |         - |
|    SetPropMapDataReader |  4.711 ns | 0.4640 ns | 0.0254 ns |         - |

---

## Tip: Using it with Dapper

If you're already using the awesome [Dapper ORM](https://github.com/DapperLib/Dapper) by Marc Gravel, Sam Saffron and Nick Craver, this is how you can use our library to speed up DataReader-to-object mapping in Dapper:

```csharp
// override Dapper extension method to use fast MapDataReader instead of Dapper's built-in reflection
public static List<T> Query<T>(this SqlConnection cn, string sql, object parameters = null)
{
	if (typeof(T) == typeof(MyClass)) //our own class that we marked with attribute?
		return cn.ExecuteReader(sql, parameters).ToMyClass() as List<T>; //use MapDataReader

	if (typeof(T) == typeof(AnotherClass)) //another class we have enabled?
		return cn.ExecuteReader(sql, parameters).ToAnotherClass() as List<T>; //again

	//fallback to Dapper by default
	return SqlMapper.Query<T>(cn, sql, parameters).AsList();
}
```
Why the C# compiler will choose your method over Dapper's?

When the C# compiler sees two extension methods with the same signature, it uses the one that's "closer" to your code. "Closiness" - is determined by multiple factors - same namespace, same assembly, derived class over base class, implementation over interface etc. Adding an override like this will silently switch your existing code from using Dapper/reflection to using our source generator (b/c it uses a more specific connection type and lives in your project's namescape), while still keeping the awesomeness of Dapper and you barely have to rewrite any of your code.

---

## P.S. But what's the point?

While reflection-based ORMs like Dapper are very fast after all the reflaction objects have been cached, they still do a lot of reflection-based heavy-lifting when you query the database *for the first time*. Which slows down application startup *significantly*. Which, in turn, can become a problem if you deploy the application multiple times a day.

Or - if you run your ASP.NET Core app on IIS - this causes 503 errors during IIS recycles, see https://github.com/dotnet/aspnetcore/issues/41340 and faster app startup helps a lot.

Also, reflection-caching causes memory pressure becasue of all the concurrent dictionaries used for caching.

And even with all the caching, a simple straightforward code like `obj.x = y` will always be faster then looking up a cached delegate in a thousands-long dictionary by a string key and invoking it via reflection.

Even if you don't care about the startup performance of your app, `MapDataReader` is still 5-7% faster than `Dapper` (note - we're not even using Dapper's command-cache store here, just the datareader parser, actual real world Dapper scenario will be even slower)

|          Method |          Mean |         Error |       StdDev |   Gen0 |   Gen1 | Allocated |
|---------------- |--------------:|--------------:|-------------:|-------:|-------:|----------:|
| DapperWithCache |     142.09 us |  8,013.663 ns |   439.256 ns | 9.0332 | 1.2207 |   57472 B |
|   MapDataReader |     133.22 us | 28,679.198 ns | 1,572.004 ns | 9.0332 | 1.2207 |   57624 B |


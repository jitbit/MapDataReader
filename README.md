# MapDataReader
Super fast mapping DataReader to a strongly typed object. High performance, lighweight (12Kb dll), uses AOT source generation and no reflection, mapping code is generated at compile time.

[![.NET](https://github.com/jitbit/MapDataReader/actions/workflows/dotnet.yml/badge.svg)](https://github.com/jitbit/MapDataReader/actions/workflows/dotnet.yml)
[![Nuget](https://img.shields.io/nuget/v/MapDataReader)](https://www.nuget.org/packages/MapDataReader/)

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

## Usage

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

//ToMyClass() method code is generated at compile time
List<MyClass> result = dbconnection.ExecuteReader("SELECT * FROM MyTable").ToMyClass();
```

## Some notes

* The `ToMyClass()` method above - is an extension method generated at compile time. You can even "go to definition" in Visual Studio and examine its code.
* The naming convention is `ToCLASSNAME()` we can't use generics here, since `<T>` is not part of method signatures in C# (considered in later versions of C#).
* Maps properies with public setters only.
* The reader is being closed after mapping, so don't reuse it.
* The example above uses `.ExecuteReader` method from Dapper, but you can generate the reader in any way you want, e.g. the plain `SqlCommand.ExecuteReader` will also work.
* Supports `enum` properties based on `int` and other implicit casting (sometimes a DataReader may decide to return `byte` for small integer database value, and it maps to `int` perfectly via some unboxing magic)
* Properly maps `DBNull` to `null`.
* Complex-type properties may not work.
* netstandard 2.0
* Contributions are very welcome.

### P.S. Using it with Dapper

If you're already using the awesome [Dapper ORM](https://github.com/DapperLib/Dapper) by Marc Gravel, Sam Saffron and Nick Craver, this is how you can use our library to speed up DataReader-to-object mapping in Dapper:

```csharp
// override Dapper extension method to use fast MapDataReader instead of Dapper's built-in reflection
public static List<T> Query<T>(this SqlConnection cn, string sql, object parameters)
{
	if (typeof(T) == typeof(MyClass)) //our own class that we marked with attribute? use MapDataReader
		return cn.ExecuteReader(sql, parameters).ToMyClass() as List<T>;

	if (typeof(T) == typeof(AnotherClass)) //another class we have enabled? use MDR
		return cn.ExecuteReader(sql, parameters).ToAnotherClass() as List<T>;

	//fallback to Dapper by default
	return SqlMapper.Query<T>(cn, sql, parameters).AsList();
}
```

When the C# compiler sees two extension methods with the same signature, it uses the one that's "[closer](https://ericlippert.com/2013/12/23/closer-is-better/)" to your code. "Closiness" - is determined by multiple factos - same namespace, same assembly, derived class vs base class etc. etc (go read the article linked). Anyways, adding an override like this will silently switch your existing code from using Dapper/reflection to using our source generator, while still keeping awesomeness of Dapper and barely touching your code.

# MapDataReader
Super fast mapping DataReader to a strongly typed object. High performance, lighweight (12Kb dll), uses AOT source generation and no reflection, mapping code is generated at compile time.

[![.NET](https://github.com/jitbit/MapDataReader/actions/workflows/dotnet.yml/badge.svg)](https://github.com/jitbit/MapDataReader/actions/workflows/dotnet.yml)

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

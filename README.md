# MapDataReader
Super fast mapping DataReader to strongly typed object, Using AOT source generator.

## Benchmarks

| Method         |      Mean |     Error |   StdDev |   Gen0 | Allocated |
|--------------- |----------:|----------:|---------:|-------:|----------:|
|  Reflection    | 951.16 ns | 15.107 ns | 0.828 ns | 0.1459 |     920 B |
|  MapDataReader |  47.15 ns |  2.840 ns | 0.156 ns | 0.0089 |      56 B |

## Install

```
Install-Package MapDataReader
```

## Usage

```csharp
[GenerateDataReaderMapper]
public class MyClass
{
	public string Name { get; set; }
	public string Size { get; set; }
}

List<MyClass> result = cn.ExecuteReader("SELECT * FROM MyTable").ToMyClass();
```

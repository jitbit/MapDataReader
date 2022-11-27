using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Dapper;
using System.Data;
using System.Reflection;

namespace MapDataReader.Benchmarks
{
	internal class Program
	{
		static void Main(string[] args)
		{
			BenchmarkRunner.Run<Benchy>();
		}
	}

	[ShortRunJob, MemoryDiagnoser]
	public class Benchy
	{
		static TestClass _o = new TestClass();
		static PropertyInfo _prop = _o.GetType().GetProperty("String1", BindingFlags.Public | BindingFlags.Instance);

		[Benchmark]
		public void SetPropReflection()
		{
			PropertyInfo prop = _o.GetType().GetProperty("String1", BindingFlags.Public | BindingFlags.Instance);
			if (null != prop && prop.CanWrite)
			{
				prop.SetValue(_o, "Value");
			}
		}

		[Benchmark]
		public void SetPropReflectionCached()
		{
			_prop.SetValue(_o, "Value");
		}

		[Benchmark]
		public void SetPropMapDataReader()
		{
			_o.SetPropertyByName("String1", "Value");
		}
		
		[Benchmark]
		public void MapDatareaderViaDapper()
		{
			var dr = _dt.CreateDataReader();
			var list = dr.Parse<TestClass>().ToList();
		}

		[Benchmark]
		public void MapDataReaderViaMapaDataReader()
		{
			var dr = _dt.CreateDataReader();
			var list = dr.ToTestClass();
		}

		static DataTable _dt;

		[GlobalSetup]
		public static void Setup()
		{
			//create datatable with test data
			_dt = new DataTable();
			_dt.Columns.AddRange(new[] { new DataColumn("String1", typeof(string)), new DataColumn("String2", typeof(string)), new DataColumn("String3", typeof(string)), new DataColumn("Int", typeof(int)), new DataColumn("Int2", typeof(int)) });
			for (int i = 0; i < 1000; i++)
			{
				_dt.Rows.Add("xxx", "yyy", "zzz", 123, 321);
			}
		}
	}

	[GenerateDataReaderMapper]
	public class TestClass
	{
		public string String1 { get; set; }
		public string String2 { get; set; }
		public string String3 { get; set; }
		public string Int { get; set; }
		public string Int2 { get; set; }
	}
}
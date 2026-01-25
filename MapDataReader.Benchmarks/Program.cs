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
		static PropertyInfo _nullableprop = _o.GetType().GetProperty("IntNullable", BindingFlags.Public | BindingFlags.Instance);

		[Benchmark]
		public void SetProp_Reflection()
		{
			PropertyInfo prop = _o.GetType().GetProperty("String1", BindingFlags.Public | BindingFlags.Instance);
			if (null != prop && prop.CanWrite)
			{
				prop.SetValue(_o, "Value");
			}
		}

		[Benchmark]
		public void SetProp_ReflectionCached()
		{
			_prop.SetValue(_o, "Value");
		}

		[Benchmark]
		public void SetProp_MapDataReader()
		{
			_o.SetPropertyByName("String1", "Value");
		}

		[Benchmark]
		public void SetNullableProp_ReflectionCached()
		{
			_nullableprop.SetValue(_o, 123);
		}

		[Benchmark]
		public void SetNullableProp_MapDataReader()
		{
			_o.SetPropertyByName("IntNullable", 123);
		}

		[Benchmark]
		public void MapDatareader_ViaDapper()
		{
			var dr = _dt.CreateDataReader();
			var list = dr.Parse<TestClass>().ToList();
		}

		[Benchmark]
		public void MapDataReader_ViaMapDataReader()
		{
			var dr = _dt.CreateDataReader();
			var list = dr.ToTestClass();
		}

		[Benchmark]
		public void MapDataReader_ViaManualMap_CastMethods()
		{
			var dr = _dt.CreateDataReader();

			var list = new List<TestClass>();
			while (dr.Read())
			{
				list.Add(new TestClass
				{
					String1 = (string)dr["String1"],
					String2 = (string)dr["String2"],
					String3 = (string)dr["String3"],
					Int = (int)dr["Int"],
					Int2 = (int)dr["Int2"],
					IntNullable = (int?)dr["IntNullable"]
				});
			}
		}

		[Benchmark]
		public void MapDataReader_ViaManualMap_AsMethods()
		{
			var dr = _dt.CreateDataReader();

			var list = new List<TestClass>();
			while (dr.Read())
			{
				list.Add(new TestClass
				{
					String1 = dr["String1"] as string,
					String2 = dr["String2"] as string,
					String3 = dr["String3"] as string,
					Int = dr["Int"] as int? ?? 0,
					Int2 = dr["Int2"] as int? ?? 0,
					IntNullable = dr["IntNullable"] as int?
				});
			}
		}

		[Benchmark]
		public void MapDataReader_ViaManualMap_GetMethods()
		{
			var dr = _dt.CreateDataReader();

			var list = new List<TestClass>();
			while (dr.Read())
			{
				list.Add(new TestClass
				{
					String1 = dr.GetString(0),
					String2 = dr.GetString(1),
					String3 = dr.GetString(2),
					Int = dr.GetInt32(3),
					Int2 = dr.GetInt32(4),
					IntNullable = dr.GetInt32(5) // this wouldn't work if the value is null though, this is just for benchmarking
				});
			}
		}

		static DataTable _dt;

		[GlobalSetup]
		public static void Setup()
		{
			//create datatable with test data
			_dt = new DataTable();
			_dt.Columns.AddRange(new[] {
				new DataColumn("String1", typeof(string)),
				new DataColumn("String2", typeof(string)),
				new DataColumn("String3", typeof(string)),
				new DataColumn("Int", typeof(int)),
				new DataColumn("Int2", typeof(int)),
				new DataColumn("IntNullable", typeof(int))
			});


			for (int i = 0; i < 1000; i++)
			{
				_dt.Rows.Add("xxx", "yyy", "zzz", 123, 321, 3211);
			}
		}
	}

	[GenerateDataReaderMapper]
	public class TestClass
	{
		public string String1 { get; set; }
		public string String2 { get; set; }
		public string String3 { get; set; }
		public int Int { get; set; }
		public int Int2 { get; set; }
		public int? IntNullable { get; set; }
	}
}
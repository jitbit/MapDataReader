using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapDataReader.Tests
{
	public enum MyEnum
	{
		FirstDude,
		SecondDude,
		Third,
	}

	[GenerateDataReaderMapper]
	public class MyObject
	{
		public int Id { get; set; }
		public bool LaBoolissimmo { get; set; }
		public byte ByteMyAss { get; set;}
		public sbyte AssByte { get; set;}
		public char SnapChar { get; set;}
		public decimal Decl { get; set;}
		public double DoubleKick { get; set;}
		public float Floating { get; set;}
		public uint What { get; set; }
		public nint ImBored { get; set; }
		public long LOOOOOoong { get; set; }
		public short Shrt	{get;set;}
		public DateTime BirthDay { get; set; }
		public DateTime? NUllableBirthDay { get; set; }
		public TimeSpan Elapsed { get; set; }
		public Guid MyGuid { get; set; }


		public MyEnum Dude { get; set; }
		public MyEnum? NullableDude { get; set; }
		public string Name { get; set; }

		public int GetOnly { get; } = 123; //property without public setter!

		public byte[] ByeArray { get; set; }
		public int[] IntArray { get; set; }
		public string[] StringArray { get; set; }
		public long[] LongArray { get; set; }
	}

	[TestClass]
	public class TestActualCode
	{
		[TestMethod]
		public void TestPrimitiveTypesAssign()
		{
			var o = new MyObject();
			o.SetPropertyByName("Id", 123);
			Assert.IsTrue(o.Id == 123);

			o.SetPropertyByName("Id", (byte)25); //test upcasting from byte
			Assert.IsTrue(o.Id == 25);

			o.SetPropertyByName("LaBoolissimmo", true);
			Assert.IsTrue(o.LaBoolissimmo);
			o.SetPropertyByName("ByteMyAss", (byte)123);
			Assert.IsTrue(o.ByteMyAss == 123);
			o.SetPropertyByName("ByteMyAss", 123); //lets try to stick an int in it
			Assert.IsTrue(o.ByteMyAss == 123);
			o.SetPropertyByName("AssByte", 123);
			Assert.IsTrue(o.AssByte == 123);
			o.SetPropertyByName("SnapChar", 123);
			Assert.IsTrue(o.SnapChar == 123);
			o.SetPropertyByName("Decl", 123);
			Assert.IsTrue(o.Decl == 123);
			o.SetPropertyByName("DoubleKick", 123);
			Assert.IsTrue(o.DoubleKick == 123);
			o.SetPropertyByName("Floating", 123);
			Assert.IsTrue(o.Floating == 123);
			o.SetPropertyByName("fLOAtInG", 123); //mess the casing, should still work
			Assert.IsTrue(o.Floating == 123);
			o.SetPropertyByName("What", 123);
			Assert.IsTrue(o.What == 123);
			o.SetPropertyByName("ImBored", (nint)123);
			Assert.IsTrue(o.ImBored == 123);
			o.SetPropertyByName("LOOOOOoong", 123);
			Assert.IsTrue(o.LOOOOOoong == 123);
			o.SetPropertyByName("Shrt", 123);
			Assert.IsTrue(o.Shrt == 123);
			o.SetPropertyByName("Elapsed", TimeSpan.FromSeconds(123));
			Assert.IsTrue(o.Elapsed == TimeSpan.FromSeconds(123));

			var guid = Guid.NewGuid();
			o.SetPropertyByName("MyGuid", guid);
			Assert.IsTrue(o.MyGuid == guid);

			var dt = new DateTime(2022, 09, 09);
			o.SetPropertyByName("BirthDay", dt);
			Assert.IsTrue(o.BirthDay == dt);
			o.SetPropertyByName("NUllableBirthDay", dt);
			Assert.IsTrue(o.NUllableBirthDay == dt);

			//test nullable assign
			DateTime? ndt = null;
			ndt = new DateTime(2022, 10, 10);
			o.SetPropertyByName("NUllableBirthDay", ndt);
			Assert.IsTrue(o.NUllableBirthDay == ndt);

			o.SetPropertyByName("GetOnly", 321); //should not throw any exception, even though this property is not settable

			o.SetPropertyByName("ByeArray", new byte[3] { 1, 2, 3 });
			Assert.IsTrue(o.ByeArray.SequenceEqual(new byte[3] { 1, 2, 3 }));
			o.SetPropertyByName("IntArray", new int[3] { 1, 2, 3 });
			Assert.IsTrue(o.IntArray.SequenceEqual(new int[3] { 1, 2, 3 }));
			o.SetPropertyByName("StringArray", new [] { "1", "2", "3" });
			Assert.IsTrue(o.StringArray.SequenceEqual(new[] { "1", "2", "3" }));
			o.SetPropertyByName("LongArray", new long[3] { 1, 2, 3 });
			Assert.IsTrue(o.LongArray.SequenceEqual(new long[3] { 1, 2, 3 }));
		}

		[TestMethod]
		public void TestEnumAssign()
		{
			var o = new MyObject();
			o.SetPropertyByName("Dude", 0);
			Assert.IsTrue(o.Dude == MyEnum.FirstDude);

			o.SetPropertyByName("Dude", 1);
			Assert.IsTrue(o.Dude == MyEnum.SecondDude);

			o.SetPropertyByName("Dude", (byte)2); //let's shove a BYTE in there!
			Assert.IsTrue(o.Dude == MyEnum.Third); //eat this, boxing!!

			o.SetPropertyByName("NullableDude", 1);
			Assert.IsTrue(o.NullableDude == MyEnum.SecondDude);

			o.SetPropertyByName("NullableDude", MyEnum.FirstDude);
			Assert.IsTrue(o.NullableDude == MyEnum.FirstDude);
		}

		[TestMethod]
		public void TestStringAssign()
		{
			var o = new MyObject();
			o.SetPropertyByName("Name", "awsdkljfghsldkgjh");
			Assert.IsTrue(o.Name == "awsdkljfghsldkgjh");
		}

		[TestMethod]
		public void TestDatatReader()
		{
			//create datatable with test data
			var dt = new DataTable();
			dt.Columns.AddRange(new[] {
				new DataColumn("ID", typeof(int)),
				new DataColumn("Name", typeof(string)),
				new DataColumn("LaBoolissimmo", typeof(bool)),
				new DataColumn("Floating", typeof(float)),
				new DataColumn("LOOOOOoong", typeof(long)),
				new DataColumn("BirthDay", typeof(DateTime)),
				new DataColumn("Elapsed", typeof(TimeSpan)),
				new DataColumn("ByeArray", typeof(byte[])),
			});
			var date = new DateTime(2022, 09, 09);
			dt.Rows.Add(123, "ggg", true, 3213, 123, date, TimeSpan.FromSeconds(123), new byte[] { 3, 2, 1 });
			dt.Rows.Add(3, "fgdk", false, 11123, 321, date, TimeSpan.FromSeconds(123), new byte[] { 5, 6, 7, 8 });

			var list = dt.CreateDataReader().ToMyObject();

			Assert.IsTrue(list.Count == 2);

			Assert.IsTrue(list[0].Id == 123);
			Assert.IsTrue(list[0].Name == "ggg");
			Assert.IsTrue(list[0].LaBoolissimmo == true);
			Assert.IsTrue(list[0].Floating == 3213);
			Assert.IsTrue(list[0].LOOOOOoong == 123);
			Assert.IsTrue(list[0].BirthDay == date);
			Assert.IsTrue(list[0].Elapsed == TimeSpan.FromSeconds(123));
			Assert.IsTrue(list[0].ByeArray.SequenceEqual(new byte[3] { 3, 2, 1 }));


			Assert.IsTrue(list[1].Id == 3);
			Assert.IsTrue(list[1].Name == "fgdk");
			Assert.IsTrue(list[1].LaBoolissimmo == false);
			Assert.IsTrue(list[1].Floating == 11123);
			Assert.IsTrue(list[1].LOOOOOoong == 321);
			Assert.IsTrue(list[1].BirthDay == date);
			Assert.IsTrue(list[1].Elapsed == TimeSpan.FromSeconds(123));
			Assert.IsTrue(list[1].ByeArray.SequenceEqual(new byte[4] { 5, 6, 7, 8 }));

			//now create datatable with different column order and test on the same code generator!!!
			var dt2 = new DataTable();
			dt2.Columns.AddRange(new[] {
				new DataColumn("LaBoolissimmo", typeof(bool)),
				new DataColumn("Name", typeof(string)),
				new DataColumn("ID", typeof(int)),
			});

			dt2.Rows.Add(true, "alex", 123);

			list = dt2.CreateDataReader().ToMyObject(); //should not throw exception

			Assert.IsTrue(list[0].Id == 123);
			Assert.IsTrue(list[0].Name == "alex");
			Assert.IsTrue(list[0].LaBoolissimmo == true);
		}

		[TestMethod]
		public void TestBaseClassAssign()
		{
			var o = new ChildClass();
			o.SetPropertyByName("Id", 123);
			o.SetPropertyByName("Name", "blahblah");

			Assert.IsTrue(o.Id == 123);
			Assert.IsTrue(o.Name == "blahblah");
		}

		[TestMethod]
		public void TestWrongProperty()
		{
			var o = new MyObject();
			o.SetPropertyByName("NonExistingProperttyName", 123); //should not throw exception

			//now test wrong type
			o.Name = "lalala";
			o.SetPropertyByName("Name", 123); //try to assign string prop to int
			Assert.IsTrue(o.Name == null); //wrong type. should be null
		}
	}

	public class BaseClass
	{
		public int Id { get; set; }
	}

	[GenerateDataReaderMapper]
	public class ChildClass : BaseClass
	{
		public string Name { get; set; }
	}
}


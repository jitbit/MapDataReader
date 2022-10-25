using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapDataReader.Tests
{
	public enum MyEnum
	{
		FirstDude,
		SecondDude
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

			o.SetPropertyByName("Dude", (byte)1); //let's shove a BYTE in there!
			Assert.IsTrue(o.Dude == MyEnum.SecondDude); //eat this, boxing!!
		}

		[TestMethod]
		public void TestStringAssign()
		{
			var o = new MyObject();
			o.SetPropertyByName("Name", "awsdkljfghsldkgjh");
			Assert.IsTrue(o.Name == "awsdkljfghsldkgjh");
		}
	}
}

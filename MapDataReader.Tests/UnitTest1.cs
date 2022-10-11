using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace MapDataReader.Tests
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestGeneral()
		{
			string userSource = @"
using MapDataReader;

namespace MyCode
{
	[GenerateDataReaderMapper]
	public class MyClass
	{
		public string Name {get;set;}
		public int Size {get;set;}
		public bool Enabled {get;set;}
	}
}
";
			var src = GetAndCheckOutputSource(userSource);

			Assert.IsTrue(src.Contains(@"case ""Name"": target.Name = (string)value; break;"));
			Assert.IsTrue(src.Contains(@"case ""Size"": target.Size = (int)Convert.ChangeType(value, typeof(int)); break;"));
			Assert.IsTrue(src.Contains(@"case ""Enabled"": target.Enabled = (bool)Convert.ChangeType(value, typeof(bool)); break;"));
		}

		[TestMethod]
		public void TestNoPublicSetter()
		{
			string userSource = @"
using MapDataReader;

namespace MyCode
{
	[GenerateDataReaderMapper]
	public class A
	{
		public string Name {get; private set; }
		public string NamePublic {get; set; }
	}
}
";
			var src = GetAndCheckOutputSource(userSource);

			Assert.IsFalse(src.Contains(@"case ""Name"""));
			Assert.IsTrue(src.Contains(@"case ""NamePublic"""));
		}

		[TestMethod]
		public void TestNoNamespace()
		{
			string userSource = @"
using MapDataReader;

[GenerateDataReaderMapper]
public class A
{
	public string B {get; set; }
}
";
			var src = GetAndCheckOutputSource(userSource);

			Assert.IsTrue(src.Contains(@"case ""B"": target.B = (string)value;"));
		}

		[TestMethod]
		public void TestWithArrays()
		{
			string userSource = @"
using MapDataReader;

[GenerateDataReaderMapper]
public class A
{
	public byte[] B {get; set; }
	public string[] C {get; set; }
	public int[] D {get; set; }
}
";
			var src = GetAndCheckOutputSource(userSource);

			Assert.IsTrue(src.Contains(@"case ""B"": target.B = (byte[])value;"));
			Assert.IsTrue(src.Contains(@"case ""C"": target.C = (string[])value;"));
			Assert.IsTrue(src.Contains(@"case ""D"": target.D = (int[])value;"));
		}

		//gets generated source and also unit-tests for exceptions and empty diagnistics etc
		private string GetAndCheckOutputSource(string inputSource)
		{
			var generator = new MapperGenerator();

			Compilation comp = CreateCompilation(inputSource);

			// Create the driver that will control the generation, passing in our generator
			GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

			// Run the generation pass
			// (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
			driver = driver.RunGeneratorsAndUpdateCompilation(comp, out var outputCompilation, out var diagnostics);

			// We can now assert things about the resulting compilation:
			Assert.IsTrue(diagnostics.IsEmpty); // there were no diagnostics created by the generators
			Assert.IsTrue(outputCompilation.SyntaxTrees.Count() == 2); // we have two syntax trees, the original 'user' provided one, and the one added by the generator
			var compDiag = outputCompilation.GetDiagnostics();
			Assert.IsTrue(compDiag.IsEmpty); // verify the compilation with the added source has no diagnostics

			// Or we can look at the results directly:
			GeneratorDriverRunResult runResult = driver.GetRunResult();

			// The runResult contains the combined results of all generators passed to the driver
			Assert.IsTrue(runResult.GeneratedTrees.Length == 1);
			Assert.IsTrue(runResult.Diagnostics.IsEmpty);

			// Or you can access the individual results on a by-generator basis
			GeneratorRunResult generatorResult = runResult.Results[0];
			Assert.IsTrue(generatorResult.Generator == generator);
			Assert.IsTrue(generatorResult.Diagnostics.IsEmpty);
			Assert.IsTrue(generatorResult.GeneratedSources.Length == 1);
			Assert.IsTrue(generatorResult.Exception is null);

			//now actually return the source generated
			return generatorResult.GeneratedSources[0].SourceText.ToString();
		}

		private static Compilation CreateCompilation(string source)
			=> CSharpCompilation.Create("compilation",
				new[] { CSharpSyntaxTree.ParseText(source) },
				new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
					MetadataReference.CreateFromFile(typeof(MapperGenerator).GetTypeInfo().Assembly.Location),
					MetadataReference.CreateFromFile(typeof(IDataReader).GetTypeInfo().Assembly.Location),
					MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "System.Runtime.dll")),
					MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "netstandard").Location)
				},
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
	}
}
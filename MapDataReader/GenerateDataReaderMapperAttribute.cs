using System;

namespace MapDataReader;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class GenerateDataReaderMapperAttribute : Attribute
{
	public string AccessModifier { get; set; }
		
	/// <summary>
	/// Gets or sets the namespace to be used in the generated methods.
	/// </summary>
	public string NamespaceName { get; set; }
		
	/// <summary>
	/// Gets or sets the method name to be used in the generated methods.
	/// </summary>
	public string MethodName { get; set; }

	public GenerateDataReaderMapperAttribute()
	{
		AccessModifier = "public";
	}

	public GenerateDataReaderMapperAttribute(string access = "public")
	{
		AccessModifier = access;
	}
}
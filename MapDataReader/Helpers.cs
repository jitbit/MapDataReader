using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapDataReader;

internal static class Helpers
{
	internal static bool IsDecoratedWithAttribute(this TypeDeclarationSyntax cdecl, string attributeName) =>
		cdecl.AttributeLists
			.SelectMany(x => x.Attributes)
			.Any(x => x.Name.ToString().Contains(attributeName));


	internal static string FullName(this ITypeSymbol typeSymbol) => typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

	internal static string StringConcat(this IEnumerable<string> source, string separator) => string.Join(separator, source);

	// returns all properties with public setters
	internal static IEnumerable<IPropertySymbol> GetAllSettableProperties(this ITypeSymbol typeSymbol)
	{
		var result = typeSymbol
			.GetMembers()
			.Where(s => s.Kind == SymbolKind.Property).Cast<IPropertySymbol>() //get all properties
			.Where(p => p.SetMethod?.DeclaredAccessibility == Accessibility.Public) //has a public setter?
			.ToList();

		//now get the base class
		var baseType = typeSymbol.BaseType;
		if (baseType != null)
			result.AddRange(baseType.GetAllSettableProperties()); //recursion

		return result;
	}

	//checks if type is a nullable num
	internal static bool IsNullableEnum(this ITypeSymbol symbol)
	{
		//tries to get underlying non-nullable type from nullable type
		//and then check if it's Enum
		if (symbol.NullableAnnotation == NullableAnnotation.Annotated
		    && symbol is INamedTypeSymbol namedType
		    && namedType.IsValueType
		    && namedType.IsGenericType
		    && namedType.ConstructedFrom?.ToDisplayString() == "System.Nullable<T>"
		   )
			return namedType.TypeArguments[0].TypeKind == TypeKind.Enum;

		return false;
	}
}
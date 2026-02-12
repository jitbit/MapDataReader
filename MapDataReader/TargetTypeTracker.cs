using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapDataReader;

internal class TargetTypeTracker : ISyntaxContextReceiver
{
	public IImmutableList<ClassDeclarationSyntax> TypesNeedingGening = ImmutableList.Create<ClassDeclarationSyntax>();

	public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
	{
		if (context.Node is not ClassDeclarationSyntax classDec) return;
			
		if (classDec.IsDecoratedWithAttribute("GenerateDataReaderMapper"))
			TypesNeedingGening = TypesNeedingGening.Add(classDec);
	}
}
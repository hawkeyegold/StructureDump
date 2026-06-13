using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class DependencyWalker : CSharpSyntaxWalker {
	private readonly Dictionary<string, HashSet<string>> _deps = new();

	public string GetOutput() {
		var sw = new StringWriter();
		foreach (var kv in _deps) {
			sw.WriteLine($"TYPE: {kv.Key}");
			sw.WriteLine("DEPENDS_ON:");
			foreach (var d in kv.Value)
				sw.WriteLine($"    {d}");
			sw.WriteLine();
		}
		return sw.ToString();
	}

	public override void VisitClassDeclaration(ClassDeclarationSyntax node)
			=> RegisterType(node.Identifier.Text, node);

	public override void VisitStructDeclaration(StructDeclarationSyntax node)
			=> RegisterType(node.Identifier.Text, node);

	public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
			=> RegisterType(node.Identifier.Text, node);

	private void RegisterType(string typeName, TypeDeclarationSyntax node) {
		if (!_deps.ContainsKey(typeName))
			_deps[typeName] = new HashSet<string>();

		foreach (var id in node.DescendantNodes().OfType<IdentifierNameSyntax>())
			_deps[typeName].Add(id.Identifier.Text);
	}
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class MinimalWalker : CSharpSyntaxWalker {
	private readonly StringWriter _sw = new();

	public string GetOutput() => _sw.ToString();

	public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node) {
		_sw.WriteLine($"NAMESPACE: {node.Name}");
		base.VisitNamespaceDeclaration(node);
	}

	public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node) {
		_sw.WriteLine($"NAMESPACE: {node.Name}");
		base.VisitFileScopedNamespaceDeclaration(node);
	}

	public override void VisitClassDeclaration(ClassDeclarationSyntax node) {
		_sw.WriteLine($"TYPE: {node.Identifier.Text} (class)");
		WriteMembers(node.Members);
	}

	public override void VisitStructDeclaration(StructDeclarationSyntax node) {
		_sw.WriteLine($"TYPE: {node.Identifier.Text} (struct)");
		WriteMembers(node.Members);
	}

	public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) {
		_sw.WriteLine($"TYPE: {node.Identifier.Text} (interface)");
		WriteMembers(node.Members);
	}

	private void WriteMembers(SyntaxList<MemberDeclarationSyntax> members) {
		var methods = members.OfType<MethodDeclarationSyntax>().Select(m => m.Identifier.Text);
		var props = members.OfType<PropertyDeclarationSyntax>().Select(p => p.Identifier.Text);
		var enums = members.OfType<EnumDeclarationSyntax>();

		if (methods.Any()) {
			_sw.WriteLine("    METHODS:");
			foreach (var m in methods)
				_sw.WriteLine($"        {m}");
		}

		if (props.Any()) {
			_sw.WriteLine("    PROPERTIES:");
			foreach (var p in props)
				_sw.WriteLine($"        {p}");
		}

		foreach (var en in enums) {
			_sw.WriteLine($"    ENUM: {en.Identifier.Text}: {string.Join(", ", en.Members.Select(m => m.Identifier.Text))}");
		}

		_sw.WriteLine();
	}
}

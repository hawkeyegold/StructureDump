using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.IO;

public class NamespaceWalker : CSharpSyntaxWalker {
	private readonly Dictionary<string, HashSet<string>> _types = new();
	private readonly Dictionary<string, HashSet<string>> _deps = new();
	private string _currentNamespace = "GLOBAL";

	public string GetOutput() {
		var sw = new StringWriter();

		foreach (var ns in _types.Keys) {
			sw.WriteLine($"NAMESPACE: {ns}");
			sw.WriteLine("TYPES:");
			foreach (var t in _types[ns])
				sw.WriteLine($"    {t}");

			sw.WriteLine("DEPENDS_ON:");
			foreach (var d in _deps[ns])
				sw.WriteLine($"    {d}");

			sw.WriteLine();
		}

		return sw.ToString();
	}

	private void EnsureNamespace(string ns) {
		if (!_types.ContainsKey(ns))
			_types[ns] = new HashSet<string>();

		if (!_deps.ContainsKey(ns))
			_deps[ns] = new HashSet<string>();
	}

	public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node) {
		_currentNamespace = node.Name.ToString();
		EnsureNamespace(_currentNamespace);
		base.VisitNamespaceDeclaration(node);
	}

	public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node) {
		_currentNamespace = node.Name.ToString();
		EnsureNamespace(_currentNamespace);
		base.VisitFileScopedNamespaceDeclaration(node);
	}

	public override void VisitClassDeclaration(ClassDeclarationSyntax node) {
		EnsureNamespace(_currentNamespace);
		_types[_currentNamespace].Add(node.Identifier.Text);
		base.VisitClassDeclaration(node);
	}

	public override void VisitStructDeclaration(StructDeclarationSyntax node) {
		EnsureNamespace(_currentNamespace);
		_types[_currentNamespace].Add(node.Identifier.Text);
		base.VisitStructDeclaration(node);
	}

	public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) {
		EnsureNamespace(_currentNamespace);
		_types[_currentNamespace].Add(node.Identifier.Text);
		base.VisitInterfaceDeclaration(node);
	}

	public override void VisitUsingDirective(UsingDirectiveSyntax node) {
		if (string.IsNullOrEmpty(_currentNamespace))
			_currentNamespace = "GLOBAL";

		EnsureNamespace(_currentNamespace);

		if (node.Name != null)
			_deps[_currentNamespace].Add(node.Name.ToString());

		base.VisitUsingDirective(node);
	}
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using System.Linq;

public class ArchitectureWalker : CSharpSyntaxWalker {
	private readonly StringWriter _sw = new();

	public string GetOutput() => _sw.ToString();

	public override void VisitCompilationUnit(CompilationUnitSyntax node) {
		_sw.WriteLine($"FILE: {node.SyntaxTree.FilePath}");
		_sw.WriteLine();
		_sw.WriteLine("USINGS:");

		foreach (var u in node.Usings)
			_sw.WriteLine($"    {u.ToString().Trim()}");

		_sw.WriteLine();
		base.VisitCompilationUnit(node);
	}

	public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node) {
		base.VisitNamespaceDeclaration(node);
	}

	public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node) {
		base.VisitFileScopedNamespaceDeclaration(node);
	}

	public override void VisitClassDeclaration(ClassDeclarationSyntax node) {
		WriteTypeHeader("class", node.Identifier.Text, node.Modifiers, node.BaseList, node.AttributeLists);
		WriteFields(node.Members);
		WriteProperties(node.Members);
		WriteMethods(node.Members);
		WriteEvents(node.Members);
		WriteEnums(node.Members);
		base.VisitClassDeclaration(node);
	}

	public override void VisitStructDeclaration(StructDeclarationSyntax node) {
		WriteTypeHeader("struct", node.Identifier.Text, node.Modifiers, node.BaseList, node.AttributeLists);
		WriteFields(node.Members);
		WriteProperties(node.Members);
		WriteMethods(node.Members);
		WriteEvents(node.Members);
		WriteEnums(node.Members);
		base.VisitStructDeclaration(node);
	}

	public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) {
		WriteTypeHeader("interface", node.Identifier.Text, node.Modifiers, node.BaseList, node.AttributeLists);
		WriteMethods(node.Members);
		base.VisitInterfaceDeclaration(node);
	}

	public override void VisitEnumDeclaration(EnumDeclarationSyntax node) {
		WriteTypeHeader("enum", node.Identifier.Text, node.Modifiers, null, node.AttributeLists);
		WriteEnums(node.Members);   // now matches the new overload
		base.VisitEnumDeclaration(node);
	}

	private void WriteTypeHeader(
			string kind,
			string name,
			SyntaxTokenList modifiers,
			BaseListSyntax? baseList,
			SyntaxList<AttributeListSyntax> attributes) {
		_sw.WriteLine($"TYPE: {kind} {name}");
		_sw.WriteLine($"ACCESS: {GetAccessModifier(modifiers)}");

		var extraModifiers = string.Join(" ",
				modifiers.Where(m =>
						m.Text != "public" &&
						m.Text != "private" &&
						m.Text != "internal" &&
						m.Text != "protected"));

		_sw.WriteLine("MODIFIERS: " + extraModifiers);

		if (baseList != null) {
			var bases = baseList.Types.Select(t => t.ToString());
			_sw.WriteLine("BASE/IMPLEMENTS: " + string.Join(", ", bases));
		}

		_sw.WriteLine();
	}

	private string GetAccessModifier(SyntaxTokenList modifiers) {
		if (modifiers.Any(m => m.Text == "public")) return "public";
		if (modifiers.Any(m => m.Text == "private")) return "private";
		if (modifiers.Any(m => m.Text == "protected")) return "protected";
		if (modifiers.Any(m => m.Text == "internal")) return "internal";
		return "internal";
	}

	private void WriteFields(SyntaxList<MemberDeclarationSyntax> members) {
		var fields = members.OfType<FieldDeclarationSyntax>().ToList();
		if (fields.Count == 0) return;

		_sw.WriteLine("FIELDS:");
		foreach (var f in fields) {
			var type = f.Declaration.Type.ToString();
			foreach (var v in f.Declaration.Variables)
				_sw.WriteLine($"    {type} {v.Identifier.Text}");
		}
		_sw.WriteLine();
	}

	private void WriteProperties(SyntaxList<MemberDeclarationSyntax> members) {
		var props = members.OfType<PropertyDeclarationSyntax>().ToList();
		if (props.Count == 0) return;

		_sw.WriteLine("PROPERTIES:");
		foreach (var p in props)
			_sw.WriteLine($"    {p.Type} {p.Identifier.Text}");
		_sw.WriteLine();
	}

	private void WriteMethods(SyntaxList<MemberDeclarationSyntax> members) {
		var methods = members.OfType<MethodDeclarationSyntax>().ToList();
		if (methods.Count == 0) return;

		_sw.WriteLine("METHODS:");
		foreach (var m in methods) {
			var parameters = string.Join(", ",
					m.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier.Text}"));

			_sw.WriteLine($"    {m.ReturnType} {m.Identifier.Text}({parameters})");
		}
		_sw.WriteLine();
	}

	private void WriteEvents(SyntaxList<MemberDeclarationSyntax> members) {
		var events = members.OfType<EventDeclarationSyntax>().ToList();
		if (events.Count == 0) return;

		_sw.WriteLine("EVENTS:");
		foreach (var e in events)
			_sw.WriteLine($"    {e.Type} {e.Identifier.Text}");
		_sw.WriteLine();
	}

	// ORIGINAL WriteEnums for classes/structs (MemberDeclarationSyntax)
	private void WriteEnums(SyntaxList<MemberDeclarationSyntax> members) {
		var enums = members.OfType<EnumDeclarationSyntax>().ToList();
		if (enums.Count == 0) return;

		_sw.WriteLine("ENUMS:");
		foreach (var en in enums)
			_sw.WriteLine($"    {en.Identifier.Text}");
		_sw.WriteLine();
	}

	// NEW overload for actual enum members
	private void WriteEnums(SeparatedSyntaxList<EnumMemberDeclarationSyntax> members) {
		if (members.Count == 0) return;

		_sw.WriteLine("ENUM MEMBERS:");
		foreach (var en in members)
			_sw.WriteLine($"    {en.Identifier.Text}");
		_sw.WriteLine();
	}
}

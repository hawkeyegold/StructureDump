using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class InheritanceWalker : CSharpSyntaxWalker {
	private readonly Dictionary<string, List<string>> _tree = new();

	public string GetOutput() {
		var sw = new StringWriter();

		foreach (var kv in _tree) {
			sw.WriteLine(kv.Key);
			foreach (var child in kv.Value)
				sw.WriteLine($"    {child}");
			sw.WriteLine();
		}

		return sw.ToString();
	}

	public override void VisitClassDeclaration(ClassDeclarationSyntax node) {
		string name = node.Identifier.Text;

		if (node.BaseList != null) {
			foreach (var b in node.BaseList.Types) {
				string baseName = b.Type.ToString();
				if (!_tree.ContainsKey(baseName))
					_tree[baseName] = new List<string>();

				_tree[baseName].Add(name);
			}
		}
	}
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class RoslynLoader {
	public static List<SyntaxTree> LoadProjectSyntaxTrees(string projectDir) {
		var trees = new List<SyntaxTree>();

		foreach (var file in Directory.GetFiles(projectDir, "*.cs", SearchOption.AllDirectories)) {
			var text = File.ReadAllText(file);
			trees.Add(CSharpSyntaxTree.ParseText(text, path: file));
		}

		return trees;
	}
}

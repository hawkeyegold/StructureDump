using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;

internal class Program {
	static void Main(string[] args) {

		Console.WriteLine("=== StructureDump Debug ===");

		if (args.Length == 0) {
			Console.WriteLine("ERROR: No solution root provided.");
			return;
		}

		string root = args[0];
		Console.WriteLine($"Solution root: {root}");

		// The directory the user EXPECTS the tool to use
		string workingDir = Directory.GetCurrentDirectory();
		Console.WriteLine($"Working directory: {workingDir}");

		// Output folder inside the project directory
		string outputDir = Path.Combine(workingDir, "Output");
		Console.WriteLine($"Output directory: {outputDir}");

		Directory.CreateDirectory(outputDir);

		// Timestamped filename
		string projectName = new DirectoryInfo(root).Name;
		string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
		string outputPath = Path.Combine(outputDir, $"{projectName}_structure_dump_{timestamp}.txt");
		Console.WriteLine($"Output file path: {outputPath}");

		var sb = new StringBuilder();

		Console.WriteLine("Scanning for .cs files...");
		var files = Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories);
		Console.WriteLine($"Found {files.Length} .cs files.");

		foreach (var file in files) {
			sb.AppendLine(Path.GetFileName(file));

			var text = File.ReadAllText(file);
			var tree = CSharpSyntaxTree.ParseText(text);
			var rootNode = tree.GetRoot();

			foreach (var node in rootNode.DescendantNodes()) {
				switch (node) {
					case Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax ns:
						sb.AppendLine($"  namespace {ns.Name}");
						break;

					case Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax cls:
						sb.AppendLine($"    class {cls.Identifier.Text}");
						break;

					case Microsoft.CodeAnalysis.CSharp.Syntax.StructDeclarationSyntax str:
						sb.AppendLine($"    struct {str.Identifier.Text}");
						break;

					case Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax iface:
						sb.AppendLine($"    interface {iface.Identifier.Text}");
						break;

					case Microsoft.CodeAnalysis.CSharp.Syntax.EnumDeclarationSyntax en:
						sb.AppendLine($"    enum {en.Identifier.Text}");
						foreach (var member in en.Members)
							sb.AppendLine($"      {member.Identifier.Text}");
						break;

					case Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax method:
						sb.AppendLine($"      {method.Identifier.Text}()");
						break;

					case Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax prop:
						sb.AppendLine($"      {prop.Identifier.Text} {{ get; set; }}");
						break;
				}
			}

			sb.AppendLine();
		}

		File.WriteAllText(outputPath, sb.ToString());
		Console.WriteLine("File written.");

		// Verify file exists
		bool exists = File.Exists(outputPath);
		Console.WriteLine($"File exists after write: {exists}");

		Console.WriteLine("=== Done ===");
	}
}

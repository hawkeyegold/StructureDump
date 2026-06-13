using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

internal class Program {
	static void Main(string[] args) {
		Console.WriteLine("=== StructureDump ===");

		if (args.Length == 0) {
			Console.WriteLine("ERROR: No input provided.");
			Console.WriteLine("Pass a folder, .sln, or .csproj.");
			return;
		}

		string input = args[0];

		// Capture timestamp ONCE for the entire dump run
		string timestamp = DateTime.Now.ToString("HH.mm.ss");
		string dateFolder = DateTime.Now.ToString("MM-dd-yyyy");

		// ------------------------------------------------------------
		// MODE 1: FOLDER MODE
		// ------------------------------------------------------------
		if (Directory.Exists(input)) {
			Console.WriteLine($"Folder mode: {input}");

			var slnFiles = Directory.GetFiles(input, "*.sln", SearchOption.AllDirectories);

			if (slnFiles.Length == 0) {
				Console.WriteLine("No .sln files found in folder.");
				return;
			}

			foreach (var sln in slnFiles) {
				Console.WriteLine($"Found solution: {sln}");
				var projects = ParseSolutionForProjects(sln);

				foreach (var proj in projects)
					CreateOutputForProject(proj, timestamp, dateFolder);
			}

			Console.WriteLine("=== Done ===");
			return;
		}

		// ------------------------------------------------------------
		// MODE 2: SOLUTION MODE
		// ------------------------------------------------------------
		if (input.EndsWith(".sln", StringComparison.OrdinalIgnoreCase)) {
			Console.WriteLine($"Solution mode: {input}");

			var projects = ParseSolutionForProjects(input);

			foreach (var proj in projects)
				CreateOutputForProject(proj, timestamp, dateFolder);

			Console.WriteLine("=== Done ===");
			return;
		}

		// ------------------------------------------------------------
		// MODE 3: PROJECT MODE
		// ------------------------------------------------------------
		if (input.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)) {
			Console.WriteLine($"Project mode: {input}");

			CreateOutputForProject(input, timestamp, dateFolder);

			Console.WriteLine("=== Done ===");
			return;
		}

		// ------------------------------------------------------------
		// INVALID INPUT
		// ------------------------------------------------------------
		Console.WriteLine("ERROR: Input must be a folder, .sln, or .csproj.");
	}

	// ========================================================================
	// SOLUTION PARSER (FINAL, CORRECT)
	// ========================================================================
	private static List<string> ParseSolutionForProjects(string slnPath) {
		Console.WriteLine($"Parsing solution: {slnPath}");

		var projectPaths = new List<string>();
		string? solutionDir = Path.GetDirectoryName(slnPath);

		if (solutionDir is null) {
			Console.WriteLine("ERROR: Could not determine solution directory.");
			return projectPaths;
		}

		foreach (var line in File.ReadLines(slnPath)) {
			if (!line.StartsWith("Project(", StringComparison.OrdinalIgnoreCase))
				continue;

			if (!line.Contains(".csproj", StringComparison.OrdinalIgnoreCase))
				continue;

			// Split on '=' first
			var eqSplit = line.Split('=');
			if (eqSplit.Length < 2)
				continue;

			// Right side: "Name", "Path", "{GUID}"
			var rightSide = eqSplit[1];

			// Now split on commas
			var parts = rightSide.Split(',');

			if (parts.Length < 2)
				continue;

			// The PATH is always the second quoted string
			string rawPathPart = parts[1].Trim();

			int firstQuote = rawPathPart.IndexOf('"');
			int lastQuote = rawPathPart.LastIndexOf('"');

			if (firstQuote >= 0 && lastQuote > firstQuote) {
				string relativePath = rawPathPart.Substring(
						firstQuote + 1,
						lastQuote - firstQuote - 1
				);

				string fullPath = Path.GetFullPath(Path.Combine(solutionDir, relativePath));
				projectPaths.Add(fullPath);

				Console.WriteLine($"  Found project: {fullPath}");
			}
		}

		return projectPaths;
	}

	// ========================================================================
	// PROJECT OUTPUT GENERATION (FINAL, CLEAN)
	// ========================================================================
	private static void CreateOutputForProject(string projectPath, string timestamp, string dateFolder) {
		Console.WriteLine($"Processing project: {projectPath}");

		string? projectDir = Path.GetDirectoryName(projectPath);
		if (projectDir is null) {
			Console.WriteLine($"ERROR: Could not determine directory for project: {projectPath}");
			return;
		}

		string projectName = Path.GetFileNameWithoutExtension(projectPath);

		// FINAL: No projectName folder — clean structure
		string dumpRoot = Path.Combine(projectDir, "_StructureDump");
		Directory.CreateDirectory(dumpRoot);

		string datedFolder = Path.Combine(dumpRoot, dateFolder);
		Directory.CreateDirectory(datedFolder);

		string baseName = $"{projectName}.{timestamp}";

		// Load syntax trees
		var trees = RoslynLoader.LoadProjectSyntaxTrees(projectDir);

		// ====================================================================
		// RUN WALKERS
		// ====================================================================
		string architecture = RunArchitectureWalker(trees);
		string minimal = RunMinimalWalker(trees);
		string dependencies = RunDependencyWalker(trees);
		string inheritance = RunInheritanceWalker(trees);
		string namespaces = RunNamespaceWalker(trees);

		// ====================================================================
		// WRITE FILES
		// ====================================================================
		WriteFile(Path.Combine(datedFolder, $"{baseName}.architecture.txt"), architecture);
		WriteFile(Path.Combine(datedFolder, $"{baseName}.minimal.txt"), minimal);
		WriteFile(Path.Combine(datedFolder, $"{baseName}.dependencies.txt"), dependencies);
		WriteFile(Path.Combine(datedFolder, $"{baseName}.inheritance.txt"), inheritance);
		WriteFile(Path.Combine(datedFolder, $"{baseName}.namespaces.txt"), namespaces);

		Console.WriteLine($"  Output written to: {datedFolder}");
	}

	// ========================================================================
	// WALKER RUNNERS
	// ========================================================================
	private static string RunArchitectureWalker(List<SyntaxTree> trees) {
		var walker = new ArchitectureWalker();
		foreach (var t in trees)
			walker.Visit(t.GetRoot());
		return walker.GetOutput();
	}

	private static string RunMinimalWalker(List<SyntaxTree> trees) {
		var walker = new MinimalWalker();
		foreach (var t in trees)
			walker.Visit(t.GetRoot());
		return walker.GetOutput();
	}

	private static string RunDependencyWalker(List<SyntaxTree> trees) {
		var walker = new DependencyWalker();
		foreach (var t in trees)
			walker.Visit(t.GetRoot());
		return walker.GetOutput();
	}

	private static string RunInheritanceWalker(List<SyntaxTree> trees) {
		var walker = new InheritanceWalker();
		foreach (var t in trees)
			walker.Visit(t.GetRoot());
		return walker.GetOutput();
	}

	private static string RunNamespaceWalker(List<SyntaxTree> trees) {
		var walker = new NamespaceWalker();
		foreach (var t in trees)
			walker.Visit(t.GetRoot());
		return walker.GetOutput();
	}

	// ========================================================================
	// FILE WRITER
	// ========================================================================
	private static void WriteFile(string path, string content) {
		File.WriteAllText(path, content);
		Console.WriteLine($"    Wrote: {Path.GetFileName(path)}");
	}
}

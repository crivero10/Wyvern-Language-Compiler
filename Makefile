wyvern.exe: Driver.cs Scanner.cs Token.cs TokenCategory.cs
	mcs -out:wyvern.exe Driver.cs Scanner.cs Token.cs TokenCategory.cs Parser.cs SyntaxError.cs Nodes.cs SemanticAnalyzer.cs SemanticError.cs SymbolTable.cs FunctionTable.cs

clean:
	rm wyvern.exe

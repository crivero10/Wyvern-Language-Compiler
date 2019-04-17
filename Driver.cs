/*=======================================================================
  Wyvern compiler: Version 0.4
  Copyright (C) 2019 Carlos Rivero A01371368, ITESM CEM
========================================================================*/

using System;
using System.IO;
using System.Text;

namespace Wyvern {

    public class Driver {

        const string VERSION = "0.4";

        //-----------------------------------------------------------
        static readonly string[] ReleaseIncludes = {
            "Lexical analysis",
            "Syntactic analysis",
            "AST construction"
        };

        //-----------------------------------------------------------
        void PrintAppHeader() {
            Console.WriteLine("Wyvern compiler, version " + VERSION);
            Console.WriteLine("Copyright \u00A9 2019 by Carlos Rivero, ITESM CEM."
            );
            Console.WriteLine("This program has absolutely no warranty.");
        }

        //-----------------------------------------------------------
        void PrintReleaseIncludes() {
            Console.WriteLine("Included in this release:");
            foreach (var phase in ReleaseIncludes) {
                Console.WriteLine("   * " + phase);
            }
        }

        //-----------------------------------------------------------
        void Run(string[] args) {

            PrintAppHeader();
            Console.WriteLine();
            PrintReleaseIncludes();
            Console.WriteLine();

            if (args.Length != 1) {
                Console.Error.WriteLine(
                    "Please specify the name of the input file.");
                Environment.Exit(1);
            }

            try {            
                var inputPath = args[0];                
                var input = File.ReadAllText(inputPath);
                var parser = new Parser(new Scanner(input).Start().GetEnumerator());
                var program = parser.Program();
                Console.WriteLine("Syntax OK.");


                var semantic = new SemanticAnalyzer();
                semantic.Visit((dynamic) program);

                Console.WriteLine("Semantics OK.");
                Console.WriteLine();
                Console.WriteLine("Global Symbol Table");
                Console.WriteLine("============");
                foreach (var entry in semantic.globalScopeSymbolTable) {
                    Console.WriteLine(entry);                        
                }
                Console.WriteLine("Global Function Table");
                Console.WriteLine("============");
                foreach (var entry in semantic.globalScopeFunctionTable) {
                    Console.WriteLine(entry);                        
                }
                Console.WriteLine("Local Tables");
                Console.WriteLine("============");
                foreach (var entry in semantic.localSymbolTables) {
                    Console.WriteLine(entry);                        
                }

            } catch (Exception e) {

                if (e is FileNotFoundException 
                    || e is SyntaxError 
                    || e is SemanticError) {
                    Console.Error.WriteLine(e.Message);
                    Environment.Exit(1);
                }

                throw;
            }
        }

        //-----------------------------------------------------------
        public static void Main(string[] args) {
            new Driver().Run(args);
        }
    }
}

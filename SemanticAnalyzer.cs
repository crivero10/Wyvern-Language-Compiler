/*=======================================================================
  Wyvern compiler: Version 0.4
  Copyright (C) 2019 Carlos Rivero A01371368, ITESM CEM
========================================================================*/

using System;
using System.Text;
using System.Collections.Generic;

namespace Wyvern {
    class SemanticAnalyzer {
        public SymbolTable globalScopeSymbolTable;
        public FunctionTable globalScopeFunctionTable;
        public SymbolTable localScopeSymbolTable;

        //HashTable que guarda las tablas de variables locales por scope
        public IDictionary<string, SymbolTable> localSymbolTables = new SortedDictionary<string, SymbolTable>();
        public bool isFirstEncounter;
        public bool functionDepth;
        public int whileLoopDepth;
        
        //-----------------------------------------------------------
        public SemanticAnalyzer() {
            
            //Verifica que es la primera vez que se recorre
            isFirstEncounter = true;

            //Bandera que verifica si estamos adentro de una variable
            functionDepth = false;

            //Profundidad del ciclo while en cuestión - cuantos ciclos anidados hay?
            whileLoopDepth = 0;
            
            //Tablas de simbolos y funciones globales
            globalScopeSymbolTable = new SymbolTable();
            globalScopeFunctionTable = new FunctionTable();

            //Funciones pre-built del API de Wyvern y el número de parámetros que reciben.
            globalScopeFunctionTable["add"] = 2;
            globalScopeFunctionTable["get"] = 2;
            globalScopeFunctionTable["new"] = 1;
            globalScopeFunctionTable["printc"] = 1;
            globalScopeFunctionTable["printi"] = 1;
            globalScopeFunctionTable["println"] = 0;
            globalScopeFunctionTable["prints"] = 1;
            globalScopeFunctionTable["readi"] = 0;
            globalScopeFunctionTable["reads"] = 0;
            globalScopeFunctionTable["set"] = 3;
            globalScopeFunctionTable["size"] = 1;

            //Tabla temporal de variables locales para x scope
            localScopeSymbolTable = new SymbolTable();

            
        }
        
        public void Visit(ProgramNode node){
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
            if(!globalScopeFunctionTable.Contains("main")){
                throw new SemanticError("Could not found 'main' function declaration.");
            }
            isFirstEncounter = false;
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }
        public void Visit(IntLiteralNode node){
            var intStr = node.AnchorToken.Lexeme;
            if(intStr.Equals("false")){
                //Equivale a cero (único valor falso de Wyvern) 7u7
            }
            else{
                try {
                Convert.ToInt32(intStr);
                } 
                catch (OverflowException) {
                    throw new SemanticError("Integer overflow, this integer is too big: " + intStr, node.AnchorToken);
                }
            }
        }
        
        public void Visit(IdentifierNode node){
            var variableName = node.AnchorToken.Lexeme;
            if (!localScopeSymbolTable.Contains(variableName) && !globalScopeSymbolTable.Contains(variableName)) {
                throw new SemanticError("Could not found variable definition for identifier: " + variableName,node.AnchorToken);
            }
        }
        public void Visit(ArrNode node){
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }
        public void Visit(CharNode node){
        }

        public void Visit(VarDefNode node){
            foreach (var subnode in node[0]) {
                var variableName = subnode.AnchorToken.Lexeme;
                if(functionDepth){
                    if(localScopeSymbolTable.Contains(variableName)){
                        throw new SemanticError("Cannot duplicate variable declaration: " + variableName,subnode.AnchorToken);
                    }
                    localScopeSymbolTable.Add(variableName);
                }
                else{
                    if (isFirstEncounter){
                        if(globalScopeSymbolTable.Contains(variableName)){
                            throw new SemanticError("Cannot duplicate variable declaration: " + variableName,subnode.AnchorToken);    
                        } 
                        else{
                            globalScopeSymbolTable.Add(variableName);
                        }
                    }
                }
                foreach (var subnode_of_subnode /* uwu */ in subnode) {
                    Visit((dynamic) subnode_of_subnode);
                }
            }
        }
        
        public void Visit(VarDefListNode node){
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }

        public void Visit(StmtNode node){
            if(node.AnchorToken.Category == TokenCategory.BREAK && whileLoopDepth == 0){
                throw new SemanticError("Encountered break statement outside while loop declaration. ",node.AnchorToken);
            }
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }

        
        public void Visit(IdListNode node){
            if(!isFirstEncounter){
                foreach(var subnode in node){
                    var variableName = subnode.AnchorToken.Lexeme;
                    if(localScopeSymbolTable.Contains(variableName)){
                        throw new SemanticError("Duplicated variable declaration: " + variableName,node[0].AnchorToken);
                    }
                    else{
                        localScopeSymbolTable.Add(variableName);
                    }
                    foreach (var subnode_of_subnode /* owo */ in subnode) {
                        Visit((dynamic) subnode_of_subnode);
                    }
                }
            }
        }
        public void Visit(FunDefNode node){
            var functionName = node.AnchorToken.Lexeme;
            if(isFirstEncounter){
                if(globalScopeFunctionTable.Contains(functionName)){
                    throw new SemanticError("Duplicated function definition: " + functionName,node.AnchorToken);    
                }
                else{
                    if(node[0] is IdListNode){
                        var ndecount = 0; 
                        foreach (var subnode in node[0]){
                            ndecount++;
                        }
                        globalScopeFunctionTable[functionName] = ndecount;              
                    }
                    else{
                        globalScopeFunctionTable[functionName] = 0;                  
                    }
                }
            }
            else{
                localScopeSymbolTable = new SymbolTable();
                functionDepth = true;
                foreach (var subnode in node) {
                    Visit((dynamic) subnode);
                }
                localSymbolTables[functionName] = localScopeSymbolTable;
                functionDepth = false;
                localScopeSymbolTable = new SymbolTable();
            }
        }
        public void Visit(AssignmentNode node){
            var varName = node.AnchorToken.Lexeme;
            if(!localScopeSymbolTable.Contains(varName) && !globalScopeSymbolTable.Contains(varName)){
                throw new SemanticError("Undeclared variable: " + varName,node[0].AnchorToken);    
            }
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }

        public void Visit(FunCallNode node){
            var varName = node.AnchorToken.Lexeme;
            if(!globalScopeFunctionTable.Contains(varName)){
                throw new SemanticError("Undeclared function: " + varName,node.AnchorToken);    
            }
            else{
                var ndecount = 0;
                foreach (var subnode in node[0]){
                    ndecount++;
                }
                if(ndecount != globalScopeFunctionTable[varName]){
                    throw new SemanticError("Function called with different parameters as defined ",node.AnchorToken);    
                }
                foreach (var subnode in node) {
                    Visit((dynamic) subnode);
                }
            }
        }
        public void Visit(StmtListNode node){
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }
        
        public void Visit(DefNode node){
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }

        public void Visit(IfNode node){
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }
        public void Visit(ElseIfListNode node){
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }
        public void Visit(ElseIfNode node){
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }
        public void Visit(ElseNode node){
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }
        
        public void Visit(DecrementNode node){
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }

        public void Visit(ExprPrimaryNode node){
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }

        public void Visit(ExprCompNode node){
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }
        public void Visit(ExprListNode node){
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }
        public void Visit(ExprOrNode node){
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }
        public void Visit(ExprAndNode node){
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }

        public void Visit(WhileNode node){
            whileLoopDepth = whileLoopDepth + 1;
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
            whileLoopDepth = whileLoopDepth - 1;
        }
        public void Visit(ReturnNode node){
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }
        public void Visit(IncrementNode node){
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }
        
        public void Visit(ExprRelNode node){
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }
        public void Visit(ExprMulNode node){
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }
        public void Visit(ExprUnaryNode node){
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }

        public void Visit(ExprAddNode node){
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }

        public void Visit(StrNode node) {
            foreach (var subnode in node) {
                Visit((dynamic) subnode);
            }
        }
        

        public void Visit(TrueNode node){
            
        }

        public void Visit(FalseNode node){
            
        }
        
    }
}
/*=======================================================================
  Wyvern compiler: Version 0.2
  Copyright (C) 2019 Carlos Rivero A01371368, ITESM CEM
========================================================================*/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace Wyvern {

     class Parser{
        static readonly ISet<TokenCategory> firstOfStmt = new HashSet<TokenCategory>(){
            TokenCategory.IDENTIFIER,
            TokenCategory.IF,
            TokenCategory.WHILE,
            TokenCategory.BREAK,
            TokenCategory.RETURN,
            TokenCategory.SEMICOLON
        };

        static readonly ISet<TokenCategory> firstOfExprRel = new HashSet<TokenCategory>(){
            TokenCategory.LESS,
            TokenCategory.LESS_EQUAL,
            TokenCategory.GREATER,
            TokenCategory.GREATER_EQUAL
        };

        static readonly ISet<TokenCategory> firstOfExprMul = new HashSet<TokenCategory>(){
            TokenCategory.MUL,
            TokenCategory.MOD,
            TokenCategory.DIV
        };

        static readonly ISet<TokenCategory> firstOfExprUnary = new HashSet<TokenCategory>(){
            TokenCategory.PLUS,
            TokenCategory.NEG,
            TokenCategory.NOT
        };

        static readonly ISet<TokenCategory> firstOfExprPrimary = new HashSet<TokenCategory>(){
            TokenCategory.IDENTIFIER,
            TokenCategory.SQUARE_OPEN,
            TokenCategory.STRING_LITERAL,
            TokenCategory.CHAR_LITERAL,
            TokenCategory.INT_LITERAL,
            TokenCategory.PARENTHESIS_OPEN
        };

        static readonly ISet<TokenCategory> firstOfExpr = new HashSet<TokenCategory>(){
            TokenCategory.IDENTIFIER,
            TokenCategory.SQUARE_OPEN,
            TokenCategory.STRING_LITERAL,
            TokenCategory.CHAR_LITERAL,
            TokenCategory.INT_LITERAL,
            TokenCategory.PARENTHESIS_OPEN,
            TokenCategory.PLUS,
            TokenCategory.NEG,
            TokenCategory.NOT,
            TokenCategory.MUL,
            TokenCategory.MOD,
            TokenCategory.DIV,
            TokenCategory.LESS,
            TokenCategory.LESS_EQUAL,
            TokenCategory.GREATER,
            TokenCategory.GREATER_EQUAL,
            TokenCategory.NOTEQUAL,
            TokenCategory.EQUAL
        };

        IEnumerator<Token> tokenStream;

        public Parser(IEnumerator<Token> tokenStream) {
            this.tokenStream = tokenStream;
            this.tokenStream.MoveNext();
        }

        public TokenCategory CurrentToken {
            get { return tokenStream.Current.Category; }
        }

        public Token Expect(TokenCategory category) {
            if (CurrentToken == category) {
                Token current = tokenStream.Current;
                tokenStream.MoveNext();
                return current;
            }
            else {
                throw new SyntaxError(category, tokenStream.Current);
            }
        }

        public void FunCall(){
            Expect(TokenCategory.IDENTIFIER);
            Expect(TokenCategory.PARENTHESIS_OPEN);
            ExprList();
            Expect(TokenCategory.PARENTHESIS_CLOSE);
        }

        public void Program(){
            // <def>*
            while(CurrentToken != TokenCategory.EOF){
                Def();
            }
            Expect(TokenCategory.EOF);
        }

        public void Def(){
            //var ‹id-list› ; | ‹fun-def›
            if(CurrentToken == TokenCategory.VAR){
                VarDef();
            }else{
                FunDef();
            }
        }

        public void VarDef(){
            Expect(TokenCategory.VAR);
            IdList();
            Expect(TokenCategory.SEMICOLON);
        }

        public void IdList(){
            //‹id› (,‹id›)*
            Expect(TokenCategory.IDENTIFIER);
            while(CurrentToken == TokenCategory.COMMA){
                Expect(TokenCategory.COMMA);
                Expect(TokenCategory.IDENTIFIER);
            }
        }

        public void FunDef(){
            Expect(TokenCategory.IDENTIFIER);
            Expect(TokenCategory.PARENTHESIS_OPEN);
            if (CurrentToken != TokenCategory.PARENTHESIS_CLOSE){
                IdList();
            }
            Expect(TokenCategory.PARENTHESIS_CLOSE);
            Expect(TokenCategory.LLAVE_OPEN);
            while(CurrentToken == TokenCategory.VAR){
                VarDef();
            }
            StmtList();
            Expect(TokenCategory.LLAVE_CLOSE);
        }

        public void ElseIfList(){
            while (CurrentToken == TokenCategory.ELSEIF){
                Expect(TokenCategory.ELSEIF);
                Expect(TokenCategory.PARENTHESIS_OPEN);
                Expr();
                Expect(TokenCategory.PARENTHESIS_CLOSE);
                Expect(TokenCategory.LLAVE_OPEN);
                StmtList();
                Expect(TokenCategory.LLAVE_CLOSE);
            }
        }

        public void Else(){
            if (CurrentToken == TokenCategory.ELSE){
                Expect(TokenCategory.ELSE);
                Expect(TokenCategory.LLAVE_OPEN);
                StmtList();
                Expect(TokenCategory.LLAVE_CLOSE);
            }
        }

        public void Stmt(){
            switch (CurrentToken){
                case TokenCategory.IDENTIFIER:
                    Expect(TokenCategory.IDENTIFIER);
                    switch (CurrentToken){
                        case TokenCategory.ASSIGN:
                            Expect(TokenCategory.ASSIGN);
                            Expr();
                            Expect(TokenCategory.SEMICOLON);
                            break;
                        case TokenCategory.INCREMENT:
                            Expect(TokenCategory.INCREMENT);
                            Expect(TokenCategory.SEMICOLON);
                            break;
                        case TokenCategory.DECREMENT:
                            Expect(TokenCategory.DECREMENT);
                            Expect(TokenCategory.SEMICOLON);
                            break;
                        case TokenCategory.PARENTHESIS_OPEN:
                            Expect(TokenCategory.PARENTHESIS_OPEN);
                            ExprList();
                            Expect(TokenCategory.PARENTHESIS_CLOSE);
                            Expect(TokenCategory.SEMICOLON);
                            break;
                    }
                    break;
                case TokenCategory.IF:
                    Expect(TokenCategory.IF);
                    Expect(TokenCategory.PARENTHESIS_OPEN);
                    Expr();
                    Expect(TokenCategory.PARENTHESIS_CLOSE);
                    Expect(TokenCategory.LLAVE_OPEN);
                    StmtList();
                    Expect(TokenCategory.LLAVE_CLOSE);
                    ElseIfList();
                    Else();
                    break;
                case TokenCategory.WHILE:
                    Expect(TokenCategory.WHILE);
                    Expect(TokenCategory.PARENTHESIS_OPEN);
                    Expr();
                    Expect(TokenCategory.PARENTHESIS_CLOSE);
                    Expect(TokenCategory.LLAVE_OPEN);
                    StmtList();
                    Expect(TokenCategory.LLAVE_CLOSE);
                    break;
                case TokenCategory.BREAK:
                    Expect(TokenCategory.BREAK);
                    Expect(TokenCategory.SEMICOLON);
                    break;
                case TokenCategory.RETURN:
                    Expect(TokenCategory.RETURN);
                    Expr();
                    Expect(TokenCategory.SEMICOLON);
                    break;
                case TokenCategory.SEMICOLON:
                    Expect(TokenCategory.SEMICOLON);
                    break;
                default:
                    throw new SyntaxError(firstOfStmt,tokenStream.Current);
            }
        }

        public void Expr(){
            ExprAnd();
            while(CurrentToken == TokenCategory.OR){
                Expect(TokenCategory.OR);
                ExprAnd();
            }
        }

        public void ExprAnd(){
            ExprComp();
            while(CurrentToken == TokenCategory.AND){
                Expect(TokenCategory.AND);
                ExprComp();
            }
        }

        public void ExprComp(){
            ExprRel();
            while(CurrentToken == TokenCategory.EQUAL || CurrentToken == TokenCategory.NOTEQUAL){
                if(CurrentToken == TokenCategory.EQUAL){
                    Expect(TokenCategory.EQUAL);
                }else{
                    Expect(TokenCategory.NOTEQUAL);
                }
                ExprRel();
            }
        }

        public void ExprRel(){
            ExprAdd();
            while (firstOfExprRel.Contains(CurrentToken)){
                switch (CurrentToken){
                    case TokenCategory.LESS:
                        Expect(TokenCategory.LESS);
                        break;

                    case TokenCategory.LESS_EQUAL:
                        Expect(TokenCategory.LESS_EQUAL);
                        break;

                    case TokenCategory.GREATER:
                        Expect(TokenCategory.GREATER);
                        break;

                    case TokenCategory.GREATER_EQUAL:
                        Expect(TokenCategory.GREATER_EQUAL);
                        break;

                    default:
                        throw new SyntaxError(firstOfExprRel,tokenStream.Current);
                }
                ExprAdd();
            }
        }

        public void ExprAdd(){
            ExprMul();
            while (CurrentToken == TokenCategory.PLUS || CurrentToken == TokenCategory.NEG){
                if(CurrentToken == TokenCategory.NEG){
                    Expect(TokenCategory.NEG);
                }
                else{
                    Expect(TokenCategory.PLUS);
                }
                ExprMul();
            }
        }

        public void ExprMul(){
            ExprUnary();
            while (firstOfExprMul.Contains(CurrentToken)){
                switch (CurrentToken){
                    case TokenCategory.MUL:
                        Expect(TokenCategory.MUL);
                        break;

                    case TokenCategory.MOD:
                        Expect(TokenCategory.MOD);
                        break;

                    case TokenCategory.DIV:
                        Expect(TokenCategory.DIV);
                        break;

                    default:
                        throw new SyntaxError(firstOfExprMul,tokenStream.Current);
                }
                ExprUnary();
            }
        }

        public void ExprUnary(){
             if (firstOfExprUnary.Contains(CurrentToken)){
                while (firstOfExprUnary.Contains(CurrentToken)){
                    switch (CurrentToken){
                        case TokenCategory.PLUS:
                            Expect(TokenCategory.PLUS);
                            break;

                        case TokenCategory.NEG:
                            Expect(TokenCategory.NEG);
                            break;

                        case TokenCategory.NOT:
                            Expect(TokenCategory.NOT);
                            break;

                        default:
                            throw new SyntaxError(firstOfExprUnary,tokenStream.Current);
                    }
                    ExprUnary();
                }
            }
            else{
                ExprPrimary();
            }
        }

        public void ExprPrimary(){
            switch (CurrentToken){
                case TokenCategory.IDENTIFIER:

                    Expect(TokenCategory.IDENTIFIER);
                    if (CurrentToken == TokenCategory.PARENTHESIS_OPEN){
                        Expect(TokenCategory.PARENTHESIS_OPEN);
                        ExprList();
                        Expect(TokenCategory.PARENTHESIS_CLOSE);
                    }
                    break;
                case TokenCategory.SQUARE_OPEN:
                    Expect(TokenCategory.SQUARE_OPEN);
                    ExprList();
                    Expect(TokenCategory.SQUARE_CLOSE);
                    break;

                case TokenCategory.PARENTHESIS_OPEN:
                    Expect(TokenCategory.PARENTHESIS_OPEN);
                    Expr();
                    Expect(TokenCategory.PARENTHESIS_CLOSE);
                    break;
                case TokenCategory.STRING_LITERAL:
                    Expect(TokenCategory.STRING_LITERAL);
                    break;
                case TokenCategory.CHAR_LITERAL:
                    Expect(TokenCategory.CHAR_LITERAL);
                    break;
                case TokenCategory.INT_LITERAL:
                    Expect(TokenCategory.INT_LITERAL);
                    break;
                case TokenCategory.TRUE:
                    Expect(TokenCategory.TRUE);
                    break;
                case TokenCategory.FALSE:
                    Expect(TokenCategory.FALSE);
                    break;
                default:
                    throw new SyntaxError(firstOfExprPrimary,tokenStream.Current);
            }
        }

        public void ExprList(){
             if (firstOfExpr.Contains(CurrentToken)){
                Expr();
                while (CurrentToken == TokenCategory.COMMA){
                    Expect(TokenCategory.COMMA);
                    Expr();
                }
            }
        }

        public void StmtList(){
            while (firstOfStmt.Contains(CurrentToken)){
                Stmt();
            }
        }

    }
}

/*=======================================================================
  Wyvern compiler: Version 0.3
  Copyright (C) 2019 Carlos Rivero A01371368, ITESM CEM
========================================================================*/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace Wyvern {
     class Parser{
         static readonly ISet<TokenCategory> firstOfExprPrimary = new HashSet<TokenCategory>(){
            TokenCategory.IDENTIFIER,
            TokenCategory.SQUARE_OPEN,
            TokenCategory.STRING_LITERAL,
            TokenCategory.CHAR_LITERAL,
            TokenCategory.INT_LITERAL,
            TokenCategory.PARENTHESIS_OPEN
        };
        static readonly ISet<TokenCategory> firstOfStmt = new HashSet<TokenCategory>(){
            TokenCategory.IDENTIFIER,
            TokenCategory.IF,
            TokenCategory.WHILE,
            TokenCategory.BREAK,
            TokenCategory.RETURN,
            TokenCategory.SEMICOLON
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

        static readonly ISet<TokenCategory> firstOfExprRel = new HashSet<TokenCategory>(){
            TokenCategory.LESS,
            TokenCategory.LESS_EQUAL,
            TokenCategory.GREATER,
            TokenCategory.GREATER_EQUAL
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

        public Node Program(){ 
            var node = new ProgramNode();
            while(CurrentToken != TokenCategory.EOF){
                if(CurrentToken == TokenCategory.VAR){
                    node.Add(VarDef());
                }
                else{
                    node.Add(FunDef());
                }
            }
            Expect(TokenCategory.EOF);
            return node;
        }
        
        public Node Return(){
            var node = new ReturnNode() {
                AnchorToken = Expect(TokenCategory.RETURN)
            };
            node.Add(Expr());
            Expect(TokenCategory.SEMICOLON);
            return node;
        }

        public Node FunDef(){
            var node = new FunDefNode(){
                AnchorToken = Expect(TokenCategory.IDENTIFIER)
            };
            Expect(TokenCategory.PARENTHESIS_OPEN);
            node.Add(IdList());   
            Expect(TokenCategory.PARENTHESIS_CLOSE);
            Expect(TokenCategory.LLAVE_OPEN);
            node.Add(VarDefList());
            node.Add(StmtList());
            Expect(TokenCategory.LLAVE_CLOSE);
            return node;
        }
        
        public Node VarDefList(){
            var node = new VarDefListNode();
            while(CurrentToken == TokenCategory.VAR){
                node.Add(VarDef());
            }
            return node;
        }
        
        public Node VarDef(){
            var node = new VarDefNode(){
                AnchorToken = Expect(TokenCategory.VAR)
            };
            node.Add(IdList());
            Expect(TokenCategory.SEMICOLON);
            return node;
        }
        
        public Node IdList(){
            var node = new IdListNode();
            if(CurrentToken == TokenCategory.PARENTHESIS_CLOSE){
                return node;
            }
            node.Add(new IdentifierNode(){
                AnchorToken = Expect(TokenCategory.IDENTIFIER)
            });
            while(CurrentToken == TokenCategory.COMMA){
                Expect(TokenCategory.COMMA);
                node.Add(new IdentifierNode(){
                    AnchorToken = Expect(TokenCategory.IDENTIFIER)
                });
            }
            return node;
        }
        
        public Node StmtList(){
            var node = new StmtListNode();
            if(CurrentToken == TokenCategory.LLAVE_CLOSE){
                return node;
            }
            while (firstOfStmt.Contains(CurrentToken)){
                node.Add(Stmt());
            }
            return node;
        }

        public Node While(){
            var node  = new WhileNode(){
                AnchorToken = Expect(TokenCategory.WHILE)
            };
            Expect(TokenCategory.PARENTHESIS_OPEN);
            node.Add(Expr());
            Expect(TokenCategory.PARENTHESIS_CLOSE);
            Expect(TokenCategory.LLAVE_OPEN);
            if(CurrentToken != TokenCategory.LLAVE_CLOSE){
                node.Add(StmtList());   
            }
            Expect(TokenCategory.LLAVE_CLOSE);
            return node;
        }

        public Node If(){
            var node = new IfNode();
            Expect(TokenCategory.IF);
            Expect(TokenCategory.PARENTHESIS_OPEN);
            node.Add(Expr());
            Expect(TokenCategory.PARENTHESIS_CLOSE);
            Expect(TokenCategory.LLAVE_OPEN);             
            node.Add(StmtList());            
            Expect(TokenCategory.LLAVE_CLOSE);             
            node.Add(ElseIf());    
            node.Add(Else());    
            return node;
        }

        public Node ElseIf(){
            var node = new ElseIfListNode();
            while (CurrentToken == TokenCategory.ELSEIF){
                var node1 = new ElseIfNode();
                Expect(TokenCategory.ELSEIF);
                Expect(TokenCategory.PARENTHESIS_OPEN);
                node1.Add(Expr());
                Expect(TokenCategory.PARENTHESIS_CLOSE);
                Expect(TokenCategory.LLAVE_OPEN);
                node1.Add(StmtList());
                Expect(TokenCategory.LLAVE_CLOSE);
                node.Add(node1);
            }
            return node;
        }

        public Node Else(){
            var node = new ElseNode();
            if (CurrentToken == TokenCategory.ELSE){
                Expect(TokenCategory.ELSE);
                Expect(TokenCategory.LLAVE_OPEN);
                if(CurrentToken != TokenCategory.LLAVE_CLOSE){
                    node.Add(StmtList());    
                }
                Expect(TokenCategory.LLAVE_CLOSE);
            }
            return node;
        }

        public Node Stmt(){
            switch (CurrentToken){
                case TokenCategory.IDENTIFIER:
                    var idToken = Expect(TokenCategory.IDENTIFIER);
                    switch (CurrentToken){
                        case TokenCategory.ASSIGN:
                            Expect(TokenCategory.ASSIGN);
                            var ass = new AssignmentNode(){
                                AnchorToken = idToken
                            };
                            ass.Add(Expr());
                            Expect(TokenCategory.SEMICOLON);
                            return ass;

                        case TokenCategory.INCREMENT:
                            Expect(TokenCategory.INCREMENT);
                             
                            var inc = new IncrementNode(){
                                AnchorToken = idToken
                            };
                            Expect(TokenCategory.SEMICOLON);
                            return inc;

                        case TokenCategory.DECREMENT:
                            Expect(TokenCategory.DECREMENT);
                            var dec = new DecrementNode(){
                                AnchorToken = idToken
                            };
                            Expect(TokenCategory.SEMICOLON);
                            return dec;
                            
                        case TokenCategory.PARENTHESIS_OPEN:
                            Expect(TokenCategory.PARENTHESIS_OPEN);
                            var fun = new FunCallNode(){
                                AnchorToken = idToken
                            };
                             
                            fun.Add(ExprList());   
                             
                            Expect(TokenCategory.PARENTHESIS_CLOSE);
                            Expect(TokenCategory.SEMICOLON);
                            return fun;
                    }
                    break;
                case TokenCategory.IF:
                    return If();
                case TokenCategory.WHILE:
                    return While();
                case TokenCategory.BREAK:
                    var bre = new StmtNode(){
                        AnchorToken = Expect(TokenCategory.BREAK)
                    };
                    Expect(TokenCategory.SEMICOLON);
                    return bre;

                case TokenCategory.RETURN:
                    return Return();

                case TokenCategory.SEMICOLON:
                    return new StmtNode(){
                        AnchorToken = Expect(TokenCategory.SEMICOLON)
                    };
                default:
                    throw new SyntaxError(firstOfStmt,tokenStream.Current);
            }
            throw new SyntaxError(firstOfStmt,tokenStream.Current);
        }

        public Node Expr(){             
            var node = ExprAnd();
            while(CurrentToken == TokenCategory.OR){
                var node1 = new ExprOrNode(){
                    AnchorToken = Expect(TokenCategory.OR)
                };
                node1.Add(node);
                node1.Add(ExprAnd());
                node = node1;
            }
            return node;
        }
        
        public Node ExprAnd(){             
            var node1 = ExprComp();
            while(CurrentToken == TokenCategory.AND){
                var node2 = new ExprAndNode(){
                    AnchorToken = Expect(TokenCategory.AND)
                };
                node2.Add(node1);
                node2.Add(ExprComp());
                node1 = node2;
            }
            return node1;
        }
        
        public Node ExprComp(){             
            var node1 = ExprRel();
            while(CurrentToken == TokenCategory.EQUAL || CurrentToken == TokenCategory.NOTEQUAL){
                var node2 = new ExprCompNode();
                if(CurrentToken == TokenCategory.EQUAL){
                    node2.AnchorToken = Expect(TokenCategory.EQUAL);
                }
                else{
                    node2.AnchorToken = Expect(TokenCategory.NOTEQUAL);
                }
                node2.Add(node1);
                node2.Add(ExprRel());
                node1 = node2;
            }
            return node1;
        }
        
        public Node ExprList(){
            var node = new ExprListNode();
            if(CurrentToken == TokenCategory.PARENTHESIS_CLOSE || CurrentToken == TokenCategory.SQUARE_CLOSE){
                return node;
            }
            node.Add(Expr());
            while (CurrentToken == TokenCategory.COMMA){
                Expect(TokenCategory.COMMA);
                node.Add(Expr());
            }
            return node;
        }
        
        public Node ExprRel(){             
            var node1 = ExprAdd();
            while (firstOfExprRel.Contains(CurrentToken)){
                var node2 = new ExprRelNode();
                switch (CurrentToken){
                    case TokenCategory.LESS:
                        node2.AnchorToken = Expect(TokenCategory.LESS);
                        break;

                    case TokenCategory.LESS_EQUAL:
                        node2.AnchorToken = Expect(TokenCategory.LESS_EQUAL);
                        break;

                    case TokenCategory.GREATER:
                        node2.AnchorToken = Expect(TokenCategory.GREATER);
                        break;

                    case TokenCategory.GREATER_EQUAL:
                        node2.AnchorToken = Expect(TokenCategory.GREATER_EQUAL);
                        break;

                    default:
                        throw new SyntaxError(firstOfExprRel,tokenStream.Current);
                }
                node2.Add(node1);
                node2.Add(ExprAdd());
                node1 = node2;
            }
            return node1;
        }
        
        public Node ExprAdd(){
            var node1 = ExprMul();
            while (CurrentToken == TokenCategory.PLUS || CurrentToken == TokenCategory.NEG){
                var node2 = new ExprAddNode();
                if(CurrentToken == TokenCategory.NEG){
                    node2.AnchorToken = Expect(TokenCategory.NEG);    
                }
                else{
                    node2.AnchorToken = Expect(TokenCategory.PLUS);
                }
                node2.Add(node1);
                node2.Add(ExprMul());
                node1 = node2;
            }
            return node1;
        }
        
        public Node ExprUnary(){
            var top = new ExprUnaryNode();
            var temp = top;  
             if (firstOfExprUnary.Contains(CurrentToken)){
                while (firstOfExprUnary.Contains(CurrentToken)){
                    switch (CurrentToken){
                        case TokenCategory.PLUS:
                            temp.AnchorToken = Expect(TokenCategory.PLUS);
                            break;
                        case TokenCategory.NEG:
                            temp.AnchorToken = Expect(TokenCategory.NEG);
                            break;
                        case TokenCategory.NOT:
                            temp.AnchorToken = Expect(TokenCategory.NOT);
                            break;
                        default:
                            throw new SyntaxError(firstOfExprUnary,tokenStream.Current);
                    }
                    if (!firstOfExprUnary.Contains(CurrentToken)){
                        temp.Add(ExprPrimary());
                    }
                    else{
                        var newNode = new ExprUnaryNode();  
                        temp.Add(newNode);
                        temp = newNode;
                    }                    
                }                
            }
            else{
                return ExprPrimary();  
            }
            return top;  
        }

        public Node ExprPrimary(){
            switch (CurrentToken){
                case TokenCategory.IDENTIFIER:  
                    var idToken = Expect(TokenCategory.IDENTIFIER);
                    if (CurrentToken == TokenCategory.PARENTHESIS_OPEN){
                        Expect(TokenCategory.PARENTHESIS_OPEN);
                        var fun = new FunCallNode(){
                            AnchorToken = idToken
                        };
                        fun.Add(ExprList());
                        Expect(TokenCategory.PARENTHESIS_CLOSE);
                        return fun;
                    }
                    else{
                        var id =  new IdentifierNode(){
                            AnchorToken = idToken
                        };
                        return id;
                    }
                case TokenCategory.SQUARE_OPEN:  
                    return Arr();

                case TokenCategory.PARENTHESIS_OPEN:  
                    Expect(TokenCategory.PARENTHESIS_OPEN);
                    var node = Expr();
                    Expect(TokenCategory.PARENTHESIS_CLOSE);
                    return node;
                    
                case TokenCategory.STRING_LITERAL:
                    return new StrNode(){
                        AnchorToken = Expect(TokenCategory.STRING_LITERAL)
                    };
    
                case TokenCategory.CHAR_LITERAL:
                    return new CharNode(){
                        AnchorToken = Expect(TokenCategory.CHAR_LITERAL)
                    };
    
                case TokenCategory.INT_LITERAL:
                    return new IntLiteralNode(){
                        AnchorToken = Expect(TokenCategory.INT_LITERAL)
                    };
                
                case TokenCategory.TRUE:
                    return new TrueNode(){
                        AnchorToken = Expect(TokenCategory.TRUE)
                    };

                case TokenCategory.FALSE:
                    return new IntLiteralNode(){
                        AnchorToken = Expect(TokenCategory.FALSE)
                    };                   
                default:
                    throw new SyntaxError(firstOfExprPrimary,tokenStream.Current);
            }
        }

        public Node ExprMul(){
            var node1 = ExprUnary();
            while (firstOfExprMul.Contains(CurrentToken)){
                var node2 = new ExprMulNode();
                switch (CurrentToken){
                    case TokenCategory.MUL:
                        node2.AnchorToken = Expect(TokenCategory.MUL);
                        break;
                    case TokenCategory.MOD:
                        node2.AnchorToken = Expect(TokenCategory.MOD);
                        break;
                    case TokenCategory.DIV:
                        node2.AnchorToken = Expect(TokenCategory.DIV);
                        break;
                    default:
                        throw new SyntaxError(firstOfExprMul,tokenStream.Current);
                }
                node2.Add(node1);
                node2.Add(ExprUnary());
                node1 = node2;
            }
            return node1;
        }
        
        public Node Arr(){
            var node = new ArrNode();
            Expect(TokenCategory.SQUARE_OPEN);
            node.Add(ExprList());
            Expect(TokenCategory.SQUARE_CLOSE);
            return node;
        }
    
    }
}
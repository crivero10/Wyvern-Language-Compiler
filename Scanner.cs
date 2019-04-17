/*=======================================================================
  Wyvern compiler: Version 0.1
  Copyright (C) 2019 Carlos Rivero A01371368, ITESM CEM
========================================================================*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Wyvern {

    class Scanner {

        readonly string input;

        static readonly Regex regex = new Regex(
            @"
                (?<And>               &&        )
              | (?<Comment>           (\/\*(\*(?!\/)|[^*])*\*\/)|(\/\/[^\n\r]+?(?:\*\)|[\n\r])) )
              | (?<NotEqual>          !=        )
              | (?<Equal>             ==        )
              | (?<Increment>         \+\+        )
              | (?<Decrement>         \-\-        )
              | (?<Assign>            [=]       )
              | (?<Or>                \|\|      )
              | (?<Not>               \!        )
              | (?<Less>              [<]       )
              | (?<Greater>           [>]       )
              | (?<GreaterEqual>      >=        )
              | (?<LessEqual>         <=        )
              | (?<StringLiteral>     "".*""    )
              | (?<CharLiteral>       '([^\n\\']|\\n|\\r|\\t|\\'|\\\\|\\""|\\u[0-9a-fA-F]{6})'       )
              | (?<Identifier>        [a-zA-Z_]+ )
              | (?<IntLiteral>        \d+       )
              | (?<Mul>               [*]       )
              | (?<Plus>              [+]       )
              | (?<Neg>               [-]       )
              | (?<Div>               [/]       )
              | (?<Mod>               [%]       )
              | (?<Newline>           \n        )
              | (?<ParLeft>           [(]       )
              | (?<ParRight>          [)]       )
              | (?<SquareLeft>        [\[]      )
              | (?<SquareRight>       [\]]       )
              | (?<LlaveLeft>         [\{]       )
              | (?<LlaveRight>        [\}]       )
              | (?<Comma>             [,]       )
              | (?<Semicolon>         [;]       )
              | (?<WhiteSpace> \s        )     # Must go anywhere after Newline.
              | (?<Other>      .         )     # Must be last: match any other character.
            ",
            RegexOptions.IgnorePatternWhitespace
                | RegexOptions.Compiled
                | RegexOptions.Multiline
            );

        static readonly IDictionary<string, TokenCategory> keywords =
            new Dictionary<string, TokenCategory>() {
                {"break", TokenCategory.BREAK},
                {"if", TokenCategory.IF},
                {"else", TokenCategory.ELSE},
                {"elseif", TokenCategory.ELSEIF},
                {"false", TokenCategory.FALSE},
                {"return", TokenCategory.RETURN},
                {"true", TokenCategory.TRUE},
                {"var", TokenCategory. VAR},
                {"while", TokenCategory.WHILE}
            };

        static readonly IDictionary<string, TokenCategory> nonKeywords =
            new Dictionary<string, TokenCategory>() {

                {"Comment", TokenCategory.COMMENT},
                {"Comma", TokenCategory.COMMA},
                {"Semicolon", TokenCategory.SEMICOLON},

                //Literals
                {"StringLiteral", TokenCategory.STRING_LITERAL},
                {"CharLiteral", TokenCategory.CHAR_LITERAL},
                {"IntLiteral", TokenCategory.INT_LITERAL},

                //Arithmetic operators
                {"Div", TokenCategory.DIV},
                {"Mul", TokenCategory.MUL},
                {"Mod", TokenCategory.MOD},
                {"Increment", TokenCategory.INCREMENT},
                {"Decrement", TokenCategory.DECREMENT},
                {"Plus", TokenCategory.PLUS},
                {"Neg", TokenCategory.NEG},

                //Logical operators
                {"And", TokenCategory.AND},
                {"Not", TokenCategory.NOT},
                {"Or", TokenCategory.OR},

                //Relational operators
                {"Equal", TokenCategory.EQUAL},
                {"NotEqual", TokenCategory.NOTEQUAL},
                {"Less", TokenCategory.LESS},
                {"Greater", TokenCategory.GREATER},
                {"GreaterEqual", TokenCategory.GREATER_EQUAL},
                {"LessEqual", TokenCategory.LESS_EQUAL},

                //Brackets
                {"ParLeft", TokenCategory.PARENTHESIS_OPEN},
                {"ParRight", TokenCategory.PARENTHESIS_CLOSE},
                {"SquareLeft", TokenCategory.SQUARE_OPEN},
                {"SquareRight", TokenCategory.SQUARE_CLOSE},
                {"LlaveLeft", TokenCategory.LLAVE_OPEN},
                {"LlaveRight", TokenCategory.LLAVE_CLOSE},

                //Others
                {"Assign", TokenCategory.ASSIGN},
                {"Identifier", TokenCategory.IDENTIFIER},


            };

        public Scanner(string input) {
            this.input = input;
        }

        public IEnumerable<Token> Start() {

            var row = 1;
            var columnStart = 0;

            Func<Match, TokenCategory, Token> newTok = (m, tc) =>
                new Token(m.Value, tc, row, m.Index - columnStart + 1);

            var matches = regex.Matches(input);
            foreach (Match m in matches) {

                if (m.Groups["Newline"].Success) {

                    // Found a new line.
                    row++;
                    columnStart = m.Index + m.Length;

                } else if (m.Groups["WhiteSpace"].Success){

                    // Skip white space +.

                } else if (m.Groups["Identifier"].Success) {

                    if (keywords.ContainsKey(m.Value)) {

                        // Matched string is a Wyvern keyword.
                        yield return newTok(m, keywords[m.Value]);

                    } else {

                        // Otherwise it's just a plain identifier.
                        yield return newTok(m, TokenCategory.IDENTIFIER);
                    }

                } else if(m.Groups["Comment"].Success){
                    var comment_token = newTok(m, TokenCategory.COMMENT);
                    row = row + Regex.Matches(m.Groups["Comment"].Value, "\n").Count;
                    columnStart = m.Index + m.Length;
                    //yield return comment_token;

                } else if (m.Groups["Other"].Success) {

                    // Found an illegal character.
                    yield return newTok(m, TokenCategory.ILLEGAL_CHAR);

                } else {

                    // Match must be one of the non keywords.
                    foreach (var name in nonKeywords.Keys) {

                        if (m.Groups[name].Success) {
                            yield return newTok(m, nonKeywords[name]);
                            break;
                        }
                    }
                }
            }

            yield return new Token(null,
                                   TokenCategory.EOF,
                                   row,
                                   input.Length - columnStart + 1);
        }
    }
}

/*=======================================================================
  Wyvern compiler: Version 0.1
  Copyright (C) 2019 Carlos Rivero A01371368, ITESM CEM
========================================================================*/

namespace Wyvern {

    enum TokenCategory {
        //Keywords-------------------------------------------------------

        BREAK,
        ELSEIF,
        ELSE,
        FALSE,
        IF,
        RETURN,
        TRUE,
        VAR,
        WHILE,

        //---------------------------------------------------------------

        //Non-keywords--------------------------------------------------

        //Literals
        STRING_LITERAL,
        CHAR_LITERAL,
        INT_LITERAL,

        //Arithmetic operators
        MUL,
        DIV,
        MOD,
        INCREMENT,
        DECREMENT,
        PLUS,
        NEG,

        //Logical operators
        AND,
        NOT,
        OR,

        //Relational operators
        EQUAL,
        NOTEQUAL,
        LESS,
        GREATER,
        GREATER_EQUAL,
        LESS_EQUAL,

        //Brackets

        PARENTHESIS_OPEN,
        PARENTHESIS_CLOSE,
        SQUARE_OPEN,
        SQUARE_CLOSE,
        LLAVE_OPEN,
        LLAVE_CLOSE,

        //Others
        ASSIGN,
        EOF,
        IDENTIFIER,
        COMMENT,
        COMMA,
        SEMICOLON,

        ILLEGAL_CHAR

        //---------------------------------------------------------------
    }
}

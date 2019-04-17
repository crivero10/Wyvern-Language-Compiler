/*=======================================================================
  Wyvern compiler: Version 0.4
  Copyright (C) 2019 Carlos Rivero A01371368, ITESM CEM
========================================================================*/

using System;
using System.Text;
using System.Collections.Generic;

using System;

namespace Wyvern {
    class SemanticError: Exception {
        public SemanticError(string message, Token token):
            base(String.Format("Semantic Error: {0} \n" +"at row {1}, column {2}.",message,token.Row,token.Column)){
        }
        public SemanticError(string message):
            base(String.Format("Semantic Error: {0} \n",message)){
        }
    }
}

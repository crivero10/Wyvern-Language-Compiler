/*=======================================================================
  Wyvern compiler: Version 0.4
  Copyright (C) 2019 Carlos Rivero A01371368, ITESM CEM
========================================================================*/

using System;
using System.Text;
using System.Collections.Generic;

namespace Wyvern{
    public class SymbolTable {
        public HashSet<string> data;
        
        public SymbolTable(){
            data = new HashSet<string>();
        }
            
        //-----------------------------------------------------------
        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("Symbol Table:\n");
            //sb.Append("====================\n");
            foreach (var entry in data) {
                sb.Append(String.Format("- {0} \n", entry));
            }
            //sb.Append("====================\n");
            return sb.ToString();
        }
        
        public void Add(string nvalue){
            data.Add(nvalue);
        }

        //-----------------------------------------------------------
        public bool Contains(string key) {
            return data.Contains(key);
        }

        //-----------------------------------------------------------
        public IEnumerator<string> GetEnumerator() {
            return data.GetEnumerator();
        }

        //-----------------------------------------------------------
        /*
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }
        */
    }
}
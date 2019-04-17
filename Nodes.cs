/*=======================================================================
  Wyvern compiler: Version 0.4
  Copyright (C) 2019 Carlos Rivero A01371368, ITESM CEM
========================================================================*/

using System; 
using System.Collections.Generic;
using System.Text;

namespace Wyvern {   
    
	//Base class
	class Node: IEnumerable<Node> {
        IList<Node> children = new List<Node>();
        String treeTag = "Generic Node";

        public Node this[int index] {
            get {
                return children[index];
            }
        }

        public Token AnchorToken { get; set; }

        public void Add(Node node) {
            children.Add(node);
        }

        public IEnumerator<Node> GetEnumerator() {
            return children.GetEnumerator();
        }

        System.Collections.IEnumerator
                System.Collections.IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }

        public override string ToString() {
            return String.Format("{0} {1}", GetType().Name, AnchorToken);                                 
        }

        public string ToStringTree() {
            var sb = new StringBuilder();
            TreeTraversal(this, "", sb);
            return sb.ToString();
        }

        static void TreeTraversal(Node node, string indent, StringBuilder sb) {
            sb.Append(indent);
            sb.Append(node);
            sb.Append('\n');
            foreach (var child in node.children) {
                TreeTraversal(child, indent + "  ", sb);
            }
        }
    }

    //Nodes
    class VarDefListNode: Node{
        
    }
    class ArrNode: Node{

    }
    class ProgramNode: Node {

    }

    class DefNode: Node {

    }
    class CharNode: Node {

    }
    class VarDefNode: Node {

    }
    class IdListNode: Node {

    }
    class IdentifierNode: Node {

    }
    class IfNode:Node{

    }
    class FunDefNode: Node {

    }
    class ElseIfListNode: Node {

    }
    class ElseIfNode: Node {

    }
    class ElseNode: Node {

    }
    class StmtListNode: Node {

    }
    class WhileNode: Node {

    }
    class ReturnNode: Node {

    }
    class AssignmentNode: Node {

    }
    class IncrementNode: Node {

    }
    class IntLiteralNode: Node{

    }
    class DecrementNode: Node {

    }
    class FunCallNode: Node {

    }
    class StmtNode: Node{

    }
    class ExprListNode: Node {

    }
    class ExprOrNode: Node {

    }
    class ExprAndNode: Node {

    }
    class ExprAddNode: Node {

    }
    class ExprCompNode: Node {

    }
    class ExprRelNode: Node {

    }
    class ExprMulNode: Node {

    }
    class ExprUnaryNode: Node {

    }
    class StrNode: Node {

    }

    class TrueNode: Node {

    }

    class FalseNode: Node {

    }

    class ExprPrimaryNode : Node{

    }

}
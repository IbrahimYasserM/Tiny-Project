using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tiny_Compiler;

namespace Tiny_Compiler
{
    public class Node
    {
        public List<Node> Children = new List<Node>();

        public string Name;
        public Node(string N)
        {
            this.Name = N;
        }
    }

    public class Parser
    {
        int InputPointer = 0;
        List<Token> TokenStream;
        public Node root;

        public Node StartParsing(List<Token> TokenStream)
        {
            this.InputPointer = 0;
            this.TokenStream = TokenStream;
            root = new Node("Program");
            return root;
        }

        Node Identifiers()
        {
            Node identifiers = new Node("Identifiers");
            if(InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                identifiers.Children.Add(match(Token_Class.Comma));
                identifiers.Children.Add(match(Token_Class.Identifier));
                identifiers.Children.Add(Identifiers());
            }
            return identifiers;
        }

        Node Argument_List()
        {
            Node argumentList = new Node("Argument_List");
            if(InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Identifier)
            {
                argumentList.Children.Add(match(Token_Class.Identifier));
                argumentList.Children.Add(Identifiers());
            }
            return argumentList;
        }

        Node Function_Call()
        {
            Node functionCall = new Node("Function_Call");
            functionCall.Children.Add(match(Token_Class.Identifier));
            functionCall.Children.Add(match(Token_Class.LeftPrant));
            functionCall.Children.Add(Argument_List());
            functionCall.Children.Add(match(Token_Class.RightPrant));
            return functionCall;
        }
        
        Node Identifier_Term()
        {
            Node identifierTerm = new Node("Identifier_Term");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.LeftPrant)
            {
                identifierTerm.Children.Add(match(Token_Class.LeftPrant));
                identifierTerm.Children.Add(Argument_List());
                identifierTerm.Children.Add(match(Token_Class.RightPrant));
            }
            return identifierTerm;
        }

        Node Term()
        {
            Node term = new Node("Term");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Identifier)
            {
                term.Children.Add(match(Token_Class.Identifier));
                term.Children.Add(Identifier_Term());
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.ConstantNumber)
                term.Children.Add(match(Token_Class.ConstantNumber));
            else
                Errors.Error_List.Add("Parsing Error: Expected a Term\r\n");
            return term;
        }

        Node Equation()
        {
            Node equation = new Node("Equation");
            if(InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.LeftPrant)
            {
                equation.Children.Add(match(Token_Class.LeftPrant));
                equation.Children.Add(Equation());
                equation.Children.Add(match(Token_Class.RightPrant));
            }
            else
            {
                equation.Children.Add(Term());
                equation.Children.Add(Other_Equation());
            }
            return equation;
        }

        Node Other_Equation()
        {
            Node otherEquation = new Node("Other_Equation");
            if(InputPointer < TokenStream.Count && 
                (TokenStream[InputPointer].token_type == Token_Class.PlusOp ||
                TokenStream[InputPointer].token_type == Token_Class.MinusOp ||
                TokenStream[InputPointer].token_type == Token_Class.MultiplyOp ||
                TokenStream[InputPointer].token_type == Token_Class.DivideOp))
            {
                otherEquation.Children.Add(match(TokenStream[InputPointer].token_type));
                otherEquation.Children.Add(Equation());
            }
            return otherEquation;
        }

        Node Expression()
        {
            Node expression = new Node("Expression");
            if(InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.ConstantString)
                expression.Children.Add(match(Token_Class.ConstantString));
            else
                expression.Children.Add(Equation());
            return expression;
        }

        Node Assignment_Statement()
        {
            Node assignmentStatement = new Node("Assignment_Statement");
            assignmentStatement.Children.Add(match(Token_Class.Identifier));
            assignmentStatement.Children.Add(match(Token_Class.AssignOp));
            assignmentStatement.Children.Add(Expression());
            assignmentStatement.Children.Add(match(Token_Class.Semicolon));
            return assignmentStatement;
        }

        Node Datatype()
        {
            Node datatype = new Node("Datatype");
            if (InputPointer < TokenStream.Count &&
                (TokenStream[InputPointer].token_type == Token_Class.Int ||
                TokenStream[InputPointer].token_type == Token_Class.String ||
                TokenStream[InputPointer].token_type == Token_Class.Float))
                datatype.Children.Add(match(TokenStream[InputPointer].token_type));
            else
                Errors.Error_List.Add("Parsing Error: Expected Datatype\r\n");
            return datatype;
        }

        Node Declaration_Statement()
        {
            Node declarationStatement = new Node("Declaration_Statement");
            declarationStatement.Children.Add(Datatype());
            declarationStatement.Children.Add(match(Token_Class.Identifier));
            declarationStatement.Children.Add(Identifiers());
            declarationStatement.Children.Add(match(Token_Class.Semicolon));
            return declarationStatement;
        }

        Node Write_Statement()
        {
            Node writeStatement = new Node("Write_Statement");
            writeStatement.Children.Add(match(Token_Class.Write));
            writeStatement.Children.Add(Expression());
            writeStatement.Children.Add(match(Token_Class.Semicolon));
            return writeStatement;
        }

        Node Read_Statement()
        {
            Node readStatement = new Node("Read_Statement");
            readStatement.Children.Add(match(Token_Class.Read));
            readStatement.Children.Add(match(Token_Class.Identifier));
            readStatement.Children.Add(match(Token_Class.Semicolon));
            return readStatement;
        }

        Node Return_Statement()
        {
            Node returnStatement = new Node("Return_Statement");
            returnStatement.Children.Add(match(Token_Class.Return));
            returnStatement.Children.Add(Expression());
            returnStatement.Children.Add(match(Token_Class.Semicolon));
            return returnStatement;
        }

        Node Condition()
        {
            Node condition = new Node("Condition");
            condition.Children.Add(match(Token_Class.Identifier));
            if (InputPointer < TokenStream.Count &&
                (TokenStream[InputPointer].token_type == Token_Class.LessThanOp ||
                TokenStream[InputPointer].token_type == Token_Class.GreaterThanOp ||
                TokenStream[InputPointer].token_type == Token_Class.EqualOp ||
                TokenStream[InputPointer].token_type == Token_Class.NotEqualOp))
                condition.Children.Add(match(TokenStream[InputPointer].token_type));
            else
                Errors.Error_List.Add("Parsing Error: Expected Condition Operator\r\n");
            condition.Children.Add(Term());
            return condition;
        }

        Node Other_Conditions()
        {
            Node otherConditions = new Node("Other_Conditions");
            if (InputPointer < TokenStream.Count &&
                (TokenStream[InputPointer].token_type == Token_Class.AndOp ||
                TokenStream[InputPointer].token_type == Token_Class.OrOp))
            {
                otherConditions.Children.Add(match(TokenStream[InputPointer].token_type));
                otherConditions.Children.Add(Condition_Statement());
            }
            return otherConditions;
        }

        Node Condition_Statement()
        {
            Node conditionStatement = new Node("Condition_Statement");
            conditionStatement.Children.Add(Condition());
            conditionStatement.Children.Add(Other_Conditions());
            return conditionStatement;
        }

        public Node match(Token_Class ExpectedToken)
        {

            if (InputPointer < TokenStream.Count)
            {
                if (ExpectedToken == TokenStream[InputPointer].token_type)
                {
                    InputPointer++;
                    Node newNode = new Node(ExpectedToken.ToString());

                    return newNode;

                }

                else
                {
                    Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString() + " and " +
                        TokenStream[InputPointer].token_type.ToString() +
                        "  found\r\n");
                    InputPointer++;
                    return null;
                }
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString() + "\r\n");
                InputPointer++;
                return null;
            }
        }

        public static TreeNode PrintParseTree(Node root)
        {
            TreeNode tree = new TreeNode("Parse Tree");
            TreeNode treeRoot = PrintTree(root);
            if (treeRoot != null)
                tree.Nodes.Add(treeRoot);
            return tree;
        }
        static TreeNode PrintTree(Node root)
        {
            if (root == null || root.Name == null)
                return null;
            TreeNode tree = new TreeNode(root.Name);
            if (root.Children.Count == 0)
                return tree;
            foreach (Node child in root.Children)
            {
                if (child == null)
                    continue;
                tree.Nodes.Add(PrintTree(child));
            }
            return tree;
        }
    }
}

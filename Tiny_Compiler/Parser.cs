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
            root.Children.Add(Program());
            return root;
        }

        Node Other_Arguments()
        {
            Node identifiers = new Node("Identifiers");
            if(InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                identifiers.Children.Add(match(Token_Class.Comma));
                identifiers.Children.Add(Expression());
                identifiers.Children.Add(Other_Arguments());
            }
            return identifiers;
        }

        Node Argument_List()
        {
            Node argumentList = new Node("Argument_List");
            if(InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type != Token_Class.RightPrant)
            {
                argumentList.Children.Add(Expression());
                argumentList.Children.Add(Other_Arguments());
            }
            return argumentList;
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
                equation.Children.Add(Other_Equation());
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

        Node Assignment_Declaration()
        {
            Node assignmentDeclaration = new Node("Assignment_Declaration");
            if(InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.AssignOp)
            {
                assignmentDeclaration.Children.Add(match(Token_Class.AssignOp));
                assignmentDeclaration.Children.Add(Expression());
            }
            return assignmentDeclaration;
        }

        Node Other_Declarations()
        {
            Node otherDeclarations = new Node("Other_Declarations");
            if(InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                otherDeclarations.Children.Add(match(Token_Class.Comma));
                otherDeclarations.Children.Add(match(Token_Class.Identifier));
                otherDeclarations.Children.Add(Assignment_Declaration());
                otherDeclarations.Children.Add(Other_Declarations());
            }
            return otherDeclarations;
        }

        Node Declaration_Statement()
        {
            Node declarationStatement = new Node("Declaration_Statement");
            declarationStatement.Children.Add(Datatype());
            declarationStatement.Children.Add(match(Token_Class.Identifier));
            declarationStatement.Children.Add(Assignment_Declaration());
            declarationStatement.Children.Add(Other_Declarations());
            declarationStatement.Children.Add(match(Token_Class.Semicolon));
            return declarationStatement;
        }

        Node Write_Expression()
        {
            Node writeExpression = new Node("Write_Expression");
            if(InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.Endl)
                writeExpression.Children.Add(match(Token_Class.Endl));
            else
                    writeExpression.Children.Add(Expression());
            return writeExpression;
        }

        Node Write_Statement()
        {
            Node writeStatement = new Node("Write_Statement");
            writeStatement.Children.Add(match(Token_Class.Write));
            writeStatement.Children.Add(Write_Expression());
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

        Node Else_Statement()
        {
            Node elseStatement = new Node("Else_Statement");
            elseStatement.Children.Add(match(Token_Class.Else));
            elseStatement.Children.Add(Statements());
            elseStatement.Children.Add(match(Token_Class.End));
            return elseStatement;
        }

        Node Else_If_Statement()
        {
            Node elseIfStatement = new Node("Else_If_Statement");
            elseIfStatement.Children.Add(match(Token_Class.ElseIf));
            elseIfStatement.Children.Add(Condition_Statement());
            elseIfStatement.Children.Add(match(Token_Class.Then));
            elseIfStatement.Children.Add(Statements());
            elseIfStatement.Children.Add(Rest_If());
            return elseIfStatement;
        }

        Node Rest_If()
        {
            Node restIf = new Node("Rest_If");
            if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.ElseIf)
                restIf.Children.Add(Else_If_Statement());
            else if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.Else)
                restIf.Children.Add(Else_Statement());
            else if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.End)
                restIf.Children.Add(match(Token_Class.End));
            else
                Errors.Error_List.Add("Parsing Error: Expected ElseIf, Else or End\r\n");
            return restIf;
        }

        Node If_Statement()
        {
            Node ifStatement = new Node("If_Statement");
            ifStatement.Children.Add(match(Token_Class.If));
            ifStatement.Children.Add(Condition_Statement());
            ifStatement.Children.Add(match(Token_Class.Then));
            ifStatement.Children.Add(Statements());
            ifStatement.Children.Add(Rest_If());
            return ifStatement;
        }

        Node Repeat_Statement()
        {
            Node repeatStatement = new Node("Repeat_Statement");
            repeatStatement.Children.Add(match(Token_Class.Repeat));
            repeatStatement.Children.Add(Statements());
            repeatStatement.Children.Add(match(Token_Class.Until));
            repeatStatement.Children.Add(Condition_Statement());
            return repeatStatement;
        }

                Node Statement()
        {
            Node statement = new Node("Statement");
            if(InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.Identifier)
                statement.Children.Add(Assignment_Statement());
            else if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.Int ||
                TokenStream[InputPointer].token_type == Token_Class.Float ||
                TokenStream[InputPointer].token_type == Token_Class.String)
                statement.Children.Add(Declaration_Statement());
            else if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.Write)
                statement.Children.Add(Write_Statement());
            else if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.Read)
                statement.Children.Add(Read_Statement());
            else if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.If)
                statement.Children.Add(If_Statement());
            else if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.Repeat)
                statement.Children.Add(Repeat_Statement());
            else if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.Return)
                statement.Children.Add(Return_Statement());
            else
                Errors.Error_List.Add("Parsing Error: Expected a Statement\r\n");
            return statement;
        }

        Node Statements()
        {
            Node statements = new Node("Statements");
            if (InputPointer < TokenStream.Count &&
                (TokenStream[InputPointer].token_type == Token_Class.Identifier ||
                TokenStream[InputPointer].token_type == Token_Class.Int ||
                TokenStream[InputPointer].token_type == Token_Class.Float ||
                TokenStream[InputPointer].token_type == Token_Class.String ||
                TokenStream[InputPointer].token_type == Token_Class.Write ||
                TokenStream[InputPointer].token_type == Token_Class.Read ||
                TokenStream[InputPointer].token_type == Token_Class.If ||
                TokenStream[InputPointer].token_type == Token_Class.Repeat ||
                TokenStream[InputPointer].token_type == Token_Class.Return ))
            {
                statements.Children.Add(Statement());
                statements.Children.Add(Statements());
            }
            return statements;
        }

        Node FunctionName()
        {
            Node functionName = new Node("FunctionName");
            functionName.Children.Add(match(Token_Class.Identifier));
            return functionName;
        }

        Node Parameter()
        {
            Node parameter = new Node("Parameter");
            parameter.Children.Add(Datatype());
            parameter.Children.Add(match(Token_Class.Identifier));
            return parameter;
        }

        Node Other_Parameter()
        {
            Node otherParameter = new Node("Other_Parameter");
            if(InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                otherParameter.Children.Add(match(Token_Class.Comma));
                otherParameter.Children.Add(Parameter());
                otherParameter.Children.Add(Other_Parameter());
            }
            return otherParameter;
        }

        Node Parameter_List()
        {
            Node parameterList = new Node("Parameter_List");
            if(InputPointer < TokenStream.Count &&
                (TokenStream[InputPointer].token_type == Token_Class.Int ||
                TokenStream[InputPointer].token_type == Token_Class.Float ||
                TokenStream[InputPointer].token_type == Token_Class.String))
            {
                parameterList.Children.Add(Parameter());
                parameterList.Children.Add(Other_Parameter());
            }
            return parameterList;
        }

        Node Function_Body()
        {
            Node functionBody = new Node("Function_Body");
            functionBody.Children.Add(match(Token_Class.LeftCurlyPrant));
            functionBody.Children.Add(Statements());
            functionBody.Children.Add(match(Token_Class.RightCurlyPrant));
            return functionBody;
        }

        Node Function_Statement()
        {
            Node functionStatement = new Node("Function_Statement");
            functionStatement.Children.Add(FunctionName());
            functionStatement.Children.Add(match(Token_Class.LeftPrant));
            functionStatement.Children.Add(Parameter_List());
            functionStatement.Children.Add(match(Token_Class.RightPrant));
            functionStatement.Children.Add(Function_Body());
            return functionStatement;
        }

        Node Main_Function()
        {
            Node mainFunction = new Node("Main_Function");
            mainFunction.Children.Add(match(Token_Class.Main));
            mainFunction.Children.Add(match(Token_Class.LeftPrant));
            mainFunction.Children.Add(match(Token_Class.RightPrant));
            mainFunction.Children.Add(Function_Body());
            int x = /*this is x*/ 0;
            return mainFunction;
        }

        Node Functions()
        {
            Node functions = new Node("Functions");
            if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.Identifier)
            {
                functions.Children.Add(Function_Statement());
                functions.Children.Add(Datatype());
                functions.Children.Add(Functions());
            }
            return functions;
        }

        Node Program()
        {
            Node program = new Node("Program");
            program.Children.Add(Datatype());
            program.Children.Add(Functions());
            program.Children.Add(Main_Function());
            return program;
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

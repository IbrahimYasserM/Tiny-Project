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

        #region CFG
        public Node StartParsing(List<Token> TokenStream)
        {
            this.InputPointer = 0;
            this.TokenStream = TokenStream;
            root = new Node("Program");
            root.Children.Add(Datatype());
            root.Children.Add(FunctionStatements());
            root.Children.Add(MainFunction());
            return root;
        }
        Node FunctionStatements()
        {
            Node functionStatements = new Node("Function Statements");
            if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.Identifier)
            {
                functionStatements.Children.Add(FunctionStatement());
                functionStatements.Children.Add(Datatype());
                functionStatements.Children.Add(FunctionStatements());
            }
            return functionStatements;
        }
        Node MainFunction()
        {
            Node mainFunction = new Node("Main Function");
            mainFunction.Children.Add(match(Token_Class.Main));
            mainFunction.Children.Add(match(Token_Class.LeftPrant));
            mainFunction.Children.Add(match(Token_Class.RightPrant));
            mainFunction.Children.Add(FunctionBody());
            return mainFunction;
        }
        Node FunctionStatement()
        {
            Node functionStatement = new Node("Function Statement");
            functionStatement.Children.Add(match(Token_Class.Identifier));
            functionStatement.Children.Add(match(Token_Class.LeftPrant));
            functionStatement.Children.Add(ParameterList());
            functionStatement.Children.Add(match(Token_Class.RightPrant));
            functionStatement.Children.Add(FunctionBody());
            return functionStatement;
        }
        Node FunctionBody()
        {
            Node functionBody = new Node("Function Body");
            functionBody.Children.Add(match(Token_Class.LeftCurlyPrant));
            functionBody.Children.Add(Statements());
            functionBody.Children.Add(ReturnStatement());
            functionBody.Children.Add(match(Token_Class.RightCurlyPrant));
            return functionBody;
        }
        Node ParameterList()
        {
            Node parameterList = new Node("Parameter List");
            if (InputPointer < TokenStream.Count &&
                (TokenStream[InputPointer].token_type == Token_Class.Int ||
                TokenStream[InputPointer].token_type == Token_Class.Float ||
                TokenStream[InputPointer].token_type == Token_Class.String))
            {
                parameterList.Children.Add(Parameter());
                parameterList.Children.Add(OtherParameters());
            }
            return parameterList;
        }
        Node OtherParameters()
        {
            Node otherParameter = new Node("Other Parameters");
            if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                otherParameter.Children.Add(match(Token_Class.Comma));
                otherParameter.Children.Add(Parameter());
                otherParameter.Children.Add(OtherParameters());
            }
            return otherParameter;
        }
        Node Parameter()
        {
            Node parameter = new Node("Parameter");
            parameter.Children.Add(Datatype());
            parameter.Children.Add(match(Token_Class.Identifier));
            return parameter;
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
                TokenStream[InputPointer].token_type == Token_Class.Repeat))
            {
                statements.Children.Add(Statement());
                statements.Children.Add(Statements());
            }
            return statements;
        }
        Node Statement()
        {
            Node statement = new Node("Statement");
            if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.Identifier)
                statement.Children.Add(AssignmentStatement());
            else if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.Int ||
                TokenStream[InputPointer].token_type == Token_Class.Float ||
                TokenStream[InputPointer].token_type == Token_Class.String)
                statement.Children.Add(DeclarationStatement());
            else if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.Write)
                statement.Children.Add(WriteStatement());
            else if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.Read)
                statement.Children.Add(ReadStatement());
            else if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.If)
                statement.Children.Add(IfStatement());
            else if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.Repeat)
                statement.Children.Add(RepeatStatement());
            else
                Errors.Error_List.Add("Parsing Error: Expected a Statement\r\n");
            return statement;
        }
        Node ReturnStatement()
        {
            Node returnStatement = new Node("Return Statement");
            returnStatement.Children.Add(match(Token_Class.Return));
            returnStatement.Children.Add(Expression());
            returnStatement.Children.Add(match(Token_Class.Semicolon));
            return returnStatement;
        }
        Node RepeatStatement()
        {
            Node repeatStatement = new Node("Repeat Statement");
            repeatStatement.Children.Add(match(Token_Class.Repeat));
            repeatStatement.Children.Add(Statements());
            repeatStatement.Children.Add(match(Token_Class.Until));
            repeatStatement.Children.Add(ConditionExpression());
            return repeatStatement;
        }
        Node IfStatement()
        {
            Node ifStatement = new Node("If Statement");
            ifStatement.Children.Add(match(Token_Class.If));
            ifStatement.Children.Add(ConditionExpression());
            ifStatement.Children.Add(match(Token_Class.Then));
            ifStatement.Children.Add(Statements());
            ifStatement.Children.Add(EndIf());
            return ifStatement;
        }
        Node EndIf()
        {
            Node endIf = new Node("End If");
            if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.ElseIf)
                endIf.Children.Add(ElseifStatement());
            else if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.Else)
                endIf.Children.Add(ElseStatement());
            else if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.End)
                endIf.Children.Add(match(Token_Class.End));
            else
                Errors.Error_List.Add("Parsing Error: Expected ElseIf, Else or End\r\n");
            return endIf;
        }
        Node ElseifStatement()
        {
            Node elseifStatement = new Node("Elseif Statement");
            elseifStatement.Children.Add(match(Token_Class.ElseIf));
            elseifStatement.Children.Add(ConditionExpression());
            elseifStatement.Children.Add(match(Token_Class.Then));
            elseifStatement.Children.Add(Statements());
            elseifStatement.Children.Add(EndIf());
            return elseifStatement;
        }
        Node ElseStatement()
        {
            Node elseStatement = new Node("Else Statement");
            elseStatement.Children.Add(match(Token_Class.Else));
            elseStatement.Children.Add(Statements());
            elseStatement.Children.Add(match(Token_Class.End));
            return elseStatement;
        }
        Node ConditionExpression()
        {
            Node conditionStatement = new Node("Condition Statement");
            conditionStatement.Children.Add(Condition());
            conditionStatement.Children.Add(OtherConditions());
            return conditionStatement;
        }
        Node OtherConditions()
        {
            Node otherConditions = new Node("Other Conditions");
            if (InputPointer < TokenStream.Count &&
                (TokenStream[InputPointer].token_type == Token_Class.AndOp ||
                TokenStream[InputPointer].token_type == Token_Class.OrOp))
            {
                otherConditions.Children.Add(match(TokenStream[InputPointer].token_type));
                otherConditions.Children.Add(ConditionExpression());
            }
            return otherConditions;
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
        Node WriteStatement()
        {
            Node writeStatement = new Node("Write Statement");
            writeStatement.Children.Add(match(Token_Class.Write));
            writeStatement.Children.Add(WriteExpression());
            writeStatement.Children.Add(match(Token_Class.Semicolon));
            return writeStatement;
        }
        Node WriteExpression()
        {
            Node writeExpression = new Node("Write Expression");
            if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.Endl)
                writeExpression.Children.Add(match(Token_Class.Endl));
            else
                writeExpression.Children.Add(Expression());
            return writeExpression;
        }
        Node ReadStatement()
        {
            Node readStatement = new Node("Read Statement");
            readStatement.Children.Add(match(Token_Class.Read));
            readStatement.Children.Add(match(Token_Class.Identifier));
            readStatement.Children.Add(match(Token_Class.Semicolon));
            return readStatement;
        }
        Node DeclarationStatement()
        {
            Node declarationStatement = new Node("Declaration Statement");
            declarationStatement.Children.Add(Datatype());
            declarationStatement.Children.Add(match(Token_Class.Identifier));
            declarationStatement.Children.Add(HasAssignment());
            declarationStatement.Children.Add(OtherDeclarations());
            declarationStatement.Children.Add(match(Token_Class.Semicolon));
            return declarationStatement;
        }
        Node OtherDeclarations()
        {
            Node otherDeclarations = new Node("Other Declarations");
            if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                otherDeclarations.Children.Add(match(Token_Class.Comma));
                otherDeclarations.Children.Add(match(Token_Class.Identifier));
                otherDeclarations.Children.Add(HasAssignment());
                otherDeclarations.Children.Add(OtherDeclarations());
            }
            return otherDeclarations;
        }
        Node HasAssignment()
        {
            Node hasAssignment = new Node("Has Assignment");
            if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.AssignOp)
            {
                hasAssignment.Children.Add(match(Token_Class.AssignOp));
                hasAssignment.Children.Add(Expression());
            }
            return hasAssignment;
        }
        Node AssignmentStatement()
        {
            Node assignmentStatement = new Node("Assignment Statement");
            assignmentStatement.Children.Add(match(Token_Class.Identifier));
            assignmentStatement.Children.Add(match(Token_Class.AssignOp));
            assignmentStatement.Children.Add(Expression());
            assignmentStatement.Children.Add(match(Token_Class.Semicolon));
            return assignmentStatement;
        }
        Node Expression()
        {
            Node expression = new Node("Expression");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.ConstantString)
                expression.Children.Add(match(Token_Class.ConstantString));
            else
                expression.Children.Add(Equation());
            return expression;
        }
        Node Equation()
        {
            Node equation = new Node("Equation");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.LeftPrant)
            {
                equation.Children.Add(match(Token_Class.LeftPrant));
                equation.Children.Add(Equation());
                equation.Children.Add(match(Token_Class.RightPrant));
                equation.Children.Add(OtherEquations());
            }
            else
            {
                equation.Children.Add(Term());
                equation.Children.Add(OtherEquations());
            }
            return equation;
        }
        Node OtherEquations()
        {
            Node otherEquations = new Node("Other Equations");
            if (InputPointer < TokenStream.Count &&
                (TokenStream[InputPointer].token_type == Token_Class.PlusOp ||
                TokenStream[InputPointer].token_type == Token_Class.MinusOp ||
                TokenStream[InputPointer].token_type == Token_Class.MultiplyOp ||
                TokenStream[InputPointer].token_type == Token_Class.DivideOp))
            {
                otherEquations.Children.Add(match(TokenStream[InputPointer].token_type));
                otherEquations.Children.Add(Equation());
            }
            return otherEquations;
        }
        Node Term()
        {
            Node term = new Node("Term");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Identifier)
            {
                term.Children.Add(match(Token_Class.Identifier));
                term.Children.Add(HasArguments());
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.ConstantNumber)
                term.Children.Add(match(Token_Class.ConstantNumber));
            else
                Errors.Error_List.Add("Parsing Error: Expected a Term\r\n");
            return term;
        }
        Node HasArguments()
        {
            Node hasArguments = new Node("Has Arguments");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.LeftPrant)
            {
                hasArguments.Children.Add(match(Token_Class.LeftPrant));
                hasArguments.Children.Add(ArgumentList());
                hasArguments.Children.Add(match(Token_Class.RightPrant));
            }
            return hasArguments;
        }
        Node ArgumentList()
        {
            Node argumentList = new Node("Argument List");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type != Token_Class.RightPrant)
            {
                argumentList.Children.Add(Expression());
                argumentList.Children.Add(OtherArguments());
            }
            return argumentList;
        }
        Node OtherArguments()
        {
            Node otherArguments = new Node("Other Arguments");
            if(InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                otherArguments.Children.Add(match(Token_Class.Comma));
                otherArguments.Children.Add(Expression());
                otherArguments.Children.Add(OtherArguments());
            }
            return otherArguments;
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
        #endregion

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
                    if (Recommender.ErrorRecommend. Recommend(TokenStream[InputPointer].lex, Tiny_Compiler.Tiny_Scanner.ReservedWords) != TokenStream[InputPointer].lex)
                        InputPointer++;
                    return null;
                }
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString() + "\r\n");
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

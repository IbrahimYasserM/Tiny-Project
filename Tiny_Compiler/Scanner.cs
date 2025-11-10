using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public enum Token_Class
{
    Int,
    Float,
    String,
    Read,
    Write,
    Repeat,
    Until,
    If,
    ElseIf,
    Else,
    Then,
    Return,
    End,
    PlusOp,
    MinusOp,
    MultiplyOp,
    DivideOp,
    EqualOp,
    LessThanOp,
    GreaterThanOp,
    NotEqualOp,
    AndOp,
    OrOp,
    Identifier,
    ConstantNumber,
    ConstantString,
    CommentStatment
}
namespace Tiny_Compiler
{
    

    public class Token
    {
       public string lex;
       public Token_Class token_type;
    }

    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();
        Dictionary<string, Token_Class> ReservedWords = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> ArithmeticOperators = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> ConditionOperators = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> BooleanOperators = new Dictionary<string, Token_Class>();

        public Scanner()
        {
            ReservedWords.Add("int", Token_Class.Int);
            ReservedWords.Add("float", Token_Class.Float);
            ReservedWords.Add("string", Token_Class.String);
            ReservedWords.Add("read", Token_Class.Read);
            ReservedWords.Add("write", Token_Class.Write);
            ReservedWords.Add("repeat", Token_Class.Repeat);
            ReservedWords.Add("until", Token_Class.Until);
            ReservedWords.Add("if", Token_Class.If);
            ReservedWords.Add("elseif", Token_Class.ElseIf);
            ReservedWords.Add("else", Token_Class.Else);
            ReservedWords.Add("then", Token_Class.Then);
            ReservedWords.Add("return", Token_Class.Return);
            ReservedWords.Add("end", Token_Class.End);

            ArithmeticOperators.Add("+", Token_Class.PlusOp);
            ArithmeticOperators.Add("-", Token_Class.MinusOp);
            ArithmeticOperators.Add("*", Token_Class.MultiplyOp);
            ArithmeticOperators.Add("/", Token_Class.DivideOp);

            ConditionOperators.Add("=", Token_Class.EqualOp);
            ConditionOperators.Add("<", Token_Class.LessThanOp);
            ConditionOperators.Add(">", Token_Class.GreaterThanOp);
            ConditionOperators.Add("<>", Token_Class.NotEqualOp);

            BooleanOperators.Add("&&", Token_Class.AndOp);
            BooleanOperators.Add("||", Token_Class.OrOp);
        }

    public void StartScanning(string SourceCode)
        {
            for(int i=0; i<SourceCode.Length;i++)
            {
                char CurrentChar = SourceCode[i];
                string CurrentLexeme = CurrentChar.ToString();

                if (CurrentChar == ' ' || CurrentChar == '\r' || CurrentChar == '\n')
                    continue;

                if (isLetter(CurrentChar))
                {
                    while (i + 1 < SourceCode.Length && (isLetter(SourceCode[i + 1]) || IsDigit(SourceCode[i + 1])))
                        CurrentLexeme += SourceCode[++i];
                }
                else if (IsDigit(CurrentChar))
                {
                    bool lastE = false;
                    while (i + 1 < SourceCode.Length && (IsDigit(SourceCode[i + 1]) || isLetter(SourceCode[i + 1]) || SourceCode[i + 1] == '.'
                        || ((SourceCode[i + 1] == '+' || SourceCode[i + 1] == '-') && lastE)))
                    {
                        CurrentLexeme += SourceCode[++i];
                        lastE = SourceCode[i] == 'e' || SourceCode[i] == 'E';
                    }
                }
                else if(CurrentChar == '"')
                {
                    bool lastBackslash = false;
                    while (i+1 < SourceCode.Length && (SourceCode[i+1] != '"' || lastBackslash))
                    {
                        CurrentLexeme += SourceCode[++i];
                        lastBackslash = SourceCode[i] == '\\';
                    }
                    if(i+1 < SourceCode.Length)
                        CurrentLexeme += SourceCode[++i];
                }
                else if (CurrentChar == '/')
                {
                    if(i + 1 < SourceCode.Length && SourceCode[i + 1] == '*')
                    {
                        CurrentLexeme += SourceCode[++i];
                        bool lastAsterisk = false;
                        while (i + 1 < SourceCode.Length && (SourceCode[i + 1] != '/' || !lastAsterisk))
                        {
                            CurrentLexeme += SourceCode[++i];
                            lastAsterisk = SourceCode[i] == '*';
                        }
                        if(i+1 < SourceCode.Length)
                            CurrentLexeme += SourceCode[++i];
                    }
                }
                else if (CurrentChar == '&' || CurrentChar == '|')
                {
                   while(i + 1 < SourceCode.Length && SourceCode[i + 1] == CurrentChar)
                    {
                        CurrentLexeme += SourceCode[++i];
                    }
                }
                else if(CurrentChar == '<')
                {
                    if(i + 1 < SourceCode.Length && SourceCode[i + 1] == '>')
                        CurrentLexeme += SourceCode[++i];
                }
                FindTokenClass(CurrentLexeme);
            }
            
            Tiny_Compiler.TokenStream = Tokens;
        }
        void FindTokenClass(string Lex)
        {
            Token_Class TC;
            Token Tok = new Token();
            Tok.lex = Lex;
            // Is Reserved Word?
            if(ReservedWords.TryGetValue(Lex, out TC))
                Tok.token_type = TC;
            // Is Arithmetic Operator?
            else if(ArithmeticOperators.TryGetValue(Lex, out TC))
                Tok.token_type = TC;
            // Is Condition Operator?
            else if(ConditionOperators.TryGetValue(Lex, out TC))
                Tok.token_type = TC;
            // Is Boolean Operator?
            else if(BooleanOperators.TryGetValue(Lex, out TC))
                Tok.token_type = TC;
            // Is Identifier?
            else if (isIdentifier(Lex))
                Tok.token_type = Token_Class.Identifier;
            // Is Constant?
            else if (isNumber(Lex))
                Tok.token_type = Token_Class.ConstantNumber;
            // Is String?
            else if (isString(Lex))
                Tok.token_type = Token_Class.ConstantString;
            // Is Comment Stattment?
            else if (isCommentStatment(Lex))
                Tok.token_type = Token_Class.CommentStatment;
            // Is Undefined?
            else {
                Errors.Error_List.Add("Undefined Token: " + Lex);
                return;
            }
            Tokens.Add(Tok);
        }



        bool isNumber(string lex)
        {
            // Check if the lex is a Number or not.
            var rx = new Regex(@"^[0-9]+(\.[0-9]+)?((e|E)(\+|-)?[0-9]+)?$", RegexOptions.Compiled);
            return rx.IsMatch(lex);
        }
        bool isString(string lex)
        {
            // Check if the lex is a String or not.
            var rx = new Regex(@"^""([^""]|\\"")*""$", RegexOptions.Compiled);
            return rx.IsMatch(lex);
        }
        bool isCommentStatment(string lex)
        {
            // Check if the lex is a Comment Statement or not.
            var rx = new Regex(@"^/\*.*\*/$", RegexOptions.Compiled);
            return rx.IsMatch(lex);
        }
        bool isIdentifier(string lex)
        {
            // Check if the lex is an Identifier or not.
            var rx = new Regex(@"^[a-zA-Z][a-zA-Z0-9]*$", RegexOptions.Compiled);
            return rx.IsMatch(lex);
        }

        bool isLetter(char ch)
        {
            return (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || ch == '_';
        }

        bool IsDigit(char ch)
        {
            return (ch >= '0' && ch <= '9');
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
namespace plt3
{



    static public class Status
    {
        static List<string> ErrorArray = new List<string>();
        public static List<(string value, string type)> lexemes { get; } = new List<(string, string)>();

        public static void AddErrorRecord(string str)
        {
            ErrorArray.Add(str);
        }
        public static void AddLexeme(string value, string type)
        {
            lexemes.Add((value, type));
        }
        public static string PrintErr()
        {
            StringBuilder str = new StringBuilder();
            foreach (string str2 in ErrorArray)
                str.Append(str2 + "\n");
            return str.ToString();
        }
        public static void Reset()
        {
            ErrorArray.Clear();
            lexemes.Clear();
        }


    }
    static public class Checker
    {
        public static string[] eqOperations = { "<>", "=", "<", ">" };
        public static string[] unarylogicalOperation = { "not" };
        public static string[] binarylogicalOperation = { "and", "or" };
        public static char[] arrithmeticOperations = { '*', '/', '-', '+' };
        public static bool IsKeyword(string str)
        {
            str = str.ToLower();
            switch (str)
            {
                case ("if"): return true;
                case ("then"): return true;
                case ("elseif"): return true;
                case ("else"): return true;
                case ("end"): return true;
                case ("not"): return true;
                case ("and"): return true;
                case ("or"): return true;
                case ("output"): return true;
                default: { return false; }
            }
        }
        public static bool IsConst(string str)
        {
            str = str.Trim();
            foreach (char c in str)
            {
                if (Char.IsDigit(c) == false)
                {
                    return false;
                }
            }
            Status.AddLexeme(str, "const");
            return true;
        }

        public static bool IsId(string str)
        {
            str = str.Trim();
            if (Checker.IsKeyword(str)) { Status.AddErrorRecord($"ключевое слово {{{str}}} не может быть идентификатором"); return false; }
            if (Char.IsLetter(str[0]))
            {
                foreach (char c in str.Substring(1, str.Length - 1))
                {
                    if (Char.IsLetterOrDigit(c) == false) { return false; }
                }
                Status.AddLexeme(str, "id");
                return true;
            }
            else return false;
        }
        public static bool IsLogicExpression(string str)
        {
            str = str.Trim();

            int index = -1;
            

            foreach (string c in binarylogicalOperation)//if logExp binaryOp EqExp
            {
                MatchCollection matches = Regex.Matches(str, c);
                if (matches.Count>0)
                {
                    index = matches[matches.Count-1].Index;
                    string leftPart = str.Substring(0, index), rightPart = str.Substring(index + c.Length);
                    if (IsLogicExpression(leftPart))
                    {
                        if (c == "and") Status.AddLexeme(c, "and");
                        else if (c == "or") Status.AddLexeme(c, "or");
                        if (IsEq(rightPart))
                            return true;
                        else { Status.AddErrorRecord($"{{{str}}} не является выражением сравнения"); return false; }
                    }
                    else { Status.AddErrorRecord($"{{{str}}} не является логическим выражением"); return false; }
                }
            }
            foreach (string c in unarylogicalOperation) //if unaryLogOp EqExp
            {
                Match match = Regex.Match(str, c + @"\s");
                if (match.Success)
                {
                    index = match.Index;
                    Status.AddLexeme(c, "UnLogOp");
                    return IsEq(str.Substring(index + c.Length));
                }
            }
            return IsEq(str);
        }
        public static bool IsEq(string str)
        {
            str = str.Trim();

            int indexEq = -1;
            foreach (string c in eqOperations)
            {
                indexEq = str.IndexOf(c);
                if (indexEq != -1)
                {
                    string leftPart = str.Substring(0, indexEq), rightPart = str.Substring(indexEq + c.Length);
                    if (IsOperand(leftPart) )
                    {
                        Status.AddLexeme(c,"EqualOp");
                        if (IsEq(rightPart)) return true;
                        else return false;
                    }
                    else { Status.AddErrorRecord($"{{{str}}} не является выражением сравнения"); return false; }
                }
            }
            return IsOperand(str);
        }
        public static bool IsOperand(string str)
        {
            str = str.Trim();
            if (IsConst(str) || IsId(str)) return true;
            else { Status.AddErrorRecord($"{{{str}}} не является операндом"); return false; }
        }
        public static bool IsArithmeticOperator(string str)
        {
            str = str.Trim();
            int separatorIndex = -1;

            foreach (char c in Checker.arrithmeticOperations)
            {
                separatorIndex = str.IndexOf(c);
                if (separatorIndex != -1)
                {
                    if (separatorIndex == str.Length - 1) return false;
                    string leftPart = str.Substring(0, separatorIndex);
                    string rightPart = str.Substring(separatorIndex + 1);
                    if (IsOperand(leftPart))
                    {
                        Status.AddLexeme(c.ToString(), "arithmeticOperation");
                        if (IsArithmeticOperator(rightPart)) return true;
                    }
                }
            }
            if (Checker.IsOperand(str) == false) { Status.AddErrorRecord($"{{{str}}} не является логической операцией"); return false; }
            else return true;
        }
        public static bool IsOperator(string str)
        {
            str = str.Trim();
            int separatorIndex = str.IndexOf('=');
            if (separatorIndex == -1) //output operand
            {
                Match match = Regex.Match(str, @"\s");
                if (match.Success == false) return false;
                string leftPart = str.Substring(0, match.Index);
                string rightPart = str.Substring(match.Index + 1);
                if (leftPart.ToLower() == "output")
                {
                    Status.AddLexeme("output", "output");
                    if (Checker.IsOperand(rightPart)) return true;
                    else return false;
                }
                else { Status.AddErrorRecord($"{{{str}}} не является оператором"); return false; }
            }
            else //id=arithmExpr
            {
                string leftPart = str.Substring(0, separatorIndex);
                string rightPart = str.Substring(separatorIndex + 1);
                if (Checker.IsId(leftPart))
                {
                    Status.AddLexeme("=", "rel");
                    if (Checker.IsArithmeticOperator(rightPart))
                        return true;
                    else return false;
                }
                else { Status.AddErrorRecord($"{{{str}}} не является оператором"); return false; }
            }
        }
        public static bool IsOperators(string str)
        {
            str = str.Trim();
            int separatorIndex = str.LastIndexOf(';');
            if (separatorIndex == -1)//operator
            {
                if (Checker.IsOperator(str)) return true;
                else return false;
            }
            else//operators;operator
            {
                string leftPart = str.Substring(0, separatorIndex);
                string rightPart = str.Substring(separatorIndex + 1);
                if (Checker.IsOperators(leftPart))
                {
                    Status.AddLexeme(";", "separator");
                    if (Checker.IsOperator(rightPart))
                        return true;
                    else return false;
                }
                else return false;
            }
        }
        public static bool GetLeftRigtParts(string str, out string left, out string right, params string[] separators)
        {
            left = "";
            right = "";
            foreach (string separator in separators)
            {
                bool searchSpaceLeft = false, searchSpaceRight = false;
                if (separator.StartsWith(" ")) searchSpaceLeft = true;
                if (separator.EndsWith(" ")) searchSpaceRight = true;
                string pattern = separator.Trim();
                if (searchSpaceLeft) pattern = @"\s" + pattern;
                if (searchSpaceRight) pattern = pattern + @"\s";
                Match match = Regex.Match(str, pattern);

                if (match.Success)
                {
                    left = str.Substring(0, match.Index);
                    right = str.Substring(match.Index + separator.Length);
                    return true;
                }
            }
            return false;
        }
    }

}

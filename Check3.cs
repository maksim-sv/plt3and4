using System;

namespace plt3
{
    enum state { S, LOGEXP, EQEXPLEFT, EQEXPMID, EQEXPRIGHT, EQEXAFTER, F, OPERATORS, ID, CONST, OPERATORSLEFT, OPERATORSRIGHT, ARITHMETICOP }
    internal class Check3
    {

        public state currentState { get; private set; } = state.S;
        string str;
        bool end = false;
        (string value, string type) operand = ("", "");
        public int index { get; private set; } = 0;
        public Check3(string strArg)
        {
            str = strArg;
        }
        public int Step()//0 continue,-1 error, 1 success
        {
            if (index == str.Length)
                return 1;
            char c = str[index];
            switch (currentState)
            {
                case state.S:
                    if (Char.IsWhiteSpace(c)) { index++; return 0; }
                    else if (c == 'i' && str.Substring(index, 2) == "if" && Char.IsWhiteSpace(str[index + 2])) { Status.AddLexeme("if", "if"); currentState = state.LOGEXP; index += 3; return 0; }
                    else
                    {
                        Status.AddErrorRecord("конструкция должна начинаться с if");
                        return -1;
                    }
                case state.LOGEXP:
                    if (Char.IsWhiteSpace(c)) { index++; return 0; }
                    else if (Char.IsLetterOrDigit(c))
                    {
                        if (str.Substring(index, 3) == "not" && Char.IsWhiteSpace(str[index + 3])) { Status.AddLexeme("not", "unaryOp"); currentState = state.EQEXPLEFT; index += 4; return 0; }
                        else { currentState = state.EQEXPLEFT; return 0; }
                    }
                    else
                    {
                        Status.AddErrorRecord("Неправильное логическое выражение");
                        return -1;
                    }
                case state.EQEXPLEFT://слева нет пробелов 
                    if (Char.IsWhiteSpace(c))
                    {
                        if (operand.value == "")
                        { index++; return 0; }
                        else if (Checker.IsKeyword(operand.value))
                        {
                            Status.AddErrorRecord($"ключевое слово {{{operand.value}}} использовано как операнд");
                            return -1;
                        }
                        else
                        {
                            index++;
                            Status.AddLexeme(operand.value, operand.type);
                            currentState = state.EQEXPMID;
                            operand = ("", "");
                            return 0;
                        }
                    }
                    else if (Char.IsLetter(c))
                    {
                        if (operand.type == "const")
                        {
                            Status.AddErrorRecord($"при чтении константы {{{operand.value}}} обнаружен нецифровой символ");
                            return -1;
                        }
                        else if (operand.type == "id")
                        {
                            index++;
                            operand.value += c;
                            return 0;
                        }
                        else if (operand.type == "")
                        {
                            index++;
                            operand.type = "id";
                            operand.value += c;
                            return 0;
                        }
                    }


                    else if (Char.IsDigit(c))
                    {
                        if (operand.type == "const")
                        {
                            index++;
                            operand.value += c;
                            return 0;
                        }

                        else if (operand.type == "id")
                        {
                            index++;
                            operand.value += c;
                            return 0;
                        }
                        else if (operand.type == "")
                        {
                            index++;
                            operand.type = "const";
                            operand.value += c;
                            return 0;
                        }
                    }
                    else if (c == '=' || c == '<' || c == '>')
                    {
                        if (operand.value == "")
                        {
                            Status.AddErrorRecord($"нет операнда перед операцией сравнения");
                            return -1;
                        }

                        if (Checker.IsKeyword(operand.value))
                        {
                            Status.AddErrorRecord($"ключевое слово {{{operand.value}}} использовано как операнд");
                            return -1;
                        }
                        Status.AddLexeme(operand.value, operand.type);
                        currentState = state.EQEXPMID;
                        operand = ("", "");
                        return 0;
                    }
                    Status.AddErrorRecord($"Неправильное выражение сравнения");
                    return -1;
                case state.EQEXPMID:
                    if (Char.IsWhiteSpace(c))
                    {
                        if (operand.value == "") { index++; return 0; }
                        currentState = state.EQEXPRIGHT; index++;
                        return 0;
                    }
                    if (c == '=' || c == '<' || c == '>')
                    {
                        if (str.Substring(index, 2) == "<>")
                        {
                            Status.AddLexeme("<>", "eqOP");
                            index += 2;
                            currentState = state.EQEXPRIGHT;
                            return 0;
                        }
                        Status.AddLexeme(c.ToString(), "eqOP");
                        index++;
                        currentState = state.EQEXPRIGHT;
                        return 0;
                    }
                    if (str.Substring(index, 4) == "then" && Char.IsWhiteSpace(str[index + 4])) { index += 5; Status.AddLexeme("then", "then"); currentState = state.OPERATORS; return 0; }
                    if (str.Substring(index, 3) == "and" && Char.IsWhiteSpace(str[index + 3])) { index += 4; Status.AddLexeme("and", "binaryOp"); currentState = state.EQEXPLEFT; return 0; }
                    if (str.Substring(index, 2) == "or" && Char.IsWhiteSpace(str[index + 2])) { index += 3; Status.AddLexeme("or", "binaryOp"); currentState = state.EQEXPLEFT; return 0; }

                    Status.AddErrorRecord($"Неправильное выражение равнения");
                    return -1;
                case state.EQEXPRIGHT:
                    if (Char.IsWhiteSpace(c))
                    {
                        if (operand.value == "")
                        { index++; return 0; }
                        else if (Checker.IsKeyword(operand.value))
                        {
                            Status.AddErrorRecord($"ключевое слово {{{operand.value}}} использовано как операнд");
                            return -1;
                        }
                        else
                        {
                            index++;
                            Status.AddLexeme(operand.value, operand.type);
                            currentState = state.EQEXAFTER;
                            operand = ("", "");
                            return 0;
                        }
                    }
                    else if (Char.IsLetter(c))
                    {
                        if (operand.type == "const")
                        {
                            Status.AddErrorRecord($"при чтении константы {{{operand.value}}} обнаружен нецифровой символ");
                            return -1;
                        }
                        else if (operand.type == "id")
                        {
                            index++;
                            operand.value += c;
                            return 0;
                        }
                        else if (operand.type == "")
                        {
                            index++;
                            operand.type = "id";
                            operand.value += c;
                            return 0;
                        }
                    }


                    else if (Char.IsDigit(c))
                    {
                        if (operand.type == "const")
                        {
                            operand.value += c;
                            return 0;
                        }

                        else if (operand.type == "id")
                        {
                            operand.value += c;
                            return 0;
                        }
                        else if (operand.type == "")
                        {
                            index++;
                            operand.type = "const";
                            operand.value += c;
                            return 0;
                        }
                    }
                    Status.AddErrorRecord($"Неправильное выражение сравнения");
                    return -1;

                case state.EQEXAFTER:
                    if (Char.IsWhiteSpace(c)) { index++; return 0; }
                    if (str.Substring(index, 4) == "then" && Char.IsWhiteSpace(str[index + 4])) { index += 5; Status.AddLexeme("then", "then"); currentState = state.OPERATORS; return 0; }
                    if (str.Substring(index, 3) == "and" && Char.IsWhiteSpace(str[index + 3])) { index += 4; Status.AddLexeme("and", "binaryOp"); currentState = state.LOGEXP; return 0; }
                    if (str.Substring(index, 2) == "or" && Char.IsWhiteSpace(str[index + 2])) { index += 3; Status.AddLexeme("or", "binaryOp"); currentState = state.LOGEXP; return 0; }

                    Status.AddErrorRecord($"Неправильное выражение сравнения");
                    return -1;

                case state.OPERATORS:
                    if (Char.IsWhiteSpace(c)) { index++; return 0; }
                    if (str.Substring(index, 6) == "output" && Char.IsWhiteSpace(str[index + 6])) { index += 7; Status.AddLexeme("output", "output"); currentState = state.OPERATORSRIGHT; return 0; }
                    if (Char.IsLetter(c)) { currentState = state.OPERATORSLEFT; return 0; }
                    Status.AddErrorRecord($"Неправильные операторы");
                    return -1;

                /////////////////////////////////////////////////
                case state.OPERATORSLEFT:
                    if (Char.IsWhiteSpace(c))
                    {
                        if (operand.value == "") { index++; return 0; }
                        index++;//пропустить уже найденный пробельный символ
                        foreach (char t in str.Substring(index))
                        {
                            if (t == '=') { index++; Status.AddLexeme("=", "eqOP"); currentState = state.OPERATORSRIGHT; operand = ("", ""); return 0; }
                            if (!Char.IsWhiteSpace(t)) { Status.AddErrorRecord($"Неправильный оператор (нет =)"); return -1; }
                        }
                    }
                    if (Char.IsLetterOrDigit(c))
                    {
                        if (operand.type == "")
                        {
                            if (Char.IsLetter(c))
                            {
                                index++;
                                operand.type = "id";
                                operand.value += c;
                                return 0;
                            }
                            else { Status.AddErrorRecord($"идентифиактор начинается с цифры"); return -1; }
                        }
                        index++;
                        operand.value += c;
                        return 0;
                    }
                    if (c == '=')
                    {
                        index++; Status.AddLexeme(operand.value, operand.type); operand = ("", ""); Status.AddLexeme("=", "eqOP"); currentState = state.OPERATORSRIGHT; return 0;
                    }

                    Status.AddErrorRecord($"Неправильное выражение равнения");
                    return -1;
                //////////////////////RIGHT///////////////////////
                case state.OPERATORSRIGHT:
                    if (Char.IsWhiteSpace(c))
                    {
                        if (operand.value == "") { index++; return 0; }
                        if (Checker.IsKeyword(operand.value)) { Status.AddErrorRecord($"ключевое слово {{{operand.value}}} использовано как операнд"); return -1; }
                        index++; currentState = state.ARITHMETICOP; Status.AddLexeme(operand.value, operand.type); operand = ("", ""); return 0;
                    }

                    if (Char.IsLetter(c))
                    {
                        if (operand.type == "const")
                        {
                            Status.AddErrorRecord($"при чтении константы {{{operand.value}}} обнаружен нецифровой символ");
                            return -1;
                        }
                        else if (operand.type == "id")
                        {
                            index++;
                            operand.value += c;
                            return 0;
                        }
                        else if (operand.type == "")
                        {
                            index++;
                            operand.type = "id";
                            operand.value += c;
                            return 0;
                        }
                    }

                    if (Char.IsDigit(c))
                    {
                        if (operand.type == "")
                        {
                            operand.type = "const";
                            operand.value += c;
                            index++;
                            return 0;
                        }
                        index++;
                        operand.value += c;
                        return 0;
                    }
                    if (c == ';')
                    {
                        if (operand.value != "")
                        {
                            index++; currentState = state.OPERATORS; Status.AddLexeme(operand.value, operand.type); Status.AddLexeme(";", "separator"); operand = ("", ""); return 0;
                        }
                        else {
                            Status.AddErrorRecord($"; использована как операнд");
                            return -1;
                        }
                    }
                    if (c == '+' || c == '-' || c == '*' || c == '/') { index++; Status.AddLexeme(operand.value, operand.type); Status.AddLexeme(c.ToString(), "arithmOP"); operand = ("", ""); return 0; }

                    Status.AddErrorRecord($"Неправильное арифметическое выражение");
                    return -1;

                ////////////////////CROP///////////////////////
                case state.ARITHMETICOP:
                    if (Char.IsWhiteSpace(c))
                    {
                        if (operand.value == "") { index++; return 0; }//слева
                        index++; currentState = state.OPERATORSRIGHT; Status.AddLexeme(operand.value, operand.type); operand = ("", ""); return 0;//справа
                    }
                    if (c == '+' || c == '-' || c == '*' || c == '/') { index++; operand = (c.ToString(), "arithOP"); return 0; }
                    if (Char.IsLetterOrDigit(c))
                    {
                        if (end == false)
                            if (str.Substring(index, 6) == "elseif" && Char.IsWhiteSpace(str[index + 6])) { index += 7; Status.AddLexeme("elseif", "elseif"); operand = ("", ""); currentState = state.LOGEXP; return 0; }
                        if (end == true)
                            if (str.Substring(index, 3) == "end") { index += 3; Status.AddLexeme("end", "end"); operand = ("", ""); currentState = state.F; return 0; }
                        if (str.Substring(index, 4) == "else" && Char.IsWhiteSpace(str[index + 4])) { index += 5; Status.AddLexeme("else", "else"); operand = ("", ""); end = true; currentState = state.OPERATORS; return 0; }
                        if (operand.value == "") { Status.AddErrorRecord("неправильное арифметическое выражение"); return -1; }//слева
                        Status.AddLexeme(operand.value, operand.type); operand = ("", ""); currentState = state.OPERATORSRIGHT; return 0;//справа
                    }
                    Status.AddErrorRecord($"Неправильное арифметическое выражение");
                    return -1;
                case state.F:
                    foreach (char t in str.Substring(index))
                    {
                        if (!Char.IsWhiteSpace(t)) { Status.AddErrorRecord($"конструкция должна заканчиваться end"); return -1; }
                    }
                    return 1;
            }
            return 1;
        }
    }
}


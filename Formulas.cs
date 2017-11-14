using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace FileManager
{
    class Formulas
    {
        static public int FirstMainOperation(string expr, char operation)
            // Return the index of the main operation in the expression, or -1 if there is none
        {
            // Main operation (MO) can not be located on the edge of the expression
            // E.g. "3-2" has a binary '-', which is a MO, "-2" has an unary '-', which is not a MO
            for (int i = 1; i < expr.Length - 1; i++) 
            {
                if (expr[i] == operation &&
                    expr[i - 1] != operation &&
                    expr[i + 1] != operation)
                {
                    int BracketCountL = 0;
                    int BracketCountR = 0;
                    for (int j = i; j < expr.Length; j++)
                    {
                        if (expr[j] == ')') BracketCountR++;
                        if (expr[j] == '(') BracketCountR--;
                    }
                    for (int j = i; j >= 0; j--)
                    {
                        if (expr[j] == ')') BracketCountL++;
                        if (expr[j] == '(') BracketCountL--;
                    }
                    if (BracketCountL == 0 && BracketCountR == 0) return i;
                }
            }
            return -1;
        }
        static public int FirstIncDecOperation(string expr, char operation)
            // Return the index of the main inc/dec operation in the expression, or -1 if there is none
        {
            for (int i = 1; i < expr.Length - 1; i++)
            {
                if (expr[i] == operation &&
                    expr[i + 1] == operation)
                {
                    int BracketCountL = 0;
                    int BracketCountR = 0;
                    for (int j = i+1; j < expr.Length; j++)
                    {
                        if (expr[j] == ')') BracketCountR++;
                        if (expr[j] == '(') BracketCountR--;
                    }
                    for (int j = i; j >= 0; j--)
                    {
                        if (expr[j] == ')') BracketCountL++;
                        if (expr[j] == '(') BracketCountL--;
                    }
                    if (BracketCountL == 0 && BracketCountR == 0) return i;
                }
            }
            return -1;
        }
        static public string[] FindFunctionArguments(string expr, string func)
            // Returns a string containing a function's arguments (only nmin and nmax are supported)
            // E.g. "=nmin(2,$B2,5)" returns "2","$B2","5"
        {
            for (int i = 0; i < expr.Length - 4; i++)
            {
                if (expr[i] == func[0] &&
                    expr[i + 1] == func[1] &&
                    expr[i + 2] == func[2] &&
                    expr[i + 3] == func[3] &&
                    expr[i + 4] == '(')
                {
                    int brackets = 1; // Find closing bracket
                    int endIndex = i + 5;
                    for (; endIndex < expr.Length; endIndex++)
                    {
                        if (expr[endIndex] == ')')
                        {
                            brackets--;
                            if (brackets == 0)
                                break; // Closing bracket found
                        }
                        else if (expr[endIndex] == '(')
                            brackets++;
                    }
                    if (brackets == 0) // Found closed function
                    {
                        int startIndex = i + 4;
                        StringBuilder arguments = new StringBuilder("");
                        for (int j = startIndex + 1; j < endIndex; j++)
                            arguments.Append(expr[j]);
                        return arguments.ToString().Split(',');
                    }
                }
            }
            return null;
        }
        static public string RemoveOuterBrackets(string expr)
            // Remove outer brackets from the expression if they are meaningless (e.g. "(2+2)")
            // This will NOT remove multiple brackets (e.g. "((2+2))")
        {
            if (expr[0] == '(' && expr[expr.Length - 1] == ')') // Expression is surrounded with brackets
            {
                bool outerBrackets = true; //Check if these brackets are actually outer
                for (int i = 1; i < expr.Length - 1; i++)
                {
                    if (expr[i] == '(')
                        break;
                    else if (expr[i] == ')')
                        outerBrackets = false;
                }
                for (int i = expr.Length - 2; i >= 1; i--)
                {
                    if (expr[i] == ')')
                        break;
                    else if (expr[i] == '(')
                        outerBrackets = false;
                }
                if (outerBrackets) // Crop string
                {
                    StringBuilder strWithoutBrackets = new StringBuilder();
                    for (int i = 1; i < expr.Length - 1; i++)
                        strWithoutBrackets.Append(expr[i]);
                    expr = strWithoutBrackets.ToString();
                }
            }
            return expr;
        }
        static public void GetCellCoordinatesFromName(string name, out int x, out int y) // All hail TOLYA
        {
            if (name[0] == '$') name = name.Substring(1); // Crop '$'
            string letterPattern = @"\D+";
            string digitPattern = @"\d+";
            Regex regex = new Regex(letterPattern); // Match column name
            MatchCollection matches = regex.Matches(name);
            x = ColumnIndexFromName(matches[0].Value);
            regex = new Regex(digitPattern); // Match row name
            matches = regex.Matches(name);
            y = RowIndexFromName(matches[0].Value);
        }
        static private int ColumnIndexFromName(string reference) // All hail TOLYA
        {
            List<int> Unconverted = new List<int>();
            int imax = -1; int k = 0;
            for (int i = 0; i < reference.Length; i++)
            {
                int AsciiNumber = reference[i];
                int OurAsciiNumber = AsciiNumber - 64;
                Unconverted.Add(OurAsciiNumber);
                imax++;
            }
            int maxstepin = imax; int ColumnCoeff1 = 0;

            for (k = 0; k <= imax; k++)
            {
                int temp = Unconverted[k] * (int)(Math.Pow(26, maxstepin));
                maxstepin--;
                ColumnCoeff1 = temp + ColumnCoeff1;
            }

            int ColumnCoeffFinal = ColumnCoeff1 - 1; //Бо в нас коефіцієнти з 0, а рахуємо з 1

            return ColumnCoeffFinal;
        }
        static private int RowIndexFromName(string reference) // All hail TOLYA
        {
            int RowCoeffFinal = Int32.Parse(reference) - 1;
            return RowCoeffFinal;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
    class Formulas
    {
        static public int FirstMainOperation(string expr, char operation)
            // Return the index of the main operation in the expression, or -1 if there is none
        {
            for (int i = 0; i < expr.Length; i++)
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
    }
}

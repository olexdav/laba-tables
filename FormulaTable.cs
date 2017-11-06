using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace FileManager
{
    class FormulaTable
    {
        private List<List<string>> table;

        public FormulaTable()
        {
            table = new List<List<string>>();
            table.Add(new List<string>()); // Add initial column
        }
        public int GetHeight()
        {
            return table[0].Count;
        }
        public int GetWidth()
        {
            return table.Count;
        }
        public void LoadFromFile(string path)
        {
            // Clear table
            Clear();
            // Read table from file to the list of lists
            using (var reader = new StreamReader(path))
            {
                bool firstLine = true;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] values = line.Split(';');

                    if (firstLine) // Create columns
                    {
                        for (int x = 0; x < values.Length; x++)
                            table.Add(new List<string>());
                        firstLine = false;
                    }

                    for (int x = 0; x < values.Length; x++) // Add row
                    {
                        table[x].Add(values[x]);
                    }
                }
            }
        }
        private void Clear()
        {
            int width = GetWidth();
            for (int x = 0; x < width; x++)
                table[x].Clear();
            table.Clear();
        }
        public void SaveToFile(string path)
        {

        }
        public void LoadToDataGridView(DataGridView dgv)
        {
            // Clear the grid view
            dgv.Rows.Clear();
            dgv.Columns.Clear();
            // Construct table
            for (int i = 0; i < GetWidth(); i++)
            {
                string columnName = IntToColumn(i);
                dgv.Columns.Add(columnName, columnName);
            }
            for (int i = 0; i < GetHeight(); i++)
            {
                dgv.Rows.Add(1);
                string rowName = (i+1).ToString();
                dgv.Rows[i].HeaderCell.Value = rowName;
            }
            // Copy values
            for (int y = 0; y < GetHeight(); y++)
                for (int x = 0; x < GetWidth(); x++)
                    dgv[x, y].Value = table[x][y];
        }
        public void EvaluateToDataGridView(DataGridView dgv)
        {
            try
            {
                for (int y = 0; y < GetHeight(); y++)
                    for (int x = 0; x < GetWidth(); x++)
                        dgv[x, y].Value = EvaluateCell(x, y);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private string EvaluateCell(int x, int y)
        {
            string str = table[x][y];
            if (str.Length == 0)
                return str;
            if (str[0] == '=')
                return EvaluateExpression(str.Substring(1));
            else return str;
        }
        private string EvaluateExpression(string expr)
        {
            if (expr.Length == 0) return expr;
            // Remove brackets that completely enclose the expression
            expr = Formulas.RemoveOuterBrackets(expr);
            // Break expression into basic operations (+,-,*,/)
            char mainOperation = ' ';
            int operationIndex = -1;
            string operations = "+-/*";
            foreach (char operation in operations)
            {
                operationIndex = Formulas.FirstMainOperation(expr, operation);
                if (operationIndex != -1)
                {
                    mainOperation = operation;
                    break;
                }
            }
            if (operationIndex != -1)
            {
                StringBuilder leftExpression = new StringBuilder("");
                for (int i = 0; i < operationIndex; i++)
                    leftExpression.Append(expr[i]);

                StringBuilder rightExpression = new StringBuilder("");
                for (int i = operationIndex + 1; i < expr.Length; i++)
                    rightExpression.Append(expr[i]);

                int a, b;
                Int32.TryParse(EvaluateExpression(leftExpression.ToString()), out a);
                Int32.TryParse(EvaluateExpression(rightExpression.ToString()), out b);
                if (mainOperation == '+') return (a + b).ToString();
                if (mainOperation == '-') return (a - b).ToString();
                if (mainOperation == '/') return (a / b).ToString();
                if (mainOperation == '*') return (a * b).ToString();
            }
            // No operations present, the expression should be a number
            return expr;
        }
        public void EditCell(int x, int y, string value)
        {

        }
        public bool IsCellValid(int x, int y)
        {
            return true;
        }
        public bool IsTableValid()
        {
            return true;
        }
        public void AddRow(DataGridView dgv)
        {
            for (int i = 0; i < GetWidth(); i++) // Add to table
                table[i].Add("");
            dgv.Rows.Add(1);  // Add to grid view
            string rowName = GetHeight().ToString();
            dgv.Rows[GetHeight() - 1].HeaderCell.Value = rowName;
        }
        public void AddColumn(DataGridView dgv)
        {
            string columnName = IntToColumn(GetWidth() - 1); // Add to table
            List<string> newColumn = new List<string>();
            for (int i = 0; i < GetHeight(); i++)
                newColumn.Add("");
            table.Add(newColumn);
            dgv.Columns.Add(columnName, columnName);  // Add to grid view
        }
        public void RemoveLastRow(DataGridView dgv)
        {
            if (GetHeight() == 0)
                return; // Cannot delete rows from an empty table

            // TODO: check validity before removing

            int height = GetHeight();  // Remove from table
            for (int i = 0; i < GetWidth(); i++)
            {
                table[i].RemoveAt(height - 1);
            }
            dgv.Rows.RemoveAt(GetHeight()); // Remove from grid view
        }
        public void RemoveLastColumn(DataGridView dgv)
        {
            if (GetWidth() == 1)
                return; // The table must always have at least 1 column

            // TODO: check validity before removing

            table.RemoveAt(GetWidth() - 1); // Remove from table
            dgv.Columns.RemoveAt(GetWidth()); // Remove from grid view
        }
        private string IntToColumn(int number)
            // Return column name for a particular index (e.g. 3 -> "C", 26 -> "AA")
        {
            char ch1 = (char)(number / 26 - 1);
            ch1 += 'A';
            if (ch1 == '@') ch1 = ' ';
            char ch2 = (char)(number % 26);
            ch2 += 'A';
            StringBuilder str = new StringBuilder("00");
            str[0] = ch1;
            str[1] = ch2;
            return str.ToString();
        }
    }
}

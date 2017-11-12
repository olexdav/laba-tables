using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace FileManager
{
    class FormulaTable
    {
        private List<List<string>> table;
        private string path; // Path leading to the file where the table is stored

        public FormulaTable()
        {
            table = new List<List<string>>();
            table.Add(new List<string>()); // Add initial column
        }
        public FormulaTable(int width, int height)
        {
            if (height < 0 || width < 1)
                throw new Exception("Invalid arguments in table constructor");
            table = new List<List<string>>();
            List<string> column = new List<string>();
            for (int y = 0; y < height; y++)
                column.Add("");
            for (int x = 0; x < width; x++)
                table.Add(new List<string>(column));
        }
        public int GetHeight()
        {
            return table[0].Count;
        }
        public int GetWidth()
        {
            return table.Count;
        }
        public void LoadFromFile(string filepath)
        {
            path = filepath;
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
        public void SaveToFile()
        {
            var csv = new StringBuilder();
            for (int y = 0; y < GetHeight(); y++)
                for (int x = 0; x < GetWidth(); x++)
                {
                    csv.Append(table[x][y]);
                    if (x == GetWidth() - 1)
                        csv.AppendLine();
                    else
                        csv.Append(';');
                }
            File.WriteAllText(path, csv.ToString());
        }
        public void SaveToFile(string filePath)
        {
            path = filePath;
            SaveToFile();
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
            for (int y = 0; y < GetHeight(); y++)
                for (int x = 0; x < GetWidth(); x++)
                    try
                    {
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
            // Crop '='
            if (expr[0] == '=') expr = expr.Substring(1);
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
            // No operations present, the expression could be a reference
            if (expr[0] == '$') // Reference symbol
            {
                int x, y;
                expr = expr.Substring(1); // Trim '$'
                Formulas.GetCellCoordinatesFromName(expr, out x, out y);
                expr = GetCell(x, y);
                return EvaluateExpression(expr); // Evaluate expression in the referenced cell
            }
            // Not a reference, it is a number
            return expr;
        }
        public string GetCell(int x, int y)
        {
            return table[x][y];
        }
        public bool EditCell(int x, int y, string value)
        {
            string oldValue = table[x][y];
            table[x][y] = value;
            if (!IsCellValid(x, y)) // Check if references in a cell are valid
            {
                table[x][y] = oldValue;
                MessageBox.Show("Invalid reference");
                return false;
            }
            return true;
        }
        private bool CheckAdress(int homeX, int homeY, int currX, int currY) // Ave YARIK
            // Check recursively whether the cell (homeX, homeY) has any invalid references
        {
            string cell = GetCell(currX, currY);
            string pattern = @"\$[A-Z]+[0-9]+";
            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(cell);
            foreach (Match match in matches)
            {
                int referenceX = 0;
                int referenceY = 0;
                string reference = match.Value.ToString();
                Formulas.GetCellCoordinatesFromName(reference, out referenceX, out referenceY);
                if (referenceX < 0 || referenceY < 0 ||
                    referenceX >= GetWidth() || referenceY >= GetHeight())
                    return false; // Reference out of table bounds
                if (referenceX == homeX && referenceY == homeY)
                    return false; // Cycle found
                if (!CheckAdress(homeX, homeY, referenceX, referenceY))
                    return false; // Cycle found somewhere deeper in the recursion tree
            }
            return true;
        }
        public bool IsCellValid(int x, int y)
            // Check whether the cell (x, y) has any invalid references
        {
            return CheckAdress(x, y, x, y);
        }
        public bool IsTableValid()
        {
            for (int y = 0; y < GetHeight(); y++)
                for (int x = 0; x < GetWidth(); x++)
                    if (!IsCellValid(x, y))
                        return false;
            return true;
        }
        private void AppendRow(DataGridView dgv)
            // Add row to the bottom of the table
        {
            for (int i = 0; i < GetWidth(); i++) // Add to table
                table[i].Add("");
            dgv.Rows.Add(1);  // Add to grid view
            string rowName = GetHeight().ToString();
            dgv.Rows[GetHeight() - 1].HeaderCell.Value = rowName;
        }
        private void InsertRow(DataGridView dgv, int rowIndex)
            // Insert a new row above a row with a particular index
        {
            AppendRow(dgv); // Add row to the bottom
            for (int x = 0; x < GetWidth(); x++) // Shift the table down one row
                for (int y = GetHeight()-1; y > rowIndex; y--)
                {
                    string value = GetCell(x, y - 1);
                    EditCell(x, y, value);
                }
            for (int x = 0; x < GetWidth(); x++) // Erase contents of the new row
                EditCell(x, rowIndex, "");
            LoadToDataGridView(dgv); // Display changes
        }
        public void AddRow(DataGridView dgv)
            // Add row above the selected cell or at the bottom of the table
        {
            if (dgv.SelectedCells.Count > 0) // Insert row before the selected cell
            {
                int y = dgv.SelectedCells[0].RowIndex;
                InsertRow(dgv, y);
            }
            else
            { // Append row to the end of the table
                AppendRow(dgv);
            }
        }
        public void AppendColumn(DataGridView dgv)
            // Add column to the right of the table
        {
            string columnName = IntToColumn(GetWidth() - 1); // Add to table
            List<string> newColumn = new List<string>();
            for (int i = 0; i < GetHeight(); i++)
                newColumn.Add("");
            table.Add(newColumn);
            dgv.Columns.Add(columnName, columnName);  // Add to grid view
        }
        public void InsertColumn(DataGridView dgv, int columnIndex)
            // Insert a new column to the left of the row with a particular index
        {
            AppendColumn(dgv); // Add column to the right
            for (int x = GetWidth()-1; x > columnIndex; x--) // Shift the table one column to the right
                for (int y = 0; y < GetHeight(); y++)
                {
                    string value = GetCell(x - 1, y);
                    EditCell(x, y, value);
                }
            for (int y = 0; y < GetHeight(); y++) // Erase contents of the new column
                EditCell(columnIndex, y, "");
            LoadToDataGridView(dgv); // Display changes
        }
        public void AddColumn(DataGridView dgv)
            // Add a column to the left of the selected cell or at the bottom of hte table
        {
            if (dgv.SelectedCells.Count > 0) // Insert column to the left of the selected cell
            {
                int x = dgv.SelectedCells[0].ColumnIndex;
                InsertColumn(dgv, x);
            }
            else
            { // Append row to the end of the table
                AppendColumn(dgv);
            }
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

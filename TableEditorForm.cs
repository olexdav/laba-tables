using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace FileManager
{
    public partial class TableEditorForm : Form
    {
        private FormulaTable table;

        public TableEditorForm()
        {
            InitializeComponent();
            table = new FormulaTable();
        }
        public TableEditorForm(string path)
        {
            InitializeComponent();
            table = new FormulaTable();
            table.LoadToDataGridView(dataGridView); // Synchronize
            try
            {
                table.LoadFromFile(path);
                table.LoadToDataGridView(dataGridView);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void addRowButton_Click(object sender, EventArgs e)
        {
            table.AddRow(dataGridView);
        }
        private void addColumnButton_Click(object sender, EventArgs e)
        {
            table.AddColumn(dataGridView);
        }
        private void removeRowButton_Click(object sender, EventArgs e)
        {
            table.RemoveLastRow(dataGridView);
        }
        private void removeColumnButton_Click(object sender, EventArgs e)
        {
            table.RemoveLastColumn(dataGridView);
        }
        private void rawTableButton_Click(object sender, EventArgs e)
        {
            table.LoadToDataGridView(dataGridView);
        }
        private void evaluateButton_Click(object sender, EventArgs e)
        {
            table.EvaluateToDataGridView(dataGridView);
        }
    }
}

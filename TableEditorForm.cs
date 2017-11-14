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
        bool evaluationMode; // Mode where each expression is evaluated

        public TableEditorForm()
        {
            InitializeComponent();
            table = new FormulaTable();
            evaluationMode = false;
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
            evaluationMode = false;
        }
        private void evaluateButton_Click(object sender, EventArgs e)
        {
            table.EvaluateToDataGridView(dataGridView);
            evaluationMode = true;
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            table.SaveToFile();
        }
        private void dataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                int x = e.ColumnIndex;
                int y = e.RowIndex;
                var val = dataGridView[x, y].Value;
                string newText;
                if (val == null) newText = "";
                else newText = val.ToString();
                bool success = table.EditCell(x, y, newText);
                if (!success) // Cancel editing
                    dataGridView[x, y].Value = table.GetCell(x, y);

                if (success && evaluationMode) // Reevaluate cells
                    table.EvaluateToDataGridView(dataGridView);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void TableEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        { // Send a confirmation before closing the editor
            if (!this.IsDisposed)
            {
                var result = MessageBox.Show("Are you sure you want to close the editor?\nThe file may not be saved!",
                                         "Close editor",
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Question);
                e.Cancel = (result == DialogResult.No);
            }
        }

        private void TableEditorForm_Click(object sender, EventArgs e)
        {
            dataGridView.ClearSelection();
        }
    }
}

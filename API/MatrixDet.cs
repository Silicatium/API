using Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace API
{
    public partial class MatrixDet : Form
    {
        private ViewModel viewModel;
        public MatrixDet()
        {
            InitializeComponent();

            viewModel = new ViewModel(); //Подключение ViewModel
            viewModel.observer += new EventHandler(this.Updates); //Добавление подписок
        }

        private void Updates(object sender, EventArgs e)
        {
            textBox.Text = viewModel.textGet();
        }

        private void numericUpDownN_ValueChanged(object sender, EventArgs e) //Разрядность матрицы
        {
            dataGridView.RowCount = (int)numericUpDownN.Value;
            dataGridView.ColumnCount = (int)numericUpDownN.Value;

            for (int i = 0; i < dataGridView.RowCount; i++)
            {
                for (int j = 0; j < dataGridView.ColumnCount; j++)
                    dataGridView.Rows[i].Cells[j].Value = 0;

                dataGridView.Rows[i].HeaderCell.Value = (i + 1).ToString();
                dataGridView.Columns[i].HeaderCell.Value = (i + 1).ToString();
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            viewModel.MatrixSend(dataGridView);
        }

        private void buttonStatus_Click(object sender, EventArgs e)
        {
            viewModel.StatusRecieve();
        }

        private void buttonResult_Click(object sender, EventArgs e)
        {
            viewModel.ResultRecieve();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            viewModel.TaskCancel();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp3
{
    public partial class Form1 : Form
    {
        private readonly string filePath = "todos.txt";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lstTodos.View = View.Details;
            lstTodos.CheckBoxes = true;
            lstTodos.FullRowSelect = true;
            lstTodos.GridLines = true;

            lstTodos.Columns.Clear();
            lstTodos.Columns.Add("할 일", 400);

            lstTodos.ItemChecked += LstTodos_ItemChecked;
            lstTodos.SelectedIndexChanged += LstTodos_SelectedIndexChanged;
        }

        private void LstTodos_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            var item = e.Item;

            if (item.Checked)
            {
                item.ForeColor = Color.Gray;
                item.Font = new Font(item.Font, FontStyle.Strikeout);
            }
            else
            {
                item.ForeColor = Color.Black;
                item.Font = new Font(item.Font, FontStyle.Regular);
            }

            lstTodos.Invalidate();
            lstTodos.Update();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string text = txtTodo.Text.ToString();
            if (string.IsNullOrWhiteSpace(txtTodo.Text))
            {
                MessageBox.Show("할 일을 입력해주세요!");
                return;
            }

            ListViewItem item = new ListViewItem(text);
            item.Checked = false;
            lstTodos.Items.Add(item);
            txtTodo.Clear();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (lstTodos.SelectedItems.Count == 0)
            {
                MessageBox.Show("수정할 항목을 선택하세요!");
                return;
            }

            string newText = txtTodo.Text.Trim();
            if (string.IsNullOrWhiteSpace(newText))
            {
                MessageBox.Show("수정할 내용을 선택하세요!");
                return;
            }

            lstTodos.SelectedItems[0].Text = newText;
            txtTodo.Clear();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lstTodos.CheckedItems.Count > 0)
            {
                for (int i = lstTodos.CheckedItems.Count - 1; i >= 0; i--)
                    lstTodos.Items.Remove(lstTodos.CheckedItems[i]);
             }
             else if (lstTodos.SelectedItems.Count > 0)
             {
                lstTodos.Items.Remove(lstTodos.SelectedItems[0]);
             }
             else
             {
                MessageBox.Show("삭제할 항목을 선택하거나 체크하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
             }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            List<string> lines = new List<string>();
            foreach (ListViewItem item in lstTodos.Items)
            {
                lines.Add($"{item.Checked} | {item.Text}");
            }

            File.WriteAllLines(filePath, lines, Encoding.UTF8);
            MessageBox.Show("저장 완료!");
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadTasks();
        }
        private void LoadTasks()
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show("저장된 파일이 없습니다.");
                return;
            }

            try
            {
                lstTodos.Items.Clear();
                var lines = File.ReadAllLines(filePath, Encoding.UTF8);

                foreach (var line in lines)
                {
                    var parts = line.Split('|');
                    if (parts.Length == 2)
                    {
                        bool isChecked= bool.TryParse(parts[0].Trim(), out bool result) && result;
                        string text = parts[1].Trim();

                        ListViewItem item = new ListViewItem(text);
                        item.Checked = isChecked;

                        if (isChecked)
                        {
                            item.ForeColor = Color.Gray;
                            item.Font = new Font(lstTodos.Font, FontStyle.Strikeout);
                        }
                        
                        lstTodos.Items.Add(item);

                    }
                }

                MessageBox.Show("불러오기 완료!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("불러오기 중 오류:" + ex.Message);
            }
        }
        private void LstTodos_SelectedIndexChanged(object sender, EventArgs e)
        {
           if (lstTodos.SelectedItems.Count > 0)
            {
                txtTodo.Text = lstTodos.SelectedItems[0].Text;
            }
        }
    }
}

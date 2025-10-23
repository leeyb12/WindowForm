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

            // 체크박스를 클릭하게 되면 글자 색이 회색으로 변하고 중간에 줄이 있는 텍스트가 생성됨.
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
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // txtTodo라는 이름의 텍스트 상자에 입력된 텍스트를 가져와서, 그 값을 text라는 문자열 변수에 저장한다.
            string text = txtTodo.Text.ToString();
            // txtTodo에 아무것도 입력되지 않았거나 공백만 있는 경우
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
            // lstTodos 목록에서 아무 항목에 선택하지 않았을 경우
            if (lstTodos.SelectedItems.Count == 0)
            {
                MessageBox.Show("수정할 항목을 선택하세요!");
                return;
            }

            // Trim() 메서드는 현재 문자열에서 모든 선행 및 후행 공백 문자를 제거함
            string newText = txtTodo.Text.Trim();
            // 지정된 문자열이 null이거나 비어 있을 경우
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
            // lstTodos라는 목록에서 체크(선택)된 항목의 개수가 0보다 클 경우
            if (lstTodos.CheckedItems.Count > 0)
            {
                // 마지막으로 체크한 항목에서 첫 번째 항목으로 뒤로 루프함
                for (int i = lstTodos.CheckedItems.Count - 1; i >= 0; i--)
                    lstTodos.Items.Remove(lstTodos.CheckedItems[i]);
             }
             else
             {
                MessageBox.Show("삭제할 항목을 선택하거나 체크하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
             }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // 문자열을 저장하는 리스트(List<string> lines)를 새로 만드는 것(new List<string>())을 의미함
            List<string> lines = new List<string>();

            // lstTodo라는 liveview 컨트롤의 모든 항목을 반복하고, ListViewItem 개체에 있는 항목에 컨트롤의 단일 행에 대한 모든 정보를 포함함.
            foreach (ListViewItem item in lstTodos.Items)
            {
                lines.Add($"{item.Checked} | {item.Text}");
            }
            // 지정된 경로의 파일(filePath)에 배열(lines)을 UTF-8 인코딩(Encoding.UTF8)으로 저장하는 기능을 수행함
            File.WriteAllLines(filePath, lines, Encoding.UTF8);
            MessageBox.Show("저장 완료!");
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadTasks();
        }
        private void LoadTasks()
        {
            // 지정된 파일이 없을 경우
            // Exists 메서드(File.Exists)는 지정된 파일이 있는지 여부를 확인
            if (!File.Exists(filePath))
            {
                MessageBox.Show("저장된 파일이 없습니다.");
                return;
            }

            try
            {
                lstTodos.Items.Clear();
                // ReadAllLines 메서드는 텍스트 파일을 읽고 파일의 모든 줄을 문자열 배열로 읽어 들인 다음 파일을 닫음.
                var lines = File.ReadAllLines(filePath, Encoding.UTF8);

                foreach (var line in lines)
                {
                    var parts = line.Split('|');
                    if (parts.Length == 2)
                    {
                        // parts[0](앞뒤 공백 제거)의 값을 불(bool)로 변환(TryParse)하려 시도하고, 변환이 성공했으며 값이 true일 때만 isChecked를 true로 설정
                        bool isChecked = bool.TryParse(parts[0].Trim(), out bool result) && result;
                        // parts[1]에서 앞뒤 공백을 모두 제거한 후, 그 결과를 text라는 새로운 문자열 변수에 저장함.
                        string text = parts[1].Trim();

                        ListViewItem item = new ListViewItem(text);
                        item.Checked = isChecked;
                    
                        if (isChecked)
                        {
                            // ForeColor 속성은 웹 서버 컨트롤의 전경색(텍스트 색)을 가져오거나 설정합니다.
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

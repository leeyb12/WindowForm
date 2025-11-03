using System;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        // 선택한 파일의 경로를 저장하는 변수
        private string selectedFilePath = string.Empty;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 프로그램 시작 시 최근 경로를 불러올 수 있도록 설정(옵션)
            LoadSaveFolders();
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "복사할 파일 선택";
            ofd.Filter = "모든 파일 (*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                selectedFilePath = ofd.FileName;
                textBoxPath.Text = selectedFilePath;
            }
        }
        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "복사할 대상 폴더를 선택하세요.";

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string folderPath = fbd.SelectedPath;
                comboBoxFolder.Text = folderPath;

                // 중복이 아니면 ComboBox에 추가
                if (!comboBoxFolder.Items.Contains(folderPath))
                {
                    comboBoxFolder.Items.Add(folderPath);
                    saveFolderList(); // 저장
                }
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                MessageBox.Show("먼저 복사할 파일을 선택하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string destinationFolder = comboBoxFolder.Text.Trim();
            if (string.IsNullOrEmpty(destinationFolder))
            {
                MessageBox.Show("복사할 폴더를 선택하거나 입력하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                if (!Directory.Exists(destinationFolder))
                    Directory.CreateDirectory(destinationFolder);

                string fileName = Path.GetFileName(selectedFilePath);
                string destinationPath = Path.Combine(destinationFolder, fileName);

                File.Copy(selectedFilePath, destinationPath, true);

                MessageBox.Show($"복사 완료!\n{destinationPath}", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            catch (Exception ex)
            {
                MessageBox.Show($"복사 중 오류 발생:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void saveFolderList()
        {
            try
            {
                string filePath = Path.Combine(Application.StartupPath);
                File.WriteAllLines(filePath, GetComboBoxItems());
            }
            catch
            {

            }
        }
        private void LoadSaveFolders()
        {
            string filePath = Path.Combine(Application.StartupPath);
            if (File.Exists(filePath))
            {
                string[] folders = File.ReadAllLines(filePath);
                comboBoxFolder.Items.AddRange(folders);
            }
        }

        private string[] GetComboBoxItems()
        {
            string[] items = new string[comboBoxFolder.Items.Count];
            comboBoxFolder.Items.CopyTo(items, 0);
            return items;
        }
    }
}

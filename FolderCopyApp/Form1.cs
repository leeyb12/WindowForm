using System;
using System.IO;
using System.Windows.Forms;

namespace FolderCopyApp
{
    public partial class Form1 : Form
    {
        private string sourceFolderPath = string.Empty;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadSaveFolders();
        }

        // 원본 폴더 선택
        private void btnSelectSource_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "복사할 원본 폴더를 선택하세요.";

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                sourceFolderPath = fbd.SelectedPath;
                textBoxSource.Text = sourceFolderPath;
            }
        }

        // 대상 폴더 선택
        private void btnSelectDest_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "복사할 대상 폴더를 선택하세요.";

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                comboBoxDest.Text = fbd.SelectedPath;
                if (!comboBoxDest.Items.Contains(fbd.SelectedPath))
                {
                    comboBoxDest.Items.Add(fbd.SelectedPath);
                    SaveFolderList();
                }
            }
        }

        // 복사 실행
        private async void btnCopy_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(sourceFolderPath))
            {
                MessageBox.Show("복사할 원본 폴더를 먼저 선택하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string destinationFolder = comboBoxDest.Text.Trim();
            if (string.IsNullOrEmpty(destinationFolder))
            {
                MessageBox.Show("복사 대상 폴더를 지정하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 폴더가 없으면 생성
                if (!Directory.Exists(destinationFolder))
                    Directory.CreateDirectory(destinationFolder);

                string[] files = Directory.GetFiles(sourceFolderPath, "*", SearchOption.AllDirectories);

                foreach (string file in files)
                {
                    string relativePath = file.Substring(sourceFolderPath.Length + 1);
                    string destPath = Path.Combine(destinationFolder, relativePath);

                    string destDir = Path.GetDirectoryName(destPath);
                    if (!Directory.Exists(destDir))
                        Directory.CreateDirectory(destDir);

                    // 파일 복사
                    using (FileStream sourceStream = File.Open(file, FileMode.Open, FileAccess.Read))
                    using (FileStream destStream = File.Create(destPath))
                    {
                        await sourceStream.CopyToAsync(destStream);
                    }
                }
                MessageBox.Show("모든 파일이 성공적으로 복사되었습니다.", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"복사 중 오류 발생:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 폴더 목록 저장
        private void SaveFolderList()
        {
            try
            {
                string filePath = Path.Combine(Application.StartupPath);
                File.WriteAllLines(filePath, GetComboBoxItems());
            }
            catch { }
        }

        private void LoadSaveFolders()
        {
            string filePath = Path.Combine(Application.StartupPath);
            if (File.Exists(filePath))
            {
                string[] folders = File.ReadAllLines(filePath);
                comboBoxDest.Items.AddRange(folders);
            }
        }

        private string[] GetComboBoxItems()
        {
            string[] items = new string[comboBoxDest.Items.Count];
            comboBoxDest.Items.CopyTo(items, 0);
            return items;
        }
    }
}

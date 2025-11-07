namespace FolderCopyApp
{
    partial class Form1
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBoxSource = new System.Windows.Forms.TextBox();
            this.btnSelectSource = new System.Windows.Forms.Button();
            this.comboBoxDest = new System.Windows.Forms.ComboBox();
            this.btnSelectDest = new System.Windows.Forms.Button();
            this.btnCopy = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // textBoxSource
            // 
            this.textBoxSource.Location = new System.Drawing.Point(48, 56);
            this.textBoxSource.Name = "textBoxSource";
            this.textBoxSource.Size = new System.Drawing.Size(475, 25);
            this.textBoxSource.TabIndex = 0;
            // 
            // btnSelectSource
            // 
            this.btnSelectSource.Location = new System.Drawing.Point(568, 54);
            this.btnSelectSource.Name = "btnSelectSource";
            this.btnSelectSource.Size = new System.Drawing.Size(179, 27);
            this.btnSelectSource.TabIndex = 1;
            this.btnSelectSource.Text = "원본 폴더 선택";
            this.btnSelectSource.UseVisualStyleBackColor = true;
            this.btnSelectSource.Click += new System.EventHandler(this.btnSelectSource_Click);
            // 
            // comboBoxDest
            // 
            this.comboBoxDest.FormattingEnabled = true;
            this.comboBoxDest.Location = new System.Drawing.Point(48, 177);
            this.comboBoxDest.Name = "comboBoxDest";
            this.comboBoxDest.Size = new System.Drawing.Size(470, 23);
            this.comboBoxDest.TabIndex = 2;
            // 
            // btnSelectDest
            // 
            this.btnSelectDest.Location = new System.Drawing.Point(568, 175);
            this.btnSelectDest.Name = "btnSelectDest";
            this.btnSelectDest.Size = new System.Drawing.Size(179, 25);
            this.btnSelectDest.TabIndex = 3;
            this.btnSelectDest.Text = "대상 폴더 지정";
            this.btnSelectDest.UseVisualStyleBackColor = true;
            this.btnSelectDest.Click += new System.EventHandler(this.btnSelectDest_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.Location = new System.Drawing.Point(500, 224);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(247, 78);
            this.btnCopy.TabIndex = 4;
            this.btnCopy.Text = "복사하기";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(48, 342);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(699, 96);
            this.richTextBox1.TabIndex = 5;
            this.richTextBox1.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.btnCopy);
            this.Controls.Add(this.btnSelectDest);
            this.Controls.Add(this.comboBoxDest);
            this.Controls.Add(this.btnSelectSource);
            this.Controls.Add(this.textBoxSource);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxSource;
        private System.Windows.Forms.Button btnSelectSource;
        private System.Windows.Forms.ComboBox comboBoxDest;
        private System.Windows.Forms.Button btnSelectDest;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.RichTextBox richTextBox1;
    }
}


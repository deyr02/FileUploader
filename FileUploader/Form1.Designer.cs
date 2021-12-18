
namespace FileUploader
{
    partial class FileUploader
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txt_fileName = new System.Windows.Forms.TextBox();
            this.btn_select = new System.Windows.Forms.Button();
            this.uploadPanel = new System.Windows.Forms.Panel();
            this.btn_upload = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.chk_AZURE = new System.Windows.Forms.CheckBox();
            this.chk_FTP = new System.Windows.Forms.CheckBox();
            this.uploadPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // txt_fileName
            // 
            this.txt_fileName.Location = new System.Drawing.Point(134, 85);
            this.txt_fileName.Name = "txt_fileName";
            this.txt_fileName.Size = new System.Drawing.Size(311, 23);
            this.txt_fileName.TabIndex = 0;
            this.txt_fileName.Text = "File Name";
            // 
            // btn_select
            // 
            this.btn_select.Location = new System.Drawing.Point(475, 85);
            this.btn_select.Name = "btn_select";
            this.btn_select.Size = new System.Drawing.Size(129, 23);
            this.btn_select.TabIndex = 1;
            this.btn_select.Text = "Select File";
            this.btn_select.UseVisualStyleBackColor = true;
            this.btn_select.Click += new System.EventHandler(this.btn_select_Click);
            // 
            // uploadPanel
            // 
            this.uploadPanel.Controls.Add(this.btn_upload);
            this.uploadPanel.Controls.Add(this.label2);
            this.uploadPanel.Controls.Add(this.label1);
            this.uploadPanel.Controls.Add(this.chk_AZURE);
            this.uploadPanel.Controls.Add(this.chk_FTP);
            this.uploadPanel.Location = new System.Drawing.Point(119, 152);
            this.uploadPanel.Name = "uploadPanel";
            this.uploadPanel.Size = new System.Drawing.Size(503, 228);
            this.uploadPanel.TabIndex = 2;
            this.uploadPanel.Visible = false;
            // 
            // btn_upload
            // 
            this.btn_upload.Location = new System.Drawing.Point(356, 181);
            this.btn_upload.Name = "btn_upload";
            this.btn_upload.Size = new System.Drawing.Size(129, 23);
            this.btn_upload.TabIndex = 3;
            this.btn_upload.Text = "Upload FIle";
            this.btn_upload.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label2.Location = new System.Drawing.Point(14, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(115, 21);
            this.label2.TabIndex = 2;
            this.label2.Text = "File upload to";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 23);
            this.label1.TabIndex = 0;
            // 
            // chk_AZURE
            // 
            this.chk_AZURE.AutoSize = true;
            this.chk_AZURE.Location = new System.Drawing.Point(36, 103);
            this.chk_AZURE.Name = "chk_AZURE";
            this.chk_AZURE.Size = new System.Drawing.Size(56, 19);
            this.chk_AZURE.TabIndex = 1;
            this.chk_AZURE.Text = "Azure";
            this.chk_AZURE.UseVisualStyleBackColor = true;
            // 
            // chk_FTP
            // 
            this.chk_FTP.AutoSize = true;
            this.chk_FTP.Location = new System.Drawing.Point(36, 60);
            this.chk_FTP.Name = "chk_FTP";
            this.chk_FTP.Size = new System.Drawing.Size(80, 19);
            this.chk_FTP.TabIndex = 0;
            this.chk_FTP.Text = "FTP Server";
            this.chk_FTP.UseVisualStyleBackColor = true;
            // 
            // FileUploader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.uploadPanel);
            this.Controls.Add(this.btn_select);
            this.Controls.Add(this.txt_fileName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FileUploader";
            this.Text = "File Uploader";
            this.uploadPanel.ResumeLayout(false);
            this.uploadPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_fileName;
        private System.Windows.Forms.Button btn_select;
        private System.Windows.Forms.Panel uploadPanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chk_AZURE;
        private System.Windows.Forms.CheckBox chk_FTP;
        private System.Windows.Forms.Button btn_upload;
        private System.Windows.Forms.Label label2;
    }
}


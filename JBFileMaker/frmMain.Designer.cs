namespace JBFileMaker
{
    partial class frmMain
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
            this.btnMakeFile = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.listStatus = new System.Windows.Forms.ListBox();
            this.btnMet = new System.Windows.Forms.Button();
            this.btnAgri = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnMakeFile
            // 
            this.btnMakeFile.Location = new System.Drawing.Point(173, 13);
            this.btnMakeFile.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnMakeFile.Name = "btnMakeFile";
            this.btnMakeFile.Size = new System.Drawing.Size(154, 35);
            this.btnMakeFile.TabIndex = 0;
            this.btnMakeFile.Text = "수문학적 입력자료";
            this.btnMakeFile.UseVisualStyleBackColor = true;
            this.btnMakeFile.Click += new System.EventHandler(this.btnMakeFile_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.listStatus);
            this.groupBox2.Location = new System.Drawing.Point(12, 55);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(776, 495);
            this.groupBox2.TabIndex = 57;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Event Logs";
            // 
            // listStatus
            // 
            this.listStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listStatus.FormattingEnabled = true;
            this.listStatus.ItemHeight = 15;
            this.listStatus.Location = new System.Drawing.Point(12, 23);
            this.listStatus.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.listStatus.Name = "listStatus";
            this.listStatus.Size = new System.Drawing.Size(758, 454);
            this.listStatus.TabIndex = 25;
            // 
            // btnMet
            // 
            this.btnMet.Location = new System.Drawing.Point(12, 13);
            this.btnMet.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnMet.Name = "btnMet";
            this.btnMet.Size = new System.Drawing.Size(155, 35);
            this.btnMet.TabIndex = 58;
            this.btnMet.Text = "기상학적 가뭄 입력자료";
            this.btnMet.UseVisualStyleBackColor = true;
            this.btnMet.Click += new System.EventHandler(this.btnMet_Click);
            // 
            // btnAgri
            // 
            this.btnAgri.Location = new System.Drawing.Point(333, 13);
            this.btnAgri.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnAgri.Name = "btnAgri";
            this.btnAgri.Size = new System.Drawing.Size(155, 35);
            this.btnAgri.TabIndex = 59;
            this.btnAgri.Text = "농업적 가뭄 입력자료";
            this.btnAgri.UseVisualStyleBackColor = true;
            this.btnAgri.Click += new System.EventHandler(this.btnAgri_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 562);
            this.Controls.Add(this.btnAgri);
            this.Controls.Add(this.btnMet);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnMakeFile);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "frmMain";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnMakeFile;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListBox listStatus;
        private System.Windows.Forms.Button btnMet;
        private System.Windows.Forms.Button btnAgri;
    }
}


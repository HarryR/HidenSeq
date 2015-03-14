namespace HidenSeq
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.dlgOpenFile = new System.Windows.Forms.OpenFileDialog();
            this.status = new System.Windows.Forms.StatusStrip();
            this.status_txt = new System.Windows.Forms.ToolStripStatusLabel();
            this.tblMain = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btn_openfiles = new System.Windows.Forms.Button();
            this.bar_progress = new System.Windows.Forms.ProgressBar();
            this.lst_files = new System.Windows.Forms.ListView();
            this.clmFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmFileSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmAnalysis = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.backgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.fileSystemWatcher = new System.IO.FileSystemWatcher();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.status.SuspendLayout();
            this.tblMain.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher)).BeginInit();
            this.SuspendLayout();
            // 
            // dlgOpenFile
            // 
            this.dlgOpenFile.Multiselect = true;
            // 
            // status
            // 
            this.status.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.status_txt});
            this.status.Location = new System.Drawing.Point(0, 378);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(466, 22);
            this.status.TabIndex = 0;
            this.status.Text = "statusStrip1";
            // 
            // status_txt
            // 
            this.status_txt.BackColor = System.Drawing.SystemColors.Control;
            this.status_txt.Name = "status_txt";
            this.status_txt.Size = new System.Drawing.Size(0, 17);
            // 
            // tblMain
            // 
            this.tblMain.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tblMain.ColumnCount = 1;
            this.tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblMain.Controls.Add(this.tableLayoutPanel1, 0, 1);
            this.tblMain.Controls.Add(this.lst_files, 0, 0);
            this.tblMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblMain.Location = new System.Drawing.Point(0, 0);
            this.tblMain.Margin = new System.Windows.Forms.Padding(5);
            this.tblMain.Name = "tblMain";
            this.tblMain.RowCount = 2;
            this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tblMain.Size = new System.Drawing.Size(466, 378);
            this.tblMain.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.Controls.Add(this.btn_openfiles, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.bar_progress, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 346);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(460, 29);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // btn_openfiles
            // 
            this.btn_openfiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_openfiles.Location = new System.Drawing.Point(393, 3);
            this.btn_openfiles.Name = "btn_openfiles";
            this.btn_openfiles.Size = new System.Drawing.Size(64, 23);
            this.btn_openfiles.TabIndex = 0;
            this.btn_openfiles.Text = "...";
            this.btn_openfiles.UseVisualStyleBackColor = true;
            this.btn_openfiles.Click += new System.EventHandler(this.btn_openfiles_Click);
            // 
            // bar_progress
            // 
            this.bar_progress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bar_progress.Location = new System.Drawing.Point(3, 3);
            this.bar_progress.Name = "bar_progress";
            this.bar_progress.Size = new System.Drawing.Size(384, 23);
            this.bar_progress.TabIndex = 1;
            // 
            // lst_files
            // 
            this.lst_files.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmFileName,
            this.clmFileSize,
            this.clmAnalysis});
            this.lst_files.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lst_files.FullRowSelect = true;
            this.lst_files.GridLines = true;
            this.lst_files.HideSelection = false;
            this.lst_files.Location = new System.Drawing.Point(6, 6);
            this.lst_files.Margin = new System.Windows.Forms.Padding(6, 6, 6, 0);
            this.lst_files.Name = "lst_files";
            this.lst_files.Size = new System.Drawing.Size(454, 337);
            this.lst_files.TabIndex = 1;
            this.lst_files.UseCompatibleStateImageBehavior = false;
            this.lst_files.View = System.Windows.Forms.View.Details;
            // 
            // clmFileName
            // 
            this.clmFileName.Text = "File Name";
            this.clmFileName.Width = 290;
            // 
            // clmFileSize
            // 
            this.clmFileSize.Text = "Size";
            this.clmFileSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // clmAnalysis
            // 
            this.clmAnalysis.Text = "Analysis";
            this.clmAnalysis.Width = 100;
            // 
            // backgroundWorker
            // 
            this.backgroundWorker.WorkerReportsProgress = true;
            this.backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_DoWork);
            // 
            // fileSystemWatcher
            // 
            this.fileSystemWatcher.EnableRaisingEvents = true;
            this.fileSystemWatcher.IncludeSubdirectories = true;
            this.fileSystemWatcher.NotifyFilter = System.IO.NotifyFilters.FileName;
            this.fileSystemWatcher.SynchronizingObject = this;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(466, 400);
            this.Controls.Add(this.tblMain);
            this.Controls.Add(this.status);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "HidenSeq Analyzer";
            this.status.ResumeLayout(false);
            this.status.PerformLayout();
            this.tblMain.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog dlgOpenFile;
        private System.Windows.Forms.StatusStrip status;
        private System.Windows.Forms.ToolStripStatusLabel status_txt;
        private System.Windows.Forms.TableLayoutPanel tblMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btn_openfiles;
        private System.Windows.Forms.ListView lst_files;
        private System.Windows.Forms.ProgressBar bar_progress;
        private System.Windows.Forms.ColumnHeader clmFileName;
        private System.Windows.Forms.ColumnHeader clmFileSize;
        private System.Windows.Forms.ColumnHeader clmAnalysis;
        private System.ComponentModel.BackgroundWorker backgroundWorker;
        private System.IO.FileSystemWatcher fileSystemWatcher;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
    }
}


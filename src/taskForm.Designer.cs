namespace furdown
{
    partial class taskForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(taskForm));
            this.mainTabControl = new System.Windows.Forms.TabControl();
            this.tasksTab = new System.Windows.Forms.TabPage();
            this.galleryGroupBox = new System.Windows.Forms.GroupBox();
            this.galleryDescrCheckBox = new System.Windows.Forms.CheckBox();
            this.galleryDownloadBtn = new System.Windows.Forms.Button();
            this.galleryUrlBox = new System.Windows.Forms.TextBox();
            this.settingsTab = new System.Windows.Forms.TabPage();
            this.descrFilenameLabel = new System.Windows.Forms.Label();
            this.descrFilenameBox = new System.Windows.Forms.TextBox();
            this.applyNSaveBtn = new System.Windows.Forms.Button();
            this.templateHelpBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.filenameTemplateLabel = new System.Windows.Forms.Label();
            this.filenameTemplateBox = new System.Windows.Forms.TextBox();
            this.systemPathLabel = new System.Windows.Forms.Label();
            this.systemPathBrowse = new System.Windows.Forms.Button();
            this.systemPathBox = new System.Windows.Forms.TextBox();
            this.downloadPathLabel = new System.Windows.Forms.Label();
            this.downloadPathBrowse = new System.Windows.Forms.Button();
            this.downloadPathBox = new System.Windows.Forms.TextBox();
            this.submUrlsGroupBox = new System.Windows.Forms.GroupBox();
            this.submUrlsTextBox = new System.Windows.Forms.TextBox();
            this.submUrlsDescrCheckBox = new System.Windows.Forms.CheckBox();
            this.submUrlsLoadPrvBtn = new System.Windows.Forms.Button();
            this.submUrlsLoadFileBtn = new System.Windows.Forms.Button();
            this.submUrlsDownloadBtn = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.mainTabControl.SuspendLayout();
            this.tasksTab.SuspendLayout();
            this.galleryGroupBox.SuspendLayout();
            this.settingsTab.SuspendLayout();
            this.submUrlsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainTabControl
            // 
            this.mainTabControl.Controls.Add(this.tasksTab);
            this.mainTabControl.Controls.Add(this.settingsTab);
            this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTabControl.Location = new System.Drawing.Point(0, 0);
            this.mainTabControl.Name = "mainTabControl";
            this.mainTabControl.SelectedIndex = 0;
            this.mainTabControl.Size = new System.Drawing.Size(705, 343);
            this.mainTabControl.TabIndex = 0;
            // 
            // tasksTab
            // 
            this.tasksTab.AutoScroll = true;
            this.tasksTab.Controls.Add(this.submUrlsGroupBox);
            this.tasksTab.Controls.Add(this.galleryGroupBox);
            this.tasksTab.Location = new System.Drawing.Point(4, 22);
            this.tasksTab.Name = "tasksTab";
            this.tasksTab.Padding = new System.Windows.Forms.Padding(3);
            this.tasksTab.Size = new System.Drawing.Size(697, 317);
            this.tasksTab.TabIndex = 0;
            this.tasksTab.Text = "Tasks";
            this.tasksTab.UseVisualStyleBackColor = true;
            // 
            // galleryGroupBox
            // 
            this.galleryGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.galleryGroupBox.Controls.Add(this.galleryDescrCheckBox);
            this.galleryGroupBox.Controls.Add(this.galleryDownloadBtn);
            this.galleryGroupBox.Controls.Add(this.galleryUrlBox);
            this.galleryGroupBox.Location = new System.Drawing.Point(8, 6);
            this.galleryGroupBox.Name = "galleryGroupBox";
            this.galleryGroupBox.Size = new System.Drawing.Size(681, 100);
            this.galleryGroupBox.TabIndex = 0;
            this.galleryGroupBox.TabStop = false;
            this.galleryGroupBox.Text = "Download gallery, scraps, favorites or a folder";
            // 
            // galleryDescrCheckBox
            // 
            this.galleryDescrCheckBox.AutoSize = true;
            this.galleryDescrCheckBox.Location = new System.Drawing.Point(6, 55);
            this.galleryDescrCheckBox.Name = "galleryDescrCheckBox";
            this.galleryDescrCheckBox.Size = new System.Drawing.Size(153, 17);
            this.galleryDescrCheckBox.TabIndex = 1;
            this.galleryDescrCheckBox.Text = "also save .htm descriptions";
            this.galleryDescrCheckBox.UseVisualStyleBackColor = true;
            // 
            // galleryDownloadBtn
            // 
            this.galleryDownloadBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.galleryDownloadBtn.Location = new System.Drawing.Point(512, 55);
            this.galleryDownloadBtn.Name = "galleryDownloadBtn";
            this.galleryDownloadBtn.Size = new System.Drawing.Size(163, 23);
            this.galleryDownloadBtn.TabIndex = 2;
            this.galleryDownloadBtn.Text = "Go!";
            this.galleryDownloadBtn.UseVisualStyleBackColor = true;
            this.galleryDownloadBtn.Click += new System.EventHandler(this.galleryDownloadBtn_Click);
            // 
            // galleryUrlBox
            // 
            this.galleryUrlBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.galleryUrlBox.Location = new System.Drawing.Point(6, 29);
            this.galleryUrlBox.Name = "galleryUrlBox";
            this.galleryUrlBox.Size = new System.Drawing.Size(669, 20);
            this.galleryUrlBox.TabIndex = 0;
            this.galleryUrlBox.Text = "https://www.furaffinity.net/gallery/jt47jade/";
            this.galleryUrlBox.Leave += new System.EventHandler(this.galleryUrlBox_Leave);
            // 
            // settingsTab
            // 
            this.settingsTab.AutoScroll = true;
            this.settingsTab.Controls.Add(this.descrFilenameLabel);
            this.settingsTab.Controls.Add(this.descrFilenameBox);
            this.settingsTab.Controls.Add(this.applyNSaveBtn);
            this.settingsTab.Controls.Add(this.templateHelpBox);
            this.settingsTab.Controls.Add(this.label4);
            this.settingsTab.Controls.Add(this.filenameTemplateLabel);
            this.settingsTab.Controls.Add(this.filenameTemplateBox);
            this.settingsTab.Controls.Add(this.systemPathLabel);
            this.settingsTab.Controls.Add(this.systemPathBrowse);
            this.settingsTab.Controls.Add(this.systemPathBox);
            this.settingsTab.Controls.Add(this.downloadPathLabel);
            this.settingsTab.Controls.Add(this.downloadPathBrowse);
            this.settingsTab.Controls.Add(this.downloadPathBox);
            this.settingsTab.Location = new System.Drawing.Point(4, 22);
            this.settingsTab.Name = "settingsTab";
            this.settingsTab.Padding = new System.Windows.Forms.Padding(3);
            this.settingsTab.Size = new System.Drawing.Size(697, 317);
            this.settingsTab.TabIndex = 1;
            this.settingsTab.Text = "Settings";
            this.settingsTab.UseVisualStyleBackColor = true;
            // 
            // descrFilenameLabel
            // 
            this.descrFilenameLabel.AutoSize = true;
            this.descrFilenameLabel.Location = new System.Drawing.Point(11, 272);
            this.descrFilenameLabel.Name = "descrFilenameLabel";
            this.descrFilenameLabel.Size = new System.Drawing.Size(148, 13);
            this.descrFilenameLabel.TabIndex = 15;
            this.descrFilenameLabel.Text = "Description filename template:";
            // 
            // descrFilenameBox
            // 
            this.descrFilenameBox.Location = new System.Drawing.Point(11, 288);
            this.descrFilenameBox.Name = "descrFilenameBox";
            this.descrFilenameBox.Size = new System.Drawing.Size(355, 20);
            this.descrFilenameBox.TabIndex = 14;
            // 
            // applyNSaveBtn
            // 
            this.applyNSaveBtn.Location = new System.Drawing.Point(11, 324);
            this.applyNSaveBtn.Name = "applyNSaveBtn";
            this.applyNSaveBtn.Size = new System.Drawing.Size(116, 27);
            this.applyNSaveBtn.TabIndex = 13;
            this.applyNSaveBtn.Text = "Save and apply";
            this.applyNSaveBtn.UseVisualStyleBackColor = true;
            this.applyNSaveBtn.Click += new System.EventHandler(this.applyNSaveBtn_Click);
            // 
            // templateHelpBox
            // 
            this.templateHelpBox.Location = new System.Drawing.Point(57, 156);
            this.templateHelpBox.Multiline = true;
            this.templateHelpBox.Name = "templateHelpBox";
            this.templateHelpBox.ReadOnly = true;
            this.templateHelpBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.templateHelpBox.Size = new System.Drawing.Size(306, 112);
            this.templateHelpBox.TabIndex = 12;
            this.templateHelpBox.Text = resources.GetString("templateHelpBox.Text");
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 156);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Help:";
            // 
            // filenameTemplateLabel
            // 
            this.filenameTemplateLabel.AutoSize = true;
            this.filenameTemplateLabel.Location = new System.Drawing.Point(8, 117);
            this.filenameTemplateLabel.Name = "filenameTemplateLabel";
            this.filenameTemplateLabel.Size = new System.Drawing.Size(100, 13);
            this.filenameTemplateLabel.TabIndex = 10;
            this.filenameTemplateLabel.Text = "Filenames template:";
            // 
            // filenameTemplateBox
            // 
            this.filenameTemplateBox.Location = new System.Drawing.Point(8, 133);
            this.filenameTemplateBox.Name = "filenameTemplateBox";
            this.filenameTemplateBox.Size = new System.Drawing.Size(355, 20);
            this.filenameTemplateBox.TabIndex = 9;
            // 
            // systemPathLabel
            // 
            this.systemPathLabel.AutoSize = true;
            this.systemPathLabel.Location = new System.Drawing.Point(8, 65);
            this.systemPathLabel.Name = "systemPathLabel";
            this.systemPathLabel.Size = new System.Drawing.Size(136, 13);
            this.systemPathLabel.TabIndex = 8;
            this.systemPathLabel.Text = "System and temp files path:";
            // 
            // systemPathBrowse
            // 
            this.systemPathBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.systemPathBrowse.Location = new System.Drawing.Point(322, 81);
            this.systemPathBrowse.Name = "systemPathBrowse";
            this.systemPathBrowse.Size = new System.Drawing.Size(41, 20);
            this.systemPathBrowse.TabIndex = 7;
            this.systemPathBrowse.Text = "...";
            this.systemPathBrowse.UseVisualStyleBackColor = true;
            this.systemPathBrowse.Click += new System.EventHandler(this.systemPathBrowse_Click);
            // 
            // systemPathBox
            // 
            this.systemPathBox.Location = new System.Drawing.Point(8, 81);
            this.systemPathBox.Name = "systemPathBox";
            this.systemPathBox.Size = new System.Drawing.Size(308, 20);
            this.systemPathBox.TabIndex = 6;
            // 
            // downloadPathLabel
            // 
            this.downloadPathLabel.AutoSize = true;
            this.downloadPathLabel.Location = new System.Drawing.Point(8, 14);
            this.downloadPathLabel.Name = "downloadPathLabel";
            this.downloadPathLabel.Size = new System.Drawing.Size(148, 13);
            this.downloadPathLabel.TabIndex = 5;
            this.downloadPathLabel.Text = "Location for downloaded files:";
            // 
            // downloadPathBrowse
            // 
            this.downloadPathBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.downloadPathBrowse.Location = new System.Drawing.Point(322, 30);
            this.downloadPathBrowse.Name = "downloadPathBrowse";
            this.downloadPathBrowse.Size = new System.Drawing.Size(41, 20);
            this.downloadPathBrowse.TabIndex = 4;
            this.downloadPathBrowse.Text = "...";
            this.downloadPathBrowse.UseVisualStyleBackColor = true;
            this.downloadPathBrowse.Click += new System.EventHandler(this.downloadPathBrowse_Click);
            // 
            // downloadPathBox
            // 
            this.downloadPathBox.Location = new System.Drawing.Point(8, 30);
            this.downloadPathBox.Name = "downloadPathBox";
            this.downloadPathBox.Size = new System.Drawing.Size(308, 20);
            this.downloadPathBox.TabIndex = 3;
            // 
            // submUrlsGroupBox
            // 
            this.submUrlsGroupBox.Controls.Add(this.submUrlsDownloadBtn);
            this.submUrlsGroupBox.Controls.Add(this.submUrlsLoadFileBtn);
            this.submUrlsGroupBox.Controls.Add(this.submUrlsLoadPrvBtn);
            this.submUrlsGroupBox.Controls.Add(this.submUrlsDescrCheckBox);
            this.submUrlsGroupBox.Controls.Add(this.submUrlsTextBox);
            this.submUrlsGroupBox.Location = new System.Drawing.Point(8, 112);
            this.submUrlsGroupBox.Name = "submUrlsGroupBox";
            this.submUrlsGroupBox.Size = new System.Drawing.Size(681, 112);
            this.submUrlsGroupBox.TabIndex = 1;
            this.submUrlsGroupBox.TabStop = false;
            this.submUrlsGroupBox.Text = "Download a list of submissions";
            // 
            // submUrlsTextBox
            // 
            this.submUrlsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.submUrlsTextBox.Location = new System.Drawing.Point(6, 19);
            this.submUrlsTextBox.Multiline = true;
            this.submUrlsTextBox.Name = "submUrlsTextBox";
            this.submUrlsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.submUrlsTextBox.Size = new System.Drawing.Size(493, 64);
            this.submUrlsTextBox.TabIndex = 0;
            this.submUrlsTextBox.Leave += new System.EventHandler(this.submUrlsTextBox_Leave);
            // 
            // submUrlsDescrCheckBox
            // 
            this.submUrlsDescrCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.submUrlsDescrCheckBox.AutoSize = true;
            this.submUrlsDescrCheckBox.Location = new System.Drawing.Point(6, 89);
            this.submUrlsDescrCheckBox.Name = "submUrlsDescrCheckBox";
            this.submUrlsDescrCheckBox.Size = new System.Drawing.Size(153, 17);
            this.submUrlsDescrCheckBox.TabIndex = 2;
            this.submUrlsDescrCheckBox.Text = "also save .htm descriptions";
            this.submUrlsDescrCheckBox.UseVisualStyleBackColor = true;
            // 
            // submUrlsLoadPrvBtn
            // 
            this.submUrlsLoadPrvBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.submUrlsLoadPrvBtn.Location = new System.Drawing.Point(512, 17);
            this.submUrlsLoadPrvBtn.Name = "submUrlsLoadPrvBtn";
            this.submUrlsLoadPrvBtn.Size = new System.Drawing.Size(163, 23);
            this.submUrlsLoadPrvBtn.TabIndex = 3;
            this.submUrlsLoadPrvBtn.Text = "Load previous";
            this.submUrlsLoadPrvBtn.UseVisualStyleBackColor = true;
            this.submUrlsLoadPrvBtn.Click += new System.EventHandler(this.submUrlsLoadPrvBtn_Click);
            // 
            // submUrlsLoadFileBtn
            // 
            this.submUrlsLoadFileBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.submUrlsLoadFileBtn.Location = new System.Drawing.Point(512, 46);
            this.submUrlsLoadFileBtn.Name = "submUrlsLoadFileBtn";
            this.submUrlsLoadFileBtn.Size = new System.Drawing.Size(163, 23);
            this.submUrlsLoadFileBtn.TabIndex = 4;
            this.submUrlsLoadFileBtn.Text = "Load from file";
            this.submUrlsLoadFileBtn.UseVisualStyleBackColor = true;
            this.submUrlsLoadFileBtn.Click += new System.EventHandler(this.submUrlsLoadFileBtn_Click);
            // 
            // submUrlsDownloadBtn
            // 
            this.submUrlsDownloadBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.submUrlsDownloadBtn.Location = new System.Drawing.Point(512, 83);
            this.submUrlsDownloadBtn.Name = "submUrlsDownloadBtn";
            this.submUrlsDownloadBtn.Size = new System.Drawing.Size(163, 23);
            this.submUrlsDownloadBtn.TabIndex = 5;
            this.submUrlsDownloadBtn.Text = "Go!";
            this.submUrlsDownloadBtn.UseVisualStyleBackColor = true;
            this.submUrlsDownloadBtn.Click += new System.EventHandler(this.submUrlsDownloadBtn_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "Text Files (*.txt, *.log)|*.text;*.txt;*.log|All Files|*.*";
            // 
            // taskForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(705, 343);
            this.Controls.Add(this.mainTabControl);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(420, 300);
            this.Name = "taskForm";
            this.Text = "furdown :: tasks";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.taskForm_FormClosed);
            this.Shown += new System.EventHandler(this.taskForm_Shown);
            this.mainTabControl.ResumeLayout(false);
            this.tasksTab.ResumeLayout(false);
            this.galleryGroupBox.ResumeLayout(false);
            this.galleryGroupBox.PerformLayout();
            this.settingsTab.ResumeLayout(false);
            this.settingsTab.PerformLayout();
            this.submUrlsGroupBox.ResumeLayout(false);
            this.submUrlsGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl mainTabControl;
        private System.Windows.Forms.TabPage tasksTab;
        private System.Windows.Forms.TabPage settingsTab;
        private System.Windows.Forms.TextBox templateHelpBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label filenameTemplateLabel;
        private System.Windows.Forms.TextBox filenameTemplateBox;
        private System.Windows.Forms.Label systemPathLabel;
        private System.Windows.Forms.Button systemPathBrowse;
        private System.Windows.Forms.TextBox systemPathBox;
        private System.Windows.Forms.Label downloadPathLabel;
        private System.Windows.Forms.Button downloadPathBrowse;
        private System.Windows.Forms.TextBox downloadPathBox;
        private System.Windows.Forms.Button applyNSaveBtn;
        private System.Windows.Forms.GroupBox galleryGroupBox;
        private System.Windows.Forms.TextBox galleryUrlBox;
        private System.Windows.Forms.Button galleryDownloadBtn;
        private System.Windows.Forms.Label descrFilenameLabel;
        private System.Windows.Forms.TextBox descrFilenameBox;
        private System.Windows.Forms.CheckBox galleryDescrCheckBox;
        private System.Windows.Forms.GroupBox submUrlsGroupBox;
        private System.Windows.Forms.Button submUrlsDownloadBtn;
        private System.Windows.Forms.Button submUrlsLoadFileBtn;
        private System.Windows.Forms.Button submUrlsLoadPrvBtn;
        private System.Windows.Forms.CheckBox submUrlsDescrCheckBox;
        private System.Windows.Forms.TextBox submUrlsTextBox;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    }
}
namespace School_Report
{
    partial class SchoolReportForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SchoolReportForm));
            m_tbxName = new TextBox();
            m_lblName = new Label();
            m_ctlGroupBoxGender = new GroupBox();
            m_rbDiverse = new RadioButton();
            m_rbFemale = new RadioButton();
            m_rbMale = new RadioButton();
            m_ctlReportParts = new TabControl();
            m_ctlTabPageCommunity = new TabPage();
            m_ctlTabPageWorkHabits = new TabPage();
            m_ctlTTabPageLearning = new TabPage();
            m_btnCreateNextWorseCommunity = new Button();
            m_btnCreateNextBetterCommunity = new Button();
            m_btnNextWorseCommunity = new Button();
            m_btnNextBestCommunity = new Button();
            m_tbxNextBestTextCommunity = new TextBox();
            m_tbxNextWorseTextCommunity = new TextBox();
            m_tbxCurrentTextCommunity = new TextBox();
            m_lblTextCommunity = new Label();
            m_tbxTextCommunity = new TextBox();
            m_dlgSelectFile = new OpenFileDialog();
            m_lblCurrentPoint = new Label();
            m_btnAddCurrentText = new Button();
            m_ctlUpDownPoints = new NumericUpDown();
            m_chkAddName = new CheckBox();
            m_oMenuStrip = new MenuStrip();
            m_oChooseTextsToolStripMenuItem = new ToolStripMenuItem();
            m_oAboutToolStripMenuItem = new ToolStripMenuItem();
            m_oLicenseToolStripMenuItem = new ToolStripMenuItem();
            m_btnCopyFullText = new Button();
            m_ctlGroupBoxGender.SuspendLayout();
            m_ctlReportParts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)m_ctlUpDownPoints).BeginInit();
            m_oMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // m_tbxName
            // 
            resources.ApplyResources(m_tbxName, "m_tbxName");
            m_tbxName.Name = "m_tbxName";
            m_tbxName.TextChanged += OnName_TextChanged;
            // 
            // m_lblName
            // 
            resources.ApplyResources(m_lblName, "m_lblName");
            m_lblName.Name = "m_lblName";
            // 
            // m_ctlGroupBoxGender
            // 
            resources.ApplyResources(m_ctlGroupBoxGender, "m_ctlGroupBoxGender");
            m_ctlGroupBoxGender.Controls.Add(m_rbDiverse);
            m_ctlGroupBoxGender.Controls.Add(m_rbFemale);
            m_ctlGroupBoxGender.Controls.Add(m_rbMale);
            m_ctlGroupBoxGender.Name = "m_ctlGroupBoxGender";
            m_ctlGroupBoxGender.TabStop = false;
            // 
            // m_rbDiverse
            // 
            resources.ApplyResources(m_rbDiverse, "m_rbDiverse");
            m_rbDiverse.Name = "m_rbDiverse";
            m_rbDiverse.TabStop = true;
            m_rbDiverse.UseVisualStyleBackColor = true;
            m_rbDiverse.CheckedChanged += OnGenderRadioBox_CheckedChanged;
            // 
            // m_rbFemale
            // 
            resources.ApplyResources(m_rbFemale, "m_rbFemale");
            m_rbFemale.Name = "m_rbFemale";
            m_rbFemale.TabStop = true;
            m_rbFemale.UseVisualStyleBackColor = true;
            m_rbFemale.CheckedChanged += OnGenderRadioBox_CheckedChanged;
            // 
            // m_rbMale
            // 
            resources.ApplyResources(m_rbMale, "m_rbMale");
            m_rbMale.Checked = true;
            m_rbMale.Name = "m_rbMale";
            m_rbMale.TabStop = true;
            m_rbMale.UseVisualStyleBackColor = true;
            m_rbMale.CheckedChanged += OnGenderRadioBox_CheckedChanged;
            // 
            // m_ctlReportParts
            // 
            resources.ApplyResources(m_ctlReportParts, "m_ctlReportParts");
            m_ctlReportParts.Controls.Add(m_ctlTabPageCommunity);
            m_ctlReportParts.Controls.Add(m_ctlTabPageWorkHabits);
            m_ctlReportParts.Controls.Add(m_ctlTTabPageLearning);
            m_ctlReportParts.Name = "m_ctlReportParts";
            m_ctlReportParts.SelectedIndex = 0;
            m_ctlReportParts.SelectedIndexChanged += m_ctlReportParts_SelectedIndexChanged;
            // 
            // m_ctlTabPageCommunity
            // 
            m_ctlTabPageCommunity.BackColor = SystemColors.Control;
            resources.ApplyResources(m_ctlTabPageCommunity, "m_ctlTabPageCommunity");
            m_ctlTabPageCommunity.Name = "m_ctlTabPageCommunity";
            // 
            // m_ctlTabPageWorkHabits
            // 
            resources.ApplyResources(m_ctlTabPageWorkHabits, "m_ctlTabPageWorkHabits");
            m_ctlTabPageWorkHabits.Name = "m_ctlTabPageWorkHabits";
            m_ctlTabPageWorkHabits.UseVisualStyleBackColor = true;
            // 
            // m_ctlTTabPageLearning
            // 
            resources.ApplyResources(m_ctlTTabPageLearning, "m_ctlTTabPageLearning");
            m_ctlTTabPageLearning.Name = "m_ctlTTabPageLearning";
            m_ctlTTabPageLearning.UseVisualStyleBackColor = true;
            // 
            // m_btnCreateNextWorseCommunity
            // 
            resources.ApplyResources(m_btnCreateNextWorseCommunity, "m_btnCreateNextWorseCommunity");
            m_btnCreateNextWorseCommunity.Name = "m_btnCreateNextWorseCommunity";
            m_btnCreateNextWorseCommunity.UseVisualStyleBackColor = true;
            m_btnCreateNextWorseCommunity.Click += OnCreateNextWorseCommunity_Click;
            // 
            // m_btnCreateNextBetterCommunity
            // 
            resources.ApplyResources(m_btnCreateNextBetterCommunity, "m_btnCreateNextBetterCommunity");
            m_btnCreateNextBetterCommunity.Name = "m_btnCreateNextBetterCommunity";
            m_btnCreateNextBetterCommunity.UseVisualStyleBackColor = true;
            m_btnCreateNextBetterCommunity.Click += OnCreateNextBetterCommunity_Click;
            // 
            // m_btnNextWorseCommunity
            // 
            resources.ApplyResources(m_btnNextWorseCommunity, "m_btnNextWorseCommunity");
            m_btnNextWorseCommunity.Name = "m_btnNextWorseCommunity";
            m_btnNextWorseCommunity.UseVisualStyleBackColor = true;
            m_btnNextWorseCommunity.Click += OnNextWorseCommunity_Click;
            // 
            // m_btnNextBestCommunity
            // 
            resources.ApplyResources(m_btnNextBestCommunity, "m_btnNextBestCommunity");
            m_btnNextBestCommunity.Name = "m_btnNextBestCommunity";
            m_btnNextBestCommunity.UseVisualStyleBackColor = true;
            m_btnNextBestCommunity.Click += OnNextBestCommunity_Click;
            // 
            // m_tbxNextBestTextCommunity
            // 
            resources.ApplyResources(m_tbxNextBestTextCommunity, "m_tbxNextBestTextCommunity");
            m_tbxNextBestTextCommunity.Name = "m_tbxNextBestTextCommunity";
            m_tbxNextBestTextCommunity.ReadOnly = true;
            // 
            // m_tbxNextWorseTextCommunity
            // 
            resources.ApplyResources(m_tbxNextWorseTextCommunity, "m_tbxNextWorseTextCommunity");
            m_tbxNextWorseTextCommunity.Name = "m_tbxNextWorseTextCommunity";
            m_tbxNextWorseTextCommunity.ReadOnly = true;
            // 
            // m_tbxCurrentTextCommunity
            // 
            resources.ApplyResources(m_tbxCurrentTextCommunity, "m_tbxCurrentTextCommunity");
            m_tbxCurrentTextCommunity.BackColor = SystemColors.ControlLightLight;
            m_tbxCurrentTextCommunity.Name = "m_tbxCurrentTextCommunity";
            m_tbxCurrentTextCommunity.ReadOnly = true;
            m_tbxCurrentTextCommunity.Click += OnCurrentAssessmentText_Click;
            // 
            // m_lblTextCommunity
            // 
            resources.ApplyResources(m_lblTextCommunity, "m_lblTextCommunity");
            m_lblTextCommunity.Name = "m_lblTextCommunity";
            // 
            // m_tbxTextCommunity
            // 
            resources.ApplyResources(m_tbxTextCommunity, "m_tbxTextCommunity");
            m_tbxTextCommunity.BackColor = SystemColors.ControlLightLight;
            m_tbxTextCommunity.Name = "m_tbxTextCommunity";
            m_tbxTextCommunity.ReadOnly = true;
            // 
            // m_dlgSelectFile
            // 
            m_dlgSelectFile.DefaultExt = "School-Report.xml";
            m_dlgSelectFile.FileName = "Texts.School-Report.xml";
            resources.ApplyResources(m_dlgSelectFile, "m_dlgSelectFile");
            m_dlgSelectFile.SelectReadOnly = false;
            m_dlgSelectFile.SupportMultiDottedExtensions = true;
            // 
            // m_lblCurrentPoint
            // 
            resources.ApplyResources(m_lblCurrentPoint, "m_lblCurrentPoint");
            m_lblCurrentPoint.Name = "m_lblCurrentPoint";
            // 
            // m_btnAddCurrentText
            // 
            resources.ApplyResources(m_btnAddCurrentText, "m_btnAddCurrentText");
            m_btnAddCurrentText.Name = "m_btnAddCurrentText";
            m_btnAddCurrentText.UseVisualStyleBackColor = true;
            m_btnAddCurrentText.Click += OnAddCurrentText_Click;
            // 
            // m_ctlUpDownPoints
            // 
            resources.ApplyResources(m_ctlUpDownPoints, "m_ctlUpDownPoints");
            m_ctlUpDownPoints.Name = "m_ctlUpDownPoints";
            m_ctlUpDownPoints.ValueChanged += OnUpDownPoints_ValueChanged;
            // 
            // m_chkAddName
            // 
            resources.ApplyResources(m_chkAddName, "m_chkAddName");
            m_chkAddName.Name = "m_chkAddName";
            m_chkAddName.UseVisualStyleBackColor = true;
            m_chkAddName.CheckedChanged += OnAddName_CheckedChanged;
            // 
            // m_oMenuStrip
            // 
            resources.ApplyResources(m_oMenuStrip, "m_oMenuStrip");
            m_oMenuStrip.Items.AddRange(new ToolStripItem[] { m_oChooseTextsToolStripMenuItem, m_oAboutToolStripMenuItem, m_oLicenseToolStripMenuItem });
            m_oMenuStrip.Name = "m_oMenuStrip";
            // 
            // m_oChooseTextsToolStripMenuItem
            // 
            m_oChooseTextsToolStripMenuItem.Name = "m_oChooseTextsToolStripMenuItem";
            resources.ApplyResources(m_oChooseTextsToolStripMenuItem, "m_oChooseTextsToolStripMenuItem");
            m_oChooseTextsToolStripMenuItem.Click += OnSelectFile_Click;
            // 
            // m_oAboutToolStripMenuItem
            // 
            m_oAboutToolStripMenuItem.Alignment = ToolStripItemAlignment.Right;
            m_oAboutToolStripMenuItem.Name = "m_oAboutToolStripMenuItem";
            resources.ApplyResources(m_oAboutToolStripMenuItem, "m_oAboutToolStripMenuItem");
            m_oAboutToolStripMenuItem.Click += OnAboutToolStripMenuItem_Click;
            // 
            // m_oLicenseToolStripMenuItem
            // 
            m_oLicenseToolStripMenuItem.Alignment = ToolStripItemAlignment.Right;
            m_oLicenseToolStripMenuItem.Name = "m_oLicenseToolStripMenuItem";
            resources.ApplyResources(m_oLicenseToolStripMenuItem, "m_oLicenseToolStripMenuItem");
            m_oLicenseToolStripMenuItem.Click += OnLicenseToolStripMenuItem_Click;
            // 
            // m_btnCopyFullText
            // 
            resources.ApplyResources(m_btnCopyFullText, "m_btnCopyFullText");
            m_btnCopyFullText.Name = "m_btnCopyFullText";
            m_btnCopyFullText.UseVisualStyleBackColor = true;
            m_btnCopyFullText.Click += OnCopyText_Click;
            // 
            // SchoolReportForm
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(m_btnCopyFullText);
            Controls.Add(m_chkAddName);
            Controls.Add(m_ctlUpDownPoints);
            Controls.Add(m_btnAddCurrentText);
            Controls.Add(m_lblCurrentPoint);
            Controls.Add(m_ctlReportParts);
            Controls.Add(m_btnCreateNextWorseCommunity);
            Controls.Add(m_ctlGroupBoxGender);
            Controls.Add(m_btnCreateNextBetterCommunity);
            Controls.Add(m_lblName);
            Controls.Add(m_btnNextWorseCommunity);
            Controls.Add(m_tbxName);
            Controls.Add(m_btnNextBestCommunity);
            Controls.Add(m_lblTextCommunity);
            Controls.Add(m_tbxNextBestTextCommunity);
            Controls.Add(m_tbxTextCommunity);
            Controls.Add(m_tbxNextWorseTextCommunity);
            Controls.Add(m_tbxCurrentTextCommunity);
            Controls.Add(m_oMenuStrip);
            MainMenuStrip = m_oMenuStrip;
            Name = "SchoolReportForm";
            m_ctlGroupBoxGender.ResumeLayout(false);
            m_ctlGroupBoxGender.PerformLayout();
            m_ctlReportParts.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)m_ctlUpDownPoints).EndInit();
            m_oMenuStrip.ResumeLayout(false);
            m_oMenuStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox m_tbxName;
        private Label m_lblName;
        private GroupBox m_ctlGroupBoxGender;
        private RadioButton m_rbMale;
        private RadioButton m_rbDiverse;
        private RadioButton m_rbFemale;
        private TabControl m_ctlReportParts;
        private TabPage m_ctlTabPageCommunity;
        private TabPage m_ctlTTabPageLearning;
        private TabPage m_ctlTabPageWorkHabits;
        private Label m_lblTextCommunity;
        private TextBox m_tbxTextCommunity;
        private TextBox m_tbxCurrentTextCommunity;
        private TextBox m_tbxNextWorseTextCommunity;
        private Button m_btnCreateNextWorseCommunity;
        private Button m_btnCreateNextBetterCommunity;
        private Button m_btnNextWorseCommunity;
        private Button m_btnNextBestCommunity;
        private TextBox m_tbxNextBestTextCommunity;
        private OpenFileDialog m_dlgSelectFile;
        private Label m_lblCurrentPoint;
        private Button m_btnAddCurrentText;
        private NumericUpDown m_ctlUpDownPoints;
        private CheckBox m_chkAddName;
        private MenuStrip m_oMenuStrip;
        private ToolStripMenuItem m_oChooseTextsToolStripMenuItem;
        private ToolStripMenuItem m_oAboutToolStripMenuItem;
        private ToolStripMenuItem m_oLicenseToolStripMenuItem;
        private Button m_btnCopyFullText;
    }
}

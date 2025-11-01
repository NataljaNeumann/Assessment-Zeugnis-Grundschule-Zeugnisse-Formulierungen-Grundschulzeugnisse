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
            m_ctlGroupBoxGender.SuspendLayout();
            m_ctlReportParts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)m_ctlUpDownPoints).BeginInit();
            m_oMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // m_tbxName
            // 
            m_tbxName.Enabled = false;
            m_tbxName.Font = new Font("Segoe UI", 11.25F);
            m_tbxName.Location = new Point(12, 82);
            m_tbxName.Name = "m_tbxName";
            m_tbxName.Size = new Size(210, 27);
            m_tbxName.TabIndex = 0;
            m_tbxName.TextChanged += OnName_TextChanged;
            // 
            // m_lblName
            // 
            m_lblName.Font = new Font("Segoe UI", 11.25F);
            m_lblName.Location = new Point(12, 48);
            m_lblName.Name = "m_lblName";
            m_lblName.Size = new Size(210, 31);
            m_lblName.TabIndex = 1;
            m_lblName.Text = "Name";
            m_lblName.TextAlign = ContentAlignment.BottomLeft;
            // 
            // m_ctlGroupBoxGender
            // 
            m_ctlGroupBoxGender.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            m_ctlGroupBoxGender.Controls.Add(m_rbDiverse);
            m_ctlGroupBoxGender.Controls.Add(m_rbFemale);
            m_ctlGroupBoxGender.Controls.Add(m_rbMale);
            m_ctlGroupBoxGender.Enabled = false;
            m_ctlGroupBoxGender.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            m_ctlGroupBoxGender.Location = new Point(248, 48);
            m_ctlGroupBoxGender.Name = "m_ctlGroupBoxGender";
            m_ctlGroupBoxGender.Size = new Size(766, 84);
            m_ctlGroupBoxGender.TabIndex = 2;
            m_ctlGroupBoxGender.TabStop = false;
            m_ctlGroupBoxGender.Text = "Gender";
            // 
            // m_rbDiverse
            // 
            m_rbDiverse.AutoSize = true;
            m_rbDiverse.Location = new Point(400, 35);
            m_rbDiverse.Name = "m_rbDiverse";
            m_rbDiverse.Size = new Size(76, 24);
            m_rbDiverse.TabIndex = 2;
            m_rbDiverse.TabStop = true;
            m_rbDiverse.Text = "Diverse";
            m_rbDiverse.UseVisualStyleBackColor = true;
            m_rbDiverse.CheckedChanged += OnGenderRadioBox_CheckedChanged;
            // 
            // m_rbFemale
            // 
            m_rbFemale.AutoSize = true;
            m_rbFemale.Location = new Point(198, 35);
            m_rbFemale.Name = "m_rbFemale";
            m_rbFemale.Size = new Size(75, 24);
            m_rbFemale.TabIndex = 1;
            m_rbFemale.TabStop = true;
            m_rbFemale.Text = "Female";
            m_rbFemale.UseVisualStyleBackColor = true;
            m_rbFemale.CheckedChanged += OnGenderRadioBox_CheckedChanged;
            // 
            // m_rbMale
            // 
            m_rbMale.AutoSize = true;
            m_rbMale.Checked = true;
            m_rbMale.Location = new Point(20, 35);
            m_rbMale.Name = "m_rbMale";
            m_rbMale.Size = new Size(60, 24);
            m_rbMale.TabIndex = 0;
            m_rbMale.TabStop = true;
            m_rbMale.Text = "Male";
            m_rbMale.UseVisualStyleBackColor = true;
            m_rbMale.CheckedChanged += OnGenderRadioBox_CheckedChanged;
            // 
            // m_ctlReportParts
            // 
            m_ctlReportParts.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            m_ctlReportParts.Controls.Add(m_ctlTabPageCommunity);
            m_ctlReportParts.Controls.Add(m_ctlTabPageWorkHabits);
            m_ctlReportParts.Controls.Add(m_ctlTTabPageLearning);
            m_ctlReportParts.Enabled = false;
            m_ctlReportParts.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            m_ctlReportParts.Location = new Point(-2, 138);
            m_ctlReportParts.Name = "m_ctlReportParts";
            m_ctlReportParts.SelectedIndex = 0;
            m_ctlReportParts.Size = new Size(1028, 40);
            m_ctlReportParts.TabIndex = 3;
            m_ctlReportParts.SelectedIndexChanged += m_ctlReportParts_SelectedIndexChanged;
            // 
            // m_ctlTabPageCommunity
            // 
            m_ctlTabPageCommunity.BackColor = SystemColors.Control;
            m_ctlTabPageCommunity.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            m_ctlTabPageCommunity.Location = new Point(4, 30);
            m_ctlTabPageCommunity.Name = "m_ctlTabPageCommunity";
            m_ctlTabPageCommunity.Padding = new Padding(3);
            m_ctlTabPageCommunity.Size = new Size(1020, 6);
            m_ctlTabPageCommunity.TabIndex = 0;
            m_ctlTabPageCommunity.Text = "Community";
            // 
            // m_ctlTabPageWorkHabits
            // 
            m_ctlTabPageWorkHabits.Location = new Point(4, 30);
            m_ctlTabPageWorkHabits.Name = "m_ctlTabPageWorkHabits";
            m_ctlTabPageWorkHabits.Size = new Size(1020, 6);
            m_ctlTabPageWorkHabits.TabIndex = 2;
            m_ctlTabPageWorkHabits.Text = "Work Habits";
            m_ctlTabPageWorkHabits.UseVisualStyleBackColor = true;
            // 
            // m_ctlTTabPageLearning
            // 
            m_ctlTTabPageLearning.Location = new Point(4, 30);
            m_ctlTTabPageLearning.Name = "m_ctlTTabPageLearning";
            m_ctlTTabPageLearning.Padding = new Padding(3);
            m_ctlTTabPageLearning.Size = new Size(1020, 6);
            m_ctlTTabPageLearning.TabIndex = 1;
            m_ctlTTabPageLearning.Text = "Learning";
            m_ctlTTabPageLearning.UseVisualStyleBackColor = true;
            // 
            // m_btnCreateNextWorseCommunity
            // 
            m_btnCreateNextWorseCommunity.Location = new Point(657, 500);
            m_btnCreateNextWorseCommunity.Name = "m_btnCreateNextWorseCommunity";
            m_btnCreateNextWorseCommunity.Size = new Size(45, 33);
            m_btnCreateNextWorseCommunity.TabIndex = 8;
            m_btnCreateNextWorseCommunity.Text = "+";
            m_btnCreateNextWorseCommunity.UseVisualStyleBackColor = true;
            m_btnCreateNextWorseCommunity.Click += OnCreateNextWorseCommunity_Click;
            // 
            // m_btnCreateNextBetterCommunity
            // 
            m_btnCreateNextBetterCommunity.Location = new Point(312, 500);
            m_btnCreateNextBetterCommunity.Name = "m_btnCreateNextBetterCommunity";
            m_btnCreateNextBetterCommunity.Size = new Size(45, 33);
            m_btnCreateNextBetterCommunity.TabIndex = 7;
            m_btnCreateNextBetterCommunity.Text = "+";
            m_btnCreateNextBetterCommunity.UseVisualStyleBackColor = true;
            m_btnCreateNextBetterCommunity.Click += OnCreateNextBetterCommunity_Click;
            // 
            // m_btnNextWorseCommunity
            // 
            m_btnNextWorseCommunity.Location = new Point(657, 367);
            m_btnNextWorseCommunity.Name = "m_btnNextWorseCommunity";
            m_btnNextWorseCommunity.Size = new Size(45, 33);
            m_btnNextWorseCommunity.TabIndex = 6;
            m_btnNextWorseCommunity.Text = ">";
            m_btnNextWorseCommunity.UseVisualStyleBackColor = true;
            m_btnNextWorseCommunity.Click += OnNextWorseCommunity_Click;
            // 
            // m_btnNextBestCommunity
            // 
            m_btnNextBestCommunity.Location = new Point(312, 367);
            m_btnNextBestCommunity.Name = "m_btnNextBestCommunity";
            m_btnNextBestCommunity.Size = new Size(45, 33);
            m_btnNextBestCommunity.TabIndex = 5;
            m_btnNextBestCommunity.Text = "<";
            m_btnNextBestCommunity.UseVisualStyleBackColor = true;
            m_btnNextBestCommunity.Click += OnNextBestCommunity_Click;
            // 
            // m_tbxNextBestTextCommunity
            // 
            m_tbxNextBestTextCommunity.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            m_tbxNextBestTextCommunity.Location = new Point(16, 361);
            m_tbxNextBestTextCommunity.Multiline = true;
            m_tbxNextBestTextCommunity.Name = "m_tbxNextBestTextCommunity";
            m_tbxNextBestTextCommunity.ReadOnly = true;
            m_tbxNextBestTextCommunity.Size = new Size(290, 192);
            m_tbxNextBestTextCommunity.TabIndex = 4;
            // 
            // m_tbxNextWorseTextCommunity
            // 
            m_tbxNextWorseTextCommunity.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            m_tbxNextWorseTextCommunity.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            m_tbxNextWorseTextCommunity.Location = new Point(708, 361);
            m_tbxNextWorseTextCommunity.Multiline = true;
            m_tbxNextWorseTextCommunity.Name = "m_tbxNextWorseTextCommunity";
            m_tbxNextWorseTextCommunity.ReadOnly = true;
            m_tbxNextWorseTextCommunity.Size = new Size(290, 192);
            m_tbxNextWorseTextCommunity.TabIndex = 3;
            // 
            // m_tbxCurrentTextCommunity
            // 
            m_tbxCurrentTextCommunity.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            m_tbxCurrentTextCommunity.BackColor = SystemColors.ControlLightLight;
            m_tbxCurrentTextCommunity.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            m_tbxCurrentTextCommunity.Location = new Point(363, 361);
            m_tbxCurrentTextCommunity.Multiline = true;
            m_tbxCurrentTextCommunity.Name = "m_tbxCurrentTextCommunity";
            m_tbxCurrentTextCommunity.ReadOnly = true;
            m_tbxCurrentTextCommunity.Size = new Size(288, 192);
            m_tbxCurrentTextCommunity.TabIndex = 2;
            m_tbxCurrentTextCommunity.Click += OnCurrentAssessmentText_Click;
            // 
            // m_lblTextCommunity
            // 
            m_lblTextCommunity.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            m_lblTextCommunity.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            m_lblTextCommunity.Location = new Point(16, 181);
            m_lblTextCommunity.Name = "m_lblTextCommunity";
            m_lblTextCommunity.Size = new Size(982, 36);
            m_lblTextCommunity.TabIndex = 1;
            m_lblTextCommunity.Text = "Text";
            m_lblTextCommunity.TextAlign = ContentAlignment.BottomLeft;
            // 
            // m_tbxTextCommunity
            // 
            m_tbxTextCommunity.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            m_tbxTextCommunity.BackColor = SystemColors.ControlLightLight;
            m_tbxTextCommunity.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            m_tbxTextCommunity.Location = new Point(16, 220);
            m_tbxTextCommunity.Multiline = true;
            m_tbxTextCommunity.Name = "m_tbxTextCommunity";
            m_tbxTextCommunity.ReadOnly = true;
            m_tbxTextCommunity.Size = new Size(982, 96);
            m_tbxTextCommunity.TabIndex = 0;
            // 
            // m_dlgSelectFile
            // 
            m_dlgSelectFile.DefaultExt = "School-Report.xml";
            m_dlgSelectFile.FileName = "Texts.School-Report.xml";
            m_dlgSelectFile.Filter = "School Report Texts Files|*.School-Report.xml";
            m_dlgSelectFile.SelectReadOnly = false;
            m_dlgSelectFile.SupportMultiDottedExtensions = true;
            // 
            // m_lblCurrentPoint
            // 
            m_lblCurrentPoint.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            m_lblCurrentPoint.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            m_lblCurrentPoint.Location = new Point(114, 322);
            m_lblCurrentPoint.Name = "m_lblCurrentPoint";
            m_lblCurrentPoint.Size = new Size(792, 36);
            m_lblCurrentPoint.TabIndex = 10;
            m_lblCurrentPoint.Text = "Caption";
            m_lblCurrentPoint.TextAlign = ContentAlignment.BottomCenter;
            // 
            // m_btnAddCurrentText
            // 
            m_btnAddCurrentText.Location = new Point(16, 322);
            m_btnAddCurrentText.Name = "m_btnAddCurrentText";
            m_btnAddCurrentText.Size = new Size(45, 33);
            m_btnAddCurrentText.TabIndex = 11;
            m_btnAddCurrentText.Text = "+";
            m_btnAddCurrentText.UseVisualStyleBackColor = true;
            m_btnAddCurrentText.Click += OnAddCurrentText_Click;
            // 
            // m_ctlUpDownPoints
            // 
            m_ctlUpDownPoints.Anchor = AnchorStyles.Top;
            m_ctlUpDownPoints.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            m_ctlUpDownPoints.Location = new Point(67, 325);
            m_ctlUpDownPoints.Name = "m_ctlUpDownPoints";
            m_ctlUpDownPoints.Size = new Size(41, 29);
            m_ctlUpDownPoints.TabIndex = 12;
            m_ctlUpDownPoints.UpDownAlign = LeftRightAlignment.Left;
            m_ctlUpDownPoints.ValueChanged += OnUpDownPoints_ValueChanged;
            // 
            // m_chkAddName
            // 
            m_chkAddName.AutoSize = true;
            m_chkAddName.Location = new Point(363, 559);
            m_chkAddName.Name = "m_chkAddName";
            m_chkAddName.Size = new Size(87, 21);
            m_chkAddName.TabIndex = 13;
            m_chkAddName.Text = "Add name";
            m_chkAddName.UseVisualStyleBackColor = true;
            m_chkAddName.CheckedChanged += OnAddName_CheckedChanged;
            // 
            // m_oMenuStrip
            // 
            m_oMenuStrip.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            m_oMenuStrip.Items.AddRange(new ToolStripItem[] { m_oChooseTextsToolStripMenuItem, m_oAboutToolStripMenuItem, m_oLicenseToolStripMenuItem });
            m_oMenuStrip.Location = new Point(0, 0);
            m_oMenuStrip.Name = "m_oMenuStrip";
            m_oMenuStrip.Size = new Size(1026, 28);
            m_oMenuStrip.TabIndex = 14;
            m_oMenuStrip.Text = "menuStrip1";
            // 
            // m_oChooseTextsToolStripMenuItem
            // 
            m_oChooseTextsToolStripMenuItem.Name = "m_oChooseTextsToolStripMenuItem";
            m_oChooseTextsToolStripMenuItem.Size = new Size(107, 24);
            m_oChooseTextsToolStripMenuItem.Text = "Choose Texts";
            m_oChooseTextsToolStripMenuItem.Click += OnSelectFile_Click;
            // 
            // m_oAboutToolStripMenuItem
            // 
            m_oAboutToolStripMenuItem.Alignment = ToolStripItemAlignment.Right;
            m_oAboutToolStripMenuItem.Name = "m_oAboutToolStripMenuItem";
            m_oAboutToolStripMenuItem.Size = new Size(160, 24);
            m_oAboutToolStripMenuItem.Text = "About School Report";
            m_oAboutToolStripMenuItem.Click += OnAboutToolStripMenuItem_Click;
            // 
            // m_oLicenseToolStripMenuItem
            // 
            m_oLicenseToolStripMenuItem.Alignment = ToolStripItemAlignment.Right;
            m_oLicenseToolStripMenuItem.Name = "m_oLicenseToolStripMenuItem";
            m_oLicenseToolStripMenuItem.Size = new Size(120, 24);
            m_oLicenseToolStripMenuItem.Text = "License (GPL 2)";
            m_oLicenseToolStripMenuItem.Click += OnLicenseToolStripMenuItem_Click;
            // 
            // SchoolReportForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1026, 603);
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
            Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            MainMenuStrip = m_oMenuStrip;
            MinimumSize = new Size(898, 642);
            Name = "SchoolReportForm";
            Text = "School Report / Assessment Texts";
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
        private Button m_btnLoadFile;
        private OpenFileDialog m_dlgSelectFile;
        private Label m_lblCurrentPoint;
        private Button m_btnAddCurrentText;
        private NumericUpDown m_ctlUpDownPoints;
        private CheckBox m_chkAddName;
        private MenuStrip m_oMenuStrip;
        private ToolStripMenuItem m_oChooseTextsToolStripMenuItem;
        private ToolStripMenuItem m_oAboutToolStripMenuItem;
        private ToolStripMenuItem m_oLicenseToolStripMenuItem;
    }
}

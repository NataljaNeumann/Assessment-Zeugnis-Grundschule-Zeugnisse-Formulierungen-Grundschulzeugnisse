namespace School_Report.Forms
{
    partial class TextChangeForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextChangeForm));
            m_tbxAssessmentText = new TextBox();
            m_lblDescription = new Label();
            m_btnOk = new Button();
            m_btnCancel = new Button();
            SuspendLayout();
            // 
            // m_tbxAssessmentText
            // 
            m_tbxAssessmentText.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            m_tbxAssessmentText.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            m_tbxAssessmentText.Location = new Point(12, 91);
            m_tbxAssessmentText.Multiline = true;
            m_tbxAssessmentText.Name = "m_tbxAssessmentText";
            m_tbxAssessmentText.Size = new Size(743, 207);
            m_tbxAssessmentText.TabIndex = 0;
            // 
            // m_lblDescription
            // 
            m_lblDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            m_lblDescription.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            m_lblDescription.Location = new Point(12, 9);
            m_lblDescription.Name = "m_lblDescription";
            m_lblDescription.Size = new Size(743, 68);
            m_lblDescription.TabIndex = 1;
            m_lblDescription.Text = resources.GetString("m_lblDescription.Text");
            m_lblDescription.TextAlign = ContentAlignment.BottomLeft;
            // 
            // m_btnOk
            // 
            m_btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            m_btnOk.Location = new Point(479, 312);
            m_btnOk.Name = "m_btnOk";
            m_btnOk.Size = new Size(125, 35);
            m_btnOk.TabIndex = 2;
            m_btnOk.Text = "OK";
            m_btnOk.UseVisualStyleBackColor = true;
            m_btnOk.Click += m_btnOk_Click;
            // 
            // m_btnCancel
            // 
            m_btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            m_btnCancel.Location = new Point(635, 312);
            m_btnCancel.Name = "m_btnCancel";
            m_btnCancel.Size = new Size(120, 35);
            m_btnCancel.TabIndex = 3;
            m_btnCancel.Text = "Cancel";
            m_btnCancel.UseVisualStyleBackColor = true;
            m_btnCancel.Click += m_btnCancel_Click;
            // 
            // TextChangeForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(767, 359);
            Controls.Add(m_btnCancel);
            Controls.Add(m_btnOk);
            Controls.Add(m_lblDescription);
            Controls.Add(m_tbxAssessmentText);
            Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Name = "TextChangeForm";
            Text = "New Assessment or Assessment Correction";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox m_tbxAssessmentText;
        private Label m_lblDescription;
        private Button m_btnOk;
        private Button m_btnCancel;
    }
}
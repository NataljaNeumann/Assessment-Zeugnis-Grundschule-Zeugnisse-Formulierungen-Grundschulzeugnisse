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
            resources.ApplyResources(m_tbxAssessmentText, "m_tbxAssessmentText");
            m_tbxAssessmentText.Name = "m_tbxAssessmentText";
            // 
            // m_lblDescription
            // 
            resources.ApplyResources(m_lblDescription, "m_lblDescription");
            m_lblDescription.Name = "m_lblDescription";
            // 
            // m_btnOk
            // 
            resources.ApplyResources(m_btnOk, "m_btnOk");
            m_btnOk.Name = "m_btnOk";
            m_btnOk.UseVisualStyleBackColor = true;
            m_btnOk.Click += m_btnOk_Click;
            // 
            // m_btnCancel
            // 
            resources.ApplyResources(m_btnCancel, "m_btnCancel");
            m_btnCancel.Name = "m_btnCancel";
            m_btnCancel.UseVisualStyleBackColor = true;
            m_btnCancel.Click += m_btnCancel_Click;
            // 
            // TextChangeForm
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(m_btnCancel);
            Controls.Add(m_btnOk);
            Controls.Add(m_lblDescription);
            Controls.Add(m_tbxAssessmentText);
            Name = "TextChangeForm";
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
// School report aims to help you in creation of personalized
// assessmen texts, so the school reports of primary school pupils have
// nicely formulated assessments in their reports.
//
// Copyright (C) 2025-2026 NataljaNeumann@gmx.de
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace School_Report.Forms
{
    //*******************************************************************************************************
    /// <summary>
    /// A form for entering new and corrected assessment texts
    /// </summary>
    //*******************************************************************************************************
    public partial class TextChangeForm : Form
    {
        //===================================================================================================
        /// <summary>
        /// Constructs a new TextChangeForm
        /// </summary>
        //===================================================================================================
        public TextChangeForm()
        {
            InitializeComponent();
        }


        //===================================================================================================
        /// <summary>
        /// Constructs a new TextChangeForm
        /// </summary>
        /// <param name="strAssessmentText">Initial assessment text</param>
        //===================================================================================================
        public TextChangeForm(
            string strAssessmentText
            )
        {
            InitializeComponent();
            m_tbxAssessmentText.Text = strAssessmentText;
        }

        //===================================================================================================
        /// <summary>
        /// Current text
        /// </summary>
        public string AssessmentText
        {
            get
            {
                return m_tbxAssessmentText.Text;
            }
            set
            {
                m_tbxAssessmentText.Text = value;
            }
        }

        private void m_btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void m_btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}

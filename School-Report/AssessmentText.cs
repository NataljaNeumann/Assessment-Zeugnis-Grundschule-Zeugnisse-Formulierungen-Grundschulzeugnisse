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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace School_Report
{
    //*******************************************************************************************************
    /// <summary>
    /// A single text
    /// </summary>
    //*******************************************************************************************************
    public class AssessmentText
    {
        //===================================================================================================
        /// <summary>
        /// Constructs a new Text object with data
        /// </summary>
        /// <param name="nId">Id of the text</param>
        /// <param name="strText">Assessment text as string</param>
        //===================================================================================================
        public AssessmentText(
            int nId,
            string strText
            ) 
        {
            Id = nId;
            Text = strText;
        }

        //===================================================================================================
        /// <summary>
        /// Id of text
        /// </summary>
        public int Id { get; set; }

        //===================================================================================================
        /// <summary>
        /// The text value
        /// </summary>
        public string Text { get; set; }
    }
}

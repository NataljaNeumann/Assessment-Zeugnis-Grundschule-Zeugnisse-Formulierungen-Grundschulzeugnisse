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

using School_Report.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace School_Report
{
    //*******************************************************************************************************
    /// <summary>
    /// The main form of the application
    /// </summary>
    //*******************************************************************************************************
    public partial class SchoolReportForm : Form
    {
        //===================================================================================================
        /// <summary>
        /// Contains the current file path
        /// </summary>
        string? m_strFilePath = null;

        //===================================================================================================
        /// <summary>
        /// The loaded texts
        /// </summary>
        Dictionary<string, Dictionary<string, List<AssessmentText>>> m_oTexts = [];

        //===================================================================================================
        /// <summary>
        /// The absolute frequencies of ids
        /// </summary>
        Dictionary<int, int> m_oFrequencies = [];

        //===================================================================================================
        /// <summary>
        /// The absolute number of occurences of two ids
        /// </summary>
        Dictionary<(int, int), int> m_oCoOccurences = [];

        //===================================================================================================
        /// <summary>
        /// The maximum Id, for giving new Ids
        /// </summary>
        int m_nMaxId = -1;

        //===================================================================================================
        /// <summary>
        /// Curren section
        /// </summary>
        string m_strCurrentSection = "Community";

        //===================================================================================================
        /// <summary>
        /// Current point in the section
        /// </summary>
        int m_nCurrentPoint = 0;

        //===================================================================================================
        /// <summary>
        /// The index of current value in the point
        /// </summary>
        int m_nCurrentValueIndex = 0;

        //===================================================================================================
        /// <summary>
        /// Constructs a new school report object
        /// </summary>
        //===================================================================================================
        public SchoolReportForm()
        {
            InitializeComponent();
        }

        //===================================================================================================
        /// <summary>
        /// This is executed when user clicks on "Load" button
        /// </summary>
        /// <param name="oSender">Sender object</param>
        /// <param name="oArgs">Event args</param>
        //===================================================================================================
        private void OnSelectFile_Click(
            object oSender,
            EventArgs oArgs
            )
        {
            if (string.IsNullOrEmpty(m_dlgSelectFile.InitialDirectory))
            {
                m_dlgSelectFile.InitialDirectory =
                    System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            if (m_dlgSelectFile.ShowDialog(this) == DialogResult.OK)
            {
                m_strFilePath = m_dlgSelectFile.FileName;
            }

            if (m_strFilePath != null)
            {
                LoadSchoolReportExtended(m_strFilePath,
                    out m_oTexts, out m_oFrequencies, out m_oCoOccurences, out m_nMaxId);

                m_ctlReportParts.TabPages.Clear();
                m_strCurrentSection = "";

                foreach (var sectionName in m_oTexts.Keys)
                {
                    var tabPage = new TabPage(sectionName);
                    m_ctlReportParts.TabPages.Add(tabPage);
                    m_nCurrentPoint = 0;

                    if (m_strCurrentSection.Length == 0)
                    {
                        m_strCurrentSection = sectionName;
                    }
                }

                if (m_strCurrentSection.Length > 0)
                {
                    InitCurrentPoint();
                }
            }
        }

        //===================================================================================================
        /// <summary>
        /// Loads school report texts and stats
        /// </summary>
        /// <param name="oReportData">[OUT] The main data of the report texts</param>
        /// <param name="oTextCooccurrences">[OUT] The stats for the IDs</param>
        /// <param name="oTextFrequencies">[OUT] The stats for co-occurence of IDs</param>
        /// <param name="nMaxId">[OUT] The maximum Id that is present in the file</param>
        /// <param name="strXmlPath">Path of the xml file to load</param>
        //===================================================================================================
        public static void LoadSchoolReportExtended(
            string strXmlPath,
            out Dictionary<string, Dictionary<string, List<AssessmentText>>> oReportData,
            out Dictionary<int, int> oTextFrequencies,
            out Dictionary<(int, int), int> oTextCooccurrences,
            out int nMaxId)
        {
            oReportData = new();
            oTextFrequencies = new();
            oTextCooccurrences = new();

            var oDoc = XDocument.Load(strXmlPath);
            var oRoot = oDoc.Element("school-report-texts") ??
                throw new Exception("Missing root element <school-report-texts>");

            nMaxId = -1;
            // Find maximum available id
            foreach (var oSectionElement in oRoot.XPathSelectElements("//*[@nId]"))
            {
                string? strNId = oSectionElement.Attribute("nId")?.Value;
                if (strNId != null)
                {
                    int nId = int.Parse(strNId);
                    if (nId > nMaxId)
                    {
                        nMaxId = nId;
                    }
                }
            }
            foreach (var oSectionElement in oRoot.XPathSelectElements("//*[@nId1]"))
            {
                string? strNId = oSectionElement.Attribute("nId1")?.Value;
                if (strNId != null)
                {
                    int nId = int.Parse(strNId);
                    if (nId > nMaxId)
                    {
                        nMaxId = nId;
                    }
                }
            }
            foreach (var oSectionElement in oRoot.XPathSelectElements("//*[@nId2]"))
            {
                string? strNId = oSectionElement.Attribute("nId2")?.Value;
                if (strNId != null)
                {
                    int nId = int.Parse(strNId);
                    if (nId > nMaxId)
                    {
                        nMaxId = nId;
                    }
                }
            }


            // Load sections
            foreach (var oSectionElement in oRoot.Elements("section"))
            {
                string strSectionName = oSectionElement.Attribute("strName")?.Value?.Trim()
                    ?? throw new Exception("Missing or empty 'strName' in <section>");

                Dictionary<string, List<AssessmentText>> oPoints =
                    new Dictionary<string, List<AssessmentText>>();

                foreach (XElement oPoint in oSectionElement.Elements("point"))
                {
                    string strCaption = oPoint.Attribute("strCaption")?.Value?.Trim()
                        ?? throw new Exception(
                            $"Missing or empty 'strCaption' in <point> under section '{strSectionName}'");

                    var oTexts = new List<AssessmentText>();

                    foreach (XElement oTextElement in oPoint.Elements("assessment"))
                    {
                        string? strNId = oTextElement.Attribute("nId")?.Value;
                        string? strValue = oTextElement.Attribute("strText")?.Value;

                        if (string.IsNullOrWhiteSpace(strValue))
                            throw new Exception($"Missing 'strText' attribute in <assessment> element under <point strCaption='{strCaption}'>");

                        strValue = strValue.Replace("Du", "er/sie", StringComparison.InvariantCultureIgnoreCase)
                            .Replace("Deinen", "seinen/ihren", StringComparison.InvariantCultureIgnoreCase)
                            .Replace("Deinem", "seinem/ihrem", StringComparison.InvariantCultureIgnoreCase)
                            .Replace("Deine", "seine/ihre", StringComparison.InvariantCultureIgnoreCase)
                            .Replace("Dir", "ihm/ihr", StringComparison.InvariantCultureIgnoreCase)
                            .Replace("Dich", "sich", StringComparison.InvariantCultureIgnoreCase)
                            .Replace("hast", "hat").Replace("bist", "ist").Replace("st ", " ");

                        strValue = char.ToUpper(strValue[0]) + strValue.Substring(1);

                        oTexts.Add(new AssessmentText(strNId != null ? int.Parse(strNId) : (++nMaxId), strValue));
                    }

                    oPoints[strCaption] = oTexts;
                }

                oReportData[strSectionName] = oPoints;
            }

            // Load frequencies
            XElement? oFrequenciesElement = oRoot.Element("text-frequencies");
            if (oFrequenciesElement != null)
            {
                foreach (var text in oFrequenciesElement.Elements("text"))
                {
                    int id = int.Parse(text.Attribute("nId")?.Value ??
                        throw new Exception("Missing 'nId' in <text>"));
                    int count = int.Parse(text.Attribute("count")?.Value ??
                        throw new Exception("Missing 'count' in <text>"));
                    oTextFrequencies[id] = count;
                }
            }

            // Load co-occurrences
            XElement? oCoOccurSection = oRoot.Element("text-co-occurrences");
            if (oCoOccurSection != null)
            {
                foreach (XElement oPair in oCoOccurSection.Elements("pair"))
                {
                    int id1 = int.Parse(oPair.Attribute("nId1")?.Value ??
                        throw new Exception("Missing 'nId1' in <pair>"));
                    int id2 = int.Parse(oPair.Attribute("nId2")?.Value ??
                        throw new Exception("Missing 'nId2' in <pair>"));
                    int count = int.Parse(oPair.Attribute("count")?.Value ??
                        throw new Exception("Missing 'count' in <pair>"));
                    oTextCooccurrences[(id1, id2)] = count;
                }
            }
        }

        //===================================================================================================
        /// <summary>
        /// Saves school report texts and frequencies to a
        /// </summary>
        /// <param name="oReportData"></param>
        /// <param name="oTextFrequencies"></param>
        /// <param name="oTextCoOccurrences">The co-occurences</param>
        /// <param name="strXmlPath">The path of the destination file</param>
        //===================================================================================================
        public static void SaveSchoolReportExtended(
            Dictionary<string, Dictionary<string, List<AssessmentText>>> oReportData,
            Dictionary<int, int> oTextFrequencies,
            Dictionary<(int, int), int> oTextCoOccurrences,
            string strXmlPath)
        {
            XElement oRoot = new XElement("school-report-texts");

            // Sections
            foreach (KeyValuePair<string, Dictionary<string, List<AssessmentText>>>
                oSection in oReportData)
            {
                XElement oSectionElem = new XElement("section", new XAttribute("strName", oSection.Key));

                foreach (KeyValuePair<string, List<AssessmentText>> oPoint in oSection.Value)
                {
                    XElement oPointElem = new XElement("point", new XAttribute("strCaption", oPoint.Key));

                    foreach (AssessmentText oText in oPoint.Value)
                    {
                        oPointElem.Add(new XElement("assessment",
                            new XAttribute("nId", oText.Id),
                            new XAttribute("strText", oText.Text)));
                    }

                    oSectionElem.Add(oPointElem);
                }

                oRoot.Add(oSectionElem);
            }

            // Frequencies
            XElement oFreqElem = new XElement("text-frequencies");
            foreach (var oKvp in oTextFrequencies)
            {
                oFreqElem.Add(new XElement("text",
                    new XAttribute("nId", oKvp.Key),
                    new XAttribute("count", oKvp.Value)));
            }
            oRoot.Add(oFreqElem);

            // Co-occurrences
            XElement oCoOccurElem = new XElement("text-co-occurrences");
            foreach (var oKvp in oTextCoOccurrences)
            {
                oCoOccurElem.Add(new XElement("pair",
                    new XAttribute("nId1", oKvp.Key.Item1),
                    new XAttribute("nId2", oKvp.Key.Item2),
                    new XAttribute("count", oKvp.Value)));
            }
            oRoot.Add(oCoOccurElem);

            XDocument oDoc = new XDocument(oRoot);
            string strTmpPath = strXmlPath + ".tmp";

            try
            {
                using (var oStream = new FileStream(strTmpPath, FileMode.Create, FileAccess.Write))
                using (var oWriter = new StreamWriter(oStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)))
                {
                    oDoc.Save(oWriter, SaveOptions.None);
                }

                File.Copy(strTmpPath, strXmlPath, overwrite: true);
                File.Delete(strTmpPath);
            }
            catch (Exception oEx)
            {
                throw new IOException($"Failed to save XML safely: {oEx.Message}", oEx);
            }
        }

        private void m_ctlReportParts_SelectedIndexChanged(object oSender, EventArgs oArgs)
        {
            if (m_ctlReportParts.SelectedTab != null)
            {
                m_strCurrentSection = m_ctlReportParts.SelectedTab.Text;
                m_ctlUpDownPoints.Minimum = 0;
                m_ctlUpDownPoints.Maximum = m_oTexts[m_strCurrentSection].Keys.Count;
                InitCurrentPoint();
            }
        }

        private void InitCurrentPoint()
        {
            m_nCurrentValueIndex = 0;
            ShowCurrentPoint();
        }

        private void ShowCurrentPoint()
        {
            string[] astrPoints = m_oTexts[m_strCurrentSection].Keys.ToArray();
            string strCurrentPoint = astrPoints[m_nCurrentPoint];
            m_lblCurrentPoint.Text = strCurrentPoint;
            m_tbxName.Enabled = true;
            m_ctlGroupBoxGender.Enabled = true;

            if (m_nCurrentValueIndex > 0)
            {
                m_tbxNextBestTextCommunity.Text = m_oTexts
                    [m_strCurrentSection]
                    [strCurrentPoint]
                    [m_nCurrentValueIndex - 1]
                    .Text;
                m_btnNextBestCommunity.Enabled = true;
                AdaptTextToGenderAndName(m_tbxNextBestTextCommunity);
            }
            else
            {
                m_tbxNextBestTextCommunity.Text = "";
                m_btnNextBestCommunity.Enabled = false;
            }


            if (m_nCurrentValueIndex < m_oTexts
                    [m_strCurrentSection]
                    [strCurrentPoint].Count
                    )
            {
                m_tbxCurrentTextCommunity.Text = m_oTexts
                    [m_strCurrentSection]
                    [strCurrentPoint]
                    [m_nCurrentValueIndex]
                    .Text;
                AdaptTextToGenderAndName(m_tbxCurrentTextCommunity);
            }

            if (m_nCurrentValueIndex + 1 < m_oTexts
                    [m_strCurrentSection]
                    [strCurrentPoint].Count
                    )
            {
                m_tbxNextWorseTextCommunity.Text = m_oTexts
                    [m_strCurrentSection]
                    [strCurrentPoint]
                    [m_nCurrentValueIndex + 1]
                    .Text;
                AdaptTextToGenderAndName(m_tbxNextWorseTextCommunity);
            }
            else
            {
                m_tbxNextWorseTextCommunity.Text = "";
                m_btnNextWorseCommunity.Enabled = false;
            }
        }

        //===================================================================================================
        /// <summary>
        /// Adapts texts in textboxes to given genders and names
        /// </summary>
        /// <param name="tbxToAdapt">Textbox for adaptation of text</param>
        //===================================================================================================
        void AdaptTextToGenderAndName(TextBox tbxToAdapt)
        {
            if (System.Threading.Thread.CurrentThread.CurrentUICulture.IetfLanguageTag.StartsWith("en"))
            {
                AdaptTextToGenderAndName_English(tbxToAdapt);
            }
            else
            if (System.Threading.Thread.CurrentThread.CurrentUICulture.IetfLanguageTag.StartsWith("es"))
            {
                AdaptTextToGenderAndName_Spanish(tbxToAdapt);
            }
            else
            if (System.Threading.Thread.CurrentThread.CurrentUICulture.IetfLanguageTag.StartsWith("pt"))
            {
                AdaptTextToGenderAndName_Portuguese(tbxToAdapt);
            }
            else
            if (System.Threading.Thread.CurrentThread.CurrentUICulture.IetfLanguageTag.StartsWith("it"))
            {
                AdaptTextToGenderAndName_Italian(tbxToAdapt);
            }
            else
            if (System.Threading.Thread.CurrentThread.CurrentUICulture.IetfLanguageTag.StartsWith("fr"))
            {
                AdaptTextToGenderAndName_French(tbxToAdapt);
            }
            else
            if (System.Threading.Thread.CurrentThread.CurrentUICulture.IetfLanguageTag.StartsWith("ru"))
            {
                AdaptTextToGenderAndName_Russian(tbxToAdapt);
            }
            else
            {
                // Default is German, since the primary case I'm working on is German primary school
                AdaptTextToGenderAndName_German(tbxToAdapt);
            }
        }

        //===================================================================================================
        /// <summary>
        /// Adapts texts in textboxes to given genders and names in German
        /// </summary>
        /// <param name="tbxToAdapt">Textbox for adaptation of text</param>
        //===================================================================================================
        void AdaptTextToGenderAndName_German(TextBox tbxToAdapt)
        {
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";
            if (m_chkAddName.Checked && !strCurrentText.Contains("Name"))
            {
                string? strToReplace = null;
                int nPosToReplace = -1;

                foreach (string strTextToSearch in new string[]
                                { " Er/sie ", " Ihm/ihr ", " Ihn/sie ", " Seine/ihre "})
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" Er/sie ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                    }
                }

                if (strToReplace != null)
                {
                    if (" Seine/ihre ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " Namens " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " Name " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                }
            }

            string strNameToUse = m_tbxName.Text;
            if (string.IsNullOrWhiteSpace(strNameToUse))
                strNameToUse = "NAME FEHLT";

            if (m_rbMale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" er/sie ", " er ")
                    .Replace(" Er/sie ", " Er ")
                    .Replace(" ihm/ihr ", " ihm ")
                    .Replace(" Ihm/ihr ", " Ihm ")
                    .Replace(" Ihn/sie ", " Ihn ")
                    .Replace(" ihn/sie ", " ihn ")
                    .Replace(" Seine/ihre ", " Seine ")
                    .Replace(" seine/ihre ", " seine ")
                    .Replace(" Seinen/ihren ", " Seinen ")
                    .Replace(" seinen/ihren ", " seinen ")
                    .Replace(" Seinem/ihrem ", " Seinem ")
                    .Replace(" seinem/ihrem ", " seinem ")
                    .Replace(" Name ", " " + strNameToUse + " ")
                    .Replace(" Namens ", " " + strNameToUse + "s ");
            }
            else
                if (m_rbFemale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" er/sie ", " sie ")
                    .Replace(" Er/sie ", " Sie ")
                    .Replace(" ihm/ihr ", " ihr ")
                    .Replace(" Ihm/ihr ", " Ihr ")
                    .Replace(" Ihn/sie ", " Sie ")
                    .Replace(" ihn/sie ", " sie ")
                    .Replace(" Seine/ihre ", " Ihre ")
                    .Replace(" seine/ihre ", " ihre ")
                    .Replace(" Seinen/ihren ", " Ihren ")
                    .Replace(" seinen/ihren ", " ihren ")
                    .Replace(" Seinem/ihrem ", " Ihrem ")
                    .Replace(" seinem/ihrem ", " ihrem ")
                    .Replace(" Name ", " " + strNameToUse + " ")
                    .Replace(" Namens ", " " + strNameToUse + "s ");
            }
            else
            {
                strCurrentText = strCurrentText
                    .Replace(" Name ", " " + strNameToUse + " ")
                    .Replace(" Namens ", " " + strNameToUse + "s ");
            }

            tbxToAdapt.Text = strCurrentText.Trim().Replace(" .", ".");
        }

        //===================================================================================================
        /// <summary>
        /// Adapts texts in textboxes to given genders and names in English
        /// </summary>
        /// <param name="tbxToAdapt">Textbox for adaptation of text</param>
        //===================================================================================================
        void AdaptTextToGenderAndName_English(TextBox tbxToAdapt)
        {
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";
            if (m_chkAddName.Checked && !strCurrentText.Contains("Name"))
            {
                string? strToReplace = null;
                int nPosToReplace = -1;

                foreach (string strTextToSearch in new string[]
                                { " He/she ", " Him/her ", " His/her "})
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" He/she ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                    }
                }

                if (strToReplace != null)
                {
                    if (" His/her ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " Name's " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " Name " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                }
            }

            string strNameToUse = m_tbxName.Text;
            if (string.IsNullOrWhiteSpace(strNameToUse))
                strNameToUse = "NAME IS MISING";

            if (m_rbMale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" he/she ", " he ")
                    .Replace(" He/she ", " He ")
                    .Replace(" him/her ", " him ")
                    .Replace(" Him/her ", " Him ")
                    .Replace(" His/her ", " His ")
                    .Replace(" his/her ", " his ")
                    .Replace(" Name ", " " + strNameToUse + " ")
                    .Replace(" Name's ", " " + strNameToUse + "'s ");
            }
            else
                if (m_rbFemale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" he/she ", " she ")
                    .Replace(" He/she ", " She ")
                    .Replace(" him/her ", " her ")
                    .Replace(" Him/her ", " Her ")
                    .Replace(" His/her ", " Her ")
                    .Replace(" his/her ", " her ")
                    .Replace(" Name ", " " + strNameToUse + " ")
                    .Replace(" Name's ", " " + strNameToUse + "'s ");
            }
            else
            {
                strCurrentText = strCurrentText
                    .Replace(" Name ", " " + strNameToUse + " ")
                    .Replace(" Name's ", " " + strNameToUse + "'s ");
            }

            tbxToAdapt.Text = strCurrentText.Trim().Replace(" .", ".");
        }


        //===================================================================================================
        /// <summary>
        /// Adapts texts in textboxes to given genders and names in French
        /// </summary>
        /// <param name="tbxToAdapt">Textbox for adaptation of text</param>
        //===================================================================================================
        void AdaptTextToGenderAndName_French(TextBox tbxToAdapt)
        {
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";
            if (m_chkAddName.Checked && !strCurrentText.Contains("Nom"))
            {
                string? strToReplace = null;
                int nPosToReplace = -1;

                foreach (string strTextToSearch in new string[]
                                { " Il/elle ", " Lui/elle ", " Le/la ", " Son/sa "})
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" Il/elle ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                    }
                }

                if (strToReplace != null)
                {
                    if (" Son/sa ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " du Nom " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " Nom " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                }
            }

            string strNameToUse = m_tbxName.Text;
            if (string.IsNullOrWhiteSpace(strNameToUse))
                strNameToUse = "NOM MANQUANT";

            if (m_rbMale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" il/elle ", " il ")
                    .Replace(" Il/elle ", " Il ")
                    .Replace(" lui/elle ", " lui ")
                    .Replace(" Lui/elle ", " Lui ")
                    .Replace(" le/la ", " le ")
                    .Replace(" Le/la ", " Le ")
                    .Replace(" son/sa ", " son ")
                    .Replace(" Son/sa ", " Son ")
                    .Replace(" Nom ", " " + strNameToUse + " ")
                    .Replace(" du Nom ", " du " + strNameToUse + " ");
            }
            else if (m_rbFemale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" il/elle ", " elle ")
                    .Replace(" Il/elle ", " Elle ")
                    .Replace(" lui/elle ", " elle ")
                    .Replace(" Lui/elle ", " Elle ")
                    .Replace(" le/la ", " la ")
                    .Replace(" Le/la ", " La ")
                    .Replace(" son/sa ", " sa ")
                    .Replace(" Son/sa ", " Sa ")
                    .Replace(" Nom ", " " + strNameToUse + " ")
                    .Replace(" du Nom ", " de " + strNameToUse + " ");
            }
            else
            {
                strCurrentText = strCurrentText
                    .Replace(" Nom ", " " + strNameToUse + " ")
                    .Replace(" du Nom ", " de " + strNameToUse + " ");
            }

            tbxToAdapt.Text = strCurrentText.Trim().Replace(" .", ".");
        }


        //===================================================================================================
        /// <summary>
        /// Adapts texts in textboxes to given genders and names in Spanish
        /// </summary>
        /// <param name="tbxToAdapt">Textbox for adaptation of text</param>
        //===================================================================================================
        void AdaptTextToGenderAndName_Spanish(TextBox tbxToAdapt)
        {
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";
            if (m_chkAddName.Checked && !strCurrentText.Contains("Nombre"))
            {
                string? strToReplace = null;
                int nPosToReplace = -1;

                foreach (string strTextToSearch in new string[]
                                { " Él/ella ", " Le/ella ", " Lo/la ", " Su "})
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" Él/ella ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                    }
                }

                if (strToReplace != null)
                {
                    if (" Su ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " del Nombre " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " Nombre " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                }
            }

            string strNameToUse = m_tbxName.Text;
            if (string.IsNullOrWhiteSpace(strNameToUse))
                strNameToUse = "NOMBRE FALTANTE";

            if (m_rbMale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" él/ella ", " él ")
                    .Replace(" Él/ella ", " Él ")
                    .Replace(" le/ella ", " le ")
                    .Replace(" Le/ella ", " Le ")
                    .Replace(" lo/la ", " lo ")
                    .Replace(" Lo/la ", " Lo ")
                    .Replace(" su ", " su ")
                    .Replace(" Su ", " Su ")
                    .Replace(" Nombre ", " " + strNameToUse + " ")
                    .Replace(" del Nombre ", " de " + strNameToUse + " ");
            }
            else if (m_rbFemale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" él/ella ", " ella ")
                    .Replace(" Él/ella ", " Ella ")
                    .Replace(" le/ella ", " ella ")
                    .Replace(" Le/ella ", " Ella ")
                    .Replace(" lo/la ", " la ")
                    .Replace(" Lo/la ", " La ")
                    .Replace(" su ", " su ")
                    .Replace(" Su ", " Su ")
                    .Replace(" Nombre ", " " + strNameToUse + " ")
                    .Replace(" del Nombre ", " de " + strNameToUse + " ");
            }
            else
            {
                strCurrentText = strCurrentText
                    .Replace(" Nombre ", " " + strNameToUse + " ")
                    .Replace(" del Nombre ", " de " + strNameToUse + " ");
            }

            tbxToAdapt.Text = strCurrentText.Trim().Replace(" .", ".");
        }


        //===================================================================================================
        /// <summary>
        /// Adapts texts in textboxes to given genders and names in Portuguese
        /// </summary>
        /// <param name="tbxToAdapt">Textbox for adaptation of text</param>
        //===================================================================================================
        void AdaptTextToGenderAndName_Portuguese(TextBox tbxToAdapt)
        {
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";
            if (m_chkAddName.Checked && !strCurrentText.Contains("Nome"))
            {
                string? strToReplace = null;
                int nPosToReplace = -1;

                foreach (string strTextToSearch in new string[]
                                { " Ele/ela ", " Lhe/ela ", " O/a ", " Seu/sua "})
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" Ele/ela ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                    }
                }

                if (strToReplace != null)
                {
                    if (" Seu/sua ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " do Nome " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " Nome " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                }
            }

            string strNameToUse = m_tbxName.Text;
            if (string.IsNullOrWhiteSpace(strNameToUse))
                strNameToUse = "NOME EM FALTA";

            if (m_rbMale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" ele/ela ", " ele ")
                    .Replace(" Ele/ela ", " Ele ")
                    .Replace(" lhe/ela ", " lhe ")
                    .Replace(" Lhe/ela ", " Lhe ")
                    .Replace(" o/a ", " o ")
                    .Replace(" O/a ", " O ")
                    .Replace(" seu/sua ", " seu ")
                    .Replace(" Seu/sua ", " Seu ")
                    .Replace(" Nome ", " " + strNameToUse + " ")
                    .Replace(" do Nome ", " do " + strNameToUse + " ");
            }
            else if (m_rbFemale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" ele/ela ", " ela ")
                    .Replace(" Ele/ela ", " Ela ")
                    .Replace(" lhe/ela ", " ela ")
                    .Replace(" Lhe/ela ", " Ela ")
                    .Replace(" o/a ", " a ")
                    .Replace(" O/a ", " A ")
                    .Replace(" seu/sua ", " sua ")
                    .Replace(" Seu/sua ", " Sua ")
                    .Replace(" Nome ", " " + strNameToUse + " ")
                    .Replace(" do Nome ", " da " + strNameToUse + " ");
            }
            else
            {
                strCurrentText = strCurrentText
                    .Replace(" Nome ", " " + strNameToUse + " ")
                    .Replace(" do Nome ", " de " + strNameToUse + " ");
            }

            tbxToAdapt.Text = strCurrentText.Trim().Replace(" .", ".");
        }


        //===================================================================================================
        /// <summary>
        /// Adapts texts in textboxes to given genders and names in Italian
        /// </summary>
        /// <param name="tbxToAdapt">Textbox for adaptation of text</param>
        //===================================================================================================
        void AdaptTextToGenderAndName_Italian(TextBox tbxToAdapt)
        {
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";
            if (m_chkAddName.Checked && !strCurrentText.Contains("Nome"))
            {
                string? strToReplace = null;
                int nPosToReplace = -1;

                foreach (string strTextToSearch in new string[]
                                { " Lui/lei ", " Gli/lei ", " Lo/la ", " Suo/sua "})
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" Lui/lei ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                    }
                }

                if (strToReplace != null)
                {
                    if (" Suo/sua ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " del Nome " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " Nome " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                }
            }

            string strNameToUse = m_tbxName.Text;
            if (string.IsNullOrWhiteSpace(strNameToUse))
                strNameToUse = "NOME MANCANTE";

            if (m_rbMale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" lui/lei ", " lui ")
                    .Replace(" Lui/lei ", " Lui ")
                    .Replace(" gli/lei ", " gli ")
                    .Replace(" Gli/lei ", " Gli ")
                    .Replace(" lo/la ", " lo ")
                    .Replace(" Lo/la ", " Lo ")
                    .Replace(" suo/sua ", " suo ")
                    .Replace(" Suo/sua ", " Suo ")
                    .Replace(" Nome ", " " + strNameToUse + " ")
                    .Replace(" del Nome ", " di " + strNameToUse + " ");
            }
            else if (m_rbFemale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" lui/lei ", " lei ")
                    .Replace(" Lui/lei ", " Lei ")
                    .Replace(" gli/lei ", " le ")
                    .Replace(" Gli/lei ", " Le ")
                    .Replace(" lo/la ", " la ")
                    .Replace(" Lo/la ", " La ")
                    .Replace(" suo/sua ", " sua ")
                    .Replace(" Suo/sua ", " Sua ")
                    .Replace(" Nome ", " " + strNameToUse + " ")
                    .Replace(" del Nome ", " di " + strNameToUse + " ");
            }
            else
            {
                strCurrentText = strCurrentText
                    .Replace(" Nome ", " " + strNameToUse + " ")
                    .Replace(" del Nome ", " di " + strNameToUse + " ");
            }

            tbxToAdapt.Text = strCurrentText.Trim().Replace(" .", ".");
        }

        //===================================================================================================
        /// <summary>
        /// Some of russian declination cases
        /// </summary>
        //===================================================================================================
        enum RussianCase
        {
            Nominative,
            Genitive,
            Dative,
            Accusative
        }

        //===================================================================================================
        /// <summary>
        /// Declination of russian names
        /// </summary>
        /// <param name="strName">Name for declination</param>
        /// <param name="bIsMale">Indication if it is a male name</param>
        /// <param name="eTargetCase">The case</param>
        /// <returns>Declined name</returns>
        //===================================================================================================
        string DeclineRussianName(string strName, bool bIsMale, RussianCase eTargetCase)
        {
            if (string.IsNullOrWhiteSpace(strName))
                return strName;

            string strLowerName = strName.ToLower();

            if (bIsMale)
            {
                if (strLowerName.EndsWith("й"))
                {
                    switch (eTargetCase)
                    {
                        case RussianCase.Genitive: return strName.Substring(0, strName.Length - 1) + "я";   // Сергей → Сергея
                        case RussianCase.Dative: return strName.Substring(0, strName.Length - 1) + "ю";    // Сергею
                        case RussianCase.Accusative: return strName.Substring(0, strName.Length - 1) + "я"; // Сергея
                    }
                }
                if (strLowerName.EndsWith("ь"))
                {
                    switch (eTargetCase)
                    {
                        case RussianCase.Genitive: return strName.Substring(0, strName.Length - 1) + "я";   // Игорь → Игоря
                        case RussianCase.Dative: return strName.Substring(0, strName.Length - 1) + "ю";    // Игорю
                        case RussianCase.Accusative: return strName.Substring(0, strName.Length - 1) + "я"; // Игоря
                    }
                }
                if (strLowerName.EndsWith("а"))
                {
                    switch (eTargetCase)
                    {
                        case RussianCase.Genitive: return strName.Substring(0, strName.Length - 1) + "ы";   // Никита → Никиты
                        case RussianCase.Dative: return strName.Substring(0, strName.Length - 1) + "е";    // Никите
                        case RussianCase.Accusative: return strName.Substring(0, strName.Length - 1) + "у"; // Никиту
                    }
                }
                // Default masculine (Иван)
                switch (eTargetCase)
                {
                    case RussianCase.Genitive: return strName + "а";   // Ивана
                    case RussianCase.Dative: return strName + "у";    // Ивану
                    case RussianCase.Accusative: return strName + "а"; // Ивана
                }
            }
            else // feminine
            {
                if (strLowerName.EndsWith("а"))
                {
                    switch (eTargetCase)
                    {
                        case RussianCase.Genitive: return strName.Substring(0, strName.Length - 1) + "ы";   // Анна → Анны
                        case RussianCase.Dative: return strName.Substring(0, strName.Length - 1) + "е";    // Анне
                        case RussianCase.Accusative: return strName.Substring(0, strName.Length - 1) + "у"; // Анну
                    }
                }
                if (strLowerName.EndsWith("я"))
                {
                    switch (eTargetCase)
                    {
                        case RussianCase.Genitive: return strName.Substring(0, strName.Length - 1) + "и";   // Мария → Марии
                        case RussianCase.Dative: return strName.Substring(0, strName.Length - 1) + "е";    // Марие
                        case RussianCase.Accusative: return strName.Substring(0, strName.Length - 1) + "ю"; // Марию
                    }
                }
                if (strLowerName.EndsWith("ь"))
                {
                    switch (eTargetCase)
                    {
                        case RussianCase.Genitive: return strName + "и";   // Любовь → Любови
                        case RussianCase.Dative: return strName + "и";    // Любови
                        case RussianCase.Accusative: return strName;      // Любовь (same as nominative)
                    }
                }
            }

            return strName; // fallback
        }



        //===================================================================================================
        /// <summary>
        /// Adapts texts in textboxes to given genders and names in Russian
        /// </summary>
        /// <param name="tbxToAdapt">Textbox for adaptation of text</param>
        //===================================================================================================
        void AdaptTextToGenderAndName_Russian(TextBox tbxToAdapt)
        {
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";
            if (m_chkAddName.Checked && !strCurrentText.Contains("Имя"))
            {
                string? strToReplace = null;
                int nPosToReplace = -1;

                foreach (string strTextToSearch in new string[]
                                { " Он/она ", " Ему/ей ", " Его/её ", " Свой/ю/ё/и " })
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !"  Он/она ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                    }
                }

                if (strToReplace != null)
                {
                    if (" Свой/ю/ё/и ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " Имени " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else
                    if (" Ему/ей ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " ИмениД " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else
                    if (" Его/её ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " ИмениВ " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " Имя " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                }
            }

            string strNameToUse = m_tbxName.Text;
            if (string.IsNullOrWhiteSpace(strNameToUse))
                strNameToUse = "ИМЯ ОТСУТСТВУЕТ";

            if (m_rbMale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" он/она ", " он ")
                    .Replace(" Он/она ", " Он ")
                    .Replace(" ему/ей ", " ему ")
                    .Replace(" Ему/ей ", " Ему ")
                    .Replace(" его/её ", " его ")
                    .Replace(" Его/её ", " Его ")
                    .Replace(" Имя ", " " + strNameToUse + " ")
                    .Replace(" Имени ", " " + DeclineRussianName(strNameToUse, true, RussianCase.Genitive) + " ")
                    .Replace(" ИмениД ", " " + DeclineRussianName(strNameToUse, true, RussianCase.Dative) + " ")
                    .Replace(" ИмениВ ", " " + DeclineRussianName(strNameToUse, true, RussianCase.Accusative) + " ");
            }
            else if (m_rbFemale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" он/она ", " она ")
                    .Replace(" Он/она ", " Она ")
                    .Replace(" ему/ей ", " ей ")
                    .Replace(" Ему/ей ", " Ей ")
                    .Replace(" его/её ", " её ")
                    .Replace(" Его/её ", " Её ")
                    .Replace(" Имя ", " " + strNameToUse + " ")
                    .Replace(" Имени ", " " + DeclineRussianName(strNameToUse, true, RussianCase.Genitive) + " ")
                    .Replace(" ИмениД ", " " + DeclineRussianName(strNameToUse, true, RussianCase.Dative) + " ")
                    .Replace(" ИмениВ ", " " + DeclineRussianName(strNameToUse, true, RussianCase.Accusative) + " ");
            }
            else
            {
                strCurrentText = strCurrentText
                    .Replace(" Имя ", " " + strNameToUse + " ")
                    .Replace(" Имени ", " " + DeclineRussianName(strNameToUse, true, RussianCase.Genitive) + " ")
                    .Replace(" ИмениД ", " " + DeclineRussianName(strNameToUse, true, RussianCase.Dative) + " ")
                    .Replace(" ИмениВ ", " " + DeclineRussianName(strNameToUse, true, RussianCase.Accusative) + " ");
            }

            tbxToAdapt.Text = strCurrentText.Trim().Replace(" .", ".");
        }

        //===================================================================================================
        /// <summary>
        /// This is executed when user selects the next best assessment (to the left)
        /// </summary>
        /// <param name="oSender">Sender object</param>
        /// <param name="oArgs">Event args</param>
        //===================================================================================================
        private void OnNextBestCommunity_Click(object oSender, EventArgs oArgs)
        {
            if (m_nCurrentValueIndex > 0)
            {
                m_nCurrentValueIndex--;
                ShowCurrentPoint();
            }
        }

        //===================================================================================================
        /// <summary>
        /// This is executed when user selects the next worst assessment (to the right)
        /// </summary>
        /// <param name="oSender">Sender object</param>
        /// <param name="oArgs">Event args</param>
        //===================================================================================================
        private void OnNextWorseCommunity_Click(object oSender, EventArgs oArgs)
        {
            string[] astrPoints = m_oTexts[m_strCurrentSection].Keys.ToArray();
            string strCurrentPoint = astrPoints[m_nCurrentPoint];

            if (m_nCurrentValueIndex + 1 < m_oTexts
                    [m_strCurrentSection]
                    [strCurrentPoint].Count)
            {
                m_nCurrentValueIndex++;
                ShowCurrentPoint();
            }

        }

        //===================================================================================================
        /// <summary>
        /// This is executed when user clicks on "add" button for adding current evaluation
        /// </summary>
        /// <param name="oSender">Sender object</param>
        /// <param name="oArgs">Event args</param>
        //===================================================================================================
        private void OnAddCurrentText_Click(object oSender, EventArgs oArgs)
        {
            string strNewText;
            if (!string.IsNullOrEmpty(m_tbxTextCommunity.Text))
                strNewText = m_tbxTextCommunity.Text + " " + m_tbxCurrentTextCommunity.Text;
            else
                strNewText = m_tbxCurrentTextCommunity.Text;


            if (!strNewText.EndsWith("."))
                m_tbxTextCommunity.Text = strNewText + ".";
            else
                m_tbxTextCommunity.Text = strNewText;

            if (m_nCurrentPoint + 1 < m_oTexts[m_strCurrentSection].Count)
            {
                m_ctlUpDownPoints.Value = m_nCurrentPoint + 1;
            }
        }


        //===================================================================================================
        /// <summary>
        /// This is executed when the checkbox "add name" is clicked
        /// </summary>
        /// <param name="oSender">Sender object</param>
        /// <param name="oArgs">Event args</param>
        //===================================================================================================
        private void OnAddName_CheckedChanged(object oSender, EventArgs oArgs)
        {
            ShowCurrentPoint();
        }


        //===================================================================================================
        /// <summary>
        /// This is executed when the gender radio box changes
        /// </summary>
        /// <param name="oSender">Sender object</param>
        /// <param name="oArgs">Event args</param>
        //===================================================================================================
        private void OnGenderRadioBox_CheckedChanged(object oSender, EventArgs oArgs)
        {
            RadioButton? oRb = oSender as RadioButton;
            if (oRb != null)
            {
                if (oRb.Checked)
                {
                    ShowCurrentPoint();
                }
            }
        }

        //===================================================================================================
        /// <summary>
        /// This is executed when name changes
        /// </summary>
        /// <param name="oSender">Sender object</param>
        /// <param name="oArgs">Event args</param>
        //===================================================================================================
        private void OnName_TextChanged(object oSender, EventArgs oArgs)
        {
            if (m_chkAddName.Checked)
            {
                ShowCurrentPoint();
            }
        }

        //===================================================================================================
        /// <summary>
        /// This is executed when up/down control for current point changes
        /// </summary>
        /// <param name="oSender">Sender object</param>
        /// <param name="oArgs">Event args</param>
        //===================================================================================================
        private void OnUpDownPoints_ValueChanged(object oSender, EventArgs oArgs)
        {
            m_nCurrentPoint = Convert.ToInt32(m_ctlUpDownPoints.Value);
            // TODO: choose a probable assessment value
            m_nCurrentValueIndex = 0;
            ShowCurrentPoint();
        }

        //===================================================================================================
        /// <summary>
        /// This is executed when user clicks inside the current assessment text for correction of it
        /// </summary>
        /// <param name="oSender">Sender object</param>
        /// <param name="oArgs">Event args</param>
        //===================================================================================================
        private void OnCurrentAssessmentText_Click(object oSender, EventArgs oArgs)
        {
            if (m_oTexts.Count == 0 || !m_oTexts.ContainsKey(m_strCurrentSection))
                return;

            string[] astrPoints = m_oTexts[m_strCurrentSection].Keys.ToArray();
            string strCurrentPoint = astrPoints[m_nCurrentPoint];

            if (m_nCurrentValueIndex >= 0 &&
                m_nCurrentValueIndex < m_oTexts
                    [m_strCurrentSection]
                    [strCurrentPoint].Count)
            {
                string strText = m_oTexts
                    [m_strCurrentSection]
                    [strCurrentPoint]
                    [m_nCurrentValueIndex]
                    .Text;

                using (TextChangeForm oForm = new TextChangeForm(strText))
                {
                    if (oForm.ShowDialog() == DialogResult.OK)
                    {
                        m_tbxCurrentTextCommunity.Text
                        = m_oTexts
                            [m_strCurrentSection]
                            [strCurrentPoint]
                            [m_nCurrentValueIndex]
                            .Text
                        = oForm.AssessmentText;

                        if (m_strFilePath != null)
                        {
                            SaveSchoolReportExtended(m_oTexts, m_oFrequencies, m_oCoOccurences, m_strFilePath);
                        }

                        AdaptTextToGenderAndName(m_tbxCurrentTextCommunity);
                    }
                }
            }
        }


        //===================================================================================================
        /// <summary>
        /// This is executed when user wants to see the information about the app
        /// </summary>
        /// <param name="oSender">Sender object</param>
        /// <param name="oArgs">Event args</param>
        //===================================================================================================
        private void OnAboutToolStripMenuItem_Click(object oSender, EventArgs oArgs)
        {
            using (var oAbout = new About())
            {
                oAbout.ShowDialog(this);
            }
        }


        //===================================================================================================
        /// <summary>
        /// This is executed when user wants to see the license
        /// </summary>
        /// <param name="oSender">Sender object</param>
        /// <param name="oArgs">Event args</param>
        //===================================================================================================
        private void OnLicenseToolStripMenuItem_Click(object oSender, EventArgs oArgs)
        {
            string strUrl = "https://www.gnu.org/licenses/gpl-2.0.html";
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo(strUrl) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", strUrl);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", strUrl);
                }
            }
            catch (Exception oEx)
            {
                MessageBox.Show("Could not open browser: " + oEx.Message);
            }
        }


        private void OnCreateNextBetterCommunity_Click(object sender, EventArgs e)
        {
            if (m_oTexts.Count == 0 || !m_oTexts.ContainsKey(m_strCurrentSection))
                return;

            string[] astrPoints = m_oTexts[m_strCurrentSection].Keys.ToArray();
            string strCurrentPoint = astrPoints[m_nCurrentPoint];

            if (m_nCurrentValueIndex >= 0 &&
                m_nCurrentValueIndex < m_oTexts
                    [m_strCurrentSection]
                    [strCurrentPoint].Count)
            {
                string strText = m_oTexts
                    [m_strCurrentSection]
                    [strCurrentPoint]
                    [m_nCurrentValueIndex]
                    .Text;

                using (TextChangeForm oForm = new TextChangeForm(strText))
                {
                    if (oForm.ShowDialog() == DialogResult.OK)
                    {
                        m_oTexts
                            [m_strCurrentSection]
                            [strCurrentPoint]
                            .Insert(m_nCurrentValueIndex, new
                            AssessmentText(++m_nMaxId, strText));

                        m_oTexts
                            [m_strCurrentSection]
                            [strCurrentPoint]
                            [m_nCurrentValueIndex]
                            .Text
                        = oForm.AssessmentText;

                        ShowCurrentPoint();


                        if (m_strFilePath != null)
                        {
                            SaveSchoolReportExtended(m_oTexts, m_oFrequencies, m_oCoOccurences, m_strFilePath);
                        }
                    }
                }
            }

        }

        private void OnCreateNextWorseCommunity_Click(object sender, EventArgs e)
        {
            if (m_oTexts.Count == 0 || !m_oTexts.ContainsKey(m_strCurrentSection))
                return;

            string[] astrPoints = m_oTexts[m_strCurrentSection].Keys.ToArray();
            string strCurrentPoint = astrPoints[m_nCurrentPoint];

            if (m_nCurrentValueIndex >= 0 &&
                m_nCurrentValueIndex < m_oTexts
                    [m_strCurrentSection]
                    [strCurrentPoint].Count)
            {
                string strText = m_oTexts
                    [m_strCurrentSection]
                    [strCurrentPoint]
                    [m_nCurrentValueIndex]
                    .Text;

                using (TextChangeForm oForm = new TextChangeForm(strText))
                {
                    if (oForm.ShowDialog() == DialogResult.OK)
                    {
                        m_oTexts
                            [m_strCurrentSection]
                            [strCurrentPoint]
                            .Insert(++m_nCurrentValueIndex, new
                            AssessmentText(++m_nMaxId, strText));

                        m_oTexts
                            [m_strCurrentSection]
                            [strCurrentPoint]
                            [m_nCurrentValueIndex]
                            .Text
                        = oForm.AssessmentText;

                        ShowCurrentPoint();

                        if (m_strFilePath != null)
                        {
                            SaveSchoolReportExtended(m_oTexts, m_oFrequencies, m_oCoOccurences, m_strFilePath);
                        }

                    }
                }
            }

        }

    }
}

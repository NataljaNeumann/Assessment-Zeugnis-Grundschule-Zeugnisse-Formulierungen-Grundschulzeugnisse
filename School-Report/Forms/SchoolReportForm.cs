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
using System.Text.RegularExpressions;
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
                m_ctlReportParts.Enabled = false;
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
                    m_ctlReportParts.Enabled = true;
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

                        /*
                        strValue = strValue.Replace("Du", "er/sie", StringComparison.InvariantCultureIgnoreCase)
                            .Replace("Deinen", "seinen/ihren", StringComparison.InvariantCultureIgnoreCase)
                            .Replace("Deinem", "seinem/ihrem", StringComparison.InvariantCultureIgnoreCase)
                            .Replace("Deine", "seine/ihre", StringComparison.InvariantCultureIgnoreCase)
                            .Replace("Dir", "ihm/ihr", StringComparison.InvariantCultureIgnoreCase)
                            .Replace("Dich", "sich", StringComparison.InvariantCultureIgnoreCase)
                            .Replace("hast", "hat").Replace("bist", "ist").Replace("st ", " ");
                       

                        strValue = char.ToUpper(strValue[0]) + strValue.Substring(1);
                         */

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
                m_btnNextWorseCommunity.Enabled = true;
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
            string strCulture = System.Threading.Thread.CurrentThread.CurrentUICulture.IetfLanguageTag;

            if (strCulture.StartsWith("en"))
            {
                AdaptTextToGenderAndName_English(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("es"))
            {
                AdaptTextToGenderAndName_Spanish(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("pt"))
            {
                AdaptTextToGenderAndName_Portuguese(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("it"))
            {
                AdaptTextToGenderAndName_Italian(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("fr"))
            {
                AdaptTextToGenderAndName_French(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("ru"))
            {
                AdaptTextToGenderAndName_Russian(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("ja"))
            {
                AdaptTextToGenderAndName_Japanese(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("ko"))
            {
                AdaptTextToGenderAndName_Korean(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("pl"))
            {
                AdaptTextToGenderAndName_Polish(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("fi"))
            {
                AdaptTextToGenderAndName_Finnish(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("no"))
            {
                AdaptTextToGenderAndName_Norsk(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("nl"))
            {
                AdaptTextToGenderAndName_Dutch(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("da"))
            {
                AdaptTextToGenderAndName_Danish(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("sw"))
            {
                AdaptTextToGenderAndName_Swedish(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
            {
                // Simplified Chinese regions
                if (strCulture.EndsWith("CN", StringComparison.OrdinalIgnoreCase) ||
                    strCulture.EndsWith("SG", StringComparison.OrdinalIgnoreCase) ||
                    strCulture.EndsWith("MY", StringComparison.OrdinalIgnoreCase))
                {
                    AdaptTextToGenderAndName_ChineseSimplified(tbxToAdapt);
                }
                else
                {
                    // Traditional Chinese regions (TW, HK, MO, and any future ones)
                    AdaptTextToGenderAndName_ChineseTraditional(tbxToAdapt);
                }
            }
            else
            {
                // Default is German, since the primary case I'm working on is German primary school
                AdaptTextToGenderAndName_German(tbxToAdapt);
            }
        }


        //===================================================================================================
        /// <summary>
        /// Tests, if Names in German genitive 
        /// </summary>
        /// <param name="strName"></param>
        /// <returns>true iff the genitiv needs apostrophy instead of s</returns>
        //===================================================================================================
        static bool GermanNameNeedsApostrophe(string strName)
        {
            if (string.IsNullOrWhiteSpace(strName))
                return false;

            strName = strName.Trim();

            // Für Mehrfachnamen (z.B. "Hans-Peter") nur den letzten Teil betrachten
            string lastPart = strName.Split(' ', '-').Last().ToLower();

            // Typische Endungen, die im Genitiv nur einen Apostroph bekommen
            string[] astrSoundEndings = new[]
            {
                "s", "ß", "z", "x", "ce" /* z.B. Alice */
            };

            foreach (string strEnding in astrSoundEndings)
            {
                if (lastPart.EndsWith(strEnding))
                    return true;
            }

            return false;
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

            string strGenitivEnding = GermanNameNeedsApostrophe(strNameToUse) ? "'" : "s";

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
                    .Replace(" Namens ", " " + strNameToUse + strGenitivEnding +" ");
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
                    .Replace(" Namens ", " " + strNameToUse + strGenitivEnding + " ");
            }
            else
            {
                strCurrentText = strCurrentText
                    .Replace(" Name ", " " + strNameToUse + " ")
                    .Replace(" Namens ", " " + strNameToUse + strGenitivEnding + " ");
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
        /// Adapts texts in textboxes to given genders and names in Dutch
        /// </summary>
        /// <param name="tbxToAdapt">Textbox for adaptation of text</param>
        //===================================================================================================
        void AdaptTextToGenderAndName_Dutch(TextBox tbxToAdapt)
        {
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";

            // Insert name placeholder only once
            if (m_chkAddName.Checked && !strCurrentText.Contains("Naam"))
            {
                string? strToReplace = null;
                int nPosToReplace = -1;

                foreach (string strTextToSearch in new string[]
                                { " Hij/zij ", " Hem/haar ", " Zijn/haar " })
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" Hij/zij ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                    }
                }

                if (strToReplace != null)
                {
                    if (" Zijn/haar ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Dutch genitive is simply "Naams"
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " Naams " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " Naam " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                }
            }

            // Determine name to use
            string strNameToUse = m_tbxName.Text;
            if (string.IsNullOrWhiteSpace(strNameToUse))
                strNameToUse = "NAAM ONTBREEKT";

            // Dutch genitive is always "s" (no apostrophe logic needed)
            string strGenitiveEnding = "s";

            // Gender-specific adaptation
            if (m_rbMale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" hij/zij ", " hij ")
                    .Replace(" Hij/zij ", " Hij ")
                    .Replace(" hem/haar ", " hem ")
                    .Replace(" Hem/haar ", " Hem ")
                    .Replace(" zijn/haar ", " zijn ")
                    .Replace(" Zijn/haar ", " Zijn ")
                    .Replace(" Naam ", " " + strNameToUse + " ")
                    .Replace(" Naams ", " " + strNameToUse + strGenitiveEnding + " ");
            }
            else if (m_rbFemale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" hij/zij ", " zij ")
                    .Replace(" Hij/zij ", " Zij ")
                    .Replace(" hem/haar ", " haar ")
                    .Replace(" Hem/haar ", " Haar ")
                    .Replace(" zijn/haar ", " haar ")
                    .Replace(" Zijn/haar ", " Haar ")
                    .Replace(" Naam ", " " + strNameToUse + " ")
                    .Replace(" Naams ", " " + strNameToUse + strGenitiveEnding + " ");
            }
            else
            {
                // Neutral: only insert the name
                strCurrentText = strCurrentText
                    .Replace(" Naam ", " " + strNameToUse + " ")
                    .Replace(" Naams ", " " + strNameToUse + strGenitiveEnding + " ");
            }

            tbxToAdapt.Text = strCurrentText.Trim().Replace(" .", ".");
        }


        //===================================================================================================
        /// <summary>
        /// Adapts texts in textboxes to given genders and names in Swedish
        /// </summary>
        /// <param name="tbxToAdapt">Textbox for adaptation of text</param>
        //===================================================================================================
        void AdaptTextToGenderAndName_Swedish(TextBox tbxToAdapt)
        {
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";

            // Insert name placeholder only once
            if (m_chkAddName.Checked && !strCurrentText.Contains("Namn"))
            {
                string? strToReplace = null;
                int nPosToReplace = -1;

                foreach (string strTextToSearch in new string[]
                                { " Han/hon ", " Honom/henne ", " Hans/hennes " })
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" Han/hon ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                    }
                }

                if (strToReplace != null)
                {
                    if (" Hans/hennes ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Swedish genitive: Namnets
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " Namnets " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " Namn " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                }
            }

            // Determine name to use
            string strNameToUse = m_tbxName.Text;
            if (string.IsNullOrWhiteSpace(strNameToUse))
                strNameToUse = "NAMN SAKNAS";

            // Swedish genitive is always "s"
            string strGenitiveEnding = "s";

            // Gender-specific adaptation
            if (m_rbMale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" han/hon ", " han ")
                    .Replace(" Han/hon ", " Han ")
                    .Replace(" honom/henne ", " honom ")
                    .Replace(" Honom/henne ", " Honom ")
                    .Replace(" hans/hennes ", " hans ")
                    .Replace(" Hans/hennes ", " Hans ")
                    .Replace(" Namn ", " " + strNameToUse + " ")
                    .Replace(" Namnets ", " " + strNameToUse + strGenitiveEnding + " ");
            }
            else if (m_rbFemale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" han/hon ", " hon ")
                    .Replace(" Han/hon ", " Hon ")
                    .Replace(" honom/henne ", " henne ")
                    .Replace(" Honom/henne ", " Henne ")
                    .Replace(" hans/hennes ", " hennes ")
                    .Replace(" Hans/hennes ", " Hennes ")
                    .Replace(" Namn ", " " + strNameToUse + " ")
                    .Replace(" Namnets ", " " + strNameToUse + strGenitiveEnding + " ");
            }
            else
            {
                // Neutral: only insert the name
                strCurrentText = strCurrentText
                    .Replace(" Namn ", " " + strNameToUse + " ")
                    .Replace(" Namnets ", " " + strNameToUse + strGenitiveEnding + " ");
            }

            tbxToAdapt.Text = strCurrentText.Trim().Replace(" .", ".");
        }


        //===================================================================================================
        /// <summary>
        /// Adapts texts in textboxes to given genders and names in Norwegian (Bokmål)
        /// </summary>
        /// <param name="tbxToAdapt">Textbox for adaptation of text</param>
        //===================================================================================================
        void AdaptTextToGenderAndName_Norsk(TextBox tbxToAdapt)
        {
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";

            // Insert name placeholder only once
            if (m_chkAddName.Checked && !strCurrentText.Contains("Navn"))
            {
                string? strToReplace = null;
                int nPosToReplace = -1;

                foreach (string strTextToSearch in new string[]
                                { " Han/hun ", " Ham/henne ", " Hans/hennes " })
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" Han/hun ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                    }
                }

                if (strToReplace != null)
                {
                    if (" Hans/hennes ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Norwegian genitive: Navnets
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " Navnets " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " Navn " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                }
            }

            // Determine name to use
            string strNameToUse = m_tbxName.Text;
            if (string.IsNullOrWhiteSpace(strNameToUse))
                strNameToUse = "NAVN MANGLER";

            // Norwegian genitive is always "s"
            string strGenitiveEnding = "s";

            // Gender-specific adaptation
            if (m_rbMale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" han/hun ", " han ")
                    .Replace(" Han/hun ", " Han ")
                    .Replace(" ham/henne ", " ham ")
                    .Replace(" Ham/henne ", " Ham ")
                    .Replace(" hans/hennes ", " hans ")
                    .Replace(" Hans/hennes ", " Hans ")
                    .Replace(" Navn ", " " + strNameToUse + " ")
                    .Replace(" Navnets ", " " + strNameToUse + strGenitiveEnding + " ");
            }
            else if (m_rbFemale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" han/hun ", " hun ")
                    .Replace(" Han/hun ", " Hun ")
                    .Replace(" ham/henne ", " henne ")
                    .Replace(" Ham/henne ", " Henne ")
                    .Replace(" hans/hennes ", " hennes ")
                    .Replace(" Hans/hennes ", " Hennes ")
                    .Replace(" Navn ", " " + strNameToUse + " ")
                    .Replace(" Navnets ", " " + strNameToUse + strGenitiveEnding + " ");
            }
            else
            {
                // Neutral: only insert the name
                strCurrentText = strCurrentText
                    .Replace(" Navn ", " " + strNameToUse + " ")
                    .Replace(" Navnets ", " " + strNameToUse + strGenitiveEnding + " ");
            }

            tbxToAdapt.Text = strCurrentText.Trim().Replace(" .", ".");
        }

        //===================================================================================================
        /// <summary>
        /// Adapts texts in textboxes to given genders and names in Danish
        /// </summary>
        /// <param name="tbxToAdapt">Textbox for adaptation of text</param>
        //===================================================================================================
        void AdaptTextToGenderAndName_Danish(TextBox tbxToAdapt)
        {
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";

            // Insert name placeholder only once
            if (m_chkAddName.Checked && !strCurrentText.Contains("Navn"))
            {
                string? strToReplace = null;
                int nPosToReplace = -1;

                foreach (string strTextToSearch in new string[]
                                { " Han/hun ", " Ham/hende ", " Hans/hendes " })
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" Han/hun ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                    }
                }

                if (strToReplace != null)
                {
                    if (" Hans/hendes ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Danish genitive: Navnets
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " Navnets " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " Navn " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                }
            }

            // Determine name to use
            string strNameToUse = m_tbxName.Text;
            if (string.IsNullOrWhiteSpace(strNameToUse))
                strNameToUse = "NAVN MANGLER";

            // Danish genitive is always "s"
            string strGenitiveEnding = "s";

            // Gender-specific adaptation
            if (m_rbMale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" han/hun ", " han ")
                    .Replace(" Han/hun ", " Han ")
                    .Replace(" ham/hende ", " ham ")
                    .Replace(" Ham/hende ", " Ham ")
                    .Replace(" hans/hendes ", " hans ")
                    .Replace(" Hans/hendes ", " Hans ")
                    .Replace(" Navn ", " " + strNameToUse + " ")
                    .Replace(" Navnets ", " " + strNameToUse + strGenitiveEnding + " ");
            }
            else if (m_rbFemale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" han/hun ", " hun ")
                    .Replace(" Han/hun ", " Hun ")
                    .Replace(" ham/hende ", " hende ")
                    .Replace(" Ham/hende ", " Hende ")
                    .Replace(" hans/hendes ", " hendes ")
                    .Replace(" Hans/hendes ", " Hendes ")
                    .Replace(" Navn ", " " + strNameToUse + " ")
                    .Replace(" Navnets ", " " + strNameToUse + strGenitiveEnding + " ");
            }
            else
            {
                // Neutral: only insert the name
                strCurrentText = strCurrentText
                    .Replace(" Navn ", " " + strNameToUse + " ")
                    .Replace(" Navnets ", " " + strNameToUse + strGenitiveEnding + " ");
            }

            tbxToAdapt.Text = strCurrentText.Trim().Replace(" .", ".");
        }

        //===================================================================================================
        /// <summary>
        /// Adapts texts in textboxes to given genders and names in Japanese
        /// </summary>
        /// <param name="tbxToAdapt">Textbox for adaptation of text</param>
        //===================================================================================================
        void AdaptTextToGenderAndName_Japanese(TextBox tbxToAdapt)
        {
            string strCurrentText = " " + tbxToAdapt.Text.Replace("。", " 。") + " ";

            string strName = m_tbxName.Text.Trim();
            if (string.IsNullOrWhiteSpace(strName))
                strName = "名前未設定"; // „Name not set“

            // polite suffix
            string strHonorificSuffix = "さん";
            string strFullName = strName + strHonorificSuffix;

            if (m_chkAddName.Checked && !strCurrentText.Contains("名前"))
            {
                string? strToReplace = null;
                int nPosToReplace = -1;

                foreach (string strTextToSearch in new string[]
                                { " 彼/彼女 ", " 彼の/彼女の ", " 彼を/彼女を "})
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" 彼/彼女 ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                    }
                }

                if (strToReplace != null)
                {
                    if (" 彼の/彼女の ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " 名前の " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else
                    if (" 彼を/彼女を ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " 名前を " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " 名前 " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                }
            }

            // insert name
            strCurrentText = strCurrentText.Replace("名前", strFullName);

            // gender - specific adaptation
            if (m_rbMale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" 彼の/彼女の ", " 彼の ")
                    .Replace( "彼を/彼女を ", " 彼を ")
                    .Replace(" 彼/彼女 ", " 彼 ");
            }
            else if (m_rbFemale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" 彼の/彼女の ", " 彼女の ")
                    .Replace(" 彼を/彼女を ", " 彼女を ")
                    .Replace(" 彼/彼女 ", " 彼女 ");
            }

            tbxToAdapt.Text = strCurrentText.Trim().Replace(" 。", "。");
        }

        //===================================================================================================
        /// <summary>
        /// Adapts texts in textboxes to given genders and names in Korean
        /// </summary>
        /// <param name="tbxToAdapt">Textbox for adaptation of text</param>
        //===================================================================================================
        void AdaptTextToGenderAndName_Korean(TextBox tbxToAdapt)
        {
            // Add spaces around the Korean sentence-ending period " ."
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";

            string strName = m_tbxName.Text.Trim();
            if (string.IsNullOrWhiteSpace(strName))
                strName = "이름미설정"; // "Name not set"

            // polite suffix
            string strHonorificSuffix = "씨";
            string strFullName = strName + strHonorificSuffix;

            // Insert name instead of pronouns (only first occurrence)
            if (m_chkAddName.Checked && !strCurrentText.Contains("이름"))
            {
                string? strToReplace = null;
                int nPosToReplace = -1;

                foreach (string strTextToSearch in new string[]
                                { " 그/그녀 ", " 그의/그녀의 ", " 그를/그녀를 "})
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" 그/그녀 ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                    }
                }

                if (strToReplace != null)
                {
                    if (" 그의/그녀의 ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " 이름의 " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else if (" 그를/그녀를 ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " 이름을 " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " 이름 " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                }
            }

            // Insert name
            strCurrentText = strCurrentText.Replace("이름", strFullName);

            // Gender-specific adaptation
            if (m_rbMale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" 그의/그녀의 ", " 그의 ")
                    .Replace(" 그를/그녀를 ", " 그를 ")
                    .Replace(" 그/그녀 ", " 그 ");
            }
            else if (m_rbFemale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" 그의/그녀의 ", " 그녀의 ")
                    .Replace(" 그를/그녀를 ", " 그녀를 ")
                    .Replace(" 그/그녀 ", " 그녀 ");
            }

            tbxToAdapt.Text = strCurrentText.Trim().Replace(" .", ".");
        }

        //===================================================================================================
        /// <summary>
        /// Adapts texts in textboxes to given genders and names in Simplified Chinese (PRC)
        /// </summary>
        /// <param name="tbxToAdapt">Textbox for adaptation of text</param>
        //===================================================================================================
        void AdaptTextToGenderAndName_ChineseSimplified(TextBox tbxToAdapt)
        {
            // Add spaces around the Chinese sentence-ending period "。"
            string strCurrentText = " " + tbxToAdapt.Text.Replace("。", " 。") + " ";

            string strName = m_tbxName.Text.Trim();
            if (string.IsNullOrWhiteSpace(strName))
                strName = "姓名未设置"; // "Name not set"

            // Chinese does not use honorific suffixes like Japanese/Korean
            string strFullName = strName;

            // Insert name instead of pronouns (only first occurrence)
            if (m_chkAddName.Checked && !strCurrentText.Contains("姓名"))
            {
                string? strToReplace = null;
                int nPosToReplace = -1;

                foreach (string strTextToSearch in new string[]
                                { " 他/她 ", " 他的/她的 " })
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" 他/她 ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                    }
                }

                if (strToReplace != null)
                {
                    if (" 他的/她的 ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " 姓名的 " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else if (" 他/她 ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // object or subject form
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " 姓名 " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                }
            }

            // Insert name
            strCurrentText = strCurrentText.Replace("姓名", strFullName);

            // Gender-specific adaptation
            if (m_rbMale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" 他的/她的 ", " 他的 ")
                    .Replace(" 他/她 ", " 他 ");
            }
            else if (m_rbFemale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" 他的/她的 ", " 她的 ")
                    .Replace(" 他/她 ", " 她 ");
            }

            tbxToAdapt.Text = strCurrentText.Trim().Replace(" 。", "。");
        }


        //===================================================================================================
        /// <summary>
        /// Adapts texts in textboxes to given genders and names in Traditional Chinese (Taiwan)
        /// </summary>
        /// <param name="tbxToAdapt">Textbox for adaptation of text</param>
        //===================================================================================================
        void AdaptTextToGenderAndName_ChineseTraditional(TextBox tbxToAdapt)
        {
            // Add spaces around the Chinese sentence-ending period "。"
            string strCurrentText = " " + tbxToAdapt.Text.Replace("。", " 。") + " ";

            string strName = m_tbxName.Text.Trim();
            if (string.IsNullOrWhiteSpace(strName))
                strName = "姓名未設定"; // "Name not set"

            // Traditional Chinese does not use honorific suffixes like Japanese/Korean
            string strFullName = strName;

            // Insert name instead of pronouns (only first occurrence)
            if (m_chkAddName.Checked && !strCurrentText.Contains("姓名"))
            {
                string? strToReplace = null;
                int nPosToReplace = -1;

                foreach (string strTextToSearch in new string[]
                                { " 他/她 ", " 他的/她的 " }) // object form same as subject
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" 他/她 ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                    }
                }

                if (strToReplace != null)
                {
                    if (" 他的/她的 ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " 姓名的 " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else if (" 他/她 ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // subject or object form
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " 姓名 " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                }
            }

            // Insert name
            strCurrentText = strCurrentText.Replace("姓名", strFullName);

            // Gender-specific adaptation
            if (m_rbMale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" 他的/她的 ", " 他的 ")
                    .Replace(" 他/她 ", " 他 ");
            }
            else if (m_rbFemale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" 他的/她的 ", " 她的 ")
                    .Replace(" 他/她 ", " 她 ");
            }

            tbxToAdapt.Text = strCurrentText.Trim().Replace(" 。", "。");
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



        enum PolishCase { 
            Nominative, 
            Genitive,
            Dative, 
            Accusative 
        }


        //===================================================================================================
        /// <summary>
        /// Declines Polish names 
        /// </summary>
        /// <param name="strName">The name to decline</param>
        /// <param name="bIsMale">Is the name male</param>
        /// <param name="eTargetCase">The case</param>
        /// <returns>Declined name</returns>
        //===================================================================================================
        string DeclinePolishName(string strName, bool bIsMale, PolishCase eTargetCase)
        {
            if (string.IsNullOrWhiteSpace(strName))
                return strName;

            string strResultingName = strName.Trim();
            string strLower = strResultingName.ToLower();

            // ---------------------------
            // FEMALE NAMES
            // ---------------------------
            if (!bIsMale)
            {
                // Anna → Anny, Annie, Annę
                if (strLower.EndsWith("a"))
                {
                    string strStem = strResultingName.Substring(0, strResultingName.Length - 1);

                    switch (eTargetCase)
                    {
                        case PolishCase.Genitive: return strStem + "y";
                        case PolishCase.Dative: return strStem + "ie";
                        case PolishCase.Accusative: return strStem + "ę";
                    }
                }

                // Maria → Marii, Marii, Marię
                if (strLower.EndsWith("ia"))
                {
                    string strStem = strResultingName.Substring(0, strResultingName.Length - 2);

                    switch (eTargetCase)
                    {
                        case PolishCase.Genitive: return strStem + "ii";
                        case PolishCase.Dative: return strStem + "ii";
                        case PolishCase.Accusative: return strStem + "ię";
                    }
                }

                // consonant-ending female names (Beatrycze, Ingrid)
                return strResultingName;
            }

            // ---------------------------
            // MALE NAMES
            // ---------------------------

            // Kuba → Kuby, Kubie, Kubę
            if (strLower.EndsWith("a"))
            {
                string stem = strResultingName.Substring(0, strResultingName.Length - 1);

                switch (eTargetCase)
                {
                    case PolishCase.Genitive: return stem + "y";
                    case PolishCase.Dative: return stem + "ie";
                    case PolishCase.Accusative: return stem + "ę";
                }
            }

            // Marek → Marka, Markowi, Marka
            if (strLower.EndsWith("ek"))
            {
                string stem = strResultingName.Substring(0, strResultingName.Length - 2); // remove "ek" → "Marek" → "Mar"

                switch (eTargetCase)
                {
                    case PolishCase.Genitive: return stem + "ka";
                    case PolishCase.Dative: return stem + "kowi";
                    case PolishCase.Accusative: return stem + "ka";
                }
            }

            // Paweł → Pawła, Pawłowi, Pawła
            if (strLower.EndsWith("eł") || strLower.EndsWith("el"))
            {
                string stem = strResultingName.Substring(0, strResultingName.Length - 1);

                switch (eTargetCase)
                {
                    case PolishCase.Genitive: return stem + "a";
                    case PolishCase.Dative: return stem + "owi";
                    case PolishCase.Accusative: return stem + "a";
                }
            }

            // Adam → Adama, Adamowi, Adama
            if (char.IsLetter(strResultingName.Last()))
            {
                switch (eTargetCase)
                {
                    case PolishCase.Genitive: return strResultingName + "a";
                    case PolishCase.Dative: return strResultingName + "owi";
                    case PolishCase.Accusative: return strResultingName + "a";
                }
            }

            return strResultingName;
        }

        void AdaptTextToGenderAndName_Polish(TextBox tbxToAdapt)
        {
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";

            if (m_chkAddName.Checked && !strCurrentText.Contains("Imię"))
            {
                string? strToReplace = null;
                int nPosToReplace = -1;

                foreach (string strTextToSearch in new string[]
                                { " On/ona ", " Jemu/jej ", " Jego/jej ", " Swoje/swoją/swojego " })
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" On/ona ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                    }
                }

                if (strToReplace != null)
                {
                    if (" Swoje/swoją/swojego ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " Imienia " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else if (" Jemu/jej ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " ImieniuD " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else if (" Jego/jej ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " ImieniaW " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " Imię " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                }
            }

            string strNameToUse = m_tbxName.Text;
            if (string.IsNullOrWhiteSpace(strNameToUse))
                strNameToUse = "IMIĘ BRAKUJE";

            if (m_rbMale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" on/ona ", " on ")
                    .Replace(" On/ona ", " On ")
                    .Replace(" jemu/jej ", " jemu ")
                    .Replace(" Jemu/jej ", " Jemu ")
                    .Replace(" jego/jej ", " jego ")
                    .Replace(" Jego/jej ", " Jego ")
                    .Replace(" Imię ", " " + strNameToUse + " ")
                    .Replace(" Imienia ", " " + DeclinePolishName(strNameToUse, true, PolishCase.Genitive) + " ")
                    .Replace(" ImieniuD ", " " + DeclinePolishName(strNameToUse, true, PolishCase.Dative) + " ")
                    .Replace(" ImieniaW ", " " + DeclinePolishName(strNameToUse, true, PolishCase.Accusative) + " ");
            }
            else if (m_rbFemale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" on/ona ", " ona ")
                    .Replace(" On/ona ", " Ona ")
                    .Replace(" jemu/jej ", " jej ")
                    .Replace(" Jemu/jej ", " Jej ")
                    .Replace(" jego/jej ", " jej ")
                    .Replace(" Jego/jej ", " Jej ")
                    .Replace(" Imię ", " " + strNameToUse + " ")
                    .Replace(" Imienia ", " " + DeclinePolishName(strNameToUse, false, PolishCase.Genitive) + " ")
                    .Replace(" ImieniuD ", " " + DeclinePolishName(strNameToUse, false, PolishCase.Dative) + " ")
                    .Replace(" ImieniaW ", " " + DeclinePolishName(strNameToUse, false, PolishCase.Accusative) + " ");
            }
            else
            {
                strCurrentText = strCurrentText
                    .Replace(" Imię ", " " + strNameToUse + " ")
                    .Replace(" Imienia ", " " + DeclinePolishName(strNameToUse, m_rbFemale.Checked, PolishCase.Genitive) + " ")
                    .Replace(" ImieniuD ", " " + DeclinePolishName(strNameToUse, m_rbMale.Checked, PolishCase.Dative) + " ")
                    .Replace(" ImieniaW ", " " + DeclinePolishName(strNameToUse, m_rbMale.Checked, PolishCase.Accusative) + " ");
            }

            tbxToAdapt.Text = strCurrentText.Trim().Replace(" .", ".");
        }


        enum FinnishCase
        {
            Nominative, 
            Genitive, 
            Allative,
            Accusative
        }

        //===================================================================================================
        /// <summary>
        /// Declines names according to Finnish rules
        /// </summary>
        /// <param name="strName">The name to transform</param>
        /// <param name="bIsMale">Indicates if the name is male</param>
        /// <param name="eTargetCase">The case</param>
        /// <returns>Declined name</returns>
        //===================================================================================================
        string DeclineFinnishName(string strName, bool bIsMale, FinnishCase eTargetCase)
        {
            if (string.IsNullOrWhiteSpace(strName))
                return strName;

            string strLower = strName.ToLower();

            // Helper: consonant gradation (very simplified)
            string Gradate(string stem)
            {
                return stem
                    .Replace("kk", "k")
                    .Replace("pp", "p")
                    .Replace("tt", "t")
                    .Replace("k", "v"); // extremely simplified
            }

            // -------------------------
            // Names ending in -i (Matti, Mikki, Jari)
            // -------------------------
            if (strLower.EndsWith("i"))
            {
                string stem = strName.Substring(0, strName.Length - 1);

                switch (eTargetCase)
                {
                    case FinnishCase.Genitive: return Gradate(stem) + "in";
                    case FinnishCase.Allative: return Gradate(stem) + "ille";
                    case FinnishCase.Accusative: return Gradate(stem) + "in";
                }
            }

            // -------------------------
            // Names ending in -a / -ä (Saara, Aila, Aino)
            // -------------------------
            if (strLower.EndsWith("a") || strLower.EndsWith("ä"))
            {
                string stem = strName.Substring(0, strName.Length - 1);

                switch (eTargetCase)
                {
                    case FinnishCase.Genitive: return stem + "n";
                    case FinnishCase.Allative: return stem + "lle";
                    case FinnishCase.Accusative: return stem + "a";
                }
            }

            // -------------------------
            // Names ending in -o / -ö / -u / -y (Mikko, Heikki, Tapio)
            // -------------------------
            if ("oöuy".Contains(strLower.Last()))
            {
                string stem = strName.Substring(0, strName.Length - 1);

                switch (eTargetCase)
                {
                    case FinnishCase.Genitive: return stem + "n";
                    case FinnishCase.Allative: return stem + "lle";
                    case FinnishCase.Accusative: return stem + "n";
                }
            }

            // -------------------------
            // Names ending in consonant (rare but possible)
            // -------------------------
            if (char.IsLetter(strName.Last()))
            {
                switch (eTargetCase)
                {
                    case FinnishCase.Genitive: return strName + "in";
                    case FinnishCase.Allative: return strName + "ille";
                    case FinnishCase.Accusative: return strName + "in";
                }
            }

            return strName;
        }


        //===================================================================================================
        /// <summary>
        /// Adapts text of a textbox, according to Finnish rules
        /// </summary>
        /// <param name="tbxToAdapt">Textbox to adapt the text</param>
        //===================================================================================================
        void AdaptTextToGenderAndName_Finnish(TextBox tbxToAdapt)
        {
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";

            if (m_chkAddName.Checked && !strCurrentText.Contains("Nimi"))
            {
                string? strToReplace = null;
                int nPosToReplace = -1;

                foreach (string strTextToSearch in new string[]
                                { " Hän ", " Hänelle ", " Hänen ", " Oma/omansa " })
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" Hän ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                    }
                }

                if (strToReplace != null)
                {
                    if (" Oma/omansa ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " Nimen " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else if (" Hänelle ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " Nimelle " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else if (" Hänen ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " NimenG " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " Nimi " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                }
            }

            string strNameToUse = m_tbxName.Text;
            if (string.IsNullOrWhiteSpace(strNameToUse))
                strNameToUse = "NIMI PUUTTUU";

            bool bMale = m_rbMale.Checked;
            bool bFemale = m_rbFemale.Checked;

            if (bMale)
            {
                strCurrentText = strCurrentText
                    .Replace(" Hän ", " hän ")
                    .Replace(" Hänelle ", " hänelle ")
                    .Replace(" Hänen ", " hänen ")
                    .Replace(" Nimi ", " " + strNameToUse + " ")
                    .Replace(" Nimen ", " " + DeclineFinnishName(strNameToUse, true, FinnishCase.Genitive) + " ")
                    .Replace(" Nimelle ", " " + DeclineFinnishName(strNameToUse, true, FinnishCase.Allative) + " ")
                    .Replace(" NimenG ", " " + DeclineFinnishName(strNameToUse, true, FinnishCase.Genitive) + " ");
            }
            else if (bFemale)
            {
                strCurrentText = strCurrentText
                    .Replace(" Hän ", " hän ")
                    .Replace(" Hänelle ", " hänelle ")
                    .Replace(" Hänen ", " hänen ")
                    .Replace(" Nimi ", " " + strNameToUse + " ")
                    .Replace(" Nimen ", " " + DeclineFinnishName(strNameToUse, false, FinnishCase.Genitive) + " ")
                    .Replace(" Nimelle ", " " + DeclineFinnishName(strNameToUse, false, FinnishCase.Allative) + " ")
                    .Replace(" NimenG ", " " + DeclineFinnishName(strNameToUse, false, FinnishCase.Genitive) + " ");
            }
            else
            {
                strCurrentText = strCurrentText
                    .Replace(" Nimi ", " " + strNameToUse + " ")
                    .Replace(" Nimen ", " " + DeclineFinnishName(strNameToUse, bMale, FinnishCase.Genitive) + " ")
                    .Replace(" Nimelle ", " " + DeclineFinnishName(strNameToUse, bMale, FinnishCase.Allative) + " ")
                    .Replace(" NimenG ", " " + DeclineFinnishName(strNameToUse, bMale, FinnishCase.Genitive) + " ");
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

        //===================================================================================================
        /// <summary>
        /// This is executed when user clicks on the "Copy" button
        /// </summary>
        /// <param name="oSender">Sender object</param>
        /// <param name="oArgs">Event args</param>
        //===================================================================================================
        private void OnCopyText_Click(object oSender, EventArgs oArgs)
        {
            if (!string.IsNullOrEmpty(m_tbxTextCommunity.Text))
            {
                Clipboard.SetText(m_tbxTextCommunity.Text);
            }
        }

    }
}

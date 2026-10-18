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
        /// Replacements of verbs and adjectives for languages that require this
        /// </summary>
        List<GenderReplacement> m_oGenderReplacements = new List<GenderReplacement>();

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
        /// The texts of the different secitions
        /// </summary>
        Dictionary<string, string> m_oSectionTexts = new Dictionary<string, string>();

        //===================================================================================================
        /// <summary>
        /// Constructs a new school report object
        /// </summary>
        //===================================================================================================
        public SchoolReportForm()
        {
            InitializeComponent();
            SetFont();
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
                    out m_oTexts, out m_oFrequencies, out m_oCoOccurences,
                    out m_oGenderReplacements, out m_nMaxId);

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
                    m_ctlUpDownPoints.Minimum = 0;
                    m_ctlUpDownPoints.Maximum = m_oTexts[m_strCurrentSection].Keys.Count - 1;
                    m_ctlReportParts.Enabled = true;
                    m_btnAddCurrentText.Enabled = true;
                    m_ctlUpDownPoints.Enabled = true;
                    m_tbxCurrentTextCommunity.Enabled = true;
                    m_btnNextWorseCommunity.Enabled = true;
                    m_btnNextBestCommunity.Enabled = true;
                    m_btnCreateNextWorseCommunity.Enabled = true;
                    m_btnCreateNextBetterCommunity.Enabled = true;
                    m_chkAddName.Enabled = true;
                    m_oNextToolStripMenuItem.Enabled = true;
                    m_ctlUpDownPoints.Enabled = true;
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
            out List<GenderReplacement> oGenderReplacements,
            out int nMaxId)
        {
            oReportData = new();
            oTextFrequencies = new();
            oTextCooccurrences = new();
            oGenderReplacements = new();

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


            // Load gendering replacements
            XElement? oGendering = oRoot.Element("gender-replacements");
            if (oGendering != null)
            {
                foreach (XElement oReplacementElement in oGendering.Elements("gender-replacement"))
                {
                    string strIndependent = (oReplacementElement.Attribute("strGenderIndependent")?.Value ??
                        throw new Exception("Missing 'strGenderIndependent' in <gender-replacement>"));

                    string strMale = (oReplacementElement.Attribute("strMaleForm")?.Value ??
                        throw new Exception("Missing 'strMaleForm' in <gender-replacement>"));

                    string strFemale = (oReplacementElement.Attribute("strFemaleForm")?.Value ??
                        throw new Exception("Missing 'strFemaleForm' in <gender-replacement>"));

                    oGenderReplacements.Add(new GenderReplacement
                    {
                        GenderIndependent = strIndependent,
                        MaleForm = strMale,
                        FemaleForm = strFemale
                    });
                }
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
            List<GenderReplacement> oGenderReplacements,
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

            // Gendering for adjectives, verbs
            XElement oGenderElem = new XElement("gender-replacements");
            foreach (var oGendering in oGenderReplacements)
            {
                oGenderElem.Add(new XElement("gender-replacement",
                    new XAttribute("strGenderIndependent", oGendering.GenderIndependent),
                    new XAttribute("strMaleForm", oGendering.MaleForm),
                    new XAttribute("strFemaleForm", oGendering.FemaleForm)));
            }
            oRoot.Add(oGenderElem);

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
                m_ctlUpDownPoints.Maximum = m_oTexts[m_strCurrentSection].Keys.Count - 1;
                if (m_oSectionTexts.ContainsKey(m_strCurrentSection))
                {
                    m_tbxTextCommunity.Text = m_oSectionTexts[m_strCurrentSection];
                } else
                {
                    m_tbxTextCommunity.Text = string.Empty;
                }

                m_ctlUpDownPoints.Value = 0;
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
            // first replace general replacements
            if (m_oGenderReplacements!=null && m_oGenderReplacements.Count>0 && 
                (m_rbMale.Checked || m_rbFemale.Checked) )
            {
                tbxToAdapt.Text = GenderReplacement.Apply(tbxToAdapt.Text, m_oGenderReplacements, m_rbMale.Checked);
            }

            // afterwards apply name replacements according to the rules of different languages
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
            if (strCulture.StartsWith("hi"))
            {
                AdaptTextToGenderAndName_Hindi(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("cs"))
            {
                AdaptTextToGenderAndName_Czech(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("hu"))
            {
                AdaptTextToGenderAndName_Hungarian(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("uk"))
            {
                AdaptTextToGenderAndName_Ukrainian(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("sk"))
            {
                AdaptTextToGenderAndName_Slovak(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("sl"))
            {
                AdaptTextToGenderAndName_Slovenian(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("bg"))
            {
                AdaptTextToGenderAndName_Bulgarian(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("tr"))
            {
                AdaptTextToGenderAndName_Turkish(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("ar"))
            {
                AdaptTextToGenderAndName_Arabic(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("fa"))
            {
                AdaptTextToGenderAndName_Farsi(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("ur"))
            {
                AdaptTextToGenderAndName_Urdu(tbxToAdapt);
            }
            else
            if (strCulture.StartsWith("he"))
            {
                AdaptTextToGenderAndName_Hebrew(tbxToAdapt);
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

        #region GERMAN
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
                    .Replace(" Namens ", " " + strNameToUse + strGenitivEnding + " ");
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
        #endregion

        #region ENGLISH
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
        #endregion

        #region FRENCH
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
        #endregion

        #region SPANISH
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
        #endregion

        #region PORTUGUESE
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
        #endregion

        #region ITALIAN
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
        #endregion

        #region DUTCH
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
        #endregion

        #region SWEDISH
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
        #endregion

        #region NORSK
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
        #endregion

        #region DANISH
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
        #endregion

        #region JAPANESE
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
                    .Replace("彼を/彼女を ", " 彼を ")
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
        #endregion

        #region KOREAN
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
        #endregion

        #region CHINESE
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
        #endregion

        #region RUSSIAN
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
                        (strToReplace == null || !" Он/она ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase)))
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
        #endregion

        #region POLISH
        //===================================================================================================
        /// <summary>
        /// Polish cases
        /// </summary>
        enum PolishCase
        {
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
        #endregion

        #region FINNISH
        //===================================================================================================
        /// <summary>
        /// Finnish cases
        /// </summary>
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
        #endregion

        #region HINDI

        //===================================================================================================
        /// <summary>Cases and postpositions used with Hindi personal names.</summary>
        //===================================================================================================
        enum HindiCase
        {
            Nominative,
            Genitive,          // का: masculine singular possessed noun, direct case
            GenitiveFeminine,  // की: feminine possessed noun
            GenitiveOblique,   // के: masculine plural or masculine oblique possessed noun
            Dative,
            Accusative,
            Ergative
        }

        //===================================================================================================
        /// <summary>Combines a Hindi personal name with the required postposition.</summary>
        /// <param name="strName">Personal name, normally unchanged before a postposition.</param>
        /// <param name="bIsMale">Kept for compatibility; does not determine का / की / के.</param>
        /// <param name="eTargetCase">Required case, including possessive agreement.</param>
        /// <returns>Name with the appropriate postposition.</returns>
        //===================================================================================================
        string DeclineHindiName(string strName, bool bIsMale, HindiCase eTargetCase)
        {
            if (string.IsNullOrWhiteSpace(strName))
                return strName;

            // Do not apply the common-noun लड़का -> लड़के rule to all personal names.
            // Special name forms, where needed, should be supplied explicitly by the caller.
            string strNameToUse = strName.Trim();

            switch (eTargetCase)
            {
                case HindiCase.Genitive: return strNameToUse + " का";
                case HindiCase.GenitiveFeminine: return strNameToUse + " की";
                case HindiCase.GenitiveOblique: return strNameToUse + " के";
                case HindiCase.Dative: return strNameToUse + " को";
                case HindiCase.Accusative: return strNameToUse + " को";
                case HindiCase.Ergative: return strNameToUse + " ने";
                default: return strNameToUse;
            }
        }

        //===================================================================================================
        /// <summary>Adapts prepared Hindi templates to the selected name and gender.</summary>
        /// <param name="tbxToAdapt">Textbox containing the template.</param>
        //===================================================================================================
        void AdaptTextToGenderAndName_Hindi(TextBox tbxToAdapt)
        {
            if (tbxToAdapt == null) throw new System.ArgumentNullException(nameof(tbxToAdapt));
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";
            bool bIsMale = m_rbMale.Checked;
            bool bIsFemale = m_rbFemale.Checked;
            string[] astrNameTokens = new string[] { "[नाम]", "[नाम:का]", "[नाम:की]", "[नाम:के]", "[नाम:को]", "[नाम:ने]" };
            bool bContainsName = false;
            foreach (string astrToken in astrNameTokens)
                if (strCurrentText.IndexOf(astrToken, System.StringComparison.CurrentCultureIgnoreCase) >= 0)
                { bContainsName = true; break; }

            if (m_chkAddName.Checked && !bContainsName)
            {
                string? strToReplace = null;
                int nPosToReplace = -1;
                int nSelected = -1;
                string[] astrSearch = new string[] { " वह ", " उसे ", " उसको ", " उसका ", " उसकी ", " उसके ", " उसने " };
                string[] astrReplacement = new string[] { " [नाम] ", " [नाम:को] ", " [नाम:को] ", " [नाम:का] ", " [नाम:की] ", " [नाम:के] ", " [नाम:ने] " };

                foreach (string strTextToSearch in astrSearch)
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, System.StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" वह ".Equals(strToReplace, System.StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                        nSelected = System.Array.IndexOf(astrSearch, strTextToSearch);
                    }
                }
                if (strToReplace != null)
                    strCurrentText = strCurrentText.Substring(0, nPosToReplace) + astrReplacement[nSelected] +
                        strCurrentText.Substring(nPosToReplace + strToReplace.Length);
            }
            // Only explicit alternatives referring to the selected person are resolved.
            // Reflexive possessives need agreement with the possessed noun instead.
            if (bIsMale || bIsFemale)
            {
                string[] astrAlternatives = new string[] { "करता/करती", "रहता/रहती", "जाता/जाती", "आता/आती", "चाहता/चाहती", "सकता/सकती", "रहा/रही", "गया/गई", "था/थी", "अच्छा/अच्छी", "खुश हुआ/खुश हुई" };
                string[] astrGenderReplacements = new string[astrAlternatives.Length];
                for (int i = 0; i < astrAlternatives.Length; i++)
                    astrGenderReplacements[i] = astrAlternatives[i].Split('/')[bIsMale ? 0 : 1];
                strCurrentText = ReplaceGenderTokens(strCurrentText, astrAlternatives, astrGenderReplacements);
            }

            string strNameToUse = m_tbxName.Text;
            bool bNameMissing = string.IsNullOrWhiteSpace(strNameToUse);
            if (!bNameMissing) strNameToUse = strNameToUse.Trim();
            string[] astrNameReplacements = new string[astrNameTokens.Length];
            astrNameReplacements[0] = bNameMissing ? "[नाम उपलब्ध नहीं]" : DeclineHindiName(strNameToUse, bIsMale, HindiCase.Nominative);
            astrNameReplacements[1] = bNameMissing ? "[नाम उपलब्ध नहीं]" : DeclineHindiName(strNameToUse, bIsMale, HindiCase.Genitive);
            astrNameReplacements[2] = bNameMissing ? "[नाम उपलब्ध नहीं]" : DeclineHindiName(strNameToUse, bIsMale, HindiCase.GenitiveFeminine);
            astrNameReplacements[3] = bNameMissing ? "[नाम उपलब्ध नहीं]" : DeclineHindiName(strNameToUse, bIsMale, HindiCase.GenitiveOblique);
            astrNameReplacements[4] = bNameMissing ? "[नाम उपलब्ध नहीं]" : DeclineHindiName(strNameToUse, bIsMale, HindiCase.Dative);
            astrNameReplacements[5] = bNameMissing ? "[नाम उपलब्ध नहीं]" : DeclineHindiName(strNameToUse, bIsMale, HindiCase.Ergative);

            // A single pass keeps user-entered names literal, including placeholder text.
            tbxToAdapt.Text = ReplaceTokens(strCurrentText, astrNameTokens, astrNameReplacements).Trim().Replace(" .", ".");
        }

        //===================================================================================================
        /// <summary>
        /// Replaces gender token
        /// </summary>
        /// <param name="strText">Text to seaech</param>
        /// <param name="astrSearch">Search strings</param>
        /// <param name="astrReplacements">Replacements</param>
        /// <returns>Replaced string</returns>
        //===================================================================================================
        string ReplaceGenderTokens(string strText, string[] astrSearch, string[] astrReplacements)
        {
            var oResult = new System.Text.StringBuilder();
            var oCulture = System.Globalization.CultureInfo.CurrentCulture;
            int nStart = 0;

            while (nStart < strText.Length)
            {
                int nPosToReplace = -1;
                int nSelected = -1;

                for (int i = 0; i < astrSearch.Length; i++)
                {
                    int nPos = FindToken(strText, astrSearch[i], nStart);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0))
                    { nPosToReplace = nPos; nSelected = i; }
                }

                if (nSelected < 0) 
                    break;

                string strOriginal = strText.Substring(nPosToReplace, astrSearch[nSelected].Length);
                string strReplacement = astrReplacements[nSelected];

                if (strOriginal == strOriginal.ToUpper(oCulture)) 
                    strReplacement = strReplacement.ToUpper(oCulture);

                else if (char.IsUpper(strOriginal[0]))
                    strReplacement = char.ToUpper(strReplacement[0], oCulture) + strReplacement.Substring(1);

                oResult.Append(strText, nStart, nPosToReplace - nStart);
                oResult.Append(strReplacement);
                nStart = nPosToReplace + astrSearch[nSelected].Length;
            }
            oResult.Append(strText, nStart, strText.Length - nStart);
            return oResult.ToString();
        }

        //===================================================================================================
        /// <summary>
        /// String helpers: IndexOf search, explicit token boundaries, no pattern engine.
        /// Each language has its own helper names so all six files can share one class.
        /// </summary>
        /// <param name="cValue">Char to test</param>
        /// <returns>true iff it is a word char</returns>
        //===================================================================================================
        bool IsWordCharacter(char cValue)
        {
            var category = char.GetUnicodeCategory(cValue);
            return char.IsLetterOrDigit(cValue) || cValue == '_' || cValue == '/' ||
                category == System.Globalization.UnicodeCategory.NonSpacingMark ||
                category == System.Globalization.UnicodeCategory.SpacingCombiningMark ||
                category == System.Globalization.UnicodeCategory.EnclosingMark;
        }

        //===================================================================================================
        /// <summary>
        /// Searches a token
        /// </summary>
        /// <param name="strText">Text to search in</param>
        /// <param name="strToken">Token to search</param>
        /// <param name="nStart">Start position</param>
        /// <returns>Position of the token</returns>
        //===================================================================================================
        int FindToken(string strText, string strToken, int nStart)
        {
            int nPos = strText.IndexOf(strToken, nStart, System.StringComparison.CurrentCultureIgnoreCase);
            while (nPos >= 0)
            {
                bool left = strToken[0] == '[' || nPos == 0 || !IsWordCharacter(strText[nPos - 1]);
                int end = nPos + strToken.Length;
                bool right = strToken[strToken.Length - 1] == ']' || end == strText.Length ||
                    !IsWordCharacter(strText[end]);
                if (left && right) return nPos;
                nPos = strText.IndexOf(strToken, nPos + 1, System.StringComparison.CurrentCultureIgnoreCase);
            }
            return -1;
        }

        //===================================================================================================
        /// <summary>
        /// Replaces parts in text
        /// </summary>
        /// <param name="strText">Original Text</param>
        /// <param name="astrSearchTerms">Words to search</param>
        /// <param name="astrReplacements">Replacements</param>
        /// <returns>Replaced text</returns>
        //===================================================================================================
        string ReplaceTokens(string strText, string[] astrSearchTerms, string[] astrReplacements)
        {
            var oResult = new System.Text.StringBuilder();
            int nStart = 0;
            while (nStart < strText.Length)
            {
                int nPosToReplace = -1;
                int selected = -1;
                for (int i = 0; i < astrSearchTerms.Length; i++)
                {
                    int nPos = FindToken(strText, astrSearchTerms[i], nStart);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0))
                    {
                        nPosToReplace = nPos;
                        selected = i;
                    }
                }
                if (selected < 0) break;
                oResult.Append(strText, nStart, nPosToReplace - nStart);
                oResult.Append(astrReplacements[selected]);
                nStart = nPosToReplace + astrSearchTerms[selected].Length;
            }
            oResult.Append(strText, nStart, strText.Length - nStart);
            return oResult.ToString();
        }

        //===================================================================================================
        /// <summary>
        /// Identifies, if there are no spacing or punktuation
        /// </summary>
        /// <param name="strName">The name</param>
        /// <returns>true iff it is a pure name</returns>
        //===================================================================================================
        bool IsSingleName(string strName)
        {
            if (strName.Length == 0 || !char.IsLetter(strName[0])) return false;
            for (int i = 1; i < strName.Length; i++)
            {
                var category = char.GetUnicodeCategory(strName[i]);
                if (!char.IsLetter(strName[i]) &&
                    category != System.Globalization.UnicodeCategory.NonSpacingMark &&
                    category != System.Globalization.UnicodeCategory.SpacingCombiningMark &&
                    category != System.Globalization.UnicodeCategory.EnclosingMark)
                    return false;
            }
            return true;
        }

        //===================================================================================================
        /// <summary>
        /// Tests if text is in last characters
        /// </summary>
        /// <param name="strText">Text with interesting last char</param>
        /// <param name="strCharacters">Last characters to search</param>
        /// <returns>true iff the last char of strText is found in strCharacters</returns>
        //===================================================================================================
        bool LastCharIn(string strText, string strCharacters)
        {
            return strText.Length > 0 && strCharacters.IndexOf(strText[strText.Length - 1]) >= 0;
        }
        #endregion

        #region CZECH

        //===================================================================================================
        /// <summary>
        /// Czech cases
        /// </summary>
        enum CzechCase { 
            Nominative, 
            Genitive, 
            Dative, 
            Accusative 
        }

        //===================================================================================================
        /// <summary>
        /// Declines a single name using common Czech spelling-based rules.
        /// Assumes Czech pronunciation. Foreign pronunciation and family usage may differ.
        /// Compound names are left unchanged because their parts need separate analysis.
        /// </summary>
        //===================================================================================================
        string DeclineCzechName(string strName, bool bIsMale, CzechCase eTargetCase)
        {
            if (string.IsNullOrWhiteSpace(strName) || eTargetCase == CzechCase.Nominative)
                return strName;

            string strNormalizedName = strName.Trim().Normalize(System.Text.NormalizationForm.FormC);

            if (strNormalizedName.Length < 2) return strName;

            if (!IsSingleName(strNormalizedName))
                return strName;

            var oCulture = System.Globalization.CultureInfo.GetCultureInfo("cs-CZ");

            string strLower = strNormalizedName.ToLower(oCulture);
            string strStem = strLower.Substring(0, strLower.Length - 1);
            string strResult = strLower;

            bool bDative = eTargetCase == CzechCase.Dative;
            bool bAccusative = eTargetCase == CzechCase.Accusative;

            if (eTargetCase != CzechCase.Genitive && !bDative && !bAccusative)
                return strName;

            if (bIsMale)
            {
                if (strLower.EndsWith("í", System.StringComparison.Ordinal))
                    strResult = strStem + (bDative ? "ímu" : "ího"); // Jiří
                else if (strLower.EndsWith("ý", System.StringComparison.Ordinal))
                    strResult = strStem + (bDative ? "ému" : "ého"); // Novotný
                else if (strLower.EndsWith("a", System.StringComparison.Ordinal))
                    strResult = strStem + (bAccusative ? "u" : bDative ? "ovi" : CzechGenitiveAEnding(strStem));
                else if (strLower.EndsWith("o", System.StringComparison.Ordinal))
                    strResult = strStem + (bDative ? "ovi" : "a"); // Radko
                else if (LastCharIn(strLower, "bcčdďfghjklmnňpqrřsštťvwxzž"))
                {
                    strStem = strLower;
                    // Stem changes cannot all be inferred from endings: Pavel loses e,
                    // whereas Daniel retains it. These are exceptions, not a name whitelist.
                    switch (strLower)
                    {
                        case "pavel": strStem = "pavl"; break;
                        case "karel": strStem = "karl"; break;
                        case "havel": strStem = "havl"; break;
                        case "aleš": strStem = "alš"; break;
                        default:
                            if (strLower.EndsWith("ěk", System.StringComparison.Ordinal))
                            {
                                strStem = strLower.Substring(0, strLower.Length - 2);
                                if (strStem.EndsWith("n", System.StringComparison.Ordinal))
                                    strStem = strStem.Substring(0, strStem.Length - 1) + "ň";
                                strStem += "k";
                            }
                            else if (strLower.EndsWith("ek", System.StringComparison.Ordinal) ||
                                     strLower.EndsWith("ec", System.StringComparison.Ordinal))
                                strStem = strLower.Substring(0, strLower.Length - 2) + strLower[strLower.Length - 1];
                            break;
                    }
                    bool soft = LastCharIn(strStem, "cčďjňřššťžszx");
                    strResult = strStem + (bDative ? "ovi" : soft ? "e" : "a");
                }
                // Other vowel endings require pronunciation-specific rules: unchanged.
            }
            else
            {
                if (strLower.EndsWith("á", System.StringComparison.Ordinal))
                    strResult = strStem + (bAccusative ? "ou" : bDative ? "é" : "é"); // Novotná
                else if (strLower.EndsWith("ia", System.StringComparison.Ordinal))
                    strResult = strStem + (bAccusative ? "u" : bDative ? "i" : "e"); // Maria
                else if (strLower.EndsWith("ja", System.StringComparison.Ordinal))
                    return strName; // Foreign -ja names have competing paradigms.
                else if (strLower.EndsWith("a", System.StringComparison.Ordinal))
                {
                    if (bAccusative) strResult = strStem + "u";
                    else if (!bDative)
                    {
                        // Táňa -> Táni: ň is represented by n before i.
                        if (strStem.EndsWith("ň", System.StringComparison.Ordinal))
                            strStem = strStem.Substring(0, strStem.Length - 1) + "n";
                        strResult = strStem + (strLower.EndsWith("ňa", System.StringComparison.Ordinal) ? "i" : 
                            CzechGenitiveAEnding(strStem));
                    }
                    else
                    {
                        if (strStem.EndsWith("ch", System.StringComparison.Ordinal))
                            strResult = strStem.Substring(0, strStem.Length - 2) + "še";
                        else
                        {
                            string prefix = strStem.Substring(0, strStem.Length - 1);
                            switch (strStem[strStem.Length - 1])
                            {
                                case 'k': strResult = prefix + "ce"; break;
                                case 'h': case 'g': strResult = prefix + "ze"; break;
                                case 'r': strResult = prefix + "ře"; break;
                                case 'č': case 'š': case 'ž': case 'c': strResult = strStem + "e"; break;
                                case 'ň': strResult = prefix + "ně"; break;
                                default: strResult = strStem + "ě"; break;
                            }
                        }
                    }
                }
                else if (strLower.EndsWith("ie", System.StringComparison.Ordinal))
                    strResult = bDative || bAccusative ? strStem + "i" : strLower; // Marie -> Marie / Marii / Marii
                else if (strLower.EndsWith("e", System.StringComparison.Ordinal))
                    strResult = bDative || bAccusative ? strStem + "i" : strLower; 
                // Alice, Růže; assumes Czech pronunciation
                // Female consonant endings and other vowel endings remain unchanged.
            }

            if (strNormalizedName == strNormalizedName.ToUpper(oCulture)) return strResult.ToUpper(oCulture);
            if (strNormalizedName == strNormalizedName.ToLower(oCulture)) return strResult;

            // Usual initial-capital spelling. Unusual internal capitalization is not inferred.
            return char.ToUpper(strResult[0], oCulture) + strResult.Substring(1);
        }

        //===================================================================================================
        /// <summary>
        /// Genitive ending for the common -a paradigm.
        /// </summary>
        //===================================================================================================
        string CzechGenitiveAEnding(string strStem)
        {
            return LastCharIn(strStem, "cčďjňřššťž") ? "i" : "y";
        }

        //===================================================================================================
        /// <summary>
        /// Adapts prepared Czech templates to the selected name and gender.
        /// </summary>
        //===================================================================================================
        void AdaptTextToGenderAndName_Czech(TextBox tbxToAdapt)
        {
            if (tbxToAdapt == null) throw new System.ArgumentNullException(nameof(tbxToAdapt));
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";
            bool bIsMale = m_rbMale.Checked;
            bool bIsFemale = m_rbFemale.Checked;
            string[] astrNameTokens = new string[] { "[Jméno]", "[Jméno:G]", "[Jméno:D]", "[Jméno:A]" };
            bool bContainsName = false;
            foreach (string strToken in astrNameTokens)
                if (strCurrentText.IndexOf(strToken, System.StringComparison.CurrentCultureIgnoreCase) >= 0)
                { bContainsName = true; break; }

            if (m_chkAddName.Checked && !bContainsName)
            {
                string? strToReplace = null;
                int nPosToReplace = -1;
                int nSelected = -1;
                string[] astrSearch = new string[] { " on/ona ", " jemu/jí ", " jeho/ji " };
                string[] astrReplacement = new string[] { " [Jméno] ", " [Jméno:D] ", " [Jméno:A] " };

                foreach (string strTextToSearch in astrSearch)
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, System.StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" On/ona ".Equals(strToReplace, System.StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                        nSelected = System.Array.IndexOf(astrSearch, strTextToSearch);
                    }
                }
                if (strToReplace != null)
                    strCurrentText = strCurrentText.Substring(0, nPosToReplace) + astrReplacement[nSelected] +
                        strCurrentText.Substring(nPosToReplace + strToReplace.Length);
            }
            // Only explicit alternatives referring to the selected person are resolved.
            // Reflexive possessives need agreement with the possessed noun instead.
            if (bIsMale || bIsFemale)
            {
                string[] astrAlternatives = new string[] { "on/ona", "jemu/jí", "jeho/ji", "jeho/její", "byl/byla", "udělal/udělala", "mohl/mohla", "chtěl/chtěla", "rád/ráda" };
                string[] astrGenderReplacements = new string[astrAlternatives.Length];
                for (int i = 0; i < astrAlternatives.Length; i++)
                    astrGenderReplacements[i] = astrAlternatives[i].Split('/')[bIsMale ? 0 : 1];
                strCurrentText = ReplaceGenderTokens(strCurrentText, astrAlternatives, astrGenderReplacements);
            }

            string strNameToUse = m_tbxName.Text;
            bool bNameMissing = string.IsNullOrWhiteSpace(strNameToUse);
            if (!bNameMissing) strNameToUse = strNameToUse.Trim();
            string[] astrNameReplacements = new string[astrNameTokens.Length];
            astrNameReplacements[0] = bNameMissing ? "[JMÉNO CHYBÍ]" : bIsMale || bIsFemale ? DeclineCzechName(strNameToUse, bIsMale, CzechCase.Nominative) : strNameToUse;
            astrNameReplacements[1] = bNameMissing ? "[JMÉNO CHYBÍ]" : bIsMale || bIsFemale ? DeclineCzechName(strNameToUse, bIsMale, CzechCase.Genitive) : strNameToUse;
            astrNameReplacements[2] = bNameMissing ? "[JMÉNO CHYBÍ]" : bIsMale || bIsFemale ? DeclineCzechName(strNameToUse, bIsMale, CzechCase.Dative) : strNameToUse;
            astrNameReplacements[3] = bNameMissing ? "[JMÉNO CHYBÍ]" : bIsMale || bIsFemale ? DeclineCzechName(strNameToUse, bIsMale, CzechCase.Accusative) : strNameToUse;

            // A single pass keeps user-entered names literal, including placeholder text.
            tbxToAdapt.Text = ReplaceTokens(strCurrentText, astrNameTokens, astrNameReplacements).Trim().Replace(" .", ".");
        }

        #endregion

        #region HUNGARIAN
        //===================================================================================================
        /// <summary>Cases used by the Hungarian template adapter.</summary>
        //===================================================================================================
        enum HungarianCase
        {
            Nominative,
            Genitive, // Unmarked possessor: [Név:G] könyve = Anna könyve.
            Dative,
            Accusative,
            Possessive // Independent ownership: [Név:P] = Annáé ("Anna's").
        }

        //===================================================================================================
        /// <summary>Declines a name using Hungarian suffix rules.</summary>
        /// <param name="strName">Personal name, including a full name if needed.</param>
        /// <param name="bIsMale">Retained for compatibility; Hungarian has no grammatical gender.</param>
        /// <param name="eTargetCase">Required case.</param>
        /// <returns>Inflected name. Pronunciation-dependent exceptions need explicit options.</returns>
        //===================================================================================================
        string DeclineHungarianName(string strName, bool bIsMale, HungarianCase eTargetCase)
        {
            return DeclineHungarianName(strName, bIsMale, eTargetCase, null, null, false);
        }

        //===================================================================================================
        /// <summary>
        /// Optional controls for names whose pronunciation or suffix class cannot be inferred.
        /// No fixed list of names is required.
        /// </summary>
        /// <param name="bUseBackVowels">null: infer; true: nak/ot; false: nek/et or öt.</param>
        /// <param name="strAccusativeOverride">Complete accusative form, or null for rules.</param>
        /// <param name="bHyphenate">Use for foreign names requiring a hyphen before suffixes.</param>
        //===================================================================================================
        string DeclineHungarianName(
            string strName, 
            bool bIsMale, 
            HungarianCase eTargetCase,
            bool? bUseBackVowels, 
            string? strAccusativeOverride, 
            bool bHyphenate
            )
        {
            if (string.IsNullOrWhiteSpace(strName)) return strName;
            if (eTargetCase == HungarianCase.Nominative || eTargetCase == HungarianCase.Genitive)
                return strName;
            if (eTargetCase == HungarianCase.Accusative && strAccusativeOverride != null)
                return strAccusativeOverride;
            if (eTargetCase != HungarianCase.Dative && eTargetCase != HungarianCase.Accusative &&
                eTargetCase != HungarianCase.Possessive) return strName;

            var culture = System.Globalization.CultureInfo.GetCultureInfo("hu-HU");
            string name = strName.Trim().Normalize(System.Text.NormalizationForm.FormC);
            // The suffix of a full name attaches to its final component.
            int componentStart = name.Length;
            while (componentStart > 0 && (char.IsLetter(name[componentStart - 1]) ||
                char.GetUnicodeCategory(name[componentStart - 1]) == System.Globalization.UnicodeCategory.NonSpacingMark))
                componentStart--;
            if (componentStart == name.Length) return strName;
            string component = name.Substring(componentStart);
            string part = component.ToLower(culture);
            const string vowels = "aáeéiíoóöőuúüű";
            bool back = false;
            bool roundedFront = false;
            foreach (char c in part)
            {
                if ("aáoóuú".IndexOf(c) >= 0) back = true;
                // Last non-neutral front vowel selects rounding for linking vowels.
                if ("eöőüű".IndexOf(c) >= 0) roundedFront = c != 'e';
            }
            if (bUseBackVowels.HasValue) back = bUseBackVowels.Value;

            // Before case suffixes, final a/e lengthen: Anna -> Anná-, Emese -> Emesé-.
            // With foreign silent endings this inference is disabled; use explicit options.
            string stem = name;
            if (!bHyphenate)
            {
                char last = name[name.Length - 1];
                if (last == 'a') stem = name.Substring(0, name.Length - 1) + "á";
                else if (last == 'e') stem = name.Substring(0, name.Length - 1) + "é";
                else if (last == 'A') stem = name.Substring(0, name.Length - 1) + "Á";
                else if (last == 'E') stem = name.Substring(0, name.Length - 1) + "É";
            }

            string suffix;
            switch (eTargetCase)
            {
                case HungarianCase.Dative:
                    suffix = back ? "nak" : "nek";
                    break;
                case HungarianCase.Possessive:
                    suffix = "é";
                    break;
                default:
                    // Final vowels take -t. Common single consonants l/r/n/j/s/z and
                    // digraphs ny/sz/zs take -t after a vowel. Other consonants and clusters
                    // normally need a linking vowel. Irregular stems need an override.
                    bool direct = vowels.IndexOf(part[part.Length - 1]) >= 0;
                    if (!direct)
                        direct = HungarianHasDirectAccusativeEnding(part);
                    suffix = direct ? "t" : back ? "ot" : roundedFront ? "öt" : "et";
                    break;
            }
            if (component == component.ToUpper(culture)) suffix = suffix.ToUpper(culture);
            return stem + (bHyphenate ? "-" : "") + suffix;
        }

        //===================================================================================================
        /// <summary>Adapts prepared Hungarian templates to the selected name and gender.</summary>
        /// <param name="tbxToAdapt">Textbox for adaptation of text.</param>
        //===================================================================================================
        void AdaptTextToGenderAndName_Hungarian(TextBox tbxToAdapt)
        {
            if (tbxToAdapt == null) throw new System.ArgumentNullException(nameof(tbxToAdapt));
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";

            bool bIsMale = m_rbMale.Checked;
            bool bIsFemale = m_rbFemale.Checked;

            string[] arrNameTokens = new string[] { "[Név]", "[Név:G]", "[Név:D]", "[Név:A]", "[Név:P]" };
            bool bContainsName = false;
            foreach (string token in arrNameTokens)
                if (strCurrentText.IndexOf(token, System.StringComparison.CurrentCultureIgnoreCase) >= 0)
                { bContainsName = true; break; }

            if (m_chkAddName.Checked && !bContainsName)
            {
                string? strToReplace = null;

                int nPosToReplace = -1;
                int nSelected = -1;

                string[] arrSearch = new string[] { " ő ", " neki ", " őt " };
                string[] arrReplacement = new string[] { " [Név] ", " [Név:D] ", " [Név:A] " };

                foreach (string strTextToSearch in arrSearch)
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, System.StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" ő ".Equals(strToReplace, System.StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                        nSelected = System.Array.IndexOf(arrSearch, strTextToSearch);
                    }
                }

                if (strToReplace != null)
                    strCurrentText = strCurrentText.Substring(0, nPosToReplace) + arrReplacement[nSelected] +
                        strCurrentText.Substring(nPosToReplace + strToReplace.Length);
            }

            // Only explicit alternatives referring to the selected person are resolved.
            // Reflexive possessives need agreement with the possessed noun instead.
            if (bIsMale || bIsFemale)
            {
                string[] arrAlternatives = new string[] { };
                string[] arrGenderReplacements = new string[arrAlternatives.Length];
                for (int i = 0; i < arrAlternatives.Length; i++)
                    arrGenderReplacements[i] = arrAlternatives[i].Split('/')[bIsMale ? 0 : 1];
                strCurrentText = ReplaceGenderTokens(strCurrentText, arrAlternatives, arrGenderReplacements);
            }

            string strNameToUse = m_tbxName.Text;
            bool bNameMissing = string.IsNullOrWhiteSpace(strNameToUse);

            if (!bNameMissing) 
                strNameToUse = strNameToUse.Trim();

            string[] astrNameReplacements = new string[arrNameTokens.Length];
            astrNameReplacements[0] = bNameMissing ? "[HIÁNYZÓ NÉV]" : DeclineHungarianName(strNameToUse, bIsMale, HungarianCase.Nominative);
            astrNameReplacements[1] = bNameMissing ? "[HIÁNYZÓ NÉV]" : DeclineHungarianName(strNameToUse, bIsMale, HungarianCase.Genitive);
            astrNameReplacements[2] = bNameMissing ? "[HIÁNYZÓ NÉV]" : DeclineHungarianName(strNameToUse, bIsMale, HungarianCase.Dative);
            astrNameReplacements[3] = bNameMissing ? "[HIÁNYZÓ NÉV]" : DeclineHungarianName(strNameToUse, bIsMale, HungarianCase.Accusative);
            astrNameReplacements[4] = bNameMissing ? "[HIÁNYZÓ NÉV]" : DeclineHungarianName(strNameToUse, bIsMale, HungarianCase.Possessive);

            // A single pass keeps user-entered names literal, including placeholder text.
            tbxToAdapt.Text = ReplaceTokens(strCurrentText, arrNameTokens, astrNameReplacements).Trim().Replace(" .", ".");
        }

        //===================================================================================================
        /// <summary>
        /// Provides indication, if direct accusative is possible
        /// </summary>
        /// <param name="strPart"></param>
        /// <returns>true iff direct accusative is possible</returns>
        //===================================================================================================
        bool HungarianHasDirectAccusativeEnding(string strPart)
        {
            const string vowels = "aáeéiíoóöőuúüű";
            foreach (string ending in new string[] { "ny", "sz", "zs", "l", "r", "n", "j", "s", "z" })
                if (strPart.EndsWith(ending, System.StringComparison.Ordinal) && strPart.Length > ending.Length &&
                    vowels.IndexOf(strPart[strPart.Length - ending.Length - 1]) >= 0) return true;
            return false;
        }

        #endregion

        #region UKRAINIAN
        //===================================================================================================
        /// <summary>Ukrainian cases.</summary>
        //===================================================================================================
        enum UkrainianCase
        {
            Nominative,
            Genitive,
            Dative,
            Accusative
        }

        //===================================================================================================
        /// <summary>
        /// Declines a single personal name using common Ukrainian rules.
        /// Assumes Ukrainian spelling and pronunciation. Compound names remain unchanged.
        /// This is a rule-based approximation, not a complete morphological dictionary.
        /// </summary>
        /// <param name="strName">Name to decline.</param>
        /// <param name="bIsMale">Whether this is a male name.</param>
        /// <param name="eTargetCase">Required case.</param>
        /// <returns>Declined name, or the original for unsupported endings.</returns>
        //===================================================================================================
        string DeclineUkrainianName(string strName, bool bIsMale, UkrainianCase eTargetCase)
        {
            if (string.IsNullOrWhiteSpace(strName) || eTargetCase == UkrainianCase.Nominative)
                return strName;

            if (eTargetCase != UkrainianCase.Genitive && eTargetCase != UkrainianCase.Dative &&
                eTargetCase != UkrainianCase.Accusative) return strName;

            var oCulture = System.Globalization.CultureInfo.GetCultureInfo("uk-UA");

            string strNormalizedName = strName.Trim().Normalize(System.Text.NormalizationForm.FormC);
            if (strNormalizedName.Length < 2 || !UkrainianIsCyrillicName(strNormalizedName))
                return strName;

            string strLower = strNormalizedName.ToLower(oCulture);
            string strStem = strLower.Substring(0, strLower.Length - 1);
            string strResult = strLower;

            bool bDative = eTargetCase == UkrainianCase.Dative;
            bool bAccusative = eTargetCase == UkrainianCase.Accusative;

            if (bIsMale && strLower.EndsWith("ий", System.StringComparison.Ordinal))
                strResult = strLower.Substring(0, strLower.Length - 2) + (bDative ? "ому" : "ого");

            else if (!bIsMale && (strLower.EndsWith("ова", System.StringComparison.Ordinal) || strLower.EndsWith("ева", System.StringComparison.Ordinal) || strLower.EndsWith("єва", System.StringComparison.Ordinal) || strLower.EndsWith("ська", System.StringComparison.Ordinal) || strLower.EndsWith("цька", System.StringComparison.Ordinal) || strLower.EndsWith("зька", System.StringComparison.Ordinal)))
                strResult = strStem + (bAccusative ? "у" : bDative ? "ій" : "ої");

            else if (strLower.EndsWith("а", System.StringComparison.Ordinal))
            {
                if (bAccusative) strResult = strStem + "у";
                else if (!bDative)
                    strResult = strStem + (LastCharIn(strStem, "жчшщ") ? "і" : "и");
                else
                {
                    // г/ґ, к, х alternate before і: Ольга -> Ользі, Оксанка -> Оксанці.
                    string prefix = strStem.Substring(0, strStem.Length - 1);
                    switch (strStem[strStem.Length - 1])
                    {
                        case 'г': case 'ґ': strResult = prefix + "зі"; break;
                        case 'к': strResult = prefix + "ці"; break;
                        case 'х': strResult = prefix + "сі"; break;
                        default: strResult = strStem + "і"; break;
                    }
                }
            }
            else if (strLower.EndsWith("я", System.StringComparison.Ordinal))
            {
                bool afterVowelOrApostrophe = LastCharIn(strStem, "аеєиіїоуюя'’ʼ");
                strResult = strStem + (bAccusative ? "ю" : afterVowelOrApostrophe ? "ї" : "і");
            }
            else if (bIsMale)
            {
                if (strLower.EndsWith("й", System.StringComparison.Ordinal))
                    strResult = strStem + (bDative ? "ю" : "я");
                else if (strLower.EndsWith("ь", System.StringComparison.Ordinal))
                    strResult = strStem + (bDative ? "ю" : "я");
                else if (strLower.EndsWith("о", System.StringComparison.Ordinal))
                    strResult = strStem + (bDative ? "у" : "а");
                else if (LastCharIn(strLower, "бвгґджзклмнпрстфхцчшщ"))
                {
                    strStem = strLower;
                    // A few lexical stem exceptions; ordinary names use the rules above/below.
                    // There is no whitelist restricting which names may be entered.
                    switch (strLower)
                    {
                        case "лев": strStem = "льв"; break;
                        case "федір": strStem = "федор"; break;
                        case "антін": strStem = "антон"; break;
                        case "тиміш": strStem = "тимош"; break;
                    }
                    bool softR = strLower == "ігор" || strLower == "лазар";
                    strResult = strStem + (bDative ? softR ? "ю" : "у" : softR ? "я" : "а");
                }
                // Other vowel endings need origin/pronunciation information: unchanged.
            }
            else
            {
                // Most female consonant-ending foreign names are indeclinable.
                // Любов is a Ukrainian third-declension exception; accusative is unchanged.
                if (strLower == "любов") strResult = bAccusative ? strLower : strLower + "і";
                else if (strLower.EndsWith("ь", System.StringComparison.Ordinal))
                    strResult = bAccusative ? strLower : strStem + "і";
            }

            if (strNormalizedName == strNormalizedName.ToUpper(oCulture)) return strResult.ToUpper(oCulture);
            if (strNormalizedName == strNormalizedName.ToLower(oCulture)) return strResult;

            return char.ToUpper(strResult[0], oCulture) + strResult.Substring(1);
        }

        //===================================================================================================
        /// <summary>Adapts prepared Ukrainian templates to the selected name and gender.</summary>
        /// <param name="tbxToAdapt">Textbox for adaptation of text.</param>
        //===================================================================================================
        void AdaptTextToGenderAndName_Ukrainian(TextBox tbxToAdapt)
        {
            if (tbxToAdapt == null) throw new System.ArgumentNullException(nameof(tbxToAdapt));

            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";

            bool bIsMale = m_rbMale.Checked;
            bool bIsFemale = m_rbFemale.Checked;

            string[] arrNameTokens = new string[] { "[Ім’я]", "[Ім’я:G]", "[Ім’я:D]", "[Ім’я:A]", "[Ім'я]", "[Ім'я:G]", "[Ім'я:D]", "[Ім'я:A]", "[Імʼя]", "[Імʼя:G]", "[Імʼя:D]", "[Імʼя:A]" };

            bool bContainsName = false;
            foreach (string token in arrNameTokens)
                if (strCurrentText.IndexOf(token, System.StringComparison.CurrentCultureIgnoreCase) >= 0)
                { bContainsName = true; break; }

            if (m_chkAddName.Checked && !bContainsName)
            {
                string? strToReplace = null;
                int nPosToReplace = -1;
                int nSelected = -1;
                string[] arrSearch = new string[] { " він/вона ", " йому/їй " };
                string[] arrReplacement = new string[] { " [Ім’я] ", " [Ім’я:D] " };

                foreach (string strTextToSearch in arrSearch)
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, System.StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" Він/вона ".Equals(strToReplace, System.StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                        nSelected = System.Array.IndexOf(arrSearch, strTextToSearch);
                    }
                }

                if (strToReplace != null)
                    strCurrentText = strCurrentText.Substring(0, nPosToReplace) + arrReplacement[nSelected] +
                        strCurrentText.Substring(nPosToReplace + strToReplace.Length);
            }

            // Only explicit alternatives referring to the selected person are resolved.
            // Reflexive possessives need agreement with the possessed noun instead.
            if (bIsMale || bIsFemale)
            {
                string[] arrAlternatives = new string[] { "він/вона", "йому/їй", "його/її", "був/була", "зробив/зробила", "хотів/хотіла", "міг/могла", "готовий/готова", "радий/рада" };
                string[] arrGenderReplacements = new string[arrAlternatives.Length];
                for (int i = 0; i < arrAlternatives.Length; i++)
                    arrGenderReplacements[i] = arrAlternatives[i].Split('/')[bIsMale ? 0 : 1];
                strCurrentText = ReplaceGenderTokens(strCurrentText, arrAlternatives, arrGenderReplacements);
            }

            string strNameToUse = m_tbxName.Text;
            bool bNameMissing = string.IsNullOrWhiteSpace(strNameToUse);

            if (!bNameMissing) 
                strNameToUse = strNameToUse.Trim();

            string[] astrNameReplacements = new string[arrNameTokens.Length];
            astrNameReplacements[0] = bNameMissing ? "[ІМ’Я ВІДСУТНЄ]" : bIsMale || bIsFemale ? DeclineUkrainianName(strNameToUse, bIsMale, UkrainianCase.Nominative) : strNameToUse;
            astrNameReplacements[1] = bNameMissing ? "[ІМ’Я ВІДСУТНЄ]" : bIsMale || bIsFemale ? DeclineUkrainianName(strNameToUse, bIsMale, UkrainianCase.Genitive) : strNameToUse;
            astrNameReplacements[2] = bNameMissing ? "[ІМ’Я ВІДСУТНЄ]" : bIsMale || bIsFemale ? DeclineUkrainianName(strNameToUse, bIsMale, UkrainianCase.Dative) : strNameToUse;
            astrNameReplacements[3] = bNameMissing ? "[ІМ’Я ВІДСУТНЄ]" : bIsMale || bIsFemale ? DeclineUkrainianName(strNameToUse, bIsMale, UkrainianCase.Accusative) : strNameToUse;
            astrNameReplacements[4] = bNameMissing ? "[ІМ’Я ВІДСУТНЄ]" : bIsMale || bIsFemale ? DeclineUkrainianName(strNameToUse, bIsMale, UkrainianCase.Nominative) : strNameToUse;
            astrNameReplacements[5] = bNameMissing ? "[ІМ’Я ВІДСУТНЄ]" : bIsMale || bIsFemale ? DeclineUkrainianName(strNameToUse, bIsMale, UkrainianCase.Genitive) : strNameToUse;
            astrNameReplacements[6] = bNameMissing ? "[ІМ’Я ВІДСУТНЄ]" : bIsMale || bIsFemale ? DeclineUkrainianName(strNameToUse, bIsMale, UkrainianCase.Dative) : strNameToUse;
            astrNameReplacements[7] = bNameMissing ? "[ІМ’Я ВІДСУТНЄ]" : bIsMale || bIsFemale ? DeclineUkrainianName(strNameToUse, bIsMale, UkrainianCase.Accusative) : strNameToUse;
            astrNameReplacements[8] = bNameMissing ? "[ІМ’Я ВІДСУТНЄ]" : bIsMale || bIsFemale ? DeclineUkrainianName(strNameToUse, bIsMale, UkrainianCase.Nominative) : strNameToUse;
            astrNameReplacements[9] = bNameMissing ? "[ІМ’Я ВІДСУТНЄ]" : bIsMale || bIsFemale ? DeclineUkrainianName(strNameToUse, bIsMale, UkrainianCase.Genitive) : strNameToUse;
            astrNameReplacements[10] = bNameMissing ? "[ІМ’Я ВІДСУТНЄ]" : bIsMale || bIsFemale ? DeclineUkrainianName(strNameToUse, bIsMale, UkrainianCase.Dative) : strNameToUse;
            astrNameReplacements[11] = bNameMissing ? "[ІМ’Я ВІДСУТНЄ]" : bIsMale || bIsFemale ? DeclineUkrainianName(strNameToUse, bIsMale, UkrainianCase.Accusative) : strNameToUse;

            // A single pass keeps user-entered names literal, including placeholder text.
            tbxToAdapt.Text = ReplaceTokens(strCurrentText, arrNameTokens, astrNameReplacements).Trim().Replace(" .", ".");
        }

        //===================================================================================================
        /// <summary>
        /// Decides if it is possibly an ukrainian name
        /// </summary>
        /// <param name="strName">String to test</param>
        /// <returns>true iff it can be ukrainian name</returns>
        //===================================================================================================
        bool UkrainianIsCyrillicName(string strName)
        {
            const string letters = "АБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЬЮЯабвгґдеєжзиіїйклмнопрстуфхцчшщьюя";
            for (int i = 0; i < strName.Length; i++)
            {
                if (letters.IndexOf(strName[i]) >= 0)
                {
                    continue;
                }
                if ((strName[i] == '\'' || strName[i] == '’' || strName[i] == 'ʼ') &&
                    i > 0 &&
                    i + 1 < strName.Length &&
                    letters.IndexOf(strName[i - 1]) >= 0 &&
                    letters.IndexOf(strName[i + 1]) >= 0)
                {
                    continue;
                }
                return false;
            }
            return true;
        }




        #endregion

        #region SLOVAK
        //===================================================================================================
        /// <summary>Slovak cases corresponding to the Russian implementation.</summary>
        //===================================================================================================
        enum SlovakCase
        {
            Nominative,
            Genitive,
            Dative,
            Accusative
        }

        //===================================================================================================
        /// <summary>
        /// Declines a single personal name using common Slovak rules.
        /// Assumes Slovak spelling and pronunciation. Compound names and unsupported
        /// endings remain unchanged. Foreign names may require an explicit override.
        /// </summary>
        /// <param name="strName">Name for declension.</param>
        /// <param name="bIsMale">Indication whether this is a male name.</param>
        /// <param name="eTargetCase">Required case.</param>
        /// <returns>Declined name, or original text for unsupported forms.</returns>
        //===================================================================================================
        string DeclineSlovakName(string strName, bool bIsMale, SlovakCase eTargetCase)
        {
            if (string.IsNullOrWhiteSpace(strName) || eTargetCase == SlovakCase.Nominative)
                return strName;
            if (eTargetCase != SlovakCase.Genitive && eTargetCase != SlovakCase.Dative &&
                eTargetCase != SlovakCase.Accusative) return strName;

            var culture = System.Globalization.CultureInfo.GetCultureInfo("sk-SK");
            string name = strName.Trim().Normalize(System.Text.NormalizationForm.FormC);
            if (name.Length < 2 || !IsSingleName(name)) return strName;

            string lower = name.ToLower(culture);
            string stem = lower.Substring(0, lower.Length - 1);
            string result = lower;
            bool dat = eTargetCase == SlovakCase.Dative;
            bool acc = eTargetCase == SlovakCase.Accusative;

            if (bIsMale)
            {
                if (lower.EndsWith("ý", System.StringComparison.Ordinal))
                    result = stem + (dat ? "ému" : "ého");
                else if (lower.EndsWith("a", System.StringComparison.Ordinal))
                    result = stem + (dat ? "ovi" : "u"); // Nikita -> Nikitu / Nikitovi / Nikitu
                else if (lower.EndsWith("o", System.StringComparison.Ordinal))
                    result = stem + (dat ? "ovi" : "a"); // Janko -> Janka / Jankovi / Janka
                else if (LastCharIn(lower, "bcčdďfghjklĺľmnňpqrŕsštťvwxzž"))
                {
                    stem = lower;
                    // These stem changes are lexical exceptions, not a whitelist of names.
                    // For instance Peter loses e, whereas Walter normally retains it.
                    switch (lower)
                    {
                        case "peter": stem = "petr"; break;
                        case "pavol": stem = "pavl"; break;
                        case "pavel": stem = "pavl"; break;
                        default:
                            // Common Slovak moving-e patterns: Marek -> Mark-, Nemec -> Nemc-.
                            // Foreign names with the same spelling may retain the vowel.
                            if (lower.EndsWith("ek", System.StringComparison.Ordinal) ||
                                lower.EndsWith("ec", System.StringComparison.Ordinal))
                                stem = lower.Substring(0, lower.Length - 2) + lower[lower.Length - 1];
                            break;
                    }
                    result = stem + (dat ? "ovi" : "a");
                }
                // Other vowel endings require additional pronunciation/paradigm information.
            }
            else
            {
                if (lower.EndsWith("á", System.StringComparison.Ordinal))
                    result = stem + (acc ? "ú" : "ej"); // Nováková -> Novákovej / Novákovú
                else if (lower.EndsWith("ia", System.StringComparison.Ordinal))
                    result = stem + (acc ? "u" : dat ? "i" : "e"); // Mária, Lucia
                else if (lower.EndsWith("a", System.StringComparison.Ordinal))
                {
                    if (acc) result = stem + "u";
                    else if (LastCharIn(stem, "aeiouyáäéíóôúýä"))
                        result = stem + (dat ? "i" : "y"); // Andrea -> Andrey / Andrei
                    else if (LastCharIn(stem, "cčďjľňšžť"))
                    {
                        // Soft pattern ulica: Soňa -> Sone / Soni / Soňu.
                        string softStem = stem;
                        if (stem.EndsWith("ň", System.StringComparison.Ordinal))
                            softStem = stem.Substring(0, stem.Length - 1) + "n";
                        else if (stem.EndsWith("ď", System.StringComparison.Ordinal))
                            softStem = stem.Substring(0, stem.Length - 1) + "d";
                        else if (stem.EndsWith("ť", System.StringComparison.Ordinal))
                            softStem = stem.Substring(0, stem.Length - 1) + "t";
                        else if (stem.EndsWith("ľ", System.StringComparison.Ordinal))
                            softStem = stem.Substring(0, stem.Length - 1) + "l";
                        result = softStem + (dat ? "i" : "e");
                    }
                    else
                        result = stem + (dat ? "e" : "y"); // Anna, Petra, Lenka, Oľga
                }
                // Most foreign female names ending in a consonant or another vowel
                // are indeclinable. Exceptional paradigms are not inferred here.
            }

            if (name == name.ToUpper(culture)) return result.ToUpper(culture);
            if (name == name.ToLower(culture)) return result;
            return char.ToUpper(result[0], culture) + result.Substring(1);
        }

        //===================================================================================================
        /// <summary>
        /// Adapts prepared Slovak templates to the selected name and gender.
        /// </summary>
        /// <param name="tbxToAdapt">Textbox for adaptation of text.</param>
        //===================================================================================================
        void AdaptTextToGenderAndName_Slovak(TextBox tbxToAdapt)
        {
            if (tbxToAdapt == null) throw new System.ArgumentNullException(nameof(tbxToAdapt));
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";

            bool bIsMale = m_rbMale.Checked;
            bool bIsFemale = m_rbFemale.Checked;

            string[] astrNameTokens = new string[] { "[Meno]", "[Meno:G]", "[Meno:D]", "[Meno:A]" };

            bool bContainsName = false;

            foreach (string strToken in astrNameTokens)
                if (strCurrentText.IndexOf(strToken, System.StringComparison.CurrentCultureIgnoreCase) >= 0)
                { bContainsName = true; break; }

            if (m_chkAddName.Checked && !bContainsName)
            {
                string? strToReplace = null;

                int nPosToReplace = -1;
                int nSelected = -1;

                string[] arrSearch = new string[] { " on/ona ", " jemu/jej ", " jeho/ju " };
                string[] arrReplacement = new string[] { " [Meno] ", " [Meno:D] ", " [Meno:A] " };

                foreach (string strTextToSearch in arrSearch)
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, System.StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" On/ona ".Equals(strToReplace, System.StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                        nSelected = System.Array.IndexOf(arrSearch, strTextToSearch);
                    }
                }

                if (strToReplace != null)
                    strCurrentText = strCurrentText.Substring(0, nPosToReplace) + arrReplacement[nSelected] +
                        strCurrentText.Substring(nPosToReplace + strToReplace.Length);
            }

            // Only explicit alternatives referring to the selected person are resolved.
            // Reflexive possessives need agreement with the possessed noun instead.
            if (bIsMale || bIsFemale)
            {
                string[] astrAlternatives = new string[] { "on/ona", "jemu/jej", "jeho/ju", "jeho/jej", "bol/bola", "urobil/urobila", "chcel/chcela", "mohol/mohla", "rád/rada", "pripravený/pripravená" };
                string[] arrGenderReplacements = new string[astrAlternatives.Length];

                for (int i = 0; i < astrAlternatives.Length; i++)
                    arrGenderReplacements[i] = astrAlternatives[i].Split('/')[bIsMale ? 0 : 1];

                strCurrentText = ReplaceGenderTokens(strCurrentText, astrAlternatives, arrGenderReplacements);
            }

            string strNameToUse = m_tbxName.Text;
            bool bNameMissing = string.IsNullOrWhiteSpace(strNameToUse);

            if (!bNameMissing) 
                strNameToUse = strNameToUse.Trim();

            string[] astrNameReplacements = new string[astrNameTokens.Length];
            astrNameReplacements[0] = bNameMissing ? "[MENO CHÝBA]" : bIsMale || bIsFemale ? DeclineSlovakName(strNameToUse, bIsMale, SlovakCase.Nominative) : strNameToUse;
            astrNameReplacements[1] = bNameMissing ? "[MENO CHÝBA]" : bIsMale || bIsFemale ? DeclineSlovakName(strNameToUse, bIsMale, SlovakCase.Genitive) : strNameToUse;
            astrNameReplacements[2] = bNameMissing ? "[MENO CHÝBA]" : bIsMale || bIsFemale ? DeclineSlovakName(strNameToUse, bIsMale, SlovakCase.Dative) : strNameToUse;
            astrNameReplacements[3] = bNameMissing ? "[MENO CHÝBA]" : bIsMale || bIsFemale ? DeclineSlovakName(strNameToUse, bIsMale, SlovakCase.Accusative) : strNameToUse;

            // A single pass keeps user-entered names literal, including placeholder text.
            tbxToAdapt.Text = ReplaceTokens(strCurrentText, astrNameTokens, astrNameReplacements).Trim().Replace(" .", ".");
        }


        #endregion

        #region SLOVENIAN
        //===================================================================================================
        /// <summary>
        /// Slovenian cases corresponding to the Russian implementation.
        /// </summary>
        enum SlovenianCase
        {
            Nominative,
            Genitive,
            Dative,
            Accusative
        }

        //===================================================================================================
        /// <summary>
        /// Declines a single given name using common Slovenian rules.
        /// Assumes Slovenian pronunciation. Compound names remain unchanged.
        /// </summary>
        /// <param name="strName">Name for declension.</param>
        /// <param name="bIsMale">Indicates whether this is a male name.</param>
        /// <param name="eTargetCase">Required case.</param>
        /// <returns>Declined name; unsupported forms remain unchanged.</returns>
        //===================================================================================================
        string DeclineSlovenianName(string strName, bool bIsMale, SlovenianCase eTargetCase)
        {
            return DeclineSlovenianName(strName, bIsMale, eTargetCase, null);
        }

        //===================================================================================================
        /// <summary>
        /// Optional oblique stem for pronunciation-dependent or irregular names.
        /// Example: supply "Juret" for Jure or "Petr" for Peter.
        /// A stem override is combined with male -a/-u or female -e/-i/-o endings.
        /// </summary>
        //===================================================================================================
        string DeclineSlovenianName(
            string strName, 
            bool bIsMale, 
            SlovenianCase eTargetCase,
            string? strObliqueStem
            )
        {
            if (string.IsNullOrWhiteSpace(strName) || eTargetCase == SlovenianCase.Nominative)
                return strName;

            if (eTargetCase != SlovenianCase.Genitive && eTargetCase != SlovenianCase.Dative &&
                eTargetCase != SlovenianCase.Accusative)
            {
                return strName;
            }

            var oCulture = System.Globalization.CultureInfo.GetCultureInfo("sl-SI");
            string strNormalizedName = strName.Trim().Normalize(System.Text.NormalizationForm.FormC);

            if (strNormalizedName.Length < 2 || !IsSingleName(strNormalizedName)) 
                return strName;

            string strLower = strNormalizedName.ToLower(oCulture);
            string strStem = strLower.Substring(0, strLower.Length - 1);
            string strResult;

            bool bDative = eTargetCase == SlovenianCase.Dative;
            bool bAccusative = eTargetCase == SlovenianCase.Accusative;

            if (strObliqueStem != null)
            {
                if (string.IsNullOrWhiteSpace(strObliqueStem))
                    throw new System.ArgumentException("The oblique stem must not be empty.", nameof(strObliqueStem));

                strStem = strObliqueStem.Normalize(System.Text.NormalizationForm.FormC).ToLower(oCulture);
                strResult = strStem + (bIsMale ? (bDative ? "u" : "a") : (bDative ? "i" : bAccusative ? "o" : "e"));
            }
            else if (strLower.EndsWith("a", System.StringComparison.Ordinal))
            {
                // Female names and the standard second masculine paradigm:
                // Ana -> Ane / Ani / Ano; Luka -> Luke / Luki / Luko.
                strResult = strStem + (bDative ? "i" : bAccusative ? "o" : "e");
            }
            else if (!bIsMale)
            {
                // Female names such as Karmen, Ines, Nives and Beti have zero endings.
                return strName;
            }
            else
            {
                strStem = strLower;
                if (strLower.EndsWith("o", System.StringComparison.Ordinal))
                    strStem = strLower.Substring(0, strLower.Length - 1); // Marko -> Mark-
                else if (strLower.EndsWith("i", System.StringComparison.Ordinal) ||
                         strLower.EndsWith("y", System.StringComparison.Ordinal) ||
                         strLower.EndsWith("u", System.StringComparison.Ordinal))
                    strStem = strLower + "j"; // Jani -> Janij-, Tony -> Tonyj-
                else if (strLower.EndsWith("e", System.StringComparison.Ordinal))
                {
                    // Slovenian hypocoristics commonly extend with t, foreign -e names
                    // often with j. Spelling alone cannot distinguish these classes.
                    switch (strLower)
                    {
                        case "jure":
                        case "jože":
                        case "tone":
                        case "france":
                        case "ante":
                        case "mile":
                            strStem = strLower + "t";
                            break;
                        default: strStem = strLower + "j"; break;
                    }
                }
                else if (LastCharIn(strLower, "bcčdfghjklmnprsštvwxzž"))
                {
                    // A few lexical stem exceptions, not a whitelist of allowed names.
                    switch (strLower)
                    {
                        case "peter": strStem = "petr"; break;
                        case "aleksander": strStem = "aleksandr"; break;
                        case "domen": strStem = "domn"; break;
                        default:
                            // Common moving-e patterns. Foreign names can retain e.
                            if (strLower.EndsWith("ek", System.StringComparison.Ordinal) ||
                                strLower.EndsWith("ec", System.StringComparison.Ordinal))
                            {
                                strStem = strLower.Substring(0, strLower.Length - 2) + strLower[strLower.Length - 1];
                            }
                            // Most vowel + r names extend with j: Igor -> Igorj-.
                            // Bor, for example, does not: a lexical exception.
                            else if ((strLower.EndsWith("r", System.StringComparison.Ordinal) && strLower.Length >= 2 && "aeiou".IndexOf(strLower[strLower.Length - 2]) >= 0) &&
                                     strLower != "bor")
                            {
                                strStem = strLower + "j";
                            }
                            break;
                    }
                }
                else
                {
                    return strName; // Unsupported endings need a supplied oblique stem.
                }

                // In this masculine paradigm, animate accusative equals genitive.
                strResult = strStem + (bDative ? "u" : "a");
            }

            if (strNormalizedName == strNormalizedName.ToUpper(oCulture)) 
                return strResult.ToUpper(oCulture);

            if (strNormalizedName == strNormalizedName.ToLower(oCulture))
                return strResult;

            return char.ToUpper(strResult[0], oCulture) + strResult.Substring(1);
        }

        //===================================================================================================
        /// <summary>
        /// Adapts prepared Slovenian templates to the selected name and gender.
        /// </summary>
        /// <param name="tbxToAdapt">Textbox for adaptation of text.</param>
        //===================================================================================================
        void AdaptTextToGenderAndName_Slovenian(TextBox tbxToAdapt)
        {
            if (tbxToAdapt == null) 
                throw new System.ArgumentNullException(nameof(tbxToAdapt));

            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";

            bool bIsMale = m_rbMale.Checked;
            bool bIsFemale = m_rbFemale.Checked;

            string[] astrNameTokens = new string[] { "[Ime]", "[Ime:G]", "[Ime:D]", "[Ime:A]" };

            bool bContainsName = false;
            foreach (string strToken in astrNameTokens)
            {
                if (strCurrentText.IndexOf(strToken, System.StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    bContainsName = true;
                    break;
                }
            }

            if (m_chkAddName.Checked && !bContainsName)
            {
                string? strToReplace = null;

                int nPosToReplace = -1;
                int nSelected = -1;

                string[] astrSearch = new string[] { " on/ona ", " njemu/njej ", " njega/njo " };
                string[] astrReplacement = new string[] { " [Ime] ", " [Ime:D] ", " [Ime:A] " };

                foreach (string strTextToSearch in astrSearch)
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, System.StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" On/ona ".Equals(strToReplace, System.StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                        nSelected = System.Array.IndexOf(astrSearch, strTextToSearch);
                    }
                }

                if (strToReplace != null)
                    strCurrentText = strCurrentText.Substring(0, nPosToReplace) + astrReplacement[nSelected] +
                        strCurrentText.Substring(nPosToReplace + strToReplace.Length);
            }

            // Only explicit alternatives referring to the selected person are resolved.
            // Reflexive possessives need agreement with the possessed noun instead.
            if (bIsMale || bIsFemale)
            {
                string[] astrAlternatives = new string[] { "on/ona", "njemu/njej", "njega/njo", "njegov/njen", "njegova/njena", "njegovo/njeno", "bil/bila", "naredil/naredila", "želel/želela", "mogel/mogla", "rad/rada", "pripravljen/pripravljena" };
                string[] astrGenderReplacements = new string[astrAlternatives.Length];

                for (int i = 0; i < astrAlternatives.Length; i++)
                    astrGenderReplacements[i] = astrAlternatives[i].Split('/')[bIsMale ? 0 : 1];

                strCurrentText = ReplaceGenderTokens(strCurrentText, astrAlternatives, astrGenderReplacements);
            }

            string strNameToUse = m_tbxName.Text;
            bool bNameMissing = string.IsNullOrWhiteSpace(strNameToUse);

            if (!bNameMissing) 
                strNameToUse = strNameToUse.Trim();

            string[] astrNameReplacements = new string[astrNameTokens.Length];
            astrNameReplacements[0] = bNameMissing ? "[IME MANJKA]" : bIsMale || bIsFemale ? DeclineSlovenianName(strNameToUse, bIsMale, SlovenianCase.Nominative) : strNameToUse;
            astrNameReplacements[1] = bNameMissing ? "[IME MANJKA]" : bIsMale || bIsFemale ? DeclineSlovenianName(strNameToUse, bIsMale, SlovenianCase.Genitive) : strNameToUse;
            astrNameReplacements[2] = bNameMissing ? "[IME MANJKA]" : bIsMale || bIsFemale ? DeclineSlovenianName(strNameToUse, bIsMale, SlovenianCase.Dative) : strNameToUse;
            astrNameReplacements[3] = bNameMissing ? "[IME MANJKA]" : bIsMale || bIsFemale ? DeclineSlovenianName(strNameToUse, bIsMale, SlovenianCase.Accusative) : strNameToUse;

            // A single pass keeps user-entered names literal, including placeholder text.
            tbxToAdapt.Text = ReplaceTokens(strCurrentText, astrNameTokens, astrNameReplacements).Trim().Replace(" .", ".");
        }

        #endregion

        #region BULGARIAN
        //===================================================================================================
        /// <summary>
        /// Semantic cases 
        /// </summary>
        //===================================================================================================
        enum BulgarianCase
        {
            Nominative,
            Genitive,
            Dative,
            Accusative
        }

        //===================================================================================================
        /// <summary>
        /// Formats a Bulgarian personal name for the requested relationship.
        /// Modern Bulgarian normally keeps the name unchanged. Genitive and dative
        /// relationships use на + name. No vocative is generated by this method.
        /// </summary>
        /// <param name="strName">Name to use.</param>
        /// <param name="bIsMale">Retained for compatibility; these name forms do not depend on gender.</param>
        /// <param name="eTargetCase">Required semantic case.</param>
        /// <returns>Name, or на followed by the name.</returns>
        //===================================================================================================
        string DeclineBulgarianName(string strName, bool bIsMale, BulgarianCase eTargetCase)
        {
            if (string.IsNullOrWhiteSpace(strName))
                return strName;

            switch (eTargetCase)
            {
                case BulgarianCase.Genitive:
                case BulgarianCase.Dative:
                    return "на " + strName;
                case BulgarianCase.Accusative:
                case BulgarianCase.Nominative:
                default:
                    return strName;
            }
        }

        //===================================================================================================
        /// <summary>
        /// Adapts Bulgarian templates using the search and replacement logic of the
        /// Russian original, including the nominative guard with ONE leading space.
        /// </summary>
        /// <param name="tbxToAdapt">Textbox for adaptation of text.</param>
        //===================================================================================================
        void AdaptTextToGenderAndName_Bulgarian(TextBox tbxToAdapt)
        {
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";
            if (m_chkAddName.Checked && !strCurrentText.Contains("[Име"))
            {
                string? strToReplace = null;
                int nPosToReplace = -1;

                // Use full pronouns for automatic name insertion. Short му/ѝ can also
                // express possession, so replacing them by a name would be ambiguous.
                // The complete dative expression includes на, preventing на на Иван.
                foreach (string strTextToSearch in new string[]
                    { " Той/тя ", " На него/нея ", " На него/на нея ", " Него/нея " })
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" Той/тя ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                    }
                }

                if (strToReplace != null)
                {
                    if (" На него/нея ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase) ||
                        " На него/на нея ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " [Име:D] " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else if (" Него/нея ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " [Име:A] " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " [Име] " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                }
            }

            string strNameToUse = m_tbxName.Text;
            if (string.IsNullOrWhiteSpace(strNameToUse))
                strNameToUse = "ЛИПСВА ИМЕ";

            // Like the original, template alternatives are space-delimited and use the
            // listed lowercase or initial-capital spelling. Periods are separated above.
            // Other punctuation must be separated from alternatives in the template.
            // Possessives stay possessives: негов/неин is not replaced by на + name in
            // the same position. For names use e.g. "книгата [Име:G]" instead.
            // Reflexive свой/своя/свое/свои agree with the possessed noun, not its owner.
            if (m_rbMale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" той/тя ", " той ")
                    .Replace(" Той/тя ", " Той ")
                    .Replace(" на него/нея ", " на него ")
                    .Replace(" На него/нея ", " На него ")
                    .Replace(" на него/на нея ", " на него ")
                    .Replace(" На него/на нея ", " На него ")
                    .Replace(" него/нея ", " него ")
                    .Replace(" Него/нея ", " Него ")
                    .Replace(" го/я ", " го ")
                    .Replace(" Го/я ", " Го ")
                    .Replace(" му/ѝ ", " му ")
                    .Replace(" Му/ѝ ", " Му ")
                    .Replace(" негов/неин ", " негов ")
                    .Replace(" Негов/неин ", " Негов ")
                    .Replace(" негова/нейна ", " негова ")
                    .Replace(" Негова/нейна ", " Негова ")
                    .Replace(" негово/нейно ", " негово ")
                    .Replace(" Негово/нейно ", " Негово ")
                    .Replace(" негови/нейни ", " негови ")
                    .Replace(" Негови/нейни ", " Негови ")
                    .Replace(" бил/била ", " бил ")
                    .Replace(" Бил/била ", " Бил ")
                    .Replace(" направил/направила ", " направил ")
                    .Replace(" Направил/направила ", " Направил ")
                    .Replace(" готов/готова ", " готов ")
                    .Replace(" Готов/готова ", " Готов ");
            }
            else if (m_rbFemale.Checked)
            {
                strCurrentText = strCurrentText
                    .Replace(" той/тя ", " тя ")
                    .Replace(" Той/тя ", " Тя ")
                    .Replace(" на него/нея ", " на нея ")
                    .Replace(" На него/нея ", " На нея ")
                    .Replace(" на него/на нея ", " на нея ")
                    .Replace(" На него/на нея ", " На нея ")
                    .Replace(" него/нея ", " нея ")
                    .Replace(" Него/нея ", " Нея ")
                    .Replace(" го/я ", " я ")
                    .Replace(" Го/я ", " Я ")
                    .Replace(" му/ѝ ", " ѝ ")
                    .Replace(" Му/ѝ ", " Ѝ ")
                    .Replace(" негов/неин ", " неин ")
                    .Replace(" Негов/неин ", " Неин ")
                    .Replace(" негова/нейна ", " нейна ")
                    .Replace(" Негова/нейна ", " Нейна ")
                    .Replace(" негово/нейно ", " нейно ")
                    .Replace(" Негово/нейно ", " Нейно ")
                    .Replace(" негови/нейни ", " нейни ")
                    .Replace(" Негови/нейни ", " Нейни ")
                    .Replace(" бил/била ", " била ")
                    .Replace(" Бил/била ", " Била ")
                    .Replace(" направил/направила ", " направила ")
                    .Replace(" Направил/направила ", " Направила ")
                    .Replace(" готов/готова ", " готова ")
                    .Replace(" Готов/готова ", " Готова ");
            }

            // Shared name replacement is equivalent to the original's three branches:
            // Bulgarian name formatting does not depend on the selected gender.
            // Placeholders include their complete preposition: write [Име:D], not на [Име:D].
            // Genitive templates require the possessed noun before the placeholder:
            // "книгата [Име:G]" -> "книгата на Иван".
            strCurrentText = strCurrentText
                .Replace(" [Име] ", " " + strNameToUse + " ")
                .Replace(" [Име:G] ", " " + DeclineBulgarianName(strNameToUse, m_rbMale.Checked, BulgarianCase.Genitive) + " ")
                .Replace(" [Име:D] ", " " + DeclineBulgarianName(strNameToUse, m_rbMale.Checked, BulgarianCase.Dative) + " ")
                .Replace(" [Име:A] ", " " + DeclineBulgarianName(strNameToUse, m_rbMale.Checked, BulgarianCase.Accusative) + " ");

            tbxToAdapt.Text = strCurrentText.Trim().Replace(" .", ".");
        }

        #endregion

        #region TURKISH
        //===================================================================================================
        /// <summary>Turkish cases.</summary>
        //===================================================================================================
        enum TurkishCase
        {
            Nominative,
            Genitive,
            Dative,
            Accusative
        }

        //===================================================================================================
        /// <summary>Inflects a personal name using common Turkish case-suffix rules.</summary>
        /// <param name="strName">Personal name, normally in its nominative form.</param>
        /// <param name="bIsMale">Retained for compatibility; Turkish has no grammatical gender.</param>
        /// <param name="eTargetCase">Required case.</param>
        /// <returns>Name with an apostrophe and the appropriate suffix.</returns>
        //===================================================================================================
        string DeclineTurkishName(string strName, bool bIsMale, TurkishCase eTargetCase)
        {
            return DeclineTurkishName(strName, bIsMale, eTargetCase, null, null);
        }

        //===================================================================================================
        /// <summary>
        /// Optional pronunciation controls for names not predictable from spelling.
        /// chHarmonyVowel selects suffix harmony; e.g. 'e' for the front-vowel suffix
        /// class of Kemal (Kemal'e, Kemal'in, Kemal'i).
        /// bEndsInVowel overrides whether a buffer consonant is needed.
        /// The three-argument method uses spelling-based defaults. The textbox adapter
        /// also uses those defaults; connect these options to your own data if needed.
        /// </summary>
        //===================================================================================================
        string DeclineTurkishName(
            string strName, 
            bool bIsMale, 
            TurkishCase eTargetCase,
            char? cHarmonyVowel, 
            bool? bEndsInVowel
            )
        {
            if (string.IsNullOrWhiteSpace(strName) || eTargetCase == TurkishCase.Nominative)
                return strName;

            if (eTargetCase != TurkishCase.Genitive && eTargetCase != TurkishCase.Dative &&
                eTargetCase != TurkishCase.Accusative) return strName;

            var oCulture = System.Globalization.CultureInfo.GetCultureInfo("tr-TR");
            string strNameToUse = strName.Trim().Normalize(System.Text.NormalizationForm.FormC);

            if (!char.IsLetter(strNameToUse[strNameToUse.Length - 1])) 
                return strName;

            string strLowerName = strNameToUse.ToLower(oCulture);
            const string strVowels = "aeıioöuüâîû";
            char cLastVowel = '\0';

            // Use the last component's pronunciation, not earlier parts of a full name.
            for (int i = strLowerName.Length - 1; i >= 0; i--)
            {
                char cCurrent = strLowerName[i];

                if (char.IsWhiteSpace(cCurrent) || cCurrent == '-') 
                    break;

                if (strVowels.IndexOf(cCurrent) >= 0)
                {
                    cLastVowel = cCurrent;
                    break;
                }
            }

            if (cHarmonyVowel.HasValue)
            {
                cLastVowel = char.ToLower(cHarmonyVowel.Value, oCulture);

                if (strVowels.IndexOf(cLastVowel) < 0)
                    throw new System.ArgumentException("Supply a Turkish harmony vowel.", nameof(cHarmonyVowel));
            }

            if (cLastVowel == '\0') 
                return strName;

            // Circumflex letters have pronunciation-dependent behavior in some names.
            // These defaults use the underlying vowel; use the override where needed.
            if (cLastVowel == 'â') 
                cLastVowel = 'a';

            else if (cLastVowel == 'î')
                cLastVowel = 'i';

            else if (cLastVowel == 'û') 
                cLastVowel = 'u';

            string strHighVowel;
            string strLowVowel;

            switch (cLastVowel)
            {
                case 'a': case 'ı': strHighVowel = "ı"; strLowVowel = "a"; break;
                case 'e': case 'i': strHighVowel = "i"; strLowVowel = "e"; break;
                case 'o': case 'u': strHighVowel = "u"; strLowVowel = "a"; break;
                default: strHighVowel = "ü"; strLowVowel = "e"; break; // ö, ü
            }

            bool bVowelEnding = bEndsInVowel.HasValue ? bEndsInVowel.Value :
                strVowels.IndexOf(strLowerName[strLowerName.Length - 1]) >= 0;

            string strSuffix;
            switch (eTargetCase)
            {
                case TurkishCase.Genitive:
                    strSuffix = (bVowelEnding ? "n" : "") + strHighVowel + "n";
                    break;
                case TurkishCase.Dative:
                    strSuffix = (bVowelEnding ? "y" : "") + strLowVowel;
                    break;
                default: // Definite accusative, appropriate for personal names.
                    strSuffix = (bVowelEnding ? "y" : "") + strHighVowel;
                    break;
            }

            // Proper-name spelling preserves the stem: Ahmet'i, not Ahmedi.
            // An ASCII apostrophe is used consistently in this source.
            return strNameToUse + "'" + strSuffix;
        }

        //===================================================================================================
        /// <summary>
        /// Adapts Turkish templates using the search/replacement logic of the Russian
        /// original, including the nominative guard with ONE leading space.
        /// </summary>
        /// <param name="tbxToAdapt">Textbox for adaptation of text.</param>
        //===================================================================================================
        void AdaptTextToGenderAndName_Turkish(TextBox tbxToAdapt)
        {
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";
            if (m_chkAddName.Checked && !strCurrentText.Contains("[Ad"))
            {
                string? strToReplace = null;
                int nPosToReplace = -1;

                // O = he/she, ona = to him/her, onun = his/her, onu = him/her.
                // These forms are identical for men and women; no o/o alternatives needed.
                // Only use this adapter with templates where these words refer to the
                // intended person: o can also mean "that" or "it" in arbitrary text.
                foreach (string strTextToSearch in new string[]
                    { " O ", " Ona ", " Onun ", " Onu " })
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" O ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                    }
                }

                if (strToReplace != null)
                {
                    if (" Onun ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " [Ad:G] " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else if (" Ona ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " [Ad:D] " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else if (" Onu ".Equals(strToReplace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " [Ad:A] " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " [Ad] " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                }
            }

            string strNameToUse = m_tbxName.Text;
            if (string.IsNullOrWhiteSpace(strNameToUse))
                strNameToUse = "AD EKSİK";
            else
                strNameToUse = strNameToUse.Trim();

            // Turkish pronouns, adjectives and verbs do not have grammatical gender.
            // Therefore name replacement is shared by all gender selections.
            // A possessive template must already inflect the possessed noun correctly:
            // [Ad:G] kitabı -> Ayşe'nin kitabı; not Ayşe'nin kitap.
            // Placeholders contain the entire case suffix; do not append another suffix.
            // As in the original, tokens must have spaces around them; periods are handled
            // above. Other punctuation must be separated in the prepared template.
            strCurrentText = strCurrentText
                .Replace(" [Ad] ", " " + strNameToUse + " ")
                .Replace(" [Ad:G] ", " " + DeclineTurkishName(strNameToUse, m_rbMale.Checked, TurkishCase.Genitive) + " ")
                .Replace(" [Ad:D] ", " " + DeclineTurkishName(strNameToUse, m_rbMale.Checked, TurkishCase.Dative) + " ")
                .Replace(" [Ad:A] ", " " + DeclineTurkishName(strNameToUse, m_rbMale.Checked, TurkishCase.Accusative) + " ");

            tbxToAdapt.Text = strCurrentText.Trim().Replace(" .", ".");
        }
        #endregion

        #region ARABIC

        // Modern Standard Arabic; prepared templates, not arbitrary sentence analysis.

        enum ArabicCase 
        { 
            Nominative, 
            Genitive, 
            Dative, 
            Accusative 
        }
        enum ArabicNameDeclension
        { 
            Automatic, 
            Triptote, 
            Diptote, 
            Indeclinable 
        }

        // Automatic recognizes a fully declined name from final nominative tanwin.
        // For other vocalized names set the known class explicitly, e.g. Diptote for أحمد.
        // This is a template/application setting, not something determined by gender.
        ArabicNameDeclension m_eArabicNameDeclension = ArabicNameDeclension.Automatic;

        //===================================================================================================
        /// <summary>
        /// Formats an Arabic name for a case. Dative means لِـ + genitive here.
        /// Arabic has no separate noun dative ending. Other prepositions need their own
        /// templates. Unvocalized names normally retain their spelling. A known triptote can need
        /// accusative supporting alif; dative also adds the ل prefix.
        /// </summary>
        //===================================================================================================
        string DeclineArabicName(string strName, bool bIsMale, ArabicCase eTargetCase)
        {
            return DeclineArabicName(strName, bIsMale, eTargetCase, m_eArabicNameDeclension);
        }

        //===================================================================================================
        /// <summary>
        /// Declines a single nominative-form name. Internal vowel marks are preserved.
        /// Automatic infers triptote only from final ٌ; otherwise it preserves the name.
        /// Explicit classes require a vocalized nominative input with final ُ or ٌ.
        /// Compound names and names ending in ا/ى require separate morphology and are
        /// left unchanged internally. Use pre-inflected text for those templates.
        /// </summary>
        //===================================================================================================
        string DeclineArabicName(
            string strName, 
            bool bIsMale, 
            ArabicCase eTargetCase,
            ArabicNameDeclension eDeclension
            )
        {
            if (string.IsNullOrWhiteSpace(strName)) 
                return strName;

            if (eTargetCase != ArabicCase.Nominative && eTargetCase != ArabicCase.Genitive &&
                eTargetCase != ArabicCase.Dative && eTargetCase != ArabicCase.Accusative)
            {
                return strName;
            }

            string strTrimmedName = strName.Trim();
            string strBareName = RemoveArabicFarsiUrduVowelMarks(strTrimmedName);

            bool bVocalized = strBareName != strTrimmedName;
            bool bDative = eTargetCase == ArabicCase.Dative;

            ArabicCase eActualCase = bDative ? ArabicCase.Genitive : eTargetCase;
            string strResult = strTrimmedName;

            bool bSingle = true;

            foreach (char c in strTrimmedName)
            {
                if (char.IsWhiteSpace(c) || c == '-')
                {
                    bSingle = false;
                    break;
                }
            }

            if (bSingle && bVocalized && strBareName.Length > 0 &&
                !strBareName.EndsWith("ا", StringComparison.Ordinal) && 
                !strBareName.EndsWith("ى", StringComparison.Ordinal) &&
                eDeclension != ArabicNameDeclension.Indeclinable)
            {
                // Accept case ending before or after a final shadda.
                int nEnding = -1;
                for (int i = strTrimmedName.Length - 1; i >= 0 && IsArabicFarsiUrduVowelMark(strTrimmedName[i]); i--)
                {
                    if (strTrimmedName[i] == '\u064F' || strTrimmedName[i] == '\u064C')
                    {
                        nEnding = i;
                        break;
                    }
                }
                if (nEnding >= 0)
                {
                    ArabicNameDeclension eKind = eDeclension;

                    if (eKind == ArabicNameDeclension.Automatic && strTrimmedName[nEnding] == '\u064C')
                        eKind = ArabicNameDeclension.Triptote;

                    string strStem = strTrimmedName.Remove(nEnding, 1);
                    string? strMark = null;

                    if (eKind == ArabicNameDeclension.Triptote)
                    {
                        strMark = eActualCase == ArabicCase.Nominative ? "ٌ" :
                            eActualCase == ArabicCase.Genitive ? "ٍ" : "ً";

                        // The accusative tanwin normally needs a supporting alif.
                        if (eActualCase == ArabicCase.Accusative &&
                            !strBareName.EndsWith("ة", StringComparison.Ordinal) &&
                            !strBareName.EndsWith("أ", StringComparison.Ordinal) &&
                            !strBareName.EndsWith("اء", StringComparison.Ordinal))
                        {
                            strResult = strStem + strMark + "ا";
                        }
                        else
                        {
                            strResult = strStem + strMark;
                        }
                    }
                    else if (eKind == ArabicNameDeclension.Diptote)
                    {
                        // Diptotes with definite article take kasra in the genitive.
                        bool article = strBareName.StartsWith("ال", StringComparison.Ordinal) ||
                            strBareName.StartsWith("ٱل", StringComparison.Ordinal);

                        strMark = eActualCase == ArabicCase.Nominative ? "ُ" :
                            eActualCase == ArabicCase.Genitive && article ? "ِ" : "َ";

                        strResult = strStem + strMark;
                    }
                    // Without a known class, preserve the supplied form rather than guess.
                }
            }
            // Supporting accusative alif is part of the spelling even without harakat.
            // Only add it when the caller explicitly identifies the triptote paradigm.
            if (bSingle && !bVocalized && eDeclension == ArabicNameDeclension.Triptote &&
                eActualCase == ArabicCase.Accusative && strBareName.Length > 0 &&
                !strBareName.EndsWith("ا", StringComparison.Ordinal) && !strBareName.EndsWith("ى", StringComparison.Ordinal) &&
                !strBareName.EndsWith("ة", StringComparison.Ordinal) && !strBareName.EndsWith("أ", StringComparison.Ordinal) &&
                !strBareName.EndsWith("ء", StringComparison.Ordinal))
            {
                strResult = strTrimmedName + "ا";
            }

            if (bDative)
            {
                strResult = PrefixArabicLam(strResult, bVocalized);
            }

            return strResult;
        }

        //===================================================================================================
        /// <summary>
        /// Provides indication, if a char represents an arabic vowel
        /// </summary>
        /// <param name="c">Char for testing</param>
        /// <returns>true iff the char is an arabic vowel</returns>
        //===================================================================================================
        bool IsArabicFarsiUrduVowelMark(char c)
        {
            // Harakat, shadda, sukun and dagger alif. Hamza/madda combining marks are
            // intentionally preserved because they affect the identity of letters.
            return (c >= '\u064B' && c <= '\u0652') || c == '\u0670';
        }

        //===================================================================================================
        /// <summary>
        /// Removes arabic vowels
        /// </summary>
        /// <param name="strText">Text for devocalization</param>
        /// <returns>Non-vocalized text</returns>
        //===================================================================================================
        string RemoveArabicFarsiUrduVowelMarks(string strText)
        {
            var result = new System.Text.StringBuilder();
            foreach (char c in strText)
            {
                if (!IsArabicFarsiUrduVowelMark(c)) 
                    result.Append(c);
            }
            return result.ToString();
        }

        //===================================================================================================
        /// <summary>
        /// Handles arabic alef
        /// </summary>
        /// <param name="strName">The name</param>
        /// <param name="bVocalized">Indication, if vocalized</param>
        /// <returns>the modified name</returns>
        //===================================================================================================
        string PrefixArabicLam(string strName, bool bVocalized)
        {
            string strBareName = RemoveArabicFarsiUrduVowelMarks(strName);

            // Special spelling for Allah, including an existing marked genitive ending.
            if (strBareName == "الله" || strBareName == "ٱلله")
                return bVocalized ? "لِلّٰهِ" : "لله";

            // Initial alif of the article disappears after ل: الحسن -> للحسن.
            if (strBareName.StartsWith("ال", StringComparison.Ordinal) ||
                strBareName.StartsWith("ٱل", StringComparison.Ordinal))
            {
                int nIndex = 1;

                while (nIndex < strName.Length && IsArabicFarsiUrduVowelMark(strName[nIndex])) 
                    nIndex++;

                strName = strName.Substring(nIndex);
            }

            return (bVocalized ? "لِ" : "ل") + strName;
        }

        //===================================================================================================
        /// <summary>
        /// Adapts unvocalized and vocalized Arabic template alternatives.
        /// Uses the Russian original's IndexOf/earliest-match logic and nominative guard
        /// with exactly ONE leading space. No regular expressions are used.
        /// </summary>
        //===================================================================================================
        void AdaptTextToGenderAndName_Arabic(TextBox tbxToAdapt)
        {
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";
            if (m_chkAddName.Checked && !strCurrentText.Contains("[اسم"))
            {
                string? strToReplace = null;
                int nPosToReplace = -1;
                // Generate plain, fully vocalized and partially vocalized spellings from
                // the same alternatives. Nominal alternatives are searched first so the
                // original nominative guard preserves their priority.
                foreach (string strTextToSearch in ArabicFarsiUrduHebrewTemplateVariants(new string[]
                            { " هُوَ/هِيَ ", " لَهُ/لَهَا ", " إِيَّاهُ/إِيَّاهَا " }))
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, StringComparison.CurrentCultureIgnoreCase);

                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" هو/هي ".Equals(RemoveArabicFarsiUrduVowelMarks(strToReplace), StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                    }
                }

                if (strToReplace != null)
                {
                    string plain = RemoveArabicFarsiUrduVowelMarks(strToReplace);
                    if (" له/لها ".Equals(plain, StringComparison.CurrentCultureIgnoreCase))
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " [اسم:D] " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);

                    else if (" إياه/إياها ".Equals(plain, StringComparison.CurrentCultureIgnoreCase))
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " [اسم:A] " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);

                    else
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " [اسم] " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                }
            }

            string strNameToUse = m_tbxName.Text;
            bool missing = string.IsNullOrWhiteSpace(strNameToUse);
            if (missing) strNameToUse = "[الاسم غير موجود]";

            // Only explicit alternatives are changed. Attached pronouns such as ـه/ـها
            // inside verbs or nouns require sentence restructuring to insert a name.
            // Do not replace them automatically. Possessive names need an idafa template,
            // e.g. كتاب [اسم:G], not a literal replacement of a suffix inside كتابه.
            if (m_rbMale.Checked || m_rbFemale.Checked)
            {
                bool male = m_rbMale.Checked;
                foreach (string alternative in ArabicFarsiUrduHebrewTemplateVariants(new string[]
                    { " هُوَ/هِيَ ", " لَهُ/لَهَا ", " إِيَّاهُ/إِيَّاهَا ",
                      " سَعِيدٌ/سَعِيدَةٌ ", " ذَهَبَ/ذَهَبَتْ " }))
                {
                    string[] forms = alternative.Trim().Split('/');
                    strCurrentText = strCurrentText.Replace(alternative, " " + forms[male ? 0 : 1] + " ");
                }
            }

            // Missing-name messages must not receive grammatical suffixes or prefixes.
            strCurrentText = strCurrentText
                .Replace(" [اسم] ", " " + strNameToUse + " ")
                .Replace(" [اسم:G] ", " " + (missing ? strNameToUse : DeclineArabicName(strNameToUse, m_rbMale.Checked, ArabicCase.Genitive)) + " ")
                .Replace(" [اسم:D] ", " " + (missing ? strNameToUse : DeclineArabicName(strNameToUse, m_rbMale.Checked, ArabicCase.Dative)) + " ")
                .Replace(" [اسم:A] ", " " + (missing ? strNameToUse : DeclineArabicName(strNameToUse, m_rbMale.Checked, ArabicCase.Accusative)) + " ");
            tbxToAdapt.Text = strCurrentText.Trim().Replace(" .", ".");
        }

        //===================================================================================================
        /// <summary>
        /// Enumerates optional harakat without modifying the text being searched.
        /// Every returned item is searched with IndexOf or replaced with string.Replace.
        /// Keeps shadda and vowel ordering in the source and also accepts canonical order.
        /// </summary>
        //===================================================================================================
        string[] ArabicFarsiUrduHebrewTemplateVariants(string[] astrAlternatives)
        {
            var oResult = new System.Collections.Generic.List<string>();
            var oSeen = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);

            foreach (string strOriginal in astrAlternatives)
            {
                foreach (string strSpelling in new string[] { strOriginal, 
                    strOriginal.Normalize(System.Text.NormalizationForm.FormC) })
                {
                    var variants = new System.Collections.Generic.List<string> { "" };
                    foreach (char c in strSpelling)
                    {
                        int count = variants.Count;
                        if (IsArabicFarsiUrduVowelMark(c))
                        {
                            // Keep vocalized forms first, then generate omissions.
                            for (int i = 0; i < count; i++)
                            {
                                string strPrefix = variants[i];
                                variants[i] = strPrefix + c;
                                variants.Add(strPrefix);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < count; i++)
                                variants[i] += c;
                        }
                    }
                    foreach (string strVariant in variants)
                    {
                        if (oSeen.Add(strVariant)) 
                            oResult.Add(strVariant);
                    }
                }
            }
            return oResult.ToArray();
        }
        #endregion

        #region FARSI
        // Standard Iranian Persian (Farsi); prepared templates, not arbitrary text parsing.

        //===================================================================================================
        /// <summary>
        /// Semantic cases corresponding 
        /// </summary>
        //===================================================================================================
        enum FarsiCase
        {
            Nominative,
            Genitive,
            Dative,
            Accusative
        }

        //===================================================================================================
        /// <summary>
        /// Formats a Persian name. Names do not change case endings.
        /// Genitive: unchanged name after an ezafe-bearing noun.
        /// Dative: به + name. Definite direct object: name + را.
        /// Gender does not affect these forms.
        /// </summary>
        //===================================================================================================
        string DeclineFarsiName(
            string strName, 
            bool bIsMale, 
            FarsiCase eTargetCase
            )
        {
            return DeclineFarsiName(strName, bIsMale, eTargetCase,
                !string.IsNullOrEmpty(strName) && HasArabicFarsiUrduVowelMarks(strName));
        }

        //===================================================================================================
        /// <summary>
        /// Explicitly chooses marked or unmarked به / را. Preserves the supplied name's
        /// spelling and internal vowel marks; does not invent a name's pronunciation.
        /// Dative here means the relationship expressed by به, not every indirect object.
        /// </summary>
        //===================================================================================================
        string DeclineFarsiName(
            string strName, 
            bool bIsMale, 
            FarsiCase eTargetCase,
            bool bVocalizedParticles
            )
        {
            if (string.IsNullOrWhiteSpace(strName)) return strName;
            switch (eTargetCase)
            {
                case FarsiCase.Dative:
                    return (bVocalizedParticles ? "بِه " : "به ") + strName;

                case FarsiCase.Accusative:
                    return strName + (bVocalizedParticles ? " رَا" : " را");

                case FarsiCase.Nominative:
                case FarsiCase.Genitive:

                default:
                    return strName;
            }
        }

        //===================================================================================================
        /// <summary>
        /// Adapts Persian templates using the original IndexOf/earliest-match logic and
        /// nominative guard with ONE leading space. Handles optional vowel marks.
        /// Persian او is gender-neutral; no masculine/feminine verb alternatives needed.
        /// </summary>
        //===================================================================================================
        void AdaptTextToGenderAndName_Farsi(TextBox tbxToAdapt)
        {
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";
            bool bVocalized = HasArabicFarsiUrduVowelMarks(strCurrentText);

            if (m_chkAddName.Checked && !strCurrentText.Contains("[نام"))
            {
                string? strToReplace = null;
                int nPosToReplace = -1;

                // Nominal forms first, as in the Russian original. The same او also
                // occurs after prepositions or ezafe and before را. Replacing only او
                // leaves the surrounding relationship intact in those constructions.
                foreach (string strTextToSearch in ArabicFarsiUrduHebrewTemplateVariants(new string[]
                    { " اُو ", " بِه اُو ", " اُو رَا " }))
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" او ".Equals(RemoveArabicFarsiUrduVowelMarks(strToReplace), StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                    }
                }

                if (strToReplace != null)
                {
                    string plain = RemoveArabicFarsiUrduVowelMarks(strToReplace);
                    if (" به او ".Equals(plain, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " [نام:D] " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else if (" او را ".Equals(plain, StringComparison.CurrentCultureIgnoreCase))
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " [نام:A] " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                    else
                    {
                        strCurrentText = strCurrentText.Substring(0, nPosToReplace) + " [نام] " +
                            strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                    }
                }
            }

            string strNameToUse = m_tbxName.Text;
            bool bNameMissing = string.IsNullOrWhiteSpace(strNameToUse);
            if (bNameMissing) strNameToUse = "[نام وارد نشده است]";
            bVocalized = bVocalized || HasArabicFarsiUrduVowelMarks(strNameToUse);

            // There is no grammatical gender to adapt in Persian. The original's male,
            // female and unselected branches therefore share the same name replacement.
            // Possessive construction: کتابِ [نام:G] -> کتابِ علی.
            // Ezafe belongs to the preceding noun, not to the owner's name. It must
            // already be correct in the template, including هٔ / ی where appropriate.
            // Attached possessive/object ـش cannot safely be replaced without parsing.
            // Reflexive خود is not automatically replaced by the selected person's name.
            // D/A placeholders include به / را: do not add those particles a second time.
            strCurrentText = strCurrentText
                .Replace(" [نام] ", " " + strNameToUse + " ")
                .Replace(" [نام:G] ", " " + (bNameMissing ? strNameToUse : DeclineFarsiName(strNameToUse, m_rbMale.Checked, FarsiCase.Genitive, bVocalized)) + " ")
                .Replace(" [نام:D] ", " " + (bNameMissing ? strNameToUse : DeclineFarsiName(strNameToUse, m_rbMale.Checked, FarsiCase.Dative, bVocalized)) + " ")
                .Replace(" [نام:A] ", " " + (bNameMissing ? strNameToUse : DeclineFarsiName(strNameToUse, m_rbMale.Checked, FarsiCase.Accusative, bVocalized)) + " ");

            // As in the original, alternatives/placeholders require surrounding spaces.
            // Periods are handled above; separate other punctuation in the template.
            tbxToAdapt.Text = strCurrentText.Trim().Replace(" .", ".");
        }


        //===================================================================================================
        /// <summary>
        /// Provides indication if there are farsi vowels
        /// </summary>
        /// <param name="strText">Text to test</param>
        /// <returns>true iff there are vowels</returns>
        //===================================================================================================
        bool HasArabicFarsiUrduVowelMarks(string strText)
        {
            foreach (char c in strText)
            {
                if (IsArabicFarsiUrduVowelMark(c)) 
                    return true;
            }
            return false;
        }


        #endregion

        #region URDU
        //===================================================================================================
        /// <summary>Semantic cases and possessive agreement for Urdu names.</summary>
        //===================================================================================================
        enum UrduCase
        {
            Nominative,
            Genitive,                    // کا: masculine singular possessed noun, direct case
            Dative,
            Accusative,
            GenitiveFeminine,            // کی: feminine possessed noun
            GenitivePluralOrOblique,     // کے: masculine plural or masculine oblique possessed noun
            Ergative                    // نے: only in grammatically appropriate clauses
        }

        //===================================================================================================
        /// <summary>
        /// Combines a personal name with an Urdu postposition. The gender of the name
        /// bearer does not select کا/کی/کے: agreement is with the possessed noun.
        /// </summary>
        //===================================================================================================
        string DeclineUrduName(string strName, bool bIsMale, UrduCase eTargetCase)
        {
            return DeclineUrduName(strName, bIsMale, eTargetCase,
                !string.IsNullOrEmpty(strName) && HasArabicFarsiUrduVowelMarks(strName), null);
        }

        //===================================================================================================
        /// <summary>
        /// Optional particle vocalization and an explicit oblique name form.
        /// Most personal names retain their form before a postposition. Where a name
        /// has a distinct oblique form, supply it as strObliqueName rather than applying
        /// a common-noun ending rule to every name. Compound names are accepted literally.
        /// Names' internal marks are preserved; their pronunciation is not inferred.
        /// </summary>
        //===================================================================================================
        string DeclineUrduName(
            string strName,
            bool bIsMale, 
            UrduCase eTargetCase,
            bool bVocalizedParticles, 
            string? strObliqueName
            )
        {
            if (string.IsNullOrWhiteSpace(strName) || eTargetCase == UrduCase.Nominative)
                return strName;

            string strParticle;

            switch (eTargetCase)
            {
                case UrduCase.Genitive: strParticle = bVocalizedParticles ? "کَا" : "کا"; break;
                case UrduCase.GenitiveFeminine: strParticle = bVocalizedParticles ? "کِی" : "کی"; break;
                case UrduCase.GenitivePluralOrOblique: strParticle = "کے"; break;
                case UrduCase.Dative:
                case UrduCase.Accusative: strParticle = "کو"; break;
                case UrduCase.Ergative: strParticle = "نے"; break;
                default: return strName;
            }

            // The vowels e/o in کے/نے/کو are not manufactured with an Arabic short-vowel mark.
            string strResultName = strObliqueName ?? strName;
            if (string.IsNullOrWhiteSpace(strResultName))
            {
                throw new System.ArgumentException("The oblique name must not be empty.", nameof(strObliqueName));
            }
            return strResultName + " " + strParticle;
        }

        //===================================================================================================
        /// <summary>
        /// Adapts plain, vocalized and partially vocalized Urdu templates using the
        /// Russian original's direct IndexOf and nominative guard with ONE leading space.
        /// </summary>
        //===================================================================================================
        void AdaptTextToGenderAndName_Urdu(TextBox tbxToAdapt)
        {
            // Urdu full stop is handled in the same way as the original's Latin period.
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .").Replace("۔", " ۔") + " ";
            bool bVocalized = HasArabicFarsiUrduVowelMarks(strCurrentText);
            if (m_chkAddName.Checked && !strCurrentText.Contains("[نام"))
            {
                string? strToReplace = null;
                int nPosToReplace = -1;
                foreach (string strTextToSearch in ArabicFarsiUrduHebrewTemplateVariants(new string[]
                    { " وُہ ", " وَہ ", " اُس کو ", " اُسے ", " اُس کَا ", " اُس کِی ", " اُس کے ", " اُس نے " }))
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" وہ ".Equals(RemoveArabicFarsiUrduVowelMarks(strToReplace), StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                    }
                }
                if (strToReplace != null)
                {
                    string replacement;
                    switch (RemoveArabicFarsiUrduVowelMarks(strToReplace))
                    {
                        case " اس کو ": case " اسے ": replacement = " [نام:D] "; break;
                        case " اس کا ": replacement = " [نام:G] "; break;
                        case " اس کی ": replacement = " [نام:GF] "; break;
                        case " اس کے ": replacement = " [نام:GE] "; break;
                        case " اس نے ": replacement = " [نام:E] "; break;
                        default: replacement = " [نام] "; break;
                    }
                    strCurrentText = strCurrentText.Substring(0, nPosToReplace) + replacement +
                        strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                }
            }

            string strNameToUse = m_tbxName.Text;
            bool bNameMissing = string.IsNullOrWhiteSpace(strNameToUse);

            if (bNameMissing) 
                strNameToUse = "[نام موجود نہیں]";

            bVocalized = bVocalized || HasArabicFarsiUrduVowelMarks(strNameToUse);

            // وہ and اُس are gender-neutral. Explicit verb alternatives can agree with
            // the selected person. Only put alternatives here when that person controls
            // agreement: perfective transitive clauses with نے often agree differently.
            // اپنا/اپنی/اپنے and کا/کی/کے must not be selected by the name bearer's gender.
            if (m_rbMale.Checked || m_rbFemale.Checked)
            {
                bool male = m_rbMale.Checked;
                foreach (string alternative in ArabicFarsiUrduHebrewTemplateVariants(new string[]
                    { " کَرْتَا/کَرْتِی ", " رَہْتَا/رَہْتِی ", " تھَا/تھِی " }))
                {
                    string[] forms = alternative.Trim().Split('/');
                    strCurrentText = strCurrentText.Replace(alternative, " " + forms[male ? 0 : 1] + " ");
                }
            }

            // Prepared placeholders, not ordinary occurrences of the word نام.
            // Each placeholder includes its postposition: do not add کو/کا/etc. again.
            // The textbox uses unchanged name stems; connect a known oblique form to the
            // five-argument overload above if your application supports such names.
            strCurrentText = strCurrentText
                .Replace(" [نام] ", " " + strNameToUse + " ")
                .Replace(" [نام:G] ", " " + (bNameMissing ? strNameToUse : DeclineUrduName(strNameToUse, m_rbMale.Checked, UrduCase.Genitive, bVocalized, null)) + " ")
                .Replace(" [نام:GF] ", " " + (bNameMissing ? strNameToUse : DeclineUrduName(strNameToUse, m_rbMale.Checked, UrduCase.GenitiveFeminine, bVocalized, null)) + " ")
                .Replace(" [نام:GE] ", " " + (bNameMissing ? strNameToUse : DeclineUrduName(strNameToUse, m_rbMale.Checked, UrduCase.GenitivePluralOrOblique, bVocalized, null)) + " ")
                .Replace(" [نام:D] ", " " + (bNameMissing ? strNameToUse : DeclineUrduName(strNameToUse, m_rbMale.Checked, UrduCase.Dative, bVocalized, null)) + " ")
                .Replace(" [نام:A] ", " " + (bNameMissing ? strNameToUse : DeclineUrduName(strNameToUse, m_rbMale.Checked, UrduCase.Accusative, bVocalized, null)) + " ")
                .Replace(" [نام:E] ", " " + (bNameMissing ? strNameToUse : DeclineUrduName(strNameToUse, m_rbMale.Checked, UrduCase.Ergative, bVocalized, null)) + " ");

            // Space-delimited tokens, as in the original. Other punctuation must be
            // separated in the template. Word spelling and zero-width characters stay intact.
            tbxToAdapt.Text = strCurrentText.Trim().Replace(" .", ".").Replace(" ۔", "۔");
        }

        #endregion

        #region HEBREW
        // Modern Hebrew prepared templates; no regular expressions.

        enum HebrewCase 
        { 
            Nominative, 
            Genitive, 
            Dative, 
            Accusative 
        }

        //===================================================================================================
        /// <summary>
        /// Formats a name with Hebrew relationship markers; the name has no case ending.
        /// </summary>
        //===================================================================================================
        string DeclineHebrewName(string strName, bool bIsMale, HebrewCase eTargetCase)
        {
            return DeclineHebrewName(strName, bIsMale, eTargetCase,
                !string.IsNullOrEmpty(strName) && HasHebrewPoints(strName), null);
        }

        //===================================================================================================
        /// <summary>
        /// Genitive: של + name. Definite direct object: את + name. Dative: attached ל.
        /// bVocalizedParticles selects pointed particles; the name's own spelling stays.
        /// strDativeOverride optionally supplies the COMPLETE ל-prefixed name, including
        /// points, for exceptional pronunciations. The adapter uses the default rules.
        /// </summary>
        //===================================================================================================
        string DeclineHebrewName(
            string strName, 
            bool bIsMale, 
            HebrewCase eTargetCase,
            bool bVocalizedParticles, 
            string? strDativeOverride
            )
        {
            if (string.IsNullOrWhiteSpace(strName)) return strName;
            switch (eTargetCase)
            {
                case HebrewCase.Genitive: 
                    return (bVocalizedParticles ? "שֶׁל " : "של ") + strName;
                case HebrewCase.Accusative: 
                    return (bVocalizedParticles ? "אֶת " : "את ") + strName;
                case HebrewCase.Dative:
                    if (strDativeOverride != null) 
                        return strDativeOverride;

                    return PrefixHebrewLamed(strName, bVocalizedParticles);

                default: 
                    return strName;
            }
        }

        //===================================================================================================
        /// <summary>
        /// Common ל-prefix pointing rules for a personal name: לְדָוִד, לִשְׁמוּאֵל,
        /// לִיהוּדָה, לַאֲבִיגַיִל. Not a complete Hebrew vocalization engine.
        /// Pointed output assumes the name has correct input points; an unpointed name's
        /// pronunciation cannot be reconstructed. Use the explicit override if needed.
        /// </summary>
        //===================================================================================================
        string PrefixHebrewLamed(
            string strName, 
            bool bPointed
            )
        {
            if (!bPointed) 
                return "ל" + strName;

            if (strName.Length == 0 || strName[0] < '\u05D0' || strName[0] > '\u05EA') 
                return "לְ" + strName;

            int nFirstEnd = 1;

            while (nFirstEnd < strName.Length && IsHebrewPoint(strName[nFirstEnd])) 
                nFirstEnd++;

            string strFirst = strName.Substring(0, nFirstEnd);
            string strPrefix = "לְ";

            if (strFirst.IndexOf('\u05B2') >= 0) strPrefix = "לַ"; // hataf patah
            else if (strFirst.IndexOf('\u05B1') >= 0) strPrefix = "לֶ"; // hataf segol
            else if (strFirst.IndexOf('\u05B3') >= 0) strPrefix = "לָ"; // hataf qamats
            else if (strFirst.IndexOf('\u05B0') >= 0)
            {
                strPrefix = "לִ";
                if (strName[0] == 'י') strFirst = strFirst.Replace("ְ", "");
            }

            // Initial weak dagesh in begadkefat disappears after the vocalic prefix.
            // Lexical strong dagesh or unusual names may need strDativeOverride.
            if ("בגדכפת".IndexOf(strName[0]) >= 0) 
                strFirst = strFirst.Replace("ּ", "");

            return strPrefix + strFirst + strName.Substring(nFirstEnd);
        }

        //===================================================================================================
        /// <summary>
        /// Uses the Russian original's direct IndexOf and nominative guard with ONE
        /// leading space. Accepts pointed, unpointed and partially pointed alternatives.
        /// </summary>
        //===================================================================================================
        void AdaptTextToGenderAndName_Hebrew(TextBox tbxToAdapt)
        {
            string strCurrentText = " " + tbxToAdapt.Text.Replace(".", " .") + " ";
            bool bVocalized = HasHebrewPoints(strCurrentText);

            if (m_chkAddName.Checked && !strCurrentText.Contains("[שם"))
            {
                string? strToReplace = null;
                int nPosToReplace = -1;
                // Both plene אותו/אותה and defective אתו/אתה spellings are included.
                // Only use these explicit alternatives for the intended person in templates;
                // Hebrew הוא/היא can also function as a copula, not an ordinary subject.
                foreach (string strTextToSearch in ArabicFarsiUrduHebrewTemplateVariants(new string[]
                    { " הוּא/הִיא ", " לוֹ/לָהּ ", " שֶׁלּוֹ/שֶׁלָּהּ ",
                      " אֹתוֹ/אֹתָהּ ", " אוֹתוֹ/אוֹתָהּ " }))
                {
                    int nPos = strCurrentText.IndexOf(strTextToSearch, StringComparison.CurrentCultureIgnoreCase);
                    if (nPos >= 0 && (nPos < nPosToReplace || nPosToReplace < 0) &&
                        (strToReplace == null || !" הוא/היא ".Equals(RemoveHebrewPoints(strToReplace), StringComparison.CurrentCultureIgnoreCase)))
                    {
                        strToReplace = strCurrentText.Substring(nPos, strTextToSearch.Length);
                        nPosToReplace = nPos;
                    }
                }
                if (strToReplace != null)
                {
                    string replacement;
                    switch (RemoveHebrewPoints(strToReplace))
                    {
                        case " לו/לה ": replacement = " [שם:D] "; break;
                        case " שלו/שלה ": replacement = " [שם:G] "; break;
                        case " אתו/אתה ": case " אותו/אותה ": replacement = " [שם:A] "; break;
                        default: replacement = " [שם] "; break;
                    }
                    strCurrentText = strCurrentText.Substring(0, nPosToReplace) + replacement +
                        strCurrentText.Substring(nPosToReplace + strToReplace.Length);
                }
            }

            string strNameToUse = m_tbxName.Text;
            bool bNameMissing = string.IsNullOrWhiteSpace(strNameToUse);

            if (bNameMissing)
                strNameToUse = "[השם חסר]";

            bVocalized = bVocalized || HasHebrewPoints(strNameToUse);

            if (m_rbMale.Checked || m_rbFemale.Checked)
            {
                bool bMale = m_rbMale.Checked;
                foreach (string alternative in ArabicFarsiUrduHebrewTemplateVariants(new string[]
                    {   " הוּא/הִיא ", " לוֹ/לָהּ ", " שֶׁלּוֹ/שֶׁלָּהּ ",
                        " אֹתוֹ/אֹתָהּ ", " אוֹתוֹ/אוֹתָהּ ",
                        " מוּכָן/מוּכָנָה ", " שָׂמֵחַ/שְׂמֵחָה ",
                        " הָיָה/הָיְתָה ", " היה/הייתה " }))
                {
                    string[] astrForms = alternative.Trim().Split('/');
                    strCurrentText = strCurrentText.Replace(alternative, " " + astrForms[bMale ? 0 : 1] + " ");
                }
            }

            // Each placeholder includes its marker: no extra ל/של/את in the template.
            // Possessive example: הספר [שם:G] -> הספר של דנה.
            // Attached possessive suffixes and other prepositions require separate
            // templates; they are not replaced by guessing inside words.
            strCurrentText = strCurrentText
                .Replace(" [שם] ", " " + strNameToUse + " ")
                .Replace(" [שם:G] ", " " + (bNameMissing ? strNameToUse : DeclineHebrewName(strNameToUse, m_rbMale.Checked, HebrewCase.Genitive, bVocalized, null)) + " ")
                .Replace(" [שם:D] ", " " + (bNameMissing ? strNameToUse : DeclineHebrewName(strNameToUse, m_rbMale.Checked, HebrewCase.Dative, bVocalized, null)) + " ")
                .Replace(" [שם:A] ", " " + (bNameMissing ? strNameToUse : DeclineHebrewName(strNameToUse, m_rbMale.Checked, HebrewCase.Accusative, bVocalized, null)) + " ");

            // Space-delimited tokens, as in the Russian original. Periods are handled
            // above; separate other punctuation in the prepared template.
            tbxToAdapt.Text = strCurrentText.Trim().Replace(" .", ".");
        }

        //===================================================================================================
        /// <summary>
        /// Optional niqqud, dagesh, shin/sin dot and related point handling.
        /// Matres lectionis are not deleted; alternate consonantal spellings are explicit.
        /// </summary>
        //===================================================================================================
        bool IsHebrewPoint(char c)
        {
            return (c >= '\u05B0' && c <= '\u05BD') || c == '\u05BF' ||
                c == '\u05C1' || c == '\u05C2' || c == '\u05C4' || c == '\u05C5' || c == '\u05C7';
        }

        //===================================================================================================
        /// <summary>
        /// Provides indication if given string has hebrew points
        /// </summary>
        /// <param name="strText">Text for testing</param>
        /// <returns>true iff there are hebrew points</returns>
        //===================================================================================================
        bool HasHebrewPoints(string strText)
        {
            foreach (char c in strText)
            {
                if (IsHebrewPoint(c))
                    return true;
            }
            return false;
        }

        //===================================================================================================
        /// <summary>
        /// Removes hebrew points from the text
        /// </summary>
        /// <param name="strText">Text for devocalization</param>
        /// <returns>Devocalized text</returns>
        //===================================================================================================
        string RemoveHebrewPoints(string strText)
        {
            var oResult = new System.Text.StringBuilder();
            foreach (char c in strText)
            {
                if (!IsHebrewPoint(c))
                    oResult.Append(c);
            }
            return oResult.ToString();
        }

        #endregion

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

            m_chkAddName.Checked = false;
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
                            SaveSchoolReportExtended(
                                m_oTexts, m_oFrequencies, m_oCoOccurences, 
                                m_oGenderReplacements, m_strFilePath);
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
                            SaveSchoolReportExtended(
                                m_oTexts, m_oFrequencies, m_oCoOccurences,
                                m_oGenderReplacements, m_strFilePath);
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
                            SaveSchoolReportExtended(
                                m_oTexts, m_oFrequencies, m_oCoOccurences,
                                m_oGenderReplacements, m_strFilePath);
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

        //===================================================================================================
        /// <summary>
        // Plan / Pseudocode:
        // 1. Read saved font from settings ("TextFont").
        //    - Try to read it as a System.Drawing.Font.
        //    - If not present, try to read it as a serialized string and convert with FontConverter.
        //    - If still not available or empty, create a fallback font: "Cambie", 14pt, Regular.
        // 2. Set m_oFontDialog.Font to the resolved initial font.
        // 3. Show the font dialog. If user clicks OK:
        //    - Obtain the selected font from the dialog.
        //    - Save the font back into settings:
        //        * If the settings property "TextFont" exists and is of type Font, save the Font directly.
        //        * If the settings property "TextFont" exists and is a string, serialize using FontConverter and store the string.
        //        * Otherwise attempt to set via the indexer and fall back to storing a string.
        //    - Call settings.Save() to persist the change.
        // 4. Handle errors quietly (no crash) and keep behavior deterministic.
        //
        // Implementation:
        // - Use System.Drawing.Font and System.Drawing.FontConverter qualified names to avoid adding using lines.
        // - Use reflection to detect typed property "TextFont" on Properties.Settings.Default and choose appropriate storage.
        // - Fallbacks are conservative to maximize compatibility with various settings schemas.
        /// </summary>
        /// <param name="oSender">Sender object</param>
        /// <param name="oArgs">Event args</param>
        //===================================================================================================
        private void OnSelectFontMenuItem_Click(
            object oSender, 
            EventArgs oArgs)
        {
            // Resolve settings instance
            var oSettings = Properties.Settings.Default;
            var oFontConverter = new System.Drawing.FontConverter();

            // Try to get stored value
            object? oStored = null;
            try
            {
                oStored = oSettings["TextFont"];
            }
            catch
            {
                oStored = null;
            }

            // Determine initial font
            System.Drawing.Font oInitialFont;
            if (oStored != null && oStored is System.Drawing.Font oStoredFont)
            {
                oInitialFont = oStoredFont;
            }
            else if (oStored is string sStored && !string.IsNullOrWhiteSpace(sStored))
            {
                try
                {
                    oInitialFont = (System.Drawing.Font)oFontConverter.ConvertFromString(sStored)!;
                }
                catch
                {
                    oInitialFont = new System.Drawing.Font("Segoe UI", 14.25f, System.Drawing.FontStyle.Regular);
                }
            }
            else
            {
                oInitialFont = new System.Drawing.Font("Segoe UI", 14.25f, System.Drawing.FontStyle.Regular);
            }

            // Configure and show dialog
            if (m_oFontDialog != null)
            {
                m_oFontDialog.Font = oInitialFont;

                if (m_oFontDialog.ShowDialog(this) == DialogResult.OK)
                {
                    var selectedFont = m_oFontDialog.Font ?? oInitialFont;

                    // Try strong-typed property first using reflection
                    try
                    {
                        var settingsType = oSettings.GetType();
                        var propInfo = settingsType.GetProperty("TextFont");
                        if (propInfo != null && propInfo.CanWrite)
                        {
                            if (propInfo.PropertyType == typeof(System.Drawing.Font))
                            {
                                propInfo.SetValue(oSettings, selectedFont);
                            }
                            else if (propInfo.PropertyType == typeof(string))
                            {
                                propInfo.SetValue(oSettings, oFontConverter.ConvertToString(selectedFont));
                            }
                            else
                            {
                                // Unknown property type - attempt to set via indexer
                                oSettings["TextFont"] = selectedFont;
                            }
                        }
                        else
                        {
                            // No strong-typed property found - attempt indexer, falling back to string
                            try
                            {
                                oSettings["TextFont"] = selectedFont;
                            }
                            catch
                            {
                                try
                                {
                                    oSettings["TextFont"] = oFontConverter.ConvertToString(selectedFont);
                                }
                                catch
                                {
                                    // swallow - cannot persist font
                                }
                            }
                        }

                        // Persist settings
                        try
                        {
                            oSettings.Save();
                            SetFont();
                        }
                        catch
                        {
                            // ignore save errors
                        }
                    }
                    catch
                    {
                        // ignore all reflection/assignment errors to avoid crashes
                    }
                }
            }

        }

        //===================================================================================================
        /// <summary>
        // Pseudocode / Plan:
        // 1. Obtain settings instance: Properties.Settings.Default.
        // 2. Attempt to read stored value for "TextFont" via indexer in a safe try/catch.
        // 3. Prepare a System.Drawing.FontConverter for converting string representations.
        // 4. Resolve initial font in this order:
        //    a) If stored value is a System.Drawing.Font, use it.
        //    b) Else if stored value is a non-empty string, try ConvertFromString to a Font.
        //    c) Else attempt to select the requested fallback font "Cambia" (exact name) at 14.25pt Regular
        //       but only if that font family is installed (check System.Drawing.FontFamily.Families).
        //    d) If "Cambia" is not available, and the platform is non-Windows (Linux/macOS), try a list of
        //       common serif fonts available on Linux: "DejaVu Serif", "Liberation Serif", "Times New Roman", "Serif".
        //    e) If none of the above succeed, do not change the existing font (leave the textbox as-is).
        // 5. Apply the resolved font to m_tbxCurrentTextCommunity.Font if a font has been resolved.
        // 6. Wrap all steps in try/catch to avoid throwing exceptions (fail silently).
        //
        // Notes:
        // - Use fully-qualified System.Drawing types to avoid adding new using directives.
        // - Keep the size at 14.25f as requested.
        // - Do not persist settings here; this method only reads/applies the font.
        /// </summary>
        //===================================================================================================
        private void SetFont()
        {
            try
            {
                var oSettings = Properties.Settings.Default;
                var oFontConverter = new System.Drawing.FontConverter();

                // Try to read the stored setting safely
                object? oStored = null;
                try
                {
                    oStored = oSettings["TextFont"];
                }
                catch
                {
                    oStored = null;
                }

                System.Drawing.Font? oResolvedFont = null;

                // 1) If stored is already a Font, use it
                if (oStored is System.Drawing.Font oStoredFont)
                {
                    oResolvedFont = oStoredFont;
                }
                else
                // 2) If stored is a string, try to convert it
                if (oStored is string strStored && !string.IsNullOrWhiteSpace(strStored))
                {
                    try
                    {
                        var oConvertedFont = oFontConverter.ConvertFromString(strStored);
                        if (oConvertedFont is System.Drawing.Font oFont)
                            oResolvedFont = oFont;
                    }
                    catch
                    {
                        oResolvedFont = null;
                    }
                }

                // 3) If still null, try the requested default "Cambia" at 14.25pt (only if installed)
                if (oResolvedFont == null)
                {
                    string[] astrPreferredFontNames = new[] { "Segoe UI" }; // requested exact name
                    var aFamilies = System.Drawing.FontFamily.Families;

                    foreach (string strCandidate in astrPreferredFontNames)
                    {
                        try
                        {
                            if (aFamilies.Any(ff => string.Equals(
                                ff.Name, strCandidate, StringComparison.OrdinalIgnoreCase)))
                            {
                                oResolvedFont = new System.Drawing.Font(
                                    strCandidate, 14.25f, System.Drawing.FontStyle.Regular);
                                break;
                            }
                        }
                        catch
                        {
                            // ignore and try next
                        }
                    }
                }

                // 4) Platform-specific fallback: try common serif fonts (useful on Linux)
                if (oResolvedFont == null)
                {
                    // Only attempt additional fallbacks when not on Windows (user asked to try a serif on Linux)
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        string[] astrLinuxSerifs = new[] {
                            "DejaVu Serif", "Liberation Serif", "Times New Roman", "Serif" };

                        var families = System.Drawing.FontFamily.Families;

                        foreach (string strCandidate in astrLinuxSerifs)
                        {
                            try
                            {
                                if (families.Any(ff => string.Equals(
                                    ff.Name, strCandidate, StringComparison.OrdinalIgnoreCase)))
                                {
                                    oResolvedFont = new System.Drawing.Font(
                                        strCandidate, 14.25f, System.Drawing.FontStyle.Regular);
                                    break;
                                }
                            }
                            catch
                            {
                                // ignore and try next
                            }
                        }
                    }
                }

                // 5) Apply resolved font if available; otherwise leave textbox font unchanged
                if (oResolvedFont != null)
                {
                    m_tbxCurrentTextCommunity.Font = oResolvedFont;
                    m_tbxNextBestTextCommunity.Font = oResolvedFont;
                    m_tbxNextWorseTextCommunity.Font = oResolvedFont;
                    m_tbxTextCommunity.Font = oResolvedFont;
                }
            }
            catch
            {
                // ignore
            }
        }

        //===================================================================================================
        /// <summary>
        /// This is executed when the total text for the category changes
        /// </summary>
        /// <param name="oSender">Sender object</param>
        /// <param name="oArgs">Event args</param>
        //===================================================================================================
        private void OnTextCommunity_TextChanged(object oSender, EventArgs oArgs)
        {
            m_oSectionTexts[m_strCurrentSection] = m_tbxTextCommunity.Text;
        }


        //===================================================================================================
        /// <summary>
        /// This is executed when user wants to switch to next pupil
        /// </summary>
        /// <param name="oSender">Sender object</param>
        /// <param name="oArgs">Event args</param>
        //===================================================================================================
        private void OnNextToolStripMenuItem_Click(object oSender, EventArgs oArgs)
        {
            m_oSectionTexts = new Dictionary<string, string>();
            m_tbxName.Text = string.Empty;
            m_tbxTextCommunity.Text = string.Empty;
            m_ctlReportParts.SelectedIndex = 0;
            m_ctlUpDownPoints.Value = 0;
        }
    }

}

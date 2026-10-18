using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace School_Report
{

    //*******************************************************************************************************
    /// <summary>
    /// A configurable gender-dependent text replacement.
    /// </summary>
    //*******************************************************************************************************
    public sealed class GenderReplacement
    {
        public string GenderIndependent { get; set; } = "";
        public string MaleForm { get; set; } = "";
        public string FemaleForm { get; set; } = "";

        //===================================================================================================
        /// <summary>
        /// Applies externally supplied rules in their listed order.
        /// Matching is literal and case-sensitive, using string.Replace.
        /// Include spaces or delimiters in the configured strings when required.
        /// </summary>
        //===================================================================================================
        public static string Apply(
            string strText, 
            IEnumerable<GenderReplacement> iRules,
            bool bIsMale
            )
        {
            if (strText == null)
                throw new ArgumentNullException(nameof(strText));
            if (iRules == null)
                throw new ArgumentNullException(nameof(iRules));

            foreach (GenderReplacement oRule in iRules)
            {
                if (oRule == null)
                    throw new ArgumentException("A replacement rule must not be null.", nameof(iRules));
                if (string.IsNullOrEmpty(oRule.GenderIndependent))
                    throw new ArgumentException("A search string must not be empty.", nameof(iRules));

                string strReplacement = bIsMale ? oRule.MaleForm : oRule.FemaleForm;
                if (strReplacement == null)
                    throw new ArgumentException("A selected replacement must not be null.", nameof(iRules));

                strText = strText.Replace(oRule.GenderIndependent, strReplacement);
            }

            return strText;
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.bodurov.WpfControls;
using System.Windows.Documents;
using System.Text.RegularExpressions;
using System.Windows.Media;


namespace EffectEditor
{
	public class MSCProcesser : IParagraphProcessor
	{

		private static readonly Regex _splitRegex = new Regex(@"(\s|\(|\)|\+|\-|\%|\*|\[|\]|/)", RegexOptions.Compiled);
        #region Keywords
        private static readonly HashSet<string> _keyWords =
            new HashSet<string>(StringComparer.CurrentCultureIgnoreCase)
                {
                    "state",
					"label",
					"goto",
					"if",
					"loop",
					"repeat",
					"var",
					"varg",
					"blank",

                };
        #endregion

        #region Functions
        private static readonly HashSet<string> _functions =
            new HashSet<string>(StringComparer.CurrentCultureIgnoreCase)
                {
                    "make",
                };
        #endregion


        public Regex SplitWordsRegex { get { return _splitRegex; } }
        public int GetWordTypeID(string word)
        {
            if (_keyWords.Contains(word))
            {
                return 1;
            }
            if (_functions.Contains(word))
            {
                return 2;
            }
            if (word.StartsWith(";"))
            {
                return 3;
            }
            return 0;
        }
        public Inline FormatInlineForID(Inline inline, int id)
        {
            if (id == 1)
            {
                inline.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0xFF));
            }
            else if (id == 2)
            {
                inline.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x00, 0xFF));
            }
            else if (id == 3)
            {
                inline.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0x99, 0xCC));
            }
            return inline;
        }

    }
}

using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Eto.UnitTest
{
    class KeywordFilter : BaseFilter
    {
        string keywords;
        string[][] keywordTokens;

        string[] SplitMatches(string value, string regex) => Regex.Matches(value, regex).OfType<Match>().Select(r => r.Value).ToArray();

        /// <summary>
        /// Gets or sets the keyword string to search for.
        /// </summary>
        /// <remarks>
        /// Supports:
        ///	  - '-' prefix to exclude keyword
        ///   - Quotes for literal matches e.g. "my test"
        ///   - Multiple keywords separated by whitespace
        /// </remarks>
        /// <value>The keywords.</value>
        public string Keywords
        {
            get => keywords;
            set
            {
                keywords = value;
                if (string.IsNullOrWhiteSpace(value))
                    keywordTokens = null;
                else
                {
                    var searches = SplitMatches(value, @"([-]?""[^""]*"")|((?<=[\s]|^)[^""\s]+(?=[\s]|$))");
                    keywordTokens = searches
                        .Select(s => SplitMatches(s, @"(^-)|((?<=^-?"").+(?=""$))|([A-Z][^A-Z]*[^A-Z""]?)|((?<!^-"")[^A-Z""][^A-Z]*[^A-Z""])|\w+"))
                        .ToArray();
                    if (keywordTokens.Length == 1 && keywordTokens[0].Length == 1 && keywordTokens[0][0] == keywords)
                        keywordTokens = null;
                }
            }
        }

        protected override bool Matches(ITest test, bool parent)
        {
            if (string.IsNullOrEmpty(keywords))
                return true;

            var name = test.FullName;
            if (name == null)
                return true;
            if (keywordTokens != null)
            {
                // poor man's search algo
                bool lastIsUpper = false;
                for (int i = 0; i < keywordTokens.Length; i++)
                {
                    var search = keywordTokens[i];
                    int index = 0;
                    bool inverse = false;

                    for (int j = 0; j < search.Length; j++)
                    {
                        var kw = search[j];
                        if (!inverse && j == 0 && kw.Length == 1 && kw[0] == '-')
                        {
                            if (search.Length == 1) // just a '-', which is invalid
                                break;
                            if (parent) // only match inverse expressions on test itself or its children.
                                return false;
                            inverse = true;
                            continue;
                        }

                        var isUpper = kw.Length == 1 && char.IsUpper(kw[0]);
                        var idx = name.IndexOf(kw, index, isUpper ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
                        if (idx == -1)
                        {
                            if (lastIsUpper && isUpper && char.ToUpper(name[index]) == kw[0])
                                index++;
                            else if (inverse)
                            {
                                inverse = false;
                                break;
                            }
                            else
                                return !parent && inverse;
                        }
                        else
                            index = idx + kw.Length;

                        lastIsUpper = isUpper;
                    }
                    if (inverse)
                        return false;
                }
                return true;
            }
            return name.IndexOf(keywords, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}

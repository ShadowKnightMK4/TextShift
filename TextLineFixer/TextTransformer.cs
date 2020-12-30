using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TextShift
{
    /// <summary>
    /// We use the reg expressions to transform a string based on flag setting.
    /// </summary>
    public class TextTransformer
    {
        #region Public Window


        /// <summary>
        /// how to handle whitespaces that aren't \r\n varients
        /// </summary>
        public enum SpaceStyle
        {
            /// <summary>
            /// No special handling
            /// </summary>
            None = 0,
            /// <summary>
            /// Repeated non \r\n Whitespaces are dropped to a single whitespace of ' ' (32)
            /// </summary>
            ForceSingle = 1
        }

        /// <summary>
        /// How to transform line endings
        /// </summary>
        public enum LineStyle
        {
            /// <summary>
            /// No special line handling
            /// </summary>
            None  = 0,
            /// <summary>
            /// Lone '\r' and '\n' will be replaced with '\r\n'
            /// </summary>
            WindowsStyle = 1,

            /// <summary>
            /// '\r\n', '\n' will be replaced with \n
            /// </summary>
            UnixStyle = 2,

            /// <summary>
            /// Same as Unix
            /// </summary>
            MacStyle = UnixStyle,



        }

        /// <summary>
        /// Controls what to do with line endings of '\r', '\n' and '\r\n'
        /// </summary>
        public LineStyle LinePreference = LineStyle.WindowsStyle;

        /// <summary>
        /// Controls what to do with whitespaces that aren't line ending related
        /// </summary>
        public SpaceStyle SpacePreference = SpaceStyle.ForceSingle;
        

        
        #endregion

        /// <summary>
        /// the regex we use needs a rider with what re replace with and out we replace
        /// </summary>
        private class ReplaceRegEx : Regex
        {
            public ReplaceRegEx(string Pattern, RegexOptions Options, string ReplaceStr):base(Pattern, Options)
            {
                ReplaceWith = ReplaceStr;
                ReplaceHow = ReplaceType.Fixed;
            }

            public override string ToString()
            {
                return   base.ToString();
            }

            public enum ReplaceType
            {
                /// <summary>
                /// Default. Any match gets the Replacement string regardles.
                /// </summary>
              Fixed = 0,
              /// <summary>
              /// The matched part is sliced out and the replacement string is repeated and instereted.
              /// </summary>

              LengthMatch = 1
            }
            public ReplaceType ReplaceHow;
            public string ReplaceWith;
        }


        /// <summary>
        /// Convert the public settings to the internal represnation
        /// </summary>
        private void ConvertConfigToSetting()
        {
            switch (LinePreference)
            {
                case LineStyle.WindowsStyle:
                    AddNewLineWindowsFix();
                    break;
                case LineStyle.UnixStyle:
                    AddNewLineUnixFix();
                    break;
                case LineStyle.None:
                    break;
                default:
                    throw new NotImplementedException();
            }

            switch (SpacePreference)
            {
                case SpaceStyle.None:
                    break;
                case SpaceStyle.ForceSingle:
                    AddSpaceFix();
                    break;
                default: throw new NotImplementedException();
            }

        }

        #region pass list and creating RegEx passes

        private readonly List<Regex> Passes = new List<Regex>();

        bool ChangedLast = false;



        /// <summary>
        /// add passes to transform line inding to '\n'
        /// </summary>
        /// <param name="ReplaceLine"></param>
        private void AddNewLineUnixFix(string ReplaceLine="\n")
        {
            ReplaceRegEx Pass1 = new ReplaceRegEx("\r", RegexOptions.IgnoreCase, ReplaceLine);
            ReplaceRegEx Pass2 = new ReplaceRegEx("\r\n", RegexOptions.IgnoreCase, ReplaceLine);
            ChangedLast = true;
            Passes.Add(Pass2);
            Passes.Add(Pass1);
        }
        /// <summary>
        /// add regex to the Pass list to fix solo \r and \n for windows
        /// </summary>
        private void AddNewLineWindowsFix(string ReplaceLine="\r\n")
        {
            ReplaceRegEx Pass1 = new ReplaceRegEx("\r[^\n]", RegexOptions.IgnoreCase , ReplaceLine);
            ReplaceRegEx Pass2 = new ReplaceRegEx("[^\r]\n", RegexOptions.IgnoreCase, ReplaceLine);
            ChangedLast = true;
            Passes.Add(Pass2);
            Passes.Add(Pass1);

        }


        /// <summary>
        /// 
        /// </summary>
        private void AddSpaceFix()
        {
            ReplaceRegEx Pass1 = new ReplaceRegEx("\\s{2,}", RegexOptions.IgnoreCase, " ");
            Passes.Add(Pass1);
            Pass1.ReplaceHow = ReplaceRegEx.ReplaceType.LengthMatch;
        }


        /// <summary>
        /// This won't work in .net Core
        /// </summary>
        /// <param name="Target"></param>
        public void ExportCompiledAssembly(string Target)
        {
            throw new NotSupportedException();
        }
        #endregion


        /// <summary>
        /// Apply this <see cref="TextTransformer"/>'s settting to this string and return the result
        /// </summary>
        /// <param name="target"></param>
        /// <returns>return the string after applying the settings</returns>
        public string Apply(string target)
        {
            if ( (ChangedLast) || (Passes.Count == 0))
            {
                ConvertConfigToSetting();
                
                ChangedLast = false;
            }

            foreach (ReplaceRegEx Pass in Passes)
            {
                target = Pass.Replace(target, p => {
                    if (Pass.ReplaceHow == ReplaceRegEx.ReplaceType.Fixed)
                    {
                        return Pass.ReplaceWith;
                    }
                    else
                    {
                        if (Pass.ReplaceHow == ReplaceRegEx.ReplaceType.LengthMatch)
                        {
                            if (p.Length != Pass.ReplaceWith.Length)
                            {
                                if (Pass.ReplaceWith.Length == 0)
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    string ret;
                                    ret = string.Empty;
                                    while (ret.Length < Pass.ReplaceWith.Length)
                                    {
                                        ret += Pass.ReplaceWith;
                                    }
                                    return ret;
                                }
                            }
                            else
                            {
                                return Pass.ReplaceWith;
                            }
                        }
                        throw new NotImplementedException();
                    }
                });
            }

            return target;
        }


    }
}

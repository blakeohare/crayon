using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /*
        Sorts two strings taking numerical components into consideration and sorting by value, rather than
        by string characters.

        For example the string "2.1.10" will be sorted higher than "2.1.2" because 10 > 2
    */
    public class VersionComparator : IComparer<string>
    {
        // The parsing out of the components is the heaviest part, and there are lots of duplicate comparisons per
        // list sort, so cache the strings on a per-comparator basis.
        private Dictionary<string, object[]> parsedCache = new Dictionary<string, object[]>();

        public int Compare(string a, string b)
        {
            object[] aPieces = GetPieces(a);
            object[] bPieces = GetPieces(b);

            int aLength = aPieces.Length;
            int bLength = bPieces.Length;
            int length = System.Math.Max(aLength, bLength);
            for (int i = 0; i <= length; ++i)
            {
                if (i >= aLength && i >= bLength) return 0;
                if (i >= aPieces.Length) return 1;
                if (i >= bPieces.Length) return -1;

                object aCur = aPieces[i];
                object bCur = bPieces[i];
                if (aCur is int && bCur is string) return -1;
                else if (aCur is string && bCur is int) return 1;
                else if (aCur is int && bCur is int)
                {
                    int aInt = (int)aCur;
                    int bInt = (int)bCur;
                    if (aInt != bInt)
                    {
                        return (int)aCur < (int)bCur ? -1 : 1;
                    }
                }
                else
                {
                    string aStr = (string)aCur;
                    string bStr = (string)bCur;
                    if (aStr != bStr)
                    {
                        return aStr.CompareTo(bStr);
                    }
                }
            }
            return 0;
        }

        private object[] GetPieces(string value)
        {
            object[] output = null;
            if (parsedCache.TryGetValue(value, out output))
            {
                return output;
            }

            value = value.Trim().ToLower();
            if (value.Length == 0) return new object[0];
            List<object> pieces = new List<object>();
            StringBuilder currentString = new StringBuilder();
            int currentInt = 0;
            char currentType = '?';
            char c;
            bool isInt = false;
            for (int i = 0; i < value.Length; ++i)
            {
                c = value[i];
                isInt = (c >= '0' && c <= '9');
                if (currentType == '?')
                {
                    if (isInt)
                    {
                        currentInt = (int)(c - '0');
                        currentType = 'i';
                    }
                    else
                    {
                        currentString.Clear();
                        currentString.Append(c);
                        currentType = 's';
                    }
                }
                else if (isInt)
                {
                    if (currentType == 'i')
                    {
                        currentInt = currentInt * 10 + (int)(c - '0');
                    }
                    else
                    {
                        pieces.Add(currentInt);
                        currentType = '?';
                        --i;
                    }
                }
                else
                {
                    if (currentType == 's')
                    {
                        currentString.Append(c);
                    }
                    else
                    {
                        string s = currentString.ToString().Trim();
                        if (s.Length > 0)
                        {
                            pieces.Add(s);
                        }
                        currentString.Clear();
                        currentType = '?';
                        --i;
                    }
                }
            }

            if (currentType == 's')
            {
                string s = currentString.ToString().Trim();
                if (s.Length > 0)
                {
                    pieces.Add(s);
                }
            }
            else
            {
                pieces.Add(currentInt);
            }
            return pieces.ToArray();
        }
    }
}

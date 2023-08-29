using LibRedminePower.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace RedmineTimePuncher.Extentions
{
    public class SearchPatern
    {
        public static bool Check(string searchText, string target)
        {
            var ors = new List<string>();
            var ands = new List<string>();
            var exs = new List<string>();
            var mc = Regex.Matches(searchText, "\".*\"");
            if(mc.Count > 0)
            {
                foreach(var temp in mc.Cast<object>().Select(a => a.ToString()))
                {
                    var myResult = temp.Replace("\"", "");
                    var index = searchText.IndexOf(temp);
                    if(index > 0)
                    {
                        var optChar = searchText.Substring(index - 1, 1);
                        if (optChar == "+")
                            ands.Add(myResult);
                        else if (optChar == "-")
                            exs.Add(myResult);
                        else
                            ors.Add(myResult);
                        searchText = searchText.Replace(optChar + temp, "");
                    }
                    else
                    {
                        ors.Add(myResult);
                        searchText = searchText.Replace(temp, "");
                    }
                }
            }

            // OR検索が一つでもヒットするか？
            var texts = new List<string>();
            if(!string.IsNullOrEmpty(searchText))
                texts.AddRange(searchText.Split(' ').Select(b => b.Trim()));
            ors.AddRange(texts.Where(a => !a.StartsWith("-") && !a.StartsWith("+")).ToList());
            if (!ors.Any() || ors.Any(a => target.Contains(a)))
            {
                // AND検索が全てヒットするか？
                ands.AddRange(texts.Where(a => a.StartsWith("+")).ToList());
                if (!ands.Any() || ands.All(a => target.Contains(a.Substring(1))))
                {
                    // 除外検索がすべて含まないか？
                    exs.AddRange(texts.Where(a => a.StartsWith("-")).ToList());
                    if(!exs.Any() || exs.All(a => !target.Contains(a.Substring(1))))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

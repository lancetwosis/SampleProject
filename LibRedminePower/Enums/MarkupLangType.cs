using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Enums
{
    public enum MarkupLangType
    {
        // システム書式のチェックが未実行の場合、Undefined とする
        Undefined = -1,
        None,
        Textile,
        Markdown
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Visualize.Factors
{
    public enum FactorValueType
    {
        None,
        Date,
        Issue,
        Project,
        User,
        Category,
        ASC,
        DESC,
        Center,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        OnTime,
        IssueCustomField,
        FixedVersion,
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Visualize.Factors
{
    public static class FactorTypes
    {
        public static FactorType None         = new FactorType(FactorValueType.None);
        public static FactorType Date         = new FactorType(FactorValueType.Date);
        public static FactorType Issue        = new FactorType(FactorValueType.Issue);
        public static FactorType Project      = new FactorType(FactorValueType.Project);
        public static FactorType User         = new FactorType(FactorValueType.User);
        public static FactorType Category     = new FactorType(FactorValueType.Category);
        public static FactorType ASC          = new FactorType(FactorValueType.ASC);
        public static FactorType DESC         = new FactorType(FactorValueType.DESC);
        public static FactorType Center       = new FactorType(FactorValueType.Center);
        public static FactorType TopLeft      = new FactorType(FactorValueType.TopLeft);
        public static FactorType TopRight     = new FactorType(FactorValueType.TopRight);
        public static FactorType BottomLeft   = new FactorType(FactorValueType.BottomLeft);
        public static FactorType BottomRight  = new FactorType(FactorValueType.BottomRight);
        public static FactorType OnTime       = new FactorType(FactorValueType.OnTime);
        public static FactorType FixedVersion = new FactorType(FactorValueType.FixedVersion);

        public static List<FactorType> CustomFields = new List<FactorType>();

        public static void SetCustomFields(ResultModel model)
        {
            CustomFields.Clear();
            CustomFields.AddRange(model.CustomFields.Where(c => c.CanUseVisualizeFactor())
                                              .Select(cf => new FactorType(cf, model))
                                              .ToList());
        }

        public static FactorType[] GetGroupings()
        {
            return new FactorType[] 
            {
                FactorTypes.Issue,
                FactorTypes.Project,
                FactorTypes.Date,
                FactorTypes.User,
                FactorTypes.Category,
                FactorTypes.FixedVersion,
                FactorTypes.OnTime
            };
        }

        public static FactorType[] Get2ndGroupings()
        {
            var fs = GetGroupings().ToList();
            fs.Insert(0, FactorTypes.None);
            return fs.ToArray();
        }

        public static FactorType[] GetSortDirections()
        {
            return new FactorType[]
            {
                FactorTypes.None,
                FactorTypes.ASC,
                FactorTypes.DESC
            };
        }
    }
}

using LibRedminePower.Applications;
using System.Collections.Generic;
using Telerik.Windows.Controls;

namespace LibRedminePower.Localization
{
    public class MyLocalizationManager : LocalizationManager
    {
        private IDictionary<string, string> dictionary;

        public MyLocalizationManager()
        {
            this.dictionary = new Dictionary<string, string>();

            this.dictionary["Pivot_Row"] = "行";
            this.dictionary["Pivot_Column"] = "列";
            this.dictionary["Pivot_Value"] = "値: {0}";

            this.dictionary["PivotFieldList_ShowItemsForWhichTheLabel"] = "Show items for which the label";
            this.dictionary["PivotFieldList_And"] = "and";
            this.dictionary["PivotFieldList_AscendingBy"] = "Ascending (A to Z) by:";
            this.dictionary["PivotFieldList_BaseField"] = "Base field:";
            this.dictionary["PivotFieldList_BaseItem"] = "Base item:";
            this.dictionary["PivotFieldList_By"] = "by:";
            this.dictionary["PivotFieldList_ChooseAggregateFunction"] = "集計に使用する計算の種類を選択してください。";
            this.dictionary["PivotFieldList_ChooseFieldsToAddToReport"] = "レポートに追加するフィールドを選択してください：";
            this.dictionary["PivotFieldList_ColumnLabels"] = "列";
            this.dictionary["PivotFieldList_DeferLayoutUpdate"] = "レイアウトの更新を保留する";
            this.dictionary["PivotFieldList_DescendingBy"] = "Descending (Z to A) by:";
            this.dictionary["PivotFieldList_DragFieldsBetweenAreasBelow"] = "次のボックス間でフィールドをドラッグしてください：";
            this.dictionary["PivotFieldList_Format"] = "format:";
            this.dictionary["PivotFieldList_GeneralFormat"] = "General Format";
            this.dictionary["PivotFieldList_IgnoreCase"] = "大文字/小文字を無視する";
            this.dictionary["PivotFieldList_PleaseRefreshThePivot"] = "Please refresh the pivot.";
            this.dictionary["PivotFieldList_Refresh"] = "更新";
            this.dictionary["PivotFieldList_ReportFilter"] = "フィルタ";
            this.dictionary["PivotFieldList_RowLabels"] = "行";
            this.dictionary["PivotFieldList_SelectItem"] = "アイテムを選択";
            this.dictionary["PivotFieldList_Show"] = "Show";
            this.dictionary["PivotFieldList_ShowItemsForWhich"] = "PivotFieldList_ShowItemsForWhich";
            this.dictionary["PivotFieldList_ShowValuesAs"] = "Show Values As";
            this.dictionary["PivotFieldList_SortOptions"] = "Sort options";
            this.dictionary["PivotFieldList_StringFormatDescription"] = "The format should identify the measurement type of the value. The format would be used for general computations such as Sum, Average, Min, Max and others.";
            this.dictionary["PivotFieldList_SummarizeValuesBy"] = "集計方法";
            this.dictionary["PivotFieldList_TheActionRequiresMoreRecentInformation"] = "The action requires more recent information.";
            this.dictionary["PivotFieldList_Update"] = "更新";
            this.dictionary["PivotFiledList_Values"] = "値";

            this.dictionary["Ok"] = "OK";
            this.dictionary["Cancel"] = "キャンセル";
            this.dictionary["Clear"] = "クリア";
            this.dictionary["Close"] = "閉じる";
            this.dictionary["Maximize"] = "最大化";
            this.dictionary["Minimize"] = "最小化";
            this.dictionary["Restore"] = "元に戻す（縮小）";

            this.dictionary["Untitled"] = "未設定";
            this.dictionary["Subject"] = "題名";
            this.dictionary["Body"] = "説明";

            this.dictionary["PivotFieldList_SetSumAggregate"] = "Sum";
            this.dictionary["PivotFieldList_SetCountAggregate"] = "Count";
            this.dictionary["PivotFieldList_SetAverageAggregate"] = "Average";
            this.dictionary["PivotFieldList_SetIndexTotalFormat"] = "Index";
            this.dictionary["PivotFieldList_SetPercentOfGrandTotalFormat"] = "% of Grand Total";
            this.dictionary["PivotFieldList_SortAtoZ"] = "Sort A to Z";
            this.dictionary["PivotFieldList_SortZtoA"] = "Sort Z to A";
            this.dictionary["PivotFieldList_MoreSortingOptions"] = "並び替えオプション...";
            this.dictionary["PivotFieldList_LabelFilter"] = "ラベル フィルタ";
            this.dictionary["PivotFieldList_ValueFilter"] = "値 フィルタ";
            this.dictionary["PivotFieldList_TopTenFilter"] = "トップ10 フィルタ";
            this.dictionary["PivotFieldList_ClearFilter"] = "フィルタをクリア";
            this.dictionary["PivotFieldList_ShowEmptyGroups"] = "Show Empty Groups";
            this.dictionary["PivotFieldList_ShowSubTotals "] = "Show Subtotals";
            this.dictionary["PivotFieldList_SelectItems"] = "アイテムを選択";
            this.dictionary["PivotFieldList_MoreAggregateOptions"] = "集計オプション...";
            this.dictionary["PivotFieldList_MoreCalculationOptions"] = "計算オプション...";
            this.dictionary["PivotFieldList_ClearCalculations"] = "Clear Calculations";
            this.dictionary["PivotFieldList_NumberFormat"] = "Number Format";

            this.dictionary["Pivot_GrandTotal"] = "総計";
            this.dictionary["Pivot_Values"] = "値";
            this.dictionary["Pivot_Error"] = "Error";
            this.dictionary["Pivot_TotalP0"] = "Total {0}";
            this.dictionary["Pivot_P0Total"] = "{0} Total";

            this.dictionary["Pivot_GroupP0AggregateP1"] = "{0} {1}";

            this.dictionary["Pivot_AggregateP0ofP1"] = "{0} / {1}";

            this.dictionary["Pivot_AggregateSum"] = "合計";
            this.dictionary["Pivot_AggregateCount"] = "個数";
            this.dictionary["Pivot_AggregateAverage"] = "平均";
            this.dictionary["Pivot_AggregateMin"] = "最小";
            this.dictionary["Pivot_AggregateMax"] = "最大";
            this.dictionary["Pivot_AggregateProduct"] = "積";
            this.dictionary["Pivot_AggregateVar"] = "分散";
            this.dictionary["Pivot_AggregateVarP"] = "標本分散";
            this.dictionary["Pivot_AggregateStdDev"] = "標準偏差";
            this.dictionary["Pivot_AggregateStdDevP"] = "標本標準偏差";
            this.dictionary["Pivot_HourGroup"] = "{0} - 時間";
            this.dictionary["Pivot_MinuteGroup"] = "{0} - 分";
            this.dictionary["Pivot_MonthGroup"] = "{0} - 月";
            this.dictionary["Pivot_QuarterGroup"] = "{0} - 四半期";
            this.dictionary["Pivot_SecondGroup"] = "{0} - 秒";
            this.dictionary["Pivot_WeekGroup"] = "{0} - 週";
            this.dictionary["Pivot_YearGroup"] = "{0} - 年";
            this.dictionary["Pivot_DayGroup"] = "{0} - 日";
            this.dictionary["PivotFieldList_PercentOfColumnTotal"] = "% of Column Total";
            this.dictionary["PivotFieldList_PercentOfRowTotal"] = "% of Row Total";
            this.dictionary["PivotFieldList_PercentOfGrandTotal"] = "% of Grand Total";
            this.dictionary["PivotFieldList_NoCalculation"] = "No Calculation";
            this.dictionary["PivotFieldList_DifferenceFrom"] = "Difference From";
            this.dictionary["PivotFieldList_PercentDifferenceFrom"] = "% Difference From";
            this.dictionary["PivotFieldList_PercentOf"] = "% Of";
            this.dictionary["PivotFieldList_RankSmallestToLargest"] = "Rank Smallest to Largest";
            this.dictionary["PivotFieldList_RankLargestToSmallest"] = "Rank Largest to Smallest";
            this.dictionary["PivotFieldList_PercentRunningTotalIn"] = "% Running Total In";
            this.dictionary["PivotFieldList_RunningTotalIn"] = "Running Total In";
            this.dictionary["PivotFieldList_Index"] = "Index";

            this.dictionary["PivotFieldList_RelativeToPrevious"] = "(previous)";
            this.dictionary["PivotFieldList_RelativeToNext"] = "(next)";

            this.dictionary["PivotFieldList_ConditionEquals"] = "等しい";
            this.dictionary["PivotFieldList_DoesNotEqual"] = "等しくない";
            this.dictionary["PivotFieldList_IsGreaterThan"] = "より大きい";
            this.dictionary["PivotFieldList_IsGreaterThanOrEqualTo"] = "以上";
            this.dictionary["PivotFieldList_IsLessThan"] = "より小さい";
            this.dictionary["PivotFieldList_IsLessThanOrEqualTo"] = "以下";
            this.dictionary["PivotFieldList_BeginsWith"] = "始まる";
            this.dictionary["PivotFieldList_DoesNotBeginWith"] = "始まらない";
            this.dictionary["PivotFieldList_EndsWith"] = "終わる";
            this.dictionary["PivotFieldList_DoesNotEndWith"] = "終わらない";
            this.dictionary["PivotFieldList_Contains"] = "含む";
            this.dictionary["PivotFieldList_DoesNotContains"] = "含まない";
            this.dictionary["PivotFieldList_IsBetween"] = "範囲内";
            this.dictionary["PivotFieldList_IsNotBetween"] = "範囲外";

            this.dictionary["PivotFieldList_SelectAll"] = "(全て選択)";

            this.dictionary["PivotFieldList_Top10Items"] = "Items";
            this.dictionary["PivotFieldList_Top10Percent"] = "Percent";
            this.dictionary["PivotFieldList_Top10Sum"] = "Sum";

            this.dictionary["PivotFieldList_TopItems"] = "Top";
            this.dictionary["PivotFieldList_BottomItems"] = "Bottom";

            this.dictionary["PivotFieldList_FilterItemsP0"] = "フィルタ ({0})";
            this.dictionary["PivotFieldList_LabelFilterP0"] = "ラベル フィルタ({0})";
            this.dictionary["PivotFieldList_SortP0"] = "並び替え ({0})";
            this.dictionary["PivotFieldList_FormatCellsP0"] = "Format Cells ({0})";
            this.dictionary["PivotFieldList_ValueSummarizationP0"] = "{0} の 集計方法";
            this.dictionary["PivotFieldList_Top10FilterP0"] = "トップ10 フィルタ ({0})";
            this.dictionary["PivotFieldList_ShowValuesAsP0"] = "Show Values As ({0})";
            this.dictionary["PivotFieldList_ValueFilterP0"] = "値 フィルタ ({0})";
            this.dictionary["PivotFieldList_Null"] = "(null)";

            this.dictionary["Pivot_CalculatedFields"] = "Calculated Fields";
            this.dictionary["PivotFieldList_ItemFilterConditionCaption"] = "以下の条件を満たす値を表示する";
            this.dictionary["PivotFieldList_None"] = "Data source order";
            this.dictionary["PivotFieldList_Sort_BySortKeys"] = "by Sort Keys";

            this.dictionary["DeleteItem"] = ApplicationInfo.Title;
            this.dictionary["DeleteItemQuestion"] = "指定された予定を削除しますか？";

            // https://docs.telerik.com/devtools/wpf/controls/radgridview/localization/localization2
            this.dictionary["GridViewFilterSelectAll"] = "すべて選択";
            this.dictionary["GridViewFilterShowRowsWithValueThat"] = "行の表示条件の指定";
            this.dictionary["GridViewFilterAnd"] = "かつ";
            this.dictionary["GridViewFilterOr"] = "または";
            this.dictionary["GridViewFilterIsEqualTo"] = "指定の値に等しい";
            this.dictionary["GridViewFilterIsNotEqualTo"] = "指定の値に等しくない";
            this.dictionary["GridViewFilter"] = "フィルタ";
            this.dictionary["GridViewClearFilter"] = "クリア";
            this.dictionary["GridViewAlwaysVisibleNewRow"] = "新しい行の追加";
            this.dictionary["GridViewSearchPanelTopText"] = "検索文字列：";

            // https://docs.telerik.com/devtools/wpf/controls/radribbonview/localization
            this.dictionary["RibbonWindowClose"] = "閉じる";
            this.dictionary["RibbonWindowMaximize"] = "最大化";
            this.dictionary["RibbonWindowMinimize"] = "最小化";
            this.dictionary["RibbonWindowRestoreDown"] = "元に戻す（縮小）";
            this.dictionary["RibbonViewMinimizeRibbon"] = "リボンを折りたたむ（Ctrl + F1）";
            this.dictionary["RibbonViewExpandRibbon"] = "リボンの固定（Ctrl + F1）";
            this.dictionary["RibbonViewWindowTitleDivider"] = "  -  ";
        }

        public override string GetStringOverride(string key)
        {
            if (this.dictionary.TryGetValue(key, out var localized))
                return localized;
            return base.GetStringOverride(key);
        }
    }
}

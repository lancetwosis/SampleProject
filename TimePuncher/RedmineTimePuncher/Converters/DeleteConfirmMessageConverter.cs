using RedmineTimePuncher.Models;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Converters
{
    public class DeleteConfirmMessageConverter : LibRedminePower.Converters.Bases.ConverterBase<List<MyAppointment>, string>
    {
        public override string Convert(List<MyAppointment> value, object parameter, CultureInfo culture)
        {
            // 予定を選択せずに右上の「x」ボタンをクリックすると SelectedAppointments が 0 の状態でこのパスに入る
            // よって以下の処理を行う
            var selectedCount = value.Count != 0 ? value.Count : 1;
            return string.Format(Resources.msgConfDeleteSelectedAppointments, selectedCount);
        }

        public override List<MyAppointment> ConvertBack(string value, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

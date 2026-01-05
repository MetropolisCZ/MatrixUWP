using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using static MatrixUWP.ApiWebKlient;

namespace MatrixUWP
{
    public class KonvertorJedenChatTextZpravy : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Dictionary<string, object> contentJedneZpravy = value as Dictionary<string, object>;

            return ZiskatHodnotuDictionary(contentJedneZpravy, "body") ?? "Jiný druh zprávy";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

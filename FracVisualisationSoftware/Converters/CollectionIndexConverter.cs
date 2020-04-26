using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using OfficeOpenXml;

namespace FracVisualisationSoftware.Converters
{
    public class CollectionIndexConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || !(values[0] as IEnumerable<object>).Any() || !((int?) values[1]).HasValue || ((int?) values[1]).Value == -1) return null;

            List<object> objects = new List<object>(values[0] as IEnumerable<object>);

            return objects[((int?)values[1]).Value];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

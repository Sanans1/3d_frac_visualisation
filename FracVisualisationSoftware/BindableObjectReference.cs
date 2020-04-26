using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FracVisualisationSoftware
{
    public class BindableObjectReference : DependencyObject
    {
        public object Object
        {
            get { return GetValue(ObjectProperty); }
            set { SetValue(ObjectProperty, value); }
        }

        public static readonly DependencyProperty ObjectProperty =
            DependencyProperty.Register("Object", typeof(object),
                typeof(BindableObjectReference), new PropertyMetadata(null));
    }
}

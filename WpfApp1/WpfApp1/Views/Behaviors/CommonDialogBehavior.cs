using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1.Views.Behaviors
{
    internal class CommonDialogBehavior
    {
        public static readonly DependencyProperty CallbackProperty = DependencyProperty.RegisterAttached(
            "Callback",
            typeof(Action<bool, string>),
            typeof(CommonDialogBehavior),
            new PropertyMetadata(null));

        public static Action<bool, string> GetCallback(DependencyObject target)
        {
            return (Action<bool, string>)target.GetValue(CallbackProperty);
        }

        public static void SetCallback(DependencyObject target, Action<bool, string> value)
        {
            target.SetValue(CallbackProperty, value);
        }
    }
}

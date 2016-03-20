using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.Views
{
    public static class Behaviours
    {
        #region DandBehaviour
        public static readonly DependencyProperty DandBehaviourProperty =
            DependencyProperty.RegisterAttached("DandBehaviour", typeof(ICommand), typeof(Behaviours),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.None,
                    OnDandBehaviourChanged));
        public static ICommand GetDandBehaviour(DependencyObject d)
        {
            return (ICommand)d.GetValue(DandBehaviourProperty);
        }
        public static void SetDandBehaviour(DependencyObject d, ICommand value)
        {
            d.SetValue(DandBehaviourProperty, value);
        }
        private static void OnDandBehaviourChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var txtbox = d as TextBox;
            if (txtbox == null)
                throw new ApplicationException("Non textbox");
             
            txtbox.Drop += (s, a) =>
            {
                ICommand iCommand = GetDandBehaviour(d);
                if (iCommand != null)
                {
                    if (iCommand.CanExecute(a.Data))
                    {
                        iCommand.Execute(a.Data);
                    }
                }
            };
        }
        #endregion
    }
}

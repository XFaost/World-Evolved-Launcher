using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace NFS
{
    public class AtPropPBox //: INotifyPropertyChanged, IMultiValueConverter
    {
        public static readonly DependencyProperty IsPasswordNotNullProperty =
            DependencyProperty.RegisterAttached("IsPasswordNotNull", typeof(string), typeof(AtPropPBox),
            new FrameworkPropertyMetadata((o, e) =>
            {
                (o as PasswordBox).PasswordChanged -= AtPropPBox_PasswordChanged;
                (o as PasswordBox).PasswordChanged += AtPropPBox_PasswordChanged;
            }));

        private static void AtPropPBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var pbox = sender as PasswordBox;
            if (pbox != null && pbox.Password.Length > 0)
                SetIsTrueState(pbox, true);
            else
                SetIsTrueState(pbox, false);

        }

        public static string GetIsPasswordNotNull(UIElement element)
        {
            return (string)element.GetValue(IsPasswordNotNullProperty);
        }

        public static void SetIsPasswordNotNull(UIElement element, string IsPasswordNotNull)
        {
            element.SetValue(IsPasswordNotNullProperty, IsPasswordNotNull);
        }



        public static readonly DependencyProperty IsTrueStateProperty =
           DependencyProperty.RegisterAttached("IsTrueState", typeof(bool), typeof(AtPropPBox),
           new FrameworkPropertyMetadata((o, e) =>
           {

           }));

        public static bool GetIsTrueState(UIElement element)
        {
            return (bool)element.GetValue(IsTrueStateProperty);
        }

        public static void SetIsTrueState(UIElement element, bool IsPasswordNotNull)
        {
            element.SetValue(IsTrueStateProperty, IsPasswordNotNull);
        }

    }

    public class WindowViewModel : BaseViewModel
    {
        private Window mWindow;

        public int TitleHeight { get; set; } = 110;

        #region Commands

        public ICommand MinimizeCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        #endregion

        #region Constructor

        public WindowViewModel(Window window)
        {
            mWindow = window;
            // Create commands
            MinimizeCommand = new RelayCommand(() => mWindow.WindowState = WindowState.Minimized);
            CloseCommand = new RelayCommand(() => mWindow.Close());
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NFS
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        void backFunk()
        {
            NavigationService.GoBack();
        }
        public Settings()
        {
            InitializeComponent();
        }
        private void Press_Back(object sender, RoutedEventArgs e)
        {
            backFunk();
            return;
        }
    }
}

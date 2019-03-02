using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
using NFS.Class.Diss;
using System.Globalization;

namespace NFS
{
    /// <summary>
    /// Interaction logic for WelcomeWindows.xaml
    /// </summary>
    public partial class WelcomeWindows : Window
    {
        string autoUpadte = "1";
        string DRPCOnline = "0", DRPCCar = "0", DRPCEvent = "0", DRPCLobby = "0";

        string getStrFromResource(string key)
        {
            return (string)Application.Current.Resources[key];
        }
        void updateSaveData(string newText, int line_to_edit)
        {
            string[] file = new string[8];

            try { file[0] = File.ReadLines("saveData.txt").Skip(0).First(); }// way to nfsw.exe
            catch { file[0] = ""; }
            try { file[1] = File.ReadLines("saveData.txt").Skip(1).First(); }// loogin
            catch { file[1] = ""; }
            try { file[2] = File.ReadLines("saveData.txt").Skip(2).First(); }// encryptPass
            catch { file[2] = ""; }
            try { file[3] = File.ReadLines("saveData.txt").Skip(3).First(); }// autoUpdate
            catch { file[3] = ""; }
            try { file[4] = File.ReadLines("saveData.txt").Skip(4).First(); }// DRPCOnline
            catch { file[4] = ""; }
            try { file[5] = File.ReadLines("saveData.txt").Skip(5).First(); }// DRPCCar
            catch { file[5] = ""; }
            try { file[6] = File.ReadLines("saveData.txt").Skip(6).First(); }// DRPCEvent
            catch { file[6] = ""; }
            try { file[7] = File.ReadLines("saveData.txt").Skip(7).First(); }// DRPCLobby
            catch { file[7] = ""; }

            file[line_to_edit] = newText;

            using (StreamWriter writetext = new StreamWriter("saveData.txt"))
            {
                writetext.WriteLine(file[0]);
                writetext.WriteLine(file[1]);
                writetext.WriteLine(file[2]);
                writetext.WriteLine(file[3]);
                writetext.WriteLine(file[4]);
                writetext.WriteLine(file[5]);
                writetext.WriteLine(file[6]);
                writetext.WriteLine(file[7]);
            }
        }
        void readSaveData()
        {
            try
            {
                autoUpadte = File.ReadLines("saveData.txt").Skip(4).First() == "" ? autoUpadte : File.ReadLines("saveData.txt").Skip(4).First();
                DRPCOnline = File.ReadLines("saveData.txt").Skip(5).First() == "" ? DRPCOnline : File.ReadLines("saveData.txt").Skip(5).First();
                DRPCCar = File.ReadLines("saveData.txt").Skip(6).First() == "" ? DRPCCar : File.ReadLines("saveData.txt").Skip(6).First();
                DRPCEvent = File.ReadLines("saveData.txt").Skip(7).First() == "" ? DRPCEvent : File.ReadLines("saveData.txt").Skip(7).First();
                DRPCLobby = File.ReadLines("saveData.txt").Skip(8).First() == "" ? DRPCLobby : File.ReadLines("saveData.txt").Skip(8).First();
            }
            catch
            {
                using (var tw = new StreamWriter("saveData.txt", true)) ;
            }
            //MessageBox.Show("'" + saveWayToFileNFSW + "'\n'" + saveLogin + "'\n'" + saveEncryptPass + "'");
            return;
        }

        public WelcomeWindows()
        {
            InitializeComponent();

            App.LanguageChanged += LanguageChanged;

            CultureInfo currLang = App.Language;

            //Заполняем меню смены языка:
            LangBox.Items.Clear();
            foreach (var lang in App.Languages)
            {
                ComboBoxItem menuLang = new ComboBoxItem();
                menuLang.Content = lang.DisplayName;
                menuLang.Tag = lang;
                menuLang.IsSelected = lang.Equals(currLang);
                menuLang.Selected += ChangeLanguageClick;
                LangBox.Items.Add(menuLang);
            }

            readSaveData();

            CheckForAutoUpdate.IsChecked = Int32.Parse(autoUpadte) != 0;

            CheckForDRPCOnline.IsChecked = Int32.Parse(DRPCOnline) != 0;
            CheckForDRPCCar.IsChecked = Int32.Parse(DRPCCar) != 0;
            CheckForDRPCEvent.IsChecked = Int32.Parse(DRPCEvent) != 0;
            CheckForDRPCLobby.IsChecked = Int32.Parse(DRPCLobby) != 0;

            CheckForDRPCCar.IsEnabled = CheckForDRPCEvent.IsEnabled = CheckForDRPCLobby.IsEnabled = (bool)CheckForDRPCOnline.IsChecked;
            if (CheckForDRPCOnline.IsChecked == false)
                CheckForDRPCCar.IsChecked = CheckForDRPCEvent.IsChecked = CheckForDRPCLobby.IsChecked = (bool)CheckForDRPCOnline.IsChecked;

        }

        private void Press_Save(object sender, RoutedEventArgs e)
        {
            updateSaveData((CheckForAutoUpdate.IsChecked == true ? 1 : 0).ToString(),3);
            updateSaveData((CheckForDRPCOnline.IsChecked == true ? 1 : 0).ToString(),4);
            updateSaveData((CheckForDRPCCar.IsChecked ==    true ? 1 : 0).ToString(),5);
            updateSaveData((CheckForDRPCEvent.IsChecked ==  true ? 1 : 0).ToString(),6);
            updateSaveData((CheckForDRPCLobby.IsChecked ==  true ? 1 : 0).ToString(),7);

            Close();
            return;
        }

        private void CheckForDRPCOnline_Checked(object sender, RoutedEventArgs e)
        {
            CheckForDRPCCar.IsEnabled = CheckForDRPCEvent.IsEnabled = CheckForDRPCLobby.IsEnabled = (bool)CheckForDRPCOnline.IsChecked;
            if (CheckForDRPCOnline.IsChecked == false)
                CheckForDRPCCar.IsChecked = CheckForDRPCEvent.IsChecked = CheckForDRPCLobby.IsChecked = (bool)CheckForDRPCOnline.IsChecked;
        }

        private void LanguageChanged(Object sender, EventArgs e)
        {
            CultureInfo currLang = App.Language;

            //Отмечаем нужный пункт смены языка как выбранный язык
            foreach (ComboBoxItem i in LangBox.Items)
            {
                CultureInfo ci = i.Tag as CultureInfo;
                i.IsSelected = ci != null && ci.Equals(currLang);
            }
        }

        private void ChangeLanguageClick(Object sender, EventArgs e)
        {
            ComboBoxItem mi = sender as ComboBoxItem;
            if (mi != null)
            {
                CultureInfo lang = mi.Tag as CultureInfo;
                if (lang != null)
                {
                    App.Language = lang;
                }
            }

        }
    }
}

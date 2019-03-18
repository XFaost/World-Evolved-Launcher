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
using System.Xml;
using System.Globalization;

namespace NFS
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        string autoUpdate = "1", wayToUserSettings = "";
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
            try { file[3] = File.ReadLines("saveData.txt").Skip(3).First(); }// fileSize
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
        public void readSaveData()
        {
            try
            {
                autoUpdate  = File.ReadLines("saveData.txt").Skip(3).First() == "" ? autoUpdate   : File.ReadLines("saveData.txt").Skip(3).First();
                DRPCOnline  = File.ReadLines("saveData.txt").Skip(4).First() == "" ? DRPCOnline   : File.ReadLines("saveData.txt").Skip(4).First();
                DRPCCar     = File.ReadLines("saveData.txt").Skip(5).First() == "" ? DRPCCar      : File.ReadLines("saveData.txt").Skip(5).First();
                DRPCEvent   = File.ReadLines("saveData.txt").Skip(6).First() == "" ? DRPCEvent    : File.ReadLines("saveData.txt").Skip(6).First();
                DRPCLobby   = File.ReadLines("saveData.txt").Skip(7).First() == "" ? DRPCLobby    : File.ReadLines("saveData.txt").Skip(7).First();

                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(wayToUserSettings);
                XmlElement xRoot = xDoc.DocumentElement;

                foreach (XmlNode xnode in xRoot)
                {
                    foreach (XmlNode childnode in xnode.ChildNodes)
                    {
                        if (childnode.Name == "Language")
                        {
                            for (int i = 0; i < GameLangBox.Items.Count; i++)
                            {
                                if (((ComboBoxItem)GameLangBox.Items[i]).Content.ToString() == childnode.InnerText)
                                {
                                    GameLangBox.SelectedIndex = i;
                                    break;
                                }
                            }
                        }
                        if (childnode.Name == "performancelevel")
                        {
                            if (childnode.InnerText != "5")
                            {
                                settings1.IsEnabled = settings2.IsEnabled = false;
                                activationHideSettingsButton.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                settings1.IsEnabled = settings2.IsEnabled = true;
                                activationHideSettingsButton.Visibility = Visibility.Hidden;
                            }
                        }
                    }
                }

                foreach (XmlNode xnode in xRoot)
                {
                    foreach (XmlNode childnode in xnode.ChildNodes)
                    {
                        if (childnode.Name == "screenwindowed")             screenwindowedBox.IsChecked         = childnode.InnerText == "1" ? true : false;
                        if (childnode.Name == "screenrefresh")              screenrefreshBox.Text               = childnode.InnerText;
                        if (childnode.Name == "screenheight")               screenheightBox.Text                = childnode.InnerText;
                        if (childnode.Name == "screenwidth")                screenwidthBox.Text                 = childnode.InnerText;
                        if (childnode.Name == "brightness")                 brightnessBox.Text                  = childnode.InnerText;
                        if (childnode.Name == "enableaero")                 enableaeroBox.IsChecked             = childnode.InnerText == "1" ? false : true;
                        if (childnode.Name == "pixelaspectratiooverride")   pixelaspectratiooverrideBox.Text    = childnode.InnerText;

                        if (childnode.Name == "audiomode")                  audiomodeBox.SelectedIndex          = Int32.Parse(childnode.InnerText)-1;

                        if (childnode.Name == "visualtreatment")            visualtreatmentBox.IsChecked        = childnode.InnerText == "1" ? true : false;
                        if (childnode.Name == "globaldetaillevel")          globaldetaillevelBox.Value          = Int32.Parse(childnode.InnerText);
                        if (childnode.Name == "basetexturefilter")          basetexturefilterBox.Value          = Int32.Parse(childnode.InnerText);
                        if (childnode.Name == "roadtexturefilter")          roadtexturefilterBox.Value          = Int32.Parse(childnode.InnerText);
                        if (childnode.Name == "basetexturelodbias")         basetexturelodbiasBox.Text          = childnode.InnerText;
                        if (childnode.Name == "roadtexturelodbias")         roadtexturelodbiasBox.Text          = childnode.InnerText;
                        if (childnode.Name == "shaderdetail")               shaderdetailBox.Value               = Int32.Parse(childnode.InnerText) == 4 ? 3 : Int32.Parse(childnode.InnerText); //3 - глючная
                        if (childnode.Name == "fsaalevel")                  fsaalevelBox.Value                  = Int32.Parse(childnode.InnerText);
                        if (childnode.Name == "carlodlevel")                carlodlevelBox.SelectedIndex        = Int32.Parse(childnode.InnerText);
                        if (childnode.Name == "overbrightenable")           overbrightenableBox.IsChecked       = childnode.InnerText == "1" ? true : false;
                        if (childnode.Name == "particlesystemenable")       particlesystemenableBox.IsChecked   = childnode.InnerText == "1" ? true : false;
                        if (childnode.Name == "carenvironmentmapenable")    carenvironmentmapenableBox.Value    = Int32.Parse(childnode.InnerText);
                        if (childnode.Name == "roadreflectionenable")       roadreflectionenableBox.Value       = Int32.Parse(childnode.InnerText);
                        if (childnode.Name == "motionblurenable")           motionblurenableBox.IsChecked       = childnode.InnerText == "1" ? true : false;
                        if (childnode.Name == "basetexturemaxani")
                        {
                            for (int i = 0; i < roadtexturemaxaniBox.Items.Count; i++)
                            {
                                if (((ComboBoxItem)basetexturemaxaniBox.Items[i]).Content.ToString() == childnode.InnerText + "x")
                                {
                                    basetexturemaxaniBox.SelectedIndex = i;
                                    break;
                                }
                            }
                        }
                        if (childnode.Name == "roadtexturemaxani")
                        {
                            for (int i = 0; i < roadtexturemaxaniBox.Items.Count; i++)
                            {
                                if (((ComboBoxItem)roadtexturemaxaniBox.Items[i]).Content.ToString() == childnode.InnerText + "x")
                                {
                                    roadtexturemaxaniBox.SelectedIndex = i;
                                    break;
                                }
                            }
                        }
                        if (childnode.Name == "vsyncon")                    vsynconBox.IsChecked                = childnode.InnerText == "1" ? true : false;

                    }
                }

            }
            catch
            {
                using (var tw = new StreamWriter("saveData.txt", true)) ;
            }
            //MessageBox.Show("'" + saveWayToFileNFSW + "'\n'" + saveLogin + "'\n'" + saveEncryptPass + "'");
            return;
        }
        void backFunk()
        {
            NavigationService.GoBack();
        }
        void Press_activationHideSettings(object sender, RoutedEventArgs e)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(wayToUserSettings);
            XmlElement xRoot = xDoc.DocumentElement;

            foreach (XmlNode xnode in xRoot)
            {
                foreach (XmlNode childnode in xnode.ChildNodes)
                {
                    if (childnode.Name == "performancelevel") childnode.InnerText = "5";
                }
            }

            xDoc.Save(wayToUserSettings);

            MessageBox.Show(getStrFromResource("succHideSettings"), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public Settings()
        {
            InitializeComponent();
            
            wayToUserSettings += Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Need for Speed World\Settings\UserSettings.xml";

            App.LanguageChanged += LanguageChanged;

            CultureInfo currLang = App.Language;

            //Заполняем меню смены языка:
            LangBox.Items.Clear();
            foreach (var lang in App.Languages)
            {
                ComboBoxItem menuLang = new ComboBoxItem();
                menuLang.Content = lang.EnglishName;
                menuLang.Tag = lang;
                menuLang.IsSelected = lang.Equals(currLang);
                menuLang.Selected += ChangeLanguageClick;
                LangBox.Items.Add(menuLang);
            }

            readSaveData();

            CheckForAutoUpdate.IsChecked = Int32.Parse(autoUpdate) != 0;

            CheckForDRPCOnline.IsChecked =  Int32.Parse(DRPCOnline) != 0;
            CheckForDRPCCar.IsChecked =     Int32.Parse(DRPCCar) != 0;
            CheckForDRPCEvent.IsChecked =   Int32.Parse(DRPCEvent) != 0;
            CheckForDRPCLobby.IsChecked =   Int32.Parse(DRPCLobby) != 0;

            CheckForDRPCCar.IsEnabled = CheckForDRPCEvent.IsEnabled = CheckForDRPCLobby.IsEnabled = (bool)CheckForDRPCOnline.IsChecked;
            if (CheckForDRPCOnline.IsChecked == false)
                CheckForDRPCCar.IsChecked = CheckForDRPCEvent.IsChecked = CheckForDRPCLobby.IsChecked = (bool)CheckForDRPCOnline.IsChecked;

        }
        private void Press_Back(object sender, RoutedEventArgs e)
        {
            backFunk();
            return;
        }
        private void Press_Save(object sender, RoutedEventArgs e)
        {
            updateSaveData((CheckForAutoUpdate.IsChecked == true ? 1 : 0).ToString(), 3);
            updateSaveData((CheckForDRPCOnline.IsChecked == true ? 1 : 0).ToString(), 4);
            updateSaveData((CheckForDRPCCar.IsChecked ==    true ? 1 : 0).ToString(), 5);
            updateSaveData((CheckForDRPCEvent.IsChecked ==  true ? 1 : 0).ToString(), 6);
            updateSaveData((CheckForDRPCLobby.IsChecked ==  true ? 1 : 0).ToString(), 7);

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(wayToUserSettings);
            XmlElement xRoot = xDoc.DocumentElement;

            foreach (XmlNode xnode in xRoot)
            {
                foreach (XmlNode childnode in xnode.ChildNodes)
                {
                    if (childnode.Name == "Language")
                    {
                        childnode.InnerText = ((ComboBoxItem)GameLangBox.SelectedItem).Content.ToString();
                        break;
                    }
                }
            }

            if (settings1.IsEnabled == false)
            {
                xDoc.Save(wayToUserSettings);
                backFunk();
                return;
            }       

            foreach (XmlNode xnode in xRoot)
            {
                foreach (XmlNode childnode in xnode.ChildNodes)
                {
                    if (childnode.Name == "screenwindowed")             childnode.InnerText = (screenwindowedBox.IsChecked == true ? "1" : "0");
                    if (childnode.Name == "screenrefresh")              childnode.InnerText = screenrefreshBox.Text;
                    if (childnode.Name == "screenheight")               childnode.InnerText = screenheightBox.Text;
                    if (childnode.Name == "screenwidth")                childnode.InnerText = screenwidthBox.Text;
                    if (childnode.Name == "brightness")                 childnode.InnerText = brightnessBox.Text;
                    if (childnode.Name == "enableaero")                 childnode.InnerText = (enableaeroBox.IsChecked == true ? "0" : "1");
                    if (childnode.Name == "pixelaspectratiooverride")   childnode.InnerText = pixelaspectratiooverrideBox.Text;

                    if (childnode.Name == "audiomode")                  childnode.InnerText = (audiomodeBox.SelectedIndex + 1).ToString();

                    if (childnode.Name == "visualtreatment")            childnode.InnerText = (visualtreatmentBox.IsChecked == true ? "1" : "0");
                    if (childnode.Name == "globaldetaillevel")          childnode.InnerText = globaldetaillevelBox.Value.ToString();
                    if (childnode.Name == "basetexturefilter")          childnode.InnerText = basetexturefilterBox.Value.ToString();
                    if (childnode.Name == "roadtexturefilter")          childnode.InnerText = roadtexturefilterBox.Value.ToString();
                    if (childnode.Name == "basetexturelodbias")         childnode.InnerText = basetexturelodbiasBox.Text;
                    if (childnode.Name == "roadtexturelodbias")         childnode.InnerText = roadtexturelodbiasBox.Text;
                    if (childnode.Name == "shaderdetail")               childnode.InnerText = shaderdetailBox.Value.ToString() == "3" ? "4" : shaderdetailBox.Value.ToString();
                    if (childnode.Name == "fsaalevel")                  childnode.InnerText = fsaalevelBox.Value.ToString();
                    if (childnode.Name == "carlodlevel")                childnode.InnerText = carlodlevelBox.SelectedIndex.ToString();
                    if (childnode.Name == "overbrightenable")           childnode.InnerText = (overbrightenableBox.IsChecked == true ? "1" : "0");
                    if (childnode.Name == "particlesystemenable")       childnode.InnerText = (particlesystemenableBox.IsChecked == true ? "1" : "0");
                    if (childnode.Name == "carenvironmentmapenable")    childnode.InnerText = carenvironmentmapenableBox.Value.ToString();
                    if (childnode.Name == "roadreflectionenable")       childnode.InnerText = roadreflectionenableBox.Value.ToString();
                    if (childnode.Name == "motionblurenable")           childnode.InnerText = (motionblurenableBox.IsChecked == true ? "1" : "0");
                    if (childnode.Name == "basetexturemaxani")          childnode.InnerText = ((ComboBoxItem)basetexturemaxaniBox.SelectedItem).Content.ToString().Remove(((ComboBoxItem)basetexturemaxaniBox.SelectedItem).Content.ToString().Length - 1);// например выбрано "16x", а нужно записать в файл "16"
                    if (childnode.Name == "roadtexturemaxani")          childnode.InnerText = ((ComboBoxItem)roadtexturemaxaniBox.SelectedItem).Content.ToString().Remove(((ComboBoxItem)roadtexturemaxaniBox.SelectedItem).Content.ToString().Length - 1);//
                    if (childnode.Name == "vsyncon")                    childnode.InnerText = (vsynconBox.IsChecked == true ? "1" : "0");
                }
            }

            try
            {
                xDoc.Save(wayToUserSettings);
            }
            catch
            {
                foreach (string fileName in System.IO.Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Need for Speed World\Settings"))
                {
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(fileName);
                    fileInfo.Attributes = FileAttributes.Normal;
                }
                xDoc.Save(wayToUserSettings);
            }
            

            backFunk();
            return;
        }

        private void CheckForDRPCOnline_Checked(object sender, RoutedEventArgs e)
        {
            CheckForDRPCCar.IsEnabled = CheckForDRPCEvent.IsEnabled = CheckForDRPCLobby.IsEnabled = (bool)CheckForDRPCOnline.IsChecked;
            if (CheckForDRPCOnline.IsChecked == false)
                CheckForDRPCCar.IsChecked = CheckForDRPCEvent.IsChecked = CheckForDRPCLobby.IsChecked = (bool)CheckForDRPCOnline.IsChecked;
            MessageBox.Show(getStrFromResource("changeOnline"), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Warning);
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

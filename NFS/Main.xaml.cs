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
using GameLauncher;
using SoapBox.JsonScheme;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using System.Threading;
using DiscordRPC;
using NFS.Class.Diss;
using System.Net;
using System.IO;
using NFS;

namespace NFS
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        string serverIP = "http://185.125.231.50:8680/soapbox-race-core/Engine.svc";
        public Main()
        {
            InitializeComponent();
        }
        string getStrFromResource(string key)
        {
            return (string)Application.Current.Resources[key];
        }
        void startReg(object sender, RoutedEventArgs e)
        {
           NavigationService.Navigate(new Registration());
        }
        void recoveryAcc(object sender, RoutedEventArgs e)
        {
            if (email.Text.ToString() == "")
            {
                MessageBox.Show(getStrFromResource("questToFillLogin"), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Question);
                return;
            }

            string responseString;
            try
            {
                Uri resetPasswordUrl = new Uri(serverIP + "/RecoveryPassword/forgotPassword");

                var request = (HttpWebRequest)System.Net.WebRequest.Create(resetPasswordUrl);
                var postData = "email=" + email.Text.ToString();
                var data = Encoding.ASCII.GetBytes(postData);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();
                responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch
            {
                MessageBox.Show(getStrFromResource("unknownError"), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (responseString[0] == 'L' && responseString[5] == 't' && responseString[23] == 's')//Link to reset password sent to: [e-mail]
            {
                MessageBox.Show(getStrFromResource("recoveryAcc") + " [" + email.Text.ToString() + "]", getStrFromResource("WE"), MessageBoxButton.OK);
                return;
            }
            if (responseString == "ERROR: Recovery password link already sent, please check your spam mail box or try again in 1 hour.")
            {
                MessageBox.Show(getStrFromResource("checkSpam"), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (responseString == "ERROR: Invalid email!")
            {
                MessageBox.Show(getStrFromResource("invalidEmail"), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            MessageBox.Show(responseString, getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        void openSettings(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Settings());
        }
    }
}

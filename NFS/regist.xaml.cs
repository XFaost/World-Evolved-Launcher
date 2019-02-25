using System;
using System.IO;
using System.Net;
using GameLauncher;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GameLauncher.HashPassword;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NFS
{
    public partial class regist : Window
    {
        private string serverIP = "";

        public regist(string ip)
        {
            serverIP = ip;
            InitializeComponent();
            this.DataContext = new WindowViewModel(this);
        }

        private void Press_Regist(object sender, RoutedEventArgs e)
        {
            if (emailForRegist.Text.ToString() == "" || password0.Password.ToString() == "" || password1.Password.ToString() == "")
            {
                MessageBox.Show("Пожалуйста, заполните все данные.", "World Evolved", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (password0.Password.ToString() != password1.Password.ToString())
            {
                password0.Password = "";
                password1.Password = "";
                MessageBox.Show("Пароли не совпадают. Попробуйте еще раз.", "World Evolved", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string serverLoginResponse;

            try
            {
                WebClient wcr = new WebClientWithTimeout();
                wcr.Headers.Add("user-agent", "GameLauncher (+https://github.com/SoapboxRaceWorld/GameLauncher_NFSW)");

                string BuildURL = serverIP + "/User/createUser?email=" + emailForRegist.Text.ToString() + "&password=" + SHA.HashPassword(password0.Password.ToString()).ToLower();

                serverLoginResponse = wcr.DownloadString(BuildURL);
            }
            catch (WebException ex)
            {
                var serverReply = (HttpWebResponse)ex.Response;
                if (serverReply == null)
                {
                    serverLoginResponse = "<LoginStatusVO><UserId/><LoginToken/><Description>Failed to get reply from server. Please retry.</Description></LoginStatusVO>";
                }
                else
                {
                    using (var sr = new StreamReader(serverReply.GetResponseStream()))
                    {
                        serverLoginResponse = sr.ReadToEnd();
                    }
                }
            }

            XmlDocument SBRW_XML = new XmlDocument();
            SBRW_XML.LoadXml(serverLoginResponse);
            var nodes = SBRW_XML.SelectNodes("LoginStatusVO");

            try
            {
                foreach (XmlNode childrenNode in nodes)
                {
                    string Description = childrenNode["Description"].InnerText;

                    if (Description == "Registration Error: Email already exists!")
                    {
                        MessageBox.Show("Указанная электронная почта занята.", "World Evolved", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    else if(Description != "")
                    {
                        MessageBox.Show(Description);
                    }
                    if (childrenNode["UserId"].InnerText != "" && childrenNode["LoginToken"].InnerText != "")
                    {
                        MessageBox.Show("Вы были успешно зарегистрированы.", "World Evolved", MessageBoxButton.OK);
                        Close();
                    }
                };
            }
            catch
            {
                MessageBox.Show("Что-то пошло не так.", "World Evolved", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
    }
}

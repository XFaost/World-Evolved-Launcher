using System;
using System.Xml;
using System.Net;
using GameLauncher.HashPassword;
using System.IO;
using System.Diagnostics;
using GameLauncher;
using SoapBox.JsonScheme;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Net.NetworkInformation;
using System.Threading;
using DiscordRPC;
using NFS.Class.Diss;
using System.ComponentModel;
using System.Globalization;

//(string)Application.Current.Resources["news"]
namespace NFS
{
    public partial class MainWindow : Window
    {

        public Main mPage;

        public static RichPresence _presence = new RichPresence()
        {
            Details = (string)Application.Current.Resources["launcherOpen"],

            Assets = new Assets()
            {
                LargeImageKey = "maxmlpzbsrw"
            },

            Timestamps = new Timestamps()
            {
                Start = DateTime.UtcNow
            }
        };

        public static DiscordRpcClient discordRpcClient;
        public void HideWin()
        {
            WindowState = WindowState.Minimized;
        }

        private void myFrame_ContentRendered(object sender, EventArgs e)
        {
            UserPanel.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new WindowViewModel(this);

            mPage = new Main();
            UserPanel.NavigationService.Navigate(mPage);

            
        }

    }
}

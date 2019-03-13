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

namespace NFS
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        private
        string version = (string)Application.Current.Resources["version"];
        string serverIP = "http://185.125.231.50:8680/soapbox-race-core/Engine.svc";
        string saveWayToFileNFSW = "", wayToLog = "";
        string saveLogin = "", saveEncryptPass = "";
        string autoUpdate = "1";
        string DRPCOnline = "0", DRPCCar = "0", DRPCEvent = "0", DRPCLobby = "0";
        Thread ThreadForMonitoringOnlineAndPing;
        Thread _nfswstarted, updateThread;

        Settings form1 = new Settings();
        Registration form2 = new Registration();

        string SiteLink = "";
        string VKLink = "";
        string DiscordLink = "";

        void checkUpdate()
        {
            if (autoUpdate == "1")
            {
                try
                {
                    string v = version;


                    using (var client = new WebClient())
                    {
                        client.DownloadFile("http://world-evolved.ru/launcher/version.txt", "v.txt");
                    }
                    v = File.ReadLines("v.txt").Skip(0).First();

                    File.Delete(@"v.txt");

                    if (v != version)
                    {
                        MessageBoxResult result = MessageBox.Show(getStrFromResource("questUpdate"), getStrFromResource("WE") + " | " + version + " --> " + v, MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            Process.Start(@".\Update\Update.exe");
                            Process.GetCurrentProcess().Kill();
                        }
                    }
                }
                catch { }
            }
        }

        void getWayToLog()
        {
            int num = saveWayToFileNFSW.Length - 1;
            for (; num > 0; num--)
            {
                if (saveWayToFileNFSW[num] == '\\') break;
            }
            for (int i = 0; i <= num; i++)
            {
                wayToLog += saveWayToFileNFSW[i];
            }
            wayToLog += "NFSWO_COMMUNICATION_LOG.txt";
        }

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
            try { file[3] = File.ReadLines("saveData.txt").Skip(3).First(); }// checkUpdate
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
                saveWayToFileNFSW = File.ReadLines("saveData.txt").Skip(0).First();
                saveLogin = File.ReadLines("saveData.txt").Skip(1).First();
                saveEncryptPass = File.ReadLines("saveData.txt").Skip(2).First();
                autoUpdate = File.ReadLines("saveData.txt").Skip(3).First() == "" ? autoUpdate : File.ReadLines("saveData.txt").Skip(3).First();
                DRPCOnline = File.ReadLines("saveData.txt").Skip(4).First() == "" ? DRPCOnline : File.ReadLines("saveData.txt").Skip(4).First();
                DRPCCar = File.ReadLines("saveData.txt").Skip(5).First() == "" ? DRPCCar : File.ReadLines("saveData.txt").Skip(5).First();
                DRPCEvent = File.ReadLines("saveData.txt").Skip(6).First() == "" ? DRPCEvent : File.ReadLines("saveData.txt").Skip(6).First();
                DRPCLobby = File.ReadLines("saveData.txt").Skip(7).First() == "" ? DRPCLobby : File.ReadLines("saveData.txt").Skip(7).First();
            }
            catch
            {
                using (var tw = new StreamWriter("saveData.txt", true)) ;
            }

            return;
        }
        void setFileForGame()
        {
            var openFolder = new CommonOpenFileDialog();
            openFolder.InitialDirectory = "";
            openFolder.IsFolderPicker = false;
            openFolder.Filters.Add(new CommonFileDialogFilter("nfsw", "*.exe"));
            openFolder.Title = getStrFromResource("questWaytoGame0");
            if (openFolder.ShowDialog() != CommonFileDialogResult.Ok) { MessageBox.Show(getStrFromResource("questWaytoGame1"), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Warning); Process.GetCurrentProcess().Kill(); }
            saveWayToFileNFSW = openFolder.FileName;
            updateSaveData(saveWayToFileNFSW, 0);
        }

        void getInfAboutServer()
        {
            string ping = "";
            bool update = true;
            while (true)
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate ()
                {
                    update = PlayButton.IsEnabled;
                });

                if (update == false)
                {
                    System.Threading.Thread.Sleep(60000);
                    continue;//если игра запущена, то незачем обновлять данные
                }

                ping = new Ping().Send("185.125.231.50").RoundtripTime.ToString();

                if (ping == "0")//если пинг ноль, то данные не были получены по причине отсутствия подключения к интернету (раньше после такого был прописан код на закрытие клиента, но отсутствие интеренета может быть временным)
                {
                    System.Threading.Thread.Sleep(60000);
                    continue;//если игра запущена, то незачем обновлять данные
                }

                WebClientWithTimeout client = new WebClientWithTimeout();
                var stringToUri = new Uri(serverIP + "/GetServerInformation");
                client.DownloadStringAsync(stringToUri);

                client.DownloadStringCompleted += (sender2, e2) =>
                {
                    var json = JsonConvert.DeserializeObject<GetServerInformation>(e2.Result);

                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate ()
                    {
                        PingInf.Content = ping + "ms";
                        OnlineInf.Content = string.Format("{0}/{1}", json.onlineNumber, json.maxOnlinePlayers);
                    });

                    SiteLink = json.homePageUrl;
                    VKLink = "https://vk.com/worldevolved";
                    DiscordLink = json.discordUrl;
                };
                System.Threading.Thread.Sleep(60000);
            }
        }

        XmlNodeList getInfAboutAcc(string login, string encryptPass)
        {
            string serverLoginResponse = "";

            try
            {
                WebClient wc = new WebClientWithTimeout();
                wc.Headers.Add("user-agent", "World Evolved Launcher");
                serverLoginResponse = wc.DownloadString(serverIP + "/User/authenticateUser?email=" + login + "&password=" + encryptPass.ToLower());
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse serverReply = (HttpWebResponse)ex.Response;
                    serverLoginResponse = ex.Message;

                    switch ((int)serverReply.StatusCode)
                    {
                        case 5: MessageBox.Show(getStrFromResource("error5"), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Error); break;
                        case 6: MessageBox.Show(getStrFromResource("error6"), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Error); break;
                        case 7: MessageBox.Show(getStrFromResource("error7"), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Error); break;
                        case 10: MessageBox.Show(getStrFromResource("error10"), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Error); break;
                        case 13: MessageBox.Show(getStrFromResource("error13"), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Error); break;
                        case 410: MessageBox.Show(getStrFromResource("error500"), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Error); break;
                        case 500:   //MessageBox.Show("Внутренняя ошибка сервера");
                            using (StreamReader sr = new StreamReader(serverReply.GetResponseStream()))
                            {
                                serverLoginResponse = sr.ReadToEnd();
                            }
                            break;

                        default: MessageBox.Show(getStrFromResource("anotherError") + (int)serverReply.StatusCode, getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Error); break;
                    }
                }
                else
                {
                    serverLoginResponse = ex.Message;
                }
            }
            if (string.IsNullOrEmpty(serverLoginResponse))
            {
                MessageBox.Show(getStrFromResource("offline"), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                Process.GetCurrentProcess().Kill();
            }

            XmlDocument SBRW_XML = new XmlDocument();
            try
            {
                SBRW_XML.LoadXml(serverLoginResponse);
            }
            catch
            {
                MessageBox.Show(getStrFromResource("offlineOrErrorInternet"), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                Process.GetCurrentProcess().Kill();
            }

            var nodes = SBRW_XML.SelectNodes("LoginStatusVO");
            return nodes;
        }

        public Main()
        {
            InitializeComponent();

            readSaveData();

            
            if (saveWayToFileNFSW == "" && saveLogin == "")
            {
                if (CultureInfo.InstalledUICulture.Name != "ru-Ru")     App.Language = CultureInfo.CreateSpecificCulture("en-US");
                else                                                    App.Language = CultureInfo.CreateSpecificCulture("ru-Ru");
            }

            updateThread = new Thread(checkUpdate);
            updateThread.IsBackground = true;
            updateThread.Start();

            if (!System.IO.File.Exists("getLastError.exe"))
            {
                MessageBox.Show(getStrFromResource("getlastErrorNotFound"), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Error);
                Process.GetCurrentProcess().Kill();
            }

            ServerProxy.Instance.SetCheckOnline(Int32.Parse(DRPCOnline) == 1);
            ServerProxy.Instance.Start();


            if (Int32.Parse(DRPCOnline) != 0)
            {
                MainWindow.discordRpcClient = new DiscordRpcClient("549220332527943701");
                MainWindow.discordRpcClient.Initialize();
                MainWindow.discordRpcClient.SetPresence(MainWindow._presence);
            }

            if (!System.IO.File.Exists(saveWayToFileNFSW))
            {
                saveWayToFileNFSW = ""; updateSaveData("", 0);
            }

            if (saveWayToFileNFSW == "")
                setFileForGame();


            if (saveLogin != "" && saveEncryptPass != "")
            {
                foreach (XmlNode childrenNode in getInfAboutAcc(saveLogin, saveEncryptPass))
                {
                    if (childrenNode["Description"].InnerText == "LOGIN ERROR")
                    {
                        saveLogin = ""; updateSaveData("", 1);
                        saveEncryptPass = ""; updateSaveData("", 2);
                    }
                    else
                    {
                        email.Text = saveLogin;
                        CheckForRememberUser.IsChecked = true;
                        infAboutSavePass.Text = getStrFromResource("passSaved");
                    }
                }
            }
            getWayToLog();
            ThreadForMonitoringOnlineAndPing = new Thread(getInfAboutServer);
            ThreadForMonitoringOnlineAndPing.IsBackground = true;
            ThreadForMonitoringOnlineAndPing.Start();
        }

        private
        void LaunchGame(string args)
        {
            readSaveData();

            ServerProxy.Instance.SetCheckCar(Int32.Parse(DRPCCar) == 1);
            ServerProxy.Instance.SetCheckEvent(Int32.Parse(DRPCEvent) == 1);
            ServerProxy.Instance.SetCheckLobby(Int32.Parse(DRPCLobby) == 1);

            if (Int32.Parse(DRPCCar) == 1)
            {
                MainWindow._presence.Details = getStrFromResource("loading");
                MainWindow._presence.Timestamps = new Timestamps()
                {
                    Start = DateTime.UtcNow
                };
                MainWindow.discordRpcClient.SetPresence(MainWindow._presence);
            }

            var nfswProcess = Process.Start(saveWayToFileNFSW, args);
            if (nfswProcess != null)
            {
                nfswProcess.EnableRaisingEvents = true;

                nfswProcess.Exited += (sender2, e2) => {
                    if (Int32.Parse(DRPCCar) == 1)
                    {
                        MainWindow._presence.Details = getStrFromResource("launcherOpen");
                        MainWindow._presence.Timestamps = new Timestamps()
                        {
                            Start = DateTime.UtcNow
                        };
                        MainWindow.discordRpcClient.SetPresence(MainWindow._presence);
                    }
                    var exitCode = nfswProcess.ExitCode;

                    if (exitCode != 0)
                    {
                        if (!System.IO.File.Exists("getLastError.exe"))
                        {
                            MessageBox.Show(getStrFromResource("getlastErrorNotFound"), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Error);
                            Process.GetCurrentProcess().Kill();
                        }
                        if (!System.IO.File.Exists(wayToLog))
                        {
                            MessageBox.Show(getStrFromResource("logNotFound") + " \"" + wayToLog + "\"", getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        int error;
                        args = "/d " + "\"" + wayToLog + "\"";
                        var getErrorProcess = Process.Start("getLastError.exe", args);
                        if (getErrorProcess != null)
                        {
                            getErrorProcess.EnableRaisingEvents = true;

                            getErrorProcess.Exited += (sender3, e3) =>
                            {
                                error = getErrorProcess.ExitCode;
                                switch (error)
                                {
                                    case 1001: MessageBox.Show(getStrFromResource("logNotFound") + " \"" + wayToLog + "\"", getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Error); break;
                                    case 5: MessageBox.Show(getStrFromResource("error5") + getStrFromResource("gameReturnCode") + exitCode.ToString(), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Error); break;
                                    case 6: MessageBox.Show(getStrFromResource("error6") + getStrFromResource("gameReturnCode") + exitCode.ToString(), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Error); break;
                                    case 7: MessageBox.Show(getStrFromResource("error7") + getStrFromResource("gameReturnCode") + exitCode.ToString(), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Error); break;
                                    case 10: MessageBox.Show(getStrFromResource("error10") + getStrFromResource("gameReturnCode") + exitCode.ToString(), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Error); break;
                                    case 13: MessageBox.Show(getStrFromResource("error13") + getStrFromResource("gameReturnCode") + exitCode.ToString(), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Error); break;
                                    case 410: MessageBox.Show(getStrFromResource("error410"), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Error); break;
                                    case 500: MessageBox.Show(getStrFromResource("error500") + getStrFromResource("gameReturnCode") + exitCode.ToString(), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Error); break;

                                    default: MessageBox.Show(getStrFromResource("anotherError") + error + getStrFromResource("gameReturnCode") + exitCode.ToString(), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Error); break;
                                }
                            };
                        }
                    }
                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate ()
                    {
                        PlayButton.IsEnabled = true;
                    });
                };

            }
        }
        void Press_Play(object sender, RoutedEventArgs e)
        {
            readSaveData();

            PlayButton.IsEnabled = false;
            string Login = "";
            string EncryptPass = "";

            if (saveLogin == "" && saveEncryptPass == "" && (email.Text.ToString() == "" || password.Password.ToString() == ""))
            {
                MessageBox.Show(getStrFromResource("questToFillAccInf"), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                PlayButton.IsEnabled = true;
                return;
            }
            if (saveLogin != "" && saveEncryptPass != "" && email.Text.ToString() != "" && password.Password.ToString() != "")
            {
                Login = email.Text.ToString();
                EncryptPass = SHA.HashPassword(password.Password.ToString()).ToLower();
            }
            if (saveLogin == "" && saveEncryptPass == "" && email.Text.ToString() != "" && password.Password.ToString() != "")
            {
                Login = email.Text.ToString();
                EncryptPass = SHA.HashPassword(password.Password.ToString()).ToLower();
            }
            if (saveEncryptPass != "" && email.Text.ToString() == saveLogin && password.Password.ToString() == "")
            {
                Login = saveLogin;
                EncryptPass = saveEncryptPass;
            }
            if (saveEncryptPass != "" && email.Text.ToString() != saveLogin && password.Password.ToString() == "")
            {
                Login = email.Text.ToString();
                EncryptPass = saveEncryptPass;
            }

            try
            {
                foreach (XmlNode childrenNode in getInfAboutAcc(Login, EncryptPass))
                {
                    string Description = childrenNode["Description"].InnerText;

                    if (Description != "")
                    {
                        if (Description == "LOGIN ERROR")
                        {
                            MessageBox.Show(getStrFromResource("questErrorAccInf"), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Error);
                            updateSaveData("", 1);
                            updateSaveData("", 2);
                            PlayButton.IsEnabled = true;
                            infAboutSavePass.Text = "Password";
                            saveLogin = "";
                            saveEncryptPass = "";
                            return;
                        }
                        else
                        {
                            MessageBox.Show(Description, getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else if (childrenNode["LoginToken"].InnerText != "" && childrenNode["UserId"].InnerText != "")
                    {
                        if (CheckForRememberUser.IsChecked == true && email.Text.ToString() != "" && password.Password.ToString() != "")
                        {
                            updateSaveData(Login, 1);
                            updateSaveData(EncryptPass, 2);
                        }
                        if (CheckForRememberUser.IsChecked == false)
                        {
                            updateSaveData("", 1);
                            updateSaveData("", 2);
                        }

                        string cParams = "WORLDEVOLVED " + "http://127.0.0.1:6264/nfsw/Engine.svc" + " " + childrenNode["LoginToken"].InnerText + " " + childrenNode["UserId"].InnerText + " -advancedLaunch";

                        if (!System.IO.File.Exists(saveWayToFileNFSW))
                        {
                            MessageBox.Show(getStrFromResource("notfoundTheGame") + "\n* \"" + saveWayToFileNFSW + "\"", getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Warning);
                            saveWayToFileNFSW = ""; updateSaveData("", 0);
                            PlayButton.IsEnabled = true;
                            return;
                        }

                        //MainWindow.HideWin();

                        ServerProxy.Instance.SetServerUrl(serverIP);
                        ServerProxy.Instance.SetServerName("WORLDEVOLVED");

                        _nfswstarted = new Thread(() =>
                        {
                            LaunchGame(cParams);
                        })
                        {
                            IsBackground = true
                        };
                        _nfswstarted.Start();
                    }
                }
            }
            catch
            {
                MessageBox.Show(getStrFromResource("unknownError"), getStrFromResource("WE"), MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        void openSettings(object sender, RoutedEventArgs e)
        {
            form1.readSaveData();
            NavigationService.Navigate(form1);
        }
        void startReg(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(form2);
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
        void openSite(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(SiteLink);
        }
        void openVK(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(VKLink);
        }
        void openDiscord(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(DiscordLink);
        }
    }
}

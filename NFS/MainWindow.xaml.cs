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

namespace NFS
{
    public partial class MainWindow : Window
    {
        private
        string serverIP = "http://185.125.231.50:8680/soapbox-race-core/Engine.svc";
        string saveWayToFileNFSW = ""; string wayToLog = "";
        string saveLogin = "";
        string saveEncryptPass = "";
        Thread ThreadForMonitoringOnlineAndPing;
        private Thread _nfswstarted;

        private static RichPresence _presence = new RichPresence()
        {
            Details = "Открыт лаунчер",

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

        string SiteLink = "";
        string VKLink = "";
        string DiscordLink = "";

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

        void updateSaveData(string newText, int line_to_edit)
        {
            string[] file = new string[3];

            try { file[0] = File.ReadLines("saveData.txt").Skip(0).First(); }
            catch { file[0] = ""; }
            try { file[1] = File.ReadLines("saveData.txt").Skip(1).First(); }
            catch { file[1] = ""; }
            try { file[2] = File.ReadLines("saveData.txt").Skip(3).First(); }
            catch { file[2] = ""; }

            file[line_to_edit] = newText;

            using (StreamWriter writetext = new StreamWriter("saveData.txt"))
            {
                writetext.WriteLine(file[0]);
                writetext.WriteLine(file[1]);
                writetext.WriteLine(file[2]);
            }
        }
        void readSaveData()
        {
            try
            {
                saveWayToFileNFSW = File.ReadLines("saveData.txt").Skip(0).First();
                saveLogin = File.ReadLines("saveData.txt").Skip(1).First();
                saveEncryptPass = File.ReadLines("saveData.txt").Skip(2).First();
            }
            catch
            {
                using (var tw = new StreamWriter("saveData.txt", true)) ;
            }
            //MessageBox.Show("'" + saveWayToFileNFSW + "'\n'" + saveLogin + "'\n'" + saveEncryptPass + "'");
            return;
        }
        void setFileForGame()
        {
            var openFolder = new CommonOpenFileDialog();
            openFolder.InitialDirectory = "";
            openFolder.IsFolderPicker = false;
            openFolder.Filters.Add(new CommonFileDialogFilter("nfsw", "*.exe"));
            openFolder.Title = "Пожалуйста, укажите где находиться файл nfsw.exe";
            if (openFolder.ShowDialog() != CommonFileDialogResult.Ok) { MessageBox.Show("Вы не указал файл игры.\nКлиент завершает работу."); Process.GetCurrentProcess().Kill(); }
            saveWayToFileNFSW = openFolder.FileName;
            updateSaveData(saveWayToFileNFSW, 0);
            return;
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

                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate ()
                {
                    PingInf.Content = "Пинг: " + ping + "ms";
                });

                WebClientWithTimeout client = new WebClientWithTimeout();
                var stringToUri = new Uri(serverIP + "/GetServerInformation");
                client.DownloadStringAsync(stringToUri);

                client.DownloadStringCompleted += (sender2, e2) =>
                {
                    var json = JsonConvert.DeserializeObject<GetServerInformation>(e2.Result);

                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate ()
                    {
                        OnlineInf.Content = string.Format("Онлайн: {0}/{1}", json.onlineNumber, json.maxOnlinePlayers);
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
                        case 5: MessageBox.Show("Невозможно подключиться к HTTP-части сервера", "World Evolved", MessageBoxButton.OK, MessageBoxImage.Error); break;
                        case 6: MessageBox.Show("Потеря связи", "World Evolved", MessageBoxButton.OK, MessageBoxImage.Error); break;
                        case 7: MessageBox.Show("Что-то блокирует соединение с клиентом", "World Evolved", MessageBoxButton.OK, MessageBoxImage.Error); break;
                        case 10: MessageBox.Show("Обрыв сессии, попробуйте ещё раз", "World Evolved", MessageBoxButton.OK, MessageBoxImage.Error); break;
                        case 13: MessageBox.Show("Невозможно подключиться к XMPP-части сервера", "World Evolved", MessageBoxButton.OK, MessageBoxImage.Error); break;
                        case 410: MessageBox.Show("Ваш аккаунт забанен", "World Evolved", MessageBoxButton.OK, MessageBoxImage.Error); break;
                        case 500:   //MessageBox.Show("Внутренняя ошибка сервера");
                            using (StreamReader sr = new StreamReader(serverReply.GetResponseStream()))
                            {
                                serverLoginResponse = sr.ReadToEnd();
                            }
                            break;

                        default: MessageBox.Show("StatusCode server: " + (int)serverReply.StatusCode, "World Evolved", MessageBoxButton.OK, MessageBoxImage.Error); break;
                    }
                }
                else
                {
                    serverLoginResponse = ex.Message;
                }
            }
            if (string.IsNullOrEmpty(serverLoginResponse))
            {
                MessageBox.Show("Сервер в оффлайне.\nКлиент завершает работу.", "World Evolved", MessageBoxButton.OK, MessageBoxImage.Warning);
                Process.GetCurrentProcess().Kill();
            }

            XmlDocument SBRW_XML = new XmlDocument();
            try
            {
                SBRW_XML.LoadXml(serverLoginResponse);
            }
            catch
            {
                MessageBox.Show("Сервер в оффлайне или отсутствует подключение к интернету.\nКлиент завершает работу.", "World Evolved", MessageBoxButton.OK, MessageBoxImage.Warning);
                Process.GetCurrentProcess().Kill();
            }

            var nodes = SBRW_XML.SelectNodes("LoginStatusVO");
            return nodes;
        }

        public Main mPage;
        private void myFrame_ContentRendered(object sender, EventArgs e)
        {
            UserPanel.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;
        }

        public MainWindow()
        {
            InitializeComponent();
            mPage = new Main();
            UserPanel.NavigationService.Navigate(mPage);

            ServerProxy.Instance.Start();

            discordRpcClient = new DiscordRpcClient("549220332527943701");
            discordRpcClient.Initialize();
            discordRpcClient.SetPresence(_presence);

            this.DataContext = new WindowViewModel(this);

            if (!System.IO.File.Exists("getLastError.exe"))
            {
                MessageBox.Show("Отсутствует необходимый файл для клиента \"getLastError.exe\".\nПожалуйста, переустановите клиент.\nКлиент завершает работу.", "World Evolved", MessageBoxButton.OK, MessageBoxImage.Error);
                Process.GetCurrentProcess().Kill();
            }

            readSaveData();

            if (!System.IO.File.Exists(saveWayToFileNFSW))
            {
                saveWayToFileNFSW = ""; updateSaveData("", 0);
            }

            if (saveWayToFileNFSW == "") setFileForGame();

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
                        mPage.email.Text = saveLogin;
                        mPage.CheckForRememberUser.IsChecked = true;
                        mPage.infAboutSavePass.Text = "Password saved";
                        //infAboutSavePass.Foreground = new System.Windows.Media.SolidColorBrush(Colors.Green);
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
            _presence.Details = "Загрузка игры";
            _presence.Timestamps = new Timestamps()
            {
                Start = DateTime.UtcNow
            };
            discordRpcClient.SetPresence(_presence);
            var nfswProcess = Process.Start(saveWayToFileNFSW, args);
            if (nfswProcess != null)
            {
                nfswProcess.EnableRaisingEvents = true;

                nfswProcess.Exited += (sender2, e2) => {
                    _presence.Details = "Открыт лаунчер";
                    _presence.Timestamps = new Timestamps()
                    {
                        Start = DateTime.UtcNow
                    };
                    discordRpcClient.SetPresence(_presence);
                    var exitCode = nfswProcess.ExitCode;

                    if (exitCode != 0)
                    {
                        if (!System.IO.File.Exists("getLastError.exe"))
                        {
                            MessageBox.Show("Отсутствует необходимый файл для клиента \"getLastError.exe\".\nПожалуйста, переустановите клиент.\nКлиент завершает работу.", "World Evolved", MessageBoxButton.OK, MessageBoxImage.Error);
                            Process.GetCurrentProcess().Kill();
                        }
                        if (!System.IO.File.Exists(wayToLog))
                        {
                            MessageBox.Show("Отсутствует Лог игры по пути \"" + wayToLog + "\"", "World Evolved", MessageBoxButton.OK, MessageBoxImage.Error);
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
                                    case 1001: MessageBox.Show("Отсутствует необходимый Лог игры по пути \"" + wayToLog + "\"\nИгра завершилась с ошибкой " + exitCode.ToString(), "World Evolved", MessageBoxButton.OK, MessageBoxImage.Error); break;
                                    case 5: MessageBox.Show("Невозможно подключиться к HTTP-части сервера\nИгра завершилась с ошибкой " + exitCode.ToString(), "World Evolved", MessageBoxButton.OK, MessageBoxImage.Error); break;
                                    case 6: MessageBox.Show("Потеря связи\n" + "Игра завершилась с ошибкой " + exitCode.ToString(), "World Evolved", MessageBoxButton.OK, MessageBoxImage.Error); break;
                                    case 7: MessageBox.Show("Что-то блокирует соединение с клиентом\nИгра завершилась с ошибкой " + exitCode.ToString(), "World Evolved", MessageBoxButton.OK, MessageBoxImage.Error); break;
                                    case 10: MessageBox.Show("Обрыв сессии, попробуйте ещё раз\nИгра завершилась с ошибкой " + exitCode.ToString(), "World Evolved", MessageBoxButton.OK, MessageBoxImage.Error); break;
                                    case 13: MessageBox.Show("Невозможно подключиться к XMPP-части сервера\nИгра завершилась с ошибкой " + exitCode.ToString(), "World Evolved", MessageBoxButton.OK, MessageBoxImage.Error); break;
                                    case 410: MessageBox.Show("Ваш аккаунт забанен\n", "World Evolved", MessageBoxButton.OK, MessageBoxImage.Error); break;
                                    case 500: MessageBox.Show("Внутренняя ошибка сервера\nИгра завершилась с ошибкой " + exitCode.ToString(), "World Evolved", MessageBoxButton.OK, MessageBoxImage.Error); break;

                                    default: MessageBox.Show("Ошибка со стороны сервера: " + error + "\nИгра завершилась с ошибкой " + exitCode.ToString(), "World Evolved", MessageBoxButton.OK, MessageBoxImage.Error); break;
                                }
                            };
                        }
                    }
                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate ()
                    {
                        PlayButton.IsEnabled = true;
                        PlayButton.Content = "LAUNCH";
                    });
                };

            }
        }

        void Press_Play(object sender, RoutedEventArgs e)
        {
            PlayButton.IsEnabled = false;
            PlayButton.Content = "LAUNCHED";
            string Login = "";
            string EncryptPass = "";

            if (saveLogin == "" && saveEncryptPass == "" && (mPage.email.Text.ToString() == "" || mPage.password.Password.ToString() == ""))
            {
                MessageBox.Show("Пожалуйста, введите данные для авторизации.", "World Evolved", MessageBoxButton.OK, MessageBoxImage.Warning);
                PlayButton.IsEnabled = true;
                PlayButton.Content = "LAUNCH";
                return;
            }
            if (saveLogin != "" && saveEncryptPass != "" && mPage.email.Text.ToString() != "" && mPage.password.Password.ToString() != "")
            {
                Login = mPage.email.Text.ToString();
                EncryptPass = SHA.HashPassword(mPage.password.Password.ToString()).ToLower();
            }
            if (saveLogin == "" && saveEncryptPass == "" && mPage.email.Text.ToString() != "" && mPage.password.Password.ToString() != "")
            {
                Login = mPage.email.Text.ToString();
                EncryptPass = SHA.HashPassword(mPage.password.Password.ToString()).ToLower();
            }
            if (saveEncryptPass != "" && mPage.email.Text.ToString() == saveLogin && mPage.password.Password.ToString() == "")
            {
                Login = saveLogin;
                EncryptPass = saveEncryptPass;
            }
            if (saveEncryptPass != "" && mPage.email.Text.ToString() != saveLogin && mPage.password.Password.ToString() == "")
            {
                Login = mPage.email.Text.ToString();
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
                            MessageBox.Show("Неверный логин или пароль.", "World Evolved", MessageBoxButton.OK, MessageBoxImage.Error);
                            updateSaveData("", 1);
                            updateSaveData("", 2);
                            PlayButton.IsEnabled = true;
                            PlayButton.Content = "LAUNCH";
                            mPage.infAboutSavePass.Text = "Password";
                            saveLogin = "";
                            saveEncryptPass = "";
                            return;
                        }
                        else
                        {
                            MessageBox.Show(Description, "World Evolved", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else if (childrenNode["LoginToken"].InnerText != "" && childrenNode["UserId"].InnerText != "")
                    {
                        if (mPage.CheckForRememberUser.IsChecked == true && mPage.email.Text.ToString() != "" && mPage.password.Password.ToString() != "")
                        {
                            updateSaveData(Login, 1);
                            updateSaveData(EncryptPass, 2);
                        }
                        if (mPage.CheckForRememberUser.IsChecked == false)
                        {
                            updateSaveData("", 1);
                            updateSaveData("", 2);
                        }

                        string cParams = "WORLDEVOLVED " + "http://127.0.0.1:6264/nfsw/Engine.svc" + " " + childrenNode["LoginToken"].InnerText + " " + childrenNode["UserId"].InnerText + " -advancedLaunch";

                        if (!System.IO.File.Exists(saveWayToFileNFSW))
                        {
                            saveWayToFileNFSW = ""; updateSaveData("", 0);
                            MessageBox.Show("Вы авторизовались, но файл игры(" + saveWayToFileNFSW + ") не был найден.\nПожалуйста, перезагрузите клиент.", "World Evolved", MessageBoxButton.OK, MessageBoxImage.Warning);
                            PlayButton.IsEnabled = true;
                            PlayButton.Content = "LAUNCH";
                            return;
                        }

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
                MessageBox.Show("Что-то пошло не так.");
            }

        }
        void recoveryAcc(object sender, RoutedEventArgs e)
        {
            if (mPage.email.Text.ToString() == "")
            {
                MessageBox.Show("Пожалуйста, введите почту для сброса пароля.", "World Evolved", MessageBoxButton.OK, MessageBoxImage.Question);
                return;
            }

            string responseString;
            try
            {
                Uri resetPasswordUrl = new Uri(serverIP + "/RecoveryPassword/forgotPassword");

                var request = (HttpWebRequest)System.Net.WebRequest.Create(resetPasswordUrl);
                var postData = "email=" + mPage.email.Text.ToString();
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
                MessageBox.Show("Что-то пошло не так.", "World Evolved", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (responseString[0] == 'L' && responseString[5] == 't' && responseString[23] == 's')//Link to reset password sent to: [e-mail]
            {
                MessageBox.Show("Ссылка для сброса пароля отправлена на: [" + mPage.email.Text.ToString() + "]", "World Evolved", MessageBoxButton.OK);
                return;
            }
            if (responseString == "ERROR: Recovery password link already sent, please check your spam mail box or try again in 1 hour.")
            {
                MessageBox.Show("ОШИБКА: Ссылка для восстановления пароля уже отправлена, проверьте почтовый ящик для спама или повторите попытку через 1 час.", "World Evolved", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (responseString == "ERROR: Invalid email!")
            {
                MessageBox.Show("ОШИБКА: Аккаунта с такой электронной почтой нет на сервере.", "World Evolved", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            MessageBox.Show(responseString, "World Evolved", MessageBoxButton.OK, MessageBoxImage.Warning);
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

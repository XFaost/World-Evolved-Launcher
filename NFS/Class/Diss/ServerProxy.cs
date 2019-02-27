using System;
using NFS.Class.Diss.Proxy;
using NFS.Class.Diss.Reborn;
using Nancy.Hosting.Self;
using System.Windows;

namespace NFS.Class.Diss
{
    public class ServerProxy : Singleton<ServerProxy>
    {
        private string _serverUrl;
        private string _serverName;
        private NancyHost _host;
        private bool _checkCar, _checkEvent, _checkLobby;

        public string GetServerUrl() => _serverUrl;
        public void SetServerUrl(string serverUrl)
        {
            _serverUrl = serverUrl;
        }

        public string GetServerName() => _serverName;
        public void SetServerName(string serverName)
        {
            _serverName = serverName;
        }

        public bool GetCheckCar() => _checkCar;
        public void SetCheckCar(bool checkCar)
        {
            _checkCar = checkCar;
        }

        public bool GetCheckEvent() => _checkEvent;
        public void SetCheckEvent(bool checkEvent)
        {
            _checkEvent = checkEvent;
        }

        public bool GetCheckLobby() => _checkLobby;
        public void SetCheckLobby(bool checkLobby)
        {
            _checkLobby = checkLobby;
        }

        public void Start()
        {
            if (_host != null)
            {
                throw new Exception("Server already running!");
            }

            _host = new NancyHost(new Uri("http://127.0.0.1:6264"), new NancyBootstrapper(), new HostConfiguration
            {
                AllowChunkedEncoding = false,
                UrlReservations = new UrlReservations() { CreateAutomatically = true }
            });
            _host.Start();
        }

        public void Stop()
        {
            _host?.Stop();
        }
    }
}

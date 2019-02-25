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

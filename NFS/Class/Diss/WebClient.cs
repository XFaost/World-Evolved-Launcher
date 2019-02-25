using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO.Compression;
using System.Windows;
using System.IO;
using System.Security.Cryptography;
using NFS.Class.Diss;
using GameLauncher.HashPassword;

namespace NFS.Class.Diss.Reborn
{
    public class WebClientWithTimeout : WebClient
    {
        private static string GameLauncherHash = string.Empty;
        public static string Value()
        {
            if (string.IsNullOrEmpty(GameLauncherHash))
            {
                GameLauncherHash = SHA.HashFile(AppDomain.CurrentDomain.FriendlyName);
            }

            return GameLauncherHash;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(address);
            request.UserAgent = "GameLauncher (+https://github.com/SoapboxRaceWorld/GameLauncher_NFSW)"; //this must remain untouched.
            request.Headers["X-UserAgent"] = "GameLauncherReborn WinForms (+https://github.com/worldunitedgg/GameLauncher_NFSW)";
            request.Headers["X-GameLauncherHash"] = Value();
            request.Timeout = 30000;

            return request;
        }
    }
}
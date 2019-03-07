using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using Ionic.Zip;
using System.ComponentModel;
using System.Runtime.InteropServices;



namespace Update
{
    class Program
    {
        [STAThread]
        [DllImport("User32.dll", CharSet = CharSet.Unicode)]

        public static extern int MessageBox(IntPtr h, string m, string c, int type);

        static void downloadGame()
        {
            Console.WriteLine("\nConnected to FTP...");
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://ch56558@vh174.timeweb.ru/launcherUpdate.zip");
            request.Credentials = new NetworkCredential("ch56558", "Rj8S7dvnhbqf");
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            long sizeFile = response.ContentLength;

            using (Stream ftpStream = request.GetResponse().GetResponseStream())
            using (Stream fileStream = File.Create(@"launcherUpdate.zip"))
            {
                Console.WriteLine("\nStart download");

                byte[] buffer = new byte[10240];
                int read;
                while ((read = ftpStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileStream.Write(buffer, 0, read);

                    Console.WriteLine("Downloading | {0} % | {1}MB / {2}MB",
                        ((fileStream.Position * 100) / sizeFile).ToString(),
                        Math.Round((double)fileStream.Position / (double)1000000, 2).ToString("N2"),
                        Math.Round((double)sizeFile / (double)1000000, 2).ToString("N2")
                        );
                    Console.SetCursorPosition(0, Console.CursorTop - 1);

                }
            }

            Console.WriteLine("\nDownloaded!\n\nStart Extract");

            ZipFile zip = ZipFile.Read(@"launcherUpdate.zip");
            zip.ExtractProgress += zip_ExtractProgress;

            SynchronizationContext context = SynchronizationContext.Current;
            ExtractAsync(@".", zip);

            Console.WriteLine("\n\nDone!");
            File.Delete(@"launcherUpdate.zip");

            MessageBox((IntPtr)0, "Update completed successfully\nОбновление завершилось успешно", "World Evolved", 0);
            Process.Start(@"WEX.exe");
            return;
        }

        static void ExtractAsync(string to, ZipFile zip)
        {
            zip.ExtractAll(to, ExtractExistingFileAction.OverwriteSilently);
            zip.Dispose();
        }

        static void zip_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            switch (e.EventType)
            {
                case ZipProgressEventType.Extracting_AfterExtractEntry:
                    Console.WriteLine("Extracting | {0}%", (e.EntriesExtracted * 100) / e.EntriesTotal);
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    break;
            }
        }

        
        static void Main(string[] args)
        {
            downloadGame();
            return;
        }
    }
}

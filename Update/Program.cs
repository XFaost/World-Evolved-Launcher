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
            Console.WriteLine("\nDownloading...");

            using (var client = new WebClient())
            {
                client.DownloadFile("http://world-evolved.ru/launcher/launcherUpdate.zip", "launcherUpdate.zip");
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

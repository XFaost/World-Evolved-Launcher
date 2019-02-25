using System.IO;
using System.IO.Compression;
using System.Linq;
using NFS.Class.Diss.Reborn;
using Nancy;
using Nancy.Bootstrapper;
using System.Windows;

namespace NFS.Class.Diss.Proxy
{
    public class NancyGzipCompression : IApplicationStartup
    {
        public void Initialize(IPipelines pipelines)
        {
            //MessageBox.Show("1");
            pipelines.AfterRequest += CheckForCompression;
        }

        private static void CheckForCompression(NancyContext context)
        {
            if (!RequestIsGzipCompatible(context.Request))
            {
                return;
            }

            CompressResponse(context.Response);
        }

        private static void CompressResponse(Response response)
        {
            response.Headers["Content-Encoding"] = "gzip";
            response.Headers["connection"] = "close";

            var content = new MemoryStream();

            response.Contents(content);

            content.Position = 0;

            response.Contents = stream =>
            {
                using (var gzip = new GZipStream(stream, CompressionMode.Compress, true))
                {
                    gzip.Write(content.ToArray(), 0, (int)content.Length);
                }
            };
        }

        private static bool RequestIsGzipCompatible(Request request)
        {
            return request.Headers.AcceptEncoding.Any(x => x.Contains("gzip"));
        }
    }
}

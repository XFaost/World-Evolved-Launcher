﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Flurl.Http;
using Flurl.Http.Content;
using NFS.Class.Diss;
using NFS.Class.Diss.RPC;
using NFS.Class.Diss.Reborn;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Extensions;
using Nancy.Responses;
using System.Windows;

namespace NFS.Class.Diss.Proxy
{
    public class ProxyHandler : IApplicationStartup
    {
        public void Initialize(IPipelines pipelines)
        {
            //MessageBox.Show("2");
            pipelines.BeforeRequest += ProxyRequest;
        }

        public static Dictionary<string, int> executedPowerupsRemainingSecs = new Dictionary<string, int>();
        public static Dictionary<string, bool> executedPowerups = new Dictionary<string, bool>();
        public static bool activated;
        private static readonly bool checkOnline = ServerProxy.Instance.GetCheckOnline();

        private static Response ProxyRequest(NancyContext context)
        {
            //MessageBox.Show("ProxyRequest");
            string POSTContent = String.Empty;
            string GETContent = String.Empty;

            Self.sendRequest = true;

            var serverUrl = ServerProxy.Instance.GetServerUrl();

            if (string.IsNullOrEmpty(serverUrl))
            {
                return new TextResponse(HttpStatusCode.BadGateway, "Not open for business");
            }

            var queryParams = new Dictionary<string, object>();
            var headers = new Dictionary<string, object>();

            if (Self.sendRequest == true)
            {
                var fixedPath = context.Request.Path.Replace("/nfsw/Engine.svc", "");
                var fullUrl = new Uri(serverUrl).Append(fixedPath);

                foreach (var param in context.Request.Query)
                {
                    var value = context.Request.Query[param];
                    queryParams[param] = value;
                }

                GETContent = string.Join(";", queryParams.Select(x => x.Key + "=" + x.Value).ToArray());

                foreach (var header in context.Request.Headers)
                {
                    headers[header.Key] = (header.Key == "Host") ? fullUrl.Host : header.Value.First();
                }

                var url = new Flurl.Url(fullUrl.ToString()).SetQueryParams(queryParams).WithHeaders(headers);
                HttpResponseMessage response;

                switch (context.Request.Method)
                {
                    case "GET":
                        {
                            response = url.GetAsync().Result;
                            break;
                        }
                    case "POST":
                        {
                            POSTContent = context.Request.Body.AsString();
                            response = url.PostAsync(
                                new CapturedStringContent(
                                    POSTContent
                                )
                            ).Result;
                            break;
                        }
                    case "PUT":
                        {
                            response = url.PutAsync(
                                new CapturedStringContent(
                                    context.Request.Body.AsString()
                                )
                            ).Result;
                            break;
                        }
                    case "DELETE":
                        {
                            response = url.DeleteAsync().Result;
                            break;
                        }
                    default:
                        {
                            throw new Exception($"unsupported method: {context.Request.Method}");
                        }
                }

                String replyToServer = response.Content.ReadAsStringAsync().Result;

                /*if (fixedPath == "/getrebroadcasters"){
                    replyToServer = Regex.Replace(replyToServer, @"<Host>(.*?)<\/Host>", "<Host>37.233.101.12</Host>");
                    replyToServer = Regex.Replace(replyToServer, @"<Port>(.*?)<\/Port>", "<Port>9999</Port>");
                }*/

                if (fixedPath == "/User/GetPermanentSession")
                {
                    replyToServer = Self.CleanFromUnknownChars(replyToServer);

                    var SBRW_XML = new XmlDocument();
                    SBRW_XML.LoadXml(replyToServer);
                    XmlNode UserInfo = SBRW_XML.SelectSingleNode("UserInfo");
                    XmlNodeList personas = UserInfo.SelectNodes("personas/ProfileData");

                    if (personas.Count == 0)
                    {
                        replyToServer = replyToServer.Replace("false", "true");
                    }
                }

                if (fixedPath == "")
                {

                }

                if(checkOnline)
                    DiscordGamePresence.handleGameState(fixedPath, replyToServer, POSTContent, GETContent);

                return new TextResponse(replyToServer, response.Content.Headers.ContentType.ToString()) { StatusCode = (HttpStatusCode)(int)response.StatusCode };
            }
            else
            {
                var fullUrl = new Uri(Self.internetcheckurl);

                foreach (var param in context.Request.Query)
                {
                    var value = context.Request.Query[param];
                    queryParams[param] = value;
                }

                foreach (var header in context.Request.Headers)
                {
                    headers[header.Key] = (header.Key == "Host") ? fullUrl.Host : header.Value.First();
                }

                var url = new Flurl.Url(fullUrl.ToString()).SetQueryParams().WithHeaders(headers);

                HttpResponseMessage response;
                switch (context.Request.Method)
                {
                    case "GET": { response = url.GetAsync().Result; break; }
                    case "POST": { POSTContent = context.Request.Body.AsString(); response = url.PostAsync(new CapturedStringContent(POSTContent)).Result; break; }
                    default: { throw new Exception($"unsupported method: {context.Request.Method}"); }
                }

                Self.sendRequest = true;
                return new TextResponse(String.Empty, response.Content.Headers.ContentType.ToString()) { StatusCode = (HttpStatusCode)(int)response.StatusCode };
            }
        }
    }
}

﻿using DiscordRPC;
using NFS.Class.Diss.Reborn;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace NFS.Class.Diss.RPC
{
    class DiscordGamePresence
    {
        private static DateTime RPCstartTimestamp;

        private static readonly bool checkCar = ServerProxy.Instance.GetCheckCar(), checkEvent = ServerProxy.Instance.GetCheckEvent(), checkLobby = ServerProxy.Instance.GetCheckLobby();

        //Some checks
        private static readonly string serverName = ServerProxy.Instance.GetServerName();
        private static bool canUpdateProfileField = false;
        private static bool eventTerminatedManually = false;
        private static int EventID;
        private static string carslotsXML = string.Empty;

        //Some data related, can be touched.
        public static string PersonaId = string.Empty;
        public static string PersonaName = string.Empty;
        public static string PersonaLevel = string.Empty;
        public static string PersonaAvatarId = string.Empty;
        public static string PersonaCarId = string.Empty;
        public static string PersonaCarName = string.Empty;
        public static List<string> PersonaIds = new List<string>();

        static string getStrFromResource(string key)
        {
            return (string)Application.Current.Resources[key];
        }

        public static void handleGameState(string uri, string serverreply = "", string POST = "", string GET = "")
        {
            RPCstartTimestamp = DateTime.Now;

            var SBRW_XML = new XmlDocument();
            string[] splitted_uri = uri.Split('/');

            if (uri == "/User/SecureLoginPersona")
            {
                canUpdateProfileField = true;
            }
            if (uri == "/User/SecureLogoutPersona")
            {
                PersonaId =         string.Empty;
                PersonaName =       string.Empty;
                PersonaLevel =      string.Empty;
                PersonaAvatarId =   string.Empty;
                PersonaCarId =      string.Empty;
                PersonaCarName =    string.Empty;
            }

            //FIRST PERSONA EVER LOCALIZED IN CODE
            if (uri == "/User/GetPermanentSession")
            {
                try
                {
                    SBRW_XML.LoadXml(serverreply);

                    PersonaName = SBRW_XML.SelectSingleNode("UserInfo/personas/ProfileData/Name").InnerText.Replace("¤", "[S]");
                    PersonaLevel = SBRW_XML.SelectSingleNode("UserInfo/personas/ProfileData/Level").InnerText;
                    PersonaAvatarId = (SBRW_XML.SelectSingleNode("UserInfo/personas/ProfileData/IconIndex").InnerText == "26") ? "nfsw" : "avatar_" + SBRW_XML.SelectSingleNode("UserInfo/personas/ProfileData/IconIndex").InnerText;
                    PersonaId = SBRW_XML.SelectSingleNode("UserInfo/personas/ProfileData/PersonaId").InnerText;

                    //Let's get rest of PERSONAIDs
                    XmlNode UserInfo = SBRW_XML.SelectSingleNode("UserInfo");
                    XmlNodeList personas = UserInfo.SelectNodes("personas/ProfileData");
                    foreach (XmlNode node in personas)
                    {
                        PersonaIds.Add(node.SelectSingleNode("PersonaId").InnerText);
                    }
                }
                catch (Exception)
                {

                }
            }

            //CREATE/DELETE PERSONA Handler
            if (uri == "/DriverPersona/CreatePersona")
            {
                SBRW_XML.LoadXml(serverreply);
                PersonaIds.Add(SBRW_XML.SelectSingleNode("ProfileData/PersonaId").InnerText);
            }

            //DRIVING CARNAME
            if (uri == "/DriverPersona/GetPersonaInfo" && canUpdateProfileField == true)
            {
                SBRW_XML.LoadXml(serverreply);
                PersonaName = SBRW_XML.SelectSingleNode("ProfileData/Name").InnerText.Replace("¤", "[S]");
                PersonaLevel = SBRW_XML.SelectSingleNode("ProfileData/Level").InnerText;
                PersonaAvatarId = (SBRW_XML.SelectSingleNode("ProfileData/IconIndex").InnerText == "26") ? "nfsw" : "avatar_" + SBRW_XML.SelectSingleNode("ProfileData/IconIndex").InnerText;
                PersonaId = SBRW_XML.SelectSingleNode("ProfileData/PersonaId").InnerText;
            }
            if (uri == "/matchmaking/leavelobby")
            {
                
                if (checkCar)
                {
                    MainWindow._presence.Details = getStrFromResource("driving") + PersonaCarName;
                    MainWindow._presence.Timestamps = GetCurrentTimestamp();

                }
                else MainWindow._presence.Details = getStrFromResource("dissOnline");

                MainWindow._presence.State = string.Empty;
                MainWindow.discordRpcClient.SetPresence(MainWindow._presence);

                eventTerminatedManually = true;
            }

            //IN LOBBY
            if (uri == "/matchmaking/acceptinvite")
            {
                SBRW_XML.LoadXml(serverreply);

                if (checkLobby)
                {
                    EventID = Convert.ToInt32(SBRW_XML.SelectSingleNode("LobbyInfo/EventId").InnerText);
                    MainWindow._presence.State = getStrFromResource("dissLobby") + EventList.getEventName(EventID);
                    MainWindow._presence.Timestamps = GetCurrentTimestamp();
                }
                else MainWindow._presence.State = "";

                if (checkCar)
                {
                    MainWindow._presence.Details = getStrFromResource("driving") + PersonaCarName;
                    MainWindow._presence.Timestamps = GetCurrentTimestamp();
                }
                else MainWindow._presence.Details = getStrFromResource("dissOnline");

                MainWindow.discordRpcClient.SetPresence(MainWindow._presence);

                eventTerminatedManually = false;
            }

            //IN SAFEHOUSE/FREEROAM
            if (uri == "/DriverPersona/UpdatePersonaPresence")
            {
                string UpdatePersonaPresenceParam = GET.Split(';').Last().Split('=').Last();
                if (UpdatePersonaPresenceParam == "1")
                {
                    if (checkCar)
                    {
                        MainWindow._presence.Details = getStrFromResource("driving") + PersonaCarName;
                    }
                    else MainWindow._presence.Details = getStrFromResource("dissOnline");
                }
                else
                {
                    MainWindow._presence.Details = getStrFromResource("safehouse");
                    MainWindow._presence.Timestamps = GetCurrentTimestamp();
                }
                MainWindow._presence.State = string.Empty;

                MainWindow.discordRpcClient.SetPresence(MainWindow._presence);
            }

            //IN EVENT
            if (Regex.Match(uri, "/matchmaking/launchevent").Success)
            {
                if (checkEvent)
                {
                    EventID = Convert.ToInt32(splitted_uri[3]);
                    MainWindow._presence.State = getStrFromResource("dissEvent") + EventList.getEventName(EventID);
                    MainWindow._presence.Timestamps = GetCurrentTimestamp();
                }
                else MainWindow._presence.State = "";

                if (checkCar)
                {
                    MainWindow._presence.Details = getStrFromResource("driving") + PersonaCarName;
                    MainWindow._presence.Timestamps = GetCurrentTimestamp();
                }
                else MainWindow._presence.Details = getStrFromResource("dissOnline");

                MainWindow.discordRpcClient.SetPresence(MainWindow._presence);

                eventTerminatedManually = false;
            }
            if (uri == "/event/arbitration")
            {
                if (checkEvent)
                {
                    MainWindow._presence.State = getStrFromResource("dissEvent") + EventList.getEventName(EventID);
                    MainWindow._presence.Timestamps = GetCurrentTimestamp();
                }
                else MainWindow._presence.State = "";

                if (checkCar)
                {
                    MainWindow._presence.Details = getStrFromResource("driving") + PersonaCarName;
                    MainWindow._presence.Timestamps = GetCurrentTimestamp();
                }
                else MainWindow._presence.Details = getStrFromResource("dissOnline");

                MainWindow.discordRpcClient.SetPresence(MainWindow._presence);

                eventTerminatedManually = false;
            }
            if (uri == "/event/launched" && eventTerminatedManually == false)
            {
                if (checkEvent)
                {
                    MainWindow._presence.State = getStrFromResource("dissEvent") + EventList.getEventName(EventID);
                    MainWindow._presence.Timestamps = GetCurrentTimestamp();
                }
                else MainWindow._presence.State = "";

                if (checkCar)
                {
                    MainWindow._presence.Details = getStrFromResource("driving") + PersonaCarName;
                    MainWindow._presence.Timestamps = GetCurrentTimestamp();
                }
                else MainWindow._presence.Details = getStrFromResource("dissOnline");

                MainWindow.discordRpcClient.SetPresence(MainWindow._presence);
            }

            //CARS RELATED
            foreach (var single_personaId in PersonaIds)
            {
                if (Regex.Match(uri, "/personas/" + single_personaId + "/carslots", RegexOptions.IgnoreCase).Success)
                {
                    carslotsXML = serverreply;

                    SBRW_XML.LoadXml(carslotsXML);

                    int DefaultID = Convert.ToInt32(SBRW_XML.SelectSingleNode("CarSlotInfoTrans/DefaultOwnedCarIndex").InnerText);
                    int current = 0;

                    XmlNode CarsOwnedByPersona = SBRW_XML.SelectSingleNode("CarSlotInfoTrans/CarsOwnedByPersona");
                    XmlNodeList OwnedCarTrans = CarsOwnedByPersona.SelectNodes("OwnedCarTrans");

                    foreach (XmlNode node in OwnedCarTrans)
                    {
                        if (DefaultID == current)
                        {
                            PersonaCarName = CarList.getCarName(node.SelectSingleNode("CustomCar/Name").InnerText);
                        }
                        current++;
                    }
                }
                if (Regex.Match(uri, "/personas/" + single_personaId + "/defaultcar", RegexOptions.IgnoreCase).Success)
                {
                    if (splitted_uri.Last() != "defaultcar")
                    {
                        string receivedId = splitted_uri.Last();

                        SBRW_XML.LoadXml(carslotsXML);
                        XmlNode CarsOwnedByPersona = SBRW_XML.SelectSingleNode("CarSlotInfoTrans/CarsOwnedByPersona");
                        XmlNodeList OwnedCarTrans = CarsOwnedByPersona.SelectNodes("OwnedCarTrans");

                        foreach (XmlNode node in OwnedCarTrans)
                        {
                            if (receivedId == node.SelectSingleNode("Id").InnerText)
                            {
                                PersonaCarName = CarList.getCarName(node.SelectSingleNode("CustomCar/Name").InnerText);
                            }
                        }
                    }
                }
            }
        }

        public static Timestamps GetCurrentTimestamp()
        {
            return Timestamps.Now;
        }
    }
}
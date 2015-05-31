﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using UnityEngine;
using ResponseObjects;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.IO;
using Assets.Scripts.networking;
using System.Net.Security;
using System.Collections.Specialized;

namespace Assets.Scripts.networking
{
    class DatabaseController
    {
        private static string BASE_URL = "http://www.cs.drexel.edu/~jgm55/fitbit/";
        private static string UPDATE_URL = BASE_URL + "updateUser.php";
        private static string GET_FRIENDS = BASE_URL + "fetchUsers.php";

        private static List<PlayerStats> friendsList = null;

        /**
         * Sends player stats to the server for storing
         * GET to update TODO should be POST
         * */
        public static void updatePlayer(PlayerStats stats){
            Debug.Log("Updating player");
            if (stats == null)
            {
                Debug.Log("Player is null returning");
                return;
            }
            Thread oThread = new Thread(new ThreadStart(() =>
            {
                Debug.Log("Starting thread");
                //Serialize data to string
                string serializedStats = serializeDataToString(stats);
                Debug.Log("stats: " + serializedStats);
                
                //Add info to postData
                var queryParam = "?id=" + stats.id;
                queryParam += "&stats=" + WWW.EscapeURL(serializedStats);

                var request = (HttpWebRequest)WebRequest.Create(UPDATE_URL + queryParam);
                setUpHeaders(request);

                ServicePointManager.ServerCertificateValidationCallback +=
                    new RemoteCertificateValidationCallback(
                        (sender, certificate, chain, policyErrors) => { return true; });
                HttpWebResponse response;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (Exception e)
                {
                    Debug.Log("Exception in updatePlayer(): "+e);
                    return;
                }
                using (response)
                {
                    //TODO do better error catching
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Debug.Log("There's been a problem trying to access the database:" +
                                    Environment.NewLine +
                                    response.StatusDescription);
                    }
                    else
                    {
                        Debug.Log("Updated Successfully: "+Utilities.getStringFromResponse(response));
                    }
                }
            }));
            oThread.Start();
        }
        /**
         * Updates the FriendsLst in the background. 
         * Takes a list of the Friend Ids from fitbit
         * */
        //TODO make this private
        public static void updateFriendsList(List<string> friendIds)
        {
            Debug.Log("Getting Friend Stats");
            List<PlayerStats> friendStats = new List<PlayerStats>();
            Thread oThread = new Thread(new ThreadStart(() =>
            {
                Debug.Log("getFriends()");
                HttpWebResponse response;

                try
                {
                    //StreamWriter dataStream = new StreamWriter(request.GetRequestStream());
                    var queryParam = "?a=a";
                    foreach (string friendId in friendIds)
                    {
                        //dataStream.Write("friendId[]="+ friendId);
                        queryParam += "&friendId[]=" + WWW.EscapeURL(friendId);
                    }
                    var request = (HttpWebRequest)WebRequest.Create(GET_FRIENDS + queryParam);
                    setUpHeaders(request);

                    ServicePointManager.ServerCertificateValidationCallback +=
                        new RemoteCertificateValidationCallback(
                            (sender, certificate, chain, policyErrors) => { return true; });                
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (Exception e)
                {
                    Debug.Log("Exception in updateFriendsList(): "+e);
                    return;
                }
                using (response)
                {
                    //TODO do better error catching
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Debug.Log("There's been a problem trying to access the database:" +
                                    Environment.NewLine +
                                    response.StatusDescription);
                    }
                    else
                    {
                        string line = Utilities.getStringFromResponse(response);
                        JSONObject lineObj = new JSONObject(line);
                        lineObj.GetField("friends", delegate(JSONObject idList)
                        {
                            Debug.Log("idlist: "+idList);
                            foreach (JSONObject obj in idList.list)
                            {
                                Debug.Log("obj: "+obj);
                                obj.GetField("stats", delegate(JSONObject stats)
                                {
                                    stats = new JSONObject(WWW.UnEscapeURL(stats.ToString()));
                                    PlayerStats playerStats = new PlayerStats(stats);
                                    friendsList.Add(playerStats);
                                    Debug.Log("playerstats: "+playerStats);
                                });
                            }
                        });
                        
                        
                        Debug.Log(line);

                    }
                }
            }));
            oThread.Start();
        }

        /**
         * Gets the game data for the given friend ids
         * */
        public static List<PlayerStats> getFriends()
        {
            return friendsList;
        }

        private static string serializeDataToString(JSONable objectToSerialize){
            return objectToSerialize.getJSON().Print();
        }

        /**
         * Sets up GET headers for the calls in this function
         * */
        private static void setUpHeaders(HttpWebRequest request)
        {
            request.Method = "GET";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.ContentType = "application/x-www-form-urlencoded";
            request.AllowAutoRedirect = true;
            request.MaximumAutomaticRedirections = 2;
        }
    }
}

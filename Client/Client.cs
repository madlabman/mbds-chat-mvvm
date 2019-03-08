using System;
using System.Collections.Generic;
using System.Linq;
using ChatApp.Core;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ChatApp.Client
{
    public class Client
    {
        private RestClient _restClient;
        private string _apiKey;
        private bool _isSignedIn = false;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public Client()
        {
            const string apiBase = "http://138.197.184.164:40080"; // Hardcoded Uri of the backend.
            _restClient = new RestClient(apiBase);
        }

        public bool SignIn(string login, string password)
        {
            try
            {
                var request = new RestRequest("/signin");
                request.AddParameter("login", login);
                request.AddParameter("password", password);
                var response = _restClient.Post(request);

                var jObject = JObject.Parse(response.Content);
                var error = (string) jObject["error"];
                if (string.IsNullOrEmpty(error))
                {
                    var apiKey = (string) jObject["api_token"];
                    if (!string.IsNullOrEmpty(apiKey))
                    {
                        _apiKey = apiKey;
                        _isSignedIn = true;
                        return true;
                    }
                }
                else
                {
                    Logger.Debug("Error while signing in: {0}", error);
                }
            }
            catch (Exception exception)
            {
                Logger.Warn("Error while signing in: {0}", exception.Message);
            }

            return false;
        }

        public bool SignUp(string login, string name, string password, string publicKey)
        {
            try
            {
                var request = new RestRequest("/signup");
                request.AddParameter("name", name);
                request.AddParameter("login", login);
                request.AddParameter("password", password);
                request.AddParameter("public_key", publicKey);

                var response = _restClient.Post(request);
                var jObject = JObject.Parse(response.Content);
                if (jObject.ContainsKey("success"))
                {
                    var apiKey = (string) jObject["api_token"];
                    if (!string.IsNullOrEmpty(apiKey))
                    {
                        _apiKey = apiKey;
                        _isSignedIn = true;
                        return true;
                    }
                }
                else
                {
                    Logger.Info("Unable to sign up.");
                    foreach (var error in jObject.Children().Children().Children()) // TODO: fix this ugly construction
                    {
                        Logger.Debug("Error while signing up: {0}", error.ToString());
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Warn("Error while signing up: {0}", exception.Message);
            }

            return false;
        }

        public List<Dialog> GetDialogs()
        {
            if (!_isSignedIn)
            {
                return null;
            }

            try
            {
                var request = new RestRequest("/dialogs/list");
                request.AddParameter("api_token", _apiKey);
                var response = _restClient.Get(request);

                var jObject = JObject.Parse(response.Content);
                if (jObject.ContainsKey("dialogs"))
                {
                    var dialogsList = jObject["dialogs"].Select(dialogJson =>
                        new Dialog() {Partner = _parseUserFromJObject(dialogJson["user"])}).ToList();
                    return dialogsList;
                }
            }
            catch (Exception exception)
            {
                Logger.Warn("Error while fetching user dialogs: {0}", exception.Message);
            }


            return null;
        }

        public User FetchUser(string login)
        {
            if (!_isSignedIn)
            {
                return null;
            }

            try
            {
                var request = new RestRequest("/user/{login}");
                request.AddUrlSegment("login", login);
                request.AddParameter("api_token", _apiKey);
                var response = _restClient.Get(request);

                var jObject = JObject.Parse(response.Content);
                if (jObject.ContainsKey("id")) // Assume that successfully retrieve user
                {
                    return _parseUserFromJObject(jObject);
                }
            }
            catch (Exception exception)
            {
                Logger.Warn("Error while fetching user info: {0}", exception.Message);
            }

            Logger.Info("User {0} not found.", login);
            return null;
        }

        public bool AddDialog(User user)
        {
            if (!_isSignedIn)
            {
                return false;
            }

            try
            {
                var request = new RestRequest("/dialogs/{login}/add");
                request.AddParameter("api_token", _apiKey);
                request.AddUrlSegment("login", user.Login);
                var response = _restClient.Get(request);

                var jObject = JObject.Parse(response.Content);
                if (jObject.ContainsKey("success"))
                {
                    var success = (bool) jObject["success"];
                    if (!success)
                    {
                        Logger.Debug("Error while sending add dialog request: {0}", (string) jObject["error"]);
                    }

                    return success;
                }
            }
            catch (Exception exception)
            {
                Logger.Warn("Error while sending add dialog request: {0}", exception.Message);
            }

            return false;
        }

        public List<Message> GetMessages(Dialog dialog)
        {
            if (!_isSignedIn)
            {
                return null;
            }

            try
            {
                var request = new RestRequest("/dialogs/{login}/history");
                request.AddUrlSegment("login", dialog.Partner.Login);
                request.AddParameter("api_token", _apiKey);
                var response = _restClient.Get(request);

                var jObject = JObject.Parse(response.Content);
                if (jObject.ContainsKey("messages"))
                {
                    var messagesList = jObject["messages"]
                        .Select(message => Message.FromJsonString((string) message["content"])).ToList();
                    return messagesList;
                }
            }
            catch (Exception exception)
            {
                Logger.Warn("Error while fetching messages: {0}", exception.Message);
            }

            Logger.Info("Unable to fetch messages from dialog with {0}.", dialog.Partner.Login);

            return null;
        }

        public bool SendMessage(Dialog dialog, Message message)
        {
            if (!_isSignedIn)
            {
                return false;
            }

            try
            {
                var request = new RestRequest("/dialogs/{login}/message");
                request.AddParameter("api_token", _apiKey);
                request.AddParameter("content", message.ToJsonString());
                request.AddUrlSegment("login", dialog.Partner.Login);
                var response = _restClient.Post(request);

                var jObject = JObject.Parse(response.Content);
                if (jObject.ContainsKey("success"))
                {
                    var success = (bool) jObject["success"];
                    if (!success)
                    {
                        Logger.Debug("Error while sending message: {0}", (string) jObject["error"]);
                    }

                    return success;
                }
            }
            catch (Exception exception)
            {
                Logger.Warn("Error while sending message: {0}", exception.Message);
            }

            return false;
        }

        private User _parseUserFromJObject(JToken jObject)
        {
            return new User()
            {
                Uuid = (string) jObject["id"],
                Login = (string) jObject["login"],
                Name = (string) jObject["name"],
                PublicKeyXml = (string) jObject["public_key"]
            };
        }
    }
}
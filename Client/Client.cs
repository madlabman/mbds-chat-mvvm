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
        public List<string> Errors { get; }

        public Client()
        {
            const string apiBase = "http://138.197.184.164:40080"; // Hardcoded Uri of the backend.
            _restClient = new RestClient(apiBase);
            Errors = new List<string>();
        }

        public bool SignIn(string login, string password)
        {
            Errors.Clear();

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
                Errors.Add(error);
            }

            return false;
        }

        public bool SignUp(string login, string name, string password, string publicKey)
        {
            Errors.Clear();

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
                foreach (var error in jObject.Children().Children().Children()) // TODO: fix this ugly construction
                {
                    Errors.Add(error.ToString());
                }
            }

            return false;
        }

        public List<Dialog> GetDialogs()
        {
            if (!_isSignedIn)
            {
                return null;
            }

            Errors.Clear();

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

            return null;
        }

        public User FetchUser(string login)
        {
            if (!_isSignedIn)
            {
                return null;
            }

            Errors.Clear();

            var request = new RestRequest("/user/{login}");
            request.AddUrlSegment("login", login);
            request.AddParameter("api_token", _apiKey);
            var response = _restClient.Get(request);

            var jObject = JObject.Parse(response.Content);
            if (jObject.ContainsKey("id")) // Assume that successfully retrieve user
            {
                return _parseUserFromJObject(jObject);
            }

            Errors.Add("User not found.");
            return null;
        }

        public bool AddDialog(User user)
        {
            if (!_isSignedIn)
            {
                return false;
            }

            Errors.Clear();

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
                    Errors.Add((string) jObject["error"]);
                }

                return true;
            }

            return false;
        }

        public List<Message> GetMessages(Dialog dialog)
        {
            if (!_isSignedIn)
            {
                return null;
            }

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

            return null;
        }

        public bool SendMessage(Dialog dialog, Message message)
        {
            if (!_isSignedIn)
            {
                return false;
            }

            Errors.Clear();

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
                    Errors.Add((string) jObject["error"]);
                }

                return true;
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
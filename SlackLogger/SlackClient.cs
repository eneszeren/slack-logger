using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SlackLogger
{
    public class SlackClient
    {

        private string _url;
        private readonly HttpClient _appClient;
        private readonly HttpClient _botClient;
        private string channelList { get; set; }
        private string userList { get; set; }
        private string _botName { get; set; }
        private string _prefix { get; set; }
        private Dictionary<Options, string> eventLinks = new Dictionary<Options, string> {
            { Options.CreateChannel, "https://slack.com/api/conversations.create"},
            { Options.SendMessage,"https://slack.com/api/chat.postMessage"},
            { Options.ChannelList,"https://slack.com/api/conversations.list"},
            { Options.InviteChannel,"https://slack.com/api/conversations.invite"},
            { Options.UserList,"https://slack.com/api/users.list"}
        };
        private enum Options
        {
            CreateChannel = 1,
            SendMessage = 2,
            ChannelList = 3,
            InviteChannel = 4,
            UserList = 5
        }

        public SlackClient(SlackDto.Setting slack)
        {
            _botName = slack.BotName;
            _prefix = slack.Prefix;

            _appClient = new HttpClient();

            _botClient = new HttpClient();

            _appClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", slack.AppToken);
            _botClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", slack.BotToken);

            Task.WaitAll(GetChannelList());
        }

        public async void SendMessage(string msg, SlackDto.SlackType type, bool isAttachment = false)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            await WriteMessage(msg, type.ToString().ToLower(), isAttachment);
        }

        public async void SendMessage(string msg, string channelName)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            await WriteMessage(msg, channelName.ToLower());
        }

        public async Task InviteChannel(string channelName, string userName)
        {
            try
            {
                #region Fill ChannelID
                Task.WaitAll(GetChannelList());

                JObject channelObject = ContentParser(channelList);

                JToken channels = channelObject["channels"];
                string channelID = string.Empty;

                for (int i = 0; i >= 0; i++)
                {
                    try
                    {
                        if (channels[i]["name"].ToString() == channelName)
                        {
                            channelID = channels[i]["id"].ToString();
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
                #endregion

                #region Fill Bot ID

                Task.WaitAll(GetBotList());

                JObject userObject = ContentParser(userList);

                JToken users = userObject["members"];
                string botID = string.Empty;
                for (int i = 0; i >= 0; i++)
                {
                    try
                    {
                        if (users[i]["real_name"].ToString() == userName)
                        {
                            botID = users[i]["id"].ToString();
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
                #endregion
                var postObject = new { channel = channelID, users = botID };
                await AppRequest(eventLinks[Options.InviteChannel], HttpMethod.Post, postObject);
            }
            catch
            {

            }
        }

        public async Task WriteMessage(string message, string channelName, bool isAttachment = false)
        {
            try
            {

                channelName = string.Concat(_prefix, "_", channelName);
                bool channelExist = channelIsExist(channelName);
                if (!channelExist)
                {
                    Task.WaitAll(CreateChannel(channelName));
                    await InviteChannel(channelName, _botName);
                }

                if (isAttachment)
                {
                    var postObject = new { channel = "#" + channelName, attachments = message, mrkdwn = true };
                    var json = JsonConvert.SerializeObject(postObject);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    _url = eventLinks[Options.SendMessage];
                    await BotRequest(eventLinks[Options.SendMessage], HttpMethod.Post, postObject);
                }
                else
                {
                    var postObject = new { channel = "#" + channelName, text = message, mrkdwn = true };
                    await BotRequest(eventLinks[Options.SendMessage], HttpMethod.Post, postObject);
                }

            }
            catch
            {
                //TODO Admine Mail atılabilir
            }
        }

        private async Task CreateChannel(string channelName)
        {
            try
            {
                var postObject = new { name = channelName, validate = true };
                HttpResponseMessage result = await AppRequest(eventLinks[Options.CreateChannel], HttpMethod.Post, postObject);
                string res =await result.Content.ReadAsStringAsync();


            }
            catch
            {

                throw;
            }
        }

        private async Task GetChannelList()
        {
            HttpResponseMessage result = await AppRequest(eventLinks[Options.ChannelList], HttpMethod.Get);
            channelList = await result.Content.ReadAsStringAsync();
        }

        private async Task GetBotList()
        {
            _url = eventLinks[Options.UserList];
            HttpResponseMessage result = await _botClient.GetAsync(_url);
            userList = await result.Content.ReadAsStringAsync();
        }
        public JObject ContentParser(string content)
        {
            var dataObject = JObject.Parse(content);
            return dataObject;
        }

        public bool channelIsExist(string channel)
        {
            return channelList.Contains(channel);
        }

        private async Task<HttpResponseMessage> AppRequest(string url, HttpMethod method, object postObject = null)
        {

            try
            {
                if (method == HttpMethod.Post)
                {
                    var json = JsonConvert.SerializeObject(postObject);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage result = await _appClient.PostAsync(url, content);
                    string res =await result.Content.ReadAsStringAsync();
                    return result;
                }
                else
                {
                    return await _appClient.GetAsync(url);
                }
            }
            catch
            {
                return new HttpResponseMessage();
            }
        }

        private async Task<HttpResponseMessage> BotRequest(string url, HttpMethod method, object postObject = null)
        {
            try
            {

                if (method == HttpMethod.Post)
                {
                    var json = JsonConvert.SerializeObject(postObject);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage result = await _botClient.PostAsync(url, content);
                    string res = await result.Content.ReadAsStringAsync();
                    return result;
                }
                else
                {
                    return await _botClient.GetAsync(url);
                }
            }
            catch (Exception exp)
            {

                return new HttpResponseMessage();
            }

        }
    }
}

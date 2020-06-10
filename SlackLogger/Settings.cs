using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace SlackLogger
{
    public class SlackDto
    {
        public class Setting
        {
            public string BotName { get; set; }
            public string Prefix { get; set; }
            public string AppToken { get; set; }
            public string BotToken { get; set; }
            public string Title { get; set; }
            public string TitleUrl { get; set; }
        }

        public class Attachment
        {
            public string pretext { get; set; } = "";

            public string color { get; set; } = "danger";
            public string title { get; set; }
            public string title_link { get; set; }
            public List<Field> fields { get; set; }
            public long ts { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public class Field
        {
            public string title { get; set; }
            public string value { get; set; }
            [JsonProperty("short")]
            public bool Short { get; set; } = false;
        }

        public enum SlackType
        {
            Info = 1,
            Fatal = 2,
            Warning = 3,
            Error = 4,
            LogsDelete = 5,
            LogsEdit = 6
        }
    }
}

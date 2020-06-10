using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using static SlackLogger.SlackDto;

namespace SlackLogger
{
    public interface ISLogger
    {
        /// <summary>
        /// This method is sending message be Info by default
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        void Slack(object message, SlackType type = SlackType.Info);

        /// <summary>
        /// This method is sending message with creating new channel
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        void Slack(object message, string channelName);

        void Slack(Exception exp, string pretext = "");

        /// <summary>
        /// This method is sending message by attachment
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="type"></param>
        void Slack(string title, object message, SlackType type = SlackType.Info);
    }

    public class SLogger : ISLogger
    {
        SlackClient _slackClient;
        SlackDto.Setting _setting;
        public SLogger(SlackDto.Setting setting)
        {
            _setting = setting;
            _slackClient = new SlackClient(_setting);
        }

        public void Slack(object message, SlackType type = SlackType.Info)
        {
            try
            {
                string serializedMessage = JsonConvert.SerializeObject(message);
                _slackClient.SendMessage(serializedMessage, type);
            }
            catch
            {


            }
        }

        public void Slack(object message, string channelName)
        {
            try
            {
                string serializedMessage = JsonConvert.SerializeObject(message);
                _slackClient.SendMessage(serializedMessage, channelName);
            }
            catch
            {

            }
        }

        public void Slack(Exception exp, string pretext = "")
        {

            List<SlackDto.Attachment> attachmentList = new List<SlackDto.Attachment>();
            SlackDto.Attachment attachment = new SlackDto.Attachment();
            attachment.fields = new List<SlackDto.Field>();
            attachment.fields.Add(new SlackDto.Field { title = "Message", value = exp.Message != null ? exp.Message.ToString().Trim() : "" });
            attachment.fields.Add(new SlackDto.Field { title = "Method Name", value = new StackTrace(new StackFrame(2)).GetFrame(0).ToString().Trim() });
            attachment.fields.Add(new SlackDto.Field { title = "InnerException", value = exp.InnerException != null ? exp.InnerException.ToString().Trim() : "" });
            attachment.fields.Add(new SlackDto.Field { title = "Source", value = exp.Source != null ? exp.Source.ToString().Trim() : "" });
            attachment.fields.Add(new SlackDto.Field { title = "Data", value = (exp.Data != null ? exp.Data.ToString() : "").Trim() });
            attachment.fields.Add(new SlackDto.Field { title = "StackTrace", value = exp.StackTrace != null ? exp.StackTrace.ToString().Trim() : "" });
            attachment.fields.Add(new SlackDto.Field { title = "HResult", value = exp.HResult.ToString().Trim() });
            attachment.fields.Add(new SlackDto.Field { title = "TargetSite", value = exp.TargetSite != null ? exp.HResult.ToString().Trim() : "" });

            attachment.pretext = pretext;
            attachment.title = _setting.Title;
            attachment.title_link = _setting.TitleUrl;

            attachmentList.Add(attachment);
            string serializedContent = JsonConvert.SerializeObject(attachmentList.ToArray());
            _slackClient.SendMessage(serializedContent, SlackType.Error, true);
        }

        public void Slack(string title, object message, SlackType type = SlackType.Info)
        {
            List<SlackDto.Attachment> attachmentList = new List<SlackDto.Attachment>();
            SlackDto.Attachment attachment = new SlackDto.Attachment();
            attachment.fields = new List<SlackDto.Field>();
            attachment.fields.Add(new SlackDto.Field { title = _setting.Title, value = "" });


            if (message.GetType().IsGenericType && message.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)))
            {
                IList collection = (IList)message;

                for (int i = 0; i < collection.Count; i++)
                {
                    FillAttachment(collection[i], attachment);
                }

            }
            else
            {
                FillAttachment(message, attachment);
            }



            attachment.title = title;
            attachment.title_link = _setting.TitleUrl;
            attachmentList.Add(attachment);
            string serializedContent = JsonConvert.SerializeObject(attachmentList.ToArray());
            _slackClient.SendMessage(serializedContent, type, true);
        }

        private void FillAttachment(object item, SlackDto.Attachment attachment)
        {
            foreach (PropertyInfo pi in item.GetType().GetProperties())
            {
                string value = pi.GetValue(item, null)?.ToString();

                IEnumerable<object> enumerable = pi.GetValue(item, null) as IEnumerable<object>;

                if (enumerable != null)
                    value = string.Join(", ", enumerable);


                attachment.fields.Add(new SlackDto.Field { title = pi.Name, value = value });
            }
        }
    }
}

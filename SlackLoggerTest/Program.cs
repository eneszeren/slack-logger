using SlackLogger;
using System;

namespace SlackLoggerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            SLogger logger = new SLogger(new SlackDto.Setting()
            {
                BotToken = "yourbottoken",
                BotName = "LoggerApp",
                AppToken = "yourapplicationtoken",
                Prefix = "helper-test",
                Title = "helper-test",
                TitleUrl = "yourinformationsite.com"
            });
            logger.Slack("test", SlackDto.SlackType.Info);
        }
    }
}

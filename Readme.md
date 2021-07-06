# SlackLog

I developed a logger package for Nuget. SlackLog is using Slack for logging. This has simple usage. You can look at [SlackLog](https://www.nuget.org/packages/SlackLog/)


### Feature

  - When you want to send message to channel on Slack, SlackLog will open channel automatically.
  - SlackLog has 6 channel as default. They are info,fatal,warning,error,logsdelete and logsedit. Then, you can use any specific channel for your project.


### How to use?

First of all, you need to register to Slack and you should create Application and Bot Token on Slack. If you do not know how to use create tokens you should look [Create and regenerate API tokens](https://slack.com/intl/en-tr/help/articles/215770388-Create-and-regenerate-API-tokens)

And you should send setting to SLogger class and you can start use SlackLog. Otherway you can implent as middleware.

```
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
```

You can fork **[this repository](https://github.com/YusufSina/LoggingToSlack)** to develop Web API included SlackLog. Furthermore, There is a **[medium article](https://yusufsina.medium.com/net-core-slack-kanallar%C4%B1na-loglama-i%CC%87%C5%9Flemi-84d1b9f44458)** shows how to use SlackLog with more detail.

If you give me your feedback I will be happy.

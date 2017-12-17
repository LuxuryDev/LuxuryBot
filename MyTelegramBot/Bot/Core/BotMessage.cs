using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;
using Newtonsoft.Json;
using System.Web;
using Telegram.Bot.Types.InlineKeyboardButtons;



namespace MyTelegramBot.Bot
{
    public class BotMessage
    {
        public BotMessage()
        {
          
        }

        /// <summary>
        /// Текст сообщения
        /// </summary>
        public string TextMessage { get; set; }

        /// <summary>
        /// Клавиатула из Inline кнопок
        /// </summary>
        public IReplyMarkup MessageReplyMarkup { get; set; }

        /// <summary>
        /// текст для AnswerCallbackQueryAsync
        /// </summary>
        public string CallBackTitleText { get; set; }

        public string Url { get; set; }

        /// <summary>
        /// Кнопка назад. Для некоторых случаев
        /// </summary>
        protected InlineKeyboardCallbackButton BackBtn { get; set; }


        public MediaFile MediaFile { get; set; }

        public string BuildCallData (string CommandName,string ModuleName , params int [] Argument)
        {
            BotCommand command = new BotCommand
            {
                Cmd = CommandName,
                Arg = new List<int>(),
                M= ModuleName
            };

            for (int i = 0; i < Argument.Length; i++)
                command.Arg.Add(Argument[i]);

            return JsonConvert.SerializeObject(command);
        }

        public static string Bold(string value)
        {
            return "<b>" + value + "</b>";
        }

        public static string Italic(string value)
        {
            return "<i>" + value + "</i>";
        }

        public static string NewLine()
        {
            return "\r\n";
        }

        public static string HrefUrl(string url, string text)
        {
            const string quote = "\"";
            return "<a href=" + quote+ url + quote+ ">" + text + "</a>";
        }
    }

}

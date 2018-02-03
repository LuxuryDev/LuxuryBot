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
        /// <summary>
        /// ✔️
        /// </summary>
        protected string CheckEmodji = "\u2714\ufe0f";

        protected string UnCheckEmodji = "\ud83d\udd32";

        /// <summary>
        /// 🔹 - синий ромб
        /// </summary>
        protected string BlueRhombus = "\ud83d\udd39";

        /// <summary>
        /// 🔸 - золотой ромб
        /// </summary>
        protected string GoldRhobmus = "\ud83d\udd38";

        /// <summary>
        /// ⚠️ - Воскл. знак
        /// </summary>
        protected string WarningEmodji = "\u26a0\ufe0f";

        /// <summary>
        /// 🛒 - Корзина
        /// </summary>
        protected string BasketEmodji = "\ud83d\uded2";

        /// <summary>
        /// ⚙️ - Шестеренка
        /// </summary>
        protected string CogwheelEmodji = "\u2699\ufe0f";

        /// <summary>
        /// 🖊 - Ручка
        /// </summary>
        protected string PenEmodji = "\ud83d\udd8a";

        /// <summary>
        /// 🏠 - Домик
        /// </summary>
        protected string HouseEmodji = "\ud83c\udfe0";

        /// <summary>
        /// 🚚 - Машина
        /// </summary>
        protected string CarEmodji = "\ud83d\ude9a";

        /// <summary>
        /// 🙋🏻‍♂️ - Человек
        /// </summary>
        protected string ManEmodji = "\ud83d\ude4b\ud83c\udffb\u200d\u2642\ufe0f";

        /// <summary>
        /// ⭐️- Звезда
        /// </summary>
        protected string StartEmodji = "\u2b50\ufe0f";

        /// <summary>
        /// ➡️
        /// </summary>
        protected string NextEmodji = "\u27a1\ufe0f";

        /// <summary>
        /// ⬅️
        /// </summary>
        protected string PreviuosEmodji = "\u2b05\ufe0f";

        /// <summary>
        /// ◀️
        /// </summary>
        protected string Previuos2Emodji = "\u25c0\ufe0f";

        /// <summary>
        /// ▶️
        /// </summary>
        protected string Next2Emodji = "\u25b6\ufe0f";

        /// <summary>
        /// 💰 - мешочек с деньгами
        /// </summary>
        protected string CashEmodji = "\ud83d\udcb0";

        /// <summary>
        /// ⚖️ весы
        /// </summary>
        protected string WeigherEmodji = "\u2696\ufe0f";

        /// <summary>
        /// 🖼 - картина
        /// </summary>
        protected string PictureEmodji = "\ud83d\uddbc";

        /// <summary>
        /// 📝 - тетрадь с ручкой
        /// </summary>
        protected string NoteBookEmodji = "\ud83d\udcdd";

        /// <summary>
        /// 📉 - график
        /// </summary>
        protected string DepthEmodji = "\ud83d\udcc9";

        /// <summary>
        /// 📤 - отправить
        /// </summary>
        protected string SenderEmodji = "\ud83d\udce4";

        /// <summary>
        /// 📜 - лист
        /// </summary>
        protected string PaperEmodji = "\ud83d\udcdc";


        /// <summary>
        /// ❌ - красный крест
        /// </summary>
        protected string CrossEmodji = "\u274c";

        /// <summary>
        /// ✅ 
        /// </summary>
        protected string DoneEmodji = "\u2705";

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

        protected InlineKeyboardCallbackButton BuildInlineBtn(string Text, string CallData, string Emodji=null, bool TextFirst=true)
        {
            if(Emodji!=null && TextFirst)
                return new InlineKeyboardCallbackButton(Text + " " + Emodji, CallData);

            if (Emodji != null && !TextFirst)
                return new InlineKeyboardCallbackButton(Emodji+" " + Text, CallData);

            else
                return new InlineKeyboardCallbackButton(Text, CallData);

        }

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

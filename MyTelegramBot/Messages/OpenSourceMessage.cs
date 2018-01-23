using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace MyTelegramBot.Messages
{
    public class OpenSourceMessage:Bot.BotMessage
    {

        public Bot.BotMessage BuildMessage()
        {

            this.TextMessage = "Это проект с открытым исходым. Вы можете скачать";

            BackBtn = new Telegram.Bot.Types.InlineKeyboardButtons.InlineKeyboardCallbackButton("Назад", "MainMenu");

            base.MessageReplyMarkup = new InlineKeyboardMarkup(
                new[]{
                new[]
                        {
                            BackBtn
                        },
                });

            return this;


        }
    }
}

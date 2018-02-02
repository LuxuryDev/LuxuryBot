using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.EntityFrameworkCore;
using MyTelegramBot.Bot;

namespace MyTelegramBot.Messages
{
    /// <summary>
    /// Оценка от 1 до 5 к отзыву
    /// </summary>
    public class RaitingMessage:BotMessage
    {

        private InlineKeyboardCallbackButton OneBtn { get; set; }

        private InlineKeyboardCallbackButton TwoBtn { get; set; }

        private InlineKeyboardCallbackButton ThreeBtn { get; set; }

        private InlineKeyboardCallbackButton FourBtn { get; set; }

        private InlineKeyboardCallbackButton FiveBtn { get; set; }

        private int FeedBackId { get; set; }

        MarketBotDbContext db;

        FeedBack FeedBack { get; set; }

        public RaitingMessage(int FeedBackId)
        {
            this.FeedBackId = FeedBackId;
        }

        public RaitingMessage(FeedBack FeedBack)
        {
            this.FeedBack = FeedBack;

        }
        public RaitingMessage BuildMessage()
        {
            OneBtn = new InlineKeyboardCallbackButton("1", BuildCallData(OrderBot.SelectRaitingCmd, OrderBot.ModuleName, FeedBackId,1));

            TwoBtn = new InlineKeyboardCallbackButton("2", BuildCallData(OrderBot.SelectRaitingCmd, OrderBot.ModuleName, FeedBackId,2));

            ThreeBtn = new InlineKeyboardCallbackButton("3", BuildCallData(OrderBot.SelectRaitingCmd, OrderBot.ModuleName, FeedBackId,3));

            FourBtn = new InlineKeyboardCallbackButton("4", BuildCallData(OrderBot.SelectRaitingCmd, OrderBot.ModuleName, FeedBackId,4));

            FiveBtn = new InlineKeyboardCallbackButton("5", BuildCallData(OrderBot.SelectRaitingCmd, OrderBot.ModuleName, FeedBackId,5));

            db = new MarketBotDbContext();

            if (FeedBack == null && FeedBackId > 0)
                FeedBack = db.FeedBack.Find(FeedBackId);

            db.Dispose();

            if (FeedBack != null)
            {
                base.TextMessage = FeedBack.Text+NewLine()+Italic("Введите оценку от 1 до 5");
                SetKeyBoard();
                
                return this;
            }

            else
            {
                return null;
            }
        }

        public void SetKeyBoard()
        {
            base.MessageReplyMarkup = new InlineKeyboardMarkup(
                        new[]
                        {
                        new[]
                        {
                            OneBtn,TwoBtn,ThreeBtn,FourBtn,FiveBtn
                        }
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.EntityFrameworkCore;

namespace MyTelegramBot.Messages
{
    /// <summary>
    /// Предложение оставить отзыв к заказу
    /// </summary>
    public class FeedBackOfferMessage:Bot.BotMessage
    {
        private int OrderId { get; set; }

        private InlineKeyboardCallbackButton AddFeedBackBtn { get; set; }

        private Orders Order { get; set; }

        public FeedBackOfferMessage(int OrderId)
        {
            this.OrderId = OrderId;
        }

        public FeedBackOfferMessage (Orders orders)
        {
            this.Order = orders;
        }

        public FeedBackOfferMessage BuildMessage()
        {
            if(this.Order==null)
            using(MarketBotDbContext db=new MarketBotDbContext())
                Order = db.Orders.Where(o => o.Id == OrderId).FirstOrDefault();
            

            if (Order != null)
            {
                base.TextMessage = "Заказ №" + Order.Number.ToString() + " выполнен. Вы можете оставить отзыв к заказу.";
                this.AddFeedBackBtn = BuildInlineBtn("Добавить отзыв", BuildCallData(Bot.OrderBot.CmdAddFeedBack, Bot.OrderBot.ModuleName, Order.Id),NoteBookEmodji);

                base.MessageReplyMarkup = new InlineKeyboardMarkup(
                    new[]
                    {
                        new[]
                        {
                            AddFeedBackBtn
                        }
                    });
            }

            return this;

        }
    }
}

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

        private InlineKeyboardCallbackButton [][] ProductsBtn { get; set; }

        MarketBotDbContext db;

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
            db = new MarketBotDbContext();

            if(this.Order==null)
                Order = db.Orders.Where(o => o.Id == OrderId).Include(o=>o.OrderProduct).FirstOrDefault();
            



            return this;

        }
    }
}

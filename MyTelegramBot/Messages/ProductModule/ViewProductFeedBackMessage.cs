using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.InlineQueryResults;
using MyTelegramBot.Bot;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using System.IO;

namespace MyTelegramBot.Messages
{
    public class ViewProductFeedBackMessage:Bot.BotMessage
    {

        private InlineKeyboardCallbackButton NextFeedBackBtn { get; set; }

        private InlineKeyboardCallbackButton PreviusFeedBackBtn { get; set; }

        Product Product { get; set; }

        FeedBack FeedBack { get; set; }

        private int ProductId { get; set; }

        private int FeedBackId { get; set; }

        private MarketBotDbContext db { get; set; }

        public ViewProductFeedBackMessage (int ProductId, int FeedBackId = 0)
        {
            this.ProductId = ProductId;
            this.FeedBackId = FeedBackId;
        }


        public ViewProductFeedBackMessage BuildMessage()
        {
            db = new MarketBotDbContext();

            //list<int> styleID = new List<int>(); 
            //int index = styleID.FindIndex(x => x == 998877);

            var list = db.FeedBack.Where(f => f.ProductId == ProductId).ToList();

            if (FeedBackId == 0)
                FeedBack = list.OrderBy(f => f.Id).FirstOrDefault();

            if(FeedBackId>0)
                FeedBack= list.OrderBy(f => f.Id).FirstOrDefault();

            if (FeedBack != null)
            {
                //порядковый номер отзыва 
                int index = list.FindIndex(x => x == FeedBack) + 1;

                //общее кол-во отзывов по товару
                int count = list.Count;

                base.TextMessage = BlueRhombus + " Отзыв к товару : " + Product.Name + " ( " + index.ToString() + " из " + count.ToString() + " )" + NewLine() +
                    Bold("Время:") + FeedBack.DateAdd.ToString() + NewLine() +
                    Bold("Комментарий:") + FeedBack.Text + NewLine() +
                    Bold("Оценка:") + ConcatEmodjiStar(Convert.ToInt32(FeedBack.RaitingValue));

                return this;
            }

            else
                return null;

        }

        private string ConcatEmodjiStar (int count)
        {
            string res = "";
            for (int i = 0; i < count; i++)
                res += base.StartEmodji;

            return res;
        }
    }
}

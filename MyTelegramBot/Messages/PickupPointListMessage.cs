using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.EntityFrameworkCore;

namespace MyTelegramBot.Messages
{
    public class PickupPointListMessage:Bot.BotMessage
    {
        List<PickupPoint> PickupPoitList { get; set; }

        private InlineKeyboardCallbackButton[][] PickupPointListBtn { get; set; }

        public PickupPointListMessage()
        {
            BackBtn = new Telegram.Bot.Types.InlineKeyboardButtons.InlineKeyboardCallbackButton("Назад", "Назад к выбору способа получения заказа");
        }

        public PickupPointListMessage BuildMessage()
        {
            using (MarketBotDbContext db = new MarketBotDbContext())
                PickupPoitList = db.PickupPoint.Where(p=>p.Enable==true).ToList();


            if(PickupPoitList!=null && PickupPoitList.Count > 0)
            {
                PickupPointListBtn= new InlineKeyboardCallbackButton[PickupPoitList.Count() + 1][];
                int counter = 0;
                foreach (PickupPoint point in PickupPoitList)
                {
                    PickupPointListBtn[counter] = new InlineKeyboardCallbackButton[1];
                    PickupPointListBtn[counter][0] = new InlineKeyboardCallbackButton(point.Name, BuildCallData("SelectPoint", Bot.OrderBot.ModuleName, point.Id));
                    counter++;
                }

                PickupPointListBtn[counter + 1] = new InlineKeyboardCallbackButton[1];
                PickupPointListBtn[counter + 1][0] = BackBtn;

                base.TextMessage = "Выберите пункт самовывоза";

            }

            else
            {
                PickupPointListBtn = new InlineKeyboardCallbackButton[1][];
                PickupPointListBtn[0] = new InlineKeyboardCallbackButton[1];
                PickupPointListBtn[0][0] = BackBtn;
                base.TextMessage = "Нет доступных пунктов самовывоза. Вернитесь назад и выберите другой способ получения заказа";

            }

            return this;
        }
    }
}

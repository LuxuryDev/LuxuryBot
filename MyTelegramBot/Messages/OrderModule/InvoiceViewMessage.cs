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

namespace MyTelegramBot.Messages
{
    public class InvoiceViewMessage:Bot.BotMessage
    {
        private Invoice Invoice { get; set; }

        private List<Payment> PaymentList { get; set; }

        private int OrderId { get; set; }

        private MarketBotDbContext db { get; set; }

        public InvoiceViewMessage (Invoice invoice,int OrderId,string BackCmdName= "BackToOrder")
        {
            this.Invoice = invoice;
            this.OrderId = OrderId;
            BackBtn = new InlineKeyboardCallbackButton("Вернуться к заказу", BuildCallData(BackCmdName, OrderId));
        }

        public InvoiceViewMessage(Invoice invoice, List<Payment> list, string BackCmdName = "BackToOrder")
        {
            this.Invoice = invoice;
            this.PaymentList = list;

            if(PaymentList!=null && PaymentList.Count>0)
                BackBtn = new InlineKeyboardCallbackButton("Вернуться к заказу", BuildCallData(BackCmdName, Convert.ToInt32(PaymentList.FirstOrDefault().OrderId)));
        }

        public InvoiceViewMessage BuildMessage()
        {
            db = new MarketBotDbContext();

            if (PaymentList == null || PaymentList.Count == 0)
                    PaymentList = db.Payment.Where(p => p.OrderId == OrderId).ToList();

            if (Invoice != null)
            {
                if (Invoice.PaymentType == null)
                    Invoice.PaymentType = db.PaymentType.Where(p => p.Id == Invoice.PaymentTypeId).FirstOrDefault();

                base.TextMessage = Bold("Счет на оплату №") + Invoice.InvoiceNumber.ToString() + NewLine() +
                                 Bold("Адрес счета получателя:") + Invoice.AccountNumber + NewLine() +
                                 Bold("Комментарий к платежу:") + Invoice.Comment + NewLine() +
                                 Bold("Сумма: ") + Invoice.Value.ToString() + " " + Invoice.PaymentType.Code + NewLine() +
                                 Bold("Время создания: ") + Invoice.CreateTimestamp.ToString() + NewLine() +
                                 Bold("Способо оплаты: ") + Invoice.PaymentType.Name + NewLine() + NewLine() +
                                 "Вы должны оплатить этот счет не позднее " + Invoice.CreateTimestamp.Value.Add(Invoice.LifeTimeDuration.Value).ToString();


                if (Invoice.PaymentType != null && PaymentType.GetPaymentTypeEnum(Invoice.PaymentType.Id) != Services.PaymentTypeEnum.PaymentOnReceipt &&
                    PaymentType.GetPaymentTypeEnum(Invoice.PaymentType.Id) != Services.PaymentTypeEnum.Qiwi)
                    base.TextMessage += NewLine() + NewLine() +
                        HrefUrl("https://live.blockcypher.com/" + Invoice.PaymentType.Code.ToLower() + "/address/" + Invoice.AccountNumber, "Посмотреть платеж");


                base.MessageReplyMarkup = new InlineKeyboardMarkup(
                    new[]{
                    new[]
                        {
                            BackBtn,

                        },
                    });
             
            }

                return this;
        }


    }
}

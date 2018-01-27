using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.InlineKeyboardButtons;
using System.Security.Cryptography;
using System.Text;

namespace MyTelegramBot.Messages
{
    /// <summary>
    /// Сообщение с описание заказа
    /// </summary>
    public class OrderViewMessage: Bot.BotMessage
    {
        private int OrderId { get; set; }

        private Orders Order { get; set; }

        private InlineKeyboardCallbackButton ChekPayBtn { get; set; }

        private InlineKeyboardCallbackButton AddFeedbackBtn { get; set; }

        private InlineKeyboardCallbackButton RemoveOrderBtn { get; set; }

        private InlineKeyboardCallbackButton ViewInvoiceBtn { get; set; }

        private const int QiwiPayMethodId = 2;

        private const int PaymentOnReceiptId = 1;

        private Address Address { get; set; }

        public OrderViewMessage (int OrderId)
        {
            this.OrderId = OrderId;
        }

        public OrderViewMessage (Orders order)
        {
            this.Order = order;
        }

        public OrderViewMessage BuildMessage()
        {
            using (MarketBotDbContext db = new MarketBotDbContext())
            {
                if (this.OrderId > 0) // если в конструктор был передан айди заявки
                    Order = db.Orders.Where(o => o.Id == OrderId).
                        Include(o => o.Confirm).
                        Include(o => o.Delete).
                        Include(o => o.Done).
                        Include(o => o.FeedBack).
                        Include(o => o.OrderProduct).
                        Include(o=>o.PickupPoint).
                        Include(o => o.OrderAddress).Include(o=>o.BotInfo)
                        .Include(o=>o.Invoice)
                        .Include(o=>o.OrderProduct).FirstOrDefault();

                if(Order!=null && Order.OrderProduct.Count==0)
                    Order.OrderProduct = db.OrderProduct.Where(op => op.OrderId == Order.Id).ToList();


                if (Order != null && Order.PickupPoint == null && Order.PickupPointId > 0)
                    Order.PickupPoint = db.PickupPoint.Find(Order.PickupPointId);

                if (Order != null && Order.OrderAddress==null)
                    Order.OrderAddress = db.OrderAddress.Where(o => o.OrderId == Order.Id).FirstOrDefault();

                if (Order != null)
                {
                    string Position = "";

                    string done = "";

                    string feedback = "-";

                    string paid = "";

                    double total = 0.0; // общая строисоить заказа

                    if(Order.OrderAddress!=null) // способо получения - Доставка
                        Address = db.Address.Where(a => a.Id == Order.OrderAddress.AdressId).
                        Include(a => a.House).
                        Include(a => a.House.Street).
                        Include(a => a.House.Street.City).
                        FirstOrDefault();


                    try
                    {
                        int counter = 0; // счетчки цикла
                    

                        foreach (OrderProduct p in Order.OrderProduct) // Состав заказа
                        {
                            counter++;
                            p.Product = db.Product.Where(x => x.Id == p.ProductId).Include(x => x.ProductPrice).FirstOrDefault();
                            p.Price = db.ProductPrice.Where(price => price.Id == p.PriceId).Include(price=> price.Currency).FirstOrDefault();
                            Position += counter.ToString() + ") " + p.ToString() + NewLine();
                            total += p.Price.Value * p.Count;
                        }

                        if (Order.BotInfo == null)
                            Order.BotInfo = db.BotInfo.Where(o => o.Id == Order.BotInfoId).FirstOrDefault();

                        //Прибавляем стоимость доставки к стоимости заказа
                        if (Order.OrderAddress != null)
                            total += Order.OrderAddress.ShipPriceValue;

                        if (Order.Done != null ) //Заказ выполнен
                        done = "Да";

                        else // ЗАказ не выполен
                            done = "Нет";

                        if (Order.Done != null  && Order.FeedBack != null && Order.FeedBack.Text != null) // Есть отзыв к заказ
                            feedback = Order.FeedBack.Text + " | " + Order.FeedBack.DateAdd.ToString();
                        
                        if (Order.Paid == true) // Заказ оплачен
                            paid = "Да";

                        if (Order.Done != null && Order.FeedBack == null) // Отзыва нет, Добавляем кнопку
                            feedback = "Нет";

                        if(Order.OrderAddress!=null)
                            base.TextMessage = Bold("Номер заказа: ") + Order.Number.ToString() + NewLine()
                                    + Position + NewLine()
                                    +Bold("Стоимость доставки:") + Order.OrderAddress.ShipPriceValue.ToString() + NewLine()
                                    + Bold("Общая стоимость: ") + total.ToString() + Order.OrderProduct.FirstOrDefault().Price.Currency.ShortName + NewLine()
                                    + Bold("Комментарий: ") + Order.Text + NewLine()
                                    + Bold("Способо получения закза: ") + " Доставка" + NewLine()
                                    + Bold("Адрес доставки: ") + Address.House.Street.City.Name + ", " + Address.House.Street.Name + ", " + Address.House.Number + NewLine()
                                    + Bold("Время: ") + Order.DateAdd.ToString() + NewLine()
                                    + Bold("Оплачено: ") + paid
                                    + NewLine() + Bold("Выполнено: ") + done
                                    + NewLine() + Bold("Оформлен через:") + "@" + Order.BotInfo.Name
                                    + NewLine() + Bold("Отзыв: ") + feedback;

                        if (Order.PickupPoint != null)
                            base.TextMessage = Bold("Номер заказа: ") + Order.Number.ToString() + NewLine()
                                    + Position + NewLine()
                                    + Bold("Общая стоимость: ") + total.ToString() + Order.OrderProduct.FirstOrDefault().Price.Currency.ShortName + NewLine()
                                    + Bold("Комментарий: ") + Order.Text + NewLine()
                                    + Bold("Способо получения закза: ")+" Самовывоз" + NewLine()
                                    + Bold("Пункт самовывоза: ") + Order.PickupPoint.Name + NewLine()
                                    + Bold("Время: ") + Order.DateAdd.ToString() + NewLine()
                                    + Bold("Оплачено: ") + paid
                                    + NewLine() + Bold("Выполнено: ") + done
                                    + NewLine() + Bold("Оформлен через:") + "@" + Order.BotInfo.Name
                                    + NewLine() + Bold("Отзыв: ") + feedback;

                    }
                    catch (Exception exp)
                    {

                    }

                    SetButton();
                    base.CallBackTitleText = "Номер заказа:" + Order.Number.ToString();
                }


            }

            return this;
        }

        private void SetButton()
        {

            if (Order.FeedBack==null && Order.Done != null) // Отзыва нет, заказ выполнен
                base.MessageReplyMarkup = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(
                    new[]{
                                new[]
                                    {
                                            AddFeedBack()
                                    },
                                new[]
                                    {
                                            ViewInvoice()
                                    },
                    });


            if (Order.FeedBack != null  && Order.Paid == true 
                || Order.Paid==false || Order.Paid==true) // Отзыва есть, заказ оплачен или заказ не оплачен
                base.MessageReplyMarkup = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(
                    new[]{
                                new[]
                                    {
                                            ViewInvoice()
                                    },

                    });


            if (Order.FeedBack == null && Order.Done != null  && Order.InvoiceId==null) // Отзыва нет, заказ выполнен (Тип оплаты - при получении)
                base.MessageReplyMarkup = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(
                    new[]{
                                new[]
                                    {
                                            AddFeedBack()
                                    },
                    });

            if (Order.InvoiceId == null && Order.Done==null) // Метод оплаты при получении, заказ не выполнен
                base.MessageReplyMarkup = null;


        }

        private InlineKeyboardCallbackButton AddFeedBack()
        {
            return new InlineKeyboardCallbackButton("Добавить отзыв", BuildCallData(Bot.OrderBot.CmdAddFeedBack, Bot.OrderBot.ModuleName, Order.Id)); 
        }


        private InlineKeyboardCallbackButton ViewInvoice()
        {
            InlineKeyboardCallbackButton button = new InlineKeyboardCallbackButton("Посмотреть счет на оплату", BuildCallData("ViewInvoice", Bot.OrderBot.ModuleName, Convert.ToInt32(Order.Id)));
            return button;
        }

    }
}

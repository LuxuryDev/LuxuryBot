﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.EntityFrameworkCore;
using MyTelegramBot.Messages.Admin;
using MyTelegramBot.Messages;
using MyTelegramBot.Bot.AdminModule;

namespace MyTelegramBot.Messages.Admin
{
    /// <summary>
    /// Сообщение с описанием заказа. Админка
    /// </summary>
    public class AdminOrderMessage:Bot.BotMessage
    {
        private InlineKeyboardCallbackButton EditOrderPositionBtn { get; set; }

        private InlineKeyboardCallbackButton ViewTelephoneNumberBtn { get; set; }

        /// <summary>
        /// Кнопка взять заказ в обработку
        /// </summary>
        private InlineKeyboardCallbackButton TakeOrderBtn { get; set; }

        private InlineKeyboardCallbackButton ViewAddressOnMapBtn { get; set; }

        private InlineKeyboardCallbackButton DoneBtn { get; set; }

        private InlineKeyboardCallbackButton DeleteBtn { get; set; }

        private InlineKeyboardCallbackButton RecoveryBtn { get; set; }

        private InlineKeyboardCallbackButton ConfirmBtn { get; set; }

        private InlineKeyboardCallbackButton ViewPaymentBtn { get; set; }

        /// <summary>
        /// Освободить заявку
        /// </summary>
        private InlineKeyboardCallbackButton FreeOrderBtn { get; set; }
      
        private List<Payment> Payments { get; set; }

        private Orders Order { get; set; }

        private int OrderId { get; set; }

        private Follower Follower { get; set; }

        private string PaymentMethodName { get; set; }

        /// <summary>
        /// Какому пользователю будет отсылать сообщение
        /// </summary>
        private int FollowerId { get; set; }

        /// <summary>
        /// У какого пользователя заявка сейчас находится в обработке
        /// </summary>
        private int InWorkFollowerId { get; set; }

        public AdminOrderMessage(int OrderId, int FollowerId = 0)
        {
            this.OrderId = OrderId;
            this.FollowerId = FollowerId;
        }

        public AdminOrderMessage (Orders order, int FollowerId=0)
        {
            this.Order = order;
            this.FollowerId = FollowerId;
        }

        public AdminOrderMessage BuildMessage()
        {
            using (MarketBotDbContext db = new MarketBotDbContext())
            {
                if(this.Order==null && this.OrderId>0)
                Order = db.Orders.Where(o => o.Id == OrderId).
                    Include(o=>o.FeedBack).
                    Include(o => o.OrderProduct).
                    Include(o => o.OrderAddress).
                    Include(o=>o.OrdersInWork).
                    Include(o=>o.Invoice).
                    Include(o=>o.Confirm).
                    Include(o => o.Delete).
                    Include(o => o.DoneNavigation).
                    FirstOrDefault();

                ///////////Провереряем какой метод оплаты и наличие платежей////////////
                if (Order.Invoice != null)
                    PaymentMethodName = "Метод оплаты";
                

                if (Order != null && Order.OrderAddress == null)
                    Order.OrderAddress = db.OrderAddress.Where(o => o.OrderId == Order.Id).FirstOrDefault();

                var Address = db.Address.Where(a => a.Id == Order.OrderAddress.AdressId).Include(a => a.House).Include(a => a.House.Street).Include(a => a.House.Street.City).FirstOrDefault();

                double total = 0.0;
                string Position = "";
                int counter = 0;
                string Paid = "";

                if (Order.BotInfo == null)
                    Order.BotInfo = db.BotInfo.Where(b => b.Id == Order.BotInfoId).FirstOrDefault();

                if (Order.Paid == true)
                    Paid = "Оплачено";

                else
                    Paid = "Не оплачено";


                if (Order.OrderProduct == null || Order.OrderProduct != null && Order.OrderProduct.Count == 0)
                    Order.OrderProduct = db.OrderProduct.Where(o => o.OrderId == Order.Id).ToList();

                foreach (OrderProduct p in Order.OrderProduct) // Вытаскиваем все товары из заказа
                {
                    counter++;
                    p.Product = db.Product.Where(x => x.Id == p.ProductId).Include(x => x.ProductPrice).FirstOrDefault();
                    if(p.Price==null)
                        p.Price = p.Product.ProductPrice.FirstOrDefault();

                    Position += counter.ToString() + ") " + p.AdminText() + NewLine();
                    total += p.Price.Value * p.Count;
                }

                /////////Формируем основную часть сообщения
                base.TextMessage = Bold("Номер заказа: ") + Order.Number.ToString() + NewLine()
                            + Position + NewLine()
                            + Bold("Общая стоимость: ") + total.ToString() + NewLine()
                            + Bold("Комментарий: ") + Order.Text + NewLine()
                            + Bold("Адрес доставки: ") + Address.House.Street.City.Name + ", " + Address.House.Street.Name + ", " + Address.House.Number + NewLine()
                            + Bold("Время: ") + Order.DateAdd.ToString() +NewLine()
                            + Bold("Способ оплаты: ") + PaymentMethodName + NewLine() 
                            +Bold("Оформлено через: ")+"@" + Order.BotInfo.Name +NewLine()
                            + Bold("Статус платежа: ") + Paid;

                //Детали согласования заказа
                if (Order != null && Order.Confirm != null && Order.Delete==null)
                    base.TextMessage += NewLine() + NewLine() + Bold("Заказ согласован:") + NewLine() + Italic("Комментарий: " + Order.Confirm.Text
                        + " |Время: " + Order.Confirm.Timestamp.ToString() 
                        + " |Пользователь: " + Bot.GeneralFunction.FollowerFullName(Order.Confirm.FollowerId));

                ///Детали удаления заказа
                if (Order != null && Order.Delete != null)
                    base.TextMessage += NewLine() + NewLine() + Bold("Заказ удален:") + NewLine() + Italic("Комментарий: " + Order.Delete.Text
                        + " |Время: " + Order.Delete.Timestamp.ToString()
                        + " |Пользователь: " + Bot.GeneralFunction.FollowerFullName(Order.Delete.FollowerId));

                ///Детали выполнения заказа
                if (Order != null && Order.DoneNavigation != null)
                    base.TextMessage += NewLine() + NewLine() + Bold("Заказ выполнен:") + Italic(Order.DoneNavigation.Timestamp.ToString())
                        + " |Пользователь: " + Bot.GeneralFunction.FollowerFullName(Order.DoneNavigation.FollowerId);

                //Детали Отзыва к заказу
                if (Order != null && Order.FeedBack != null && Order.FeedBack.Text != null && Order.FeedBack.Text != "")
                    base.TextMessage += NewLine() + NewLine() + Bold("Отзыв к заказу:") + NewLine() + Italic(Order.FeedBack.Text + " | Время: " + Order.FeedBack.DateAdd.ToString());

                InWorkFollowerId = WhoInWork(Order);

                CreateBtns();

                SetInlineKeyBoard();

                return this;
            }
        }

        private int WhoInWork(Orders order)
        {
            if (order != null && order.OrdersInWork.Count > 0)
            {
                var in_work = order.OrdersInWork.OrderByDescending(o => o.Id).FirstOrDefault();

                if (in_work != null && in_work.InWork == true)
                    return Convert.ToInt32(in_work.FollowerId);

                else
                    return 0;
            }

            else
                return 0;
        }

        private void CreateBtns()
        {
            EditOrderPositionBtn = new InlineKeyboardCallbackButton("Изменить содержание заказа"+ " \ud83d\udd8a", BuildCallData(OrderProccesingBot.CmdEditOrderPosition, OrderProccesingBot.ModuleName, Order.Id));

            ViewTelephoneNumberBtn = new InlineKeyboardCallbackButton("Контактные данные"+ " \ud83d\udcde", BuildCallData(OrderProccesingBot.CmdGetTelephone, OrderProccesingBot.ModuleName, Order.Id));

            ViewAddressOnMapBtn = new InlineKeyboardCallbackButton("Показать на карте"+ " \ud83c\udfd8", BuildCallData(OrderProccesingBot.CmdViewAddressOnMap, OrderProccesingBot.ModuleName, Order.Id));

            DoneBtn = new InlineKeyboardCallbackButton("Выполнено"+" \ud83d\udc4c\ud83c\udffb", BuildCallData(OrderProccesingBot.CmdDoneOrder, OrderProccesingBot.ModuleName, Order.Id));

            DeleteBtn = new InlineKeyboardCallbackButton("Удалить"+ " \u2702\ufe0f", BuildCallData(OrderProccesingBot.CmdOrderDelete, OrderProccesingBot.ModuleName, Order.Id));

            RecoveryBtn = new InlineKeyboardCallbackButton("Восстановить", BuildCallData(OrderProccesingBot.CmdRecoveryOrder, OrderProccesingBot.ModuleName, Order.Id));

            ConfirmBtn = new InlineKeyboardCallbackButton("Согласован"+ " \ud83e\udd1d", BuildCallData(OrderProccesingBot.CmdConfirmOrder, OrderProccesingBot.ModuleName, Order.Id));

            ViewPaymentBtn = new InlineKeyboardCallbackButton("Посмотреть счет" + " \ud83d\udcb5", BuildCallData("ViewInvoice", OrderProccesingBot.ModuleName, Order.Id));

            TakeOrderBtn = new InlineKeyboardCallbackButton("Взять в работу", BuildCallData("TakeOrder", OrderProccesingBot.ModuleName, Order.Id));

            FreeOrderBtn = new InlineKeyboardCallbackButton("Освободить", BuildCallData("FreeOrder", OrderProccesingBot.ModuleName, Order.Id));
        }

        private void SetInlineKeyBoard()
        {
            //Заявка еще ни кем не взята в обрабоку или Неизвстно кому мы отрпавляем это сообщение т.е переменная FollowerId=0
            if (InWorkFollowerId == 0  || FollowerId==0 )
                base.MessageReplyMarkup = new InlineKeyboardMarkup(
                new[]{
                    new[]
                    {
                        TakeOrderBtn
                    }
                });

            ///Заявка взята в обработку пользователем. Рисуем основные кнопки
            if (Order.Delete==null&& Order.Confirm==null && FollowerId==InWorkFollowerId && InWorkFollowerId!=0)
                base.MessageReplyMarkup = new InlineKeyboardMarkup(
                new[]{
                new[]
                        {
                            ViewPaymentBtn, FreeOrderBtn
                        },
                new[]
                        {
                            EditOrderPositionBtn
                        },
                new[]
                        {
                            ConfirmBtn,DeleteBtn
                        },
                new[]
                        {
                            ViewTelephoneNumberBtn,
                            ViewAddressOnMapBtn
                        },

                 });

            ///Заявка взять в обработку пользователем. Но заказ удален.
            if (Order.Delete!=null&& FollowerId == InWorkFollowerId && InWorkFollowerId != 0)
                base.MessageReplyMarkup = new InlineKeyboardMarkup(
                new[]{
                new[]
                        {
                            ViewPaymentBtn,FreeOrderBtn
                        },
                new[]
                        {
                            EditOrderPositionBtn
                        },
                new[]
                        {
                            RecoveryBtn
                        },
                new[]
                        {
                            ViewTelephoneNumberBtn,
                            ViewAddressOnMapBtn
                        },

                });
            ///Заявка взять в обработку пользователем. Зазакз уже согласован
            if (Order.Confirm!=null && Order.Delete==null&& FollowerId == InWorkFollowerId && InWorkFollowerId != 0)
                    base.MessageReplyMarkup = new InlineKeyboardMarkup(
                    new[]{
                 new[]
                        {
                            ViewPaymentBtn,FreeOrderBtn
                        },
                new[]
                        {
                            EditOrderPositionBtn
                        },
                new[]
                        {
                            DoneBtn,DeleteBtn
                        },
                new[]
                        {
                            ViewTelephoneNumberBtn,
                            ViewAddressOnMapBtn
                        },

                    });

            ///Заявка взять в обработку пользователем или может быть просто открыта любым т.к она уже выполнена
            if (Order.DoneNavigation!=null)
                base.MessageReplyMarkup = new InlineKeyboardMarkup(
                new[]{
                new[]
                        {
                            ViewPaymentBtn
                        },
                new[]
                        {
                            ViewTelephoneNumberBtn,
                            ViewAddressOnMapBtn
                        },

                });
        }

    }
}

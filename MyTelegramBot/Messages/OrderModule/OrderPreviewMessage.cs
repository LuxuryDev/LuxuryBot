﻿using System;
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
    /// Сообщение с заказом, из таблицы OrderTemp
    /// </summary>
    public class OrderTempMessage:Bot.BotMessage
    {
        InlineKeyboardCallbackButton SendBtn { get; set; }

        InlineKeyboardCallbackButton DescEditorBtn { get; set; }

        InlineKeyboardCallbackButton MethodOfObtainingEditor { get; set; }

        InlineKeyboardCallbackButton PaymentMethodEditor { get; set; }


        private int FollowerId { get; set; }

        private OrderTemp OrderTemp { get; set; }

        private int BotId { get; set; }

        private Address Address { get; set; }

        private PickupPoint PickupPoint { get; set; }

        private Configuration Configuration { get; set; }

        public OrderTempMessage(int FollowerId, int BotId)
        {
            this.FollowerId = FollowerId;
            this.BotId = BotId;
        }

        public OrderTempMessage BuildMessage()
        {
            SendBtn = new InlineKeyboardCallbackButton("Сохранить" + " \ud83d\udcbe", BuildCallData(Bot.OrderBot.CmdOrderSave, OrderBot.ModuleName));
            DescEditorBtn = new InlineKeyboardCallbackButton("Комментарий к заказу" + " \ud83d\udccb", BuildCallData(Bot.OrderBot.CmdOrderDesc, OrderBot.ModuleName));
            MethodOfObtainingEditor = new InlineKeyboardCallbackButton("Изменить способ получения заказа" + " \ud83d\udd8a", BuildCallData(Bot.OrderBot.MethodOfObtainingListCmd, OrderBot.ModuleName));
            PaymentMethodEditor = new InlineKeyboardCallbackButton("Изменить способ оплаты" + " \ud83d\udd8a", BuildCallData(Bot.OrderBot.GetPaymentMethodListCmd, OrderBot.ModuleName));


            using (MarketBotDbContext db = new MarketBotDbContext())
            {
                OrderTemp = db.OrderTemp.Where(o => o.FollowerId == FollowerId && o.BotInfoId==BotId).Include(o=>o.PaymentType).FirstOrDefault();
                string PositionInfo = BasketPositionInfo.GetPositionInfo(FollowerId,BotId);
                Configuration = db.Configuration.Where(c => c.BotInfoId == BotId).FirstOrDefault();
                double BasketTotalPrice = BasketPositionInfo.BasketTotalPrice(FollowerId, BotId);

                if (OrderTemp != null)  
                {
                    double ShipPice = 0;

                    if(OrderTemp.AddressId != null) // если  способ получения заказа "Доставка" то определям адрес доставки и стоимость доставки
                    {
                        Address = db.Address.Where(a => a.Id == OrderTemp.AddressId).Include(a => a.House).Include(a => a.House.Street).Include(a => a.House.Street.City).FirstOrDefault();

                        //определям стоимост доставки
                        //Стоимость заказа подходит под условия бесплатной доставки
                        if(Configuration.ShipPrice>0 && BasketTotalPrice >= Configuration.FreeShipPrice)
                            ShipPice = 0;

                        //Стоимость заказа НЕ подходит под условия бесплатной доставки
                        if (Configuration.ShipPrice > 0 && BasketTotalPrice < Configuration.FreeShipPrice)
                            ShipPice = Configuration.ShipPrice;

                        //Доставка бесплатая
                        if (Configuration.ShipPrice == 0)
                            ShipPice = 0;

                    }

                    if (OrderTemp.PickupPointId != null)
                        PickupPoint = db.PickupPoint.Find(OrderTemp.PickupPointId);

                    string Desc = "-";

                    string PaymentMethod = "-";

                    if (OrderTemp.PaymentType != null && OrderTemp.PaymentType.Name != null)
                        PaymentMethod = OrderTemp.PaymentType.Name;

                    if (PositionInfo != null)
                    {
                        if (OrderTemp.Text != null)
                            Desc = OrderTemp.Text;

                        if(OrderTemp.AddressId != null)
                        base.TextMessage = "Информация о заказе:" +
                                    NewLine() + PositionInfo +
                                    NewLine() + Bold("Адрес доставки: ") + Address.House.Street.City.Name + ", " + Address.House.Street.Name + ", " + Address.House.Number +
                                    NewLine()+Bold("Стоимость доставки:")+ ShipPice.ToString()+
                                    NewLine()+  Bold("Способ оплаты:")+PaymentMethod+
                                    NewLine() + Bold("Кoмментарий к заказу: ") + Desc;

                        if(OrderTemp.PickupPointId!=null)
                            base.TextMessage = "Информация о заказе:" +
                                                NewLine() + PositionInfo +
                                                NewLine() + Bold("Пункт самовывоза: ") + PickupPoint.Name +
                                                NewLine() + Bold("Способ оплаты:") + PaymentMethod +
                                                NewLine() + Bold("Кoмментарий к заказу: ") + Desc;

                        SetInlineKeyBoard();
                        return this;
                    }

                    else
                        return null;
                }

                else
                    return null;
            }
        }

        private void SetInlineKeyBoard()
        {
            base.MessageReplyMarkup = new InlineKeyboardMarkup(
                new[]{
                new[]
                        {
                            DescEditorBtn
                        },
                new[]
                        {
                            MethodOfObtainingEditor
                        },
                new[]
                        {
                            PaymentMethodEditor
                        },
                new[]
                        {
                            SendBtn
                        }

                 });
        }
    }


    /// <summary>
    /// формирует строку с позиция в корзине
    /// </summary>
    public static class BasketPositionInfo
    {
        public static string GetPositionInfo(int FollowerID, int BotId)
        {
            using (MarketBotDbContext db = new MarketBotDbContext())
            {
                var basket = db.Basket.Where(b => b.FollowerId == FollowerID && b.Enable && b.BotInfoId== BotId);

                var IdList = basket.Select(b => b.ProductId).Distinct().AsEnumerable();

                int counter = 1;

                double total = 0.0;

                string message = String.Empty;

                string currency = String.Empty;

                if (IdList.Count() > 0)
                {
                    foreach (int id in IdList)
                    {

                        string name = db.Product.Where(p => p.Id == id).FirstOrDefault().Name;
                        int count = basket.Where(p => p.ProductId == id).Count();
                        var price = db.ProductPrice.Where(p => p.ProductId == id && p.Enabled).Include(p=>p.Currency).FirstOrDefault();
                        message += counter.ToString() + ") " + name + " " + count.ToString() + 
                            " x " + price.ToString() + " = " + (count * price.Value).ToString() + price.Currency.ShortName + Bot.BotMessage.NewLine();
                        total += price.Value * count;
                        counter++;
                        currency = price.Currency.ShortName;

                    }

                    return message + Bot.BotMessage.NewLine() + Bot.BotMessage.Bold("Общая стоимость: ") + total.ToString()+ " " + currency;
                }

                else
                    return null;
            }
        }

        public static double BasketTotalPrice (int FollowerID, int BotId)
        {
            using (MarketBotDbContext db = new MarketBotDbContext())
            {
                var basket = db.Basket.Where(b => b.FollowerId == FollowerID && b.Enable && b.BotInfoId == BotId);

                var IdList = basket.Select(b => b.ProductId).Distinct().AsEnumerable();

                double total = 0.0;


                if (IdList.Count() > 0)
                {
                    foreach (int id in IdList)
                    {
                        int count = basket.Where(p => p.ProductId == id).Count();
                        var price = db.ProductPrice.Where(p => p.ProductId == id && p.Enabled).Include(p => p.Currency).FirstOrDefault();                      
                        total += price.Value * count;
                    }

                    return total;
                }

                else
                    return 0.0;
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.EntityFrameworkCore;
using MyTelegramBot.Bot;
using MyTelegramBot.Messages.Admin;
using MyTelegramBot.Messages;
using MyTelegramBot.Bot.AdminModule;

namespace MyTelegramBot.Messages.Admin
{
    /// <summary>
    /// Сообщение со списком доступнх городов
    /// </summary>
    public class AvailableCitiesMessage:Bot.BotMessage
    {

        public BotMessage BuildMessage()
        {
            using (MarketBotDbContext db=new MarketBotDbContext())
            {
                var Cities = db.AvailableСities.ToList();

                int count = 1;

                base.TextMessage = "Список доступных городов"+NewLine();

                foreach(AvailableСities ac in Cities)
                {
                    base.TextMessage+=NewLine()+count.ToString() + ") " + ac.CityName + " | удалить /cityremove" + ac.Id.ToString()+NewLine(); 
                    count++;
                }

                base.TextMessage += NewLine() + "Что бы добавить новый город нажмите сюда /newcity"+
                    NewLine()+"Вернуться в панель администратора /admin";

                return this;
            }
        }
    }
}

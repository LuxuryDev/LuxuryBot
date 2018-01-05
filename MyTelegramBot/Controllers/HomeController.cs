using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

namespace MyTelegramBot.Controllers
{
    public class HomeController : Controller
    {
        MarketBotDbContext db;

        TelegramBotClient TelegramBot;

        BotInfo BotInfo;

        public IActionResult Index()
        {
            db = new MarketBotDbContext();

            var bot = db.BotInfo.Where(b => b.Name == "MyFirstTest234Bot").FirstOrDefault();

            if (bot == null)
            {
                bot = new BotInfo
                {
                    Name = "",
                    Token = ""
                };
            }

            return View(bot);
        }

        public IActionResult Add()
        {
            return RedirectToAction("Editor");
        }

        public async Task<IActionResult> Editor()
        {
            db = new MarketBotDbContext();

            var bot = db.BotInfo.Where(b => b.Name == "MyFirstTest234Bot").FirstOrDefault();

            if (bot != null)
            {

                TelegramBot = new TelegramBotClient(bot.Token);

                var webhook = await TelegramBot.GetWebhookInfoAsync();

                var botinfo = await TelegramBot.GetMeAsync();

                string info = "Название бота:" + botinfo.Username +
                    "; WebHook адрес: " + webhook.Url + "; Самоподписанный сертификат: " + webhook.HasCustomCertificate + "; Время последней ошибки: " + webhook.LastErrorDate + "; Текст ошибки: " + webhook.LastErrorMessage;

                ViewBag.Webhook = info;
            }

            else // Если данных еще нет
            {
                bot = new BotInfo
                {
                    Name = "",
                    Token = ""
                };


            }


            return View(bot);
        }

        public async Task<IActionResult> Save(BotInfo bot, string URL)
        {
            db = new MarketBotDbContext();

            if (bot != null)
            {
                TelegramBot = new TelegramBotClient(bot.Token);

                if (URL != null && TelegramBot != null) // обновляем url вебхука
                    await TelegramBot.SetWebhookAsync(URL + "/api/values/");

                if (bot.Id == 0) //Бот еще не настроен. Добавляем новые данные
                {
                    InsertBotInfo(bot);
                    string key= Bot.GeneralFunction.GenerateHash();
                    AddOwnerKey(key);
                    return View("Own","/owner"+key);
                    
                }

                if (bot.Id > 0) // редактируем уже сущестующие данные
                {
                    UpdateBotInfo(bot);
 
                    return RedirectToAction("Index");
                }

                else
                    return RedirectToAction("Index");
            }


            else
                return RedirectToAction("Index");


        }


        private int UpdateBotInfo(BotInfo bot)
        {
            if (db == null)
                db = new MarketBotDbContext();

            BotInfo = db.BotInfo.Where(b => b.Id == bot.Id).Include(b => b.Configuration).FirstOrDefault();
            BotInfo.Name = bot.Name;
            BotInfo.Token = bot.Token;
            return db.SaveChanges();
        }

        private int InsertBotInfo(BotInfo bot)
        {
            if (db == null)
                db = new MarketBotDbContext();

            var spl = bot.Token.Split(':');
            int chat_id = Convert.ToInt32(spl[0]);

            BotInfo botInfo = new BotInfo
            {
                Name = bot.Name,
                Token = bot.Token,
                ChatId = chat_id,
                Timestamp = DateTime.Now
            };

            db.BotInfo.Add(botInfo);
            return db.SaveChanges();
        }

        private AdminKey AddOwnerKey(string key)
        {
            if (db == null)
                db = new MarketBotDbContext();

            AdminKey adminKey = new AdminKey
            {
                DateAdd = DateTime.Now,
                Enable = true,
                KeyValue = key
            };

            db.AdminKey.Add(adminKey);
            db.SaveChanges();
            return adminKey;
        }
    }
}

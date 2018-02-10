using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using MyTelegramBot.Model;
using Microsoft.AspNetCore.Mvc.Rendering;
using HtmlAgilityPack;

namespace MyTelegramBot.Controllers
{
    public class InstallController : Controller
    {

        MarketBotDbContext db;

        TelegramBotClient TelegramBot;

        public IActionResult Index()
        {
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> InstallHomeVersion(string _token)
        {
            db = new MarketBotDbContext();
            string name = Bot.GeneralFunction.GetBotName().Trim();
            string ngrok = GetNgrokUrl().Trim();
            string token = _token.Trim();

            if (token != null)
            {
                var BotInf = db.BotInfo.Where(b => b.Name == name).FirstOrDefault();
                db.Dispose();
                //если бот уже установлен
                if (BotInf != null)
                    return NotFound();

                else
                {
                    if (await SetWebhookAsync(token, ngrok, new Telegram.Bot.Types.FileToSend { }))
                    {
                        InsertNewBotToDb(token, name, ngrok);
                        return Ok();
                    }

                    else
                        return NotFound();
                }
            }

            else
                return NotFound();
        }

        /// <summary>
        /// Удаляем веб хук 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> StopHomeVersion()
        {
            using (MarketBotDbContext db = new MarketBotDbContext())
            {

                try
                {
                    var botinfo = db.BotInfo.Where(b => b.Name == Bot.GeneralFunction.GetBotName()).FirstOrDefault();

                    TelegramBot = new TelegramBotClient(botinfo.Token);
                    await TelegramBot.DeleteWebhookAsync();

                    return Ok();
                }

                catch
                {
                    return NotFound();
                }
            }
        }

        /// <summary>
        /// Запускаем бота. Вытаскиваем ngrok url и обновляем веб хук
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> StartHomeVersion()
        {
            using (MarketBotDbContext db=new MarketBotDbContext())
            {
                try
                {
                    string NgrokUrl = GetNgrokUrl();
                    var BotInfo = db.BotInfo.Where(b => b.Name == Bot.GeneralFunction.GetBotName()).FirstOrDefault();

                    TelegramBot = new TelegramBotClient(BotInfo.Token);
                    await TelegramBot.SetWebhookAsync(NgrokUrl + "/bot/");

                    BotInfo.WebHookUrl = NgrokUrl;
                    db.SaveChanges();

                    return Ok();
                }

                catch 
                {
                    return NotFound();
                }
            }
        }

        private async Task<bool> SetWebhookAsync(string token, string url, Telegram.Bot.Types.FileToSend fileToSend)
        {
            try
            {
                TelegramBot = new TelegramBotClient(token);

                if(fileToSend.Content==null)
                    await TelegramBot.SetWebhookAsync(url + "/bot/", null);

                if (fileToSend.Content != null)
                    await TelegramBot.SetWebhookAsync(url + "/bot/", fileToSend);


                return true;
            }

            catch
            {
                return false;
            }
        }

        [HttpGet]
        public string GetNgrokUrl()
        {
            try
            {
                var url = "http://127.0.0.1:4040/status";
                var web = new HtmlWeb();
                var doc = web.Load(url);

                var res = doc.ParsedText.IndexOf(".ngrok.io");

                return doc.ParsedText.Substring(res - 16, 25);
            }

            catch
            {
                return null;
            }
        }

        private void InsertNewBotToDb(string token, string name, string Url)
        {
            if(db==null)
                db = new MarketBotDbContext();

            if (token != null && name != null && Url != null)
            {
                var conf = new Configuration { VerifyTelephone = false, OwnerPrivateNotify = false, Delivery = true, Pickup = false, ShipPrice = 0, FreeShipPrice = 0, CurrencyId = 1 };

                BotInfo botInfo = new BotInfo
                {
                    Token = token,
                    Name = name,
                    WebHookUrl = Url,
                    Configuration = conf,
                    Timestamp = DateTime.Now
                };

                db.BotInfo.Add(botInfo);
                db.SaveChanges();

                Company company = new Company { Instagram = String.Empty, Vk = String.Empty, Chanel = String.Empty, Chat = String.Empty };
                db.Company.Add(company);
                db.SaveChanges();
            }
        }
    }
}
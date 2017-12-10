using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Jojatekok.MoneroAPI;
using Jojatekok.MoneroAPI.Extensions;
using Jojatekok.MoneroAPI.RpcUtilities;
using Jojatekok.MoneroAPI.Settings;
namespace MyTelegramBot.Controllers
{
    public class HomeController : Controller
    {
        MarketBotDbContext context;

        public IActionResult Index()
        {
            context = new MarketBotDbContext();

            var bot = context.BotInfo.Where(b=>b.Name== "MyFirstTest234Bot").FirstOrDefault();
            return View();
        }

        public IActionResult ProductPreview()
        {
            context = new MarketBotDbContext();

            return View(context.Product.Include(p=>p.Category).Include(p=>p.ProductPrice).FirstOrDefault());
        }

        public IActionResult Editor()
        {
            BotInfo bot = new BotInfo();
            return View(bot);
        }

        public IActionResult Save(BotInfo bot)
        {
            using(MarketBotDbContext db=new MarketBotDbContext())
            {
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
                db.SaveChanges();
                Editor();
                return Ok();
            }

            
        }
    }
}

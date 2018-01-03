using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MyTelegramBot.Controllers
{
    public class HelpController : Controller
    {
        MarketBotDbContext db;

        HelpDesk HelpDesk;

        public IActionResult Index()
        {
            db = new MarketBotDbContext();

            var list = db.HelpDesk.Include(h=>h.Follower).Include(h=>h.BotInfo).ToList();

            return View(list);
        }
    }
}
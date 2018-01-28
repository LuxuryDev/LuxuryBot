using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace MyTelegramBot.Controllers
{
    public class HelpController : Controller
    {
        MarketBotDbContext db;

        HelpDesk HelpDesk;

        public IActionResult Index()
        {
            db = new MarketBotDbContext();

            var list = db.HelpDesk.Where(h=>h.Number>0).Include(h=>h.Follower).Include(h=>h.BotInfo).OrderByDescending(h=>h.Id).ToList();

            return View(list);
        }

        [HttpGet]
        public IActionResult Get(int id)
        {
            if (db == null)
                db = new MarketBotDbContext();

            if (id > 0)
                HelpDesk = db.HelpDesk.Where(h => h.Number == id).Include(h=>h.HelpDeskInWork).Include(h=>h.HelpDeskAnswer).Include(h=>h.Follower).Include(h=>h.HelpDeskAttachment).FirstOrDefault();


            if (HelpDesk != null)
            {
                if (HelpDesk.HelpDeskInWork != null)
                    foreach (HelpDeskInWork work in HelpDesk.HelpDeskInWork)
                        work.Follower = db.Follower.Where(f => f.Id == work.FollowerId).FirstOrDefault();

                if (HelpDesk.HelpDeskAnswer != null)
                    foreach (HelpDeskAnswer answer in HelpDesk.HelpDeskAnswer)
                        answer.Follower = db.Follower.Where(f => f.Id == answer.FollowerId).FirstOrDefault();

                return View(HelpDesk);
            }



            else
                return NotFound();
        }


    }
}
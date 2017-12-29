using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MyTelegramBot.Controllers
{
    public class OrderController : Controller
    {
        MarketBotDbContext db;

        Orders Order;


        public IActionResult Index()
        {
            db = new MarketBotDbContext();

           return View(db.Orders.Include(o => o.BotInfo).OrderByDescending(o=>o.Id).ToList());


        }

        [HttpGet]
        public IActionResult Get(int Number)
        {
            if (db == null)
                db = new MarketBotDbContext();

            Order = db.Orders.Where(o => o.Number == 1).Include(o=>o.Invoice).Include(o => o.OrderConfirm).
                Include(o => o.OrderDeleted).Include(o => o.OrderDone).Include(o => o.OrderProduct).Include(o => o.OrderAddress).Include(o => o.FeedBack).Include(o => o.Follower).FirstOrDefault();

            return View(Order);
        }

        //public IActionResult Search(string query)
        //{
        //    List<Orders> list = new List<Orders>();

        //    db = new MarketBotDbContext();

        //    list = db.Orders.Where(o=>o.Number==Convert.ToDecimal(query)).ToList();

        //    return PartialView(list);
        //}
    }
}
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

           return View(db.Orders.Include(o => o.BotInfo).Include(o=>o.OrderDone).OrderByDescending(o=>o.Id).ToList());


        }

        [HttpGet]
        public IActionResult Get(int id)
        {
            if (db == null)
                db = new MarketBotDbContext();

            if(id>0)
            Order = db.Orders.Where(o => o.Number == id).Include(o=>o.Invoice).Include(o => o.OrderConfirm).
                Include(o => o.OrderDeleted).Include(o => o.OrderDone).Include(o => o.OrderProduct).Include(o=>o.FeedBack).Include(o => o.OrderAddress).Include(o => o.FeedBack).Include(o=>o.OrdersInWork).Include(o => o.Follower).FirstOrDefault();

            if (Order != null)
            {
                Order.OrderAddress.Adress = db.Address.Where(a => a.Id == Order.OrderAddress.AdressId).Include(a => a.House).Include(a => a.House.Street).Include(a => a.House.Street.City).FirstOrDefault();

                if (Order.Invoice != null)
                    Order.Invoice.PaymentType = db.PaymentType.Where(payment => payment.Id == Order.Invoice.PaymentTypeId).FirstOrDefault();

                if (Order.OrderConfirm != null)
                    foreach (OrderConfirm confirm in Order.OrderConfirm)
                        confirm.Follower = db.Follower.Where(f => f.Id == confirm.FollowerId).FirstOrDefault();

                if (Order.OrderDeleted != null)
                    foreach (OrderDeleted delete in Order.OrderDeleted)
                        delete.Follower = db.Follower.Where(f => f.Id == delete.FollowerId).FirstOrDefault();

                if (Order.OrdersInWork != null)
                    foreach (OrdersInWork work in Order.OrdersInWork)
                        work.Follower = db.Follower.Where(f => f.Id == work.FollowerId).FirstOrDefault();

                foreach (OrderProduct Op in Order.OrderProduct)
                {
                    Op.Product = db.Product.Where(p => p.Id == Op.ProductId).FirstOrDefault();
                    Op.Price = db.ProductPrice.Where(price => price.Id == Op.PriceId).Include(price => price.Currency).FirstOrDefault();

                }



                return View(Order);
            }

            else
                return NotFound();
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
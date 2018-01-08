using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MyTelegramBot.Controllers
{
    [Produces("application/json")]
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

        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Confirm ([FromBody] OrderConfirm confirm)
        {
            return Json("hui");
        }

        /// <summary>
        /// Взять заказ в обработку
        /// </summary>
        /// <param name="OrderId"></param>
        /// <param name="Take"></param>
        /// <param name="FollowerId"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TakeInWork([FromBody] OrdersInWork inWork)
        {
            
            if (db == null)
                db = new MarketBotDbContext();

            inWork.FollowerId = db.Follower.Where(f => f.ChatId == db.BotInfo.FirstOrDefault().OwnerChatId).FirstOrDefault().Id;

            var follower = CheckInWork(Convert.ToInt32(inWork.OrderId));

            if (follower == null || follower != null && follower.Id == inWork.FollowerId)
            {
                InsertInWork(inWork.OrderId, inWork.FollowerId, inWork.InWork);

                if(inWork.InWork==true)
                    return Json("В работе");

                else
                    return Json("Свободна");
            }

            else
                return Json("Заявка в обработке у " + follower.FirstName+" "+ follower.LastName);

            //Order= db.Orders.Where(o => o.Id == OrderId).Include(o => o.OrdersInWork).FirstOrDefault();

            //foreach (OrdersInWork work in Order.OrdersInWork)
            //    work.Follower = db.Follower.Find(work.FollowerId);

            
        }

        private int InsertInWork(int? OrderId, int? FollowerId, bool? Take=true)
        {
            if (db == null)
                db = new MarketBotDbContext();

            if (OrderId > 0 && FollowerId > 0 && Take!=null)
            {
                OrdersInWork inWork = new OrdersInWork
                {
                    FollowerId = FollowerId,
                    OrderId = OrderId,
                    InWork = Take,
                    Timestamp = DateTime.Now
                };

                db.OrdersInWork.Add(inWork);

                return db.SaveChanges();
            }

            else
                return -1;
        }

        /// <summary>
        /// Проверяет кем обрабатывается заказ 
        /// </summary>
        /// <param name="OrderId"></param>
        /// <returns></returns>
        private Follower CheckInWork (int OrderId)
        {
            if (db == null)
                db = new MarketBotDbContext();

            var inwork = db.OrdersInWork.Where(o => o.OrderId == OrderId && o.InWork==true).OrderByDescending(o => o.Id).FirstOrDefault();

            if (inwork != null)
                return db.Follower.Find(inwork.FollowerId);

            else
                return null;
        }


        [HttpGet]
        //[Produces("application/json")]
        public IActionResult GetInWorkList(int Id)
        {
            if (db == null)
                db = new MarketBotDbContext();

            if (Id > 0)
                Order = db.Orders.Where(o => o.Id == Id).Include(o => o.OrdersInWork).FirstOrDefault();

            if (Order.OrdersInWork != null)
                foreach (OrdersInWork work in Order.OrdersInWork)
                    work.Follower = db.Follower.Where(f => f.Id == work.FollowerId).FirstOrDefault();

            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();

            foreach (OrdersInWork work in Order.OrdersInWork)
            {
                Dictionary<string, string> name = new Dictionary<string, string>();
                name.Add("name", work.Follower.FirstName + work.Follower.LastName);
                name.Add("Timestamp", work.Timestamp.ToString());

                if(work.InWork==true)
                    name.Add("InWork", "Взял");
                if (work.InWork == false)
                    name.Add("InWork", "Освободил");

                list.Add(name);
            }

            return Json(list);
        }
    }
}
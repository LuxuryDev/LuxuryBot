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
        public IActionResult GetConfirmList(int Id)
        {
            if (db == null)
                db = new MarketBotDbContext();

            if (Id > 0)
                Order = db.Orders.Where(o => o.Id == Id).Include(o => o.OrderConfirm).FirstOrDefault();

            if (Order.OrdersInWork != null)
                foreach (OrderConfirm confirm in Order.OrderConfirm)
                    confirm.Follower = db.Follower.Where(f => f.Id == confirm.FollowerId).FirstOrDefault();

            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();

            foreach (OrderConfirm confirm in Order.OrderConfirm)
            {
                Dictionary<string, string> value = new Dictionary<string, string>();
                value.Add("name", confirm.Follower.FirstName + confirm.Follower.LastName);
                value.Add("Timestamp", confirm.DateAdd.ToString());
                value.Add("Text", confirm.Text.ToString());
                list.Add(value);
            }

            return Json(list);
        }

        [HttpGet]
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

                if (work.InWork == true)
                    name.Add("InWork", "Взял");
                if (work.InWork == false)
                    name.Add("InWork", "Освободил");

                list.Add(name);
            }

            return Json(list);
        }

        [HttpGet]
        public IActionResult GetDoneList(int Id)
        {
            if (db == null)
                db = new MarketBotDbContext();

            if (Id > 0)
                Order = db.Orders.Where(o => o.Id == Id).Include(o => o.OrderDone).FirstOrDefault();

            if (Order.OrderDone != null)
                foreach (OrderDone done in Order.OrderDone)
                    done.Follower = db.Follower.Where(f => f.Id == done.FollowerId).FirstOrDefault();

            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();

            foreach (OrderDone done in Order.OrderDone)
            {
                Dictionary<string, string> value = new Dictionary<string, string>();
                value.Add("name", done.Follower.FirstName + done.Follower.LastName);
                value.Add("Timestamp", done.DateAdd.ToString());
                value.Add("Text", done.ToString());
                list.Add(value);
            }

            return Json(list);
        }

        [HttpGet]
        public IActionResult GetDeleteList(int Id)
        {
            if (db == null)
                db = new MarketBotDbContext();

            if (Id > 0)
                Order = db.Orders.Where(o => o.Id == Id).Include(o => o.OrderDeleted).FirstOrDefault();

            if (Order.OrderDeleted != null)
                foreach (OrderDeleted del in Order.OrderDeleted)
                    del.Follower = db.Follower.Where(f => f.Id == del.FollowerId).FirstOrDefault();

            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();

            foreach (OrderDeleted del in Order.OrderDeleted)
            {
                Dictionary<string, string> value = new Dictionary<string, string>();
                value.Add("name", del.Follower.FirstName + del.Follower.LastName);
                value.Add("Timestamp", del.DateAdd.ToString());
                value.Add("Text", del.Text.ToString());
                list.Add(value);
            }

            return Json(list);
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
        public IActionResult Delete([FromBody] OrderDeleted deleted)
        {
            db = new MarketBotDbContext();

            if (deleted != null && deleted.FollowerId == null)
                deleted.FollowerId = db.Follower.Where(f => f.ChatId == db.BotInfo.FirstOrDefault().OwnerChatId).FirstOrDefault().Id;

            if (deleted != null && deleted.OrderId > 0)
                this.Order = db.Orders.Find(deleted.OrderId);

            if (this.Order!=null && !this.Order.Deleted && InsertDelete(deleted)>0)
            {
                this.Order.Deleted = true;
                db.SaveChanges();
                return Json("Удалено!");
            }

            else
                return Json("Заказ уже удален");
 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Confirm ([FromBody] OrderConfirm confirm)
        {
            db = new MarketBotDbContext();

            if (confirm != null && confirm.FollowerId == null)
            {
                confirm.FollowerId = db.Follower.Where(f => f.ChatId == db.BotInfo.FirstOrDefault().OwnerChatId).FirstOrDefault().Id;
                this.Order = db.Orders.Find(confirm.OrderId);

            }

            if (InsertConfirm(confirm) > 0 && !this.Order.Deleted)
            {
                this.Order.Confirmed = true;
                db.SaveChanges();
                return Json("Добавлено");
            }

            if (this.Order.Deleted)
                return Json("Ошибка.Заказ удален!");

            else
                return Json("Ошибка");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Done ([FromBody] OrderDone done)
        {
            if (db == null)
                db = new MarketBotDbContext();

            if (done != null && done.OrderId > 0)
                this.Order = db.Orders.Find(done.OrderId);

            if (done != null && done.FollowerId == null)
                done.FollowerId = db.Follower.Where(f => f.ChatId == db.BotInfo.FirstOrDefault().OwnerChatId).FirstOrDefault().Id;

            if (this.Order.Confirmed && done != null && InsertDone(done)>0)
            {
                this.Order.Done = true;
                db.SaveChanges();
                return Json("Сохранено");
            }

            if (!this.Order.Confirmed)
                return Json("Ошибка! Заказ еще не согласован");

            else
                return Json("Неизвестная ошибка!");
                
        }

        [HttpGet]
        public IActionResult Recovery (int Id)
        {
            db = new MarketBotDbContext();

            if (Id > 0)
                this.Order = db.Orders.Find(Id);

            if(this.Order!=null && this.Order.Deleted)
            {
                var list = db.OrderDeleted.Where(d => d.OrderId == Id).ToList();

                this.Order.Deleted = false;

                foreach (OrderDeleted del in list)
                    db.Remove(del);

              
                db.SaveChanges();

                return Json("Восстановлено");
            }

            if (this.Order != null && this.Order.Deleted)
                return Json("Заказ еще не удален");

            else
                return Json("Ошибка");
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

            var CurrentInWork = CheckInWork(Convert.ToInt32(inWork.OrderId));

            //заявка ни кем не обрабатывается или уже  обрабатывается текущим пользовтелем
            if (CurrentInWork == null || CurrentInWork != null && CurrentInWork.FollowerId == inWork.FollowerId)
            {
                if (CurrentInWork == null ||
                    CurrentInWork != null && CurrentInWork.InWork == true && inWork.InWork == false
                    )
                    // заявка ни кем не обрабатывается и пользователь берет ее в обработку или
                    // пользователь хочет освободить заявку
                    InsertInWork(inWork.OrderId, inWork.FollowerId, inWork.InWork);

                if (inWork.InWork == true)
                    return Json("В работе");

                else
                    return Json("Свободна");
            }

            else
                return Json("Заявка в обработке у " + CurrentInWork.Follower.FirstName + " " + CurrentInWork.Follower.LastName);

            //Order= db.Orders.Where(o => o.Id == OrderId).Include(o => o.OrdersInWork).FirstOrDefault();

            //foreach (OrdersInWork work in Order.OrdersInWork)
            //    work.Follower = db.Follower.Find(work.FollowerId);


        }

        private int InsertDone (OrderDone done)
        {
            if (db == null)
                db = new MarketBotDbContext();

            if (done != null && done.OrderId > 0 && done.FollowerId > 0)
            {
                done.DateAdd = DateTime.Now;
                done.Done = true;
                db.OrderDone.Add(done);
                return db.SaveChanges();
            }

            else
                return -1;
        }

        private int InsertConfirm (OrderConfirm orderConfirm)
        {
            if (db == null)
                db = new MarketBotDbContext();

            if (orderConfirm != null && orderConfirm.OrderId > 0 && orderConfirm.FollowerId > 0)
            {
                orderConfirm.Confirmed = true;
                orderConfirm.DateAdd = DateTime.Now;
                db.OrderConfirm.Add(orderConfirm);
                return db.SaveChanges();
            }

            else
                return -1;
        }

        private int InsertInWork(int? OrderId, int? FollowerId, bool? Take = true)
        {
            if (db == null)
                db = new MarketBotDbContext();

            if (OrderId > 0 && FollowerId > 0 && Take != null)
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

        private int InsertDelete(OrderDeleted deleted)
        {
            if (db == null)
                db = new MarketBotDbContext();

            if (deleted != null && deleted.OrderId > 0 && deleted.FollowerId > 0)
            {
                deleted.DateAdd = DateTime.Now;
                deleted.Deleted = true;
                db.OrderDeleted.Add(deleted);
                return db.SaveChanges();
            }

            else
                return -1;
        }

        private OrdersInWork CheckInWork(int OrderId)
        {
            if (db == null)
                db = new MarketBotDbContext();

            var inwork = db.OrdersInWork.Where(o => o.OrderId == OrderId).OrderByDescending(o => o.Id).FirstOrDefault();

            if (inwork != null)
                inwork.Follower = db.Follower.Find(inwork.FollowerId);

            if (inwork != null && inwork.InWork == true)
                return inwork;

            else
                return null;
        }

        


    }
}
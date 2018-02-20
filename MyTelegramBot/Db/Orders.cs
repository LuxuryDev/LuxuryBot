using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyTelegramBot
{
    public partial class Orders
    {
        public Orders()
        {
            OrderProduct = new HashSet<OrderProduct>();
            OrdersInWork = new HashSet<OrdersInWork>();
        }

        public int Id { get; set; }
        public int? Number { get; set; }
        public int FollowerId { get; set; }
        public string Text { get; set; }
        public DateTime? DateAdd { get; set; }
        public bool? Paid { get; set; }
        public int? BotInfoId { get; set; }
        public int? InvoiceId { get; set; }
        public int? ConfirmId { get; set; }
        public int? DeleteId { get; set; }
        public int? DoneId { get; set; }
        public int? PickupPointId { get; set; }

        public int? CurrentStatus { get; set; }

        public BotInfo BotInfo { get; set; }
        public OrderHistory Confirm { get; set; }
        public OrderHistory Delete { get; set; }
        public OrderHistory Done { get; set; }
        public Follower Follower { get; set; }
        public Invoice Invoice { get; set; }
        public PickupPoint PickupPoint { get; set; }
        public FeedBack FeedBack { get; set; }
        public OrderAddress OrderAddress { get; set; }
        public ICollection<OrderProduct> OrderProduct { get; set; }
        public ICollection<OrdersInWork> OrdersInWork { get; set; }

        public OrderStatus CurrentStatusNavigation { get; set; }
        /// <summary>
        /// Посчитать стоимость без учета стоимости доставки
        /// </summary>
        /// <returns></returns>
        public double TotalPrice()
        {
            int counter = 0;
            double total =0.0;
            using (MarketBotDbContext db = new MarketBotDbContext())
            {
                foreach (OrderProduct p in OrderProduct) // Вытаскиваем все товары из заказа
                {
                    counter++;

                    if(p.Product==null)
                    p.Product = db.Product.Where(x => x.Id == p.ProductId).Include(x => x.ProductPrice).FirstOrDefault();

                    if (p.Price == null)
                        p.Price = p.Product.ProductPrice.FirstOrDefault();


                    total += p.Price.Value * p.Count;
                }
            }

            return total;
        }
    }
}

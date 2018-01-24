using System;
using System.Collections.Generic;

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
    }
}

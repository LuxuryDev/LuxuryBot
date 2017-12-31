﻿using System;
using System.Collections.Generic;

namespace MyTelegramBot
{
    public partial class Orders
    {
        public Orders()
        {
            OrderConfirm = new HashSet<OrderConfirm>();
            OrderDeleted = new HashSet<OrderDeleted>();
            OrderDone = new HashSet<OrderDone>();
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

        public BotInfo BotInfo { get; set; }
        public Follower Follower { get; set; }
        public Invoice Invoice { get; set; }
        public FeedBack FeedBack { get; set; }
        public OrderAddress OrderAddress { get; set; }
        public ICollection<OrderConfirm> OrderConfirm { get; set; }
        public ICollection<OrderDeleted> OrderDeleted { get; set; }
        public ICollection<OrderDone> OrderDone { get; set; }
        public ICollection<OrderProduct> OrderProduct { get; set; }
        public ICollection<OrdersInWork> OrdersInWork { get; set; }
    }
}

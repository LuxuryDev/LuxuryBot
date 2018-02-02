using System;
using System.Collections.Generic;

namespace MyTelegramBot
{
    public partial class FeedBack
    {
        public FeedBack()
        {
            FeedBackAttachmentFs = new HashSet<FeedBackAttachmentFs>();
        }

        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime? DateAdd { get; set; }
        public int? OrderId { get; set; }
        public int? RaitingId { get; set; }

        public int? RaitingValue { get; set; }

        public Orders Order { get; set; }
        public Raiting Raiting { get; set; }
        public ICollection<FeedBackAttachmentFs> FeedBackAttachmentFs { get; set; }
    }
}

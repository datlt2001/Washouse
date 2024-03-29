﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class Payment
    {
        public Payment()
        {
            WalletTransactions = new HashSet<WalletTransaction>();
        }

        public int Id { get; set; }
        public string OrderId { get; set; }
        public decimal Total { get; set; }
        public decimal PlatformFee { get; set; }
        public DateTime? Date { get; set; }
        public string Status { get; set; }
        public int PaymentMethod { get; set; }
        public int? PromoCode { get; set; }
        public decimal? Discount { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }

        public virtual Order Order { get; set; }
        public virtual Promotion PromoCodeNavigation { get; set; }
        public virtual ICollection<WalletTransaction> WalletTransactions { get; set; }
    }
}

﻿using System;

namespace Washouse.Model.ResponseModels
{
    public class WalletTransactionResponseModel
    {
        public int PaymentId { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string PlusOrMinus { get; set; }
        public decimal Amount { get; set; }
        public string TimeStamp { get; set; }
        public string UpdateTimeStamp { get; set; }
    }
}
﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class FeedbackOrderRequestModel
    {
        public string OrderId { get; set; }
        public int CenterId { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public int Rating { get; set; }       
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels
{
    public class CenterDetailResponseModel
    {

        public CenterDetailResponseModel()
        {
            CenterServices = new HashSet<CenterServiceResponseModel>();
            CenterOperatingHours = new HashSet<CenterOperatingHoursResponseModel>();
        }
        public int Id { get; set; }
        public string Thumbnail { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        public virtual ICollection<CenterServiceResponseModel> CenterServices { get; set; }

        public decimal? Rating { get; set; }
        public int NumOfRating { get; set; }
        public string Phone { get; set; }
        public string CenterAddress { get; set; }
        public double Distance { get; set; }
        public CenterLocationResponseModel CenterLocation { get; set; }
        public virtual ICollection<CenterOperatingHoursResponseModel> CenterOperatingHours { get; set; }

    }
}

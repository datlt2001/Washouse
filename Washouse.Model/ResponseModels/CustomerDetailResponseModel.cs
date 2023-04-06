using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels
{
    public class CustomerDetailResponseModel
    {
        public int Id { get; set; }
        public int? AccountId { get; set; }
        //public bool Status { get; set; }
        public string Fullname { get; set; }
        public string Phone { get; set; }       
        public string Email { get; set; }

        public string ProfilePic { get; set; }
        public string AddressString { get; set; }
        public CustomerLocatonResponseModel Address { get; set; }

        public int? Gender { get; set; }
        public string Dob { get; set; }

        public int? WalletId { get; set; }
    }
}

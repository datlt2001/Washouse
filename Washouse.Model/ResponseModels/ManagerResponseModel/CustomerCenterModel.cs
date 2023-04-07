using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels.ManagerResponseModel
{
    public class CustomerCenterModel
    {
        public int Id { get; set; }
        public int? AccountId { get; set; }
        public string Fullname { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string AddressString { get; set; }
        public string DateOfBirth { get; set; }
        public int? Gender { get; set; }
    }
}

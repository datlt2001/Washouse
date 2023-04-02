using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public interface IFeedbackRepository :IRepository<Feedback>
    {
        public IEnumerable<int> GetIDList();
        public IEnumerable<Feedback> GetAllByCenterId(int id);
        public IEnumerable<Feedback> GetAllByOrderDetailId(int orderdetailId);
        public IEnumerable<Feedback> GetAllByServiceId(int serviceId);
    }
}

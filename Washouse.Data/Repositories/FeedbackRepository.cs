using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public class FeedbackRepository : RepositoryBase<Feedback>, IFeedbackRepository
    {
        public FeedbackRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }

        public IEnumerable<int> GetIDList()
        {
            var query = from fb in DbContext.Feedbacks
                        select fb.Id ;
            return query.ToList();
        }

        public  IEnumerable<Feedback> GetAllByCenterId(int centerid)
        {
            var data =  this._dbContext.Feedbacks
                        .Where(fb => fb.CenterId == centerid)
                        .ToList();
            return data;
        }

        public IEnumerable<Feedback> GetAllByOrderDetailId(int orderdetailId)
        {
            var data = this._dbContext.Feedbacks
                        .Where(fb => fb.OrderDetailId == orderdetailId)
                        .ToList();
            return data;
        }
    }
}

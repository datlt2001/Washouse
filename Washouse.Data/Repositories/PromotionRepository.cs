using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public class PromotionRepository : RepositoryBase<Promotion>, IPromotionRepository
    {
       public PromotionRepository(IDbFactory dbFactory) : base(dbFactory)
        {

        }

        public IEnumerable<Promotion> GetAllByCenterId(int centerid)
        {
            var data = this._dbContext.Promotions
                        .Where(fb => fb.CenterId == centerid)
                        .ToList();
            return data;
        }
    }
}

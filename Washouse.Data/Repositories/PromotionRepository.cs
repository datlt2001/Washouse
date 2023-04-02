using Microsoft.EntityFrameworkCore;
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

        public async Task<Promotion> CheckValidPromoCode(int centerId, string promoCode)
        {
            var promotions = await this._dbContext.Promotions
                                    .Include(pro => pro.Center)
                                .Where(pro => pro.Code == promoCode)
                                .ToListAsync();
            if (promotions != null) {
                return null;
            } else
            {
                foreach (var item in promotions)
                {
                    if (item.CenterId == centerId)
                    {
                        if (item.StartDate < DateTime.Now && item.ExpireDate > DateTime.Now && item.UseTimes > 0)
                        {
                            return item;
                        }
                    }
                }
                return null;
            }

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

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
    public class DeliveryPriceChartRepository : RepositoryBase<DeliveryPriceChart>, IDeliveryPriceChartRepository
    {
        public DeliveryPriceChartRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }

        public async Task<IEnumerable<DeliveryPriceChart>> GetDeliveryPriceChartOfACenter(int centerId)
        {
            var data = await this._dbContext.DeliveryPriceCharts
                    .Where(i => i.CenterId == centerId)
                    .ToListAsync();
            return data;
        }
    }
}

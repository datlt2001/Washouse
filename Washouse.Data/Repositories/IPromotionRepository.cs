using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public interface IPromotionRepository : IRepository<Promotion>
    {
         IEnumerable<Promotion> GetAllByCenterId(int centerid);
        Task<Promotion> CheckValidPromoCode(int centerId, string promoCode);

        decimal GetDiscountByCode(string code, int centerId);

    }
}

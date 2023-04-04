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
        public IEnumerable<Promotion> GetAllByCenterId(int centerid);
        Task<Promotion> CheckValidPromoCode(int centerId, string promoCode);

        public decimal GetDiscountByCode(string code);

    }
}

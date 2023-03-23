using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface IPromotionService
    {
        public Task Add(Promotion promotion);

        public Task Update(Promotion promotion);

        IEnumerable<Promotion> GetAll();

        public Task<Promotion> GetById(int id);

        public IEnumerable<Promotion> GetAllByCenterId(int centerid);
    }
}

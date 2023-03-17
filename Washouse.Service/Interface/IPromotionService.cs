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
        public Task Add(Customer promotion);

        public Task Update(Customer promotion);

        IEnumerable<Customer> GetAll();

        public Task<Customer> GetById(int id);
    }
}

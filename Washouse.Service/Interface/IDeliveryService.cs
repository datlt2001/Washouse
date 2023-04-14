using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface IDeliveryService
    {

        public Task Update(Delivery delivery);

        public Task<Delivery> GetById(int id);
    }
}

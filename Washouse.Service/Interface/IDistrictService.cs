using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface IDistrictService
    {
        Task<IEnumerable<District>> GetAll();
        Task<District> GetDistrictByName(string name);

        public Task<District> GetDistrictById(int id);
    }
}

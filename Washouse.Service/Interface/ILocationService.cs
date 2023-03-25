using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface ILocationService
    {
        Task<Location> GetLocationOfACenter(int centerId);
        Task<Location> Add(Location location);
        Task Update(Location location);
        Task<Location> GetById(int id);
    }
}

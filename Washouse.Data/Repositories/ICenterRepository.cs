using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public interface ICenterRepository : IRepository<Center>
    {
        IEnumerable<Center> SortCenterByLocation();
        Task ActivateCenter(int id);

        Task DeactivateCenter(int id);
    }
}
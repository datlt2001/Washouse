//using GoogleMaps.LocationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public class CenterRepository : RepositoryBase<Center>, ICenterRepository
    {
        public CenterRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }

        public Task ActivateCategory(int id)
        {
            throw new NotImplementedException();
        }

        public Task DeactivateCategory(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Center> SortCenterByLocation()
        {
            return null;
        }
    }
}

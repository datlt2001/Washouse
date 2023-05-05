using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public class CenterRequestRepository : RepositoryBase<CenterRequest>, ICenterRequestRepository
    {
        public CenterRequestRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }
        public async Task<IEnumerable<CenterRequest>> GetCenterRequests()
        {
            var newestCenterRequests =  this.DbContext.CenterRequests
                .GroupBy(cr => cr.CenterRequesting)
                .Select(g => g.OrderByDescending(cr => cr.UpdatedDate).FirstOrDefault());
            return newestCenterRequests;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Data.Infrastructure
{
    public class DbFactory : Disposable, IDbFactory
    {
        WashouseDbContext dbContext;

        public WashouseDbContext Init()
        {
            return dbContext ?? (dbContext = new WashouseDbContext());
        }

        protected override void DisposeCore()
        {
            if (dbContext != null)
                dbContext.Dispose();
        }
    }
}

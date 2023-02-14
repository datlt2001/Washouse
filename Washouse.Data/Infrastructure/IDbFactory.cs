using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Data.Infrastructure
{
    public interface IDbFactory : IDisposable
    {
        WashouseDbContext Init();
    }
}

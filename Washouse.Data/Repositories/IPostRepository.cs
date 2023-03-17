using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public interface  IPostRepository :IRepository<Post>
    {
        public Task ActivatePost(int id);

        public Task DeactivatePost(int id);
    }
}

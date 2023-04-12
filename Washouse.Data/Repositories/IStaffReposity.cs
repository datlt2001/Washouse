using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public interface  IStaffReposity : IRepository<Staff>
    {
        public Task DeactivateStaff(int id);

        public Task ActivateStaff(int id);

        public IEnumerable<Staff> GetAllByCenterId(int centerid);

        Staff GetStaffByAccountId(int id);

        Staff GetStaffByCenterId(int centerid);

    }
}

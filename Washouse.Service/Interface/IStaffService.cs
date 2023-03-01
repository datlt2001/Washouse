using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface IStaffService
    {
        public Task Add(Staff staff);

        public Task Update(Staff staff);

        IEnumerable<Staff> GetAll();

        public Task<Staff> GetById(int id);

        public Task DeactivateStaff(int id);

        public Task ActivateStaff(int id);
    }
}

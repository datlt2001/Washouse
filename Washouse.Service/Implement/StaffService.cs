using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Washouse.Data.Infrastructure;
using Washouse.Data.Repositories;
using Washouse.Model.Models;
using Washouse.Service.Interface;

namespace Washouse.Service.Implement
{
    public class StaffService : IStaffService
    {
        public IStaffReposity _staffReposity;
        public IUnitOfWork _unitOfWork;

        public StaffService(IStaffReposity staffReposity, IUnitOfWork unitOfWork)
        {
            _staffReposity = staffReposity;
            _unitOfWork = unitOfWork;
        }

        public async Task ActivateStaff(int id)
        {
            await _staffReposity.ActivateStaff(id);
        }

        public async Task Add(Staff staff)
        {
            await _staffReposity.Add(staff);
        }

        public async Task DeactivateStaff(int id)
        {
            await _staffReposity.DeactivateStaff(id);
        }

        public IEnumerable<Staff> GetAll()
        {
            return _staffReposity.Get().Include(staff => staff.Account);
        }

        public async Task<Staff> GetById(int id)
        {
            return await _staffReposity.GetById(id);
        }

        public async Task Update(Staff staff)
        {
            await _staffReposity.Update(staff);
        }

        public async Task<Staff> GetByAccountId(int accountId)
        {
            var staffs = await _staffReposity.GetAll();
            return staffs.FirstOrDefault(staff => staff.AccountId == accountId);
        }


        public IEnumerable<Staff> GetAllByCenterId(int centerid)
        {
            return _staffReposity.GetAllByCenterId(centerid);
        }

        public Staff GetStaffByAccountId(int id)
        {
            return _staffReposity.GetStaffByAccountId(id);
        }

        public Staff GetStaffByCenterId(int centerid)
        {
            return _staffReposity.GetStaffByCenterId(centerid);
        }
        
    }
}
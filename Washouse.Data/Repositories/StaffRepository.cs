﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public class StaffRepository : RepositoryBase<Staff>, IStaffReposity
    {
        public StaffRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }

        public async Task ActivateStaff(int id)
        {
            try
            {

                var staff = this.DbContext.Staffs.SingleOrDefault(c => c.Id.Equals(id));
                DbContext.Staffs.Attach(staff);
                staff.Status = true;
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeactivateStaff(int id)
        {
            try
            {

                var staff = this.DbContext.Staffs.SingleOrDefault(c => c.Id.Equals(id));
                DbContext.Staffs.Attach(staff);
                staff.Status = false;
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public IEnumerable<Staff> GetAllByCenterId(int centerid)
        {
            var data = this._dbContext.Staffs
                        .Where(s => s.CenterId == centerid)
                        .ToList();
            return data;
        }

        public async Task<Staff> GetStaffByAccountId(int id)
        {
            var data = await this._dbContext.Staffs
                        .SingleOrDefaultAsync(s => s.AccountId == id);
                        
            return data;
        }

        public Staff GetStaffByCenterId(int centerid)
        {
            var data = this._dbContext.Staffs
                        .Include(staff => staff.Account)
                        .SingleOrDefault(s => s.CenterId == centerid && s.IsManager == true);

            return data;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.ResponseModels.AdminResponseModel;
using Washouse.Model.ResponseModels.ManagerResponseModel;

namespace Washouse.Data.Repositories
{
    public interface IStatisticRepository
    {
        Task<AdminStatisticResponseModel> GetAdminStatistic(string fromDate, string toDate);
        Task<StaffStatisticModel> GetManagerStatistic(int centerId, string fromDate, string toDate);
    }
}

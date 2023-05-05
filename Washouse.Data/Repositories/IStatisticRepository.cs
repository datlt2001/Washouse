using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.ResponseModels.AdminResponseModel;

namespace Washouse.Data.Repositories
{
    public interface IStatisticRepository
    {
        Task<AdminStatisticResponseModel> GetAdminStatistic(string fromDate, string toDate);
    }
}

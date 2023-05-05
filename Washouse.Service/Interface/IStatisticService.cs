using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.ResponseModels.AdminResponseModel;

namespace Washouse.Service.Interface
{
    public interface IStatisticService
    {
        Task<AdminStatisticResponseModel> GetAdminStatistic(string fromDate, string toDate);
    }
}

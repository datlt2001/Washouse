using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Data.Repositories;
using Washouse.Model.ResponseModels.AdminResponseModel;
using Washouse.Model.ResponseModels.ManagerResponseModel;
using Washouse.Service.Interface;

namespace Washouse.Service.Implement
{
    public class StatisticService : IStatisticService
    {
        public IStatisticRepository _statisticRepository;
        private IUnitOfWork _unitOfWork;

        public StatisticService(IStatisticRepository statisticRepository, IUnitOfWork unitOfWork)
        {
            _statisticRepository = statisticRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<AdminStatisticResponseModel> GetAdminStatistic(string fromDate, string toDate)
        {
            return await _statisticRepository.GetAdminStatistic(fromDate, toDate);
        }

        public async Task<StaffStatisticModel> GetManagerStatistic(int centerId, string fromDate, string toDate)
        {
            return await _statisticRepository.GetManagerStatistic(centerId, fromDate, toDate);
        }
    }
}

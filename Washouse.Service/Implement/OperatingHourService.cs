using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Data.Repositories;
using Washouse.Model.Models;
using Washouse.Service.Interface;

namespace Washouse.Service.Implement
{
    public class OperatingHourService : IOperatingHourService
    {
        IOperatingHourRepository _operatingHourRepository;
        IUnitOfWork _unitOfWork;

        public OperatingHourService(IOperatingHourRepository operatingHourRepository, IUnitOfWork unitOfWork)
        {
            this._operatingHourRepository = operatingHourRepository;
            this._unitOfWork = unitOfWork;
        }
        public async Task Add(OperatingHour operatingHour)
        {
            await _operatingHourRepository.Add(operatingHour);
        }
    }
}

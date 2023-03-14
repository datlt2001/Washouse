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
    public class DistrictService : IDistrictService
    {
        IDistrictRepository _districtRepository;
        IUnitOfWork _unitOfWork;

        public DistrictService(IDistrictRepository districtRepository, IUnitOfWork unitOfWork)
        {
            this._districtRepository = districtRepository;
            this._unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<District>> GetAll()
        {
            return await _districtRepository.GetAll();
        }
    }
}

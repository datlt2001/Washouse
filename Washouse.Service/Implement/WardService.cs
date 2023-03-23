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
    public class WardService : IWardService
    {
        IWardRepository _wardRepository;
        IUnitOfWork _unitOfWork;

        public WardService(IWardRepository wardRepository, IUnitOfWork unitOfWork)
        {
            this._wardRepository = wardRepository;
            this._unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Ward>> GetWardListByDistrictId(int DistrictId)
        {
            return await _wardRepository.GetWardListByDistrictId(DistrictId);
        }
        public async Task<Ward> GetWardById(int id)
        {
            return await _wardRepository.GetWardById(id);
        }

        public async Task<Ward> GetWardByName(string name)
        {
            return await _wardRepository.GetWardByName(name);
        }

    }
}

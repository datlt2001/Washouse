using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Data.Repositories;
using Washouse.Model.Models;

namespace Washouse.Service
{
    public class CenterService : ICenterService
    {
        ICenterRepository _centerRepository;
        IUnitOfWork _unitOfWork;

        public CenterService(ICenterRepository centerRepository, IUnitOfWork unitOfWork)
        {
            this._centerRepository = centerRepository;
            this._unitOfWork = unitOfWork;
        }

        public IEnumerable<Center> GetAll()
        {
            return _centerRepository.Get();
        }

    }
}

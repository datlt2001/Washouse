using Microsoft.CodeAnalysis;
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
    public class LocationService : ILocationService
    {
        ILocationRepository _locationRepository;
        IUnitOfWork _unitOfWork;

        public LocationService(ILocationRepository locationRepository, IUnitOfWork unitOfWork)
        {
            this._locationRepository = locationRepository;
            this._unitOfWork = unitOfWork;
        }

        public async Task Add(Model.Models.Location location)
        {
            await _locationRepository.Add(location);
        }

        public async Task<Model.Models.Location> GetById(int id)
        {
            return await _locationRepository.GetById(id);
        }

        public async Task<Model.Models.Location> GetLocationOfACenter(int centerId)
        {
            return await _locationRepository.GetLocationOfACenter(centerId);
        }

        public async Task Update(Model.Models.Location location)
        {
            await _locationRepository.Update(location);
        }
    }
}

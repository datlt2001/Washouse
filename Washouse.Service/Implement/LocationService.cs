using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
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

        public async Task<Model.Models.Location> Add(Model.Models.Location location)
        {
            if (location.Longitude != null && location.Latitude != null) 
            {
                var locationSearch = await _locationRepository.GetBySearch(location);
                if (locationSearch != null) { return  locationSearch; }
            }
           /* var locations = await _locationRepository.GetAll();
            foreach (var item in locations.ToList())
            {
                if (item.Latitude != null && item.Longitude != null && location.Latitude != null && location.Longitude != null) 
                {
                    if (item.WardId == location.WardId
                        && (item.AddressString.ToLower().Contains(location.AddressString.ToLower()) || location.AddressString.ToLower().Contains(item.AddressString.ToLower()))
                        && ((item.Latitude - location.Latitude) < (decimal)0.05)
                        && ((item.Longitude - location.Longitude) < (decimal)0.05)
                        )
                    {
                        return item;
                    }
                }
            }*/
            await _locationRepository.Add(location);
            return location;

        }

        public async Task<Model.Models.Location> GetById(int id)
        {
            return await _locationRepository.GetById(id);
        }
        
        public async Task<Model.Models.Location> GetByIdCheckExistCenter(int id)
        {
            return await _locationRepository.GetByIdCheckExistCenter(id);
        }

        public async Task<Model.Models.Location> GetByIdIncludeWardDistrict(int id)
        {
            return await _locationRepository.GetByIdIncludeWardDistrict(id);
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

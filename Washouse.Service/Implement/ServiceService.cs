using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Data.Repositories;
using Washouse.Model.Models;
using Washouse.Service.Interface;

namespace Washouse.Service.Implement
{
    public class ServiceService : IServiceService
    {
        IServiceRepository _serviceRepository;
        IServicePriceRepository _servicePriceRepository;
        IServiceGalleryRepository _serviceGalleryRepository;
        IUnitOfWork _unitOfWork;

        public ServiceService(IServiceRepository serviceRepository, IUnitOfWork unitOfWork,
            IServicePriceRepository servicePriceRepository, IServiceGalleryRepository serviceGalleryRepository)
        {
            this._serviceRepository = serviceRepository;
            this._unitOfWork = unitOfWork;
            this._servicePriceRepository = servicePriceRepository;
            this._serviceGalleryRepository = serviceGalleryRepository;
        }

        public async Task<IEnumerable<Model.Models.Service>> GetAllByCenterId(int centerId)
        {
            return await _serviceRepository.GetAllByCenterId(centerId);
        }

        public async Task Add(Model.Models.Service service)
        {
            await _serviceRepository.Add(service);
        }

        public async Task<IEnumerable<Model.Models.Service>> GetAll()
        {
            return await _serviceRepository.GetAll();
        }

        public IEnumerable<Model.Models.Service> GetAllByCategoryPaging(int categoryId, int page, int pageSize,
            out int totalRow)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Model.Models.Service> GetAllByTagPaging(string tag, int page, int pageSize, out int totalRow)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Model.Models.Service> GetAllPaging(int page, int pageSize, out int totalRow)
        {
            throw new NotImplementedException();
        }

        public async Task<Model.Models.Service> GetById(int id)
        {
            return await _serviceRepository.GetById(id);
        }

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }

        public async Task Update(Model.Models.Service service)
        {
            await _serviceRepository.Update(service);
        }

        public async Task DeactivateService(int id)
        {
            await _serviceRepository.DeactivateService(id);
        }

        public async Task<Model.Models.Service> Create(Model.Models.Service service, List<ServicePrice> servicePrices,
            List<ServiceGallery> serviceGalleries)
        {
            try
            {
                await _serviceRepository.Add(service);
                _unitOfWork.Commit();

                foreach (var servicePrice in servicePrices)
                {
                    servicePrice.ServiceId = service.Id;
                    await _servicePriceRepository.Add(servicePrice);
                }

                foreach (var serviceGallery in serviceGalleries)
                {
                    serviceGallery.ServiceId = service.Id;
                    await _serviceGalleryRepository.Add(serviceGallery);
                }

                return service;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IEnumerable<Model.Models.Service> GetServicesByCategory(int cateID)
        {
            return _serviceRepository.GetServicesByCategory(cateID);
        }
    }
}
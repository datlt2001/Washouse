using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Data.Repositories;
using Washouse.Model.Models;
using Washouse.Service.Interface;

namespace Washouse.Service.Implement
{
    public class CenterService : ICenterService
    {
        ICenterRepository _centerRepository;
        ICenterRequestRepository _centerRequestRepository;
        IDeliveryPriceChartRepository _deliveryPriceChartRepository;
        IUnitOfWork _unitOfWork;

        public CenterService(ICenterRepository centerRepository, IUnitOfWork unitOfWork,
            ICenterRequestRepository centerRequestRepository, IDeliveryPriceChartRepository deliveryPriceChartRepository)
        {
            this._centerRepository = centerRepository;
            this._unitOfWork = unitOfWork;
            _centerRequestRepository = centerRequestRepository;
            _deliveryPriceChartRepository = deliveryPriceChartRepository;
        }

        public async Task<IEnumerable<Center>> GetAll()
        {
            return await _centerRepository.GetAll();
        }

        public IEnumerable<Center> GetAllByCategoryPaging(int categoryId, int page, int pageSize, out int totalRow)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Center> GetAllBySearchKeyPaging(string searchKey, int page, int pageSize, out int totalRow)
        {
            var query = _centerRepository.GetMulti(x =>
                (x.Status == "Active") && (x.CenterName.Contains(searchKey) || x.Alias.Contains(searchKey)));
            totalRow = query.Count();

            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        public IEnumerable<Center> GetAllByTagPaging(string tag, int page, int pageSize, out int totalRow)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Center> GetAllPaging(int page, int pageSize, out int totalRow)
        {
            throw new NotImplementedException();
        }

        public async Task<Center> GetById(int id)
        {
            return await _centerRepository.GetById(id);
        }

        public async Task<Center> GetMyCenter(int id)
        {
            return await _centerRepository.GetMyCenter(id);
        }

        public async Task<Center> GetByIdLightWeight(int id)
        {
            return await _centerRepository.GetByIdLightWeight(id);
        }

        public async Task<Center> GetByIdIncludeAddress(int id)
        {
            return await _centerRepository.GetByIdIncludeAddress(id);
        }

        public async Task<Center> GetDetailByIdLightWeight(int id)
        {
            return await _centerRepository.GetDetailByIdLightWeight(id);
        }
        
        public async Task<Center> GetCenterOperatingTimes(int id)
        {
            return await _centerRepository.GetCenterOperatingTimes(id);
        }
        
        public async Task<Center> GetByIdToCreateOrder(int id)
        {
            return await _centerRepository.GetByIdToCreateOrder(id);
        }

        public async Task<Center> GetByIdAdminDetail(int id)
        {
            return await _centerRepository.GetByIdAdminDetail(id);
        }

        public async Task<Center> GetByIdToCalculateDeliveryPrice(int id)
        {
            return await _centerRepository.GetByIdToCalculateDeliveryPrice(id);
        }

        public async Task Add(Center center, List<DeliveryPriceChart> deliveryPriceCharts)
        {
            await _centerRepository.Add(center);
            _unitOfWork.Commit();

            foreach (var item in deliveryPriceCharts)
            {
                item.CenterId = center.Id;
                await _deliveryPriceChartRepository.Add(item);
            }
        }

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }

        public async Task Update(Center center)
        {
            await _centerRepository.Update(center);
        }

        public async Task ActivateCenter(int id)
        {
            await _centerRepository.ActivateCenter(id);
        }

        public async Task DeactivateCenter(int id)
        {
            await _centerRepository.DeactivateCenter(id);
        }
    }
}
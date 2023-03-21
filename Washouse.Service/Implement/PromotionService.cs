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
    public class PromotionService : IPromotionService
    {
        private IPromotionRepository _promotionRepository;
        public IUnitOfWork unitOfWork;

        public PromotionService(IPromotionRepository promotionRepository, IUnitOfWork unitOfWork)
        {
            _promotionRepository = promotionRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task Add(Promotion promotion)
        {
            await _promotionRepository.Add(promotion);
        }

        public async Task Update(Promotion promotion)
        {
            await _promotionRepository.Update(promotion);
        }

        public IEnumerable<Promotion> GetAll()
        {
            return _promotionRepository.Get();
        }

        public async Task<Promotion> GetById(int id)
        {
            return await _promotionRepository.GetById(id);
        }
    }
}

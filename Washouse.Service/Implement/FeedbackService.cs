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
    public class FeedbackService : IFeedbackService
    {
        public IFeedbackRepository _feedbackRepository;
        private IUnitOfWork _unitOfWork;

        public FeedbackService(IFeedbackRepository feedbackRepository, IUnitOfWork unitOfWork)
        {
            _feedbackRepository = feedbackRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Add(Feedback feedback)
        {
            await _feedbackRepository.Add(feedback);
        }

        public IEnumerable<Feedback> GetAll()
        {
            return _feedbackRepository.Get();
        }

        public async Task<Feedback> GetById(int id)
        {
            return await _feedbackRepository.GetById(id);
        }

        public async Task Update(Feedback feedback)
        {
            await _feedbackRepository.Update(feedback);
        }

        public IEnumerable<int> GetIDList()
        {
            return _feedbackRepository.GetIDList();
        }
    }
}

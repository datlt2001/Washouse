﻿using System.Collections.Generic;
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

        public IEnumerable<Feedback> GetAllByCenterId(int id)
        {
            return _feedbackRepository.GetAllByCenterId(id);
        }

        public IEnumerable<Feedback> GetAllByOrderId(string orderId)
        {
            return _feedbackRepository.GetAllByOrderId(orderId);
        }

        public async Task<Feedback> GetByOrderId(string orderId)
        {
            return await _feedbackRepository.GetByOrderId(orderId);
        }

        public IEnumerable<Feedback> GetAllByServiceId(int serviceId)
        {
            return _feedbackRepository.GetAllByServiceId(serviceId);
        }

        public async Task<IEnumerable<Feedback>> GetAllByServiceIdLW(int serviceId)
        {
            return await _feedbackRepository.GetAllByServiceIdLW(serviceId);
        }

        public async Task<IEnumerable<Feedback>> GetMyFeedback(string Email)
        {
            return await _feedbackRepository.GetMyFeedback(Email);
        }

        public async Task<IEnumerable<Feedback>> GetAllByCenterIdAsync(int centerId)
        {
            return await _feedbackRepository.GetAllByCenterIdAsync(centerId);
        }
    }
}
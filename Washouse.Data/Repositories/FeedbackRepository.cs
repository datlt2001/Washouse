using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

//using System.Data.Entity;

namespace Washouse.Data.Repositories
{
    public class FeedbackRepository : RepositoryBase<Feedback>, IFeedbackRepository
    {
        public FeedbackRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }

        public IEnumerable<int> GetIDList()
        {
            var query = from fb in DbContext.Feedbacks
                select fb.Id;
            return query.ToList();
        }

        public IEnumerable<Feedback> GetAllByCenterId(int centerid)
        {
            var data = this._dbContext.Feedbacks
                .Include(feedback => feedback.Service)
                .Include(feedback => feedback.Center)
                .Where(o => o.CenterId == centerid && !string.IsNullOrEmpty(o.OrderId))
                .ToList();
            return data;
        }

        public async Task<IEnumerable<Feedback>> GetAllByCenterIdAsync(int centerId)
        {
            var data = _dbContext.Feedbacks
                .Where(o => o.CenterId == centerId && !string.IsNullOrEmpty(o.OrderId));
            return data;
        }


        public IEnumerable<Feedback> GetAllByOrderId(string orderId)
        {
            var data = this._dbContext.Feedbacks
                .Where(fb => fb.OrderId.Trim().ToLower().Equals(orderId.ToLower().Trim()))
                .ToList();
            return data;
        }

        public async Task<Feedback> GetByOrderId(string orderId)
        {
            return await _dbContext.Feedbacks
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public IEnumerable<Feedback> GetAllByServiceId(int serviceId)
        {
            var feedbackList = this._dbContext.Feedbacks
                .Include(feedback => feedback.Service)
                .Include(feedback => feedback.Center)
                .Where(o => o.ServiceId == serviceId)
                .ToList();
            return feedbackList;
        }

        public async Task<IEnumerable<Feedback>> GetAllByServiceIdLW(int serviceId)
        {
            var feedbackList = await _dbContext.Feedbacks
                .Where(o => o.ServiceId == serviceId)
                .ToListAsync();
            return feedbackList;
        }

        public async Task<IEnumerable<Feedback>> GetMyFeedback(string Email)
        {
            var data = this._dbContext.Feedbacks
                .Include(feedback => feedback.Service)
                .Include(feedback => feedback.Center)
                .Where(feedback => feedback.CreatedBy.ToLower().Trim().Equals(Email))
                .ToList();
            return data;
        }
    }
}
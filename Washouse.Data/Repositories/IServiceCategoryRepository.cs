using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public interface IServiceCategoryRepository : IRepository<Category>
    {
        public Task ActivateCategory(int id);

        public Task DeactivateCategory(int id);

        Task PinCategory(int id);
        Task UnPinCategory(int id);
    }
}

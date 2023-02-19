using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface ICenterService
    {
        Task<IEnumerable<Center>> GetAll();
        Task Add(Center center);

        Task Update(Center center);

        //Task Delete(int id);

        IEnumerable<Center> GetAllPaging(int page, int pageSize, out int totalRow);

        IEnumerable<Center> GetAllByCategoryPaging(int categoryId, int page, int pageSize, out int totalRow);

        Task<Center> GetById(int id);

        IEnumerable<Center> GetAllByTagPaging(string tag, int page, int pageSize, out int totalRow);

        void SaveChanges();
    }
}

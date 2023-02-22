using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface IServiceService
    {
        Task<IEnumerable<Model.Models.Service>> GetAll();
        Task Add(Model.Models.Service center);

        Task Update(Model.Models.Service center);

        //Task Delete(int id);

        IEnumerable<Model.Models.Service> GetAllPaging(int page, int pageSize, out int totalRow);

        IEnumerable<Model.Models.Service> GetAllByCategoryPaging(int categoryId, int page, int pageSize, out int totalRow);

        Task<Model.Models.Service> GetById(int id);

        IEnumerable<Model.Models.Service> GetAllByTagPaging(string tag, int page, int pageSize, out int totalRow);

        void SaveChanges();
    }
}

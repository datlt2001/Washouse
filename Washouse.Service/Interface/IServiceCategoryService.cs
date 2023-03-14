using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface IServiceCategoryService
    {
        public Task Add(Category Category);

        public Task Update(Category Category);

        //Category Delete(int id);

        IEnumerable<Category> GetAll();

        IEnumerable<Category> GetAllParentCategory();

        //IEnumerable<Category> GetAll(string keyword);

        //IEnumerable<Category> GetAllByParentId(int parentId);

        public Task<Category> GetById(int id);

        IEnumerable<Category> GetCategoryByParentId(int id);

        public Task DeactivateCategory(int id);
        public Task ActivateCategory(int id);

        //void Save();
    }
}

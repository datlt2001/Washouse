using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface IPostService
    {
        public Task Add(Post post);

        public Task Update(Post post);

        IEnumerable<Post> GetAll();

        public Task<Post> GetById(int id);
        public Task DeactivatePost(int id);
        public Task ActivatePost(int id);
    }
}

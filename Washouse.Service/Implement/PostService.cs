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
    public class PostService : IPostService
    {
        private IPostRepository _postRepository;
        public IUnitOfWork unitOfWork;

        public PostService(IPostRepository postRepository, IUnitOfWork unitOfWork)
        {
            _postRepository = postRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task Add(Post post)
        {
            await _postRepository.Add(post);
        }

        public async Task Update(Post post)
        {
            await _postRepository.Update(post);
        }

        public IEnumerable<Post> GetAll()
        {
            return _postRepository.Get();
        }

        public async Task<Post> GetById(int id)
        {
            return await _postRepository.GetById(id);
        }

        public async Task DeactivatePost(int id)
        {
            await _postRepository.DeactivatePost(id);
        }

        public async Task ActivatePost(int id)
        {
            await _postRepository.ActivatePost(id);
        }
    }
}

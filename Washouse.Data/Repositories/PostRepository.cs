using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public class PostRepository : RepositoryBase<Post>, IPostRepository
    {
        public PostRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }

        public async Task DeactivatePost(int id)
        {
            try
            {

                var post = this.DbContext.Posts.SingleOrDefault(c => c.Id.Equals(id));
                DbContext.Posts.Attach(post);
                post.Status = "false";
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task ActivatePost(int id)
        {
            try
            {

                var post = this.DbContext.Posts.SingleOrDefault(c => c.Id.Equals(id));
                DbContext.Posts.Attach(post);
                post.Status = "true";
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }

}

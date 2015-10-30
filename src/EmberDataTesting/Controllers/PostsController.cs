using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using EmberDataTesting.Models;

namespace EmberDataTesting.Controllers
{
    public class PostsController : ApiController
    {
        private readonly List<Post> _posts;

        public PostsController()
        {
            var user = new User {Id = 2, FirstName = "Yehuda", LastName = "Katz", Handles = new List<Handle> { new GithubHandle { Id = 1, Username = "wykatz"} } };
            _posts = new List<Post>
            {
                new Post
                {
                    Id = 1,
                    Title = "Ember.js rocks",
                    Author = user
                },
                new Post
                {
                    Id = 2,
                    Title = "Tomster rules",
                    Author = user,
                    Comments = new List<Comment>
                    {
                        new Comment { Id = 4, Text = "This is the first comment" },
                        new Comment { Id = 5, Text = "This is the second comment" }
                    }
                }
            };
        }

        // Can return action results
        public IHttpActionResult Get()
        {
            return Ok(_posts);
        }

        // Or domain objects
        public Post Get(int id)
        {
            return _posts.FirstOrDefault(p => p.Id == id);
        }

        public IHttpActionResult Post([FromBody]Post value)
        {
            if(value.Id != 0) return BadRequest();
            return Ok();
        }

        public IHttpActionResult Put(int id, [FromBody]Post value)
        {
            if (id == 0) return BadRequest();
            return Ok();
        }

        public void Delete(int id)
        {
        }

    }
}

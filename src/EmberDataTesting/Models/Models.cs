using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EmberDataTesting.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        //public ICollection<Post> Posts { get; set; }
        public ICollection<Handle> Handles { get; set; }
        public Company Company { get; set; }
    }

    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public User Author { get; set; }

        public ICollection<Comment> Comments { get; set; }
    }

    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; }
        //public Post Post { get; set; }
    }

    public class Handle
    {
        public int Id { get; set; }
        //public User User { get; set; }
    }

    public class GithubHandle : Handle
    {
        public string Username { get; set; }
    }

    public class TwitterHandle : Handle
    {
        public string Nickname { get; set; }
    }

    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //public ICollection<User> Employees { get; set; } 
    }

    public class DevelopmentShop : Company
    {
        public bool Coffee { get; set; }
    }

    public class DesignStudio : Company
    {
        public int Hipsters { get; set; }
    }

}
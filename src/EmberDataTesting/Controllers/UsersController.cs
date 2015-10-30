using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using EmberDataTesting.App_Start;
using EmberDataTesting.Models;
using JSONAPI.Documents;
using JSONAPI.Documents.Builders;
using Newtonsoft.Json.Linq;

namespace EmberDataTesting.Controllers
{
    [RoutePrefix("api/users")]
    public class UsersController : ApiController
    {
        private readonly IResourceCollectionDocumentBuilder _resourceCollectionDocumentBuilder;
        private readonly ISingleResourceDocumentBuilder _singleResourceDocumentBuilder;
        private readonly User[] _users;

        public UsersController()
            : this(
                JsonApiServiceLocator.Instance.SingleResourceDocumentBuilder,
                JsonApiServiceLocator.Instance.ResourceCollectionDocumentBuilder)
        {
        }

        public UsersController(ISingleResourceDocumentBuilder singleResourceDocumentBuilder,
            IResourceCollectionDocumentBuilder resourceCollectionDocumentBuilder)
        {
            _singleResourceDocumentBuilder = singleResourceDocumentBuilder;
            _resourceCollectionDocumentBuilder = resourceCollectionDocumentBuilder;
            var tilde = new DevelopmentShop { Coffee = true, Name = "Tilde", Id = 2 };
            _users = new[]
            {
                new User
                {
                    Id = 1,
                    FirstName = "Yehuda",
                    LastName = "Katz",
                    Company = tilde
                },
                new User
                {
                    Id = 2,
                    FirstName = "Tom",
                    LastName = "Dale",
                    Handles = new[] { new TwitterHandle { Id = 1, Nickname = "tomdale" } },
                    Company = tilde
                }
            };
        }

        // Can return JSON API documents and add metadata
        public IResourceCollectionDocument Get()
        {
            var page = _users.Take(10).ToArray();
            return _resourceCollectionDocumentBuilder.BuildDocument(page, metadata: new PagingMetadata(0, 10, page.Length));
        }

        // Can return JSON API documents and set includes
        public ISingleResourceDocument Get(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            return _singleResourceDocumentBuilder.BuildDocument(user, includePathExpressions: new[] { "*" });
        }

        public IHttpActionResult Post([FromBody] User value)
        {
            return Ok();
        }

        public void Put(int id, [FromBody] string value)
        {
            Ok();
        }

        public void Delete(int id)
        {
            Ok();
        }

        public void Patch(int id, [FromBody] User value)
        {
            Ok();
        }

        public class PagingMetadata : IMetadata
        {
            /// <summary>
            /// Creates a new ExceptionErrorMetadata
            /// </summary>
            /// <param name="exception"></param>
            public PagingMetadata(int offset, int limit, int total)
            {
                MetaObject = new JObject();
                MetaObject["offset"] = offset;
                MetaObject["limit"] = limit;
                MetaObject["total"] = total;
            }

            public JObject MetaObject { get; }
        }
    }
}

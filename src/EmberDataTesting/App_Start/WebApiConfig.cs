using System.Configuration;
using System.Web.Http;
using EmberDataTesting.App_Start;
using EmberDataTesting.Models;

namespace EmberDataTesting
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            JsonApiServiceLocator.Instance.AddPluralization("Company", "Companies");

            //JsonApiServiceLocator.Instance.RegisterType(typeof(Post));
            //JsonApiServiceLocator.Instance.RegisterType(typeof(User));
            //JsonApiServiceLocator.Instance.RegisterType(typeof(Comment));
            //JsonApiServiceLocator.Instance.RegisterType(typeof(Handle));
            //JsonApiServiceLocator.Instance.RegisterType(typeof(GithubHandle));
            //JsonApiServiceLocator.Instance.RegisterType(typeof(TwitterHandle));
            //JsonApiServiceLocator.Instance.RegisterType(typeof(Company));
            //JsonApiServiceLocator.Instance.RegisterType(typeof(DevelopmentShop));

            // JSON API formatter should get first chance to render
            JsonApiServiceLocator.Instance.JsonApiFormatter.Indent = true;
            config.Formatters.Add( JsonApiServiceLocator.Instance.JsonApiFormatter );
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
        }
    }
}

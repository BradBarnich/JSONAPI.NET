using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using JSONAPI.Core;
using JSONAPI.Documents;
using JSONAPI.Documents.Builders;
using JSONAPI.Http;

namespace JSONAPI.Json
{
    public class RegistryDrivenJsonApiFormatter : MediaTypeFormatter
    {
        private readonly JsonApiFormatter _jsonApiFormatter;
        private readonly DefaultLinkConventions _linkConventions;
        private readonly ResourceTypeRegistry _resourceTypeRegistry;

        public RegistryDrivenJsonApiFormatter(JsonApiFormatter jsonApiFormatter, ResourceTypeRegistry resourceTypeRegistry, DefaultLinkConventions linkConventions)
        {
            _jsonApiFormatter = jsonApiFormatter;
            _resourceTypeRegistry = resourceTypeRegistry;
            _linkConventions = linkConventions;

            SupportedMediaTypes.Clear();
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.api+json"));
        }

        public bool Indent
        {
            get { return _jsonApiFormatter.Indent; }
            set { _jsonApiFormatter.Indent = value; }
        }

        public override bool CanReadType(Type type)
        {
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override async Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            var value = (ISingleResourceDocument)await _jsonApiFormatter.ReadFromStreamAsync(typeof(ISingleResourceDocument), readStream, content, formatterLogger);
            var materializer = new RegistryDrivenObjectMaterializer(_resourceTypeRegistry);
            var obj = materializer.MaterializeResourceObject(value.PrimaryData);
            return obj;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger,
            CancellationToken cancellationToken)
        {
            return ReadFromStreamAsync(type, readStream, content, formatterLogger);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
            TransportContext transportContext)
        {
            if (value is ISingleResourceDocument || value is IResourceCollectionDocument || value is IErrorDocument || value is HttpError)
            {
                return _jsonApiFormatter.WriteToStreamAsync(type, value, writeStream, content, transportContext);
            }

            Type singularType;
            if (IsCollectionType(type, out singularType))
            {
                var collectionBuilder = new RegistryDrivenResourceCollectionDocumentBuilder(_resourceTypeRegistry, _linkConventions);
                try
                {
                    var collectionDocument = typeof(RegistryDrivenResourceCollectionDocumentBuilder)
                        .GetMethod("BuildDocument")
                        .MakeGenericMethod(singularType)
                        .Invoke(collectionBuilder, new[] { value, null, new[] { "*" }, null });
                    return _jsonApiFormatter.WriteToStreamAsync(typeof(IResourceCollectionDocument), collectionDocument,
                        writeStream, content, transportContext);
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }

            var singleBuilder = new RegistryDrivenSingleResourceDocumentBuilder(_resourceTypeRegistry, _linkConventions);
            var singleDocument = singleBuilder.BuildDocument(value, null, new[] { "*" }, null);
            return _jsonApiFormatter.WriteToStreamAsync(typeof(ISingleResourceDocument), singleDocument, writeStream, content, transportContext);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
            TransportContext transportContext, CancellationToken cancellationToken)
        {
            return WriteToStreamAsync(type, value, writeStream, content, transportContext);
        }

        private bool IsCollectionType(Type type, out Type singularType)
        {
            singularType = null;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                singularType = type.GetGenericArguments()[0];
                return true;
            }

            foreach (Type intType in type.GetInterfaces())
            {
                if (intType.IsGenericType && intType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    singularType = intType.GetGenericArguments()[0];
                    return true;
                }
            }
            return false;
        }
    }
}
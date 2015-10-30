using System;
using JSONAPI.Core;
using JSONAPI.Documents;
using JSONAPI.Documents.Builders;
using JSONAPI.Json;

namespace EmberDataTesting.App_Start
{
    public class JsonApiServiceLocator
    {
        private JsonApiServiceLocator()
        {
            var metadataFormatter = new MetadataFormatter();
            var linkFormatter = new LinkFormatter(metadataFormatter);
            var resourceLinkageFormatter = new ResourceLinkageFormatter();
            var relationshipObjectFormatter = new RelationshipObjectFormatter(linkFormatter, resourceLinkageFormatter, metadataFormatter);
            var resourceObjectFormatter = new ResourceObjectFormatter(relationshipObjectFormatter, linkFormatter, metadataFormatter);
            var singleResourceDocumentFormatter = new SingleResourceDocumentFormatter(resourceObjectFormatter, metadataFormatter);
            var resourceCollectionDocumentFormatter = new ResourceCollectionDocumentFormatter(resourceObjectFormatter, metadataFormatter);
            var errorFormatter = new ErrorFormatter(linkFormatter, metadataFormatter);
            var errorDocumentFormatter = new ErrorDocumentFormatter(errorFormatter, metadataFormatter);
            var errorDocumentBuilder = new ErrorDocumentBuilder();

            var linkConventions = new DefaultLinkConventions();
            _pluralizationService = new PluralizationService();
            _registrar = new ResourceTypeRegistrar(new DefaultNamingConventions(_pluralizationService));
            _resourceTypeRegistry = new ResourceTypeRegistry(_registrar);

            var jsonApiFormatter = new JsonApiFormatter(singleResourceDocumentFormatter, resourceCollectionDocumentFormatter,
                errorDocumentFormatter, errorDocumentBuilder);

            JsonApiFormatter = new RegistryDrivenJsonApiFormatter(jsonApiFormatter, _resourceTypeRegistry, linkConventions);
            ResourceCollectionDocumentBuilder = new RegistryDrivenResourceCollectionDocumentBuilder(_resourceTypeRegistry, linkConventions);
            SingleResourceDocumentBuilder = new RegistryDrivenSingleResourceDocumentBuilder(_resourceTypeRegistry, linkConventions);
        }

        public RegistryDrivenJsonApiFormatter JsonApiFormatter { get; }

        public IResourceCollectionDocumentBuilder ResourceCollectionDocumentBuilder { get; }

        public ISingleResourceDocumentBuilder SingleResourceDocumentBuilder { get; }

        public void RegisterType(Type type)
        {
            _resourceTypeRegistry.AddRegistration(_registrar.BuildRegistration(type));
        }

        public void AddPluralization(string singular, string plural)
        {
            _pluralizationService.AddMapping(singular, plural);
        }

        public static JsonApiServiceLocator Instance = new JsonApiServiceLocator();
        private readonly ResourceTypeRegistry _resourceTypeRegistry;
        private readonly ResourceTypeRegistrar _registrar;
        private readonly PluralizationService _pluralizationService;
    }
}
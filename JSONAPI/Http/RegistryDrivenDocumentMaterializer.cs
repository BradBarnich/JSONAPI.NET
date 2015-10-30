using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JSONAPI.Documents;
using JSONAPI.Core;
using JSONAPI.Json;

namespace JSONAPI.Http
{
    public class RegistryDrivenObjectMaterializer
    {
        private readonly IResourceTypeRegistry _registry;

        public RegistryDrivenObjectMaterializer(IResourceTypeRegistry registry)
        {
            _registry = registry;
        }

        public object MaterializeResourceObject(IResourceObject resourceObject, CancellationToken cancellationToken = default(CancellationToken))
        {
            var registration = _registry.GetRegistrationForResourceTypeName(resourceObject.Type);

            var relationshipsToInclude = new List<ResourceTypeRelationship>();
            if (resourceObject.Relationships != null)
            {
                relationshipsToInclude.AddRange(
                    resourceObject.Relationships
                        .Select(relationshipObject => registration.GetFieldByName(relationshipObject.Key))
                        .OfType<ResourceTypeRelationship>());
            }


            var material = Activator.CreateInstance(registration.Type);
            if(!string.IsNullOrEmpty(resourceObject.Id))
            {
                SetIdForNewResource(resourceObject.Id, material, registration);
            }

            MergeFieldsIntoProperties(resourceObject, material, registration, cancellationToken);

            return material;
        }

        protected void SetIdForNewResource(string id, object newObject, IResourceTypeRegistration typeRegistration)
        {
            typeRegistration.SetIdForResource(newObject, id);
        }

        protected void MergeFieldsIntoProperties(IResourceObject resourceObject, object material,
            IResourceTypeRegistration registration, CancellationToken cancellationToken)
        {
            foreach (var attributeValue in resourceObject.Attributes)
            {
                var attribute = registration.GetFieldByName(attributeValue.Key) as ResourceTypeAttribute;
                if (attribute == null) continue;
                attribute.SetValue(material, attributeValue.Value);
            }

            foreach (var relationshipValue in resourceObject.Relationships)
            {
                var linkage = relationshipValue.Value.Linkage;

                var typeRelationship = registration.GetFieldByName(relationshipValue.Key) as ResourceTypeRelationship;
                if (typeRelationship == null) continue;

                if (typeRelationship.IsToMany)
                {
                    if (linkage == null)
                        throw new DeserializationException("Missing linkage for to-many relationship",
                            "Expected an array for to-many linkage, but no linkage was specified.", "/data/relationships/" + relationshipValue.Key);

                    if (!linkage.IsToMany)
                        throw new DeserializationException("Invalid linkage for to-many relationship",
                            "Expected an array for to-many linkage.",
                            "/data/relationships/" + relationshipValue.Key + "/data");

                    var newCollection = (IList)Activator.CreateInstance(typeof (List<>).MakeGenericType(typeRelationship.RelatedType));
                    foreach (var resourceIdentifier in linkage.Identifiers)
                    {
                        var relatedObjectRegistration = _registry.GetRegistrationForResourceTypeName(resourceIdentifier.Type);
                        var relatedObject = Activator.CreateInstance(relatedObjectRegistration.Type);
                        SetIdForNewResource(resourceIdentifier.Id, relatedObject, relatedObjectRegistration);
                        newCollection.Add(relatedObject);
                    }

                    typeRelationship.Property.SetValue(material,newCollection);
                }
                else
                {
                    if (linkage == null)
                        throw new DeserializationException("Missing linkage for to-one relationship",
                            "Expected an object for to-one linkage, but no linkage was specified.", "/data/relationships/" + relationshipValue.Key);

                    if (linkage.IsToMany)
                        throw new DeserializationException("Invalid linkage for to-one relationship",
                            "Expected an object or null for to-one linkage",
                            "/data/relationships/" + relationshipValue.Key + "/data");

                    var identifier = linkage.Identifiers.FirstOrDefault();
                    if (identifier == null)
                    {
                        typeRelationship.Property.SetValue(material, null);
                    }
                    else
                    {
                        var relatedObjectRegistration = _registry.GetRegistrationForResourceTypeName(identifier.Type);

                        var relatedObject = Activator.CreateInstance(relatedObjectRegistration.Type);
                        SetIdForNewResource(identifier.Id, relatedObject, relatedObjectRegistration);

                        typeRelationship.Property.SetValue(material, relatedObject);
                    }
                }
            }
        }
    }
}

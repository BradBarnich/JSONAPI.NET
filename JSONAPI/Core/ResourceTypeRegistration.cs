using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JSONAPI.Http;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Core
{
    /// <summary>
    /// Represents a type's registration with a registry
    /// </summary>
    public class ResourceTypeRegistration : IResourceTypeRegistration
    {
        private readonly IReadOnlyDictionary<string, ResourceTypeField> _fields;
        private readonly Func<ParameterExpression, string, BinaryExpression> _filterByIdExpressionFactory;
        private readonly Func<ParameterExpression, Expression> _sortByIdExpressionFactory;
        private readonly ResourceTypeAttribute _idAttribute;

        internal ResourceTypeRegistration(Type type, ResourceTypeAttribute idAttribute, string resourceTypeName,
            IDictionary<string, ResourceTypeField> fields,
            Func<ParameterExpression, string, BinaryExpression> filterByIdExpressionFactory,
            Func<ParameterExpression, Expression> sortByIdExpressionFactory)
        {
            _idAttribute = idAttribute;
            Type = type;
            ResourceTypeName = resourceTypeName;
            _filterByIdExpressionFactory = filterByIdExpressionFactory;
            _sortByIdExpressionFactory = sortByIdExpressionFactory;
            Attributes = fields.Values.OfType<ResourceTypeAttribute>().ToArray();
            Relationships = fields.Values.OfType<ResourceTypeRelationship>().ToArray();
            _fields = new ReadOnlyDictionary<string, ResourceTypeField>(fields);
        }

        public Type Type { get; private set; }

        public PropertyInfo IdProperty { get { return _idAttribute.Property; } }

        public string ResourceTypeName { get; private set; }

        public ResourceTypeAttribute[] Attributes { get; private set; }

        public ResourceTypeRelationship[] Relationships { get; private set; }

        public string GetIdForResource(object resource)
        {
            return IdProperty.GetValue(resource).ToString();
        }

        public void SetIdForResource(object resource, string id)
        {
            _idAttribute.SetValue(resource, new JValue(id));
        }

        public BinaryExpression GetFilterByIdExpression(ParameterExpression parameter, string id)
        {
            return _filterByIdExpressionFactory(parameter, id);
        }

        public Expression GetSortByIdExpression(ParameterExpression parameter)
        {
            return _sortByIdExpressionFactory(parameter);
        }

        public ResourceTypeField GetFieldByName(string name)
        {
            return _fields.ContainsKey(name) ? _fields[name] : null;
        }
    }
}
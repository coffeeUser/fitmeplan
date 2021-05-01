using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Fitmeplan.Api.Core;
using Fitmeplan.Common;
using Fitmeplan.Contracts.Requests;

namespace Fitmeplan.Api
{
    public class CommandSwaggerGenerator : ISwaggerProvider
    {
        private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionsProvider;
        private readonly ISchemaGenerator _schemaGenerator;
        private readonly IEnumerable<IOperationDescription> _handlers;
        private readonly SwaggerGeneratorOptions _options;

        public CommandSwaggerGenerator(
            IApiDescriptionGroupCollectionProvider apiDescriptionsProvider,
            ISchemaGenerator schemaGenerator,
            IOptions<SwaggerGeneratorOptions> optionsAccessor,
            IEnumerable<IOperationDescription> handlers)
            : this(apiDescriptionsProvider, schemaGenerator, optionsAccessor.Value, handlers)
        { }

        public CommandSwaggerGenerator(
            IApiDescriptionGroupCollectionProvider apiDescriptionsProvider,
            ISchemaGenerator schemaGenerator,
            SwaggerGeneratorOptions options,
            IEnumerable<IOperationDescription> handlers)
        {
            _apiDescriptionsProvider = apiDescriptionsProvider;
            _schemaGenerator = schemaGenerator;
            _handlers = handlers;
            _options = options ?? new SwaggerGeneratorOptions();
        }

        public OpenApiDocument GetSwagger(string documentName, string host = null, string basePath = null)
        {
            if (!_options.SwaggerDocs.TryGetValue(documentName, out OpenApiInfo info))
                throw new UnknownSwaggerDocument(documentName, _options.SwaggerDocs.Select(d => d.Key));

            var applicableApiDescriptions = _apiDescriptionsProvider.ApiDescriptionGroups.Items
                .SelectMany(group => group.Items)
                .Where(apiDesc => !(_options.IgnoreObsoleteActions && apiDesc.CustomAttributes().OfType<ObsoleteAttribute>().Any()))
                .Where(apiDesc => _options.DocInclusionPredicate(documentName, apiDesc));

            var schemaRepository = new SchemaRepository();

            var swaggerDoc = new OpenApiDocument
            {
                Info = info,
                Servers = GenerateServers(host, basePath),
                Paths = GeneratePaths(applicableApiDescriptions, schemaRepository),
                Components = new OpenApiComponents
                {
                    Schemas = schemaRepository.Schemas,
                    SecuritySchemes = _options.SecuritySchemes
                },
                SecurityRequirements = _options.SecurityRequirements
            };

            var filterContext = new DocumentFilterContext(applicableApiDescriptions, _schemaGenerator, schemaRepository);
            foreach (var filter in _options.DocumentFilters)
            {
                filter.Apply(swaggerDoc, filterContext);
            }

            return swaggerDoc;
        }

        private IList<OpenApiServer> GenerateServers(string host, string basePath)
        {
            return (host == null && basePath == null)
                ? new List<OpenApiServer>()
                : new List<OpenApiServer> { new OpenApiServer { Url = $"{host}{basePath}" } };
        }

        private OpenApiPaths GeneratePaths(IEnumerable<ApiDescription> apiDescriptions, SchemaRepository schemaRepository)
        {
            var groups = apiDescriptions
                .OrderBy(_options.SortKeySelector)
                .GroupBy(RelativePathSansQueryString);

            var paths = new OpenApiPaths();
            foreach (var group in groups)
            {
                paths.Add($"/{group.Key}",
                    new OpenApiPathItem
                    {
                        Operations = GenerateOperations(group, schemaRepository)
                    });

                if (!group.Key.Contains("file") && !group.Key.Contains("export") && !group.Key.Contains("upload") && !group.Key.Contains("download"))
                {
                    List<IOperationDescription> handlers;
                    if (group.Key.Contains("sbm"))
                    {
                        handlers = _handlers.Where(x => x.IsMobileOperation || x.IsHybridOperation).ToList();
                    }
                    else
                    {
                        handlers = _handlers.Where(x => !x.IsMobileOperation || x.IsHybridOperation).ToList();
                    }

                    foreach (var handler in handlers.OrderBy(x => x.OperationName))
                    {
                        if (handler.OperationType.IsSubclassOf(typeof(QueryRequest)) && group.Key.Contains("query"))
                        {
                            paths.Add("/" + group.Key.Replace("{queryType}", handler.OperationName),
                                new OpenApiPathItem
                                {
                                    Operations = GenerateOperations(group, schemaRepository, handler)
                                });
                        }

                        if (handler.OperationType.IsSubclassOf(typeof(CommandRequest)) && group.Key.Contains("command"))
                        {
                            paths.Add("/" + group.Key.Replace("{commandType}", handler.OperationName),
                                new OpenApiPathItem
                                {
                                    Operations = GenerateOperations(group, schemaRepository, handler)
                                });
                        }
                    }
                }
            };

            return paths;
        }

        private IDictionary<OperationType, OpenApiOperation> GenerateOperations(
            IEnumerable<ApiDescription> apiDescriptions,
            SchemaRepository schemaRepository, 
            IOperationDescription operationDescription = null)
        {
            var apiDescriptionsByMethod = apiDescriptions
                .OrderBy(_options.SortKeySelector)
                .GroupBy(apiDesc => apiDesc.HttpMethod);

            var operations = new Dictionary<OperationType, OpenApiOperation>();

            foreach (var group in apiDescriptionsByMethod)
            {
                var httpMethod = group.Key;

                if (httpMethod == null)
                    throw new NotSupportedException(string.Format(
                        "Ambiguous HTTP method for action - {0}. " +
                        "Actions require an explicit HttpMethod binding for Swagger/OpenAPI 3.0",
                        group.First().ActionDescriptor.DisplayName));

                if (group.Count() > 1 && _options.ConflictingActionsResolver == null)
                    throw new NotSupportedException(string.Format(
                        "Conflicting method/path combination \"{0} {1}\" for actions - {2}. " +
                        "Actions require a unique method/path combination for Swagger/OpenAPI 3.0. Use ConflictingActionsResolver as a workaround",
                        httpMethod,
                        RelativePathSansQueryString(group.First()),
                        string.Join(",", group.Select(apiDesc => apiDesc.ActionDescriptor.DisplayName))));

                var apiDescription = (group.Count() > 1) ? _options.ConflictingActionsResolver(group) : group.Single();

                operations.Add(OperationTypeMap[httpMethod.ToUpper()], GenerateOperation(apiDescription, schemaRepository, operationDescription));
            };

            return operations;
        }

        private OpenApiOperation GenerateOperation(ApiDescription apiDescription, SchemaRepository schemaRepository, IOperationDescription operationDescription = null)
        {
            var operation = new OpenApiOperation
            {
                Tags = GenerateOperationTags(apiDescription),
                OperationId = _options.OperationIdSelector(apiDescription),
                Parameters = GenerateParameters(apiDescription, schemaRepository, operationDescription),
                RequestBody = GenerateRequestBody(apiDescription, schemaRepository, operationDescription),
                Responses = GenerateResponses(apiDescription, schemaRepository),
                Deprecated = apiDescription.CustomAttributes().OfType<ObsoleteAttribute>().Any()
            };

            apiDescription.TryGetMethodInfo(out MethodInfo methodInfo);
            var filterContext = new OperationFilterContext(apiDescription, _schemaGenerator, schemaRepository, methodInfo);
            foreach (var filter in _options.OperationFilters)
            {
                filter.Apply(operation, filterContext);
            }

            return operation;
        }

        private IList<OpenApiTag> GenerateOperationTags(ApiDescription apiDescription)
        {
            return _options.TagsSelector(apiDescription)
                .Select(tagName => new OpenApiTag { Name = tagName })
                .ToList();
        }

        private IList<OpenApiParameter> GenerateParameters(ApiDescription apiDescription, SchemaRepository schemaRepository, IOperationDescription operationDescription)
        {
            var applicableApiParameters = apiDescription.ParameterDescriptions
                .Where(apiParam =>
                {
                    return apiParam.Source.IsFromRequest
                        && (apiParam.ModelMetadata == null || apiParam.ModelMetadata.IsBindingAllowed);
                });

            var parameters = applicableApiParameters
                .Where(x => !operationDescription?.IsOpeationNameParameter(x.Name) ?? true).Select(apiParam => GenerateParameter(apiParam, schemaRepository, operationDescription))
                .ToList();
            return parameters;
        }

        private OpenApiParameter GenerateParameter(
            ApiParameterDescription apiParameter,
            SchemaRepository schemaRepository,
            IOperationDescription operationDescription)
        {
            var name = _options.DescribeAllParametersInCamelCase
                ? apiParameter.Name.ToCamelCase()
                : apiParameter.Name;

            var location = (apiParameter.Source != null && ParameterLocationMap.ContainsKey(apiParameter.Source))
                ? ParameterLocationMap[apiParameter.Source]
                : ParameterLocation.Query;

            var isRequired = IsFromPath(apiParameter)
                || apiParameter.CustomAttributes().Any(attr => RequiredAttributeTypes.Contains(attr.GetType()));

            var schema = (apiParameter.ModelMetadata != null)
                ? _schemaGenerator.GenerateSchema(apiParameter.ModelMetadata.ModelType, schemaRepository)
                : new OpenApiSchema { Type = "string" };

            // If it's NOT a reference schema, apply contextual metadata (i.e. from custom attributes)
            if (schema.Reference == null)
            {
                // Honor default value for optional action parameters
                var parameterInfo = apiParameter.ParameterInfo();
                if (parameterInfo != null && parameterInfo.HasDefaultValue)
                {
                    schema.Default = CreateFor(schema, parameterInfo.DefaultValue);
                }

                schema.ApplyCustomAttributes(apiParameter.CustomAttributes());
            }

            var parameter = new OpenApiParameter
            {
                Name = name,
                In = location,
                Required = isRequired,
                Schema = schema
            };

            var filterContext = new ParameterFilterContext(
                apiParameter,
                _schemaGenerator,
                schemaRepository,
                apiParameter.ParameterInfo(),
                apiParameter.PropertyInfo());

            foreach (var filter in _options.ParameterFilters)
            {
                filter.Apply(parameter, filterContext);
            }

            return parameter;
        }

        private OpenApiRequestBody GenerateRequestBody(
            ApiDescription apiDescription,
            SchemaRepository schemaRepository,
            IOperationDescription operationDescription)
        {
            var bodyParameter = apiDescription.ParameterDescriptions
                .FirstOrDefault(IsFromBody);

            if (bodyParameter != null)
                return GenerateRequestBodyFromBodyParameter(apiDescription, schemaRepository, bodyParameter, operationDescription);

            var formParameters = apiDescription.ParameterDescriptions
                .Where(IsFromForm);

            if (formParameters.Any())
                return GenerateRequestBodyFromFormParameters(apiDescription, schemaRepository, formParameters);

            return null;
        }

        private OpenApiRequestBody GenerateRequestBodyFromBodyParameter(
            ApiDescription apiDescription,
            SchemaRepository schemaRepository,
            ApiParameterDescription bodyParameter,
            IOperationDescription operationDescription)
        {
            var contentTypes = InferRequestContentTypes(apiDescription);

            var isRequired = bodyParameter.CustomAttributes().Any(attr => RequiredAttributeTypes.Contains(attr.GetType()));

            Type type;
            if (!typeof(JContainer).IsAssignableFrom(bodyParameter.Type) || operationDescription == null)
            {
                type = bodyParameter.ModelMetadata.ModelType;
            }
            else 
            {
                type = operationDescription.OperationType;
            }

            return new OpenApiRequestBody
            {
                Content = contentTypes
                    .ToDictionary(
                        contentType => contentType,
                        contentType => new OpenApiMediaType
                        {
                            Schema = _schemaGenerator.GenerateSchema(type, schemaRepository)
                        }
                    ),
                Required = isRequired
            };
        }

        private IEnumerable<string> InferRequestContentTypes(ApiDescription apiDescription)
        {
            // If there's content types explicitly specified via ConsumesAttribute, use them
            var explicitContentTypes = apiDescription.CustomAttributes().OfType<ConsumesAttribute>()
                .SelectMany(attr => attr.ContentTypes)
                .Distinct();
            if (explicitContentTypes.Any()) return explicitContentTypes;

            // If there's content types surfaced by ApiExplorer, use them
            var apiExplorerContentTypes = apiDescription.SupportedRequestFormats
                .Select(format => format.MediaType)
                .Distinct();
            if (apiExplorerContentTypes.Any()) return apiExplorerContentTypes;

            return Enumerable.Empty<string>();
        }

        private OpenApiRequestBody GenerateRequestBodyFromFormParameters(
            ApiDescription apiDescription,
            SchemaRepository schemaRepository,
            IEnumerable<ApiParameterDescription> formParameters)
        {
            var contentTypes = InferRequestContentTypes(apiDescription);
            contentTypes = contentTypes.Any() ? contentTypes : new[] { "multipart/form-data" };

            var schema = GenerateSchemaFromFormParameters(formParameters, schemaRepository);

            return new OpenApiRequestBody
            {
                Content = contentTypes
                    .ToDictionary(
                        contentType => contentType,
                        contentType => new OpenApiMediaType
                        {
                            Schema = schema,
                            Encoding = schema.Properties.ToDictionary(
                                entry => entry.Key,
                                entry => new OpenApiEncoding { Style = ParameterStyle.Form }
                            )
                        }
                    )
            };
        }

        private OpenApiSchema GenerateSchemaFromFormParameters(
            IEnumerable<ApiParameterDescription> formParameters,
            SchemaRepository schemaRepository)
        {
            var properties = new Dictionary<string, OpenApiSchema>();
            var requiredPropertyNames = new List<string>();

            foreach (var formParameter in formParameters)
            {
                var name = _options.DescribeAllParametersInCamelCase
                    ? formParameter.Name.ToCamelCase()
                    : formParameter.Name;

                var schema = (formParameter.ModelMetadata != null)
                    ? _schemaGenerator.GenerateSchema(formParameter.ModelMetadata.ModelType, schemaRepository)
                    : new OpenApiSchema { Type = "string" };

                var defaultValue = formParameter.CustomAttributes().OfType<DefaultValueAttribute>().FirstOrDefault()?.Value
                    ?? formParameter.ParameterInfo()?.DefaultValue;

                // NOTE: Oddly, ParameterInfo.DefaultValue returns DBNull if not optional, hence the additional check below
                if (schema.Reference == null && defaultValue != null && defaultValue != DBNull.Value)
                {
                    schema.Default = CreateFor(schema, defaultValue);
                }

                properties.Add(name, schema);

                if (IsFromPath(formParameter) || formParameter.CustomAttributes().Any(attr => RequiredAttributeTypes.Contains(attr.GetType())))
                    requiredPropertyNames.Add(name);
            }

            return new OpenApiSchema
            {
                Type = "object",
                Properties = properties,
                Required = new SortedSet<string>(requiredPropertyNames)
            };
        }

        private OpenApiResponses GenerateResponses(
            ApiDescription apiDescription,
            SchemaRepository schemaRepository)
        {
            var supportedResponseTypes = apiDescription.SupportedResponseTypes
                .DefaultIfEmpty(new ApiResponseType { StatusCode = 200 });

            var responses = new OpenApiResponses();
            foreach (var responseType in supportedResponseTypes)
            {
                var statusCode = IsDefaultResponse(responseType) ? "default" : responseType.StatusCode.ToString();
                responses.Add(statusCode, GenerateResponse(apiDescription, schemaRepository, statusCode, responseType));
            }
            return responses;
        }

        private OpenApiResponse GenerateResponse(
            ApiDescription apiDescription,
            SchemaRepository schemaRepository,
            string statusCode,
            ApiResponseType apiResponseType)
        {
            var description = ResponseDescriptionMap
                .FirstOrDefault((entry) => Regex.IsMatch(statusCode, entry.Key))
                .Value;

            var responseContentTypes = InferResponseContentTypes(apiDescription, apiResponseType);

            return new OpenApiResponse
            {
                Description = description,
                Content = responseContentTypes.ToDictionary(
                    contentType => contentType,
                    contentType => CreateResponseMediaType(apiResponseType.ModelMetadata, schemaRepository)
                )
            };
        }

        private IEnumerable<string> InferResponseContentTypes(ApiDescription apiDescription, ApiResponseType apiResponseType)
        {
            // If there's no associated model, return an empty list (i.e. no content)
            if (apiResponseType.ModelMetadata == null) return Enumerable.Empty<string>();

            // If there's content types explicitly specified via ProducesAttribute, use them
            var explicitContentTypes = apiDescription.CustomAttributes().OfType<ProducesAttribute>()
                .SelectMany(attr => attr.ContentTypes)
                .Distinct();
            if (explicitContentTypes.Any()) return explicitContentTypes;

            // If there's content types surfaced by ApiExplorer, use them
            var apiExplorerContentTypes = apiResponseType.ApiResponseFormats
                .Select(responseFormat => responseFormat.MediaType)
                .Distinct();
            if (apiExplorerContentTypes.Any()) return apiExplorerContentTypes;

            return Enumerable.Empty<string>();
        }

        private OpenApiMediaType CreateResponseMediaType(ModelMetadata modelMetadata, SchemaRepository schemaRespository)
        {
            return new OpenApiMediaType
            {
                Schema = _schemaGenerator.GenerateSchema(modelMetadata.ModelType, schemaRespository)
            };
        }

        private static readonly Dictionary<string, OperationType> OperationTypeMap = new Dictionary<string, OperationType>
        {
            { "GET", OperationType.Get },
            { "PUT", OperationType.Put },
            { "POST", OperationType.Post },
            { "DELETE", OperationType.Delete },
            { "OPTIONS", OperationType.Options },
            { "HEAD", OperationType.Head },
            { "PATCH", OperationType.Patch },
            { "TRACE", OperationType.Trace }
        };

        private static readonly Dictionary<BindingSource, ParameterLocation> ParameterLocationMap = new Dictionary<BindingSource, ParameterLocation>
        {
            { BindingSource.Query, ParameterLocation.Query },
            { BindingSource.Header, ParameterLocation.Header },
            { BindingSource.Path, ParameterLocation.Path }
        };

        private static readonly IEnumerable<Type> RequiredAttributeTypes = new[]
        {
            typeof(BindRequiredAttribute),
            typeof(RequiredAttribute)
        };

        private static readonly Dictionary<string, string> ResponseDescriptionMap = new Dictionary<string, string>
        {
            { "1\\d{2}", "Information" },
            { "2\\d{2}", "Success" },
            { "304", "Not Modified" },
            { "3\\d{2}", "Redirect" },
            { "400", "Bad Request" },
            { "401", "Unauthorized" },
            { "403", "Forbidden" },
            { "404", "Not Found" },
            { "405", "Method Not Allowed" },
            { "406", "Not Acceptable" },
            { "408", "Request Timeout" },
            { "409", "Conflict" },
            { "4\\d{2}", "Client Error" },
            { "5\\d{2}", "Server Error" },
            { "default", "Error" }
        };

        private IOpenApiAny CreateFor(OpenApiSchema schema, object value)
        {
            if (schema.Type == "boolean" && TryCast(value, out bool boolValue))
                return new OpenApiBoolean(boolValue);

            else if (schema.Type == "integer" && schema.Format == "int32" && TryCast(value, out int intValue))
                return new OpenApiInteger(intValue);

            else if (schema.Type == "integer" && schema.Format == "int64" && TryCast(value, out long longValue))
                return new OpenApiLong(longValue);

            else if (schema.Type == "number" && schema.Format == "float" && TryCast(value, out float floatValue))
                return new OpenApiFloat(floatValue);

            else if (schema.Type == "number" && schema.Format == "double" && TryCast(value, out double doubleValue))
                return new OpenApiDouble(doubleValue);

            else if (schema.Type == "string" && value.GetType().IsEnum)
                return new OpenApiString(Enum.GetName(value.GetType(), value));

            else if (schema.Type == "string" && schema.Format == "date-time" && TryCast(value, out DateTime dateTimeValue))
                return new OpenApiDate(dateTimeValue);

            else if (schema.Type == "string")
                return new OpenApiString(value.ToString());

            return null;
        }

        private static bool TryCast<T>(object value, out T typedValue)
        {
            try
            {
                typedValue = (T)Convert.ChangeType(value, typeof(T));
                return true;
            }
            catch (InvalidCastException)
            {
                typedValue = default(T);
                return false;
            }
        }

        private bool IsFromPath(ApiParameterDescription apiParameter)
        {
            return (apiParameter.Source == BindingSource.Path);
        }

        private bool IsFromBody(ApiParameterDescription apiParameter)
        {
            return (apiParameter.Source == BindingSource.Body);
        }

        private bool IsFromForm(ApiParameterDescription apiParameter)
        {
            var source = apiParameter.Source;
            var elementType = apiParameter.ModelMetadata?.ElementType;

            return (source == BindingSource.Form || source == BindingSource.FormFile)
                   || (elementType != null && typeof(IFormFile).IsAssignableFrom(elementType));
        }

        private string RelativePathSansQueryString(ApiDescription apiDescription)
        {
            return apiDescription.RelativePath?.Split('?').First();
        }

        private bool IsDefaultResponse(ApiResponseType apiResponseType)
        {
            var propertyInfo = apiResponseType.GetType().GetProperty("IsDefaultResponse");
            if (propertyInfo != null)
            {
                return (bool)propertyInfo.GetValue(apiResponseType);
            }

            // ApiExplorer < 2.1.0 does not support default response.
            return false;
        }
    }
}

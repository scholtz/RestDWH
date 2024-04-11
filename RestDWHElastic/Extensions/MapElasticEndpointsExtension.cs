using Microsoft.AspNetCore.Authorization;
using RestDWH.Base.Attributes;
using RestDWH.Base.Model;
using RestDWH.Elastic.Attributes.Endpoints;
using RestDWHElastic.Repository;
using System.Reflection;
using System.Security.Claims;

namespace RestDWH.Base.Extensios
{
    public static class MapElasticEndpointsExtension
    {
        public static void MapElasticEndpoints<T>(this WebApplication app, IElasticDWHRepository<T>? customRepository) where T : class
        {

            var config = typeof(T).GetCustomAttribute<RestDWHEntity>();
            if (config == null) return;
            var attribute = typeof(T).GetCustomAttribute<RestDWHEndpointElasticQuery>();
            if (attribute != null)
            {
                var endpoint = attribute.Path.Replace("{ApiName}", config.ApiName);
                if (!string.IsNullOrEmpty(endpoint))
                {
                    var builder = app.MapPost(endpoint, [Authorize] async (string query, ClaimsPrincipal user, IElasticDWHRepository<T> repository) =>
                    {
                        try
                        {
                            if (customRepository != null) repository = customRepository;
                            var ret = await repository.GetAsync(query, user);
                            return Results.Ok(ret);
                        }
                        catch (UnauthorizedAccessException unauth)
                        {
                            return Results.Unauthorized();
                        }
                    });
                    builder.Produces<DBListBase<T, DBBase<T>>>();
                    builder.ProducesProblem(500);
                    //if (!string.IsNullOrEmpty(config?.Name)) builder.WithGroupName(config.Name);
                    var docs = typeof(T).GetCustomAttribute<RestDWHDocsAttribute>();
                    builder
                        .WithOpenApi(generatedOperation =>
                        {
                            //generatedOperation.Parameters[0].Description = $"The elastic query";
                            return generatedOperation;
                        });
                    if (!string.IsNullOrEmpty(docs?.Summary)) builder.WithSummary($"Query {docs.Name}. {docs.Summary}");
                    if (!string.IsNullOrEmpty(docs?.Description)) builder.WithDescription($"Query {docs.Name} with elastic search json. {docs.Description}");
                    builder.WithName($"{config?.Name}ElasticQuery");
                    builder.WithOpenApi();
                }
            }


            var attributeProperties = typeof(T).GetCustomAttribute<RestDWHEndpointElasticPropertiesQuery>();
            if (attributeProperties != null)
            {
                var endpoint = attributeProperties.Path.Replace("{ApiName}", config.ApiName);
                if (!string.IsNullOrEmpty(endpoint))
                {
                    var builder = app.MapPost(endpoint, [Authorize] async (string query, string attribute, int? offset, int? limit, ClaimsPrincipal user, IElasticDWHRepository<T> repository) =>
                    {
                        try
                        {

                            if (customRepository != null) repository = customRepository;
                            var ret = await repository.GetPropertiesAsync(query, attribute, offset ?? 0, limit ?? 10, user);
                            return Results.Ok(ret);
                        }
                        catch (UnauthorizedAccessException unauth)
                        {
                            return Results.Unauthorized();
                        }
                    });
                    builder.Produces<List<object>?>();
                    builder.ProducesProblem(500);
                    //if (!string.IsNullOrEmpty(config?.Name)) builder.WithGroupName(config.Name);
                    var docs = typeof(T).GetCustomAttribute<RestDWHDocsAttribute>();
                    builder
                        .WithOpenApi(generatedOperation =>
                        {
                            generatedOperation.Parameters[0].Description = $"The elastic query";
                            generatedOperation.Parameters[1].Description = $"Aggregated attribute";
                            generatedOperation.Parameters[2].Description = $"Offset. Default 0";
                            generatedOperation.Parameters[3].Description = $"Limit. Default 10";
                            return generatedOperation;
                        });
                    if (!string.IsNullOrEmpty(docs?.Summary)) builder.WithSummary($"Properties query {docs.Name}. {docs.Summary}");
                    if (!string.IsNullOrEmpty(docs?.Description)) builder.WithDescription($"Properties query {docs.Name} with elastic search json is used to receive aggregated values with low level query json. {docs.Description}");
                    builder.WithName($"{config?.Name}ElasticPropertiesQuery");
                    builder.WithOpenApi();
                }
            }
        }

    }
}

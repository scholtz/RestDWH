using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using RestDWH.Base.Attributes;
using RestDWH.Base.Model;
using RestDWH.Base.Repository;
using System.Reflection;
using System.Security.Claims;

namespace RestDWH.Base.Extensios
{
    public static class MapEndpointsExtension
    {
        public static void MapEndpoints<T>(this Microsoft.AspNetCore.Builder.WebApplication app, IDWHRepository<T>? customRepository) where T : class
        {

            var config = typeof(T).GetCustomAttribute<RestDWHEntity>();
            if (config == null) return;


            var getEndpoint = config?.EndpointGet;
            if (!string.IsNullOrEmpty(getEndpoint))
            {
                var builder = app.MapGet(getEndpoint, [Authorize] async (int? offset, int? limit, string? query, string? sort, ClaimsPrincipal user, IDWHRepository<T> repository) =>
                {
                    try
                    {
                        var offsetVal = offset ?? 0;
                        var limitVal = limit ?? 10;
                        var queryVal = query ?? "*";
                        var sortVal = sort ?? "";
                        if (customRepository != null) repository = customRepository;
                        var ret = await repository.GetAsync(offsetVal, limitVal, queryVal, sortVal, user);
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
                        generatedOperation.Parameters[0].Description = $"The offset. Default is 0.";
                        generatedOperation.Parameters[1].Description = $"The limit. Default is 10.";
                        generatedOperation.Parameters[2].Description = $"The lucane query. Default is *.";
                        generatedOperation.Parameters[3].Description = $"The sort. Default is empty. Example: 'id.keyword asc,updated desc'.";
                        return generatedOperation;
                    });
                if (!string.IsNullOrEmpty(docs?.Summary)) builder.WithSummary($"Filter {docs.Name}. {docs.Summary}");
                if (!string.IsNullOrEmpty(docs?.Description)) builder.WithDescription($"Fetches {docs.Name} from DB by query. {docs.Description}");
                builder.WithName($"{config?.Name}Get");
                builder.WithOpenApi();
            }

            var getEndpointWithFields = config?.EndpointGetWithFields;
            if (!string.IsNullOrEmpty(getEndpointWithFields))
            {
                var builder = app.MapGet(getEndpointWithFields, [Authorize] async (string? fields, int? offset, int? limit, string? query, string? sort, ClaimsPrincipal user, IDWHRepository<T> repository) =>
                {
                    try
                    {
                        var fieldsVal = fields ?? "id";
                        var offsetVal = offset ?? 0;
                        var limitVal = limit ?? 10;
                        var queryVal = query ?? "*";
                        var sortVal = sort ?? "";

                        if (customRepository != null) repository = customRepository;
                        var ret = await repository.GetWithFieldsAsync(fieldsVal, offsetVal, limitVal, queryVal, sortVal, user);
                        return Results.Ok(ret);
                    }
                    catch (UnauthorizedAccessException unauth)
                    {
                        return Results.Unauthorized();
                    }
                });
                builder.Produces<Dictionary<string, object>>();
                builder.ProducesProblem(500);
                var docs = typeof(T).GetCustomAttribute<RestDWHDocsAttribute>();
                builder
                    .WithOpenApi(generatedOperation =>
                    {
                        generatedOperation.Parameters[0].Description = $"The fields. Default is id. Fields can be separated by comma. Fields can be rerefeced with :. Example: id:id2,data.year:year";
                        generatedOperation.Parameters[1].Description = $"The offset. Default is 0.";
                        generatedOperation.Parameters[2].Description = $"The limit. Default is 10.";
                        generatedOperation.Parameters[3].Description = $"The lucane query. Default is *.";
                        generatedOperation.Parameters[4].Description = $"The sort. Default is empty. Example: 'id.keyword asc,updated desc'.";
                        return generatedOperation;
                    });
                if (!string.IsNullOrEmpty(docs?.Summary)) builder.WithSummary($"Filter {docs.Name}. Returns only specified fields. {docs.Summary}");
                if (!string.IsNullOrEmpty(docs?.Description)) builder.WithDescription($"Fetches {docs.Name} from DB by query. Returns only specified fields. {docs.Description}\nFields is string field separated by comma (,). It is possible to rereference the field to new field name by colon (:), for example id:id2");
                builder.WithName($"{config?.Name}GetWithFields");
                builder.WithOpenApi();
            }

            var getByIdEndpoint = config?.EndpointGetById;
            if (!string.IsNullOrEmpty(getByIdEndpoint))
            {
                if (!getByIdEndpoint.Contains("{id}")) getByIdEndpoint = getByIdEndpoint + "/{id}";
                var builder = app.MapGet(getByIdEndpoint, [Authorize] async (string id, ClaimsPrincipal user, IDWHRepository<T> repository) =>
                {
                    try
                    {
                        if (customRepository != null) repository = customRepository;
                        var ret = await repository.GetByIdAsync(id, user);
                        if (ret == null) return Results.NoContent();
                        return Results.Ok(ret);
                    }
                    catch (UnauthorizedAccessException unauth)
                    {
                        return Results.Unauthorized();
                    }
                });
                builder.Produces<DBBase<T>>();
                builder.ProducesProblem(500);
                builder.Produces(204);// null = no content
                var docs = typeof(T).GetCustomAttribute<RestDWHDocsAttribute>();
                builder
                    .WithOpenApi(generatedOperation =>
                    {
                        generatedOperation.Parameters[0].Description = $"The {docs?.Name} ID";
                        return generatedOperation;
                    });
                if (!string.IsNullOrEmpty(docs?.Summary)) builder.WithSummary($"Get {docs.Name} by ID. {docs.Summary}");
                if (!string.IsNullOrEmpty(docs?.Description)) builder.WithDescription($"Fetches {docs.Name} from DB by ID. {docs.Description}");
                builder.WithName($"{config?.Name}GetById");
                builder.WithOpenApi();
            }

            var getPropertiesEndpoint = config?.EndpointProperties;
            if (!string.IsNullOrEmpty(getPropertiesEndpoint))
            {
                if (!getPropertiesEndpoint.Contains("{propertyId}")) getPropertiesEndpoint = getPropertiesEndpoint + "/{propertyId}";
                var builder = app.MapGet(getPropertiesEndpoint, [Authorize] async ([FromRoute] string propertyId, [FromQuery] int? offset, [FromQuery] int? limit, [FromQuery] string? query, [FromQuery] string? sortType, ClaimsPrincipal user, IDWHRepository<T> repository) =>
                {
                    try
                    {
                        if (customRepository != null) repository = customRepository;
                        var offsetVal = offset ?? 0;
                        var limitVal = limit ?? 10;
                        var queryVal = query ?? "*";
                        sortType = sortType ?? "CountDescending";
                        if (sortType != "CountAscending"
                            && sortType != "CountDescending"
                            && sortType != "KeyAscending"
                            && sortType != "KeyDescending"
                            && !sortType.StartsWith("Ascending:")
                            && !sortType.StartsWith("Descending:")) throw new Exception("Allowed options for sortType is CountAscending|CountDescending|KeyAscending|KeyDescending|Ascending:[otherProperty]|Descending:[otherProperty]");
                        var ret = await repository.GetPropertiesAsync(propertyId, offsetVal, limitVal, queryVal, sortType, user);
                        if (ret == null) return Results.NoContent();
                        return Results.Ok(ret);
                    }
                    catch (UnauthorizedAccessException unauth)
                    {
                        return Results.Unauthorized();
                    }
                });
                builder.Produces<List<object>>();
                builder.ProducesProblem(500);
                builder.Produces(204);// null = no content
                var docs = typeof(T).GetCustomAttribute<RestDWHDocsAttribute>();
                builder
                    .WithOpenApi(generatedOperation =>
                    {
                        generatedOperation.Parameters[0].Description = $"The property to get disctinct value from. Example: id";
                        generatedOperation.Parameters[1].Description = $"The offset. Default is 0.";
                        generatedOperation.Parameters[2].Description = $"The limit. Default is 10.";
                        generatedOperation.Parameters[3].Description = $"The lucane query. Default is *.";
                        generatedOperation.Parameters[4].Description = $"The sort type. Allowed options for sortType is CountAscending|CountDescending|KeyAscending|KeyDescending|Ascending:[otherProperty]|Descending:[otherProperty]";
                        return generatedOperation;
                    });
                if (!string.IsNullOrEmpty(docs?.Summary)) builder.WithSummary($"Get distinct values from specific property. {docs.Summary}");
                if (!string.IsNullOrEmpty(docs?.Description)) builder.WithDescription($"Fetches distinct values of specific property from {docs.Name}. To query string fields the '.keyword' may be required, for example id.keyword. {docs.Description}\nAllowed options for sortType is CountAscending|CountDescending|KeyAscending|KeyDescending|Ascending:[otherProperty]|Descending:[otherProperty]");
                builder.WithName($"{config?.Name}GetProperty");
                builder.WithOpenApi();
            }


            var getByIdWithFieldsEndpoint = config?.EndpointGetByIdWithFields;
            if (!string.IsNullOrEmpty(getByIdWithFieldsEndpoint))
            {
                if (!getByIdWithFieldsEndpoint.Contains("{id}")) getByIdWithFieldsEndpoint = getByIdWithFieldsEndpoint + "/{id}";
                var builder = app.MapGet(getByIdWithFieldsEndpoint, [Authorize] async (string id, string fields, ClaimsPrincipal user, IDWHRepository<T> repository) =>
                {
                    try
                    {
                        var fieldsVal = fields ?? "id";
                        if (customRepository != null) repository = customRepository;
                        var ret = await repository.GetByIdWithFieldsAsync(id, fieldsVal, user);
                        return Results.Ok(ret);

                    }
                    catch (UnauthorizedAccessException unauth)
                    {
                        return Results.Unauthorized();
                    }
                });
                builder.Produces<Dictionary<string, object>>();
                builder.ProducesProblem(500);
                var docs = typeof(T).GetCustomAttribute<RestDWHDocsAttribute>();
                builder
                    .WithOpenApi(generatedOperation =>
                    {
                        generatedOperation.Parameters[0].Description = $"The {docs?.Name} ID.";
                        generatedOperation.Parameters[1].Description = $"The fields. Default is id. Fields can be separated by comma. Fields can be rerefeced with :. Example: id:id2,data.year:year";
                        return generatedOperation;
                    });
                if (!string.IsNullOrEmpty(docs?.Summary)) builder.WithSummary($"Get {docs.Name} by ID. Returns only specified fields. {docs.Summary}");
                if (!string.IsNullOrEmpty(docs?.Description)) builder.WithDescription($"Fetches {docs.Name} from DB by ID. Returns only specified fields. {docs.Description}\nFields is string field separated by comma (,). It is possible to rereference the field to new field name by colon (:), for example id:id2");
                builder.WithName($"{config?.Name}GetByIdWithFields");
                builder.WithOpenApi();
            }

            var postEndpoint = config?.EndpointPost;
            if (!string.IsNullOrEmpty(postEndpoint))
            {
                var builder = app.MapPost(postEndpoint, [Authorize] async (T data, ClaimsPrincipal user, IDWHRepository<T> repository) =>
                {
                    try
                    {
                        if (customRepository != null) repository = customRepository;
                        return Results.Ok(await repository.PostAsync(data, user));

                    }
                    catch (UnauthorizedAccessException unauth)
                    {
                        return Results.Unauthorized();
                    }
                });
                builder.Produces<DBBase<T>>();
                builder.ProducesProblem(500);
                var docs = typeof(T).GetCustomAttribute<RestDWHDocsAttribute>();
                if (!string.IsNullOrEmpty(docs?.Summary)) builder.WithSummary($"Create new instance of {docs.Name}. {docs.Summary}");
                if (!string.IsNullOrEmpty(docs?.Description)) builder.WithDescription($"Create new instance of {docs.Name}. {docs.Description}");
                builder.WithName($"{config?.Name}Post");
                builder.WithOpenApi();
            }

            var putEndpoint = config?.EndpointPut;
            if (!string.IsNullOrEmpty(putEndpoint))
            {
                if (!putEndpoint.Contains("{id}")) putEndpoint = putEndpoint + "/{id}";

                var builder = app.MapPut(putEndpoint, [Authorize] async (string id, T data, ClaimsPrincipal user, IDWHRepository<T> repository) =>
                {
                    try
                    {
                        if (customRepository != null) repository = customRepository;
                        return Results.Ok(await repository.PutAsync(id, data, user));
                    }
                    catch (UnauthorizedAccessException unauth)
                    {
                        return Results.Unauthorized();
                    }
                });
                builder.Produces<DBBase<T>>();
                builder.ProducesProblem(500);
                var docs = typeof(T).GetCustomAttribute<RestDWHDocsAttribute>();
                if (!string.IsNullOrEmpty(docs?.Summary)) builder.WithSummary($"Fully updates instance of {docs.Name}. {docs.Summary}");
                if (!string.IsNullOrEmpty(docs?.Description)) builder.WithDescription($"Replace instance of {docs.Name} with input data. {docs.Description}");
                builder.WithName($"{config?.Name}Put");
                builder.WithOpenApi();
            }

            var upsertEndpoint = config?.EndpointUpsert;
            if (!string.IsNullOrEmpty(upsertEndpoint))
            {
                if (!upsertEndpoint.Contains("{id}")) upsertEndpoint = upsertEndpoint + "/{id}";

                var builder = app.MapPut(upsertEndpoint, [Authorize] async (string id, T data, ClaimsPrincipal user, IDWHRepository<T> repository) =>
                {
                    try
                    {
                        if (customRepository != null) repository = customRepository;
                        return Results.Ok(await repository.UpsertAsync(id, data, user));
                    }
                    catch (UnauthorizedAccessException unauth)
                    {
                        return Results.Unauthorized();
                    }
                });
                builder.Produces<DBBase<T>>();
                builder.ProducesProblem(500);
                var docs = typeof(T).GetCustomAttribute<RestDWHDocsAttribute>();
                builder
                    .WithOpenApi(generatedOperation =>
                    {
                        generatedOperation.Parameters[0].Description = $"The {docs?.Name} ID.";
                        return generatedOperation;
                    });
                if (!string.IsNullOrEmpty(docs?.Summary)) builder.WithSummary($"Insert or update instance of {docs.Name}. {docs.Summary}");
                if (!string.IsNullOrEmpty(docs?.Description)) builder.WithDescription($"Insert or update instance of {docs.Name} with input data. {docs.Description}");
                builder.WithName($"{config?.Name}Upsert");
                builder.WithOpenApi();
            }

            var patchEndpoint = config?.EndpointPatch;
            if (!string.IsNullOrEmpty(patchEndpoint))
            {
                if (!patchEndpoint.Contains("{id}")) patchEndpoint = patchEndpoint + "/{id}";

                var builder = app.MapPatch(patchEndpoint, [Authorize] async (string id, [FromBody] List<Microsoft.AspNetCore.JsonPatch.Operations.Operation<T>> data, ClaimsPrincipal user, IDWHRepository<T> repository) =>
                {
                    try
                    {
                        //Console.WriteLine(data.First().value.GetType());
                        if (customRepository != null) repository = customRepository;
                        var doc = new JsonPatchDocument<T>(data.Select(o =>
                        {

                            var serialized = (o.value as System.Text.Json.JsonElement?)?.GetRawText() ?? o.value.ToString() ?? "";
                            var value = Newtonsoft.Json.JsonConvert.DeserializeObject(serialized);
                            return new Microsoft.AspNetCore.JsonPatch.Operations.Operation<T>()
                            {
                                from = o.from,
                                op = o.op,
                                path = o.path,
                                value = value
                            };

                        }).Where(o => o.value != null).ToList(), new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver());
                        return Results.Ok(await repository.PatchAsync(id, doc, user));
                    }
                    catch (UnauthorizedAccessException unauth)
                    {
                        return Results.Unauthorized();
                    }
                });
                builder.Produces<DBBase<T>>();
                builder.ProducesProblem(500);
                var docs = typeof(T).GetCustomAttribute<RestDWHDocsAttribute>();
                builder
                    .WithOpenApi(generatedOperation =>
                    {
                        generatedOperation.Parameters[0].Description = $"The {docs?.Name} ID.";
                        return generatedOperation;
                    });
                if (!string.IsNullOrEmpty(docs?.Summary)) builder.WithSummary($"Patch {docs.Name}. {docs.Summary}");
                if (!string.IsNullOrEmpty(docs?.Description)) builder.WithDescription($"Fetch instance of {docs.Name} by id an applies patch operation. {docs.Description}");
                builder.WithName($"{config?.Name}Patch");
                builder.WithOpenApi();
            }

            var deleteEndpoint = config?.EndpointDelete;
            if (!string.IsNullOrEmpty(deleteEndpoint))
            {
                if (!deleteEndpoint.Contains("{id}")) deleteEndpoint = deleteEndpoint + "/{id}";

                var builder = app.MapDelete(deleteEndpoint, [Authorize] async (string id, ClaimsPrincipal user, IDWHRepository<T> repository) =>
                {
                    try
                    {
                        if (customRepository != null) repository = customRepository;
                        return Results.Ok(await repository.DeleteAsync(id, user));
                    }
                    catch (UnauthorizedAccessException unauth)
                    {
                        return Results.Unauthorized();
                    }
                });
                builder.Produces<DBBase<T>>();
                builder.ProducesProblem(500);
                var docs = typeof(T).GetCustomAttribute<RestDWHDocsAttribute>();
                builder
                    .WithOpenApi(generatedOperation =>
                    {
                        generatedOperation.Parameters[0].Description = $"The {docs?.Name} ID.";
                        return generatedOperation;
                    });
                if (!string.IsNullOrEmpty(docs?.Summary)) builder.WithSummary($"Delete {docs.Name}. {docs.Summary}");
                if (!string.IsNullOrEmpty(docs?.Description)) builder.WithDescription($"Deletes instance of {docs.Name} by id. {docs.Description}");
                builder.WithName($"{config?.Name}Delete");
                builder.WithOpenApi();
            }
        }
    }
}

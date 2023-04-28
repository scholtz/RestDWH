# Rest DWH library

The aim of this library is to provide REST CRUD endpoints for data storage

The enterprise grade level of audit data where all data are queryable and historic modifications with user information and exact timestamps.

Rest DWH library provides data governance platform for fast code first development.

## Currently supported DBs:

- elasticsearch

## How it works

User defines model and sets attribute RestDWHEntity. The library produces the controllers, the DB object models, and repositories.

```
using RestDWH.Attributes;

namespace MyApp.Model
{
    [RestDWHEntity("Test")]
    public class Test
    {
        public string Name { get; set; }
    }
}
```

This creates Get,GetById,Put,Post,Patch and Delete operations on object Test.

## Events

Coder can create custom code in events before or after the data are handled by the controller method or before are stored to the database.

For example

```
using MyApp.Model;
using RestDWH.Model;
using System.Security.Claims;

namespace MyApp.Events
{
    public class TestEvents : RestDWHEvents<Test>
    {
        public override async Task<(int from, int size, string query, string sort)> BeforeGetAsync(int from = 0, int size = 10, string query = "*", string sort = "", ClaimsPrincipal? user = null)
        {
            return (from, size, query, sort);
        }

        public override async Task<DBBase<Test>> ToCreate(DBBase<Test> item, ClaimsPrincipal? user = null)
        {
            return item;
        }

        public override async Task<DBBase<Test>> AfterDeleteAsync(DBBase<Test> result, string id, ClaimsPrincipal? user = null)
        {
            return result;
        }
    }
}
```

To tell the RestDWH you use this events for model test, reference your event class in the attribute.

```
using MyApp.Events;
using RestDWH.Attributes;

namespace MyApp.Model
{
    [RestDWHEntity("Test", typeof(TestEvents))]
    public class Test
    {
        public string Name { get; set; }
    }
}
```

## Installation

```c#
using RestDWH.Extensions;
...
# use CreateAPIControllers to add generated controllers
builder.Services.AddControllers().AddNewtonsoftJson().CreateAPIControllers();
builder.Services.AddProblemDetails();
builder.Services.AddAuthentication(...)
...
# use ExtendElasticConnectionSettings to add default mapping to the db objects
var settings =
    new ConnectionSettings(new Uri(".."))
    .ApiKeyAuthentication(new ApiKeyAuthenticationCredentials(".."))
    .ExtendElasticConnectionSettings();
... 
var client = new ElasticClient(settings);
builder.Services.AddSingleton<IElasticClient>(client);
...
# use RegisterRestDWHRepositories to register repositories
builder.Services.RegisterRestDWHRepositories();
# use RegisterRestDWHEvents to register object events classes
builder.Services.RegisterRestDWHEvents();
...
# use PreloadRestDWHRepositories to preload repositories to memory
app.PreloadRestDWHRepositories();
...
app.UseAuthentication();
app.UseAuthorization();
...
```
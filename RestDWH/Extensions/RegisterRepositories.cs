using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Newtonsoft.Json.Serialization;
using RestDWH.Attributes;
using RestDWH.Model;
using RestDWH.Repository;
using System.Reflection;
using System.Runtime.Loader;

namespace RestDWH.Extensions
{
    public static class RegisterRepositories
    {
        public static void RegisterRestDWHEvents(this IServiceCollection services)
        {
            var types = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                        from type in assembly.GetTypes()
                        where type.IsDefined(typeof(RestDWHEntity))
                        select type;
            foreach (var type in types)
            {
                RestDWHEntity[] MyAttributes = (RestDWHEntity[])Attribute.GetCustomAttributes(type, typeof(RestDWHEntity));
                if (MyAttributes.FirstOrDefault()?.Events == null)
                {
                    var genericBaseDBBase = typeof(RestDWHEvents<>).MakeGenericType(type);
                    services.AddSingleton(genericBaseDBBase);
                }
                else
                {
                    var genericBaseDBBase = typeof(RestDWHEvents<>).MakeGenericType(type);
                    services.AddSingleton(genericBaseDBBase, MyAttributes.FirstOrDefault().Events);
                }
                //var x = new Type[] { typeof(string) };
            }
        }


        public static void RegisterRestDWHRepositories(this IServiceCollection services)
        {
            var types = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                        from type in assembly.GetTypes()
                        where type.IsDefined(typeof(RestDWHEntity))
                        select type;
            foreach (var type in types)
            {
                var genericBase = typeof(BaseRepository<>).MakeGenericType(type);
                services.AddSingleton(genericBase);
            }
        }
        public static void PreloadRestDWHRepositories(this WebApplication app)
        {
            var types = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                        from type in assembly.GetTypes()
                        where type.IsDefined(typeof(RestDWHEntity))
                        select type;
            foreach (var type in types)
            {
                var genericBase = typeof(BaseRepository<>).MakeGenericType(type);

                _ = app.Services.GetService(genericBase);
            }
        }
        public static ConnectionSettings ExtendElasticConnectionSettings(this ConnectionSettings conn)
        {
            var types = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                        from type in assembly.GetTypes()
                        where type.IsDefined(typeof(RestDWHEntity))
                        select type;
            var ret = conn;
            foreach (var type in types)
            {
                RestDWHEntity[] MyAttributes = (RestDWHEntity[])Attribute.GetCustomAttributes(type, typeof(RestDWHEntity));
                var name = (MyAttributes.FirstOrDefault()?.Name ?? "obj").ToLower();
                //var x = new Type[] { typeof(string) };
                var genericBaseDBBase = typeof(DBBase<>).MakeGenericType(type);
                var genericBaseDBListBase = typeof(DBListBase<,>).MakeGenericType(type, genericBaseDBBase);
                var genericBaseDBBaseLog = typeof(DBBaseLog<>).MakeGenericType(type);

                ret = ret.DefaultMappingFor(genericBaseDBBase, r => r.IndexName($"restdwh-{name}-main"));
                ret = ret.DefaultMappingFor(genericBaseDBBaseLog, r => r.IndexName($"restdwh-{name}-log"));
            }
            return ret;
        }
        public static string? ToCamelCase(this string? str) => str is null
        ? null
        : new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() }.GetResolvedPropertyName(str);

        public static string? ToSnakeCase(this string? str) => str is null
            ? null
            : new DefaultContractResolver() { NamingStrategy = new SnakeCaseNamingStrategy() }.GetResolvedPropertyName(str);

        public static string ToPascalCase(this string word)
        {
            return string.Join("", word.Split('_')
                         .Select(w => w.Trim())
                         .Where(w => w.Length > 0)
                         .Select(w => w.Substring(0, 1).ToUpper() + w.Substring(1).ToLower()));
        }

        public static IMvcBuilder CreateAPIControllers(this IMvcBuilder builder)
        {
            var ret = builder;
            var types = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                        from type in assembly.GetTypes()
                        where type.IsDefined(typeof(RestDWHEntity))
                        select type;
            foreach (var type in types)
            {
                RestDWHEntity[] MyAttributes = (RestDWHEntity[])Attribute.GetCustomAttributes(type, typeof(RestDWHEntity));
                var name = (MyAttributes.FirstOrDefault()?.Name ?? "obj").ToPascalCase();
                string code = $@"
using RestDWH.Controllers;
using RestDWH.Model;
using RestDWH.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RestDWH.Controllers.Data
{{
    [Authorize]
    [ApiController]
    public class {name}Controller: BaseController<
            {type.ToString()}
        >
    {{
        public {name}Controller(BaseRepository<{type.ToString()}> repo) : base(repo)
        {{
        }}
    }}
}}
";

                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
                string assemblyName = $"{name}.RestDWH.dll";
                var refPaths = new[] {
                    type.GetTypeInfo().Assembly.Location,
                    typeof(System.Object).GetTypeInfo().Assembly.Location,
                    typeof(ControllerBase).GetTypeInfo().Assembly.Location,
                    Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll"),
                    Path.Combine(Path.GetDirectoryName(typeof(RestDWH.Model.DBBase<>).GetTypeInfo().Assembly.Location), "RestDWH.dll"),
                    Path.Combine(Path.GetDirectoryName(typeof(Microsoft.AspNetCore.Authorization.AuthorizationBuilder).GetTypeInfo().Assembly.Location), "Microsoft.AspNetCore.Authorization.dll"),
                    Path.Combine(Path.GetDirectoryName(typeof(Microsoft.AspNetCore.Mvc.AcceptedResult).GetTypeInfo().Assembly.Location), "Microsoft.AspNetCore.Mvc.dll")
                };
                MetadataReference[] references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();
                CSharpCompilation compilation = CSharpCompilation.Create(
                    assemblyName,
                    syntaxTrees: new[] { syntaxTree },
                    references: references,
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));


                using (var ms = new MemoryStream())
                {
                    EmitResult result = compilation.Emit(ms);

                    if (!result.Success)
                    {
                        IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                            diagnostic.IsWarningAsError ||
                            diagnostic.Severity == DiagnosticSeverity.Error);

                        foreach (Diagnostic diagnostic in failures)
                        {
                            Console.Error.WriteLine("\t{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                        }
                    }
                    else
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
                        //File.WriteAllBytes(assemblyName, ms.ToArray());
                        //var assembly = Assembly.LoadFrom(assemblyName);
                        ret = ret.AddApplicationPart(assembly);
                        //
                    }
                }
                //System.CodeDom.Compiler.CompilerParameters parameters = new CompilerParameters();
                //parameters.GenerateExecutable = false;
                //parameters.OutputAssembly = $"{name}.RestDWH.dll";
                //parameters.ReferencedAssemblies.Add(@"RestDWH.dll");
                //parameters.ReferencedAssemblies.Add(@"Microsoft.AspNetCore.Authorization.dll");
                //parameters.ReferencedAssemblies.Add(@"Microsoft.AspNetCore.Mvc");
                //CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, code);
                //Assembly.LoadFrom(parameters.OutputAssembly);
            }
            return ret;
        }
    }
}

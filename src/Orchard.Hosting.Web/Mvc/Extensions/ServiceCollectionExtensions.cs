using Microsoft.AspNet.Mvc.Infrastructure;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.AspNet.Mvc.Razor.Compilation;
using Microsoft.AspNet.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Hosting.Mvc.Razor;
using Orchard.Hosting.Mvc.Routing;

namespace Orchard.Hosting.Mvc
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrchardMvc(this IServiceCollection services)
        {
            services
                .AddMvcCore()
                .AddViews()
                .AddRazorViewEngine();

            services.AddScoped<IAssemblyProvider, OrchardMvcAssemblyProvider>();

            services.AddSingleton<ICompilationService, DefaultRoslynCompilationService>();

            services.Configure<RazorViewEngineOptions>(options =>
            {
                var expander = new ModuleViewLocationExpander();
                options.ViewLocationExpanders.Add(expander);
            });
            return services;
        }
    }
}
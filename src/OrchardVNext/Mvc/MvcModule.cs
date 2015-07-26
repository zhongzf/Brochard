using Autofac;
using Autofac.Framework.DependencyInjection;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Razor.Compilation;
using Microsoft.Framework.DependencyInjection;
using OrchardVNext.Mvc.Razor;

namespace OrchardVNext.Mvc {
    public class MvcModule : Module {
        protected override void Load(ContainerBuilder builder) {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMvc();
            serviceCollection.AddSingleton<ICompilationService, DefaultRoslynCompilationService>();

            serviceCollection.AddTransient<IAssemblyProvider, OrchardMvcAssemblyProvider>();

            serviceCollection.ConfigureRazorViewEngine(options => {
                options.ViewLocationExpanders.Add(new ModuleViewLocationExpander());
            });

            builder.Populate(serviceCollection);
        }
    }
}
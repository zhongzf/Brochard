using System;
using Autofac;
using Autofac.Dnx;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Runtime.Caching;
using OrchardVNext.Environment.Configuration;
using OrchardVNext.Environment.Extensions;
using OrchardVNext.Environment.Extensions.Folders;
using OrchardVNext.Environment.Extensions.Loaders;
using OrchardVNext.Environment.ShellBuilders;
using OrchardVNext.FileSystems.AppData;
using OrchardVNext.FileSystems.VirtualPath;
using OrchardVNext.FileSystems.WebSite;
using OrchardVNext.Routing;

namespace OrchardVNext.Environment {
    public class OrchardStarter {
        public static IServiceProvider ConfigureHost(IServiceCollection services) {
            services.AddSingleton<IHostEnvironment, DefaultHostEnvironment>();
            services.AddSingleton<IAppDataFolderRoot, AppDataFolderRoot>();

            services.AddSingleton<IWebSiteFolder, WebSiteFolder>();
            services.AddSingleton<IAppDataFolder, AppDataFolder>();
            services.AddSingleton<IVirtualPathProvider, DefaultVirtualPathProvider>();

            // Caching - Move out
            services.AddInstance<ICacheContextAccessor>(new CacheContextAccessor());
            services.AddSingleton<ICache, Cache>();

            services.AddSingleton<IOrchardHost, DefaultOrchardHost>();
            {
                services.AddSingleton<IShellSettingsManager, ShellSettingsManager>();

                services.AddSingleton<IShellContextFactory, ShellContextFactory>();
                {
                    services.AddSingleton<ICompositionStrategy, CompositionStrategy>();
                    {
                        services.AddSingleton<IOrchardLibraryManager, OrchardLibraryManager>();
                        services.AddSingleton<IExtensionManager, ExtensionManager>();
                        {
                            services.AddSingleton<IExtensionHarvester, ExtensionHarvester>();
                            services.AddSingleton<IExtensionFolders, CoreModuleFolders>();
                            services.AddSingleton<IExtensionFolders, ModuleFolders>();

                            services.AddSingleton<IExtensionLoader, CoreExtensionLoader>();
                            services.AddSingleton<IExtensionLoader, DynamicExtensionLoader>();
                        }
                    }

                    services.AddSingleton<IShellContainerFactory, ShellContainerFactory>();
                }
            }

            services.AddTransient<IOrchardShellHost, DefaultOrchardShellHost>();

            var builder = new ContainerBuilder();
            
            builder.Populate(services);

            builder.RegisterType<DefaultOrchardShell>().As<IOrchardShell>().InstancePerMatchingLifetimeScope("shell");

            var container = builder.Build();

            return container.Resolve<IServiceProvider>();
        }

        public static IOrchardHost CreateHost(IApplicationBuilder app, ILoggerFactory loggerFactory) {
            loggerFactory.AddProvider(new TestLoggerProvider());

            app.UseMiddleware<OrchardContainerMiddleware>();
            app.UseMiddleware<OrchardShellHostMiddleware>();

            // Think this needs to be inserted in a different part of the pipeline, possibly
            // when DI is created for the shell
            app.UseMiddleware<OrchardRouterMiddleware>();
            
            return app.ApplicationServices.GetService<IOrchardHost>();
        }
    }
}
using System;
using System.Linq;
using Microsoft.AspNet.Routing;
using OrchardVNext.Environment.Configuration;
using OrchardVNext.Environment.ShellBuilders.Models;
using OrchardVNext.Routing;
using Autofac.Builder;
using Autofac;
using Autofac.Core;
using Autofac.Features.Indexed;

#if DNXCORE50
using System.Reflection;
#endif

namespace OrchardVNext.Environment.ShellBuilders {
    public interface IShellContainerFactory {
        IServiceProvider CreateContainer(ShellSettings settings, ShellBlueprint blueprint);
    }

    public class ShellContainerFactory : IShellContainerFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILifetimeScope _lifetimeScope;

        public ShellContainerFactory(IServiceProvider serviceProvider,
            ILifetimeScope lifetimeScope) {
            _serviceProvider = serviceProvider;
            _lifetimeScope = lifetimeScope;
        }

        public IServiceProvider CreateContainer(ShellSettings settings, ShellBlueprint blueprint)
        {
            var intermediateScope = _lifetimeScope.BeginLifetimeScope(
                builder => {
                    foreach (var item in blueprint.Dependencies.Where(t => typeof(IModule).IsAssignableFrom(t.Type))) {
                        var registration = RegisterType(builder, item)
                            .Keyed<IModule>(item.Type)
                            .InstancePerDependency();

                        foreach (var parameter in item.Parameters) {
                            registration = registration
                                .WithParameter(parameter.Name, parameter.Value)
                                .WithProperty(parameter.Name, parameter.Value);
                        }
                    }
                });

            return intermediateScope
                .BeginLifetimeScope(
                    "shell",
                    builder => {
                        builder.RegisterType<DefaultShellRouteBuilder>().As<IRouteBuilder>().InstancePerLifetimeScope();
                        builder.Register(ctx => settings);
                        builder.Register(ctx => blueprint.Descriptor);
                        builder.Register(ctx => blueprint);

                        var moduleIndex = intermediateScope.Resolve<IIndex<Type, IModule>>();
                        foreach (
                            var item in blueprint.Dependencies.Where(t => typeof (IModule).IsAssignableFrom(t.Type))) {
                            builder.RegisterModule(moduleIndex[item.Type]);
                        }

                        foreach (
                            var item in blueprint.Dependencies.Where(t => typeof (IDependency).IsAssignableFrom(t.Type))
                            ) {
                            var registration = RegisterType(builder, item)
                                .InstancePerLifetimeScope();

                            foreach (var interfaceType in item.Type.GetInterfaces()
                                .Where(itf => typeof (IDependency).IsAssignableFrom(itf))) {

                                Logger.Debug("Type: {0}, Interface Type: {1}", item.Type, interfaceType);

                                registration = registration.As(interfaceType).AsSelf();
                                if (typeof (ISingletonDependency).IsAssignableFrom(interfaceType)) {
                                    registration = registration.InstancePerMatchingLifetimeScope("shell");
                                }
                                else if (typeof (IUnitOfWorkDependency).IsAssignableFrom(interfaceType)) {
                                    registration = registration.InstancePerMatchingLifetimeScope("work");
                                }
                                else if (typeof (ITransientDependency).IsAssignableFrom(interfaceType)) {
                                    registration = registration.InstancePerDependency();
                                }
                            }

                            foreach (var parameter in item.Parameters) {
                                registration = registration
                                    .WithParameter(parameter.Name, parameter.Value)
                                    .WithProperty(parameter.Name, parameter.Value);
                            }
                        }

                        /* Get rid of this */
                        //ServiceCollection services = new ServiceCollection();
                        //services.AddLogging();
                        //builder.Populate(services);
                        /*******************/
                    })
                .Resolve<IServiceProvider>();
        }
        
        private IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterType(ContainerBuilder builder, ShellBlueprintItem item) {
            return builder.RegisterType(item.Type)
                .WithProperty("Feature", item.Feature)
                .WithMetadata("Feature", item.Feature);
        }
    }
}
using OrchardVNext.Mvc.Routes;
using OrchardVNext.Mvc;
using Autofac;

namespace OrchardVNext.Setup {
    public class SetupMode : Module {
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterModule<MvcModule>();

            builder.RegisterType<RoutePublisher>().As<IRoutePublisher>().InstancePerLifetimeScope();
        }
    }
}

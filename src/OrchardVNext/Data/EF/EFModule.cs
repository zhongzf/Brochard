using Autofac;
using Autofac.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection;

namespace OrchardVNext.Data.EF {
    public class EFModule : Module {
        protected override void Load(ContainerBuilder builder) {
            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddEntityFramework()
                .AddInMemoryDatabase()
                .AddDbContext<DataContext>();

            builder.Populate(serviceCollection);
        }
    }
}
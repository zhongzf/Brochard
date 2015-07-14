using System;
using Autofac;
using Microsoft.AspNet.Http;

namespace OrchardVNext.Environment {
    public class WorkContextAccessor : IWorkContextAccessor {
        private readonly ILifetimeScope _lifetimeScope;

        public WorkContextAccessor(
            ILifetimeScope lifetimeScope) {
            _lifetimeScope = lifetimeScope;
        }

        public IWorkContextScope CreateWorkContextScope() {
            var workLifetime = _lifetimeScope.BeginLifetimeScope("work");
            return null;
        }

        public IWorkContextScope CreateWorkContextScope(HttpContext httpContext) {
            throw new NotImplementedException();
        }
    }

    public class WorkContextModule : Module {
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterType<WorkContextAccessor>()
                .As<IWorkContextAccessor>()
                .InstancePerMatchingLifetimeScope("shell");
        }
    }
}
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Detached.EntityFramework
{
    /// <summary>
    /// Provides custom configuration parameters for EntityFramework.Detached.
    /// </summary>
    public class DetachedOptionsExtension : IDbContextOptionsExtension
    {
        /// <summary>
        /// Gets an extension for the internal service collection of the DbContext.
        /// Allows to insert custom services into the internal service provider.
        /// </summary>
        public IServiceCollection DetachedServices { get; } = new ServiceCollection();

        /// <summary>
        /// Defines the behavior of detached update when an entity with a specified key value
        /// does not exist in the database. Usually happens if the entity was deleted or it never existed.
        /// Entities with no key value defined are automatically added.
        /// </summary>
        public bool ThrowExceptionOnEntityNotFound { get; set; }

        public bool ApplyServices(IServiceCollection services)
        {
            services.AddDetachedEntityFramework();
            foreach (var detachedService in DetachedServices)
            {
                services.Add(detachedService);
            }

            return true;
        }

        public long GetServiceProviderHashCode()
        {
            return GetHashCode();
        }

        public void Validate(IDbContextOptions options)
        {
        }

        public string LogFragment { get; } = "EntityFramework.Dettached";
    }
}
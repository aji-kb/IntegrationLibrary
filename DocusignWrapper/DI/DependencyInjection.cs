using IntegrationLibrary.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationLibrary.DI
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDocusignRegistration(this IServiceCollection serviceCollection)
        {

            serviceCollection.AddScoped<IDocusignWrapper, DocusignWrapper>();

            return serviceCollection;
        }
    }
}

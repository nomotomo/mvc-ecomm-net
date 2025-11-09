using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Behaviour;
using System.Reflection;

namespace Ordering.Application.Extensions
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register AutoMapper with assembly scanning
            services.AddAutoMapper(expression => {} ,Assembly.GetExecutingAssembly());

            // Register MediatR services from assembly
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            
            // If AddValidatorsFromAssembly is missing, use AddValidatorsFromAssemblyContaining<T>,
            // or manually add validators; here we use AddValidatorsFromAssemblyContaining
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Register pipeline behaviors for MediatR
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));

            return services;
        }
    }
}
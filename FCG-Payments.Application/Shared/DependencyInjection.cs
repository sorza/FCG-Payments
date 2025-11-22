using FCG_Payments.Application.Payments.Requests;
using FCG_Payments.Application.Payments.Services;
using FCG_Payments.Application.Payments.Validators;
using FCG_Payments.Application.Shared.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FCG_Payments.Application.Shared
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IValidator<PaymentRequest>, PaymentRequestValidator>();

            return services;
        }
    }
}
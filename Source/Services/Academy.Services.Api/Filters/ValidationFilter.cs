using FluentValidation;

namespace Academy.Services.Api.Filters
{
    public sealed class ValidationFilter<T> : IEndpointFilter where T : class
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
        {
            T? arg = ctx.Arguments.OfType<T>().FirstOrDefault();
            if (arg is null)
            {
                return await next(ctx);
            }

            IValidator<T>? validator = ctx.HttpContext.RequestServices.GetService<IValidator<T>>();
            if (validator is null)
            {
                return await next(ctx);
            }

            FluentValidation.Results.ValidationResult result = await validator.ValidateAsync(arg);
            if (!result.IsValid)
            {
                IDictionary<string, string[]> errors = result.ToDictionary();
                return Results.ValidationProblem(errors);
            }

            return await next(ctx);
        }
    }

    public static class ValidationFilterExtensions
    {
        public static TBuilder Validate<TBuilder, TRequest>(this TBuilder builder)
            where TBuilder : IEndpointConventionBuilder
            where TRequest : class
            => builder.AddEndpointFilter(new ValidationFilter<TRequest>());
    }
}

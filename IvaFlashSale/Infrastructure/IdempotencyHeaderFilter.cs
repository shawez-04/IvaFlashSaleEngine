using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace IvaFlashSaleEngine.Infrastructure;
public class IdempotencyHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Only add the header if the [RequiresIdempotency] attribute is present on the action
        var hasAttribute = context.MethodInfo.GetCustomAttributes(true)
            .Any(attr => attr is RequiresIdempotencyAttribute);

        if (hasAttribute)
        {
            operation.Parameters ??= new List<OpenApiParameter>();
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Idempotency-Key",
                In = ParameterLocation.Header,
                Required = true,
                Schema = new OpenApiSchema { Type = "string", Format = "uuid" },
                Description = "Unique GUID to prevent duplicate processing of this specific purchase."
            });
        }
    }
}
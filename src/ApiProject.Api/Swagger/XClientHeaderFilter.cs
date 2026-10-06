using ApiProject.Api.Tenancy;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiProject.Api.Swagger;

public class XClientHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = FirmaHeaderlari.XClient,
            In = ParameterLocation.Header,
            Required = false,
            Description = "Firma kodu (Host bir firmaya ait değilse kullanılır)",
            Schema = new OpenApiSchema { Type = "string" }
        });
    }
}

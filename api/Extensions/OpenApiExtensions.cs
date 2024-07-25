// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.OpenApi.Extensions;
using Swashbuckle.AspNetCore.Swagger;

namespace BudgetTracker.Extensions;

/// <summary>
/// Implements OpenAPI-related extensions.
/// </summary>
public static class OpenApiExtensions
{
    /// <summary>
    /// Saves the generated OpenAPI specs in both JSON and YAML format.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to get OpenAPI from.</param>
    /// <returns>An asynchronous task indicating the status of the operation.</returns>
    public static async Task SaveOpenApiDocs(this WebApplication app)
    {
        var swagger = app.Services.GetRequiredService<IAsyncSwaggerProvider>();
        var openApiDoc = await swagger.GetSwaggerAsync("v1");

        var swaggerFile = openApiDoc.SerializeAsJson(Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0);
        await File.WriteAllTextAsync("../openapi/swagger.json", swaggerFile);

        var openApiFile = openApiDoc.SerializeAsYaml(Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0);
        await File.WriteAllTextAsync("../openapi/openapi.yml", openApiFile);
    }
}

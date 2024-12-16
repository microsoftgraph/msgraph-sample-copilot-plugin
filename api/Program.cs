// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using BudgetTracker.Endpoints;
using BudgetTracker.Models;
using BudgetTracker.Services;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

// To enable emoji's in logger output to the terminal
Console.OutputEncoding = Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

// Set up OpenAPI to generate OpenAPI specification
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi((options) =>
{
    // Add document transform to add additional info
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        var settings = builder.Configuration
            .Get<AppSettings>() ?? throw new ApplicationException("Could not load settings from appsettings.json");

        // Add the dev tunnel URL if specified in app settings
        if (!string.IsNullOrEmpty(settings.ServerUrl))
        {
            // Clear localhost entries added automatically
            document.Servers.Clear();
            document.Servers.Add(new()
            {
                Url = settings.ServerUrl,
            });
        }

        _ = settings.AzureAd?.Instance ?? throw new ApplicationException(nameof(settings.AzureAd.Instance));
        _ = settings.AzureAd?.TenantId ?? throw new ApplicationException(nameof(settings.AzureAd.TenantId));
        _ = settings.AzureAd?.ClientId ?? throw new ApplicationException(nameof(settings.AzureAd.ClientId));

        var baseAuthUrl = settings.AzureAd.Instance + settings.AzureAd.TenantId;
        var apiScope = $"api://{settings.AzureAd.ClientId}/.default";

        // Add the OAuth2 security scheme
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes.Add("OAuth2", new()
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new()
            {
                AuthorizationCode = new()
                {
                    AuthorizationUrl = new Uri($"{baseAuthUrl}/oauth2/v2.0/authorize"),
                    TokenUrl = new Uri($"{baseAuthUrl}/oauth2/v2.0/token"),
                    RefreshUrl = new Uri($"{baseAuthUrl}/oauth2/v2.0/token"),
                    Scopes = new Dictionary<string, string>()
                    {
                        { apiScope, "Access Budget Tracker as you" },
                    },
                },
            },
        });

        // Add security requirement to all endpoints
        document.SecurityRequirements.Add(new()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new()
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "OAuth2",
                    },
                },
                [
                    apiScope
                ]
            },
        });

        return Task.CompletedTask;
    });

    // Add operation transform to add x-openai-isConsequential to override prompt behavior
    // See https://learn.microsoft.com/microsoft-365-copilot/extensibility/api-plugin-confirmation-prompts
    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        if (string.Compare(operation.OperationId, "SendTransactionReport", StringComparison.Ordinal) == 0)
        {
            operation.Extensions.Add("x-openai-isConsequential", new OpenApiBoolean(false));
        }

        return Task.CompletedTask;
    });
});

// Configure authentication
builder.Services
    /* Use Web API authentication (default JWT bearer token scheme) */
    .AddMicrosoftIdentityWebApiAuthentication(builder.Configuration)
    /* Enable token acquisition via on-behalf-of flow */
    .EnableTokenAcquisitionToCallDownstreamApi()
    /* Add authenticated Graph client via dependency injection */
    .AddMicrosoftGraph(builder.Configuration.GetSection("Graph"))
    /* Use in-memory token cache */
    /* See https://github.com/AzureAD/microsoft-identity-web/wiki/token-cache-serialization */
    .AddInMemoryTokenCaches();
builder.Services.AddAuthorization();

// Add services to the container.
var budgetService = new BudgetService();
builder.Services.AddSingleton(budgetService);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/BudgetTracker.json");
}

app.UseHttpsRedirection();

BudgetsEndpoint.Map(app);
TransactionsEndpoint.Map(app);

app.UseAuthentication();
app.UseAuthorization();

app.Run();

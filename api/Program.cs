// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using BudgetTracker.Endpoints;
using BudgetTracker.Extensions;
using BudgetTracker.Models;
using BudgetTracker.Services;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

// To enable emoji's in logger output to the terminal
Console.OutputEncoding = Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

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

var settings = builder.Configuration
    .Get<AppSettings>() ?? throw new ApplicationException("Could not load settings from appsettings.json");

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddServer(new OpenApiServer()
    {
        Url = string.IsNullOrEmpty(settings.ServerUrl) ? "https://localhost:7196" : settings.ServerUrl,
    });

    _ = settings.AzureAd?.Instance ?? throw new ApplicationException(nameof(settings.AzureAd.Instance));
    _ = settings.AzureAd?.TenantId ?? throw new ApplicationException(nameof(settings.AzureAd.TenantId));
    _ = settings.AzureAd?.ClientId ?? throw new ApplicationException(nameof(settings.AzureAd.ClientId));

    var baseAuthUrl = settings.AzureAd.Instance + settings.AzureAd.TenantId;
    var apiScope = $"api://{settings.AzureAd.ClientId}/.default";

    options.AddSecurityDefinition("OAuth2", new()
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new()
        {
            AuthorizationCode = new()
            {
                AuthorizationUrl = new Uri($"{baseAuthUrl}/oauth2/v2.0/authorize"),
                TokenUrl = new Uri($"{baseAuthUrl}/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string>()
                {
                    { apiScope, "Access Budget Tracker as you" },
                },
            },
        },
    });

    options.AddSecurityRequirement(new()
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

    // Enable example responses
    var filePath = Path.Combine(AppContext.BaseDirectory, "BudgetTracker.xml");
    options.IncludeXmlComments(filePath);
});

var budgetService = new BudgetService();
builder.Services.AddSingleton(budgetService);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

BudgetsEndpoint.Map(app);
TransactionsEndpoint.Map(app);

app.UseAuthentication();
app.UseAuthorization();

app.Lifetime.ApplicationStarted.Register(async () =>
{
    await app.SaveOpenApiDocs();
});

app.Run();

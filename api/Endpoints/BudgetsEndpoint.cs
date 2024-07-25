// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using BudgetTracker.Models;
using BudgetTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Microsoft.OpenApi.Any;

namespace BudgetTracker.Endpoints;

/// <summary>
/// Implements handlers for incoming requests to the budgets endpoint.
/// </summary>
[Authorize]
public static class BudgetsEndpoint
{
    private static readonly string Endpoint = "/budgets";

    /// <summary>
    /// Maps the endpoint to allowed HTTP methods.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance to map endpoints with.</param>
    public static void Map(WebApplication app)
    {
        app.MapGet(Endpoint, GetBudgets)
            .WithName("GetBudgets")
            .WithSummary("Get budgets based on budget name")
            .WithDescription("Returns details including name and available funds of budgets, optionally filtered by budget name")
            .WithOpenApi(operation =>
            {
                operation.Parameters[0].Description = "The name of the budget to retrieve";
                operation.Parameters[0].Required = false;
                return operation;
            })
            .RequireAuthorization();

        app.MapPost(Endpoint, CreateBudget)
            .WithName("CreateBudget")
            .WithSummary("Create a new budget")
            .WithDescription("Create a new budget with a specified name and available funds")
            .RequireAuthorization();

        app.MapPost($"{Endpoint}/charge", ChargeBudget)
            .WithName("ChargeBudget")
            .WithSummary("Charge an amount to a budget")
            .WithDescription("Charge an amount to a budget with a specified name. This removes the specified amount from the budget's available funds")
            .WithOpenApi()
            .RequireAuthorization();

        app.MapPost($"{Endpoint}/extend", ExtendBudget)
            .WithName("ExtendBudget")
            .WithSummary("Add an amount to a budget")
            .WithDescription("Add an amount to a budget with a specified name. This adds the specified amount to the budget's available funds")
            .WithOpenApi()
            .RequireAuthorization();
    }

    private static Results<Ok<List<Budget>>, BadRequest<ApiResponse>> GetBudgets(
        HttpContext context,
        [FromQuery] string? budgetName,
        [FromServices] BudgetService budgetService,
        [FromServices] ILogger<Program> logger)
    {
        context.VerifyUserHasAnyAcceptedScope(["access_as_user"]);
        var apiPath = $"{context.Request.Path}{context.Request.QueryString}";
        try
        {
            logger.LogInformation("➡️ GET {api}", apiPath);
            var budgets = budgetService.GetBudgets(budgetName);
            logger.LogInformation("✅ GET {api} returning {count} budgets", apiPath, budgets.Count);
            return TypedResults.Ok(budgets);
        }
        catch (Exception ex)
        {
            logger.LogError("⛔ GET {api} returning error {error}", apiPath, ex.Message);
            return TypedResults.BadRequest(new ApiResponse(ex.Message));
        }
    }

    private static Results<Created<Budget>, BadRequest<ApiResponse>> CreateBudget(
        HttpContext context,
        [FromBody] Budget budget,
        [FromServices] BudgetService budgetService,
        [FromServices] ILogger<Program> logger)
    {
        context.VerifyUserHasAnyAcceptedScope(["access_as_user"]);
        try
        {
            logger.LogInformation("➡️ POST {endpoint}: name {name}, available funds {funds}", Endpoint, budget.Name, budget.AvailableFunds);
            var newBudget = budgetService.CreateBudget(budget);
            logger.LogInformation("✅ POST {endpoint} created new budget named {name}", Endpoint, newBudget.Name);
            return TypedResults.Created(string.Empty, newBudget);
        }
        catch (Exception ex)
        {
            logger.LogError("⛔ POST {endpoint} returning error {error}", Endpoint, ex.Message);
            return TypedResults.BadRequest(new ApiResponse(ex.Message));
        }
    }

    private static Results<Accepted<ApiResponse>, BadRequest<ApiResponse>> ChargeBudget(
        HttpContext context,
        [FromBody] Transaction charge,
        [FromServices] BudgetService budgetService,
        [FromServices] ILogger<Program> logger)
    {
        context.VerifyUserHasAnyAcceptedScope(["access_as_user"]);
        try
        {
            logger.LogInformation("➡️ POST {endpoint}/charge: budget {name}, charge {amount}", Endpoint, charge.BudgetName, charge.Amount);
            var amount = charge.Amount;
            var newBalance = budgetService.ChargeBudget(charge);
            logger.LogInformation(
                "✅ POST {endpoint}/charge successfully charged {amount} to {budget}, new budget balance {newBalance}",
                Endpoint,
                amount,
                charge.BudgetName,
                newBalance);
            return TypedResults.Accepted(
                uri: string.Empty,
                value: new ApiResponse($"{amount} has been deducted from {charge.BudgetName}. The budget has {newBalance} remaining in available funds."));
        }
        catch (Exception ex)
        {
            logger.LogError("⛔ POST {endpoint}/charge returning error {error}", Endpoint, ex.Message);
            return TypedResults.BadRequest(new ApiResponse(ex.Message));
        }
    }

    private static Results<Accepted<ApiResponse>, BadRequest<ApiResponse>> ExtendBudget(
        HttpContext context,
        [FromBody] Transaction extension,
        [FromServices] BudgetService budgetService,
        [FromServices] ILogger<Program> logger)
    {
        context.VerifyUserHasAnyAcceptedScope(["access_as_user"]);
        try
        {
            logger.LogInformation("➡️ POST {endpoint}/extend: budget {name}, charge {amount}", Endpoint, extension.BudgetName, extension.Amount);
            var newBalance = budgetService.ExtendBudget(extension);
            logger.LogInformation(
                "✅ POST {endpoint}/charge successfully added {amount} to {budget}, new budget balance {newBalance}",
                Endpoint,
                extension.Amount,
                extension.BudgetName,
                newBalance);
            return TypedResults.Accepted(
                uri: string.Empty,
                value: new ApiResponse($"{extension.Amount} has been added to {extension.BudgetName}. The budget has {newBalance} remaining in available funds."));
        }
        catch (Exception ex)
        {
            logger.LogError("⛔ POST {endpoint}/extend returning error {error}", Endpoint, ex.Message);
            return TypedResults.BadRequest(new ApiResponse(ex.Message));
        }
    }
}

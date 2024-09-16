// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using BudgetTracker.Models;
using BudgetTracker.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Identity.Web.Resource;
using Microsoft.OpenApi.Any;

namespace BudgetTracker.Endpoints;

/// <summary>
/// Implements handlers for incoming requests to the transactions endpoint.
/// </summary>
public static class TransactionsEndpoint
{
    private static readonly string Endpoint = "/transactions";

    /// <summary>
    /// Maps the endpoint to allowed HTTP methods.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance to map endpoints with.</param>
    public static void Map(WebApplication app)
    {
        app.MapGet(Endpoint, GetTransactions)
            .WithName("GetTransactions")
            .WithSummary("Get transactions based on budget name or category")
            .WithDescription("Returns details of transactions identified from filters like budget name or category. Multiple filters can be used in combination to refine the list of transactions returned")
            .WithOpenApi(operation =>
            {
                operation.Parameters[0].Description = "The name of the budget to filter results on";
                operation.Parameters[0].Required = false;
                operation.Parameters[1].Description = "The name of the category to filter results on";
                operation.Parameters[1].Required = false;
                return operation;
            })
            .RequireAuthorization();

        app.MapPost($"{Endpoint}/send", SendTransactionReport)
            .WithName("SendTransactionReport")
            .WithSummary("Send a transaction report to my email")
            .WithDescription("Sends a transaction report via email, optionally filtered by budget.")
            .WithOpenApi(operation =>
            {
                // Set this to false so Copilot will show an "always allow" option
                operation.Extensions.Add("x-openai-isConsequential", new OpenApiBoolean(false));

                operation.Parameters[0].Description = "The name of the budget to filter report on";
                operation.Parameters[0].Required = false;
                return operation;
            })
            .RequireAuthorization();
    }

    private static Results<Ok<TransactionListResponse>, BadRequest<ApiResponse>> GetTransactions(
        HttpContext context,
        [FromQuery] string? budgetName,
        [FromQuery] string? category,
        [FromServices] BudgetService budgetService,
        [FromServices] ILogger<Program> logger)
    {
        context.VerifyUserHasAnyAcceptedScope(["access_as_user"]);
        var apiPath = $"{context.Request.Path}{context.Request.QueryString}";
        try
        {
            logger.LogInformation("➡️ GET {api}", apiPath);
            var transactions = budgetService.GetTransactions(budgetName, category);
            logger.LogInformation("✅ GET {api} returning {count} transactions", apiPath, transactions.Count);
            return TypedResults.Ok(new TransactionListResponse(transactions));
        }
        catch (Exception ex)
        {
            logger.LogError("⛔ GET {api} returning error {error}", apiPath, ex.Message);
            return TypedResults.BadRequest(new ApiResponse(ex.Message));
        }
    }

    private static async Task<Results<Accepted<ApiResponse>, BadRequest<ApiResponse>>> SendTransactionReport(
        HttpContext context,
        [FromQuery] string? budgetName,
        [FromServices] BudgetService budgetService,
        [FromServices] GraphServiceClient graphClient,
        [FromServices] ILogger<Program> logger)
    {
        context.VerifyUserHasAnyAcceptedScope(["access_as_user"]);
        var apiPath = $"{context.Request.Path}{context.Request.QueryString}";

        try
        {
            logger.LogInformation("➡️ POST {api}", apiPath);
            var transactions = budgetService.GetTransactions(budgetName, null);

            // Get authenticated user's email address
            var user = await graphClient.Me.GetAsync(config =>
            {
                config.QueryParameters.Select = ["displayName, mail"];
            });

            logger.LogInformation("Authenticated user: {name} ({email})", user?.DisplayName, user?.Mail);
            if (string.IsNullOrEmpty(user?.Mail))
            {
                throw new Exception("Authenticated user has no email address");
            }

            var report = new Microsoft.Graph.Models.Message
            {
                Subject = "Budget Tracker Transaction Report",
                ToRecipients =
                [
                    new()
                    {
                        EmailAddress = new()
                        {
                            Name = user.DisplayName,
                            Address = user.Mail,
                        },
                    },
                ],
                Body = new()
                {
                    ContentType = Microsoft.Graph.Models.BodyType.Html,
                    Content = GenerateTransactionReport(transactions),
                },
            };

            await graphClient.Me.SendMail.PostAsync(new()
            {
                Message = report,
                SaveToSentItems = true,
            });

            logger.LogInformation("✅ POST {api} sent email successfully", apiPath);
            return TypedResults.Accepted(
                uri: string.Empty,
                value: new ApiResponse("Transaction report sent."));
        }
        catch (Exception ex)
        {
            logger.LogError("⛔ POST {api} returning error {error}", apiPath, ex.Message);
            return TypedResults.BadRequest(new ApiResponse(ex.Message));
        }
    }

    private static string? GenerateTransactionReport(List<Transaction>? transactions)
    {
        List<string> report =
        [
            "<h1>Budget Tracker Transaction Report</h1>"
        ];

        if (transactions == null)
        {
            report.Add("<p>No transactions.</p>");
            return string.Join(Environment.NewLine, report);
        }

        // Get the budgets included in the list
        var budgets = transactions.Select(t => t.BudgetName).Distinct();
        foreach (var budget in budgets)
        {
            // Get transactions for this budget
            var budgetTransactions = transactions.Where(t => t.BudgetName == budget);
            report.Add($"<h2>{budget}</h2>");
            report.Add("<table border=\"1px solid black\" cellspacing=\"0\" cellpadding=\"3\">");
            report.Add("<tr>");
            report.Add("<th>Amount</th>");
            report.Add("<th>Category</th>");
            report.Add("<th>Description</th>");
            report.Add("</tr>");

            foreach (var transaction in budgetTransactions)
            {
                report.Add("<tr>");
                report.Add(string.Format("<td>{0:C}</td>", transaction.Amount));
                report.Add($"<td>{transaction.ExpenseCategory}</td>");
                report.Add($"<td>{transaction.Description}</td>");
                report.Add("</tr>");
            }

            report.Add("</table>");
        }

        return string.Join(Environment.NewLine, report);
    }
}

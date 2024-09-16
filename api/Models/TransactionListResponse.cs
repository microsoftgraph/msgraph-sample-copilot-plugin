// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using AdaptiveCards;

namespace BudgetTracker.Models;

/// <summary>
/// Represents an API response payload that includes a list of transactions and a list of Adaptive Card templates.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TransactionListResponse"/> class.
/// </remarks>
/// <param name="transactions">The list of transactions to include in the response.</param>
public class TransactionListResponse(List<Transaction> transactions)
{
    /// <summary>
    /// Gets the API response payload.
    /// </summary>
    public List<Transaction> Transactions { get; private set; } = transactions;

    /// <summary>
    /// Gets the Adaptive Card templates.
    /// </summary>
    public Dictionary<string, JsonDocument?> Templates { get; private set; } = new()
    {
        { "debit", GetDebitTransactionCard() },
        { "credit", GetCreditTransactionCard() },
    };

    private static JsonDocument? GetDebitTransactionCard()
    {
        var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 5));
        card.AdditionalProperties.Add("$schema", "http://adaptivecards.io/schemas/adaptive-card.json");

        card.Body.Add(new AdaptiveTextBlock()
        {
            // Red for a debit
            Color = AdaptiveTextColor.Attention,
            Size = AdaptiveTextSize.Medium,
            Weight = AdaptiveTextWeight.Bolder,
            Text = "Debit",
        });

        card.Body.Add(new AdaptiveFactSet()
        {
            Facts =
            [
                new AdaptiveFact() { Title = "Budget", Value = "${budgetName}" },
                new AdaptiveFact() { Title = "Amount", Value = "${formatNumber(amount, 2)}" },
                new AdaptiveFact() { Title = "Category", Value = "${if(expenseCategory, expenseCategory, 'N/A')}" },
                new AdaptiveFact() { Title = "Description", Value = "${if(description, description, 'N/A')}" },
            ],
        });

        return JsonSerializer.Deserialize<JsonDocument>(card.ToJson());
    }

    private static JsonDocument? GetCreditTransactionCard()
    {
        var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 5));
        card.AdditionalProperties.Add("$schema", "http://adaptivecards.io/schemas/adaptive-card.json");

        card.Body.Add(new AdaptiveTextBlock()
        {
            // Green for a credit
            Color = AdaptiveTextColor.Good,
            Size = AdaptiveTextSize.Medium,
            Weight = AdaptiveTextWeight.Bolder,
            Text = "Credit",
        });

        card.Body.Add(new AdaptiveFactSet()
        {
            Facts =
            [
                new AdaptiveFact() { Title = "Budget", Value = "${budgetName}" },
                new AdaptiveFact() { Title = "Amount", Value = "${formatNumber(amount, 2)}" },
                new AdaptiveFact() { Title = "Description", Value = "${if(description, description, 'N/A')}" },
            ],
        });

        return JsonSerializer.Deserialize<JsonDocument>(card.ToJson());
    }
}

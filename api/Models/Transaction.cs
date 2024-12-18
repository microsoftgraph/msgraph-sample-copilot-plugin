// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Models;

/// <summary>
/// Represents a change to a budget's available funds.
/// </summary>
public class Transaction
{
    /// <summary>
    /// Gets or sets the name of the budget to adjust.
    /// </summary>
    /// <example>Contoso Copilot plugin project</example>
    [Required]
    [Description("The name of the budget to adjust")]
    public string? BudgetName { get; set; }

    /// <summary>
    /// Gets or sets the amount of the adjustment.
    /// </summary>
    /// <example>-5000</example>
    [Required]
    [Description("The amount of the adjustment")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets a description for the change.
    /// </summary>
    /// <example>Purchase new laptops for team</example>
    [Required]
    [Description("The description for the change")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the expense category for charges against the budget.
    /// </summary>
    /// <example>hardware</example>
    [Description("The expense category for charges against the budget")]
    public string? ExpenseCategory { get; set; }

    /// <summary>
    /// Gets the JSONPath to the Adaptive Card template to use for rendering this transaction in an API plugin.
    /// </summary>
    [Description("The JSONPath to the Adaptive Card template to use for rendering this transaction in an API plugin")]
    public string DisplayTemplate
    {
        get
        {
            return $"$.templates.{(Amount < 0 ? "debit" : "credit")}";
        }
    }
}

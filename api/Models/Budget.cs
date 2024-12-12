// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Models;

/// <summary>
/// Represents a project budget.
/// </summary>
public class Budget
{
    /// <summary>
    /// Gets or sets the name of the budget.
    /// </summary>
    /// <example>Contoso Copilot plugin project</example>
    [Required]
    [Description("The name of the budget")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the available funds in the budget.
    /// </summary>
    /// <example>50000</example>
    [Required]
    [Description("The available funds in the budget")]
    public decimal AvailableFunds { get; set; }
}

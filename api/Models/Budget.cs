// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the available funds in the budget.
    /// </summary>
    /// <example>50000</example>
    [Required]
    public decimal AvailableFunds { get; set; }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using BudgetTracker.Models;

namespace BudgetTracker.Services;

/// <summary>
/// Provides methods for working with budgets.
/// </summary>
public class BudgetService
{
    private readonly List<Budget> budgets =
    [
        new() { Name = "Contoso Copilot plugin project", AvailableFunds = 50000 },
        new() { Name = "Fourth Coffee lobby renovation", AvailableFunds = 12000 },
        new() { Name = "Alpine Ski House website redesign", AvailableFunds = 23000 },
    ];

    private readonly List<Transaction> transactions =
    [
        new() { BudgetName = "Contoso Copilot plugin project", Amount = -5000, ExpenseCategory = "hardware", Description = "Purchase new laptops for team" },
        new() { BudgetName = "Fourth Coffee lobby renovation", Amount = -2000, ExpenseCategory = "permits", Description = "Property survey for permit application" },
        new() { BudgetName = "Fourth Coffee lobby renovation", Amount = -7200, ExpenseCategory = "materials", Description = "Lumber and drywall for lobby" },
        new() { BudgetName = "Fourth Coffee lobby renovation", Amount = 5000, Description = "Additional funds to cover cost overruns" },
        new() { BudgetName = "Alpine Ski House website redesign", Amount = -2200, ExpenseCategory = "consultant", Description = "Consulting fee for Adele Vance (web designer)" },
        new() { BudgetName = "Alpine Ski House website redesign", Amount = -700, ExpenseCategory = "hosting", Description = "Hosting costs" },
    ];

    /// <summary>
    /// Gets a list of budgets.
    /// </summary>
    /// <param name="budgetName">Specifies a budget name to filter on.</param>
    /// <returns>A list of <see cref="Budget"/> objects.</returns>
    public List<Budget> GetBudgets(string? budgetName)
    {
        return string.IsNullOrEmpty(budgetName) ? budgets :
            budgets
                .Where(b => string.Compare(b.Name, budgetName, StringComparison.OrdinalIgnoreCase) == 0)
                .ToList();
    }

    /// <summary>
    /// Creates a budget in the service.
    /// </summary>
    /// <param name="budget">The budget to create.</param>
    /// <returns>The created budget.</returns>
    /// <exception cref="ArgumentException">Thrown if a budget with the specified name already exists.</exception>
    public Budget CreateBudget(Budget budget)
    {
        if (budgets.Any(b => string.Compare(b.Name, budget.Name, StringComparison.OrdinalIgnoreCase) == 0))
        {
            throw new ArgumentException($"Budget with name {budget.Name} already exists");
        }

        budgets.Add(budget);
        return budget;
    }

    /// <summary>
    /// Charge an amount to a budget.
    /// </summary>
    /// <param name="charge">The transaction representing the charge.</param>
    /// <returns>The remaining funds available in the budget after charging.</returns>
    public decimal ChargeBudget(Transaction charge)
    {
        charge.Amount = -charge.Amount;
        return ApplyTransaction(charge);
    }

    /// <summary>
    /// Add funds to a budget.
    /// </summary>
    /// <param name="extension">The transaction representing the funds to add.</param>
    /// <returns>The remaining funds available in the budget after adding funds.</returns>
    public decimal ExtendBudget(Transaction extension)
    {
        return ApplyTransaction(extension);
    }

    /// <summary>
    /// Get a list of transactions.
    /// </summary>
    /// <param name="budgetName">A budget name to filter on.</param>
    /// <param name="category">An expense category to filter on.</param>
    /// <returns>A list of transactions.</returns>
    public List<Transaction> GetTransactions(string? budgetName, string? category)
    {
        return transactions
                .Where(t => string.IsNullOrEmpty(budgetName) || string.Compare(t.BudgetName, budgetName, StringComparison.OrdinalIgnoreCase) == 0)
                .Where(t => string.IsNullOrEmpty(category) || string.Compare(t.ExpenseCategory, category, StringComparison.OrdinalIgnoreCase) == 0)
                .ToList();
    }

    private Budget? GetBudgetByName(string budgetName)
    {
        return budgets
            .Where(b => string.Compare(b.Name, budgetName, StringComparison.OrdinalIgnoreCase) == 0)
            .FirstOrDefault();
    }

    private decimal ApplyTransaction(Transaction transaction)
    {
        _ = transaction.BudgetName ?? throw new ArgumentException("Missing budgetName");
        var budget = GetBudgetByName(transaction.BudgetName) ?? throw new Exception($"Budget with name {transaction.BudgetName} not found");

        budget.AvailableFunds += transaction.Amount;
        transactions.Add(transaction);
        return budget.AvailableFunds;
    }
}

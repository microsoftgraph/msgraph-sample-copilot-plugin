// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace BudgetTracker.Models;

/// <summary>
/// Represents an API response.
/// </summary>
public class ApiResponse(string message)
{
    /// <summary>
    /// Gets the response message.
    /// </summary>
    /// <example>1000 has been deducted from Contoso Copilot plugin project. The budget has 49000 remaining in available funds.</example>
    /// <example>1000 has been added to Contoso Copilot plugin project. The budget has 51000 remaining in available funds.</example>
    public string Message => message;
}

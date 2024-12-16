// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;

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
    [Description("The response message")]
    public string Message => message;
}

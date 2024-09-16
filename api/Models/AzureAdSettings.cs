// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace BudgetTracker.Models;

/// <summary>
/// Represents the Azure AD settings for the application.
/// </summary>
public class AzureAdSettings
{
    /// <summary>
    /// Gets or sets the "Application (client) ID" of the app registration in Azure.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Gets or sets the client secret of the app registration in Azure.
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the "Directory (tenant) ID" of the app registration in Azure.
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the Azure AD instance.
    /// </summary>
    public string? Instance { get; set; }
}

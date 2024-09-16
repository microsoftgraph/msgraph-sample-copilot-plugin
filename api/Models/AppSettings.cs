// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace BudgetTracker.Models;

/// <summary>
/// Represents the settings for the application.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Gets or sets the server URL for the API.
    /// </summary>
    public string? ServerUrl { get; set; }

    /// <summary>
    /// Gets or sets the Azure AD settings for the API.
    /// </summary>
    public AzureAdSettings? AzureAd { get; set; }
}

/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Microsoft.Extensions.DependencyInjection;
using Piranha.Editorial.Services;

namespace Piranha.Editorial.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to register editorial services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds editorial permission services to the service collection.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddEditorialPermissions(this IServiceCollection services)
    {
        // Register the editorial permission service
        services.AddScoped<EditorialPermissionService>();
        
        return services;
    }
}

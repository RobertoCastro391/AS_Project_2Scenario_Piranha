/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Piranha.Editorial.Extensions;

/// <summary>
/// Extension methods for AuthorizationOptions to register editorial permissions.
/// </summary>
public static class AuthorizationOptionsExtensions
{
    // Admin permission constant to avoid circular dependency
    private const string AdminPermission = "piranha.admin";

    /// <summary>
    /// Adds editorial permissions to the authorization options.
    /// </summary>
    /// <param name="options">The authorization options</param>
    /// <returns>The authorization options</returns>
    public static AuthorizationOptions AddEditorialPermissions(this AuthorizationOptions options)
    {
        // Add policies following Piranha's pattern of requiring admin permission
        // Base workflow permission
        options.AddPolicy(Permissions.Workflow, policy =>
            policy.RequireClaim(AdminPermission, AdminPermission));

        options.AddPolicy(Permissions.WorkflowView, policy =>
            policy.RequireClaim(AdminPermission, AdminPermission));

        // Role-based permissions
        options.AddPolicy(Permissions.WorkflowAuthor, policy =>
            policy.RequireClaim(AdminPermission, AdminPermission));

        options.AddPolicy(Permissions.WorkflowEditor, policy =>
            policy.RequireClaim(AdminPermission, AdminPermission));

        options.AddPolicy(Permissions.WorkflowLegal, policy =>
            policy.RequireClaim(AdminPermission, AdminPermission));

        options.AddPolicy(Permissions.WorkflowDirector, policy =>
            policy.RequireClaim(AdminPermission, AdminPermission));

        // Action-based permissions
        options.AddPolicy(Permissions.WorkflowSubmitForReview, policy =>
            policy.RequireClaim(AdminPermission, AdminPermission));

        options.AddPolicy(Permissions.WorkflowApproveEditorial, policy =>
            policy.RequireClaim(AdminPermission, AdminPermission));

        options.AddPolicy(Permissions.WorkflowRejectEditorial, policy =>
            policy.RequireClaim(AdminPermission, AdminPermission));

        options.AddPolicy(Permissions.WorkflowSubmitLegal, policy =>
            policy.RequireClaim(AdminPermission, AdminPermission));

        options.AddPolicy(Permissions.WorkflowApproveLegal, policy =>
            policy.RequireClaim(AdminPermission, AdminPermission));

        options.AddPolicy(Permissions.WorkflowRejectLegal, policy =>
            policy.RequireClaim(AdminPermission, AdminPermission));

        options.AddPolicy(Permissions.WorkflowPublish, policy =>
            policy.RequireClaim(AdminPermission, AdminPermission));

        options.AddPolicy(Permissions.WorkflowUnpublish, policy =>
            policy.RequireClaim(AdminPermission, AdminPermission));

        return options;
    }
}

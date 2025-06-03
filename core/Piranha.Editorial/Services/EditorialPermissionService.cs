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
using System.Security.Claims;

namespace Piranha.Editorial.Services;

/// <summary>
/// Service for checking editorial workflow permissions.
/// </summary>
public class EditorialPermissionService
{
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="authorizationService">The authorization service</param>
    public EditorialPermissionService(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Checks if a user has permission to perform a workflow transition based on their role.
    /// </summary>
    /// <param name="user">The user principal</param>
    /// <param name="requiredRole">The role required for the transition</param>
    /// <returns>True if the user has permission</returns>
    public async Task<bool> CanPerformTransitionAsync(ClaimsPrincipal user, string requiredRole)
    {
        if (string.IsNullOrEmpty(requiredRole) || user?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        // Check if the user has the specific role-based permission
        if (Permissions.RoleToPermissionMap.TryGetValue(requiredRole, out var permission))
        {
            var result = await _authorizationService.AuthorizeAsync(user, permission);
            return result.Succeeded;
        }

        // Fallback: check if user has the role directly
        return user.IsInRole(requiredRole);
    }

    /// <summary>
    /// Checks if a user has permission to perform a specific workflow action.
    /// </summary>
    /// <param name="user">The user principal</param>
    /// <param name="action">The workflow action</param>
    /// <returns>True if the user has permission</returns>
    public async Task<bool> CanPerformActionAsync(ClaimsPrincipal user, string action)
    {
        if (string.IsNullOrEmpty(action) || user?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        // Check if the user has permission for the specific action
        if (Permissions.ActionToPermissionMap.TryGetValue(action, out var permission))
        {
            var result = await _authorizationService.AuthorizeAsync(user, permission);
            return result.Succeeded;
        }

        return false;
    }

    /// <summary>
    /// Checks if a user has basic workflow access.
    /// </summary>
    /// <param name="user">The user principal</param>
    /// <returns>True if the user has workflow access</returns>
    public async Task<bool> HasWorkflowAccessAsync(ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var result = await _authorizationService.AuthorizeAsync(user, Permissions.Workflow);
        return result.Succeeded;
    }

    /// <summary>
    /// Checks if a user can view workflow status and history.
    /// </summary>
    /// <param name="user">The user principal</param>
    /// <returns>True if the user can view workflow information</returns>
    public async Task<bool> CanViewWorkflowAsync(ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var result = await _authorizationService.AuthorizeAsync(user, Permissions.WorkflowView);
        return result.Succeeded;
    }

    /// <summary>
    /// Gets the user's workflow role based on their permissions.
    /// </summary>
    /// <param name="user">The user principal</param>
    /// <returns>The workflow role if found, null otherwise</returns>
    public async Task<string?> GetUserWorkflowRoleAsync(ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        // Check each role permission to find the user's role
        foreach (var roleMapping in Permissions.RoleToPermissionMap)
        {
            var result = await _authorizationService.AuthorizeAsync(user, roleMapping.Value);
            if (result.Succeeded)
            {
                return roleMapping.Key;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets all workflow actions that a user is permitted to perform.
    /// </summary>
    /// <param name="user">The user principal</param>
    /// <returns>List of permitted actions</returns>
    public async Task<List<string>> GetPermittedActionsAsync(ClaimsPrincipal user)
    {
        var permittedActions = new List<string>();

        if (user?.Identity?.IsAuthenticated != true)
        {
            return permittedActions;
        }

        foreach (var actionMapping in Permissions.ActionToPermissionMap)
        {
            var result = await _authorizationService.AuthorizeAsync(user, actionMapping.Value);
            if (result.Succeeded)
            {
                permittedActions.Add(actionMapping.Key);
            }
        }

        return permittedActions;
    }
}

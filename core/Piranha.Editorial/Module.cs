/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Piranha.Extend;
using Piranha.Security;

namespace Piranha.Editorial;

/// <summary>
/// The editorial module for registering workflow permissions.
/// </summary>
public sealed class Module : IModule
{
    private readonly List<PermissionItem> _permissions = new()
    {
        new PermissionItem { Name = Permissions.Workflow, Title = "Workflow Management", Category = "Editorial", IsInternal = false },
        new PermissionItem { Name = Permissions.WorkflowView, Title = "View Workflow Status", Category = "Editorial", IsInternal = false },
        new PermissionItem { Name = Permissions.WorkflowAuthor, Title = "Author Role", Category = "Editorial Roles", IsInternal = false },
        new PermissionItem { Name = Permissions.WorkflowEditor, Title = "Editor Role", Category = "Editorial Roles", IsInternal = false },
        new PermissionItem { Name = Permissions.WorkflowLegal, Title = "Legal Reviewer Role", Category = "Editorial Roles", IsInternal = false },
        new PermissionItem { Name = Permissions.WorkflowDirector, Title = "Director Role", Category = "Editorial Roles", IsInternal = false },
        new PermissionItem { Name = Permissions.WorkflowSubmitForReview, Title = "Submit for Editorial Review", Category = "Editorial Actions", IsInternal = false },
        new PermissionItem { Name = Permissions.WorkflowApproveEditorial, Title = "Approve Editorial Review", Category = "Editorial Actions", IsInternal = false },
        new PermissionItem { Name = Permissions.WorkflowRejectEditorial, Title = "Reject Editorial Review", Category = "Editorial Actions", IsInternal = false },
        new PermissionItem { Name = Permissions.WorkflowSubmitLegal, Title = "Submit for Legal Review", Category = "Editorial Actions", IsInternal = false },
        new PermissionItem { Name = Permissions.WorkflowApproveLegal, Title = "Approve Legal Review", Category = "Editorial Actions", IsInternal = false },
        new PermissionItem { Name = Permissions.WorkflowRejectLegal, Title = "Reject Legal Review", Category = "Editorial Actions", IsInternal = false },
        new PermissionItem { Name = Permissions.WorkflowPublish, Title = "Publish Content", Category = "Editorial Actions", IsInternal = false },
        new PermissionItem { Name = Permissions.WorkflowUnpublish, Title = "Unpublish Content", Category = "Editorial Actions", IsInternal = false }
    };

    /// <summary>
    /// Gets the module permissions.
    /// </summary>
    public IList<PermissionItem> PermissionItems => _permissions;

    /// <summary>
    /// Gets the module author.
    /// </summary>
    public string Author => "Piranha CMS Editorial Team";

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public string Name => "Piranha Editorial Workflow";

    /// <summary>
    /// Gets the module version.
    /// </summary>
    public string Version => Piranha.Utils.GetAssemblyVersion(this.GetType().Assembly);

    /// <summary>
    /// Gets the module description.
    /// </summary>
    public string Description => "Editorial workflow permissions for content management";

    /// <summary>
    /// Gets the module package url.
    /// </summary>
    public string PackageUrl => "https://github.com/piranhacms/piranha.core";

    /// <summary>
    /// Gets the module icon url.
    /// </summary>
    public string IconUrl => "https://piranhacms.org/assets/twitter-card.png";

    /// <summary>
    /// Initializes the module.
    /// </summary>
    public void Init()
    {
        // Register permissions
        foreach (var permission in _permissions)
        {
            App.Permissions["Editorial"].Add(permission);
        }
    }
}

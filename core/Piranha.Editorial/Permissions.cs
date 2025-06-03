/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

namespace Piranha.Editorial;

/// <summary>
/// The available editorial permissions.
/// </summary>
public static class Permissions
{
    /// <summary>
    /// Base workflow permission for accessing workflow features.
    /// </summary>
    public const string Workflow = "PiranhaWorkflow";
    
    /// <summary>
    /// Permission to view workflow status and history.
    /// </summary>
    public const string WorkflowView = "PiranhaWorkflowView";
    
    /// <summary>
    /// Role-based permission for Authors (corresponds to "Autor" role).
    /// </summary>
    public const string WorkflowAuthor = "PiranhaWorkflowAuthor";
    
    /// <summary>
    /// Role-based permission for Editors (corresponds to "Editor" role).
    /// </summary>
    public const string WorkflowEditor = "PiranhaWorkflowEditor";
    
    /// <summary>
    /// Role-based permission for Legal reviewers (corresponds to "Jurista" role).
    /// </summary>
    public const string WorkflowLegal = "PiranhaWorkflowLegal";
    
    /// <summary>
    /// Role-based permission for Directors (corresponds to "Diretor" role).
    /// </summary>
    public const string WorkflowDirector = "PiranhaWorkflowDirector";
    
    /// <summary>
    /// Permission to submit content for editorial review.
    /// </summary>
    public const string WorkflowSubmitForReview = "PiranhaWorkflowSubmitForReview";
    
    /// <summary>
    /// Permission to approve content after editorial review.
    /// </summary>
    public const string WorkflowApproveEditorial = "PiranhaWorkflowApproveEditorial";
    
    /// <summary>
    /// Permission to reject content during editorial review.
    /// </summary>
    public const string WorkflowRejectEditorial = "PiranhaWorkflowRejectEditorial";
    
    /// <summary>
    /// Permission to submit content for legal review.
    /// </summary>
    public const string WorkflowSubmitLegal = "PiranhaWorkflowSubmitLegal";
    
    /// <summary>
    /// Permission to approve content after legal review.
    /// </summary>
    public const string WorkflowApproveLegal = "PiranhaWorkflowApproveLegal";
    
    /// <summary>
    /// Permission to reject content during legal review.
    /// </summary>
    public const string WorkflowRejectLegal = "PiranhaWorkflowRejectLegal";
    
    /// <summary>
    /// Permission to publish approved content.
    /// </summary>
    public const string WorkflowPublish = "PiranhaWorkflowPublish";
    
    /// <summary>
    /// Permission to unpublish content.
    /// </summary>
    public const string WorkflowUnpublish = "PiranhaWorkflowUnpublish";

    /// <summary>
    /// Maps workflow roles to their corresponding permission claims.
    /// </summary>
    public static readonly Dictionary<string, string> RoleToPermissionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Autor"] = WorkflowAuthor,
        ["Editor"] = WorkflowEditor,
        ["Jurista"] = WorkflowLegal,
        ["Diretor"] = WorkflowDirector
    };

    /// <summary>
    /// Maps workflow actions to their corresponding permission claims.
    /// </summary>
    public static readonly Dictionary<string, string> ActionToPermissionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["submit_for_review"] = WorkflowSubmitForReview,
        ["approve_editorial"] = WorkflowApproveEditorial,
        ["reject_editorial"] = WorkflowRejectEditorial,
        ["submit_legal"] = WorkflowSubmitLegal,
        ["approve_legal"] = WorkflowApproveLegal,
        ["reject_legal"] = WorkflowRejectLegal,
        ["publish"] = WorkflowPublish,
        ["unpublish"] = WorkflowUnpublish
    };

    /// <summary>
    /// Gets all workflow permission names.
    /// </summary>
    /// <returns>Array of all permission names</returns>
    public static string[] All()
    {
        return new[]
        {
            Workflow,
            WorkflowView,
            WorkflowAuthor,
            WorkflowEditor,
            WorkflowLegal,
            WorkflowDirector,
            WorkflowSubmitForReview,
            WorkflowApproveEditorial,
            WorkflowRejectEditorial,
            WorkflowSubmitLegal,
            WorkflowApproveLegal,
            WorkflowRejectLegal,
            WorkflowPublish,
            WorkflowUnpublish
        };
    }
}

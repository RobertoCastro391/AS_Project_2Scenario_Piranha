/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Piranha.Editorial.Abstractions.Models;

namespace Piranha.Data.EF.SQLite;

[ExcludeFromCodeCoverage]
public sealed class SQLiteDb : Db<SQLiteDb>
{
    /// <summary>
    /// Gets/sets the PageEditorialStatus set.
    /// </summary>
    public DbSet<PageEditorialStatus> PageEditorialStatuses { get; set; }

    /// <summary>
    /// Gets/sets the Workflow set.
    /// </summary>
    public DbSet<Workflow> Workflows { get; set; }

    /// <summary>
    /// Gets/sets the WorkflowStage set.
    /// </summary>
    public DbSet<WorkflowStage> WorkflowStages { get; set; }

    /// <summary>
    /// Gets/sets the WorkflowTransition set.
    /// </summary>
    public DbSet<WorkflowTransition> WorkflowTransitions { get; set; }

    /// <summary>
    /// Gets/sets the ContentStateHistory set.
    /// </summary>
    public DbSet<ContentStateHistory> ContentStateHistories { get; set; }

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="options">Configuration options</param>
    public SQLiteDb(DbContextOptions<SQLiteDb> options) : base(options)
    {
    }

    /// <summary>
    /// Creates and configures the data model.
    /// </summary>
    /// <param name="mb">The current model builder</param>
    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // Configure Editorial entities
        mb.Entity<PageEditorialStatus>();
        mb.Entity<Workflow>();
        mb.Entity<WorkflowStage>();
        mb.Entity<WorkflowTransition>();
        mb.Entity<ContentStateHistory>();

        // Configure relationships
        mb.Entity<WorkflowStage>()
            .HasOne<Workflow>()
            .WithMany("Stages")
            .HasForeignKey(ws => ws.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

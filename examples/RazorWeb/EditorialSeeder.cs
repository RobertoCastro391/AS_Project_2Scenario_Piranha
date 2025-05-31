using System;
using System.Collections.Generic;
using System.Linq;
using Piranha.Data.EF.SQLite;
using Piranha.Editorial.Abstractions.Models;
using Piranha.Editorial.Abstractions.Enums;

namespace RazorWeb;

public static class EditorialSeeder
{
    public static void Seed(ExtendedSQLiteDb db)
    {
        if (!db.Workflows.Any())
        {
            var workflow = new Workflow
            {
                Id = Guid.NewGuid(),
                Name = "Workflow Académico",
                CreatedAt = DateTime.Now,
                IsActive = true,
                Stages = new List<WorkflowStage>
                {
                    new WorkflowStage
                    {
                        Name = "Rascunho",
                        Status = EditorialStatus.Draft,
                        Order = 0,
                        RoleName = "Autor",
                        Instructions = "Criação inicial do conteúdo."
                    },
                    new WorkflowStage
                    {
                        Name = "Revisão Editorial",
                        Status = EditorialStatus.EditorialReview,
                        Order = 1,
                        RoleName = "Editor",
                        Instructions = "Verifica clareza e estilo do conteúdo."
                    },
                    new WorkflowStage
                    {
                        Name = "Revisão Jurídica",
                        Status = EditorialStatus.LegalReview,
                        Order = 2,
                        RoleName = "Jurista",
                        Instructions = "Confirma conformidade legal."
                    },
                    new WorkflowStage
                    {
                        Name = "Aprovação Final",
                        Status = EditorialStatus.Approved,
                        Order = 3,
                        RoleName = "Diretor",
                        Instructions = "Aprova para publicação final."
                    },
                    new WorkflowStage
                    {
                        Name = "Publicado",
                        Status = EditorialStatus.Published,
                        Order = 4,
                        RoleName = "Diretor",
                        Instructions = "Publicado"
                    }
                }
            };

            db.Workflows.Add(workflow);
            db.SaveChanges();
        }

        if (!db.WorkflowTransitions.Any())
        {
            var workflowId = db.Workflows.First(w => w.Name == "Workflow Académico").Id;

            var transitions = new List<WorkflowTransition>
            {
                new WorkflowTransition
                {
                    Id = Guid.NewGuid(),
                    WorkflowId = workflowId,
                    FromStatus = EditorialStatus.Draft,
                    ToStatus = EditorialStatus.EditorialReview,
                    ActionName = "Submeter para Revisão Editorial",
                    RequiredRole = "Autor"
                },
                new WorkflowTransition
                {
                    Id = Guid.NewGuid(),
                    WorkflowId = workflowId,
                    FromStatus = EditorialStatus.EditorialReview,
                    ToStatus = EditorialStatus.LegalReview,
                    ActionName = "Enviar para Revisão Jurídica",
                    RequiredRole = "Editor"
                },
                new WorkflowTransition
                {
                    Id = Guid.NewGuid(),
                    WorkflowId = workflowId,
                    FromStatus = EditorialStatus.LegalReview,
                    ToStatus = EditorialStatus.Approved,
                    ActionName = "Aprovar para Publicação",
                    RequiredRole = "Jurista"
                },
                new WorkflowTransition
                {
                    Id = Guid.NewGuid(),
                    WorkflowId = workflowId,
                    FromStatus = EditorialStatus.Approved,
                    ToStatus = EditorialStatus.Published,
                    ActionName = "Publicar Conteúdo",
                    RequiredRole = "Diretor"
                },
                new WorkflowTransition
                {
                    Id = Guid.NewGuid(),
                    WorkflowId = workflowId,
                    FromStatus = EditorialStatus.EditorialReview,
                    ToStatus = EditorialStatus.Draft,
                    ActionName = "Rejeitar",
                    RequiredRole = "Editor"
                },
                new WorkflowTransition
                {
                    Id = Guid.NewGuid(),
                    WorkflowId = workflowId,
                    FromStatus = EditorialStatus.LegalReview,
                    ToStatus = EditorialStatus.Draft,
                    ActionName = "Rejeitar",
                    RequiredRole = "Jurista"
                },
                new WorkflowTransition
                {
                    Id = Guid.NewGuid(),
                    WorkflowId = workflowId,
                    FromStatus = EditorialStatus.Approved,
                    ToStatus = EditorialStatus.Draft,
                    ActionName = "Rejeitar",
                    RequiredRole = "Diretor"
                },
                new WorkflowTransition
                {
                    Id = Guid.NewGuid(),
                    WorkflowId = workflowId,
                    FromStatus = EditorialStatus.Published,
                    ToStatus = EditorialStatus.Draft,
                    ActionName = "Voltar a Rascunho",
                    RequiredRole = "Diretor"
                }
            };

            db.WorkflowTransitions.AddRange(transitions);
            db.SaveChanges();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Piranha.Data.EF.SQLite;
using Piranha.Editorial.Models;
using Piranha.Editorial.Enums;

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
                        Name = "Revisão Editorial",
                        Status = EditorialStatus.EditorialReview,
                        Order = 1,
                        RoleName = "Editor",
                        Instructions = "Verifica clareza e estilo do conteúdo."
                    },
                    new WorkflowStage
                    {
                        Name = "Rejeitado pelo Editor",
                        Status = EditorialStatus.Rejected,
                        Order = 2,
                        RoleName = "Editor",
                        Instructions = "Motivar a rejeição com observações claras."
                    },
                    new WorkflowStage
                    {
                        Name = "Revisão Jurídica",
                        Status = EditorialStatus.LegalReview,
                        Order = 3,
                        RoleName = "Jurista",
                        Instructions = "Confirma conformidade legal."
                    },
                    new WorkflowStage
                    {
                        Name = "Rejeitado pelo Jurista",
                        Status = EditorialStatus.Rejected,
                        Order = 4,
                        RoleName = "Jurista",
                        Instructions = "Especificar as bases legais da rejeição."
                    },
                    new WorkflowStage
                    {
                        Name = "Aprovação Final",
                        Status = EditorialStatus.Approved,
                        Order = 5,
                        RoleName = "Diretor",
                        Instructions = "Aprova para publicação final."
                    }
                }
            };

            db.Workflows.Add(workflow);
            db.SaveChanges();
        }
    }
}

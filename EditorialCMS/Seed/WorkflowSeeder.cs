using Editorial.Workflows.Data;
using Editorial.Workflows.Models;

namespace EditorialCMS.Seed
{
    public static class WorkflowSeeder
    {
        public static void Seed(IHost app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();

            if (!db.Workflows.Any())
            {
                var draft = new WorkflowStage { Id = Guid.NewGuid(), Name = "Draft", Order = 1 };
                var review = new WorkflowStage { Id = Guid.NewGuid(), Name = "Editorial Review", Order = 2 };
                var legal = new WorkflowStage { Id = Guid.NewGuid(), Name = "Legal Review", Order = 3 };
                var published = new WorkflowStage { Id = Guid.NewGuid(), Name = "Published", Order = 4 };

                var transitions = new[]
                {
                    new WorkflowTransition { FromStageId = draft.Id, ToStageId = review.Id },
                    new WorkflowTransition { FromStageId = review.Id, ToStageId = legal.Id },
                    new WorkflowTransition { FromStageId = legal.Id, ToStageId = published.Id }
                };

                var workflow = new Workflow
                {
                    Name = "Default Publishing Workflow",
                    Stages = new[] { draft, review, legal, published },
                };

                draft.Transitions.Add(transitions[0]);
                review.Transitions.Add(transitions[1]);
                legal.Transitions.Add(transitions[2]);

                db.Workflows.Add(workflow);
                db.SaveChanges();
            }
        }
    }
}

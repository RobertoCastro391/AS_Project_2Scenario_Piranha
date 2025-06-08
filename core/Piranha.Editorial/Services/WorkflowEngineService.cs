using System;
using System.Collections.Generic;
using System.Linq;
using Piranha.Editorial.Abstractions.Enums;
using Piranha.Editorial.Abstractions.Models;

namespace Piranha.Editorial.Services
{
    public class WorkflowEngineService
    {
        private readonly List<Workflow> _workflows;
        private readonly List<WorkflowTransition> _transitions;
        private readonly List<ContentStateHistory> _history;

        // Simula uma "base de dados na memória" para este exemplo
        public WorkflowEngineService(List<Workflow> workflows, List<WorkflowTransition> transitions)
        {
            _workflows = workflows;
            _transitions = transitions;
            _history = new List<ContentStateHistory>();
        }

        /// <summary>
        /// Tenta aplicar uma transição para um conteúdo.
        /// </summary>
        public bool TryTransition(Guid contentId, EditorialStatus currentStatus, string action, string userRole, string userId, out EditorialStatus newStatus, string? comment = null)
        {
            newStatus = currentStatus;

            // Procurar transição válida no workflow
            var transition = _transitions.FirstOrDefault(t =>
                t.FromStatus == currentStatus &&
                t.ActionName.Equals(action, StringComparison.OrdinalIgnoreCase) &&
                t.RequiredRole.Equals(userRole, StringComparison.OrdinalIgnoreCase));

            if (transition == null)
                return false; // A transição não é permitida

            // Executar a mudança de estado
            newStatus = transition.ToStatus;

            // Guardar no histórico
            _history.Add(new ContentStateHistory
            {
                Id = Guid.NewGuid(),
                ContentId = contentId,
                FromStatus = currentStatus,
                ToStatus = newStatus,
                Action = action,
                Comment = comment,
                UserId = userId,
                Timestamp = DateTime.UtcNow
            });

            return true;
        }

        /// <summary>
        /// Devolve o histórico completo de um conteúdo.
        /// </summary>
        public List<ContentStateHistory> GetHistory(Guid contentId)
        {
            return _history
                .Where(h => h.ContentId == contentId)
                .OrderBy(h => h.Timestamp)
                .ToList();
        }

        /// <summary>
        /// Devolve a lista de ações possíveis para o estado atual.
        /// </summary>
        public List<string> GetAvailableActions(EditorialStatus currentStatus, string userRole)
        {
            return _transitions
                .Where(t => t.FromStatus == currentStatus && t.RequiredRole == userRole)
                .Select(t => t.ActionName)
                .ToList();
        }
    }
}

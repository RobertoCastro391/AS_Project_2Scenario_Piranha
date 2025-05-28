using System;
using Piranha.Editorial.Enums;

namespace Piranha.Editorial.Models
{
    public class WorkflowTransition
    {
        /// <summary>
        /// Identificador único da transição.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Referência ao workflow a que esta transição pertence.
        /// </summary>
        public Guid WorkflowId { get; set; }

        /// <summary>
        /// Estado de origem (ex: EditorialReview).
        /// </summary>
        public EditorialStatus FromStatus { get; set; }

        /// <summary>
        /// Estado de destino (ex: LegalReview).
        /// </summary>
        public EditorialStatus ToStatus { get; set; }

        /// <summary>
        /// Nome da ação (ex: "Aprovar", "Rejeitar", "Publicar").
        /// Isto será usado na interface (botões de ação).
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// Papel necessário para executar esta transição.
        /// Pode ser validado em conjunto com o estado atual.
        /// </summary>
        public string RequiredRole { get; set; }
    }
}

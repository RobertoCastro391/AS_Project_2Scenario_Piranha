using System;
using Piranha.Editorial.Enums;

namespace Piranha.Editorial.Models
{
    public class WorkflowStage
    {
        /// <summary>
        /// Identificador único da etapa.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Referência ao workflow pai.
        /// </summary>
        public Guid WorkflowId { get; set; }

        /// <summary>
        /// Estado editorial representado nesta etapa.
        /// </summary>
        public EditorialStatus Status { get; set; }

        /// <summary>
        /// Ordem de execução no fluxo editorial.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Nome do papel (role) autorizado a interagir nesta etapa.
        /// Ex: "Editor", "Legal", "Publisher"
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// Instruções visuais ou comentários para o utilizador nesta etapa.
        /// </summary>
        public string? Instructions { get; set; }

        public string Name { get; set; } // ✅ NOVO
    }
}

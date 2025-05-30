using System;
using Piranha.Editorial.Abstractions.Enums;

namespace Piranha.Editorial.Abstractions.Models
{
    public class ContentStateHistory
    {
        /// <summary>
        /// Identificador único do registo de transição.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID do conteúdo (Post, Page, etc.) ao qual esta transição se refere.
        /// </summary>
        public Guid ContentId { get; set; }

        /// <summary>
        /// Estado anterior do conteúdo.
        /// </summary>
        public EditorialStatus FromStatus { get; set; }

        /// <summary>
        /// Estado seguinte após a transição.
        /// </summary>
        public EditorialStatus ToStatus { get; set; }

        /// <summary>
        /// Ação tomada (ex: "Aprovar", "Rejeitar").
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Comentário ou justificativa opcional do utilizador.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// ID do utilizador que realizou a transição.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Data e hora em que a ação foi realizada.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}

using System;
using Piranha.Editorial.Enums;

namespace Piranha.Editorial.Models
{
    public class PostEditorialStatus
    {
        public Guid Id { get; set; }

        public Guid PostId { get; set; }

        public EditorialStatus Status { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}

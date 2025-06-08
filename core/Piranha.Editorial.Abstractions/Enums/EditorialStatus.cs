using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piranha.Editorial.Abstractions.Enums
{
    public enum EditorialStatus
    {
        Draft,
        EditorialReview,
        LegalReview,
        Approved,
        Published, 
        RejectedByEditor,
        RejectedByLegal
    }
}

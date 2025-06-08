namespace Piranha.Editorial.Abstractions.Models
{
    public class PageEditorialStatusDto
    {
        public string Status { get; set; }              // Ex: "Draft", "EditorialReview"
        public Guid? CurrentStageId { get; set; }       // Etapa atual
        public string StageName { get; set; }           // Nome da etapa atual (opcional)
    }
}

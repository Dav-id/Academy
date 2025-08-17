using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Tenants
{
    public class Tenant : BaseModelNoTenant
    {
        [Required]
        [MaxLength(20)]
        public string UrlStub { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
    }
}

using Academy.Shared.Data.Models.Tenants;

using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models
{
    public abstract class BaseModelNoTenantOrId
    {
        [Required]
        public string CreatedBy { get; set; } = "System";
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string UpdatedBy { get; set; } = "System";
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public bool IsDeleted { get; set; }
    }

    public abstract class BaseModelNoId : BaseModelNoTenantOrId
    {
        [Required]
        public long TenantId { get; set; }

        public Tenant? Tenant { get; set; }
    }

    public abstract class BaseModelNoTenant : BaseModelNoTenantOrId
    {
        [Key]
        public long Id { get; set; }
    }

    public abstract class BaseModel : BaseModelNoId
    {
        [Key]
        public long Id { get; set; }
    }
}

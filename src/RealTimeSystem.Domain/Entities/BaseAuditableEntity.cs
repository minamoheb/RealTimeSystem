namespace RealTimeSystem.Domain.Entities
{
    public abstract class BaseAuditableEntity
    {
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }

        public long? CreatedBy { get; set; }

        public long? ModifiedBy { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; }
    }
}

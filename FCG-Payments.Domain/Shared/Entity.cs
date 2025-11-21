namespace FCG_Payments.Domain.Shared
{
    public abstract class Entity(Guid id) : IEquatable<Entity>
    {
        public Guid Id { get; init; } = id;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }
        public bool Equals(Entity? other)
        {
            if (other is null) return false;
            return ReferenceEquals(this, other) || Id.Equals(other.Id);
        }
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Entity)obj);
        }
        public override int GetHashCode() => Id.GetHashCode();
        public static bool operator ==(Entity? left, Entity? right) => Equals(left, right);
        public static bool operator !=(Entity? left, Entity? right) => !Equals(left, right);
        public void UpdateLastDateChanged() => UpdatedAt = DateTime.UtcNow;
    }
}

namespace CommerceMono.Modules.Core.Domain;

public abstract class AuditedEntity : AuditedEntity<int>
{
}

public abstract class AuditedEntity<TPrimaryKey> : Entity<TPrimaryKey>, IAuditedEntity<TPrimaryKey>
{
    public virtual long? CreatorUserId { get; set; }

    public virtual DateTimeOffset CreationTime { get; set; } = DateTimeOffset.Now;

    public virtual long? LastModifierUserId { get; set; }

    public virtual DateTimeOffset? LastModificationTime { get; set; } = DateTimeOffset.Now;
}

public interface IAuditedEntity : IAuditedEntity<int>
{
}

public interface IAuditedEntity<TPrimaryKey> : IEntity<TPrimaryKey>, ICreationAudited
{
}

public interface ICreationAudited
{
    long? CreatorUserId { get; set; }

    DateTimeOffset CreationTime { get; set; }

    long? LastModifierUserId { get; set; }

    DateTimeOffset? LastModificationTime { get; set; }
}

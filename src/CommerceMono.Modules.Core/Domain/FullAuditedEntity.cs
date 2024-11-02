namespace CommerceMono.Modules.Core.Domain;

public abstract class FullAuditedEntity : FullAuditedEntity<int>
{
}

public abstract class FullAuditedEntity<TPrimaryKey> : AuditedEntity<TPrimaryKey>, IFullAuditedEntity<TPrimaryKey>
{
    public virtual long? DeleterUserId { get; set; }

    public virtual DateTimeOffset? DeletionTime { get; set; }

    public virtual bool IsDeleted { get; set; }
}

public interface IFullAuditedEntity : IFullAuditedEntity<int>
{
}

public interface IFullAuditedEntity<TPrimaryKey> : IAuditedEntity<TPrimaryKey>, ISoftDelete
{
}

public interface ISoftDelete
{
    long? DeleterUserId { get; set; }

    DateTimeOffset? DeletionTime { get; set; }

    bool IsDeleted { get; set; }
}

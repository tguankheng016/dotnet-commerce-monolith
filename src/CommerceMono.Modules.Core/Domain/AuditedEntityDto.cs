namespace CommerceMono.Modules.Core.Domain;

public abstract class AuditedEntityDto : AuditedEntityDto<int, long?>
{
}

public abstract class AuditedEntityDto<TPrimaryKey> : AuditedEntityDto<TPrimaryKey, long?>
{
}

public abstract class AuditedEntityDto<TPrimaryKey, TUser> : EntityDto<TPrimaryKey>, IAuditedEntityDto<TPrimaryKey, TUser>
{
    public virtual string? CreatorUser { get; set; }

    public virtual TUser? CreatorUserId { get; set; }

    public virtual DateTimeOffset CreationTime { get; set; }

    public virtual string? LastModifierUser { get; set; }

    public virtual TUser? LastModifierUserId { get; set; }

    public virtual DateTimeOffset? LastModificationTime { get; set; }
}

public interface IAuditedEntityDto<TPrimaryKey> : IAuditedEntityDto<TPrimaryKey, long?>
{
}

public interface IAuditedEntityDto<TPrimaryKey, TUser> : IEntityDto<TPrimaryKey>
{
    string? CreatorUser { get; set; }

    TUser? CreatorUserId { get; set; }

    DateTimeOffset CreationTime { get; set; }

    string? LastModifierUser { get; set; }

    TUser? LastModifierUserId { get; set; }

    DateTimeOffset? LastModificationTime { get; set; }
}

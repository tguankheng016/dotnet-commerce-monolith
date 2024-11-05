namespace CommerceMono.Modules.Core.Domain;

public abstract class Entity : Entity<int>
{
}

public abstract class Entity<TPrimaryKey> : IEntity<TPrimaryKey>
{
	public virtual required TPrimaryKey Id { get; set; }

	public virtual long Version { get; set; }
}

public interface IEntity<TPrimaryKey> : IVersion
{
	TPrimaryKey Id { get; set; }
}

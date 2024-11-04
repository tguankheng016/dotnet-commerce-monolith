namespace CommerceMono.Modules.Core.Domain;

public class EntityDto : EntityDto<int>
{
	public EntityDto()
	{
	}

	public EntityDto(int id)
		: base(id)
	{
	}
}

public class EntityDto<TPrimaryKey> : IEntityDto<TPrimaryKey>
{
	public TPrimaryKey? Id { get; set; }

	public EntityDto()
	{
	}

	public EntityDto(TPrimaryKey id) => Id = id;
}

public interface IEntityDto : IEntityDto<int>
{
}

public interface IEntityDto<TPrimaryKey>
{
	TPrimaryKey? Id { get; set; }
}

using System.Linq.Expressions;

namespace CommerceMono.Modules.Core.Specifications;

public abstract class Specification<T> : ISpecification<T>
{
	public bool IsSatisfiedBy(T obj)
	{
		return ToExpression().Compile()(obj);
	}

	public abstract Expression<Func<T, bool>> ToExpression();
}

public interface ISpecification<T>
{
	bool IsSatisfiedBy(T obj);

	Expression<Func<T, bool>> ToExpression();
}

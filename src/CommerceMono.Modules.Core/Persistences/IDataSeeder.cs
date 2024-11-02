using CommerceMono.Modules.Core.Dependencies;

namespace CommerceMono.Modules.Core.Persistences;

public interface IDataSeeder : IScopedDependency
{
    Task SeedAllAsync();
}

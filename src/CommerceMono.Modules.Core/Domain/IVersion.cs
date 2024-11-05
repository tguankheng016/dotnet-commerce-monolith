namespace CommerceMono.Modules.Core.Domain;

// For handling optimistic concurrency
public interface IVersion
{
    long Version { get; set; }
}
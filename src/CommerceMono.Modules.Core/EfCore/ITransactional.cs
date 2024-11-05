namespace CommerceMono.Modules.Core.EFCore;

public interface ITransactional
{
}

// To Skip EF Transactional Behavior
// Self Managed
public interface INonTransactional
{
}
namespace Smurfs.Persistence;

/// <summary>Creates independent persistence sessions.</summary>
public interface IUnitOfWorkFactory
{
    /// <summary>Creates a distinct unit of work.</summary>
    /// <returns>A new session owned by the caller, who must dispose it asynchronously.</returns>
    IUnitOfWork CreateUnitOfWork();
}

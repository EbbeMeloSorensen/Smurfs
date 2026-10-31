namespace Smurfs.Persistence;

public interface IUnitOfWorkFactory
{
    IUnitOfWork CreateUnitOfWork();
}
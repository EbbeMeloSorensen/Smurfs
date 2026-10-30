namespace Smurfs.Persistence;

public interface IUnitOfWorkFactory
{
    IUnitOfWork GenerateUnitOfWork();
}
namespace Smurfs.UI.Console.API.Authentication;

internal interface ITokenStore
{
    string? Read(Uri address);

    void Save(Uri address, string token);

    void Delete(Uri address);
}

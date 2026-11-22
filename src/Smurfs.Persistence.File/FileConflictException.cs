namespace Smurfs.Persistence.File;

/// <summary>
/// Reports that a file persistence session attempted to overwrite a newer revision.
/// </summary>
public sealed class FileConflictException(string message) : IOException(message);

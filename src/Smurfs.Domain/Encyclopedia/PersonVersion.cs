namespace Smurfs.Domain.Encyclopedia;

public class PersonVersion
{
    private PersonVersion()
    {
    }

    public PersonVersion(Person person)
    {
        ArgumentNullException.ThrowIfNull(person);

        Person = person;
        PersonId = person.Id;
    }

    public Guid ArchiveId { get; private set; } = Guid.NewGuid();

    public Guid PersonId { get; private set; }

    public Person Person { get; private set; } = null!;

    public required DateTimeOffset RecordedAt { get; init; }

    public DateTimeOffset SupersededAt { get; private set; } =
        DateTimeOffset.MaxValue;

    public required DateOnly ValidFrom { get; init; }

    public DateOnly ValidTo { get; init; } = DateOnly.MaxValue;

    public string? FirstName { get; init; }

    public string? Surname { get; init; }

    public string? Nickname { get; init; }

    public Gender? Gender { get; init; }

    public string? StreetAddress { get; init; }

    public string? PostalCode { get; init; }

    public string? City { get; init; }

    public string? Country { get; init; }

    public string? PhoneNumber { get; init; }

    public string? EmailAddress { get; init; }

    public DateOnly? LatestPossibleBirthDate { get; init; }

    public DateOnly? EarliestPossibleDeathDate { get; init; }
}

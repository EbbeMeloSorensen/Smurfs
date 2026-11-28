using Smurfs.Domain.VillageLife;

namespace Smurfs.Persistence.File;

internal sealed class FileDocument
{
    public required int Version { get; init; }

    public required long Revision { get; init; }

    public required List<SmurfRecord> Smurfs { get; init; }

    public required List<PetRecord> Pets { get; init; }

    public required List<VillageRecord> Villages { get; init; }
}

internal sealed class SmurfRecord
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required DateTime CreatedUtc { get; init; }

    public required int Health { get; init; }

    public required DateTime? Birthday { get; init; }

    public required bool? Infected { get; init; }

    public required Gender? Gender { get; init; }

    public required Guid? PetId { get; init; }

    public required Guid? VillageId { get; init; }
}

internal sealed class PetRecord
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }
}

internal sealed class VillageRecord
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }
}

internal sealed class FileCatalog
{
    public long Revision { get; set; }

    public Dictionary<Guid, Smurf> Smurfs { get; } = [];

    public Dictionary<Guid, Pet> Pets { get; } = [];

    public Dictionary<Guid, Guid?> Memberships { get; } = [];

    public List<VillageRecord> Villages { get; } = [];

    public static FileCatalog Restore(FileDocument document)
    {
        if (document.Version != 1 || document.Revision < 0)
        {
            throw new InvalidDataException("Unsupported catalog version or revision.");
        }

        ArgumentNullException.ThrowIfNull(document.Smurfs);
        ArgumentNullException.ThrowIfNull(document.Pets);
        ArgumentNullException.ThrowIfNull(document.Villages);
        var catalog = new FileCatalog { Revision = document.Revision };
        var villageIds = new HashSet<Guid>();
        foreach (var village in document.Villages)
        {
            if (village.Id == Guid.Empty || village.Name is null || !villageIds.Add(village.Id))
            {
                throw new InvalidDataException("Invalid or duplicate village.");
            }

            catalog.Villages.Add(village);
        }

        foreach (var pet in document.Pets)
        {
            ArgumentNullException.ThrowIfNull(pet.Name);
            catalog.Pets.Add(pet.Id, new Pet(pet.Id) { Name = pet.Name });
        }

        foreach (var row in document.Smurfs)
        {
            if (row.CreatedUtc.Kind != DateTimeKind.Utc ||
                (row.PetId.HasValue && !catalog.Pets.ContainsKey(row.PetId.Value)) ||
                (row.VillageId.HasValue && !villageIds.Contains(row.VillageId.Value)))
            {
                throw new InvalidDataException("Invalid creation time or missing related record.");
            }

            catalog.Smurfs.Add(row.Id, new Smurf(row.Id)
            {
                Name = row.Name,
                CreatedUtc = row.CreatedUtc,
                Health = row.Health,
                Birthday = row.Birthday,
                Infected = row.Infected,
                Gender = row.Gender,
                PetId = row.PetId,
                Pet = row.PetId is Guid petId ? catalog.Pets[petId] : null
            });
            catalog.Memberships.Add(row.Id, row.VillageId);
        }

        return catalog;
    }

    public void AttachPet(Smurf smurf)
    {
        if (smurf.Pet is not null)
        {
            if (smurf.PetId.HasValue && smurf.PetId != smurf.Pet.Id)
            {
                throw new InvalidDataException("Pet and PetId refer to different pets.");
            }

            if (Pets.TryGetValue(smurf.Pet.Id, out var existing))
            {
                existing.Name = smurf.Pet.Name;
                smurf.Pet = existing;
            }
            else
            {
                Pets.Add(smurf.Pet.Id, smurf.Pet);
            }

            smurf.PetId = smurf.Pet.Id;
        }
    }

    public FileDocument Capture(long revision)
    {
        foreach (var smurf in Smurfs.Values)
        {
            AttachPet(smurf);
            if (smurf.CreatedUtc.Kind != DateTimeKind.Utc ||
                (smurf.PetId.HasValue && !Pets.ContainsKey(smurf.PetId.Value)))
            {
                throw new InvalidDataException("A Smurf needs a UTC creation time and a valid pet reference.");
            }
        }

        foreach (var pet in Pets.Values)
        {
            ArgumentNullException.ThrowIfNull(pet.Name);
        }

        return new FileDocument
        {
            Version = 1,
            Revision = revision,
            Smurfs = Smurfs.Values.Select(smurf => new SmurfRecord
            {
                Id = smurf.Id,
                Name = smurf.Name,
                CreatedUtc = smurf.CreatedUtc,
                Health = smurf.Health,
                Birthday = smurf.Birthday,
                Infected = smurf.Infected,
                Gender = smurf.Gender,
                PetId = smurf.PetId,
                VillageId = Memberships.GetValueOrDefault(smurf.Id)
            }).ToList(),
            Pets = Pets.Values.Select(pet => new PetRecord { Id = pet.Id, Name = pet.Name }).ToList(),
            Villages = Villages.ToList()
        };
    }
}

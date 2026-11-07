using AutoMapper;
using Smurfs.Application.Smurfs;
using Smurfs.Domain;

namespace Smurfs.Application.Core;

/// <summary>
/// Defines an optional AutoMapper mapping from Smurf entities to summaries.
/// </summary>
public class MappingProfiles : Profile
{
    /// <summary>
    /// Registers the entity-to-summary mapping with AutoMapper.
    /// </summary>
    public MappingProfiles()
    {
        CreateMap<Smurf, SmurfDto>();
    }
}


using AutoMapper;
using Smurfs.Application.Smurfs;
using Smurfs.Domain;

namespace Smurfs.Application.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Smurf, SmurfDto>();
    }
}


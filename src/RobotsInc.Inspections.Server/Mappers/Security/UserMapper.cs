using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using RobotsInc.Inspections.Models.Security;
using RobotsInc.Inspections.Repositories.Security;

namespace RobotsInc.Inspections.Server.Mappers.Security;

public class UserMapper : IUserMapper
{
    private readonly IUserRepository _userRepository;

    public UserMapper(
        IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <inheritdoc />
    public async Task<Inspections.API.I.Security.User> MapAsync(User model, CancellationToken cancellationToken)
    {
        Inspections.API.I.Security.User dto = new();
        await MapAsync(model, dto, cancellationToken);
        return dto;
    }

    /// <inheritdoc />
    public Task MapAsync(User model, Inspections.API.I.Security.User dto, CancellationToken cancellationToken)
    {
        dto.Id = model.Id;
        dto.Email = model.Email;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<User> MapAsync(Inspections.API.I.Security.User dto, CancellationToken cancellationToken)
    {
        User model;

        if (dto.Id != null)
        {
            User? user = await _userRepository.GetByIdAsync(dto.Id.Value, cancellationToken);
            model = user ?? throw new BadHttpRequestException("Bad id");
        }
        else
        {
            model = new();
        }

        await MapAsync(dto, model, cancellationToken);
        return model;
    }

    /// <inheritdoc />
    public Task MapAsync(Inspections.API.I.Security.User dto, User model, CancellationToken cancellationToken)
    {
        model.Email = dto.Email;
        return Task.CompletedTask;
    }
}

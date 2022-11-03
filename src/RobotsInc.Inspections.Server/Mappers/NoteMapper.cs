using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using RobotsInc.Inspections.Models;
using RobotsInc.Inspections.Repositories;

namespace RobotsInc.Inspections.Server.Mappers;

public class NoteMapper : IMapper<Note, Inspections.API.I.Note>
{
    private readonly INoteRepository _noteRepository;

    public NoteMapper(
        INoteRepository noteRepository)
    {
        _noteRepository = noteRepository;
    }

    /// <inheritdoc />
    public async Task<Inspections.API.I.Note> MapAsync(Note model, CancellationToken cancellationToken)
    {
        Inspections.API.I.Note dto = new();
        await MapAsync(model, dto, cancellationToken);
        return dto;
    }

    /// <inheritdoc />
    public Task MapAsync(Note model, Inspections.API.I.Note dto, CancellationToken cancellationToken)
    {
        dto.Id = model.Id;
        dto.Importance = model.Importance;
        dto.Summary = model.Summary;
        dto.Description = model.Description;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<Note> MapAsync(Inspections.API.I.Note dto, CancellationToken cancellationToken)
    {
        Note model;

        if (dto.Id != null)
        {
            Note? note = await _noteRepository.GetByIdAsync(dto.Id.Value, cancellationToken);
            model = note ?? throw new BadHttpRequestException("Bad id");
        }
        else
        {
            model = new();
        }

        await MapAsync(dto, model, cancellationToken);
        return model;
    }

    /// <inheritdoc />
    public Task MapAsync(Inspections.API.I.Note dto, Note model, CancellationToken cancellationToken)
    {
        model.Importance = dto.Importance;
        model.Summary = dto.Summary;
        model.Description = dto.Description;
        return Task.CompletedTask;
    }
}

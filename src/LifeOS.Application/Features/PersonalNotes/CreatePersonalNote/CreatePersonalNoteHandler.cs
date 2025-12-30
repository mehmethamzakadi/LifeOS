using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Features.PersonalNotes.GetPersonalNoteById;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;

namespace LifeOS.Application.Features.PersonalNotes.CreatePersonalNote;

public sealed class CreatePersonalNoteHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cache;

    public CreatePersonalNoteHandler(LifeOSDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<CreatePersonalNoteResponse> HandleAsync(
        CreatePersonalNoteCommand command,
        CancellationToken cancellationToken)
    {
        var personalNote = PersonalNote.Create(
            command.Title,
            command.Content,
            command.Category,
            command.IsPinned,
            command.Tags);

        await _context.PersonalNotes.AddAsync(personalNote, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Cache invalidation
        await _cache.Add(
            CacheKeys.PersonalNote(personalNote.Id),
            new GetPersonalNoteByIdResponse(
                personalNote.Id,
                personalNote.Title,
                personalNote.Content,
                personalNote.Category,
                personalNote.IsPinned,
                personalNote.Tags),
            DateTimeOffset.UtcNow.Add(CacheDurations.PersonalNote),
            null);

        await _cache.Add(
            CacheKeys.PersonalNoteGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new CreatePersonalNoteResponse(personalNote.Id);
    }
}


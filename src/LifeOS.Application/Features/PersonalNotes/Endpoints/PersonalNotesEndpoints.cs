using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.PersonalNotes.Endpoints;

public static class PersonalNotesEndpoints
{
    public static void MapPersonalNotesEndpoints(this IEndpointRouteBuilder app)
    {
        CreatePersonalNote.MapEndpoint(app);
        UpdatePersonalNote.MapEndpoint(app);
        DeletePersonalNote.MapEndpoint(app);
        GetPersonalNoteById.MapEndpoint(app);
        SearchPersonalNotes.MapEndpoint(app);
    }
}


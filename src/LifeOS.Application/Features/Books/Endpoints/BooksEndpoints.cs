using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Books.Endpoints;

public static class BooksEndpoints
{
    public static void MapBooksEndpoints(this IEndpointRouteBuilder app)
    {
        CreateBook.MapEndpoint(app);
        UpdateBook.MapEndpoint(app);
        DeleteBook.MapEndpoint(app);
        GetBookById.MapEndpoint(app);
        SearchBooks.MapEndpoint(app);
    }
}


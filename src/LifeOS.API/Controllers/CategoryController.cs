using LifeOS.Application.Features.Categories.Commands.Create;
using LifeOS.Application.Features.Categories.Commands.Delete;
using LifeOS.Application.Features.Categories.Commands.Update;
using LifeOS.Application.Features.Categories.Queries.GetAll;
using LifeOS.Application.Features.Categories.Queries.GetById;
using LifeOS.Application.Features.Categories.Queries.GetPaginatedListByDynamic;
using LifeOS.Domain.Common.Requests;
using LifeOS.Domain.Common.Responses;
using LifeOS.Domain.Constants;
using LifeOS.Domain.Services;
using LifeOS.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LifeOS.API.Controllers
{
    public class CategoryController(IMediator mediator, IAiService aiService) : BaseApiController(mediator)
    {
        [HttpPost("search")]
        [HasPermission(Permissions.CategoriesViewAll)]
        public async Task<IActionResult> Search([FromBody] DataGridRequest dataGridRequest)
        {
            PaginatedListResponse<GetPaginatedListByDynamicCategoriesResponse> response = await Mediator.Send(new GetPaginatedListByDynamicCategoriesQuery(dataGridRequest));
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await Mediator.Send(new GetAllListCategoriesQuery());
            return Ok(response);
        }

        [HttpGet("{id}")]
        [HasPermission(Permissions.CategoriesRead)]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var response = await Mediator.Send(new GetByIdCategoryQuery(id));
            return Ok(response);
        }

        [HttpPost]
        [HasPermission(Permissions.CategoriesCreate)]
        public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command)
        {
            return ToResponse(await Mediator.Send(command));
        }

        [HttpPut("{id}")]
        [HasPermission(Permissions.CategoriesUpdate)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCategoryCommand command)
        {
            if (id != command.Id)
                return BadRequest("ID mismatch");

            return ToResponse(await Mediator.Send(command));
        }

        [HttpDelete("{id}")]
        [HasPermission(Permissions.CategoriesDelete)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            return ToResponse(await Mediator.Send(new DeleteCategoryCommand(id)));
        }

        [HttpGet("generate-description")]
        [HasPermission(Permissions.CategoriesCreate)]
        public async Task<IActionResult> GenerateDescription([FromQuery] string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return BadRequest(new { message = "Kategori adı boş olamaz." });
            }

            try
            {
                var description = await aiService.GenerateCategoryDescriptionAsync(categoryName);
                return Ok(new { description });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Açıklama üretilirken bir hata oluştu.", error = ex.Message });
            }
        }
    }
}

using LifeOS.Application.Features.Dashboards.Queries.GetStatistics;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LifeOS.API.Controllers
{
    [Route("api/[controller]")]
    public class DashboardsController(IMediator mediator) : BaseApiController(mediator)
    {
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            GetStatisticsResponse response = await Mediator.Send(new GetStatisticsQuery());
            return Ok(response);
        }
    }
}

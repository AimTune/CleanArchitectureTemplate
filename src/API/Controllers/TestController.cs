using Application.Futures.Test.CreateTest;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            await _mediator.Send(new CreateTestCommand { Name = "test" });
            return Ok();
        }
    }
}

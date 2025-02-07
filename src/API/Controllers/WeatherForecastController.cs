using Application.Futures.Test.CreateTest;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(ILogger<WeatherForecastController> logger, IMediator mediator) : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    [HttpGet(Name = "GetWeatherForecast")]
    [AllowAnonymous]
    public async Task<List<WeatherForecast>> Get()
    {
        await mediator.Send(new CreateTestCommand { Name = $"WeatherForecast Example {DateTime.Now}" });
        logger.LogWarning("WeatherForecastActivity Started");
        using System.Diagnostics.Activity? activity = ActivitySourceProvider.Source.StartActivity("WeatherForecastActivity");
        activity?.AddEvent(new($"WeatherForecast {DateTime.Now}"));
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToList();
    }
}

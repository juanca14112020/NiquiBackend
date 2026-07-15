using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NiquiBackend.Application.DTOs.Calls;
using NiquiBackend.Application.Interfaces.Services;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using Twilio.Types;
using HttpMethod = Twilio.Http.HttpMethod;

namespace NiquiBackend.Controllers;

[ApiController]
[Route("api/calls")]
public class CallsController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ICustomerService _customerService;

    public CallsController(IConfiguration config, ICustomerService customerService)
    {
        _config = config;
        _customerService = customerService;
    }

    // Twilio por defecto pide esta URL con POST (no GET), asi que aceptamos ambos.
    [HttpGet("twiml/{customerId}")]
    [HttpPost("twiml/{customerId}")]
    [AllowAnonymous]
    public IActionResult GetTwiml(string customerId)
    {
        var audioUrl = _config["Twilio:AudioUrl"]!;

        var response = new VoiceResponse();
        response.Play(new Uri(audioUrl));

        return Content(response.ToString(), "application/xml");
    }

    // Usamos IFormCollection en vez de parametros individuales para evitar
    // problemas de binding estricto con los campos que manda Twilio.
    [HttpPost("status-callback")]
    [AllowAnonymous]
    public async Task<IActionResult> StatusCallback([FromQuery] string customerId)
    {
        var callStatus = Request.Form["CallStatus"].ToString();

        if (callStatus == "completed" && customerId != "test" && Guid.TryParse(customerId, out var id))
        {
            await _customerService.MarkAsCalledAsync(id);
        }

        return Ok();
    }

    [HttpPost("test-call")]
    [Authorize(Policy = "AnyAuthenticatedRole")]
    public IActionResult TestCall([FromBody] TestCallRequestDto dto)
    {
        var baseUrl = _config["PublicBaseUrl"]!.TrimEnd('/');
        var fromNumber = _config["Twilio:PhoneNumber"]!;

        var call = CallResource.Create(
            to: new PhoneNumber(dto.PhoneNumber),
            from: new PhoneNumber(fromNumber),
            url: new Uri($"{baseUrl}/api/calls/twiml/test"),
            method: HttpMethod.Get,
            statusCallback: new Uri($"{baseUrl}/api/calls/status-callback?customerId=test"),
            statusCallbackMethod: HttpMethod.Post,
            statusCallbackEvent: new List<string> { "completed" }
        );

        return Ok(new { callSid = call.Sid, status = call.Status.ToString() });
    }
}
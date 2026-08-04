using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NiquiBackend.Application.DTOs.Calls;
using NiquiBackend.Application.Interfaces.Services;
using NiquiBackend.Infrastructure.Common;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using Twilio.TwiML.Voice;
using Twilio.Types;
using HttpMethod = Twilio.Http.HttpMethod;
using Task = System.Threading.Tasks.Task;

namespace NiquiBackend.Controllers;

[ApiController]
[Route("api/calls")]
public class CallsController : ControllerBase
{
    private const int MaxAttempts = 3;

    private readonly IConfiguration _config;
    private readonly ICustomerService _customerService;

    public CallsController(IConfiguration config, ICustomerService customerService)
    {
        _config = config;
        _customerService = customerService;
    }

    private string BaseUrl => _config["PublicBaseUrl"]!.TrimEnd('/');

    [HttpGet("twiml/{customerId}")]
    [HttpPost("twiml/{customerId}")]
    [AllowAnonymous]
    public IActionResult GetTwiml(string customerId)
    {
        var response = new VoiceResponse();
        
        response.Play(new Uri(_config["Twilio:Audio1Url"]!));
        var gather = BuildGather($"gather/{customerId}/audio1", attempt: 1);
        response.Append(gather);
        response.Hangup();

        return Content(response.ToString(), "application/xml");
    }

    [HttpPost("gather/{customerId}/audio1")]
    [AllowAnonymous]
    public async Task<IActionResult> GatherAudio1(string customerId, [FromQuery] int attempt = 1)
    {
        Request.Form.TryGetValue("SpeechResult", out var speechResultValue);
        var speechResult = speechResultValue.ToString();

        var response = new VoiceResponse();

        if (string.IsNullOrWhiteSpace(speechResult))
        {
            response.Hangup();
            await MarkResult(customerId, isApproved: false);
            return Content(response.ToString(), "application/xml");
        }

        var answer = SpeechYesNoParser.Parse(speechResult);

        if (answer == true)
        {
            response.Play(new Uri(_config["Twilio:Audio3Url"]!));
            var gather = BuildGather($"gather/{customerId}/audio3", attempt: 1);
            response.Append(gather);
            response.Hangup();
        }
        else if (answer == false)
        {
            response.Play(new Uri(_config["Twilio:Audio5Url"]!));
            response.Hangup();
            await MarkResult(customerId, isApproved: false);
        }
        else if (attempt < MaxAttempts)
        {
            response.Play(new Uri(_config["Twilio:Audio1Url"]!));
            var gather = BuildGather($"gather/{customerId}/audio1", attempt: attempt + 1);
            response.Append(gather);
            response.Hangup();
        }
        else
        {
            response.Hangup();
            await MarkResult(customerId, isApproved: false);
        }

        return Content(response.ToString(), "application/xml");
    }

    [HttpPost("gather/{customerId}/audio2")]
    [AllowAnonymous]
    public async Task<IActionResult> GatherAudio2(string customerId, [FromQuery] int attempt = 1)
    {
        Request.Form.TryGetValue("SpeechResult", out var speechResultValue);
        var speechResult = speechResultValue.ToString();
        
        var response = new VoiceResponse();

        if (string.IsNullOrWhiteSpace(speechResult))
        {
            response.Hangup();
            await MarkResult(customerId, isApproved: false);
            return Content(response.ToString(), "application/xml");
        }

        var answer = SpeechYesNoParser.Parse(speechResult);

        if (answer == true)
        {
            response.Play(new Uri(_config["Twilio:Audio3Url"]!));
            var gather = BuildGather($"gather/{customerId}/audio3", attempt: 1);
            response.Append(gather);
            response.Hangup();
        }
        else if (attempt < MaxAttempts)
        {
            response.Play(new Uri(_config["Twilio:Audio2Url"]!));
            var gather = BuildGather($"gather/{customerId}/audio2", attempt: attempt + 1);
            response.Append(gather);
            response.Hangup();
        }
        else
        {
            response.Hangup();
            await MarkResult(customerId, isApproved: false);
        }

        return Content(response.ToString(), "application/xml");
    }

    [HttpPost("gather/{customerId}/audio3")]
    [AllowAnonymous]
    public async Task<IActionResult> GatherAudio3(string customerId, [FromQuery] int attempt = 1)
    {
        Request.Form.TryGetValue("SpeechResult", out var speechResultValue);
        var speechResult = speechResultValue.ToString();
        
        var response = new VoiceResponse();

        if (string.IsNullOrWhiteSpace(speechResult))
        {
            response.Hangup();
            await MarkResult(customerId, isApproved: false);
            return Content(response.ToString(), "application/xml");
        }

        var answer = SpeechYesNoParser.Parse(speechResult);

        if (answer == true)
        {
            response.Play(new Uri(_config["Twilio:Audio4Url"]!));
            response.Hangup();
            await MarkResult(customerId, isApproved: true);
        }
        else if (answer == false || attempt >= MaxAttempts)
        {
            response.Play(new Uri(_config["Twilio:Audio5Url"]!));
            response.Hangup();
            await MarkResult(customerId, isApproved: false);
        }
        else
        {
            response.Play(new Uri(_config["Twilio:Audio3Url"]!));
            var gather = BuildGather($"gather/{customerId}/audio3", attempt: attempt + 1);
            response.Append(gather);
            response.Hangup();
        }

        return Content(response.ToString(), "application/xml");
    }

    [HttpPost("status-callback")]
    [AllowAnonymous]
    public async Task<IActionResult> StatusCallback([FromQuery] string customerId)
    {
        Request.Form.TryGetValue("CallStatus", out var callStatusValue);
        var callStatus = callStatusValue.ToString();

        if (callStatus == "completed" && customerId != "test" && Guid.TryParse(customerId, out var id))
        {
            await _customerService.MarkAsCalledAsync(id);
        }

        return Ok();
    }

    private Gather BuildGather(string relativePath, int attempt)
    {
        return new Gather(
            input: new List<Gather.InputEnum> { Gather.InputEnum.Speech },
            action: new Uri($"{BaseUrl}/api/calls/{relativePath}?attempt={attempt}"),
            method: HttpMethod.Post,
            language: Gather.LanguageEnum.EsMx,
            hints: "sí,si,no,está bien,esta bien,claro,claro que sí,claro que si,por supuesto,ajá,aja,sizas,sisas,de una,hágale,hagale,firme,obvio,listo,copiado,gracias,sí gracias,si gracias,listo gracias,de una gracias,bueno,ah bueno,vale,ok,si por favor,no señor,no senor,no señora,no senora,no gracias,nanay,qué va,que va,ni de fundas,nada que ver",
            speechTimeout: "4",
            actionOnEmptyResult: true
        );
    }

    private async Task MarkResult(string customerId, bool isApproved)
    {
        if (customerId != "test" && Guid.TryParse(customerId, out var id))
        {
            await _customerService.MarkCallResultAsync(id, isApproved);
        }
    }
}
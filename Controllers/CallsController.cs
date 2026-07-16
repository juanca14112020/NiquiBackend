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

      // Entrada de la llamada: Audio 1 + pregunta
      [HttpGet("twiml/{customerId}")]
      [HttpPost("twiml/{customerId}")]
      [AllowAnonymous]
      public IActionResult GetTwiml(string customerId)
      {
          var response = new VoiceResponse();
          var gather = BuildGather($"gather/{customerId}/audio1", attempt: 1);
          gather.Play(new Uri(_config["Twilio:Audio1Url"]!));
          response.Append(gather);
          response.Hangup();

          return Content(response.ToString(), "application/xml");
      }

      // Respuesta al Audio 1: SI -> Audio 3 | NO -> Audio 2 | silencio -> cuelga | confuso -> reintenta
      [HttpPost("gather/{customerId}/audio1")]
      [AllowAnonymous]
      public IActionResult GatherAudio1(string customerId, [FromQuery] int attempt = 1)
      {
          var speechResult = Request.Form["SpeechResult"].ToString();
          var response = new VoiceResponse();

          // Silencio total (nadie hablo dentro del timeout) -> cuelga de inmediato, sin reintentos
          if (string.IsNullOrWhiteSpace(speechResult))
          {
              response.Hangup();
              return Content(response.ToString(), "application/xml");
          }

          var answer = SpeechYesNoParser.Parse(speechResult);

          if (answer == true)
          {
              var gather = BuildGather($"gather/{customerId}/audio3", attempt: 1);
              gather.Play(new Uri(_config["Twilio:Audio3Url"]!));
              response.Append(gather);
              response.Hangup();
          }
          else if (answer == false)
          {
              var gather = BuildGather($"gather/{customerId}/audio2", attempt: 1);
              gather.Play(new Uri(_config["Twilio:Audio2Url"]!));
              response.Append(gather);
              response.Hangup();
          }
          else if (attempt < MaxAttempts)
          {
              // Respuesta no reconocida -> repetimos el Audio 1 (cuenta como un intento mas)
              var gather = BuildGather($"gather/{customerId}/audio1", attempt: attempt + 1);
              gather.Play(new Uri(_config["Twilio:Audio1Url"]!));
              response.Append(gather);
              response.Hangup();
          }
          else
          {
              // Se agotaron los intentos sin entender la respuesta -> cuelga
              response.Hangup();
          }

          return Content(response.ToString(), "application/xml");
      }

      // Respuesta al Audio 2: SI -> Audio 3 | NO/confuso -> repite Audio 2 (hasta el limite) | silencio -> cuelga
      [HttpPost("gather/{customerId}/audio2")]
      [AllowAnonymous]
      public async Task<IActionResult> GatherAudio2(string customerId, [FromQuery] int attempt = 1)
      {
          var speechResult = Request.Form["SpeechResult"].ToString();
          var response = new VoiceResponse();

          if (string.IsNullOrWhiteSpace(speechResult))
          {
              response.Hangup();
              return Content(response.ToString(), "application/xml");
          }

          var answer = SpeechYesNoParser.Parse(speechResult);

          if (answer == true)
          {
              var gather = BuildGather($"gather/{customerId}/audio3", attempt: 1);
              gather.Play(new Uri(_config["Twilio:Audio3Url"]!));
              response.Append(gather);
              response.Hangup();
          }
          else if (attempt < MaxAttempts)
          {
              // "no" o respuesta confusa -> repite el Audio 2 (cuenta como intento)
              var gather = BuildGather($"gather/{customerId}/audio2", attempt: attempt + 1);
              gather.Play(new Uri(_config["Twilio:Audio2Url"]!));
              response.Append(gather);
              response.Hangup();
          }
          else
          {
              // Se agotaron los intentos diciendo que no -> se toma como respuesta definitiva negativa
              response.Hangup();
              await MarkResult(customerId, isApproved: false);
          }

          return Content(response.ToString(), "application/xml");
      }

      // Respuesta al Audio 3 (pregunta final): SI -> Audio 4 (aprobado) | NO/confuso -> Audio 5 (no aprobado)
      [HttpPost("gather/{customerId}/audio3")]
      [AllowAnonymous]
      public async Task<IActionResult> GatherAudio3(string customerId, [FromQuery] int attempt = 1)
      {
          var speechResult = Request.Form["SpeechResult"].ToString();
          var response = new VoiceResponse();

          if (string.IsNullOrWhiteSpace(speechResult))
          {
              response.Hangup();
              return Content(response.ToString(), "application/xml");
          }

          var answer = SpeechYesNoParser.Parse(speechResult);

          if (answer == true)
          {
              response.Play(new Uri(_config["Twilio:Audio4Url"]!));
              await MarkResult(customerId, isApproved: true);
              response.Hangup();
          }
          else if (answer == false || attempt >= MaxAttempts)
          {
              response.Play(new Uri(_config["Twilio:Audio5Url"]!));
              await MarkResult(customerId, isApproved: false);
              response.Hangup();
          }
          else
          {
              // Respuesta confusa y todavia quedan intentos -> repite el Audio 3
              var gather = BuildGather($"gather/{customerId}/audio3", attempt: attempt + 1);
              gather.Play(new Uri(_config["Twilio:Audio3Url"]!));
              response.Append(gather);
              response.Hangup();
          }

          return Content(response.ToString(), "application/xml");
      }

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
          var fromNumber = _config["Twilio:PhoneNumber"]!;

          var call = CallResource.Create(
              to: new PhoneNumber(dto.PhoneNumber),
              from: new PhoneNumber(fromNumber),
              url: new Uri($"{BaseUrl}/api/calls/twiml/test"),
              method: HttpMethod.Get,
              statusCallback: new Uri($"{BaseUrl}/api/calls/status-callback?customerId=test"),
              statusCallbackMethod: HttpMethod.Post,
              statusCallbackEvent: new List<string> { "completed" }
          );

          return Ok(new { callSid = call.Sid, status = call.Status.ToString() });
      }

      // timeout=3 -> si el cliente no dice nada en 3 segundos, Twilio corta el Gather
      // y Twilio igual pega a nuestra action (con SpeechResult vacio), donde colgamos.
      private Gather BuildGather(string relativePath, int attempt)
      {
          return new Gather(
              input: new List<Gather.InputEnum> { Gather.InputEnum.Speech },
              action: new Uri($"{BaseUrl}/api/calls/{relativePath}?attempt={attempt}"),
              method: HttpMethod.Post,
              language: Gather.LanguageEnum.EsMx,
              hints: "sí,si,no,claro,correcto,negativo",
              speechTimeout: "auto",
              timeout: 3
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
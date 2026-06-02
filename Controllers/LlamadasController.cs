using Microsoft.AspNetCore.Mvc;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Twilio.TwiML;

namespace NiquiBackedn.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LlamadasController : ControllerBase
    {
        private readonly ILogger<LlamadasController> _logger;
        private readonly IConfiguration _configuration; // <--- Agregamos la configuración

        public LlamadasController(ILogger<LlamadasController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration; // <--- Inyectamos
        }

        [HttpPost("iniciar")]
        public IActionResult IniciarLlamada([FromBody] SolicitudLlamadaTest solicitud)
        {
            _logger.LogInformation("Iniciando llamada de prueba con twilio hacia: {Numero}", solicitud.NumeroDestino);

            string accountSid = _configuration["Twilio:AccountSid"] ?? string.Empty;
            string authToken = _configuration["Twilio:AuthToken"] ?? string.Empty;
            string twilioPhoneNumber = _configuration["Twilio:PhoneNumber"] ?? string.Empty;

            TwilioClient.Init(accountSid, authToken);

            try
            {
                string urlRaw = "https://evasive-skedaddle-kinsman.ngrok-free.dev/api/Llamadas/Instrucciones";
                string urlLimpia = urlRaw.Trim().Replace("\u00A0", "").Replace(" ", "");

                var call = CallResource.Create(
                    to: new PhoneNumber(solicitud.NumeroDestino),
                    from: new PhoneNumber(twilioPhoneNumber),
                    url: new Uri(urlLimpia)
                );

                return Ok(new { mensaje = "La llamada fue enviada a la red de Twilio", callSid = call.Sid, estado = call.Status });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falló la creación de la llamada en Twilio");
                return StatusCode(500, $"Error interno de Twilio: {ex.Message}");
            }
        }

        [HttpPost("Instrucciones")]
        public IActionResult DarInstruccionesVoz()
        {
            var response = new VoiceResponse();
            response.Say("Hola, esta es una prueba directa desde tu controlador en punto net.", voice: "Polly.Mia", language: "es-MX");
            return Content(response.ToString(), "application/xml");
        }
    }

    public class SolicitudLlamadaTest
    {
        public string NumeroDestino { get; set; } = string.Empty;
    }
}
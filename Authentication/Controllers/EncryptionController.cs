using BlindIdea.API.Infrastructure.Encryption;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlindIdea.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EncryptionController : ControllerBase
    {
        [HttpGet("test-encryption")]
        public IActionResult TestEncryption([FromServices] EncryptionService enc)
        {
            var original = "Hello BlindIdea!";
            var encrypted = enc.Encrypt(original);
            var decrypted = enc.Decrypt(encrypted);

            return Ok(new
            {
                Original = original,
                Encrypted = encrypted,
                Decrypted = decrypted,
                Match = original == decrypted
            });
        }
    }
}

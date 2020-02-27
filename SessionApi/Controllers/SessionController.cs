using Microsoft.AspNetCore.Mvc;
using SessionLib;
using SharedModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SessionApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private ISessionService _sessionService;

        public SessionController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        /// <summary>
        /// Creates a new session storing the key/json values
        /// submitted and returns a new key for it.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Post(
            [FromBody] ICollection<SessionKeyJsonValue> values)
        {
            try
            {
                var key = await _sessionService.Create(values);
                return Ok(key);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new EmptyResult();
            }
        }

        /// <summary>
        /// Retrieves existing session values for a given key.
        /// </summary>
        [HttpGet("{key}")]
        public async Task<ActionResult> Get(string key)
        {
            try
            {
                var obj = await _sessionService.Get(key);

                if (obj == null)
                    return NotFound();

                return Ok(obj);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new EmptyResult();
            }
        }

        /// <summary>
        /// Deletes an existing session.
        /// </summary>
        [HttpDelete("{key}")]
        public async Task<ActionResult> Delete(string key)
        {
            try
            {
                await _sessionService.Delete(key);

                return Ok();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new EmptyResult();
            }
        }
    }
}

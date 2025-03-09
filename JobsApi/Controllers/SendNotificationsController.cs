using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobsApi.Models;
using JobsApi.Services;
using Microsoft.Extensions.Options;

namespace JobsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SendNotificationsController : ControllerBase
    {
        private readonly JobsApiContext _context;
        private readonly HttpService httpService;
        private readonly AppSettings appSettings;

        public SendNotificationsController(JobsApiContext context, HttpService httpService, IOptions<AppSettings> appSettings)
        {
            _context = context;
            this.httpService = httpService;
            this.appSettings = appSettings.Value;
        }

        // GET: api/SendNotifications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SendNotification>>> GetSendNotifications()
        {
            return await _context.SendNotifications.ToListAsync();
        }

        // GET: api/SendNotifications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SendNotification>> GetSendNotification(int id)
        {
            var sendNotification = await _context.SendNotifications.FindAsync(id);

            if (sendNotification == null)
            {
                return NotFound();
            }

            return sendNotification;
        }

        // PUT: api/SendNotifications/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSendNotification(int id, SendNotification sendNotification)
        {
            if (id != sendNotification.Id)
            {
                return BadRequest();
            }

            _context.Entry(sendNotification).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SendNotificationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/SendNotifications
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SendNotification>> PostSendNotification(SendNotification sendNotification)
        {
            try
            {
                _context.SendNotifications.Add(sendNotification);
                await _context.SaveChangesAsync();
                var state = _context.Entry(sendNotification).State;
                await TriggerNotificationAZFunction(sendNotification.Type);
                return CreatedAtAction("GetSendNotification", new { id = sendNotification.Id }, sendNotification);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // DELETE: api/SendNotifications/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSendNotification(int id)
        {
            var sendNotification = await _context.SendNotifications.FindAsync(id);
            if (sendNotification == null)
            {
                return NotFound();
            }

            _context.SendNotifications.Remove(sendNotification);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SendNotificationExists(int id)
        {
            return _context.SendNotifications.Any(e => e.Id == id);
        }

        private async Task TriggerNotificationAZFunction(string notitificationtype)
        {
            switch (notitificationtype)
            {
                case "whatsup":
                    await this.httpService.TriggerFunction(this.appSettings.azureFunctions!.WhatsupFunctionUrl);
                    break;
                case "mail":
                    await this.httpService.TriggerFunction(this.appSettings.azureFunctions!.EmailFunctionUrl);
                    break;
            }

        }
    }
}

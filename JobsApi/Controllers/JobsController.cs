using JobsApi.Models;
using JobsApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.ComponentModel.Design;
using System.Linq;

namespace JobsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly JobsApiContext _context;
        private readonly HttpService httpService;
        private readonly AppSettings appSettings;

        public JobsController(IServiceProvider serviceProvider, JobsApiContext context, HttpService httpService, IOptions<AppSettings> appSettings)
        {
            this.serviceProvider = serviceProvider;
            _context = context;
            this.httpService = httpService;
            this.appSettings = appSettings.Value;
        }

        // GET: api/Jobs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobs()
        {
            return await _context.Jobs.ToListAsync();
        }

        // GET: api/Jobs
        [HttpGet("user/{userid}")]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobsByUser(int userid)
        {
            var user = await _context.Users.FindAsync(userid);
            if (string.IsNullOrEmpty(user!.Location))
                return await _context.Jobs.ToListAsync();
            else
                return await _context.Jobs.AsNoTracking()
                    .Where(x => x.Location.Contains(user.Location))
                    .ToListAsync();

        }

        // GET: api/jobs/organisation/5
        [HttpGet("organisation/{orgId}")]
        public async Task<IActionResult> GetJobsByOrganization(int orgId)
        {
            if (orgId <= 0)
            {
                return BadRequest("Invalid organization ID provided.");
            }

            List<Job> jobs;
            try
            {
                jobs = await _context.Jobs
                                         .Where(job => job.UserId == orgId || job.UserId == 5)
                                         .ToListAsync();

                if (jobs.Count == 0)
                {
                    return NotFound($"No jobs found for the organization ID: {orgId}");
                }

            }
            catch (Exception ex)
            {
                // Implement logging as necessary
                return StatusCode(500, "A problem occurred while handling your request.");
            }

            return Ok(jobs);  // Return HTTP 200 with the list of jobs DTOs
        }

        // GET: api/Jobs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Job>> GetJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
            {
                return NotFound();
            }

            return job;
        }

        // PUT: api/Jobs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutJob(int id, Job job)
        {
            if (id != job.Id)
            {
                return BadRequest();
            }

            _context.Entry(job).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JobExists(id))
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

        // POST: api/Jobs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Job>> PostJob(Job job)
        {
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            _ = Task.Run(async () =>
            {
                using var scope = this.serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<JobsApiContext>();
                var httpService = scope.ServiceProvider.GetRequiredService<HttpService>();
                await CreateAlerts(context, httpService, job, this.appSettings);

            });
            return CreatedAtAction("GetJob", new { id = job.Id }, job);
        }

        // DELETE: api/Jobs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{jobId}/matched-profiles")]
        public async Task<IActionResult> GetMatchedProfiles(int jobId, [FromQuery] int page = 1, [FromQuery] int pageSize = 5, [FromQuery] string search = "")
        {
            var job = await _context.Jobs.FindAsync(jobId);
            var totalCount = await _context.Users.CountAsync(u => u.Qualification == job!.Qualification); // Total candidates for the job
            var profiles = _context.Users
                 .AsNoTracking() // Skip change tracking for better performance
                .Where(u => u.Qualification == job!.Qualification);
            if (!string.IsNullOrEmpty(search))
            {
                profiles = profiles.Where(u => u.Description!.Contains(search) || u.Location!.Contains(search) || u.Name!.Contains(search));
            }
            var matchedProfiles = await profiles
                  .OrderByDescending(x => x.LastUpdatedTime)
                  .Skip((page - 1) * pageSize)
                  .Take(pageSize)
                  .ToListAsync();

            return Ok(new { candidates = matchedProfiles, totalCount });
        }

        private bool JobExists(int id)
        {
            return _context.Jobs.Any(e => e.Id == id);
        }

        private async Task CreateAlerts(JobsApiContext context, HttpService httpService, Job job, AppSettings appSettings)
        {
            try
            {
                // Fetch subscribed users
                var subscribedUsers = await context.Users
                    .AsNoTracking()
                    .Where(x => x.Qualification!.Equals(job.Qualification) &&
                                x.IsSubscribed &&
                                x.OrganisationName == null)
                    .Select(x => new { x.Id, x.SubscriptionType })
                    .ToListAsync();

                // Prepare and insert notifications in bulk
                var notifications = subscribedUsers.Select(user => new SendNotification
                {
                    UserId = user.Id,
                    JobId = job.Id,
                    Type = user.SubscriptionType!,
                    IsNotificationSent = false
                }).ToList();

                context.SendNotifications.AddRange(notifications);
                await context.SaveChangesAsync();

                // Trigger Azure functions in parallel
                Task triggerWhatsup = httpService.TriggerFunction(this.appSettings.azureFunctions!.WhatsupFunctionUrl);
                Task triggerEmail = httpService.TriggerFunction(this.appSettings.azureFunctions!.EmailFunctionUrl);

                await Task.WhenAll(triggerWhatsup, triggerEmail);
            }
            catch (Exception ex)
            {
                // Log exception (use a proper logging framework!)
                Console.WriteLine($"Error occurred in CreateAlerts: {ex.Message}");
            }
        }
    }
}

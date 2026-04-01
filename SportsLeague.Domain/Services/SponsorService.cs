using Microsoft.Extensions.Logging;

using SportsLeague.Domain.Entities;

using SportsLeague.Domain.Interfaces.Repositories;

using SportsLeague.Domain.Interfaces.Services;
 
using System.Text.RegularExpressions;

using System.Threading.Tasks;


namespace SportsLeague.Domain.Services
{

    public class SponsorService : ISponsorService
    {
        private readonly ISponsorRepository _sponsorRepository;
        private readonly ILogger<SponsorService> _logger;

        public SponsorService(ISponsorRepository sponsorRepository, ILogger<SponsorService> logger)
        {
            _sponsorRepository = sponsorRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Sponsor>> GetAllAsync()

        {

            _logger.LogInformation("Retrieving all Sponsors");

            return await _sponsorRepository.GetAllAsync();

        }
        public async Task<Sponsor?> GetByIdAsync(int id)

        {

            _logger.LogInformation("Retrieving sponsor with ID: {SponsorId}", id);

            var sponsor = await _sponsorRepository.GetByIdAsync(id);

            if (sponsor == null)

                _logger.LogWarning("Sponsor with ID {SponsorId} not found", id);

            return sponsor;

        }
        public async Task<Sponsor> CreateAsync(Sponsor sponsor)
        {
            _logger.LogInformation("Creating sponsor: {Name}", sponsor.Name);

            //validate that there are no empty fields
            if (string.IsNullOrWhiteSpace(sponsor.Name))
            {
                throw new InvalidOperationException("Name is required");
            }

            //Validate emaiL
            if (!Regex.IsMatch(sponsor.ContactEmail, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
            {
                throw new InvalidOperationException("Invalid email format");
            }

            var exists = await _sponsorRepository.ExistsByNameAsync(sponsor.Name);
            // Validate duplicate
            if (exists)
            {
                throw new InvalidOperationException("Sponsor name already exists");
            }

            //Create sponsor
            var created = await _sponsorRepository.CreateAsync(sponsor);

            return created;
        }
        public async Task UpdateAsync(int id, Sponsor sponsor)
        {
            _logger.LogInformation("Updating sponsor with ID: {SponsorId}", id);

            //Validate existence
            var existing = await _sponsorRepository.GetByIdAsync(id);

            if (existing == null)
            {
                throw new KeyNotFoundException("Sponsor not found");
            }
            if (string.IsNullOrWhiteSpace(sponsor.Name))
            {
                throw new InvalidOperationException("Name is required");
            }

            //Validate email
            if (!Regex.IsMatch(sponsor.ContactEmail, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
            {
                throw new InvalidOperationException("Invalid email format");
            }

            // Validate duplicate ONLY if the name changes
            if (existing.Name != sponsor.Name)
            {
                var exists = await _sponsorRepository.ExistsByNameAsync(sponsor.Name);

                if (exists)
                {
                    throw new InvalidOperationException("Sponsor name already exists");
                }
            }

            //Update fields
            existing.Name = sponsor.Name;
            existing.ContactEmail = sponsor.ContactEmail;
            existing.Phone = sponsor.Phone;
            existing.WebsiteUrl = sponsor.WebsiteUrl;
            existing.Category = sponsor.Category;

            //Save changes
            await _sponsorRepository.UpdateAsync(existing);
        }
        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting sponsor with ID: {SponsorId}", id);

            //Validate existence
            var existing = await _sponsorRepository.GetByIdAsync(id);

            if (existing == null)
            {
                throw new KeyNotFoundException("Sponsor not found");
            }

            //Delete sponsor
            await _sponsorRepository.DeleteAsync(id);
        }
    }
}



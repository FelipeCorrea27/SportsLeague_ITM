using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;
using System.Text.RegularExpressions;

namespace SportsLeague.Domain.Services
{
    public class SponsorService : ISponsorService
    {
        private readonly ISponsorRepository _sponsorRepository;
        private readonly ITournamentRepository _tournamentRepository;
        private readonly ITournamentSponsorRepository _tournamentSponsorRepository;
        private readonly ILogger<SponsorService> _logger;

        public SponsorService(
            ISponsorRepository sponsorRepository,
            ITournamentRepository tournamentRepository,
            ITournamentSponsorRepository tournamentSponsorRepository,
            ILogger<SponsorService> logger)
        {
            _sponsorRepository = sponsorRepository;
            _tournamentRepository = tournamentRepository;
            _tournamentSponsorRepository = tournamentSponsorRepository;
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

            if (string.IsNullOrWhiteSpace(sponsor.Name))
                throw new InvalidOperationException("Name is required");
            sponsor.Name = sponsor.Name.Trim();
            var normalizedName = sponsor.Name.Replace(" ", "").ToLower();

            if (!Regex.IsMatch(sponsor.ContactEmail, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
                throw new InvalidOperationException("Invalid email format");

            var exists = await _sponsorRepository.ExistsByNameAsync(sponsor.Name);

            if (exists)
                throw new InvalidOperationException("Sponsor name already exists");

            return await _sponsorRepository.CreateAsync(sponsor);
        }

        public async Task UpdateAsync(int id, Sponsor sponsor)
        {
            _logger.LogInformation("Updating sponsor with ID: {SponsorId}", id);

            var existing = await _sponsorRepository.GetByIdAsync(id);

            if (existing == null)
                throw new KeyNotFoundException("Sponsor not found");

            if (string.IsNullOrWhiteSpace(sponsor.Name))
                throw new InvalidOperationException("Name is required");

            sponsor.Name = sponsor.Name.Trim();
            var normalizedName = sponsor.Name.Replace(" ", "").ToLower();

            if (!Regex.IsMatch(sponsor.ContactEmail, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
                throw new InvalidOperationException("Invalid email format");



            existing.Name = sponsor.Name;
            existing.ContactEmail = sponsor.ContactEmail;
            existing.Phone = sponsor.Phone;
            existing.WebsiteUrl = sponsor.WebsiteUrl;
            existing.Category = sponsor.Category;

            await _sponsorRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting sponsor with ID: {SponsorId}", id);

            var existing = await _sponsorRepository.GetByIdAsync(id);

            if (existing == null)
                throw new KeyNotFoundException("Sponsor not found");

            await _sponsorRepository.DeleteAsync(id);
        }

        //VINCULAR
        public async Task<TournamentSponsor> LinkToTournamentAsync(int sponsorId, int tournamentId, decimal contractAmount)
        {
            _logger.LogInformation("Linking Sponsor {SponsorId} to Tournament {TournamentId}", sponsorId, tournamentId);

            var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);
            if (sponsor == null)
                throw new KeyNotFoundException("Sponsor not found");

            var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
            if (tournament == null)
                throw new KeyNotFoundException("Tournament not found");

            var exists = await _tournamentSponsorRepository
                .ExistsByTournamentAndSponsorAsync(tournamentId, sponsorId);

            if (exists)
                throw new InvalidOperationException("Sponsor already linked to this tournament");

            if (contractAmount <= 0)
                throw new InvalidOperationException("Contract amount must be greater than 0");

            var relation = new TournamentSponsor
            {
                SponsorId = sponsorId,
                TournamentId = tournamentId,
                ContractAmount = contractAmount,
                JoinedAt = DateTime.UtcNow
            };

            return await _tournamentSponsorRepository.CreateAsync(relation);
        }

        //DESVINCULAR
        public async Task UnlinkFromTournamentAsync(int sponsorId, int tournamentId)
        {
            _logger.LogInformation("Unlinking Sponsor {SponsorId} from Tournament {TournamentId}", sponsorId, tournamentId);

            var relation = await _tournamentSponsorRepository
                .GetByTournamentAndSponsorAsync(tournamentId, sponsorId);

            if (relation == null)
                throw new KeyNotFoundException("Relationship not found");

            await _tournamentSponsorRepository.DeleteAsync(relation.Id);
        }

        public async Task<IEnumerable<TournamentSponsor>> GetTournamentsBySponsorAsync(int sponsorId)
        {
            var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);

            if (sponsor == null)
                throw new KeyNotFoundException("Sponsor not found");

            return await _tournamentSponsorRepository.GetBySponsorIdAsync(sponsorId);
        }
    }
}
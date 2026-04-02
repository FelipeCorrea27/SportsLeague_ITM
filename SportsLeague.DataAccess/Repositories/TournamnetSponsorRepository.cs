using Microsoft.EntityFrameworkCore;
using SportsLeague.DataAccess.Context;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.DataAccess.Repositories
{
    public class TournamentSponsorRepository: GenericRepository<TournamentSponsor>, ITournamentSponsorRepository
    {
        private readonly LeagueDbContext _context;

        public TournamentSponsorRepository(LeagueDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TournamentSponsor>> GetByTournamentIdAsync(int tournamentId)
        {
            return await _context.TournamentSponsors
                .Where(ts => ts.TournamentId == tournamentId)
                .Include(ts => ts.Sponsor)
                .ToListAsync();
        }

        public async Task<IEnumerable<TournamentSponsor>> GetBySponsorIdAsync(int sponsorId)
        {
            return await _context.TournamentSponsors
                .Where(ts => ts.SponsorId == sponsorId)
                .Include(ts => ts.Tournament)
                .ToListAsync();
        }

        public async Task<bool> ExistsByTournamentAndSponsorAsync(int tournamentId, int sponsorId)
        {
            return await _context.TournamentSponsors
                .AnyAsync(ts => ts.TournamentId == tournamentId && ts.SponsorId == sponsorId);
        }

        public async Task<TournamentSponsor?> GetByTournamentAndSponsorAsync(int tournamentId, int sponsorId)
        {
            return await _context.TournamentSponsors
                .FirstOrDefaultAsync(ts => ts.TournamentId == tournamentId && ts.SponsorId == sponsorId);
        }
    }
}
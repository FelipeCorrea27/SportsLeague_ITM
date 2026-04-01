namespace SportsLeague.Domain.Entities
{
    public class TournamentSponsor : AuditBase
    {
        public int TournamentId { get; set; } // Foreign Key to Tournament
        public int SponsorId { get; set; } // Foreign Key to Sponsor

        public decimal ContractAmount { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public  Tournament Tournament { get; set; } = null!;
        public  Sponsor Sponsor { get; set; }  = null!;

    }
}

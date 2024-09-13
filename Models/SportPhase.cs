namespace SporttiporssiAPI.Models
{
    public class SportPhase
    {
        public Guid SportPhaseId { get; set; }
        public Guid SerieId { get; set; }
        public int PhaseNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool Active { get; set; }
    }
}

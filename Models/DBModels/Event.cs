using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SporttiporssiAPI.Models.DBModels
{
    public class Event
    {
        public Guid Id { get; set; } // Eid
        public Guid StageId { get; set; } // Foreign key to Stage

        public Stage Stage { get; set; } // Navigation property
        public Guid HomeTeamId { get; set; } // Foreign key to Team

        public Team HomeTeam { get; set; } // Navigation property
        public Guid AwayTeamId { get; set; } // Foreign key to Team

        public Team AwayTeam { get; set; } // Navigation property

        public string EventStatus { get; set; } // Eps

        public int EventSeriesId { get; set; } // Esid

        public int EventPriority { get; set; } // Epr

        public int EventCoverage { get; set; } // Ecov

        public string EarnInfo { get; set; } // ErnInf

        public int EventType { get; set; } // Et

        public long EventStartDate { get; set; } // Esd

        public int ExternalId { get; set; } // EO

        public int ExternalIdX { get; set; } // EOX

        public int EventId { get; set; } // Ehid
    }
}

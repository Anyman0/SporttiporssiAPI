using System.ComponentModel.DataAnnotations;

namespace SporttiporssiAPI.Models.DBModels
{
    public class Team
    {
        public Guid Id { get; set; } // ID

        [Required]
        public string Name { get; set; } // Nm

        public string ImageUrl { get; set; } // Img

        public string Abbreviation { get; set; } // Abr

        public string CountryId { get; set; } // CoId

        public string CountryName { get; set; } // CoNm

        public bool HasVideo { get; set; } // HasVideo

        public ICollection<Event> HomeEvents { get; set; } // Navigation property for home games
        public ICollection<Event> AwayEvents { get; set; } // Navigation property for away games
    }
}

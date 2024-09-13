using System.ComponentModel.DataAnnotations;

namespace SporttiporssiAPI.Models.DBModels
{
    public class Stage
    {
        public Guid Id { get; set; } // Sid

        [Required]
        public string Name { get; set; } // Snm

        public string Code { get; set; } // Scd

        public string CountryId { get; set; } // Cid

        public string CountryName { get; set; } // Cnm

        public string CountryNameT { get; set; } // CnmT

        public string CountryShortName { get; set; } // Csnm

        public string CountryCode { get; set; } // Ccd

        public int Scu { get; set; } // Scu

        public string Sds { get; set; } // Sds

        public int Chi { get; set; } // Chi

        public int Shi { get; set; } // Shi

        public ICollection<Event> Events { get; set; } // Navigation property
    }
}

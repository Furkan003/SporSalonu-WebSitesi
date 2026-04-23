using System.ComponentModel.DataAnnotations;

namespace NfaSporSalonu.ViewModels
{
    public class MeasurementListViewModel
    {
        public List<MeasurementItemDto> Measurements { get; set; } = new();
    }

    public class MeasurementItemDto
    {
        public int MeasurementId { get; set; }
        public DateTime? MeasurementDate { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Bicep { get; set; }
        public decimal? Chest { get; set; }
        public decimal? Waist { get; set; }
        public string? Notes { get; set; }
    }

    public class AddMeasurementViewModel
    {
        [Display(Name = "Boy (cm)")]
        [Range(50, 250, ErrorMessage = "Boy 50-250 cm aralığında olmalıdır.")]
        public decimal? Height { get; set; }

        [Display(Name = "Kilo (kg)")]
        [Range(20, 300, ErrorMessage = "Kilo 20-300 kg aralığında olmalıdır.")]
        public decimal? Weight { get; set; }

        [Display(Name = "Bicep (cm)")]
        [Range(10, 80, ErrorMessage = "Bicep 10-80 cm aralığında olmalıdır.")]
        public decimal? Bicep { get; set; }

        [Display(Name = "Göğüs (cm)")]
        [Range(40, 200, ErrorMessage = "Göğüs 40-200 cm aralığında olmalıdır.")]
        public decimal? Chest { get; set; }

        [Display(Name = "Bel (cm)")]
        [Range(30, 200, ErrorMessage = "Bel 30-200 cm aralığında olmalıdır.")]
        public decimal? Waist { get; set; }

        [Display(Name = "Notlar")]
        [StringLength(255)]
        public string? Notes { get; set; }
    }

    public class MemberProgramListViewModel
    {
        public List<MemberProgramItemDto> Programs { get; set; } = new();
        public string? TrainerName { get; set; }
    }

    public class MemberProgramItemDto
    {
        public int ProgramId { get; set; }
        public string Title { get; set; } = null!;
        public string ProgramType { get; set; } = null!;
        public string? TrainerName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive => EndDate == null || EndDate > DateTime.Now;
    }

    public class ProgramDetailViewModel
    {
        public int ProgramId { get; set; }
        public string Title { get; set; } = null!;
        public string ProgramType { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? TrainerName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

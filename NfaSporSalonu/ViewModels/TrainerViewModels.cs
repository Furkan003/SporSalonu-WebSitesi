using System.ComponentModel.DataAnnotations;

namespace NfaSporSalonu.ViewModels
{
    public class TrainerListViewModel
    {
        public List<TrainerDto> Trainers { get; set; } = new();
    }

    public class TrainerDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public int TraineeCount { get; set; }
        public int ProgramCount { get; set; }
    }

    public class AssignTrainerViewModel
    {
        [Required(ErrorMessage = "Üye seçiniz.")]
        [Display(Name = "Üye")]
        public int TraineeId { get; set; }

        [Required(ErrorMessage = "Antrenör seçiniz.")]
        [Display(Name = "Antrenör")]
        public int TrainerId { get; set; }

        [Display(Name = "Notlar")]
        [StringLength(500)]
        public string? Notes { get; set; }

        // Dropdown listeleri için
        public List<UserSelectItem> AvailableTrainees { get; set; } = new();
        public List<UserSelectItem> AvailableTrainers { get; set; } = new();
    }

    public class UserSelectItem
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
    }

    public class CreateProgramViewModel
    {
        [Required(ErrorMessage = "Antrenör seçiniz.")]
        [Display(Name = "Antrenör")]
        public int TrainerId { get; set; }

        [Required(ErrorMessage = "Üye seçiniz.")]
        [Display(Name = "Üye")]
        public int TraineeId { get; set; }

        [Required(ErrorMessage = "Program türü seçiniz.")]
        [Display(Name = "Program Türü")]
        [StringLength(20)]
        public string ProgramType { get; set; } = null!;

        [Required(ErrorMessage = "Başlık zorunludur.")]
        [Display(Name = "Başlık")]
        [StringLength(100)]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "İçerik zorunludur.")]
        [Display(Name = "Program İçeriği")]
        public string Content { get; set; } = null!;

        [Display(Name = "Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        // Dropdown listeleri
        public List<UserSelectItem> AvailableTrainees { get; set; } = new();
        public List<UserSelectItem> AvailableTrainers { get; set; } = new();
    }

    public class ProgramListViewModel
    {
        public List<ProgramItemDto> Programs { get; set; } = new();
    }

    public class ProgramItemDto
    {
        public int ProgramId { get; set; }
        public string Title { get; set; } = null!;
        public string ProgramType { get; set; } = null!;
        public string? TrainerName { get; set; }
        public string? TraineeName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive => EndDate == null || EndDate > DateTime.Now;
    }

    public class AddTrainerNoteViewModel
    {
        [Required(ErrorMessage = "Antrenör seçiniz.")]
        [Display(Name = "Antrenör")]
        public int TrainerId { get; set; }

        [Required(ErrorMessage = "Üye seçiniz.")]
        [Display(Name = "Üye")]
        public int TraineeId { get; set; }

        [Required(ErrorMessage = "Not içeriği zorunludur.")]
        [Display(Name = "Not İçeriği")]
        [StringLength(1000, ErrorMessage = "Not en fazla 1000 karakter olabilir.")]
        public string NoteContent { get; set; } = null!;

        // Dropdown listeleri
        public List<UserSelectItem> AvailableTrainees { get; set; } = new();
        public List<UserSelectItem> AvailableTrainers { get; set; } = new();
    }
}

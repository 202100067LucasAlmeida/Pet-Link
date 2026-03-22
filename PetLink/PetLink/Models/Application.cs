using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PetLink.Models.Enums;

namespace PetLink.Models
{
    public class Application
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int AnimalListingId { get; set; }
        
        public string Message { get; set; }
        
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
        
        public int CurrentStep { get; set; } = 1;
        public int TotalSteps { get; set; } = 4;
        
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
        
        // propiedade de navegação
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        
        [ForeignKey("AnimalListingId")]
        public virtual AnimalListing AnimalListing { get; set; }
    }
    
    public enum ApplicationStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Completed = 4
    }
}
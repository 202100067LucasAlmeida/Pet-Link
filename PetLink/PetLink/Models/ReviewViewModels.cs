using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PetLink.Models.ViewModels
{
    public class CreateReviewViewModel
    {
        public int AnimalListingId { get; set; }
        public string? AnimalName { get; set; }
        public int ReviewedId { get; set; }
        public string? ReviewedName { get; set; }
        public string ReviewType { get; set; }

        [Required(ErrorMessage = "Please select a rating")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

    }

    public class UserReviewsViewModel
    {
        public User User { get; set; }
        public List<Review> Reviews { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public bool CanReceiveReviews { get; set; }  // Adicionado
    }

    public class TutorProfileViewModel
    {
        public User Tutor { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<Review> RecentReviews { get; set; }
        public List<AnimalListing> Listings { get; set; }
    }

    public class ReviewDisplayViewModel
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ReviewerName { get; set; }
        public string AnimalName { get; set; }
    }
}
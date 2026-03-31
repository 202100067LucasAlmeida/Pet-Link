using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PetLink.Models.ViewModels
{
    public class CreateReviewViewModel
    {
        public int AnimalListingId { get; set; }
        public string AnimalName { get; set; }
        public int ReviewedId { get; set; }
        public string ReviewedName { get; set; }

        [Required(ErrorMessage = "Please select a rating")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [MaxLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string Comment { get; set; }
    }

    public class UserReviewsViewModel
    {
        public User User { get; set; }
        public List<Review> Reviews { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }

    public class TutorProfileViewModel
    {
        public User Tutor { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<Review> RecentReviews { get; set; }
        public List<AnimalListing> Listings { get; set; }
    }
}
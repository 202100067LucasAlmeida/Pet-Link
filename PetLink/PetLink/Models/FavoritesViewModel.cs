using PetLink.Models;
using System.Collections.Generic;

namespace PetLink.Models.ViewModels
{
    public class FavoritesViewModel
    {
        public List<AnimalListing> FavoritePets { get; set; }
        public List<Petsitter> FavoritePetsitters { get; set; }
    }
}
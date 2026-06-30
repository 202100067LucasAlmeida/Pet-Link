using PetLink.Models;
using System.Collections.Generic;

namespace PetLink.Models.ViewModels
{
    /// <summary>
    /// ViewModel utilizado para apresentar os favoritos do utilizador,
    /// agrupando os anúncios de animais e os pet sitters guardados.
    /// </summary>
    public class FavoritesViewModel
    {
        /// <summary>
        /// Lista de anúncios de animais marcados como favoritos pelo utilizador.
        /// </summary>
        public List<AnimalListing> FavoritePets { get; set; }

        /// <summary>
        /// Lista de pet sitters marcados como favoritos pelo utilizador.
        /// </summary>
        public List<Petsitter> FavoritePetsitters { get; set; }
    }
}
using Microsoft.AspNetCore.Mvc;
using PetLink.Data;
using PetLink.Models.Enums;

namespace PetLink.Controllers
{
    public class SearchEngineController
    {
        private readonly ApplicationDbContext _context;

        public SearchEngineController(ApplicationDbContext context)
        {
            _context = context;
        }

        
    }
}

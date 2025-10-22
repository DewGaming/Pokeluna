using System;
using System.ComponentModel.DataAnnotations;

namespace Pokedex.DataAccess.Models
{
    public class Game
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
        [Required, Display(Name = "Release Date")]
        public DateTime ReleaseDate { get; set; }
        [Required, Display(Name = "Generation")]
        public int GenerationId { get; set; }
        public Generation Generation { get; set; }
        [Required, Display(Name = "Is Breeding Possible in Game")]
        public bool IsBreedingPossible { get; set; }
        [Required, Display(Name = "Is Marks Available for Wild Pokemon in Game")]
        public bool WildMarksPossible { get; set; }
        [Required, Display(Name = "Are Alphas in Game")]
        public bool AreAlphasAvailable { get; set; }
        [Required, Display(Name = "Game Background Color")]
        public string GameColor { get; set; }
    }
}
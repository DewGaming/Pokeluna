using System;
using System.ComponentModel.DataAnnotations;

namespace Pokedex.DataAccess.Models
{
    public class ShinyHunt
    {
        public ShinyHunt()
        { }

        public ShinyHunt(ShinyHunt shinyHunt)
        {
            Id = shinyHunt.Id;
            UserId = shinyHunt.UserId;
            User = shinyHunt.User;
            PokemonId = shinyHunt.PokemonId;
            Pokemon = shinyHunt.Pokemon;
            Nickname = shinyHunt.Nickname;
            GameId = shinyHunt.GameId;
            Game = shinyHunt.Game;
            HuntingMethodId = shinyHunt.HuntingMethodId;
            HuntingMethod = shinyHunt.HuntingMethod;
            HasShinyCharm = shinyHunt.HasShinyCharm;
            UsingLures = shinyHunt.UsingLures;
            IsAlpha = shinyHunt.IsAlpha;
            SparklingPowerLevel = shinyHunt.SparklingPowerLevel;
            DateOfCapture = shinyHunt.DateOfCapture;
            Phases = shinyHunt.Phases;
            CurrentPhaseEncounters = shinyHunt.CurrentPhaseEncounters;
            TotalEncounters = shinyHunt.TotalEncounters;
            IncrementAmount = shinyHunt.IncrementAmount;
            PokeballId = shinyHunt.PokeballId;
            Pokeball = shinyHunt.Pokeball;
            SweetId = shinyHunt.SweetId;
            Sweet = shinyHunt.Sweet;
            MarkId = shinyHunt.MarkId;
            Mark = shinyHunt.Mark;
            Gender = shinyHunt.Gender;
            IsCaptured = shinyHunt.IsCaptured;
            IsPinned = shinyHunt.IsPinned;
            PhaseOfHuntId = shinyHunt.PhaseOfHuntId;
            PhaseOfHunt = shinyHunt.PhaseOfHunt;
            ExcludeFromShinyDex = shinyHunt.ExcludeFromShinyDex;
            Notes = shinyHunt.Notes;
        }

        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public User User { get; set; }

        [Display(Name = "Pokémon Hunting")]
        public int? PokemonId { get; set; }
        public Pokemon Pokemon { get; set; }

        [StringLength(12), Display(Name = "Nickname")]
        public string Nickname { get; set; }

        [Required, Display(Name = "Game Hunting In")]
        public int GameId { get; set; }
        public Game Game { get; set; }

        [Required, Display(Name = "Hunting Method")]
        public int HuntingMethodId { get; set; }
        public HuntingMethod HuntingMethod { get; set; }

        [Display(Name = "Have the Shiny Charm")]
        public bool HasShinyCharm { get; set; }

        [Display(Name = "Using Lures")]
        public bool UsingLures { get; set; }

        [Display(Name = "Alpha Pokemon")]
        public bool IsAlpha { get; set; }

        [Display(Name = "Sparkling Power Level"), Range(0, 3)]
        public int SparklingPowerLevel { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date Captured")]
        public DateTime DateOfCapture { get; set; }

        [Display(Name = "Number of Phases")]
        public int Phases { get; set; }

        [Display(Name = "Number of Current Phase Encounters")]
        public int CurrentPhaseEncounters { get; set; }

        [Display(Name = "Number of Total Encounters")]
        public int TotalEncounters { get; set; }

        [Display(Name = "Increment Amount")]
        public int IncrementAmount { get; set; }

        [Display(Name = "Pokéball Used")]
        public int? PokeballId { get; set; }
        public Pokeball Pokeball { get; set; }

        [Display(Name = "Alcremie Sweet")]
        public int? SweetId { get; set; }
        public Sweet Sweet { get; set; }

        [Display(Name = "Mark")]
        public int? MarkId { get; set; }
        public Mark Mark { get; set; }

        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Display(Name = "Captured")]
        public bool IsCaptured { get; set; }

        [Display(Name = "Pinned")]
        public bool IsPinned { get; set; }

        [Display(Name = "Phase of This Hunt")]
        public int? PhaseOfHuntId { get; set; }
        public ShinyHunt PhaseOfHunt { get; set; }

        [Display(Name = "Exclude from Shiny Dex Page")]
        public bool ExcludeFromShinyDex { get; set; }

        [StringLength(1000), Display(Name = "Notes")]
        public string Notes { get; set; }
    }
}
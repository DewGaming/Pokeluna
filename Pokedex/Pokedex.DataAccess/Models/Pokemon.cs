using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pokedex.DataAccess.Models
{
    public class Pokemon
    {
        public int Id { get; set; }
        [Display(Name = "Pokedex Number")]
        public int PokedexNumber { get; set; }
        [Display(Name = "Pokemon Name"), StringLength(25), Required]
        public string Name { get; set; }
        [Required, Column(TypeName = "decimal(4,1)")]
        public decimal Height { get; set; }
        [Required, Column(TypeName = "decimal(4,1)")]
        public decimal Weight { get; set; }
        [Required]
        public bool IsComplete { get; set; }
        [Required, Display(Name = "Is Always Shiny Locked")]
        public bool IsShinyLocked { get; set; }
        [Display(Name = "Egg Cycle Count"), Required]
        public int EggCycleId { get; set; }
        public EggCycle EggCycle { get; set; }
        [Display(Name = "Gender Ratios"), Required]
        public int GenderRatioId { get; set; }
        public GenderRatio GenderRatio { get; set; }
        [Display(Name = "Classification"), Required]
        public int ClassificationId { get; set; }
        public Classification Classification { get; set; }
        [Display(Name = "Game(s) of Origin"), Required]
        public int GameId { get; set; }
        public Game Game { get; set; }
        [Display(Name = "Experience Growth"), Required]
        public int ExperienceGrowthId { get; set; }
        public ExperienceGrowth ExperienceGrowth { get; set; }
        [Display(Name = "Original Pokemon")]
        public int? OriginalFormId { get; set; }
        public Pokemon OriginalForm { get; set; }
        [Display(Name = "Form")]
        public int? FormId { get; set; }
        public Form Form { get; set; }
        [Required, Display(Name = "Special Group")]
        public int SpecialGroupingId { get; set; }
        public SpecialGrouping SpecialGrouping { get; set; }
        [Display(Name = "Has Gender Difference (Appearance Only)")]
        public bool HasGenderDifference { get; set; }
        [Display(Name = "Has Home Artwork")]
        public bool HasHomeArtwork { get; set; }
        [Display(Name = "Has Shiny Artwork")]
        public bool HasShinyArtwork { get; set; }
        [NotMapped]
        public bool IsAltForm { get { return OriginalFormId != null; } }
        [NotMapped]
        public string NameWithForm
        {
            get
            {
                if (this.IsAltForm && this.Form != null)
                {
                    return string.Concat(this.Name, " (", this.Form.Name, ")");
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        
        public virtual ICollection<PokemonBaseHappinessDetail> BaseHappinesses {get; set;}
        public virtual ICollection<BaseStat> BaseStats  {get; set;}
        public virtual ICollection<EVYield> EVYields {get; set;}
        public virtual ICollection<PokemonTypeDetail> Typings {get; set;}
        public virtual ICollection<PokemonAbilityDetail> Abilities  {get; set;}
        public virtual ICollection<PokemonEggGroupDetail> EggGroups  {get; set;}
        public virtual ICollection<PokemonCaptureRateDetail> CaptureRates {get; set;}

        public override bool Equals(object obj)
        {
            if (!(obj is Pokemon item))
            {
                return false;
            }

            return this.Id.Equals(item.Id);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
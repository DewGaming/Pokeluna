using System.ComponentModel.DataAnnotations;

namespace Pokedex.DataAccess.Models
{
    public class FormGroup
    {
        public int Id { get; set; }
        [Display(Name = "Form Group Name (Will Appear In Team Randomizer)"), Required]
        public string Name { get; set; }
        [Display(Name = "Will this Form Group appear separetely in the Team Randomizer"), Required]
        public bool AppearInTeamRandomizer { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is FormGroup item))
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
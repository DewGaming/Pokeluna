using System.ComponentModel.DataAnnotations;

namespace Pokedex.DataAccess.Models
{
    /// <summary>
    /// The class that is used to represent the form table in a database.
    /// </summary>
    public class Form
    {
        /// <summary>
        /// Gets or sets the primary key of the table.
        /// </summary>
        public int Id { get; set; }

        [Required]
        /// <summary>
        /// Gets or sets the name of the form.
        /// </summary>
        public string Name { get; set; }

        [Display(Name="Part Of \"Other Forms\" In Randomize Team Page"), Required]
        /// <summary>
        /// Gets or sets the ability to appear in the other forms category in the randomize team page.
        /// </summary>
        public bool Randomizable { get; set; }

        [Display(Name="Separate Form Group")]
        /// <summary>
        /// Gets or sets the id of the group this form belongs to.
        /// </summary>
        public int? FormGroupId { get; set; }

        /// <summary>
        /// Gets or sets the group this form belongs to.
        /// </summary>
        public FormGroup FormGroup { get; set; }

        [Display(Name="Requires An Item To Use"), Required]
        /// <summary>
        /// Gets or sets the necessity of an item.
        /// </summary>
        public bool NeedsItem { get; set; }

        [Display(Name="Only Available During Battle"), Required]
        /// <summary>
        /// Gets or sets whether or not it is only available during a pokemon battle.
        /// </summary>6
        public bool OnlyDuringBattle { get; set; }

        [Display(Name="Fusion Form"), Required]
        /// <summary>
        /// Gets or sets whether or not it is only possible through fusion.
        /// </summary>
        public bool FusionForm { get; set; }

        [Display(Name="Only With Tera"), Required]
        /// <summary>
        /// Gets or sets whether or not it is only available thru terastallization.
        /// </summary>
        public bool OnlyWithTera { get; set; }
    }
}
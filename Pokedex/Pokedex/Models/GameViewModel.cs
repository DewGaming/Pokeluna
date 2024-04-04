using Pokedex.DataAccess.Models;
using System.Collections.Generic;

namespace Pokedex.Models
{
    /// <summary>
    /// The class that is used to represent the Add, Update, and Admin Game pages and Edit Pokemon Game Availability page.
    /// </summary>
    public class GameViewModel : Game
    {
        /// <summary>
        /// Gets or sets a list of all generations.
        /// </summary>
        public List<Generation> AllGenerations { get; set; }
    }
}

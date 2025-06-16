using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pokedex.DataAccess.Models;
using Pokedex.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pokedex.Controllers
{
    /// <summary>
    /// The class that handles the requests for the owner.
    /// </summary>
    [Authorize(Roles = "Owner")]
    [Route("admin")]
    public class OwnerController : Controller
    {
        private readonly DataService dataService;

        private readonly AppConfig appConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwnerController"/> class.
        /// </summary>
        /// <param name="dataContext">The database context.</param>
        /// <param name="appConfig">The application configuration.</param>
        public OwnerController(DataContext dataContext, IOptions<AppConfig> appConfig)
        {
            // Instantiate an instance of the data service.
            this.dataService = new DataService(dataContext);
            this.appConfig = appConfig.Value;
        }

        /// <summary>
        /// Opens the page to view the stats of all pages.
        /// </summary>
        /// <returns>The page stat page.</returns>
        [Route("page_stats")]
        public IActionResult PageStats()
        {
            DateTime year = this.appConfig.PageStatStartDate;
            List<int> model = new List<int>()
            {
                year.Year,
            };
            while (year.Year < DateTime.Now.Year)
            {
                year = year.AddYears(1);
                model.Add(year.Year);
            }

            return this.View(model);
        }

        /// <summary>
        /// Opens the page to view the pokemon stats of all pages.
        /// </summary>
        /// <returns>The pokemon page stat page.</returns>
        [Route("pokemon_page_stats")]
        public IActionResult PokemonPageStats()
        {
            DateTime year = this.appConfig.PageStatStartDate;
            List<int> model = new List<int>()
            {
                year.Year,
            };
            while (year.Year < DateTime.Now.Year)
            {
                year = year.AddYears(1);
                model.Add(year.Year);
            }

            return this.View(model);
        }

        /// <summary>
        /// Opens the page to view all pokemon by generation.
        /// </summary>
        /// <returns>The all pokemon page.</returns>
        [Route("pokemon")]
        public IActionResult Pokemon()
        {
            List<Pokemon> pokemonList = this.dataService.GetObjects<Pokemon>(includes: "Game, Game.Generation").Where(x => !x.IsAltForm).ToList();
            List<int> model = pokemonList.Select(x => x.Game.GenerationId).Distinct().OrderBy(x => x).ToList();

            return this.View(model);
        }

        /// <summary>
        /// Opens the page to managing Generations in the database.
        /// </summary>
        /// <returns>The owner Generations page.</returns>
        [Route("generations")]
        public IActionResult Generations()
        {
            GenerationViewModel model = new GenerationViewModel()
            {
                AllGenerations = this.dataService.GetObjects<Generation>(),
                AllGames = this.dataService.GetObjects<Game>("ReleaseDate, Id"),
            };

            return this.View(model);
        }

        /// <summary>
        /// Opens the page to managing Games in the database.
        /// </summary>
        /// <returns>The owner Games page.</returns>
        [Route("games")]
        public IActionResult Games()
        {
            AdminGameViewModel model = new AdminGameViewModel()
            {
                AllGames = this.dataService.GetObjects<Game>("ReleaseDate, Id", "Generation").ToList(),
                AllPokemon = this.dataService.GetObjects<Pokemon>("PokedexNumber, Id", "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth"),
                CurrentTime = DateTime.UtcNow,
            };

            return this.View(model);
        }

        /// <summary>
        /// Opens the page to managing Types in the database.
        /// </summary>
        /// <returns>The owner Types page.</returns>
        [Route("types")]
        public IActionResult Types()
        {
            TypeViewModel model = new TypeViewModel()
            {
                AllTypes = this.dataService.GetObjects<DataAccess.Models.Type>("Name"),
                AllPokemon = this.dataService.GetObjects<PokemonTypeDetail>("PokemonId", "Pokemon, PrimaryType, SecondaryType"),
            };

            return this.View(model);
        }

        /// <summary>
        /// Opens the page to managing Egg Cycles in the database.
        /// </summary>
        /// <returns>The owner Egg Cycles page.</returns>
        [Route("egg_cycles")]
        public IActionResult EggCycles()
        {
            EggCycleViewModel model = new EggCycleViewModel()
            {
                AllEggCycles = this.dataService.GetObjects<EggCycle>("CycleCount"),
                AllPokemon = this.dataService.GetObjects<Pokemon>("PokedexNumber, Id", "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth"),
            };

            return this.View(model);
        }

        /// <summary>
        /// Opens the page to managing Experience Growths in the database.
        /// </summary>
        /// <returns>The owner Experience Growths page.</returns>
        [Route("experience_growths")]
        public IActionResult ExperienceGrowths()
        {
            ExperienceGrowthViewModel model = new ExperienceGrowthViewModel()
            {
                AllExperienceGrowths = this.dataService.GetObjects<ExperienceGrowth>("Name"),
                AllPokemon = this.dataService.GetObjects<Pokemon>("PokedexNumber, Id", "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth"),
            };

            return this.View(model);
        }

        /// <summary>
        /// Opens the page to managing Gender Ratios in the database.
        /// </summary>
        /// <returns>The owner Gender Ratios page.</returns>
        [Route("gender_ratios")]
        public IActionResult GenderRatios()
        {
            GenderRatiosViewModel model = new GenderRatiosViewModel()
            {
                AllGenderRatios = this.dataService.GetObjects<GenderRatio>(),
                AllPokemon = this.dataService.GetObjects<Pokemon>("PokedexNumber, Id", "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth"),
            };

            return this.View(model);
        }

        /// <summary>
        /// Opens the page to managing Form Items in the database.
        /// </summary>
        /// <returns>The owner Form Items page.</returns>
        [Route("form_items")]
        public IActionResult FormItems()
        {
            List<FormItem> model = this.dataService.GetFormItems();

            return this.View(model);
        }

        /// <summary>
        /// Opens the page to managing Form Groups in the database.
        /// </summary>
        /// <returns>The owner Form Groups page.</returns>
        [Route("form_groups")]
        public IActionResult FormGroups()
        {
            FormGroupViewModel model = new FormGroupViewModel()
            {
                AllFormGroups = this.dataService.GetObjects<FormGroup>(),
                AllForms = this.dataService.GetObjects<Form>("Name"),
                AllFormGroupGameDetails = this.dataService.GetObjects<FormGroupGameDetail>(includes: "Game"),
            };

            return this.View(model);
        }

        /// <summary>
        /// Opens the page to managing Abilities in the database.
        /// </summary>
        /// <returns>The owner Abilities page.</returns>
        [Route("abilities")]
        public IActionResult Abilities()
        {
            AbilityViewModel model = new AbilityViewModel()
            {
                AllAbilities = this.dataService.GetObjects<Ability>("GenerationId, Name"),
                AllPokemon = this.dataService.GetObjects<PokemonAbilityDetail>(includes: "Pokemon, PrimaryAbility, SecondaryAbility, HiddenAbility"),
            };

            return this.View(model);
        }

        /// <summary>
        /// Opens the page to managing Forms in the database.
        /// </summary>
        /// <returns>The owner Forms page.</returns>
        [Route("forms")]
        public IActionResult Forms()
        {
            FormViewModel model = new FormViewModel()
            {
                AllForms = this.dataService.GetObjects<Form>("Name", "FormGroup"),
                AllPokemon = this.dataService.GetObjects<Pokemon>(),
            };

            return this.View(model);
        }

        /// <summary>
        /// Opens the page to managing Egg Groups in the database.
        /// </summary>
        /// <returns>The owner Egg Groups page.</returns>
        [Route("egg_groups")]
        public IActionResult EggGroups()
        {
            EggGroupViewModel model = new EggGroupViewModel()
            {
                AllEggGroups = this.dataService.GetObjects<EggGroup>("Name"),
                AllPokemon = this.dataService.GetObjects<PokemonEggGroupDetail>(includes: "Pokemon, PrimaryEggGroup, SecondaryEggGroup"),
            };

            return this.View(model);
        }

        /// <summary>
        /// Opens the page to managing Evolution Methods in the database.
        /// </summary>
        /// <returns>The owner Evolution Methods page.</returns>
        [Route("evolution_methods")]
        public IActionResult EvolutionMethods()
        {
            EvolutionMethodViewModel model = new EvolutionMethodViewModel()
            {
                AllEvolutionMethods = this.dataService.GetObjects<EvolutionMethod>("Name"),
                AllEvolutions = this.dataService.GetObjects<Evolution>(includes: "PreevolutionPokemon, PreevolutionPokemon.Game, EvolutionPokemon, EvolutionPokemon.Game, EvolutionMethod"),
            };

            return this.View(model);
        }

        /// <summary>
        /// Opens the page to managing Capture Rates in the database.
        /// </summary>
        /// <returns>The owner Capture Rates page.</returns>
        [Route("capture_rates")]
        public IActionResult CaptureRates()
        {
            CaptureRateViewModel model = new CaptureRateViewModel()
            {
                AllCaptureRates = this.dataService.GetObjects<CaptureRate>("CatchRate"),
                AllPokemonCaptureRates = this.dataService.GetAllPokemonWithCaptureRates(),
            };

            return this.View(model);
        }

        /// <summary>
        /// Opens the page to managing Base Happinesses in the database.
        /// </summary>
        /// <returns>The owner Base Happinesses page.</returns>
        [Route("base_happinesses")]
        public IActionResult BaseHappinesses()
        {
            BaseHappinessViewModel model = new BaseHappinessViewModel()
            {
                AllBaseHappinesses = this.dataService.GetObjects<BaseHappiness>("Happiness"),
                AllPokemon = this.dataService.GetObjects<PokemonBaseHappinessDetail>(),
            };

            return this.View(model);
        }

        /// <summary>
        /// Opens the page to managing Classifications in the database.
        /// </summary>
        /// <returns>The owner Classifications page.</returns>
        [Route("classifications")]
        public IActionResult Classifications()
        {
            ClassificationViewModel model = new ClassificationViewModel()
            {
                AllClassifications = this.dataService.GetObjects<Classification>("Name"),
                AllPokemon = this.dataService.GetObjects<Pokemon>(includes: "Classification"),
            };

            return this.View(model);
        }

        /// <summary>
        /// Opens the page to managing Natures in the database.
        /// </summary>
        /// <returns>The owner Natures page.</returns>
        [Route("natures")]
        public IActionResult Natures()
        {
            List<Nature> model = this.dataService.GetObjects<Nature>("Name", "RaisedStat, LoweredStat");

            return this.View(model);
        }

        /// <summary>
        /// Opens the page to managing Stats in the database.
        /// </summary>
        /// <returns>The owner Stats page.</returns>
        [Route("stats")]
        public IActionResult Stats()
        {
            List<Stat> model = this.dataService.GetObjects<Stat>("Name");

            return this.View(model);
        }

        /// <summary>
        /// Opens the page to managing Marks in the database.
        /// </summary>
        /// <returns>The owner Marks page.</returns>
        [Route("marks")]
        public IActionResult Marks()
        {
            List<Mark> model = this.dataService.GetObjects<Mark>("GenerationId, Name");

            return this.View(model);
        }

        /// <summary>
        /// The owner's page for managing Sweets in the database.
        /// </summary>
        /// <returns>The owner Sweets page.</returns>
        [Route("sweets")]
        public IActionResult Sweets()
        {
            List<Sweet> model = this.dataService.GetObjects<Sweet>("Name");

            return this.View(model);
        }

        /// <summary>
        /// The owner's page for managing Special Groupings in the database.
        /// </summary>
        /// <returns>The owner Special Groupings page.</returns>
        [Route("special_groupings")]
        public IActionResult SpecialGroupings()
        {
            SpecialGroupingViewModel model = new SpecialGroupingViewModel()
            {
                AllSpecialGroupings = this.dataService.GetObjects<SpecialGrouping>("Name"),
                AllPokemon = this.dataService.GetObjects<Pokemon>(includes: "SpecialGrouping"),
            };

            return this.View(model);
        }

        /// <summary>
        /// The owner's page for managing Pokeballs in the database.
        /// </summary>
        /// <returns>The owner Pokeballs page.</returns>
        [Route("pokeball")]
        public IActionResult Pokeballs()
        {
            List<Pokeball> model = this.dataService.GetObjects<Pokeball>("GenerationId, Name");

            return this.View(model);
        }

        /// <summary>
        /// The owner's page for managing Hunting Methods in the database.
        /// </summary>
        /// <returns>The owner Hunting Methods page.</returns>
        [Route("hunting_method")]
        public IActionResult HuntingMethods()
        {
            List<HuntingMethod> model = this.dataService.GetObjects<HuntingMethod>("Name");

            return this.View(model);
        }

        /// <summary>
        /// The owner's page for managing Regional Dexes in the database.
        /// </summary>
        /// <returns>The owner Regional Dexes page.</returns>
        [Route("regional_dex")]
        public IActionResult RegionalDexes()
        {
            List<RegionalDex> regionalDexes = this.dataService.GetObjects<RegionalDex>("Game.ReleaseDate, Id", "Game");
            List<Game> allGames = this.dataService.GetGamesGroupedByReleaseDate();
            List<RegionalDexViewModel> model = new List<RegionalDexViewModel>();
            foreach (var dex in regionalDexes)
            {
                model.Add(new RegionalDexViewModel(dex)
                {
                    AllGames = allGames,
                });
            }

            return this.View(model);
        }

        /// <summary>
        /// Allows owners to view all users on the site.
        /// </summary>
        /// <returns>The owner user page.</returns>
        [Route("users")]
        public IActionResult Users()
        {
            UserViewModel model = new UserViewModel()
            {
                UserList = this.dataService.GetObjects<User>(),
                UsersWithPokemonTeams = new List<User>(),
            };

            List<PokemonTeam> pokemonTeams = this.dataService.GetObjects<PokemonTeam>();

            foreach (var u in model.UserList)
            {
                if (pokemonTeams.Where(x => x.UserId == u.Id).ToList().Count > 0)
                {
                    model.UsersWithPokemonTeams.Add(u);
                }
            }

            return this.View(model);
        }

        /// <summary>
        /// Opens the page to send a message based off of a comment.
        /// </summary>
        /// <param name="commentId">The id of the comment being replied to.</param>
        /// <returns>The send message page.</returns>
        [HttpGet]
        [Route("send_message/{commentId:int}")]
        public IActionResult SendMessage(int commentId)
        {
            Comment comment = this.dataService.GetObjectByPropertyValue<Comment>("Id", commentId);
            User receiver = this.dataService.GetObjectByPropertyValue<User>("Id", comment.CommentorId);

            Message model = new Message()
            {
                SenderId = this.dataService.GetCurrentUser(this.User).Id,
                ReceiverId = comment.CommentorId,
                Receiver = receiver,
                MessageTitle = string.Concat("Regaring your comment \"", comment.Name, "\" "),
            };

            return this.View(model);
        }

        /// <summary>
        /// Sends the message to the user.
        /// </summary>
        /// <param name="message">The message being sent.</param>
        /// <param name="commentId">The id of the comment being replied to.</param>
        /// <returns>The owner's comment page.</returns>
        [HttpPost]
        [Route("send_message/{commentId:int}")]
        public IActionResult SendMessage(Message message, int commentId)
        {
            if (!this.ModelState.IsValid)
            {
                Comment comment = this.dataService.GetObjectByPropertyValue<Comment>("Id", commentId);

                Message model = new Message()
                {
                    SenderId = this.dataService.GetCurrentUser(this.User).Id,
                    ReceiverId = comment.CommentorId,
                    MessageTitle = string.Concat("Regaring your comment \"", comment.Name, "\""),
                };

                return this.View(model);
            }

            this.dataService.AddObject(message);

            return this.RedirectToAction("Comments", "Owner");
        }

        /// <summary>
        /// Opens the page to send a message without the need of a comment.
        /// </summary>
        /// <returns>The send message page.</returns>
        [HttpGet]
        [Route("send_message")]
        public IActionResult SendMessageNoComment()
        {
            List<User> users = this.dataService.GetObjects<User>();
            users.Remove(users.Find(x => x.Username == this.User.Identity.Name));

            MessageViewModel model = new MessageViewModel()
            {
                SenderId = this.dataService.GetCurrentUser(this.User).Id,
                AllUsers = users,
            };

            return this.View(model);
        }

        /// <summary>
        /// Sends the message to the user.
        /// </summary>
        /// <param name="message">The message being sent.</param>
        /// <returns>The owner's view message page.</returns>
        [HttpPost]
        [Route("send_message")]
        public IActionResult SendMessageNoComment(MessageViewModel message)
        {
            if (!this.ModelState.IsValid)
            {
                List<User> users = this.dataService.GetObjects<User>();
                users.Remove(users.Find(x => x.Username == this.User.Identity.Name));

                MessageViewModel model = new MessageViewModel()
                {
                    SenderId = this.dataService.GetCurrentUser(this.User).Id,
                    AllUsers = users,
                };

                return this.View(model);
            }

            this.dataService.AddObject(message);

            return this.RedirectToAction("ViewMessages", "User");
        }

        /// <summary>
        /// Opens the page to view comments left by users.
        /// </summary>
        /// <returns>The comments page.</returns>
        [Route("comments")]
        public IActionResult Comments()
        {
            List<Comment> model = this.dataService.GetObjects<Comment>("Id", "Commentor");

            return this.View(model);
        }

        /// <summary>
        /// Marks a comment as being completed.
        /// </summary>
        /// <param name="id">The comment's id.</param>
        /// <returns>The comments page.</returns>
        [Route("complete_comment/{id:int}")]
        public IActionResult CompleteComment(int id)
        {
            Comment comment = this.dataService.GetObjectByPropertyValue<Comment>("Id", id);
            comment.IsCompleted = true;

            this.dataService.UpdateObject(comment);

            return this.RedirectToAction("Comments", "Owner");
        }

        /// <summary>
        /// Marks automated errors as being completed.
        /// </summary>
        /// <returns>The comments page.</returns>
        [Route("complete_automated_errors")]
        public IActionResult CompleteAutomatedErrors()
        {
            List<Comment> comments = this.dataService.GetObjects<Comment>().Where(x => x.IsAutomatedError && !x.IsCompleted).ToList();

            foreach (var c in comments)
            {
                c.IsCompleted = true;
                this.dataService.UpdateObject(c);
            }

            return this.RedirectToAction("Comments", "Owner");
        }

        /// <summary>
        /// Opens the review incomplete pokemon page.
        /// </summary>
        /// <param name="pokemonId">The pokemon's id.</param>
        /// <returns>The pokemon review page.</returns>
        [HttpGet]
        [Route("review_pokemon/{pokemonId:int}")]
        public IActionResult ReviewPokemon(int pokemonId)
        {
            Pokemon pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form, BaseHappinesses, BaseHappinesses.BaseHappiness, BaseStats, EVYields, Typings.PrimaryType, Typings.SecondaryType, Typings.Generation, Abilities.PrimaryAbility, Abilities.SecondaryAbility, Abilities.HiddenAbility, EggGroups.PrimaryEggGroup, EggGroups.SecondaryEggGroup, CaptureRates.CaptureRate, CaptureRates.Generation");

            // Ensuring that the pokemon really has all of these added.
            bool pokemonIsComplete = pokemon.Typings.Count() > 0 && pokemon.Abilities.Count() > 0 && pokemon.EggGroups.Count() > 0 && pokemon.BaseStats.Count() > 0 && pokemon.EVYields.Count() > 0;

            if (pokemonIsComplete && !pokemon.IsComplete)
            {
                List<Game> games = this.dataService.GetObjects<PokemonGameDetail>("Game.ReleaseDate, GameId, Id", "Pokemon, Pokemon.Game, Game", "PokemonId", pokemon.Id).ConvertAll(x => x.Game);
                games.Remove(games.Find(x => x.Id == 43));
                PokemonViewModel model = new PokemonViewModel()
                {
                    Pokemon = pokemon,
                    BaseStats = pokemon.BaseStats.OrderByDescending(x => x.GenerationId).ToList(),
                    EVYields = pokemon.EVYields.OrderByDescending(x => x.GenerationId).ToList(),
                    Typings = pokemon.Typings.OrderByDescending(x => x.GenerationId).ToList(),
                    Abilities = pokemon.Abilities.OrderByDescending(x => x.GenerationId).ToList(),
                    EggGroups = pokemon.EggGroups.OrderByDescending(x => x.GenerationId).ToList(),
                    CaptureRates = pokemon.CaptureRates.OrderByDescending(x => x.GenerationId).ToList(),
                    BaseHappinesses = pokemon.BaseHappinesses.OrderByDescending(x => x.GenerationId).ToList(),
                    PreEvolutions = this.dataService.GetPreEvolution(pokemon.Id),
                    Evolutions = this.dataService.GetPokemonEvolutions(pokemon.Id),
                    Effectiveness = this.dataService.GetTypeChartPokemon(pokemon.Id),
                    GamesAvailableIn = games,
                    AppConfig = this.appConfig,
                };

                if (pokemon.IsAltForm)
                {
                    model.OriginalPokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemon.OriginalFormId);
                }

                return this.View(model);
            }
            else
            {
                return this.RedirectToAction("Pokemon", "Owner");
            }
        }

        /// <summary>
        /// Marks a newly finished pokemon as reviewed.
        /// </summary>
        /// <param name="pokemon">The newly finished pokemon.</param>
        /// <returns>The admin pokemon page.</returns>
        [HttpPost]
        [Route("review_pokemon/{pokemonId:int}")]
        public IActionResult ReviewPokemon(Pokemon pokemon)
        {
            pokemon.IsComplete = true;
            this.dataService.UpdateObject(pokemon);

            return this.RedirectToAction("Pokemon", "Owner");
        }

        /// <summary>
        /// Marks a newly finished pokemon as completed.
        /// </summary>
        /// <param name="pokemonId">The pokemon's id.</param>
        /// <returns>The admin pokemon page.</returns>
        [Route("complete_pokemon/{pokemonId:int}")]
        public IActionResult CompletePokemon(int pokemonId)
        {
            Pokemon pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonId);
            pokemon.IsComplete = true;
            this.dataService.UpdateObject(pokemon);

            return this.RedirectToAction("Pokemon", "Owner");
        }
    }
}

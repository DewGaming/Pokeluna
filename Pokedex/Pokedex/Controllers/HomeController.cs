using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MoreLinq;
using Pokedex.DataAccess.Models;
using Pokedex.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Pokedex.Controllers
{
    /// <summary>
    /// The class that handles the main website pages.
    /// </summary>
    [Authorize]
    [Route("")]
    public class HomeController : Controller
    {
        private static int selectedPokemonId = 0;

        private static int selectedGenerationId = 0;

        private readonly DataService dataService;

        private readonly AppConfig appConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="appConfig">The application configuration.</param>
        /// <param name="dataContext">The database context.</param>
        public HomeController(IOptions<AppConfig> appConfig, DataContext dataContext)
        {
            this.appConfig = appConfig.Value;
            this.dataService = new DataService(dataContext);
        }

        /// <summary>
        /// The home page of the website.
        /// </summary>
        /// <returns>The view for the home page.</returns>
        [AllowAnonymous]
        [Route("")]
        public IActionResult Index()
        {
            return this.View(this.appConfig);
        }

        /// <summary>
        /// Searchs for the pokemon that contains the searched text.
        /// </summary>
        /// <param name="searchText">The text being searched.</param>
        /// <returns>Returns the all pokemon page if no pokemon fits the search, or continues to do a more in-depth search.</returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("search")]
        public IActionResult Search(string searchText)
        {
            searchText = HttpUtility.UrlDecode(searchText);
            searchText = string.IsNullOrEmpty(searchText) ? string.Empty : searchText.Replace("/", string.Empty).Replace("\\", string.Empty);
            if (string.IsNullOrEmpty(searchText))
            {
                return this.RedirectToAction("AllPokemon", "Home");
            }
            else
            {
                return this.RedirectToAction("SearchRedirect", "Home", new { search = searchText });
            }
        }

        /// <summary>
        /// Searchs for the pokemon that contains the searched text.
        /// </summary>
        /// <param name="search">The text being searched.</param>
        /// <returns>Returns the pokemon's page if only one pokemon fits the search, or shows all pokemon with the given text.</returns>
        [AllowAnonymous]
        [Route("search/{search}")]
        public IActionResult SearchRedirect(string search)
        {
            search = HttpUtility.UrlDecode(search);

            if (!string.IsNullOrEmpty(search))
            {
                this.ViewData["Search"] = search;
                search = this.FormatPokemonName(search);
                List<Pokemon> model = this.dataService.GetObjects<Pokemon>(includes: "Form");
                if (model.Where(x => x.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).Count() > 0)
                {
                    model = model.Where(x => x.Name.ToString().Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                else
                {
                    model = model.Where(x => x.PokedexNumber.ToString().Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                this.dataService.AddPageView("Search Page", this.User.IsInRole("Owner"));
                if (model.Count == 1)
                {
                    return this.RedirectToAction("Pokemon", "Home", new { Name = model[0].Name.Replace(": ", "_").Replace(' ', '_').ToLower() });
                }
                else if (model.Count > 1)
                {
                    AllPokemonTypeViewModel viewModel = new AllPokemonTypeViewModel()
                    {
                        AllPokemon = model.OrderBy(x => x.PokedexNumber).ThenBy(x => x.Id).ToList(),
                        AppConfig = this.appConfig,
                    };

                    return this.View("Search", viewModel);
                }
                else
                {
                    return this.RedirectToAction("AllPokemon", "Home");
                }
            }

            return this.RedirectToAction("AllPokemon", "Home");
        }

        /// <summary>
        /// The page that lists off all of the completed pokemon.
        /// </summary>
        /// <returns>Returns the all pokemon's page.</returns>
        [AllowAnonymous]
        [Route("pokemon")]
        public IActionResult AllPokemon()
        {
            List<Pokemon> pokemonList = this.dataService.GetAllPokemon().Where(x => !x.IsAltForm).ToList();
            List<Game> model = this.dataService.GetObjects<Game>("ReleaseDate, Id");

            this.dataService.AddPageView("All Pokemon Page", this.User.IsInRole("Owner"));
            return this.View(model);
        }

        /// <summary>
        /// The page that calculates how long.
        /// </summary>
        /// <returns>Returns the all pokemon's page.</returns>
        [AllowAnonymous]
        [Route("exp_leveling")]
        public IActionResult ExpLeveling()
        {
            List<Pokemon> pokemonList = this.dataService.GetNonBattlePokemonWithFormNames();
            ExpLevelingViewModel model = new ExpLevelingViewModel()
            {
                PokemonList = pokemonList,
                AppConfig = this.appConfig,
            };

            this.dataService.AddPageView("Exp Leveling Page", this.User.IsInRole("Owner"));
            return this.View(model);
        }

        /// <summary>
        /// The page that allows users to generate randomized teams.
        /// </summary>
        /// <returns>Returns the team randomizer's page.</returns>
        [AllowAnonymous]
        [Route("team_randomizer")]
        public IActionResult TeamRandomizer()
        {
            List<Pokemon> allPokemon = this.dataService.GetAllPokemon();
            List<Generation> generations = this.dataService.GetObjects<Generation>();
            List<Game> selectableGames = this.dataService.GetGamesGroupedByReleaseDate().Where(x => x.Id != 43).ToList();

            foreach (var gen in generations)
            {
                List<Pokemon> pokemonInGen = allPokemon.Where(x => x.Game.GenerationId == gen.Id).ToList();
                if (pokemonInGen.Count == 0)
                {
                    selectableGames.RemoveAll(x => x.GenerationId == gen.Id);
                }
            }

            TeamRandomizerListViewModel model = new TeamRandomizerListViewModel()
            {
                AllGames = selectableGames,
                AllTypes = this.dataService.GetObjects<DataAccess.Models.Type>("Name").Where(x => x.Name != "Stellar").ToList(),
                AllSpecialGroupings = this.dataService.GetObjects<SpecialGrouping>(),
                AllFormGroups = this.dataService.GetObjects<FormGroup>("Name", whereProperty: "AppearInTeamRandomizer", wherePropertyValue: true),
                AllFormGroupGameDetails = this.dataService.GetObjects<FormGroupGameDetail>("FormGroup.Name", "FormGroup", "AppearInTeamRandomizer", true),
                IncompleteCount = allPokemon.Where(x => !x.IsComplete).Count(),
            };

            this.dataService.AddPageView("Team Randomizer Page", this.User.IsInRole("Owner"));
            return this.View(model);
        }

        /// <summary>
        /// The page that is used to evaluate the different possible pokemon type combinations. This will also showcase any completed pokemon with the selected type combination.
        /// </summary>
        /// <returns>Returns the typing evaluator page.</returns>
        [AllowAnonymous]
        [Route("typing_lookup")]
        public IActionResult TypingEvaluator()
        {
            TypeEvaluatorViewModel model = new TypeEvaluatorViewModel()
            {
                AllTypes = this.dataService.GetObjects<DataAccess.Models.Type>("Name"),
                AllGames = this.dataService.GetGamesGroupedByReleaseDate().Where(x => x.Id != 43).ToList(),
            };

            this.dataService.AddPageView("Typing Lookup Page", this.User.IsInRole("Owner"));
            return this.View(model);
        }

        /// <summary>
        /// The page that is used to evaluate the different abilities, providing a description of the selected ability and showcasing any completed pokemon with said ability.
        /// </summary>
        /// <returns>Returns the ability evaluator page.</returns>
        [AllowAnonymous]
        [Route("ability_lookup")]
        public IActionResult AbilityEvaluator()
        {
            AbilityEvaluatorViewModel model = new AbilityEvaluatorViewModel()
            {
                AllAbilities = this.dataService.GetObjects<Ability>("Name"),
                AllGames = this.dataService.GetGamesGroupedByReleaseDate().Where(x => x.Id != 16 && x.Id != 37 && x.Id != 43 && x.GenerationId >= 3).ToList(),
            };

            this.dataService.AddPageView("Ability Evalutator Page", this.User.IsInRole("Owner"));
            return this.View(model);
        }

        /// <summary>
        /// The page that is used to evaluate the pokemon available for use in day cares.
        /// </summary>
        /// <returns>Returns the day care evaluator.</returns>
        [AllowAnonymous]
        [Route("day_care_combinations")]
        public IActionResult DayCareEvaluator()
        {
            EggGroupEvaluatorViewModel model = new EggGroupEvaluatorViewModel()
            {
                AllGames = this.dataService.GetGamesGroupedByReleaseDate().Where(x => x.IsBreedingPossible).ToList(),
                AppConfig = this.appConfig,
                GenerationId = this.dataService.GetObjects<Generation>().Last().Id,
            };

            this.dataService.AddPageView("Day Care Combinations Page", this.User.IsInRole("Owner"));
            return this.View(model);
        }

        /// <summary>
        /// The page that is used to evaluate the alternate forms of pokemon.
        /// </summary>
        /// <returns>Returns the form evaluator.</returns>
        [AllowAnonymous]
        [Route("form_viewer")]
        public IActionResult FormEvaluator()
        {
            List<Form> model = this.dataService.GetObjects<Form>();
            List<FormGroup> formGroupList = this.dataService.GetObjects<FormGroup>();

            model.Remove(model.FirstOrDefault(x => x.Name == "Female"));

            foreach (var fg in formGroupList)
            {
                List<Form> formsFromGroupList = model.Where(x => x.FormGroupId == fg.Id).ToList();
                model.Add(new Form() { Id = formsFromGroupList.First().Id, Name = fg.Name, FormGroupId = fg.Id });
                foreach (var f in formsFromGroupList)
                {
                    model.Remove(f);
                }
            }

            model = model.OrderBy(x => x.Name).ToList();
            this.dataService.AddPageView("Form Viewer Page", this.User.IsInRole("Owner"));
            return this.View(model);
        }

        /// <summary>
        /// The page that is used to showcase what pokemon are available for use in a generation. This is not showing what pokemon are catchable in a game.
        /// </summary>
        /// <returns>Returns the game availability page.</returns>
        [AllowAnonymous]
        [Route("game_availability")]
        public IActionResult GameAvailability()
        {
            GameAvailabilityViewModel model = new GameAvailabilityViewModel()
            {
                EdittedGames = this.GetGamesForEachReleaseDate(),
                UnedittedGames = this.dataService.GetObjects<Game>("ReleaseDate, Id"),
            };

            this.dataService.AddPageView("Game Availability Page", this.User.IsInRole("Owner"));
            return this.View(model);
        }

        /// <summary>
        /// The page that is used to showcase the ev yield of opponent pokemon.
        /// </summary>
        /// <returns>Returns the ev trainer page.</returns>
        [AllowAnonymous]
        [Route("ev_trainer")]
        public IActionResult EVTrainer()
        {
            List<Game> games = this.dataService.GetGamesGroupedByReleaseDate().Where(x => x.Id != 43 && x.GenerationId >= 3).ToList();

            this.dataService.AddPageView("EV Trainer Page", this.User.IsInRole("Owner"));
            return this.View(games);
        }

        /// <summary>
        /// The method that is used to specify a generation and alternate form for the pokemon page.
        /// </summary>
        /// <param name="pokemonId">The Id of the pokemon.</param>
        /// <param name="generationId">The Id of the generation. Defaults to 0.</param>
        /// <returns>Returns the pokemon page's method.</returns>
        [AllowAnonymous]
        [Route("pokemon/{pokemonId:int}/{generationId:int}")]
        public IActionResult PokemonWithOnlyId(int pokemonId, int generationId = 0)
        {
            Pokemon pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonId);

            return this.RedirectToAction("PokemonWithId", "Home", new { pokemonName = pokemon.Name.ToLower(), pokemonId, generationId });
        }

        /// <summary>
        /// The method that is used to specify a generation and alternate form for the pokemon page.
        /// </summary>
        /// <param name="pokemonName">The name of the pokemon.</param>
        /// <param name="pokemonId">The Id of the pokemon.</param>
        /// <param name="generationId">The Id of the generation.</param>
        /// <returns>Returns the pokemon page's method.</returns>
        [AllowAnonymous]
        [Route("pokemon/{pokemonName}/{pokemonId:int}/{generationId:int}")]
        public IActionResult PokemonWithId(string pokemonName, int pokemonId, int generationId)
        {
            selectedPokemonId = pokemonId;
            if (generationId > this.dataService.GetObjects<PokemonGameDetail>(includes: "Game", whereProperty: "PokemonId", wherePropertyValue: pokemonId).Select(x => x.Game).Last().GenerationId || generationId < this.dataService.GetObjects<PokemonGameDetail>(includes: "Game", whereProperty: "PokemonId", wherePropertyValue: pokemonId).Select(x => x.Game).First().GenerationId)
            {
                selectedGenerationId = 0;
            }
            else
            {
                selectedGenerationId = generationId;
            }

            return this.RedirectToAction("Pokemon", "Home", new { name = pokemonName });
        }

        /// <summary>
        /// The page that is used to showcase all of the information pertaining to a particular pokemon. This includes any alternate forms relating to the selected pokemon.
        /// </summary>
        /// <param name="name">The name of the pokemon.</param>
        /// <returns>Returns the pokemon's page.</returns>
        [AllowAnonymous]
        [Route("pokemon/{Name}")]
        public IActionResult Pokemon(string name)
        {
            int pokemonId = selectedPokemonId;
            int generationId = selectedGenerationId;
            int formId = 0;
            selectedPokemonId = 0;
            selectedGenerationId = 0;
            name = this.FormatPokemonName(name);

            try
            {
                Pokemon pokemon;
                List<Pokemon> pokemonList = this.dataService.GetAllPokemon().Where(x => x.Name.ToString().Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
                if (pokemonId != 0)
                {
                    pokemon = pokemonList.Find(x => x.Id == pokemonId);
                }
                else
                {
                    pokemon = pokemonList.Find(x => x.Name.ToString().Equals(name, StringComparison.OrdinalIgnoreCase));
                    pokemonId = pokemon.Id;
                }

                if (pokemon == null)
                {
                    return this.RedirectToAction("AllPokemon", "Home");
                }
                else
                {
                    string pokemonName = pokemon.Name;
                    List<DataAccess.Models.Type> typeList = this.dataService.GetObjects<DataAccess.Models.Type>("Name").Where(x => x.Name != "Stellar").ToList();

                    if (generationId == 0)
                    {
                        generationId = this.dataService.GetObjects<PokemonGameDetail>("Game.GenerationId, GameId, Id", "Pokemon, Pokemon.Game, Game", "PokemonId", pokemonId).Where(x => x.Game.ReleaseDate <= DateTime.UtcNow).Last().Game.GenerationId;
                    }

                    List<PokemonViewModel> pokemonDetailList = new List<PokemonViewModel>
                    {
                        this.dataService.GetPokemonDetails(pokemon, this.appConfig),
                    };

                    if (pokemonList.Where(x => x.OriginalFormId == pokemonId).Count() > 0 || pokemon.IsAltForm)
                    {
                        int originalPokemonId = pokemonId;
                        if (pokemon.IsAltForm)
                        {
                            originalPokemonId = (int)pokemon.OriginalFormId;
                            formId = (int)pokemon.FormId;
                        }

                        foreach (var p in pokemonList.Where(x => (x.OriginalFormId == originalPokemonId || x.Id == originalPokemonId) && x.Id != pokemonId))
                        {
                            pokemonDetailList.Add(this.dataService.GetPokemonDetails(p, this.appConfig));
                        }
                    }

                    List<int> pokemonIds = pokemonDetailList.ConvertAll(x => x.Pokemon.Id);

                    if (pokemonIds.IndexOf(pokemonId) == -1)
                    {
                        pokemonId = pokemon.Id;
                    }

                    AdminPokemonDropdownViewModel model = new AdminPokemonDropdownViewModel()
                    {
                        PokemonList = pokemonDetailList,
                        PokemonId = pokemonId,
                        GenerationId = generationId,
                        LatestGenerationId = this.dataService.GetObjects<Generation>("Id").Last().Id,
                        AllTypes = new List<DataAccess.Models.Type>(),
                    };

                    if (this.User.IsInRole("Owner"))
                    {
                        AllAdminPokemonViewModel allAdminPokemon = this.dataService.GetAllAdminPokemonDetails();
                        DropdownViewModel dropdownViewModel = new DropdownViewModel()
                        {
                            AllPokemon = allAdminPokemon,
                            AppConfig = this.appConfig,
                        };
                        AdminGenerationTableViewModel adminDropdown = new AdminGenerationTableViewModel()
                        {
                            PokemonList = new List<Pokemon>(),
                            DropdownViewModel = dropdownViewModel,
                            AppConfig = this.appConfig,
                        };

                        foreach (var p in pokemonDetailList)
                        {
                            adminDropdown.PokemonList.Add(p.Pokemon);
                        }

                        model.AdminDropdown = adminDropdown;
                    }

                    if (name == "Arceus")
                    {
                        model.AllTypes.Add(new DataAccess.Models.Type { Id = 0, Name = "No Plate" });
                    }
                    else if (name == "Silvally")
                    {
                        model.AllTypes.Add(new DataAccess.Models.Type { Id = 0, Name = "No Memory" });
                    }
                    else
                    {
                        model.AllTypes.Add(new DataAccess.Models.Type { Id = 0, Name = "Not Terastallized" });
                    }

                    model.AllTypes.Add(new DataAccess.Models.Type { Id = 99, Name = "Stellar" });
                    model.AllTypes.AddRange(typeList);
                    this.dataService.AddPageView(string.Concat("Pokemon Page"), this.User.IsInRole("Owner"));
                    this.dataService.AddPokemonPageView(pokemonId, formId, this.User.IsInRole("Owner"));

                    return this.View(model);
                }
            }
            catch (Exception e)
            {
                if (!this.User.IsInRole("Owner"))
                {
                    string commentBody;
                    if (e != null)
                    {
                        commentBody = string.Concat(e.GetType().ToString(), " error while setting up pokemon page.");
                    }
                    else
                    {
                        commentBody = "Unknown error while setting up pokemon page.";
                    }

                    commentBody = string.Concat(commentBody, " - Selected Pokemon: ", name);
                    commentBody = string.Concat(commentBody, " - Selected Pokemon Id: ", pokemonId);
                    commentBody = string.Concat(commentBody, " - Selected Generation Id: ", generationId);

                    Comment comment = new Comment()
                    {
                        Name = commentBody,
                    };

                    if (this.User.Identity.Name != null)
                    {
                        comment.CommentorId = this.dataService.GetCurrentUser(this.User).Id;
                    }

                    this.dataService.AddObject(comment);
                    this.dataService.EmailComment(this.appConfig, comment);
                }

                return null;
            }
        }

        /// <summary>
        /// The page that is used to showcase the different type charts throughout the generations.
        /// </summary>
        /// <returns>The type chart page.</returns>
        [AllowAnonymous]
        [Route("type_chart")]
        public IActionResult TypeChart()
        {
            this.dataService.AddPageView("Type Chart Page", this.User.IsInRole("Owner"));
            TypeChartViewModel model = new TypeChartViewModel()
            {
                TypeChart = this.dataService.GetObjects<TypeChart>("AttackId, DefendId", "Attack, Defend"),
                AllTypes = this.dataService.GetObjects<DataAccess.Models.Type>("Name"),
                AllGenerations = this.dataService.GetObjects<Generation>("Id"),
                AppConfig = this.appConfig,
            };

            return this.View(model);
        }

        /// <summary>
        /// The page that is used to showcase the different type charts throughout the generations.
        /// </summary>
        /// <returns>The type chart page.</returns>
        [AllowAnonymous]
        [Route("pokemon_difference")]
        public IActionResult PokemonDifference()
        {
            this.dataService.AddPageView("Pokemon Difference Page", this.User.IsInRole("Owner"));
            PokemonDifferenceViewModel model = new PokemonDifferenceViewModel()
            {
                AllPokemon = this.dataService.GetPokemonWithFormNames(),
                AppConfig = this.appConfig,
            };

            return this.View(model);
        }

        /// <summary>
        /// The page used for users to create comments regarding the website.
        /// </summary>
        /// <returns>Returns the comment page.</returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("comment")]
        public IActionResult Comment()
        {
            this.dataService.AddPageView("Comment Page", this.User.IsInRole("Owner"));
            Comment model = new Comment();
            return this.View(model);
        }

        /// <summary>
        /// Verifies if the comment is valid, then adds it to the database and sends the owner an email.
        /// </summary>
        /// <param name="comment">The comment the user made.</param>
        /// <returns>Returns the main page.</returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("comment")]
        public IActionResult Comment(Comment comment)
        {
            if (!this.ModelState.IsValid)
            {
                Comment model = new Comment();
                return this.View(model);
            }

            if (this.User.Identity.Name != null)
            {
                comment.CommentorId = this.dataService.GetCurrentUser(this.User).Id;
            }

            this.dataService.AddObject(comment);

            this.dataService.EmailComment(this.appConfig, comment);

            return this.RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// The page that is used to provide more information about the website.
        /// </summary>
        /// <returns>Returns the about page.</returns>
        [AllowAnonymous]
        [Route("about")]
        public IActionResult About()
        {
            this.dataService.AddPageView("About Page", this.User.IsInRole("Owner"));

            return this.View();
        }

        /// <summary>
        /// Only viewed if the user gets an unaccounted for error while using the website. An email with the exact error will be sent to the owner's specified email.
        /// </summary>
        /// <returns>Returns the error page.</returns>
        [AllowAnonymous]
        [Route("error")]
        public IActionResult Error()
        {
            this.dataService.AddPageView("Error Page", this.User.IsInRole("Owner"));
            Exception error = this.HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>().Error;
            StackTrace stackTrace = new StackTrace(error);
            StackFrame[] stackFrames = stackTrace.GetFrames();

            if (!this.User.IsInRole("Owner") && error != null)
            {
                Comment comment = new Comment()
                {
                    Name = string.Concat(error.GetType().ToString(), " (", error.Message, ")"),
                    IsAutomatedError = true,
                };

                if (error.InnerException != null)
                {
                    comment.Name = string.Concat(comment.Name, " (", error.InnerException.Message, ")");
                }

                foreach (StackFrame sf in stackFrames)
                {
                    comment.Name = string.Concat(comment.Name, "\r\nMethod: ", sf.GetMethod().Name, ", Line Number: ", sf.GetFileLineNumber());
                }

                if (this.User.Identity.Name != null)
                {
                    comment.CommentorId = this.dataService.GetCurrentUser(this.User).Id;
                }

                this.dataService.AddObject(comment);

                this.dataService.EmailComment(this.appConfig, comment);
            }
            else if (error != null)
            {
                string name = string.Concat(error.GetType().ToString(), " (", error.Message, ")");

                if (error.InnerException != null)
                {
                    name = string.Concat(name, " (", error.InnerException.Message, ")");
                }

                name = string.Concat(name, error.StackTrace.Replace("   ", " "));
                return this.Problem(detail: name, title: error.Message);
            }

            return this.View();
        }

        /// <summary>
        /// Formats the name of the pokemon to how the database stores their names.
        /// </summary>
        /// <param name="pokemonName">The name being formatted.</param>
        /// <returns>The formatted pokemon name string.</returns>
        private string FormatPokemonName(string pokemonName)
        {
            pokemonName.ToLower();

            if (pokemonName.Contains("type"))
            {
                pokemonName = "Type: Null";
            }

            if (pokemonName.Contains("_(") && !pokemonName.Contains("nidoran"))
            {
                pokemonName = pokemonName.Split("_(")[0];
            }

            if (pokemonName.Contains('_'))
            {
                pokemonName = pokemonName.Replace('_', ' ');
            }

            if (pokemonName == "flabe" || pokemonName == "flabeb" || pokemonName == "flabebe")
            {
                pokemonName = "Flabébé";
            }

            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            pokemonName = textInfo.ToTitleCase(pokemonName);

            return pokemonName;
        }

        /// <summary>
        /// Grabs a list of games, separated by the release dates.
        /// </summary>
        /// <returns>Returns a list of games, based on the release dates.</returns>
        private List<Game> GetGamesForEachReleaseDate()
        {
            List<Game> gameList = this.dataService.GetObjects<Game>("ReleaseDate, Id");
            if (!this.User.IsInRole("Owner"))
            {
                gameList = gameList.Where(x => x.ReleaseDate <= DateTime.UtcNow).ToList();
            }

            List<Game> games = new List<Game>();
            foreach (var r in gameList.ConvertAll(x => x.ReleaseDate).Distinct())
            {
                if (gameList.First(x => x.ReleaseDate == r).Id != 4)
                {
                    games.Add(new Game()
                    {
                        Id = gameList.First(x => x.ReleaseDate == r).Id,
                        Name = string.Join(" / ", gameList.Where(x => x.ReleaseDate == r).Select(x => x.Name)),
                        GenerationId = gameList.First(x => x.ReleaseDate == r).GenerationId,
                        ReleaseDate = r,
                        GameColor = gameList.First(x => x.ReleaseDate == r).GameColor,
                    });
                }
                else
                {
                    foreach (var g in gameList.Where(x => x.ReleaseDate == r).ToList())
                    {
                        games.Add(g);
                    }
                }
            }

            return games;
        }
    }
}

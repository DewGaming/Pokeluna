using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MoreLinq;
using Pokedex.DataAccess.Models;
using Pokedex.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pokedex.Controllers
{
    /// <summary>
    /// The class that handles all ShinyHunt related requests.
    /// </summary>
    [Authorize]
    [Route("")]
    public class ShinyHuntController : Controller
    {
        private readonly DataService dataService;

        private readonly AppConfig appConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShinyHuntController"/> class.
        /// </summary>
        /// <param name="appConfig">The application's configuration.</param>
        /// <param name="dataContext">The pokemon database context.</param>
        public ShinyHuntController(IOptions<AppConfig> appConfig, DataContext dataContext)
        {
            // Instantiate an instance of the data service.
            this.appConfig = appConfig.Value;
            this.dataService = new DataService(dataContext);
        }

        /// <summary>
        /// Shows shiny dex progress.
        /// </summary>
        /// <returns>The shiny dex progress page.</returns>
        [Route("shiny_dex_progress")]
        public IActionResult ShinyDexProgress()
        {
            this.dataService.AddPageView("Shiny Dex Progression Page", this.User.IsInRole("Owner"));
            int userId = this.dataService.GetCurrentUser(this.User).Id;
            List<ShinyHunt> shinyHunts = this.dataService.GetObjects<ShinyHunt>("Game.GenerationId, Pokemon.PokedexNumber, PokemonId, Id", "User, Pokemon, Pokemon.Game, Game, HuntingMethod, Mark, Pokeball", "UserId", userId).Where(x => x.IsCaptured && !x.ExcludeFromShinyDex).ToList();
            List<Pokemon> pokemonCaptured = shinyHunts.ConvertAll(x => x.Pokemon).DistinctBy(x => x.Id).ToList();
            List<Pokemon> pokemonList = this.dataService.GetNonBattlePokemonWithFormNames().Where(x => !x.IsShinyLocked).ToList();
            pokemonList = pokemonList.Where(x => x.Form == null || x.Form.Name != "Primal").ToList();

            List<PokemonShinyHuntDetails> pokemonShinyHuntList = pokemonList.Where(x => x.HasGenderDifference == false).ToList().ConvertAll(x => new PokemonShinyHuntDetails() { Pokemon = x, IsCaptured = false, IsAltForm = x.IsAltForm, IsMaleGenderDifference = false, IsFemaleGenderDifference = false });
            pokemonShinyHuntList.Where(x => pokemonCaptured.Any(y => y.Id == x.Pokemon.Id)).ToList().ForEach(x => x.IsCaptured = true);
            List<PokemonShinyHuntDetails> maleGenderDifferencesList = pokemonList.Where(x => x.HasGenderDifference == true).ToList().ConvertAll(x => new PokemonShinyHuntDetails() { Pokemon = x, IsCaptured = false, IsAltForm = x.IsAltForm, IsMaleGenderDifference = true, IsFemaleGenderDifference = false });
            maleGenderDifferencesList.Where(x => shinyHunts.Any(y => y.PokemonId == x.Pokemon.Id && y.Gender == "Male")).ToList().ForEach(x => x.IsCaptured = true);
            List<PokemonShinyHuntDetails> femaleGenderDifferencesList = pokemonList.Where(x => x.HasGenderDifference == true).ToList().ConvertAll(x => new PokemonShinyHuntDetails() { Pokemon = x, IsCaptured = false, IsAltForm = x.IsAltForm, IsMaleGenderDifference = false, IsFemaleGenderDifference = true });
            femaleGenderDifferencesList.Where(x => shinyHunts.Any(y => y.PokemonId == x.Pokemon.Id && y.Gender == "Female")).ToList().ForEach(x => x.IsCaptured = true);
            pokemonShinyHuntList.AddRange(maleGenderDifferencesList);
            pokemonShinyHuntList.AddRange(femaleGenderDifferencesList);

            ShinyDexViewModel model = new ShinyDexViewModel()
            {
                AllPokemon = pokemonShinyHuntList.OrderBy(x => x.Pokemon.PokedexNumber).ThenBy(x => x.Pokemon.Id).ToList(),
                AllShinyHunts = shinyHunts,
                AllGames = this.dataService.GetObjects<Game>(),
                CurrentUser = this.dataService.GetCurrentUser(this.User),
                IsShared = false,
                AppConfig = this.appConfig,
            };

            return this.View(model);
        }

        /// <summary>
        /// Shows shiny dex progress.
        /// </summary>
        /// <param name="username">The name of the user's shiny dex progress.</param>
        /// <returns>The shiny dex progress page.</returns>
        [AllowAnonymous]
        [Route("shiny_dex_progress/{username}")]
        public IActionResult SharableShinyDexProgress(string username)
        {
            User user = this.dataService.GetObjectByPropertyValue<User>("Username", username);
            if (user != null)
            {
                this.dataService.AddPageView("Shared Shiny Dex Progression Page", this.User.IsInRole("Owner"));
                List<ShinyHunt> shinyHunts = this.dataService.GetObjects<ShinyHunt>("Game.GenerationId, Pokemon.PokedexNumber, PokemonId, Id", "User, Pokemon, Pokemon.Game, Game, HuntingMethod, Mark, Pokeball", "UserId", user.Id).Where(x => x.IsCaptured && !x.ExcludeFromShinyDex).ToList();
                List<Pokemon> pokemonCaptured = shinyHunts.ConvertAll(x => x.Pokemon).DistinctBy(x => x.Id).ToList();
                List<Pokemon> pokemonList = this.dataService.GetNonBattlePokemonWithFormNames().Where(x => !x.IsShinyLocked).ToList();
                pokemonList = pokemonList.Where(x => x.Form == null || x.Form.Name != "Primal").ToList();

                List<PokemonShinyHuntDetails> pokemonShinyHuntList = pokemonList.Where(x => x.HasGenderDifference == false).ToList().ConvertAll(x => new PokemonShinyHuntDetails() { Pokemon = x, IsCaptured = false, IsAltForm = x.IsAltForm, IsMaleGenderDifference = false, IsFemaleGenderDifference = false });
                pokemonShinyHuntList.Where(x => pokemonCaptured.Any(y => y.Id == x.Pokemon.Id)).ToList().ForEach(x => x.IsCaptured = true);
                List<PokemonShinyHuntDetails> maleGenderDifferencesList = pokemonList.Where(x => x.HasGenderDifference == true).ToList().ConvertAll(x => new PokemonShinyHuntDetails() { Pokemon = x, IsCaptured = false, IsAltForm = x.IsAltForm, IsMaleGenderDifference = true, IsFemaleGenderDifference = false });
                maleGenderDifferencesList.Where(x => shinyHunts.Any(y => y.PokemonId == x.Pokemon.Id && y.Gender == "Male")).ToList().ForEach(x => x.IsCaptured = true);
                List<PokemonShinyHuntDetails> femaleGenderDifferencesList = pokemonList.Where(x => x.HasGenderDifference == true).ToList().ConvertAll(x => new PokemonShinyHuntDetails() { Pokemon = x, IsCaptured = false, IsAltForm = x.IsAltForm, IsMaleGenderDifference = false, IsFemaleGenderDifference = true });
                femaleGenderDifferencesList.Where(x => shinyHunts.Any(y => y.PokemonId == x.Pokemon.Id && y.Gender == "Female")).ToList().ForEach(x => x.IsCaptured = true);
                pokemonShinyHuntList.AddRange(maleGenderDifferencesList);
                pokemonShinyHuntList.AddRange(femaleGenderDifferencesList);

                ShinyDexViewModel model = new ShinyDexViewModel()
                {
                    AllPokemon = pokemonShinyHuntList.OrderBy(x => x.Pokemon.PokedexNumber).ThenBy(x => x.Pokemon.Id).ToList(),
                    AllShinyHunts = shinyHunts,
                    AllGames = this.dataService.GetObjects<Game>(),
                    CurrentUser = user,
                    IsShared = true,
                    AppConfig = this.appConfig,
                };

                return this.View("ShinyDexProgress", model);
            }
            else
            {
                return this.RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Starts a shiny hunt.
        /// </summary>
        /// <returns>The shiny hunt page.</returns>
        [HttpGet]
        [Route("start_hunt")]
        public IActionResult StartHunt()
        {
            StartShinyHuntViewModel model = new StartShinyHuntViewModel()
            {
                AllPokemon = this.dataService.GetHuntablePokemon(),
                UserId = this.dataService.GetCurrentUser(this.User).Id,
                AppConfig = this.appConfig,
            };

            return this.View(model);
        }

        /// <summary>
        /// Starts a shiny hunt.
        /// </summary>
        /// <param name="shinyHunt">The started shiny hunt.</param>
        /// <returns>The user's shiny hunt page.</returns>
        [HttpPost]
        [Route("start_hunt")]
        public IActionResult StartHunt(StartShinyHuntViewModel shinyHunt)
        {
            if (!this.ModelState.IsValid)
            {
                StartShinyHuntViewModel model = new StartShinyHuntViewModel()
                {
                    AllPokemon = this.dataService.GetHuntablePokemon(),
                    UserId = this.dataService.GetCurrentUser(this.User).Id,
                    AppConfig = this.appConfig,
                };

                model.AllPokemon.Prepend(new Pokemon()
                {
                    Id = 0,
                    PokedexNumber = 0,
                    Name = "Unknown",
                    IsShinyLocked = false,
                    IsComplete = true,
                    GenderRatioId = 10,
                });

                return this.View(model);
            }

            shinyHunt.IncrementAmount = 1;

            foreach (var p in shinyHunt.PokemonIds.Distinct())
            {
                shinyHunt.Id = 0;
                if (p != 0)
                {
                    shinyHunt.PokemonId = p;
                }

                this.dataService.AddObject(shinyHunt);
            }

            if (shinyHunt.HuntingMethodId == 8)
            {
                return this.RedirectToAction("ShinyFound", "ShinyHunt", new { shinyHuntId = shinyHunt.Id });
            }
            else
            {
                return this.RedirectToAction("IncompleteShinyHunts", "User");
            }
        }

        /// <summary>
        /// Starts a shiny hunt.
        /// </summary>
        /// <returns>The shiny hunt page.</returns>
        [HttpGet]
        [Route("add_completed_hunt")]
        public IActionResult AddCompletedHunt()
        {
            List<Pokemon> pokemonList = this.dataService.GetHuntablePokemon();
            AddCompletedShinyHuntViewModel model = new AddCompletedShinyHuntViewModel()
            {
                AllPokemon = pokemonList,
                DateOfCapture = DateTime.Now.ToLocalTime().Date,
                IncrementAmount = 1,
                UserId = this.dataService.GetCurrentUser(this.User).Id,
                AppConfig = this.appConfig,
            };

            return this.View(model);
        }

        /// <summary>
        /// Starts a shiny hunt.
        /// </summary>
        /// <param name="numberOfHunts">The number of hunts being added. Either one hunt or multiple.</param>
        /// <param name="shinyHunt">The started shiny hunt.</param>
        /// <returns>The user's shiny hunt page.</returns>
        [HttpPost]
        [Route("add_completed_hunt")]
        public IActionResult AddCompletedHunt(string numberOfHunts, AddCompletedShinyHuntViewModel shinyHunt)
        {
            if (!this.ModelState.IsValid)
            {
                List<Pokemon> pokemonList = this.dataService.GetHuntablePokemon();
                AddCompletedShinyHuntViewModel model = new AddCompletedShinyHuntViewModel()
                {
                    AllPokemon = pokemonList,
                    AllMarks = this.dataService.GetMarks(shinyHunt.GameId),
                    DateOfCapture = DateTime.Now.ToLocalTime().Date,
                    IncrementAmount = 1,
                    UserId = this.dataService.GetCurrentUser(this.User).Id,
                    AppConfig = this.appConfig,
                };

                return this.View(model);
            }

            shinyHunt.IsCaptured = true;

            if (shinyHunt.CurrentPhaseEncounters > 0 && shinyHunt.TotalEncounters == 0)
            {
                shinyHunt.TotalEncounters = shinyHunt.CurrentPhaseEncounters;
            }
            else if (shinyHunt.TotalEncounters > 0 && shinyHunt.CurrentPhaseEncounters == 0)
            {
                shinyHunt.CurrentPhaseEncounters = shinyHunt.TotalEncounters;
            }

            this.dataService.AddObject(shinyHunt);

            if (numberOfHunts == "multipleHunts")
            {
                return this.RedirectToAction("AddCompletedHunt", "ShinyHunt");
            }
            else
            {
                return this.RedirectToAction("CompletedShinyHunts", "ShinyHunt");
            }
        }

        /// <summary>
        /// Complete a shiny hunt.
        /// </summary>
        /// <param name="shinyHuntId">The completed shiny hunt's Id.</param>
        /// <returns>The shiny hunt page.</returns>
        [HttpGet]
        [Route("complete_shiny_hunt/{shinyHuntId:int}")]
        public IActionResult ShinyFound(int shinyHuntId)
        {
            ShinyHunt shinyHunt = this.dataService.GetObjectByPropertyValue<ShinyHunt>("Id", shinyHuntId, "Game");
            List<Pokemon> pokemonList = this.dataService.GetHuntablePokemon(shinyHunt.GameId);
            Pokemon pokemon;
            List<string> genders = new List<string>();
            if (shinyHunt.PokemonId == null)
            {
                pokemon = pokemonList.First();
                genders = this.dataService.GrabGenders(pokemonList.First().Id, "shinyHunt");
            }
            else
            {
                pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", shinyHunt.PokemonId);
                genders = this.dataService.GrabGenders(shinyHunt.PokemonId, "shinyHunt");
            }

            shinyHunt.TotalEncounters += shinyHunt.CurrentPhaseEncounters;

            CompleteShinyHuntViewModel model = new CompleteShinyHuntViewModel(shinyHunt)
            {
                Id = shinyHunt.Id,
                PokemonHunted = pokemon,
                GameHuntedIn = this.dataService.GetObjectByPropertyValue<Game>("Id", shinyHunt.GameId),
                DateOfCapture = DateTime.Now.ToLocalTime().Date,
                AllPokemon = pokemonList,
                AllPokeballs = this.dataService.GetPokeballs(shinyHunt.GameId, shinyHunt.HuntingMethodId, this.User, this.appConfig),
                AllGenders = genders,
                AllMarks = this.dataService.GetMarks(shinyHunt.GameId),
                AllHuntingMethods = this.dataService.GetObjects<HuntingMethodGameDetail>(includes: "HuntingMethod", whereProperty: "GameId", wherePropertyValue: shinyHunt.GameId).ConvertAll(x => x.HuntingMethod),
                AppConfig = this.appConfig,
            };

            return this.View(model);
        }

        /// <summary>
        /// Completed a shiny hunt.
        /// </summary>
        /// <param name="shinyHunt">The completed shiny hunt.</param>
        /// <returns>The user's shiny hunt page.</returns>
        [HttpPost]
        [Route("complete_shiny_hunt/{shinyHuntId:int}")]
        public IActionResult ShinyFound(CompleteShinyHuntViewModel shinyHunt)
        {
            if (this.ModelState.IsValid)
            {
                shinyHunt.IsCaptured = true;
                shinyHunt.TotalEncounters = shinyHunt.CurrentPhaseEncounters;
                List<ShinyHunt> phases = this.dataService.GetObjects<ShinyHunt>(whereProperty: "PhaseOfHuntId", wherePropertyValue: shinyHunt.Id);
                if (phases.Count() > 0)
                {
                    shinyHunt.TotalEncounters += phases.Select(x => x.CurrentPhaseEncounters).Sum();
                }

                if (shinyHunt.Phases < phases.Count())
                {
                    shinyHunt.Phases = phases.Count();
                }

                this.dataService.UpdateObject(shinyHunt);
            }

            return this.RedirectToAction("IncompleteShinyHunts", "User");
        }

        /// <summary>
        /// Complete a shiny hunt.
        /// </summary>
        /// <param name="shinyHuntId">The completed shiny hunt's Id.</param>
        /// <returns>The shiny hunt page.</returns>
        [HttpGet]
        [Route("found_phase_shiny/{shinyHuntId:int}")]
        public IActionResult PhaseShinyFound(int shinyHuntId)
        {
            TimeZoneInfo localZone = TimeZoneInfo.Local;
            ShinyHunt shinyHunt = this.dataService.GetObjectByPropertyValue<ShinyHunt>("Id", shinyHuntId, "Game");
            List<Pokemon> pokemonList = this.dataService.GetHuntablePokemon(shinyHunt.GameId);
            Pokemon pokemon;
            List<string> genders = new List<string>();
            if (shinyHunt.PokemonId == null)
            {
                pokemon = pokemonList.First();
                genders = this.dataService.GrabGenders(pokemonList.First().Id, "shinyHunt");
            }
            else
            {
                pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", (int)shinyHunt.PokemonId);
                genders = this.dataService.GrabGenders(shinyHunt.PokemonId, "shinyHunt");
            }

            CompleteShinyHuntViewModel model = new CompleteShinyHuntViewModel(shinyHunt)
            {
                PokemonHunted = pokemon,
                Nickname = string.Empty,
                GameHuntedIn = this.dataService.GetObjectByPropertyValue<Game>("Id", shinyHunt.GameId),
                DateOfCapture = DateTime.Now.Date,
                IsCaptured = true,
                AllPokemon = pokemonList,
                AllPokeballs = this.dataService.GetPokeballs(shinyHunt.GameId, shinyHunt.HuntingMethodId, this.User, this.appConfig),
                AllGenders = genders,
                AllMarks = this.dataService.GetMarks(shinyHunt.GameId),
                AllHuntingMethods = this.dataService.GetObjects<HuntingMethodGameDetail>(includes: "HuntingMethod", whereProperty: "GameId", wherePropertyValue: shinyHunt.GameId).ConvertAll(x => x.HuntingMethod),
                AppConfig = this.appConfig,
            };

            return this.View(model);
        }

        /// <summary>
        /// Completed a shiny hunt.
        /// </summary>
        /// <param name="shinyHunt">The completed shiny hunt.</param>
        /// <param name="shinyHuntId">The original shiny hunt's Id.</param>
        /// <returns>The user's shiny hunt page.</returns>
        [HttpPost]
        [Route("found_phase_shiny/{shinyHuntId:int}")]
        public IActionResult PhaseShinyFound(CompleteShinyHuntViewModel shinyHunt, int shinyHuntId)
        {
            if (this.ModelState.IsValid)
            {
                shinyHunt.TotalEncounters = shinyHunt.CurrentPhaseEncounters;
                shinyHunt.PhaseOfHuntId = shinyHuntId;
                if (!shinyHunt.IsCaptured)
                {
                    shinyHunt.PokeballId = null;
                }

                this.dataService.AddObject(shinyHunt);

                ShinyHunt originalShinyHunt = this.dataService.GetObjectByPropertyValue<ShinyHunt>("Id", shinyHuntId);
                originalShinyHunt.TotalEncounters += originalShinyHunt.CurrentPhaseEncounters;
                originalShinyHunt.CurrentPhaseEncounters = 0;
                this.dataService.UpdateObject(originalShinyHunt);
            }

            return this.RedirectToAction("IncompleteShinyHunts", "User");
        }

        /// <summary>
        /// Updates an incomplete shiny hunt.
        /// </summary>
        /// <param name="shinyHuntId">The shiny hunt's id.</param>
        /// <returns>The shiny hunt edit page.</returns>
        [HttpGet]
        [Route("edit_incomplete_hunt/{shinyHuntId:int}")]
        public IActionResult EditIncompleteShinyHunt(int shinyHuntId)
        {
            ShinyHunt shinyHunt = this.dataService.GetObjectByPropertyValue<ShinyHunt>("Id", shinyHuntId, "Pokemon");
            List<string> genders = this.dataService.GrabGenders(shinyHunt.PokemonId, "shinyHunt");
            if (genders.First() == string.Empty)
            {
                genders.Remove(string.Empty);
            }

            EditShinyHuntViewModel model = new EditShinyHuntViewModel(shinyHunt)
            {
                AllPokemon = this.dataService.GetHuntablePokemon(),
                AllPokeballs = this.dataService.GetPokeballs(shinyHunt.GameId, shinyHunt.HuntingMethodId, this.User, this.appConfig),
                AllMarks = this.dataService.GetMarks(shinyHunt.GameId),
                AllGenders = genders,
                AllSweets = this.dataService.GetObjects<Sweet>(),
                AllGames = this.dataService.GetShinyHuntGames(shinyHunt.PokemonId),
                AllHuntingMethods = this.dataService.GetObjects<HuntingMethodGameDetail>(includes: "HuntingMethod", whereProperty: "GameId", wherePropertyValue: shinyHunt.GameId).ConvertAll(x => x.HuntingMethod),
                AppConfig = this.appConfig,
            };

            model.AllPokemon.Prepend(new Pokemon()
            {
                Id = 0,
                PokedexNumber = 0,
                Name = "Unknown",
                IsShinyLocked = false,
                IsComplete = true,
                GenderRatioId = 10,
            });

            return this.View(model);
        }

        /// <summary>
        /// Updates an incomplete shiny hunt.
        /// </summary>
        /// <param name="shinyHunt">The editted shiny hunt.</param>
        /// <returns>The user's shiny hunt page.</returns>
        [HttpPost]
        [Route("edit_incomplete_hunt/{shinyHuntId:int}")]
        public IActionResult EditIncompleteShinyHunt(EditShinyHuntViewModel shinyHunt)
        {
            if (!this.ModelState.IsValid)
            {
                ShinyHunt oldShinyHunt = this.dataService.GetObjectByPropertyValue<ShinyHunt>("Id", shinyHunt.Id, "Pokemon");
                List<string> genders = this.dataService.GrabGenders(oldShinyHunt.PokemonId, "shinyHunt");
                if (genders.First() == string.Empty)
                {
                    genders.Remove(string.Empty);
                }

                EditShinyHuntViewModel model = new EditShinyHuntViewModel(oldShinyHunt)
                {
                    AllPokemon = this.dataService.GetHuntablePokemon(),
                    AllPokeballs = this.dataService.GetPokeballs(oldShinyHunt.GameId, oldShinyHunt.HuntingMethodId, this.User, this.appConfig),
                    AllGenders = genders,
                    AllSweets = this.dataService.GetObjects<Sweet>(),
                    AllGames = this.dataService.GetShinyHuntGames(oldShinyHunt.PokemonId),
                    AllHuntingMethods = this.dataService.GetObjects<HuntingMethodGameDetail>(includes: "HuntingMethod", whereProperty: "GameId", wherePropertyValue: oldShinyHunt.GameId).ConvertAll(x => x.HuntingMethod),
                    AppConfig = this.appConfig,
                };

                model.AllPokemon.Prepend(new Pokemon()
                {
                    Id = 0,
                    PokedexNumber = 0,
                    Name = "Unknown",
                    IsShinyLocked = false,
                    IsComplete = true,
                    GenderRatioId = 10,
                });

                return this.View(model);
            }

            this.dataService.UpdateObject(shinyHunt);

            return this.RedirectToAction("IncompleteShinyHunts", "User");
        }

        /// <summary>
        /// Updates a complete shiny hunt.
        /// </summary>
        /// <param name="shinyHuntId">The shiny hunt's id.</param>
        /// <returns>The shiny hunt edit page.</returns>
        [HttpGet]
        [Route("edit_complete_hunt/{shinyHuntId:int}")]
        public IActionResult EditCompleteShinyHunt(int shinyHuntId)
        {
            ShinyHunt shinyHunt = this.dataService.GetObjectByPropertyValue<ShinyHunt>("Id", shinyHuntId, "Pokemon, Game");
            List<Game> gamesList = this.dataService.GetShinyHuntGames(shinyHunt.PokemonId);

            EditShinyHuntViewModel model = new EditShinyHuntViewModel(shinyHunt)
            {
                PokemonHunted = shinyHunt.Pokemon,
                GameHuntedIn = shinyHunt.Game,
                AllPokemon = this.dataService.GetHuntablePokemon(),
                AllGames = gamesList,
                AllHuntingMethods = this.dataService.GetObjects<HuntingMethodGameDetail>(includes: "HuntingMethod", whereProperty: "GameId", wherePropertyValue: shinyHunt.GameId).ConvertAll(x => x.HuntingMethod),
                AllPokeballs = this.dataService.GetPokeballs(shinyHunt.GameId, shinyHunt.HuntingMethodId, this.User, this.appConfig),
                AllMarks = this.dataService.GetMarks(shinyHunt.GameId),
                AllSweets = this.dataService.GetObjects<Sweet>(),
                AllGenders = this.dataService.GrabGenders(shinyHunt.PokemonId, "shinyHunt"),
                AppConfig = this.appConfig,
            };

            return this.View(model);
        }

        /// <summary>
        /// Updates a complete shiny hunt.
        /// </summary>
        /// <param name="shinyHunt">The editted shiny hunt.</param>
        /// <returns>The user's shiny hunt page.</returns>
        [HttpPost]
        [Route("edit_complete_hunt/{shinyHuntId:int}")]
        public IActionResult EditCompleteShinyHunt(EditShinyHuntViewModel shinyHunt)
        {
            if (!this.ModelState.IsValid)
            {
                ShinyHunt oldShinyHunt = this.dataService.GetObjectByPropertyValue<ShinyHunt>("Id", shinyHunt.Id, "Pokemon, Game");
                List<Game> gamesList = this.dataService.GetShinyHuntGames(oldShinyHunt.PokemonId);
                List<string> genders = this.dataService.GrabGenders(oldShinyHunt.PokemonId, "shinyHunt");

                EditShinyHuntViewModel model = new EditShinyHuntViewModel(oldShinyHunt)
                {
                    PokemonHunted = oldShinyHunt.Pokemon,
                    GameHuntedIn = oldShinyHunt.Game,
                    AllPokemon = this.dataService.GetHuntablePokemon(),
                    AllGames = gamesList,
                    AllHuntingMethods = this.dataService.GetObjects<HuntingMethodGameDetail>(includes: "HuntingMethod", whereProperty: "GameId", wherePropertyValue: oldShinyHunt.GameId).ConvertAll(x => x.HuntingMethod),
                    AllPokeballs = this.dataService.GetPokeballs(oldShinyHunt.GameId, oldShinyHunt.HuntingMethodId, this.User, this.appConfig),
                    AllMarks = this.dataService.GetMarks(oldShinyHunt.GameId),
                    AllSweets = this.dataService.GetObjects<Sweet>(),
                    AllGenders = genders,
                    UserId = this.dataService.GetCurrentUser(this.User).Id,
                    AppConfig = this.appConfig,
                };

                return this.View(model);
            }

            if (!shinyHunt.IsCaptured)
            {
                shinyHunt.PokeballId = null;
            }

            shinyHunt.TotalEncounters = shinyHunt.CurrentPhaseEncounters;
            List<ShinyHunt> allHunts = this.dataService.GetObjects<ShinyHunt>();
            List<ShinyHunt> phases = allHunts.Where(x => x.PhaseOfHuntId == shinyHunt.Id).ToList();
            if (phases.Count() > 0)
            {
                shinyHunt.TotalEncounters += phases.Select(x => x.CurrentPhaseEncounters).Sum();
            }

            this.dataService.UpdateObject(shinyHunt);

            if (shinyHunt.PhaseOfHuntId != null)
            {
                ShinyHunt parentHunt = allHunts.Find(x => x.Id == shinyHunt.PhaseOfHuntId);
                parentHunt.TotalEncounters = parentHunt.CurrentPhaseEncounters;
                parentHunt.TotalEncounters += shinyHunt.CurrentPhaseEncounters;
                phases = allHunts.Where(x => x.PhaseOfHuntId == parentHunt.Id && x.Id != shinyHunt.Id).ToList();
                if (phases.Count() > 0)
                {
                    parentHunt.TotalEncounters += phases.Select(x => x.CurrentPhaseEncounters).Sum();
                }

                this.dataService.UpdateObject(parentHunt);
            }

            return this.RedirectToAction("CompletedShinyHunts", "ShinyHunt");
        }

        /// <summary>
        /// Grabs all of the prior phases for a shiny hunt.
        /// </summary>
        /// <param name="shinyHuntId">The shiny hunt.</param>
        /// <returns>The shiny hunt phases page.</returns>
        [Route("shiny_hunt_phases/{shinyHuntId:int}")]
        public IActionResult ShinyHuntPhases(int shinyHuntId)
        {
            List<ShinyHunt> shinyHunts = this.dataService.GetObjects<ShinyHunt>(includes: "Pokemon, Pokemon.Form, Mark, Pokeball, Sweet, Game, HuntingMethod");
            List<ShinyHunt> phases = shinyHunts.Where(x => x.PhaseOfHuntId == shinyHuntId).ToList();
            List<Game> gamesList = this.dataService.GetObjects<Game>("ReleaseDate, Id").Where(x => x.ReleaseDate <= DateTime.UtcNow).ToList();
            gamesList = gamesList.Where(x => phases.DistinctBy(x => x.Game).Any(y => y.Game.ReleaseDate == x.ReleaseDate)).ToList();
            List<Game> edittedGamesList = new List<Game>();
            foreach (var r in gamesList.ConvertAll(x => x.ReleaseDate).Distinct())
            {
                if (gamesList.First(x => x.ReleaseDate == r).Id != 4)
                {
                    edittedGamesList.Add(new Game()
                    {
                        Id = gamesList.First(x => x.ReleaseDate == r).Id,
                        Name = string.Join(" / ", gamesList.Where(x => x.ReleaseDate == r).Select(x => x.Name)),
                        GenerationId = gamesList.First(x => x.ReleaseDate == r).GenerationId,
                        ReleaseDate = r,
                        GameColor = gamesList.First(x => x.ReleaseDate == r).GameColor,
                    });
                }
                else
                {
                    foreach (var g in gamesList.Where(x => x.ReleaseDate == r).ToList())
                    {
                        edittedGamesList.Add(g);
                    }
                }
            }

            phases.ForEach(x => x.Game.Name = edittedGamesList.Find(y => y.Id == x.GameId).Name);
            shinyHunts.Where(x => x.Pokemon.IsAltForm).ToList().ForEach(x => x.Pokemon.Name = x.Pokemon.NameWithForm);
            ShinyHuntsViewModel model = new ShinyHuntsViewModel()
            {
                AllShinyHunts = phases.OrderBy(x => x.DateOfCapture).ThenBy(x => x.Id).ToList(),
                ShinyHunt = shinyHunts.Find(x => x.Id == shinyHuntId),
                IsShared = false,
                AppConfig = this.appConfig,
            };

            this.dataService.AddPageView("Shiny Hunt Phases Page", this.User.IsInRole("Owner"));
            return this.View("ShinyHuntPhases", model);
        }

        /// <summary>
        /// Grabs all of the prior phases for a shiny hunt.
        /// </summary>
        /// <param name="username">The username of the user being searched.</param>
        /// <param name="shinyHuntId">The shiny hunt.</param>
        /// <returns>The shiny hunt phases page.</returns>
        [AllowAnonymous]
        [Route("shiny_hunt_phases/{username}/{shinyHuntId:int}")]
        public IActionResult ShareableShinyHuntPhases(string username, int shinyHuntId)
        {
            List<ShinyHunt> shinyHunts = this.dataService.GetObjects<ShinyHunt>(includes: "Pokemon, Mark, Pokeball, Sweet, Game, HuntingMethod");
            List<ShinyHunt> phases = shinyHunts.Where(x => x.PhaseOfHuntId == shinyHuntId).ToList();
            List<Game> gamesList = this.dataService.GetObjects<Game>("ReleaseDate, Id").Where(x => x.ReleaseDate <= DateTime.UtcNow).ToList();
            gamesList = gamesList.Where(x => phases.DistinctBy(x => x.Game).Any(y => y.Game.ReleaseDate == x.ReleaseDate)).ToList();
            List<Game> edittedGamesList = new List<Game>();
            foreach (var r in gamesList.ConvertAll(x => x.ReleaseDate).Distinct())
            {
                if (gamesList.First(x => x.ReleaseDate == r).Id != 4)
                {
                    edittedGamesList.Add(new Game()
                    {
                        Id = gamesList.First(x => x.ReleaseDate == r).Id,
                        Name = string.Join(" / ", gamesList.Where(x => x.ReleaseDate == r).Select(x => x.Name)),
                        GenerationId = gamesList.First(x => x.ReleaseDate == r).GenerationId,
                        ReleaseDate = r,
                        GameColor = gamesList.First(x => x.ReleaseDate == r).GameColor,
                    });
                }
                else
                {
                    foreach (var g in gamesList.Where(x => x.ReleaseDate == r).ToList())
                    {
                        edittedGamesList.Add(g);
                    }
                }
            }

            phases.ForEach(x => x.Game.Name = edittedGamesList.Find(y => y.Id == x.GameId).Name);
            ShinyHuntsViewModel model = new ShinyHuntsViewModel()
            {
                AllShinyHunts = phases,
                ShinyHunt = shinyHunts.Find(x => x.Id == shinyHuntId),
                IsShared = true,
                AppConfig = this.appConfig,
            };

            this.dataService.AddPageView("Shiny Hunt Phases Page", this.User.IsInRole("Owner"));
            return this.View("ShinyHuntPhases", model);
        }

        /// <summary>
        /// Transport the user to their shiny hunt page.
        /// </summary>
        /// <returns>The shiny hunt page.</returns>
        [Route("completed_shiny_hunts")]
        public IActionResult CompletedShinyHunts()
        {
            this.dataService.AddPageView("Completed Shiny Hunting Page", this.User.IsInRole("Owner"));
            List<ShinyHunt> shinyHunts = this.dataService.GetObjects<ShinyHunt>("Game.GenerationId, Pokemon.PokedexNumber, PokemonId, Id", "User, Pokemon.Form, Game, HuntingMethod, Mark, Sweet, Pokeball, PhaseOfHunt, PhaseOfHunt.Pokemon, PhaseOfHunt.Pokemon.Form", "User.Username", this.User.Identity.Name).Where(x => x.IsCaptured).ToList();
            List<Game> gamesList = this.dataService.GetObjects<Game>("ReleaseDate, Id").Where(x => x.ReleaseDate <= DateTime.UtcNow).ToList();
            gamesList = gamesList.Where(x => shinyHunts.DistinctBy(x => x.Game).Any(y => y.Game.ReleaseDate == x.ReleaseDate)).ToList();
            if (shinyHunts.Count(x => x.Game.Name == "Fire Red") > 0)
            {
                if (shinyHunts.Count(x => x.Game.Name == "Leaf Green") == 0)
                {
                    gamesList.Remove(gamesList.Find(x => x.Name == "Leaf Green"));
                }
            }
            else if (shinyHunts.Count(x => x.Game.Name == "Leaf Green") > 0)
            {
                    gamesList.Remove(gamesList.Find(x => x.Name == "Fire Red"));
            }

            List<Game> edittedGamesList = new List<Game>();
            foreach (var r in gamesList.ConvertAll(x => x.ReleaseDate).Distinct())
            {
                if (gamesList.First(x => x.ReleaseDate == r).Id != 4)
                {
                    edittedGamesList.Add(new Game()
                    {
                        Id = gamesList.First(x => x.ReleaseDate == r).Id,
                        Name = string.Join(" / ", gamesList.Where(x => x.ReleaseDate == r).Select(x => x.Name)),
                        GenerationId = gamesList.First(x => x.ReleaseDate == r).GenerationId,
                        ReleaseDate = r,
                        GameColor = gamesList.First(x => x.ReleaseDate == r).GameColor,
                    });
                }
                else
                {
                    foreach (var g in gamesList.Where(x => x.ReleaseDate == r).ToList())
                    {
                        edittedGamesList.Add(g);
                    }
                }
            }

            shinyHunts.ForEach(x => x.Game.Name = edittedGamesList.Find(y => y.Id == x.GameId).Name);
            shinyHunts.Where(x => x.Pokemon.IsAltForm).ToList().ForEach(x => x.Pokemon.Name = x.Pokemon.NameWithForm);
            shinyHunts.Where(x => x.PhaseOfHunt != null && x.PhaseOfHunt.Pokemon.IsAltForm).ToList().ForEach(x => x.PhaseOfHunt.Pokemon.Name = x.PhaseOfHunt.Pokemon.NameWithForm);

            ShinyHuntsViewModel model = new ShinyHuntsViewModel()
            {
                AllShinyHunts = shinyHunts.OrderBy(x => x.Pokemon.PokedexNumber).ThenBy(x => x.PokemonId).ToList(),
                EdittedGames = edittedGamesList.OrderBy(x => x.ReleaseDate).ThenBy(x => x.Id).ToList(),
                UnedittedGames = gamesList,
                Username = this.User.Identity.Name,
                IsShared = false,
                AppConfig = this.appConfig,
            };

            model.ShinyHuntCount = model.AllShinyHunts.Count();

            return this.View(model);
        }

        /// <summary>
        /// Transport the user to their shiny hunt page.
        /// </summary>
        /// <param name="username">The username of the user being searched.</param>
        /// <returns>The shiny hunt page.</returns>
        [AllowAnonymous]
        [Route("completed_shiny_hunts/{username}")]
        public IActionResult ShareableCompletedShinyHunts(string username)
        {
            User user = this.dataService.GetObjectByPropertyValue<User>("Username", username);
            if (user != null)
            {
                this.dataService.AddPageView("Share Completed Shiny Hunting Page", this.User.IsInRole("Owner"));
                List<ShinyHunt> shinyHunts = this.dataService.GetObjects<ShinyHunt>("Game.GenerationId, Pokemon.PokedexNumber, PokemonId, Id", "User, Pokemon, Pokemon.Form, Game, HuntingMethod, Mark, Pokeball, PhaseOfHunt, PhaseOfHunt.Pokemon, PhaseOfHunt.Pokemon.Form", "User.Username", user.Username);
                List<Game> gamesList = this.dataService.GetObjects<Game>("ReleaseDate, Id").Where(x => x.ReleaseDate <= DateTime.UtcNow).ToList();
                shinyHunts = shinyHunts.Where(x => x.IsCaptured).ToList();
                gamesList = gamesList.Where(x => shinyHunts.DistinctBy(x => x.Game).Any(y => y.Game.ReleaseDate == x.ReleaseDate)).ToList();
                if (shinyHunts.Count(x => x.Game.Name == "Fire Red") > 0)
                {
                    if (shinyHunts.Count(x => x.Game.Name == "Leaf Green") == 0)
                    {
                        gamesList.Remove(gamesList.Find(x => x.Name == "Leaf Green"));
                    }
                }
                else if (shinyHunts.Count(x => x.Game.Name == "Leaf Green") > 0)
                {
                        gamesList.Remove(gamesList.Find(x => x.Name == "Fire Red"));
                }

                List<Game> edittedGamesList = new List<Game>();
                foreach (var r in gamesList.ConvertAll(x => x.ReleaseDate).Distinct())
                {
                    if (gamesList.First(x => x.ReleaseDate == r).Id != 4)
                    {
                        edittedGamesList.Add(new Game()
                        {
                            Id = gamesList.First(x => x.ReleaseDate == r).Id,
                            Name = string.Join(" / ", gamesList.Where(x => x.ReleaseDate == r).Select(x => x.Name)),
                            GenerationId = gamesList.First(x => x.ReleaseDate == r).GenerationId,
                            ReleaseDate = r,
                            GameColor = gamesList.First(x => x.ReleaseDate == r).GameColor,
                        });
                    }
                    else
                    {
                        foreach (var g in gamesList.Where(x => x.ReleaseDate == r).ToList())
                        {
                            edittedGamesList.Add(g);
                        }
                    }
                }

                shinyHunts.ForEach(x => x.Game.Name = edittedGamesList.Find(y => y.Id == x.GameId).Name);
                shinyHunts.Where(x => x.PokemonId == null).ToList().ForEach(x => x.Pokemon = new Pokemon() { Id = 0, Name = "Unknown", PokedexNumber = 0 });
                shinyHunts.Where(x => x.Pokemon.IsAltForm).ToList().ForEach(x => x.Pokemon.Name = x.Pokemon.NameWithForm);
                shinyHunts.Where(x => x.PhaseOfHunt != null && x.PhaseOfHunt.Pokemon.IsAltForm).ToList().ForEach(x => x.PhaseOfHunt.Pokemon.Name = x.PhaseOfHunt.Pokemon.NameWithForm);

                ShinyHuntsViewModel model = new ShinyHuntsViewModel()
                {
                    AllShinyHunts = shinyHunts.OrderBy(x => x.Pokemon.PokedexNumber).ThenBy(x => x.PokemonId).ToList(),
                    EdittedGames = edittedGamesList.OrderBy(x => x.ReleaseDate).ThenBy(x => x.Id).ToList(),
                    UnedittedGames = gamesList,
                    Username = username,
                    IsShared = true,
                    AppConfig = this.appConfig,
                };

                model.ShinyHuntCount = model.AllShinyHunts.Count();

                return this.View("CompletedShinyHunts", model);
            }
            else
            {
                return this.RedirectToAction("Index", "Home");
            }
        }
    }
}

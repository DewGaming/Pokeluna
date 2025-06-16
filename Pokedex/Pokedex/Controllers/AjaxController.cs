using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MoreLinq;
using Pokedex.DataAccess.Models;
using Pokedex.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;

namespace Pokedex.Controllers
{
    /// <summary>
    /// The class that handles all AJAX requests.
    /// </summary>
    [Route("")]
    public class AjaxController : Controller
    {
        private readonly DataService dataService;

        private readonly AppConfig appConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="AjaxController"/> class.
        /// </summary>
        /// <param name="appConfig">The application's config.</param>
        /// <param name="dataContext">The database's context.</param>
        public AjaxController(IOptions<AppConfig> appConfig, DataContext dataContext)
        {
            this.appConfig = appConfig.Value;
            this.dataService = new DataService(dataContext);
        }

        /// <summary>
        /// Marks a message as read.
        /// </summary>
        [Route("mark-as-read")]
        public void MarkAsRead()
        {
            List<Message> messages = this.dataService.GetObjects<Message>(includes: "Receiver", whereProperty: "Receiver.Username", wherePropertyValue: this.User.Identity.Name);
            foreach (var m in messages.Where(x => !x.IsRead))
            {
                m.IsRead = true;
                this.dataService.UpdateObject(m);
            }
        }

        /// <summary>
        /// Checks to see if there are any unread messages for the user.
        /// </summary>
        /// <returns>The amount of unread messages.</returns>
        [Route("check-unread-messages")]
        public List<int> CheckUnreadMessages()
        {
            List<Message> messages = this.dataService.GetObjects<Message>(includes: "Receiver", whereProperty: "Receiver.Username", wherePropertyValue: this.User.Identity.Name).ToList();
            List<int> counts = new List<int>()
            {
                messages.Where(x => !x.IsSeen).Count(),
                messages.Where(x => !x.IsRead).Count(),
            };

            foreach (var m in messages.Where(x => !x.IsSeen).ToList())
            {
                m.IsSeen = true;
                this.dataService.UpdateObject(m);
            }

            return counts;
        }

        /// <summary>
        /// Prepares the required information for the admin to view the admin pokemon page.
        /// </summary>
        /// <param name="generationId">The generation requested by the admin.</param>
        /// <returns>The fill admin generation table shared view.</returns>
        [Route("get-pokemon-by-generation-admin/{generationId}")]
        public IActionResult GetPokemonByGenerationAdmin(int generationId)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                List<Pokemon> pokemonList = this.dataService.GetObjects<Pokemon>("PokedexNumber, Id", "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form");
                AllAdminPokemonViewModel allAdminPokemon = this.dataService.GetAllAdminPokemonDetails();
                DropdownViewModel dropdownViewModel = new DropdownViewModel()
                {
                    AllPokemon = allAdminPokemon,
                    AppConfig = this.appConfig,
                    GenerationId = generationId,
                };

                AdminGenerationTableViewModel model = new AdminGenerationTableViewModel()
                {
                    PokemonList = pokemonList.Where(x => !x.IsAltForm).Where(x => x.Game.GenerationId == generationId).ToList(),
                    DropdownViewModel = dropdownViewModel,
                    AppConfig = this.appConfig,
                };

                return this.PartialView("_FillAdminGenerationTable", model);
            }
            else
            {
                return this.RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Prepares the required information for the admin to view all incomplete pokemon.
        /// </summary>
        /// <returns>The fill admin generation table shared view.</returns>
        [Route("get-incomplete-pokemon-admin")]
        public IActionResult GetIncompletePokemonAdmin()
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                AllAdminPokemonViewModel allAdminPokemon = this.dataService.GetAllAdminPokemonDetails();
                DropdownViewModel dropdownViewModel = new DropdownViewModel()
                {
                    AllPokemon = allAdminPokemon,
                    AppConfig = this.appConfig,
                    GenerationId = this.dataService.GetObjects<Generation>().Last().Id,
                };

                AdminGenerationTableViewModel model = new AdminGenerationTableViewModel()
                {
                    PokemonList = this.dataService.GetObjects<Pokemon>("PokedexNumber, Id", "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form").Where(x => !x.IsComplete).ToList(),
                    DropdownViewModel = dropdownViewModel,
                    AppConfig = this.appConfig,
                };

                model.PokemonList.Where(x => x.IsAltForm).ForEach(x => x.Name = x.NameWithForm);

                return this.PartialView("_FillAdminGenerationTable", model);
            }
            else
            {
                return this.RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Prepares the required information for the admin to view all incomplete pokemon.
        /// </summary>
        /// <returns>The fill admin generation table shared view.</returns>
        [Route("get-missing-threed-pokemon-admin")]
        public IActionResult GetMissingThreeDPokemonAdmin()
        {
            AllAdminPokemonViewModel allAdminPokemon = this.dataService.GetAllAdminPokemonDetails();
            List<Pokemon> pokemonList = this.dataService.GetObjects<Pokemon>("PokedexNumber, Id", "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form", "HasHomeArtwork", false);

            DropdownViewModel dropdownViewModel = new DropdownViewModel()
            {
                AllPokemon = allAdminPokemon,
                AppConfig = this.appConfig,
                GenerationId = this.dataService.GetObjects<Generation>().Last().Id,
            };

            AdminGenerationTableViewModel model = new AdminGenerationTableViewModel()
            {
                PokemonList = pokemonList,
                DropdownViewModel = dropdownViewModel,
                AppConfig = this.appConfig,
            };

            model.PokemonList.Where(x => x.IsAltForm).ToList().ForEach(x => x.Name = x.NameWithForm);

            return this.PartialView("_FillAdminGenerationTable", model);
        }

        /// <summary>
        /// Prepares the required information for the admin to view all pokemon missing shiny artwork.
        /// </summary>
        /// <returns>The fill admin generation table shared view.</returns>
        [Route("get-missing-shiny-pokemon-admin")]
        public IActionResult GetMissingShinyPokemonAdmin()
        {
            AllAdminPokemonViewModel allAdminPokemon = this.dataService.GetAllAdminPokemonDetails();
            List<Pokemon> pokemonList = this.dataService.GetObjects<Pokemon>("PokedexNumber, Id", "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form", "HasShinyArtwork", false);

            DropdownViewModel dropdownViewModel = new DropdownViewModel()
            {
                AllPokemon = allAdminPokemon,
                AppConfig = this.appConfig,
                GenerationId = this.dataService.GetObjects<Generation>().Last().Id,
            };

            AdminGenerationTableViewModel model = new AdminGenerationTableViewModel()
            {
                PokemonList = pokemonList,
                DropdownViewModel = dropdownViewModel,
                AppConfig = this.appConfig,
            };

            model.PokemonList.Where(x => x.IsAltForm).ToList().ForEach(x => x.Name = x.NameWithForm);

            return this.PartialView("_FillAdminGenerationTable", model);
        }

        /// <summary>
        /// Exports a user's team into a format that can be imported into pokemon showdown.
        /// </summary>
        /// <param name="teamId">The team being exported.</param>
        /// <returns>A list of all the pokemon exported to be copied to clipboard.</returns>
        [Route("grab-user-pokemon-team")]
        public ExportPokemonViewModel ExportUserPokemonTeam(int teamId)
        {
            PokemonTeam pokemonTeam = this.dataService.GetObjectByPropertyValue<PokemonTeam>("Id", teamId, "Game, FirstPokemon, FirstPokemon.Pokemon, FirstPokemon.Pokemon.Game, FirstPokemon.Ability, FirstPokemon.PokemonTeamEV, FirstPokemon.PokemonTeamIV, FirstPokemon.PokemonTeamMoveset, FirstPokemon.Nature, FirstPokemon.TeraType, SecondPokemon, SecondPokemon.Pokemon, SecondPokemon.Pokemon.Game, SecondPokemon.Ability, SecondPokemon.PokemonTeamEV, SecondPokemon.PokemonTeamIV, SecondPokemon.PokemonTeamMoveset, SecondPokemon.Nature, SecondPokemon.TeraType, ThirdPokemon, ThirdPokemon.Pokemon, ThirdPokemon.Pokemon.Game, ThirdPokemon.Ability, ThirdPokemon.PokemonTeamEV, ThirdPokemon.PokemonTeamIV, ThirdPokemon.PokemonTeamMoveset, ThirdPokemon.Nature, ThirdPokemon.TeraType, FourthPokemon, FourthPokemon.Pokemon, FourthPokemon.Pokemon.Game, FourthPokemon.Ability, FourthPokemon.PokemonTeamEV, FourthPokemon.PokemonTeamIV, FourthPokemon.PokemonTeamMoveset, FourthPokemon.Nature, FourthPokemon.TeraType, FifthPokemon, FifthPokemon.Pokemon, FifthPokemon.Pokemon.Game, FifthPokemon.Ability, FifthPokemon.PokemonTeamEV, FifthPokemon.PokemonTeamIV, FifthPokemon.PokemonTeamMoveset, FifthPokemon.Nature, FifthPokemon.TeraType, SixthPokemon, SixthPokemon.Pokemon, SixthPokemon.Pokemon.Game, SixthPokemon.Ability, SixthPokemon.PokemonTeamEV, SixthPokemon.PokemonTeamIV, SixthPokemon.PokemonTeamMoveset, SixthPokemon.Nature, SixthPokemon.TeraType, User");
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest" && (pokemonTeam.User.Username == this.User.Identity.Name || this.User.IsInRole("Owner")))
            {
                return this.ExportTeamString(pokemonTeam);
            }

            return null;
        }

        /// <summary>
        /// Exports all of a user's teams into a format that can be imported into pokemon showdown.
        /// </summary>
        /// <returns>A list of all the pokemon exported to be copied to clipboard.</returns>
        [Route("grab-all-user-pokemon-teams")]
        public List<ExportPokemonViewModel> ExportAllUserPokemonTeams()
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                List<PokemonTeam> pokemonTeams = this.dataService.GetObjects<PokemonTeam>("Id", "Game, FirstPokemon, FirstPokemon.Pokemon, FirstPokemon.Pokemon.Game, FirstPokemon.Ability, FirstPokemon.PokemonTeamEV, FirstPokemon.PokemonTeamIV, FirstPokemon.PokemonTeamMoveset, FirstPokemon.Nature, FirstPokemon.TeraType, SecondPokemon, SecondPokemon.Pokemon, SecondPokemon.Pokemon.Game, SecondPokemon.Ability, SecondPokemon.PokemonTeamEV, SecondPokemon.PokemonTeamIV, SecondPokemon.PokemonTeamMoveset, SecondPokemon.Nature, SecondPokemon.TeraType, ThirdPokemon, ThirdPokemon.Pokemon, ThirdPokemon.Pokemon.Game, ThirdPokemon.Ability, ThirdPokemon.PokemonTeamEV, ThirdPokemon.PokemonTeamIV, ThirdPokemon.PokemonTeamMoveset, ThirdPokemon.Nature, ThirdPokemon.TeraType, FourthPokemon, FourthPokemon.Pokemon, FourthPokemon.Pokemon.Game, FourthPokemon.Ability, FourthPokemon.PokemonTeamEV, FourthPokemon.PokemonTeamIV, FourthPokemon.PokemonTeamMoveset, FourthPokemon.Nature, FourthPokemon.TeraType, FifthPokemon, FifthPokemon.Pokemon, FifthPokemon.Pokemon.Game, FifthPokemon.Ability, FifthPokemon.PokemonTeamEV, FifthPokemon.PokemonTeamIV, FifthPokemon.PokemonTeamMoveset, FifthPokemon.Nature, FifthPokemon.TeraType, SixthPokemon, SixthPokemon.Pokemon, SixthPokemon.Pokemon.Game, SixthPokemon.Ability, SixthPokemon.PokemonTeamEV, SixthPokemon.PokemonTeamIV, SixthPokemon.PokemonTeamMoveset, SixthPokemon.Nature, SixthPokemon.TeraType, User", "User.Username", this.User.Identity.Name);
                List<ExportPokemonViewModel> exportList = new List<ExportPokemonViewModel>();
                foreach (var team in pokemonTeams)
                {
                    exportList.Add(this.ExportTeamString(team));
                }

                return exportList;
            }

            return null;
        }

        /// <summary>
        /// Saves a team that was generated from the team randomizer.
        /// </summary>
        /// <param name="selectedGame">The game selected in the randomizer.</param>
        /// <param name="pokemonIdList">The list of ids of the pokemon generated.</param>
        /// <param name="shinyPokemonList">The list of trues and falses if the pokemon generated is shiny.</param>
        /// <param name="pokemonTeamName">The given pokemon team name. Defaults to "Save from Team Randomizer" if no name given.</param>
        /// <returns>The string confirming the team has been generated. Tells a user they need to be logged in if they aren't.</returns>
        [AllowAnonymous]
        [Route("save-pokemon-team")]
        public string SavePokemonTeam(int selectedGame, List<int> pokemonIdList, List<bool> shinyPokemonList, string pokemonTeamName)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                try
                {
                    // Used for testing errors.
                    // selectedGame = 0;
                    // pokemonIdList = new List<int>()
                    // {
                    //     1552, 1386, 1433, 1423, 1432, 1405
                    // };
                    // shinyPokemonList = new List<bool>()
                    // {
                    //     false, false, false, false, false, false
                    // };
                    // pokemonTeamName = "test";
                    if (string.IsNullOrEmpty(pokemonTeamName))
                    {
                        pokemonTeamName = "Save from Team Randomizer";
                    }

                    if (this.dataService.GetCurrentUser(this.User) != null)
                    {
                        Game game = this.dataService.GetObjectByPropertyValue<Game>("Id", selectedGame);
                        this.dataService.AddPageView("Save Pokemon Team from Team Randomizer", this.User.IsInRole("Owner"));
                        PokemonTeam pokemonTeam = new PokemonTeam()
                        {
                            PokemonTeamName = pokemonTeamName,
                            UserId = this.dataService.GetCurrentUser(this.User).Id,
                        };

                        if (selectedGame != 0)
                        {
                            pokemonTeam.GameId = selectedGame;
                        }

                        Pokemon pokemon;
                        Ability ability = null;
                        PokemonTeamDetail pokemonTeamDetail;

                        for (var i = 0; i < pokemonIdList.Count; i++)
                        {
                            pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonIdList[i], "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth");

                            pokemonTeamDetail = new PokemonTeamDetail()
                            {
                                PokemonId = pokemon.Id,
                                NatureId = this.dataService.GetObjectByPropertyValue<Nature>("Name", "Serious").Id,
                                Level = 100,
                            };

                            if (selectedGame == 0 || game.GenerationId >= 3)
                            {
                                if (ability != null)
                                {
                                    pokemonTeamDetail.AbilityId = ability.Id;
                                }
                            }

                            if ((selectedGame == 0 || game.GenerationId >= 2) && shinyPokemonList[i])
                            {
                                pokemonTeamDetail.IsShiny = true;
                            }

                            this.dataService.AddPokemonTeamDetail(pokemonTeamDetail);

                            pokemonTeam.InsertPokemon(pokemonTeamDetail);
                        }

                        this.dataService.AddObject(pokemonTeam);

                        return string.Concat("Team \"", pokemonTeam.PokemonTeamName, "\" has been added successfully!");
                    }
                    else
                    {
                        return "You must be logged in to save a team.";
                    }
                }
                catch (Exception e)
                {
                    if (!this.User.IsInRole("Owner"))
                    {
                        string commentBody;
                        Game game = this.dataService.GetObjectByPropertyValue<Game>("Id", selectedGame);

                        if (e != null)
                        {
                            commentBody = string.Concat(e.GetType().ToString(), " error while saving generated team.");
                        }
                        else
                        {
                            commentBody = "Unknown error while saving generated team.";
                        }

                        if (game != null)
                        {
                            commentBody = string.Concat(commentBody, " - Selected Game: ", game.Name);
                        }
                        else
                        {
                            commentBody = string.Concat(commentBody, " - Selected Game: ", selectedGame);
                        }

                        commentBody = string.Concat(commentBody, " - Generated Pokemon Ids: {", string.Join(", ", pokemonIdList), "}");
                        commentBody = string.Concat(commentBody, " - Generated Shiny Pokemon: {", string.Join(", ", shinyPokemonList), "}");
                        commentBody = string.Concat(commentBody, " - Pokemon Team Name: ", pokemonTeamName);

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

            return null;
        }

        /// <summary>
        /// Prepares the required information for the user to view the pokemon page.
        /// </summary>
        /// <param name="generationId">The generation requested by the user.</param>
        /// <returns>The fill generation table shared view.</returns>
        [Route("get-pokemon-by-generation/{generationId}")]
        public IActionResult GetPokemonByGeneration(int generationId)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                GenerationTableViewModel model = new GenerationTableViewModel()
                {
                    PokemonList = this.dataService.GetAllPokemonWithTypes().Where(x => x.Pokemon.Game.GenerationId == generationId).ToList(),
                    AppConfig = this.appConfig,
                };

                List<PokemonTypeDetail> typingDetail = this.dataService.GetObjects<PokemonTypeDetail>(includes: "Pokemon, Pokemon.Game", whereProperty: "Pokemon.Game.GenerationId", wherePropertyValue: generationId).ToList();
                List<Pokemon> noTypingPokemon = this.dataService.GetAllPokemon().Where(x => x.Game.GenerationId == generationId && !x.IsAltForm).Except(typingDetail.Select(x => x.Pokemon)).ToList();

                foreach (var p in noTypingPokemon)
                {
                    model.PokemonList.Add(new PokemonTypeDetail()
                    {
                        Pokemon = p,
                        PokemonId = p.Id,
                        PrimaryTypeId = 0,
                        PrimaryType = new DataAccess.Models.Type()
                        {
                            Id = 0,
                            Name = "None",
                        },
                    });
                }

                model.PokemonList = model.PokemonList.OrderBy(x => x.Pokemon.PokedexNumber).ThenBy(x => x.PokemonId).ToList();

                return this.PartialView("_FillGenerationTable", model);
            }
            else
            {
                return this.RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Prepares the required information for the user to view the available pokemon page.
        /// </summary>
        /// <param name="gameId">The game requested by the user.</param>
        /// <returns>The fill available generation table shared view.</returns>
        [Route("get-available-pokemon-by-game/{gameId}")]
        public IActionResult GetAvailablePokemonByGame(int gameId)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                List<PokemonGameDetail> pokemonGameDetails = this.dataService.GetObjects<PokemonGameDetail>(includes: "Pokemon, Game", whereProperty: "GameId", wherePropertyValue: gameId);
                List<Pokemon> pokemonList = this.dataService.GetObjects<Pokemon>("PokedexNumber, Id", "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form");
                pokemonList = pokemonList.Where(x => pokemonGameDetails.Any(y => y.PokemonId == x.Id)).ToList();
                pokemonList.Where(x => x.IsAltForm).ToList().ForEach(x => x.Name = x.NameWithForm);

                GenerationTableViewModel model = new GenerationTableViewModel()
                {
                    PokemonNoTypeList = pokemonList,
                    AppConfig = this.appConfig,
                    Generation = this.dataService.GetObjectByPropertyValue<Game>("Id", gameId, "Generation").Generation,
                };

                return this.PartialView("_FillAvailableGenerationTable", model);
            }
            else
            {
                return this.RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Generatres a pokemon team for the team randomizer.
        /// </summary>
        /// <param name="pokemonCount">The amount of pokemon requested.</param>
        /// <param name="selectedGens">The generations allowed for generation.</param>
        /// <param name="selectedTypes">The types allowed for generation.</param>
        /// <param name="selectedGameId">The specific game used for generation.</param>
        /// <param name="selectedSpecialGroupings">The specific special groupings available for generation.</param>
        /// <param name="selectedForms">The specific forms available for generation.</param>
        /// <param name="selectedEvolutions">The specific evolution classification available for generation.</param>
        /// <param name="onlyLegendaries">Whether or not only legendaries are allowed for generation.</param>
        /// <param name="onlyAltForms">Whether or not only alternate forms are allowed for generation.</param>
        /// <param name="multipleMegas">Whether or not multiple mega pokemon are allowed for generation.</param>
        /// <param name="multipleGMax">Whether or not multiple gigantimax pokemon are allowed for generation.</param>
        /// <param name="onePokemonForm">Whether or not only one form of a pokemon is allowed for generation.</param>
        /// <param name="monotypeOnly">Whether or not only monotypes are allowed for generation.</param>
        /// <param name="noRepeatType">Whether or not repeat types are allowed for generation.</param>
        /// <param name="allowIncomplete">Whether or not incomplete pokemon can appear.</param>
        /// <param name="onlyUseRegionalDexes">Whether or not pokemon only in the regional dex are allowed for generation.</param>
        /// <returns>The view model of the generated pokemon team.</returns>
        [Route("get-pokemon-team")]
        public TeamRandomizerViewModel GetPokemonTeam(int pokemonCount, List<int> selectedGens, List<int> selectedTypes, int selectedGameId, List<string> selectedSpecialGroupings, List<string> selectedForms, List<string> selectedEvolutions, bool onlyLegendaries, bool onlyAltForms, bool multipleMegas, bool multipleGMax, bool onePokemonForm, bool monotypeOnly, bool noRepeatType, bool allowIncomplete, bool onlyUseRegionalDexes)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                List<Pokemon> pokemonList = new List<Pokemon>();
                List<int> testPokemonIds = new List<int>();

                // Modify to test errors in team generation.
                // selectedGameId = this.dataService.GetObjectByPropertyValue<Game>("Name", "").Id;
                // selectedTypes = new List<int>() {  };
                // pokemonCount = 6;
                // testPokemonIds = new List<int>() {  };
                // selectedGens = new List<int>() {  };
                // selectedEvolutions = new List<string>() {  };
                // selectedForms = new List<string>() {  };
                // selectedSpecialGroupings = new List<string>() {  };
                // onlyLegendaries = false;
                // onlyAltForms = false;
                // multipleMegas = false;
                // multipleGMax = false;
                // onePokemonForm = true;
                // monotypeOnly = false;
                // noRepeatType = false;
                // allowIncomplete = false;
                // onlyUseRegionalDexes = false;
                try
                {
                    Random rnd = new Random();
                    Game selectedGame = new Game();
                    List<PokemonTypeDetail> pokemonTypeDetails = this.dataService.GetObjects<PokemonTypeDetail>("GenerationId", "Pokemon, Pokemon.Game, Pokemon.Form, Pokemon.SpecialGrouping, PrimaryType, SecondaryType");
                    List<Pokemon> allPokemon = this.dataService.GetObjects<Pokemon>("PokedexNumber, Id", "Game, Form, SpecialGrouping").Where(x => selectedGens.Contains(x.Game.GenerationId)).ToList();
                    Pokemon pokemon;
                    List<Pokemon> altForms = new List<Pokemon>();
                    PokemonTypeDetail typing;
                    int originalPokemonId = 0;
                    pokemonCount = Math.Max(Math.Min(pokemonCount, 6), 1);

                    if (!allowIncomplete)
                    {
                        allPokemon = allPokemon.Where(x => x.IsComplete).ToList();
                    }

                    if (selectedGameId != 0)
                    {
                        selectedGame = this.dataService.GetObjectByPropertyValue<Game>("Id", selectedGameId);
                        List<Pokemon> availablePokemon = new List<Pokemon>();
                        if (onlyUseRegionalDexes)
                        {
                            availablePokemon = this.dataService.GetObjects<RegionalDexEntry>(includes: "Pokemon, RegionalDex.Game", whereProperty: "RegionalDex.GameId", wherePropertyValue: selectedGame.Id).ConvertAll(x => x.Pokemon);
                        }
                        else
                        {
                            availablePokemon = this.dataService.GetObjects<PokemonGameDetail>(includes: "Pokemon, Game", whereProperty: "GameId", wherePropertyValue: selectedGame.Id).ConvertAll(x => x.Pokemon);
                        }

                        allPokemon = allPokemon.Where(x => availablePokemon.Any(y => y.Id == x.Id)).ToList();
                        pokemonTypeDetails = pokemonTypeDetails.Where(x => x.GenerationId <= selectedGame.GenerationId).ToList();
                    }

                    pokemonTypeDetails = pokemonTypeDetails.GroupBy(x => new { x.PokemonId }).Select(x => x.LastOrDefault()).ToList();
                    if (selectedSpecialGroupings.Count() < this.dataService.GetObjects<SpecialGrouping>().Count())
                    {
                        allPokemon = this.FilterSpecialGroupings(allPokemon, selectedSpecialGroupings);
                    }

                    allPokemon = this.FilterForms(allPokemon, selectedForms, selectedGame, onlyAltForms, multipleMegas, multipleGMax);
                    allPokemon = this.FilterEvolutions(allPokemon, selectedEvolutions, selectedGame);
                    allPokemon = this.FilterTypes(allPokemon, selectedTypes, selectedGame);

                    if (monotypeOnly)
                    {
                        if (selectedTypes.Count() > 0)
                        {
                            allPokemon = allPokemon.Where(x => pokemonTypeDetails.Any(y => selectedTypes.Any(z => y.PrimaryTypeId == z && y.SecondaryTypeId == null) && y.PokemonId == x.Id)).ToList();
                        }
                        else
                        {
                            allPokemon = allPokemon.Where(x => pokemonTypeDetails.Any(y => y.SecondaryTypeId == null && y.PokemonId == x.Id)).ToList();
                        }
                    }

                    while (allPokemon.Count() > 0 && pokemonList.Count() < pokemonCount)
                    {
                        pokemon = allPokemon[rnd.Next(allPokemon.Count)];

                        if (onePokemonForm)
                        {
                            originalPokemonId = pokemon.Id;
                            if (pokemon.IsAltForm)
                            {
                                originalPokemonId = (int)pokemon.OriginalFormId;
                            }

                            altForms = allPokemon.Where(x => x.OriginalFormId == originalPokemonId).ToList();
                            if (pokemon.IsAltForm)
                            {
                                altForms.Remove(altForms.Find(x => x.Id == pokemon.Id));
                                allPokemon.Remove(allPokemon.Find(x => x.Id == originalPokemonId));
                            }

                            allPokemon = allPokemon.Where(x => !altForms.Any(y => y.Id == x.Id)).ToList();
                        }

                        if (noRepeatType)
                        {
                            typing = pokemonTypeDetails.Find(x => x.PokemonId == pokemon.Id);
                            if (typing != null)
                            {
                                allPokemon = allPokemon.Where(x => !pokemonTypeDetails.Any(y => (y.PrimaryTypeId == typing.PrimaryTypeId || y.SecondaryTypeId == typing.PrimaryTypeId) && y.PokemonId == x.Id)).ToList();
                                if (typing.SecondaryType != null)
                                {
                                    allPokemon = allPokemon.Where(x => !pokemonTypeDetails.Any(y => (y.PrimaryTypeId == typing.SecondaryTypeId || y.SecondaryTypeId == typing.SecondaryTypeId) && y.PokemonId == x.Id)).ToList();
                                }
                            }
                        }

                        pokemonList.Add(pokemon);
                        allPokemon.Remove(allPokemon.Find(x => x.Id == pokemon.Id));
                    }

                    List<int> pokemonIds = new List<int>();
                    List<Pokemon> pokemonDetails = this.dataService.GetObjects<Pokemon>("PokedexNumber, Id", "Form");
                    if (testPokemonIds.Count() > 0)
                    {
                        pokemonIds = testPokemonIds;
                        pokemonList = pokemonDetails.Where(x => pokemonIds.Any(y => y == x.Id)).ToList();
                    }
                    else
                    {
                        pokemonIds = pokemonList.ConvertAll(x => x.Id);
                    }

                    TeamRandomizerViewModel model = new TeamRandomizerViewModel()
                    {
                        AllPokemonChangedNames = pokemonList,
                        AllPokemonOriginalNames = pokemonDetails.Where(x => pokemonIds.Any(y => y == x.Id)).OrderBy(x => pokemonIds.IndexOf(x.Id)).ToList(),
                        PokemonURLs = new List<string>(),
                        AppConfig = this.appConfig,
                    };

                    foreach (var p in model.AllPokemonOriginalNames)
                    {
                        if (selectedGame.Id != 0)
                        {
                            model.PokemonURLs.Add(this.Url.Action("PokemonWithId", "Home", new { pokemonName = p.Name.Replace(": ", "_").Replace(' ', '_').ToLower(), pokemonId = p.Id, selectedGame.GenerationId }));
                        }
                        else
                        {
                            model.PokemonURLs.Add(this.Url.Action("PokemonWithId", "Home", new { pokemonName = p.Name.Replace(": ", "_").Replace(' ', '_').ToLower(), pokemonId = p.Id, generationId = 0 }));
                        }
                    }

                    model.ExportString = this.ExportPokemonTeam(model.AllPokemonOriginalNames.ConvertAll(x => x.Id));

                    this.dataService.AddPageView("Random Team Generated", this.User.IsInRole("Owner"));

                    return model;
                }
                catch (Exception e)
                {
                    if (!this.User.IsInRole("Owner"))
                    {
                        string commentBody;
                        Game game = this.dataService.GetObjectByPropertyValue<Game>("Id", selectedGameId);
                        List<DataAccess.Models.Type> allTypes = this.dataService.GetObjects<DataAccess.Models.Type>();

                        if (e != null)
                        {
                            commentBody = string.Concat(e.GetType().ToString(), " error during team generation.");
                        }
                        else
                        {
                            commentBody = "Unknown error during team generation.";
                        }

                        if (game != null)
                        {
                            commentBody = string.Concat(commentBody, " - Selected Game: ", game.Name);
                        }
                        else
                        {
                            commentBody = string.Concat(commentBody, " - Selected Game: ", selectedGameId);
                        }

                        commentBody = string.Concat(commentBody, " - Pokemon Count: ", pokemonCount);
                        commentBody = string.Concat(commentBody, " - Pokemon Ids Generated: {", string.Join(", ", pokemonList.Select(x => x.Id)), "}");
                        commentBody = string.Concat(commentBody, " - Selected Generation Ids: {", string.Join(", ", selectedGens), "}");
                        commentBody = string.Concat(commentBody, " - Selected Types: {", string.Join(", ", allTypes.Where(x => selectedTypes.Any(y => y == x.Id)).Select(x => x.Name)), "}");
                        commentBody = string.Concat(commentBody, " - Selected Evolutions: {", string.Join(", ", selectedEvolutions), "}");
                        commentBody = string.Concat(commentBody, " - Selected Forms: {", string.Join(", ", selectedForms), "}");
                        commentBody = string.Concat(commentBody, " - Selected Special Grouping: {", string.Join(", ", selectedSpecialGroupings), "}");
                        commentBody = string.Concat(commentBody, " - Only Legendaries: ", onlyLegendaries);
                        commentBody = string.Concat(commentBody, " - Only Alternate Forms: ", onlyAltForms);
                        commentBody = string.Concat(commentBody, " - Multiple Megas: ", multipleMegas);
                        commentBody = string.Concat(commentBody, " - Multiple Gigantamax: ", multipleGMax);
                        commentBody = string.Concat(commentBody, " - Only One Form per Pokemon: ", onePokemonForm);
                        commentBody = string.Concat(commentBody, " - No Repeating Types: ", noRepeatType);
                        commentBody = string.Concat(commentBody, " - Allow Incomplete Pokemon: ", allowIncomplete);
                        commentBody = string.Concat(commentBody, " - Only Use Regional Dexes: ", onlyUseRegionalDexes);

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

            return null;
        }

        /// <summary>
        /// Gets a list of abilities for a pokemon.
        /// </summary>
        /// <param name="pokemonId">The pokemon requested.</param>
        /// <param name="gameId">The game the search will take place in.</param>
        /// <returns>The list of available abilities for the specified pokemon in the specified game.</returns>
        [Route("get-pokemon-abilities")]
        public List<Ability> GetPokemonAbilities(int pokemonId, int gameId)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                List<Ability> pokemonAbilities = this.dataService.GetAbilitiesForPokemon(pokemonId, gameId, this.User, this.appConfig);
                return pokemonAbilities;
            }

            return null;
        }

        /// <summary>
        /// Updates the type chart for the given type.
        /// </summary>
        /// <param name="typeId">The type being modified.</param>
        /// <param name="genId">The generation this modification is taking place.</param>
        /// <param name="resistances">The type's resistances.</param>
        /// <param name="weaknesses">The type's weaknesses.</param>
        /// <param name="immunities">The type's immunities.</param>
        /// <returns>The admin type page.</returns>
        [Route("update-type-chart")]
        public string UpdateTypeChart(int typeId, int genId, List<int> resistances, List<int> weaknesses, List<int> immunities)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                TypeChart typeChart;
                List<int> duplicateIds = resistances.Where(x => weaknesses.Contains(x)).ToList();
                List<TypeChart> existingCharts = this.dataService.GetTypeChartByDefendType(typeId, genId);

                foreach (var t in duplicateIds)
                {
                    resistances.Remove(resistances.Find(x => x == t));
                    weaknesses.Remove(weaknesses.Find(x => x == t));
                }

                foreach (var t in immunities)
                {
                    if (resistances.IndexOf(t) != -1)
                    {
                        resistances.Remove(resistances.Find(x => x == t));
                    }

                    if (weaknesses.IndexOf(t) != -1)
                    {
                        weaknesses.Remove(weaknesses.Find(x => x == t));
                    }
                }

                foreach (var r in resistances)
                {
                    typeChart = new TypeChart()
                    {
                        AttackId = r,
                        DefendId = typeId,
                        Effective = 0.5m,
                        GenerationId = genId,
                    };
                    this.dataService.AddObject(typeChart);
                }

                foreach (var w in weaknesses)
                {
                    typeChart = new TypeChart()
                    {
                        AttackId = w,
                        DefendId = typeId,
                        Effective = 2m,
                        GenerationId = genId,
                    };
                    this.dataService.AddObject(typeChart);
                }

                foreach (var i in immunities)
                {
                    typeChart = new TypeChart()
                    {
                        AttackId = i,
                        DefendId = typeId,
                        Effective = 0m,
                        GenerationId = genId,
                    };
                    this.dataService.AddObject(typeChart);
                }

                foreach (var t in existingCharts)
                {
                    this.dataService.DeleteObject<TypeChart>(t.Id);
                }

                return this.Json(this.Url.Action("Types", "Owner")).Value.ToString();
            }

            return null;
        }

        /// <summary>
        /// Deletes the teams selected by the user.
        /// </summary>
        /// <param name="teamIds">The teams for deletion.</param>
        /// <returns>The pokemon teams page.</returns>
        [Route("delete-pokemon-teams")]
        public string DeletePokemonTeams(List<int> teamIds)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                List<PokemonTeam> pokemonTeams = this.dataService.GetObjects<PokemonTeam>("Id", "User", "User.Username", this.User.Identity.Name);
                pokemonTeams = pokemonTeams.Where(x => teamIds.Any(y => y == x.Id)).ToList();

                foreach (var t in pokemonTeams)
                {
                    this.dataService.DeletePokemonTeam(t.Id, this.User, this.appConfig);
                }

                return this.Json(this.Url.Action("PokemonTeams", "User")).Value.ToString();
            }

            return null;
        }

        /// <summary>
        /// Updates the availablity of the specified form group in different games.
        /// </summary>
        /// <param name="formGroupId">The form group's id.</param>
        /// <param name="games">The games the pokemon is able to be used in.</param>
        /// <returns>The admin pokemon page.</returns>
        [Route("update-form-group-game-availability")]
        public string UpdateFormGroupGameAvailability(int formGroupId, List<int> games)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                FormGroupGameDetail formGroupGameDetail;
                List<FormGroupGameDetail> existingGameDetails = this.dataService.GetObjects<FormGroupGameDetail>(whereProperty: "FormGroupId", wherePropertyValue: formGroupId);

                foreach (var g in games)
                {
                    formGroupGameDetail = new FormGroupGameDetail()
                    {
                        FormGroupId = formGroupId,
                        GameId = g,
                    };
                    this.dataService.AddObject(formGroupGameDetail);
                }

                foreach (var g in existingGameDetails)
                {
                    this.dataService.DeleteObject<FormGroupGameDetail>(g.Id);
                }

                return this.Json(this.Url.Action("FormGroups", "Owner")).Value.ToString();
            }

            return null;
        }

        /// <summary>
        /// Updates the availablity of the specified pokemon in different games.
        /// </summary>
        /// <param name="pokemonId">The pokemon's id.</param>
        /// <param name="games">The games the pokemon is able to be used in.</param>
        /// <returns>The admin pokemon page.</returns>
        [Route("update-pokemon-game-availability")]
        public string UpdatePokemonGameAvailability(int pokemonId, List<int> games)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                PokemonGameDetail pokemonGameDetail;
                List<PokemonGameDetail> existingGameDetails = this.dataService.GetObjects<PokemonGameDetail>("Game.GenerationId, GameId, Id", "Pokemon, Pokemon.Game, Game", "PokemonId", pokemonId);

                foreach (var g in games)
                {
                    pokemonGameDetail = new PokemonGameDetail()
                    {
                        PokemonId = pokemonId,
                        GameId = g,
                    };
                    this.dataService.AddObject(pokemonGameDetail);
                }

                foreach (var g in existingGameDetails)
                {
                    this.dataService.DeleteObject<PokemonGameDetail>(g.Id);
                }

                return this.Json(this.Url.Action("Pokemon", "Owner")).Value.ToString();
            }

            return null;
        }

        /// <summary>
        /// Updates the availablity of the specified hunting method in different games.
        /// </summary>
        /// <param name="huntingMethodId">The hunting method's id.</param>
        /// <param name="games">The games the pokemon is able to be used in.</param>
        /// <returns>The admin pokemon page.</returns>
        [Route("update-hunting-method-game-availability")]
        public string UpdateHuntingMethodGameAvailability(int huntingMethodId, List<int> games)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                HuntingMethodGameDetail huntingMethodGameDetail;
                List<HuntingMethodGameDetail> existingGameDetails = this.dataService.GetObjects<HuntingMethodGameDetail>("Game.GenerationId, GameId, Id", "HuntingMethod, Game", "HuntingMethodId", huntingMethodId);

                foreach (var g in games)
                {
                    huntingMethodGameDetail = new HuntingMethodGameDetail()
                    {
                        HuntingMethodId = huntingMethodId,
                        GameId = g,
                    };
                    this.dataService.AddObject(huntingMethodGameDetail);
                }

                foreach (var g in existingGameDetails)
                {
                    this.dataService.DeleteObject<HuntingMethodGameDetail>(g.Id);
                }

                return this.Json(this.Url.Action("HuntingMethods", "Owner")).Value.ToString();
            }

            return null;
        }

        /// <summary>
        /// Updates the availablity of the pokemon in a specified game.
        /// </summary>
        /// <param name="regionalDexId">The game's id.</param>
        /// <param name="pokemonList">The pokemon that are available in the given regional dex.</param>
        /// <returns>The admin pokemon page.</returns>
        [Route("update-regional-dex-entries")]
        public string UpdateRegionalDexEntries(int regionalDexId, List<string> pokemonList)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                RegionalDex regionalDex = this.dataService.GetObjectByPropertyValue<RegionalDex>("Id", regionalDexId);
                List<Pokemon> allPokemon = this.dataService.GetAllPokemon().Where(x => pokemonList.Any(y => y == x.Name)).ToList();
                List<int> pokemonIds = allPokemon.DistinctBy(x => x.Name).OrderBy(x => pokemonList.IndexOf(x.Name)).Select(x => x.Id).ToList();
                List<RegionalDexEntry> existingDexEntries = this.dataService.GetObjects<RegionalDexEntry>(includes: "Pokemon, RegionalDex", whereProperty: "RegionalDexId", wherePropertyValue: regionalDexId);
                List<RegionalDexEntry> newDexEntries = new List<RegionalDexEntry>();
                if (allPokemon.First().Name == "Victini")
                {
                    if (existingDexEntries.Find(x => x.PokemonId == pokemonIds.First() && x.RegionalDexId == regionalDexId && x.RegionalPokedexNumber == 0) == null)
                    {
                        newDexEntries.Add(new RegionalDexEntry()
                        {
                            PokemonId = pokemonIds.First(),
                            RegionalDexId = regionalDexId,
                            RegionalPokedexNumber = 0,
                        });
                    }
                    else
                    {
                        existingDexEntries.Remove(existingDexEntries.Find(x => x.PokemonId == pokemonIds.First() && x.RegionalDexId == regionalDexId));
                    }

                    pokemonIds.Remove(pokemonIds.First());
                }

                for (var i = 0; i < pokemonIds.Count(); i++)
                {
                    if (existingDexEntries.Find(x => x.PokemonId == pokemonIds[i] && x.RegionalDexId == regionalDexId && x.RegionalPokedexNumber == i + 1) == null)
                    {
                        newDexEntries.Add(new RegionalDexEntry()
                        {
                            PokemonId = pokemonIds[i],
                            RegionalDexId = regionalDexId,
                            RegionalPokedexNumber = i + 1,
                        });
                        if (allPokemon.Exists(x => x.OriginalFormId == pokemonIds[i]))
                        {
                            foreach (var a in allPokemon.Where(x => x.OriginalFormId == pokemonIds[i]))
                            {
                                newDexEntries.Add(new RegionalDexEntry()
                                {
                                    PokemonId = a.Id,
                                    RegionalDexId = regionalDexId,
                                    RegionalPokedexNumber = i + 1,
                                });
                            }
                        }
                    }
                    else
                    {
                        existingDexEntries.Remove(existingDexEntries.Find(x => x.PokemonId == pokemonIds[i] && x.RegionalDexId == regionalDexId));
                        if (allPokemon.Exists(x => x.OriginalFormId == pokemonIds[i]))
                        {
                            foreach (var a in allPokemon.Where(x => x.OriginalFormId == pokemonIds[i]))
                            {
                                existingDexEntries.Remove(existingDexEntries.Find(x => x.PokemonId == a.Id && x.RegionalDexId == regionalDexId));
                            }
                        }
                    }
                }

                foreach (var e in newDexEntries)
                {
                    this.dataService.AddObject(e);
                }

                foreach (var e in existingDexEntries)
                {
                    this.dataService.DeleteObject<RegionalDexEntry>(e.Id);
                }

                return this.Json(this.Url.Action("RegionalDexes", "Owner")).Value.ToString();
            }

            return null;
        }

        /// <summary>
        /// Updates the availablity of the pokemon in a specified game.
        /// </summary>
        /// <param name="gameId">The game's id.</param>
        /// <param name="pokemonList">The pokemon that are available in the given game.</param>
        /// <returns>The admin pokemon page.</returns>
        [Route("update-game-availability")]
        public string UpdateGameAvailability(int gameId, List<int> pokemonList)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                List<PokemonGameDetail> existingGameDetails = new List<PokemonGameDetail>();
                List<PokemonGameDetail> newGameDetails = new List<PokemonGameDetail>();
                List<int> pokemonIds = new List<int>();
                List<Game> games = this.dataService.GetObjects<Game>("ReleaseDate, Id");
                DateTime releaseDate = games.Find(x => x.Id == gameId).ReleaseDate;
                games = games.Where(x => x.ReleaseDate == releaseDate).ToList();
                int generationId = games.First().GenerationId;

                if (gameId == 4 || gameId == 5)
                {
                    games = games.Where(x => x.Id == gameId).ToList();
                }
                else if (gameId == 16 || gameId == 28)
                {
                    generationId = 1;
                }
                else if (gameId == 35 || gameId == 36)
                {
                    generationId = 4;
                }

                List<Evolution> evolutions = this.dataService.GetObjects<Evolution>().Where(x => x.GenerationId <= generationId).ToList();

                pokemonIds.AddRange(pokemonList.Except(evolutions.Select(x => x.EvolutionPokemonId).ToList()));

                foreach (var p in pokemonList.Intersect(evolutions.Select(x => x.EvolutionPokemonId).ToList()))
                {
                    Evolution evolutionDetails = evolutions.Find(x => x.EvolutionPokemonId == p);
                    if (!pokemonIds.Contains(evolutionDetails.PreevolutionPokemonId))
                    {
                        pokemonIds.Add(evolutionDetails.PreevolutionPokemonId);
                        if (evolutions.FirstOrDefault(x => x.EvolutionPokemonId == evolutionDetails.PreevolutionPokemonId) != null)
                        {
                            pokemonIds.Add(evolutions.First(x => x.EvolutionPokemonId == evolutionDetails.PreevolutionPokemonId).PreevolutionPokemonId);
                        }
                    }

                    pokemonIds.Add(p);
                }

                foreach (var game in games.ConvertAll(x => x.Id))
                {
                    newGameDetails = new List<PokemonGameDetail>();
                    existingGameDetails = this.dataService.GetObjects<PokemonGameDetail>(includes: "Pokemon, Game", whereProperty: "GameId", wherePropertyValue: game);

                    foreach (var p in pokemonIds)
                    {
                        if (existingGameDetails.Find(x => x.PokemonId == p && x.GameId == game) == null)
                        {
                            newGameDetails.Add(new PokemonGameDetail()
                            {
                                GameId = game,
                                PokemonId = p,
                            });
                        }
                        else
                        {
                            existingGameDetails.Remove(existingGameDetails.Find(x => x.PokemonId == p && x.GameId == game));
                        }
                    }

                    foreach (var g in newGameDetails)
                    {
                        this.dataService.AddObject(g);
                    }

                    foreach (var g in existingGameDetails)
                    {
                        this.dataService.DeleteObject<PokemonGameDetail>(g.Id);
                    }
                }

                return this.Json(this.Url.Action("GameAvailability", "Home")).Value.ToString();
            }

            return null;
        }

        /// <summary>
        /// Updates the availablity of the pokemon in a specified game.
        /// </summary>
        /// <param name="gameId">The game's id.</param>
        /// <param name="markList">The marks that are available in the given game.</param>
        /// <returns>The admin pokemon page.</returns>
        [Route("update-mark-games")]
        public string UpdateMarkGame(int gameId, List<int> markList)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                List<MarkGameDetail> existingMarkGames = new List<MarkGameDetail>();
                List<MarkGameDetail> newMarkGames = new List<MarkGameDetail>();
                List<Game> games = this.dataService.GetObjects<Game>("ReleaseDate, Id").Where(x => x.GenerationId >= 8).ToList();
                DateTime releaseDate = games.Find(x => x.Id == gameId).ReleaseDate;
                games = games.Where(x => x.ReleaseDate == releaseDate).ToList();
                int generationId = games.First().GenerationId;

                foreach (var game in games.ConvertAll(x => x.Id))
                {
                    newMarkGames = new List<MarkGameDetail>();
                    existingMarkGames = this.dataService.GetObjects<MarkGameDetail>(includes: "Mark, Game", whereProperty: "GameId", wherePropertyValue: game);

                    foreach (var m in markList)
                    {
                        if (existingMarkGames.Find(x => x.MarkId == m && x.GameId == game) == null)
                        {
                            newMarkGames.Add(new MarkGameDetail()
                            {
                                GameId = game,
                                MarkId = m,
                            });
                        }
                        else
                        {
                            existingMarkGames.Remove(existingMarkGames.Find(x => x.MarkId == m && x.GameId == game));
                        }
                    }

                    foreach (var g in newMarkGames)
                    {
                        this.dataService.AddObject(g);
                    }

                    foreach (var g in existingMarkGames)
                    {
                        this.dataService.DeleteObject<MarkGameDetail>(g.Id);
                    }
                }

                return this.Json(this.Url.Action("Games", "Owner")).Value.ToString();
            }

            return null;
        }

        /// <summary>
        /// Updates the availablity of the pokemon in a specified game.
        /// </summary>
        /// <param name="gameId">The game's id.</param>
        /// <param name="pokeballList">The pokeballs that are available in the given game.</param>
        /// <returns>The admin pokemon page.</returns>
        [Route("update-pokeball-games")]
        public string UpdatePokeballGame(int gameId, List<int> pokeballList)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                List<PokeballGameDetail> existingPokeballGames = new List<PokeballGameDetail>();
                List<PokeballGameDetail> newPokeballGames = new List<PokeballGameDetail>();
                List<Game> games = this.dataService.GetObjects<Game>("ReleaseDate, Id");
                DateTime releaseDate = games.Find(x => x.Id == gameId).ReleaseDate;
                games = games.Where(x => x.ReleaseDate == releaseDate).ToList();
                int generationId = games.First().GenerationId;

                foreach (var game in games.ConvertAll(x => x.Id))
                {
                    newPokeballGames = new List<PokeballGameDetail>();
                    existingPokeballGames = this.dataService.GetObjects<PokeballGameDetail>(includes: "Pokeball, Game", whereProperty: "GameId", wherePropertyValue: game);

                    foreach (var m in pokeballList)
                    {
                        if (existingPokeballGames.Find(x => x.PokeballId == m && x.GameId == game) == null)
                        {
                            newPokeballGames.Add(new PokeballGameDetail()
                            {
                                GameId = game,
                                PokeballId = m,
                            });
                        }
                        else
                        {
                            existingPokeballGames.Remove(existingPokeballGames.Find(x => x.PokeballId == m && x.GameId == game));
                        }
                    }

                    foreach (var g in newPokeballGames)
                    {
                        this.dataService.AddObject(g);
                    }

                    foreach (var g in existingPokeballGames)
                    {
                        this.dataService.DeleteObject<PokeballGameDetail>(g.Id);
                    }
                }

                return this.Json(this.Url.Action("Games", "Owner")).Value.ToString();
            }

            return null;
        }

        /// <summary>
        /// Updates the pokemon list that give the specified evs.
        /// </summary>
        /// <param name="gameId">The generation's id.</param>
        /// <param name="health">The health ev.</param>
        /// <param name="attack">The attack ev.</param>
        /// <param name="defense">The defense ev.</param>
        /// <param name="specialAttack">The special attack ev.</param>
        /// <param name="specialDefense">The special defense ev.</param>
        /// <param name="speed">The speed ev.</param>
        /// <returns>The list of pokemon with this specific ev yield spread.</returns>
        [Route("get-pokemon-by-ev-yields")]
        public IActionResult GetPokemonByEVYields(int gameId, int health, int attack, int defense, int specialAttack, int specialDefense, int speed)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                int evYieldTotal = health + attack + defense + specialAttack + specialDefense + speed;
                Game game = this.dataService.GetObjectByPropertyValue<Game>("Id", gameId);
                List<Pokemon> pokemonList = this.dataService.GetObjects<PokemonGameDetail>("Pokemon.PokedexNumber, Pokemon.Id", "Pokemon, Pokemon.Form", "GameId", game.Id).Select(x => x.Pokemon).ToList();
                List<EVYield> allEVYields = this.dataService.GetObjects<EVYield>("Pokemon.PokedexNumber, Pokemon.Id", "Pokemon, Pokemon.Form").Where(x => x.GenerationId <= game.GenerationId).ToList();
                List<EVYield> evYields = new List<EVYield>();
                foreach (var ev in allEVYields)
                {
                    if (!evYields.Any(x => x.PokemonId == ev.PokemonId))
                    {
                        evYields.Add(allEVYields.Where(x => x.PokemonId == ev.PokemonId).OrderByDescending(x => x.GenerationId).First(x => x.GenerationId <= game.GenerationId));
                    }
                }

                pokemonList = evYields.Where(x => x.Health == health && x.Attack == attack && x.Defense == defense && x.SpecialAttack == specialAttack && x.SpecialDefense == specialDefense && x.Speed == speed && pokemonList.Any(y => y.Id == x.PokemonId)).Select(x => x.Pokemon).ToList();
                if (gameId == 10)
                {
                    if (attack == 1 && evYieldTotal == 1)
                    {
                        pokemonList.Add(this.dataService.GetObjectByPropertyValue<Pokemon>("Id", 839));
                    }
                    else if (attack == 2 && evYieldTotal == 2)
                    {
                        pokemonList.Remove(pokemonList.Find(x => x.Id == 839));
                    }
                }

                pokemonList = pokemonList.OrderBy(x => x.PokedexNumber).ThenBy(x => x.Id).ToList();

                EVTrainerViewModel pokemonEVYields = new EVTrainerViewModel()
                {
                    AllPokemon = pokemonList,
                    AppConfig = this.appConfig,
                };

                return this.PartialView("_FillEVTrainer", pokemonEVYields);
            }

            return this.RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Gets the available genders for a pokemon.
        /// </summary>
        /// <param name="pokemonId">The pokemon's id.</param>
        /// <param name="isShinyHunt">Whether or not this method is used for a shiny hunt.</param>
        /// <returns>A list of available genders.</returns>
        [Route("get-pokemon-genders")]
        public List<string> GetPokemonGenders(int pokemonId, bool isShinyHunt)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return this.dataService.GrabGenders(pokemonId, isShinyHunt);
            }

            return null;
        }

        /// <summary>
        /// Gets the available sweets for a pokemon.
        /// </summary>
        /// <param name="pokemonId">The pokemon's id.</param>
        /// <returns>A list of available sweets.</returns>
        [Route("get-pokemon-sweets")]
        public List<Sweet> GetPokemonSweets(int pokemonId)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                List<Sweet> model = new List<Sweet>();
                if (pokemonId != 0)
                {
                    Pokemon pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonId);
                    if (pokemon.Name == "Alcremie")
                    {
                        model = this.dataService.GetObjects<Sweet>();
                    }
                }

                return model;
            }

            return null;
        }

        /// <summary>
        /// Gets the type chart for the given typing combination.
        /// </summary>
        /// <param name="pokemonId">The pokemon's id.</param>
        /// <param name="generationId">The generation used to specify the type chart.</param>
        /// <param name="teraType">The tera type of the pokemon.</param>
        /// <returns>The file type evaluator chart shared view.</returns>
        [Route("get-stellar-typing-chart")]
        public IActionResult GetStellarTypingChart(int pokemonId, int generationId, string teraType)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                PokemonTypeDetail pokemonTypeDetail = this.dataService.GetObjects<PokemonTypeDetail>(whereProperty: "PokemonId", wherePropertyValue: pokemonId).OrderByDescending(x => x.GenerationId).First(x => x.GenerationId <= generationId);
                int primaryId = pokemonTypeDetail.PrimaryTypeId ?? 0;
                int secondaryId = pokemonTypeDetail.SecondaryTypeId ?? 0;
                return this.PartialView("_FillTypeEvaluatorChart", this.GetTypeChartTyping(primaryId, secondaryId, generationId, teraType));
            }
            else
            {
                return this.RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Gets the type chart for the given typing combination.
        /// </summary>
        /// <param name="pokemonId">The pokemon's id.</param>
        /// <param name="generationId">The generation used to specify the type chart.</param>
        /// <returns>The file type evaluator chart shared view.</returns>
        [Route("get-typing-evaluator-chart-by-pokemon")]
        public IActionResult GetTypingEvaluatorChartByPokemon(int pokemonId, int generationId)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                PokemonTypeDetail pokemonTypeDetail = this.dataService.GetObjects<PokemonTypeDetail>(whereProperty: "PokemonId", wherePropertyValue: pokemonId).OrderByDescending(x => x.GenerationId).First(x => x.GenerationId <= generationId);
                int primaryId = pokemonTypeDetail.PrimaryTypeId ?? 0;
                int secondaryId = pokemonTypeDetail.SecondaryTypeId ?? 0;
                return this.PartialView("_FillTypeEvaluatorChart", this.GetTypeChartTyping(primaryId, secondaryId, generationId));
            }
            else
            {
                return this.RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Gets the type chart for the given typing combination.
        /// </summary>
        /// <param name="primaryTypeId">The primary type's id.</param>
        /// <param name="secondaryTypeId">The secondary type's id.</param>
        /// <param name="generationId">The generation used to specify the type chart.</param>
        /// <param name="teraType">The tera type of the pokemon.</param>
        /// <returns>The file type evaluator chart shared view.</returns>
        [Route("get-typing-evaluator-chart")]
        public IActionResult GetTypingEvaluatorChart(int primaryTypeId, int secondaryTypeId, int generationId, string teraType)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return this.PartialView("_FillTypeEvaluatorChart", this.GetTypeChartTyping(primaryTypeId, secondaryTypeId, generationId, teraType));
            }
            else
            {
                return this.RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Updates the encounters for a shiny hunt.
        /// </summary>
        /// <param name="shinyHuntId">The shiny hunt's id.</param>
        /// <param name="encounters">The amount to encounters.</param>
        [Route("set-shiny-hunt-encounters")]
        public void SetShinyHuntEncounters(int shinyHuntId, int encounters)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                ShinyHunt shinyHunt = this.dataService.GetObjectByPropertyValue<ShinyHunt>("Id", shinyHuntId);
                User user = this.dataService.GetCurrentUser(this.User);
                if (user != null && user.Id == shinyHunt.UserId)
                {
                    shinyHunt.CurrentPhaseEncounters = Math.Max(encounters, 0);
                    this.dataService.UpdateObject(shinyHunt);
                }
            }
        }

        /// <summary>
        /// Updates the increments for a shiny hunt.
        /// </summary>
        /// <param name="shinyHuntId">The shiny hunt's id.</param>
        /// <param name="increments">The amount to increments.</param>
        [Route("set-shiny-hunt-increments")]
        public void SetShinyHuntIncrements(int shinyHuntId, int increments)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                ShinyHunt shinyHunt = this.dataService.GetObjectByPropertyValue<ShinyHunt>("Id", shinyHuntId);
                User user = this.dataService.GetCurrentUser(this.User);
                if (user != null && user.Id == shinyHunt.UserId)
                {
                    shinyHunt.IncrementAmount = Math.Max(increments, 1);
                    this.dataService.UpdateObject(shinyHunt);
                }
            }
        }

        /// <summary>
        /// Checks to see if shiny charm can be used.
        /// </summary>
        /// <param name="gameId">The game's id.</param>
        /// <param name="huntingMethodId">The hunting method's id.</param>
        /// <returns>The boolean determining if a shiny charm can be shown.</returns>
        [Route("check-shiny-charm")]
        public bool CheckShinyCharm(int gameId, int huntingMethodId)
        {
            try
            {
                if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    bool shinyCharm = true;
                    if (huntingMethodId == 8 || huntingMethodId == 14 || huntingMethodId == 38 || gameId == 43)
                    {
                        shinyCharm = false;
                    }
                    else
                    {
                        Game game = this.dataService.GetObjectByPropertyValue<Game>("Id", gameId);
                        if (game == null)
                        {
                            shinyCharm = false;
                        }
                        else if (game.GenerationId < 6 && gameId != 11 && gameId != 29)
                        {
                            shinyCharm = false;
                        }
                        else if ((gameId == 35 || gameId == 36) && huntingMethodId != 4 && huntingMethodId != 5)
                        {
                            shinyCharm = false;
                        }
                    }

                    return shinyCharm;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                if (!this.User.IsInRole("Owner") && e != null)
                {
                    string commentBody;
                    if (e != null)
                    {
                        commentBody = string.Concat(e.GetType().ToString(), " error during shiny charm check.");
                    }
                    else
                    {
                        commentBody = "Unknown error during shiny charm check.";
                    }

                    commentBody = string.Concat(commentBody, " - Selected Game Id: ", gameId);
                    commentBody = string.Concat(commentBody, " - Selected Hunting Method Id: ", huntingMethodId);

                    Comment comment = new Comment()
                    {
                        Name = commentBody,
                    };

                    if (this.User.Identity.Name != null)
                    {
                        comment.CommentorId = this.dataService.GetCurrentUser(this.User).Id;
                    }

                    this.dataService.EmailComment(this.appConfig, comment);
                }

                return false;
            }
        }

        /// <summary>
        /// Checks to see if sparkling power can be used.
        /// </summary>
        /// <param name="gameId">The game's id.</param>
        /// <param name="huntingMethodId">The hunting method's id.</param>
        /// <returns>The boolean determining if a sparkling power can be shown.</returns>
        [Route("check-sparkling-power")]
        public bool CheckSparklingPower(int gameId, int huntingMethodId)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest" && (gameId == 41 || gameId == 42) && huntingMethodId != 4 && huntingMethodId != 5 && huntingMethodId != 14)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks to see if marks can be used.
        /// </summary>
        /// <param name="gameId">The game's id.</param>
        /// <returns>The boolean determining if a mark can be shown.</returns>
        [Route("check-mark")]
        public bool CheckMark(int gameId)
        {
            Game game = this.dataService.GetObjectByPropertyValue<Game>("Id", gameId);
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest" && game.GenerationId >= 8 && gameId != 35 && gameId != 36)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Toggles whether a shiny hunt is pinned or not.
        /// </summary>
        /// <param name="shinyHuntId">The shiny hunt's id.</param>
        /// <returns>The boolean determining if a mark can be shown.</returns>
        [Route("toggle-hunt-pin")]
        public bool ToggleHuntPin(int shinyHuntId)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                ShinyHunt hunt = this.dataService.GetObjectByPropertyValue<ShinyHunt>("Id", shinyHuntId);
                hunt.IsPinned = !hunt.IsPinned;
                this.dataService.UpdateObject(hunt);

                return hunt.IsPinned;
            }

            return false;
        }

        /// <summary>
        /// Gets the type chart for the given typing combination.
        /// </summary>
        /// <param name="pokemonId">The pokemon's id.</param>
        /// <param name="typeId">The tera/plate/memory type's id.</param>
        /// <param name="currentImage">The currently viewed image.</param>
        /// <returns>The file type evaluator chart shared view.</returns>
        [Route("get-pokemon-images-with-type")]
        public IActionResult GetTypingEvaluatorChart(int pokemonId, int typeId, string currentImage)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (typeId == 0)
                {
                    typeId = 5;
                }

                Pokemon pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonId);
                PokemonImageViewModel model = new PokemonImageViewModel()
                {
                    Id = pokemon.Id,
                    Name = pokemon.Name,
                    CurrentImage = currentImage,
                    AppConfig = this.appConfig,
                    Type = this.dataService.GetObjectByPropertyValue<DataAccess.Models.Type>("Id", typeId),
                };

                HttpWebRequest webRequest;
                HttpWebResponse imageRequest;
                try
                {
                    webRequest = (HttpWebRequest)HttpWebRequest.Create(string.Concat(this.appConfig.WebUrl, this.appConfig.ShinyPokemonImageUrl, pokemon.Id, ".png"));
                    imageRequest = (HttpWebResponse)webRequest.GetResponse();
                    if (imageRequest.StatusCode == HttpStatusCode.OK)
                    {
                        model.HasShiny = true;
                    }
                    else
                    {
                        model.HasShiny = false;
                    }
                }
                catch
                {
                    model.HasShiny = false;
                }

                try
                {
                    webRequest = (HttpWebRequest)HttpWebRequest.Create(string.Concat(this.appConfig.WebUrl, this.appConfig.HomePokemonImageUrl, pokemon.Id, ".png"));
                    imageRequest = (HttpWebResponse)webRequest.GetResponse();
                    if (imageRequest.StatusCode == HttpStatusCode.OK)
                    {
                        model.HasHome = true;
                    }
                    else
                    {
                        model.HasHome = false;
                    }
                }
                catch
                {
                    model.HasHome = false;
                }

                return this.PartialView("_FillPokemonImages", model);
            }
            else
            {
                return this.RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Gets a list of generations based on what pokemon are available in the given game.
        /// </summary>
        /// <param name="selectedGame">The game used to filter the generations.</param>
        /// <returns>The refreshed list of generations.</returns>
        [Route("get-generations")]
        public TeamRandomizerListViewModel GetGenerations(int selectedGame)
        {
            try
            {
                if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    if (selectedGame != 0)
                    {
                        List<PokemonGameDetail> availablePokemon = this.dataService.GetObjects<PokemonGameDetail>(includes: "Pokemon, Pokemon.SpecialGrouping, Game", whereProperty: "GameId", wherePropertyValue: selectedGame).Where(x => !x.Pokemon.IsAltForm).ToList();
                        List<Pokemon> allPokemon = this.dataService.GetAllPokemon().Where(x => availablePokemon.Any(y => y.PokemonId == x.Id)).ToList();
                        Generation selectedGen = this.dataService.GetObjectByPropertyValue<Game>("Id", selectedGame, "Generation").Generation;
                        List<Generation> generationList = this.dataService.GetObjects<Generation>().Where(x => x.Id <= selectedGen.Id).ToList();
                        List<Generation> availableGenerations = new List<Generation>();
                        Game game = this.dataService.GetObjectByPropertyValue<Game>("Id", selectedGame);

                        foreach (var gen in generationList)
                        {
                            if (allPokemon.Where(x => x.Game.GenerationId == gen.Id).ToList().Count != 0)
                            {
                                availableGenerations.Add(gen);
                            }
                        }

                        TeamRandomizerListViewModel model = new TeamRandomizerListViewModel()
                        {
                            AllGenerations = availableGenerations,
                            AllTypes = this.dataService.GetObjects<DataAccess.Models.Type>("Name").Where(x => x.GenerationId <= selectedGen.Id).ToList(),
                            AllSpecialGroupings = availablePokemon.Select(x => x.Pokemon.SpecialGrouping).DistinctBy(x => x.Id).OrderBy(x => x.Id).ToList(),
                            AllFormGroups = this.dataService.GetObjects<FormGroup>("Name", whereProperty: "AppearInTeamRandomizer", wherePropertyValue: true),
                            AllFormGroupGameDetails = this.dataService.GetObjects<FormGroupGameDetail>("FormGroup.Name", "FormGroup"),
                        };

                        return model;
                    }
                    else
                    {
                        TeamRandomizerListViewModel model = new TeamRandomizerListViewModel()
                        {
                            AllGenerations = this.dataService.GetObjects<Generation>().Where(x => this.dataService.GetObjects<Pokemon>(includes: "Game").Where(x => !x.IsAltForm).Any(y => y.Game.GenerationId == x.Id)).ToList(),
                            AllTypes = this.dataService.GetObjects<DataAccess.Models.Type>("Name"),
                            AllSpecialGroupings = this.dataService.GetObjects<SpecialGrouping>(),
                            AllFormGroups = this.dataService.GetObjects<FormGroup>("Name", whereProperty: "AppearInTeamRandomizer", wherePropertyValue: true),
                            AllFormGroupGameDetails = this.dataService.GetObjects<FormGroupGameDetail>("FormGroup.Name", "FormGroup"),
                        };

                        return model;
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                if (!this.User.IsInRole("Owner") && e != null)
                {
                    string commentBody;
                    if (e != null)
                    {
                        commentBody = string.Concat(e.GetType().ToString(), " error during generation fetching.");
                    }
                    else
                    {
                        commentBody = "Unknown error during generation fetching.";
                    }

                    Comment comment = new Comment()
                    {
                        Name = string.Concat(commentBody, " - Selected Game Id: ", selectedGame),
                    };

                    if (this.User.Identity.Name != null)
                    {
                        comment.CommentorId = this.dataService.GetCurrentUser(this.User).Id;
                    }

                    this.dataService.EmailComment(this.appConfig, comment);
                }

                return null;
            }
        }

        /// <summary>
        /// Gets a list of all pokemon by their type combination.
        /// </summary>
        /// <param name="primaryTypeId">The primary type's id.</param>
        /// <param name="secondaryTypeId">The secondary type's id.</param>
        /// <param name="gameId">The game used to specify the type chart.</param>
        /// <param name="regionalDexId">The game's regional dex used to specify the type chart.</param>
        /// <returns>The fill typing evaluator shared view.</returns>
        [Route("get-pokemon-by-typing")]
        public IActionResult GetPokemonByTyping(int primaryTypeId, int secondaryTypeId, int gameId, int regionalDexId)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                Game game = new Game();

                if (gameId != 0)
                {
                    game = this.dataService.GetObjectByPropertyValue<Game>("Id", gameId);
                }
                else
                {
                    game = new Game()
                    {
                        Id = 0,
                        GenerationId = this.dataService.GetObjects<Generation>("Id").Last().Id,
                        Name = "Any Game",
                    };
                }

                List<PokemonTypeDetail> typingList = this.GetAllPokemonWithSpecificTypes(primaryTypeId, secondaryTypeId, game, regionalDexId);
                List<Pokemon> pokemonList = typingList.ConvertAll(x => x.Pokemon);

                TypingEvaluatorViewModel model = new TypingEvaluatorViewModel()
                {
                    AllPokemonWithTypes = typingList,
                    AllPokemon = pokemonList,
                    AppConfig = this.appConfig,
                    Game = game,
                };

                return this.PartialView("_FillTypingEvaluator", model);
            }
            else
            {
                return this.RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Gets a list of all pokemon by their ability.
        /// </summary>
        /// <param name="abilityId">The ability's id.</param>
        /// <param name="gameId">The game the ability is in.</param>
        /// <returns>The fill typing evaluator shared view.</returns>
        [Route("get-pokemon-by-ability")]
        public IActionResult GetPokemonByAbility(int abilityId, int gameId)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                List<PokemonAbilityDetail> abilityList = this.GetPokemonByAbilityAndGame(abilityId, gameId);
                List<Pokemon> pokemonList = abilityList.ConvertAll(x => x.Pokemon);
                int generationId = 0;
                if (gameId != 0)
                {
                    generationId = this.dataService.GetObjectByPropertyValue<Game>("Id", gameId).GenerationId;
                }

                AbilityEvaluatorPageViewModel model = new AbilityEvaluatorPageViewModel()
                {
                    AllPokemonWithAbility = abilityList,
                    AllPokemon = pokemonList,
                    AppConfig = this.appConfig,
                    GenerationId = generationId,
                    Ability = this.dataService.GetObjectByPropertyValue<Ability>("Id", abilityId),
                };

                return this.PartialView("_FillAbilityEvaluator", model);
            }
            else
            {
                return this.RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Gets a list of all pokemon that is capable of breeding with the given pokemon.
        /// </summary>
        /// <param name="gameId">The game breeding is done in.</param>
        /// <returns>The fill day care evaluator dropdown shared view.</returns>
        [Route("get-pokemon-by-egg-group-dropdown")]
        public IActionResult GetPokemonByEggGroupDropdown(int gameId)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                List<PokemonEggGroupDetail> eggGroupDetails = this.dataService.GetAllBreedablePokemon(gameId);

                return this.PartialView("_FillDayCareEvaluatorDropdown", eggGroupDetails.ConvertAll(x => x.Pokemon));
            }
            else
            {
                return this.RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Adds a shiny hunt into the completed shiny hunt section.
        /// </summary>
        /// <param name="shinyHuntId">The shiny hunt that was completed.</param>
        /// <returns>The fill completed shiny hunt shared view.</returns>
        [Route("add-completed-shiny-hunt")]
        public IActionResult AddCompletedShinyHunt(int shinyHuntId)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                ShinyHunt shinyHunt = this.dataService.GetObjectByPropertyValue<ShinyHunt>("Id", shinyHuntId, "User, Pokemon, Game, HuntingMethod, Pokeball, Mark, PhaseOfHunt");
                List<Game> gamesList = this.dataService.GetObjects<Game>("ReleaseDate, Id").Where(x => x.ReleaseDate <= DateTime.UtcNow && x.GenerationId >= 2).ToList();
                if (gamesList.Exists(x => x.Id == shinyHunt.GameId))
                {
                    shinyHunt.Game = new Game()
                    {
                        Id = gamesList.First(x => x.ReleaseDate == shinyHunt.Game.ReleaseDate).Id,
                        Name = string.Join(" / ", gamesList.Where(x => x.ReleaseDate == shinyHunt.Game.ReleaseDate).Select(x => x.Name)),
                        GenerationId = gamesList.First(x => x.ReleaseDate == shinyHunt.Game.ReleaseDate).GenerationId,
                        ReleaseDate = shinyHunt.Game.ReleaseDate,
                        GameColor = gamesList.First(x => x.ReleaseDate == shinyHunt.Game.ReleaseDate).GameColor,
                    };
                }

                ShinyHuntsViewModel model = new ShinyHuntsViewModel()
                {
                    ShinyHunt = shinyHunt,
                    AppConfig = this.appConfig,
                    IsShared = true,
                };

                return this.PartialView("_FillCompletedShinyHunt", model);
            }
            else
            {
                return this.RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Gets a list of all pokemon that is capable of breeding with the given pokemon.
        /// </summary>
        /// <param name="pokemonId">The parent pokemon.</param>
        /// <param name="gameId">The game breeding is done in.</param>
        /// <returns>The fill day care evaluator shared view.</returns>
        [Route("get-pokemon-by-egg-group")]
        public IActionResult GetPokemonByEggGroup(int pokemonId, int gameId)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                try
                {
                    PokemonEggGroupDetail searchedEggGroupDetails = this.dataService.GetObjects<PokemonEggGroupDetail>(includes: "Pokemon, Pokemon.Form, PrimaryEggGroup, SecondaryEggGroup", whereProperty: "PokemonId", wherePropertyValue: pokemonId).Last();
                    GenderRatio genderRatio = this.dataService.GetObjectByPropertyValue<GenderRatio>("Id", searchedEggGroupDetails.Pokemon.GenderRatioId);
                    List<PokemonEggGroupDetail> eggGroupList = new List<PokemonEggGroupDetail>();
                    List<Pokemon> pokemonList = new List<Pokemon>();
                    List<Pokemon> originalPokemon = new List<Pokemon>();

                    if (pokemonId == this.dataService.GetPokemon("Manaphy").Id || pokemonId == this.dataService.GetPokemon("Phione").Id || (genderRatio.MaleRatio == 0 && genderRatio.FemaleRatio == 0 && pokemonId != this.dataService.GetPokemon("Ditto").Id))
                    {
                        eggGroupList.Add(this.dataService.GetObjectByPropertyValue<PokemonEggGroupDetail>("Pokemon.Name", "Ditto", "Pokemon, Pokemon.GenderRatio, PrimaryEggGroup, SecondaryEggGroup"));
                        pokemonList.Add(this.dataService.GetPokemon("Ditto"));
                        originalPokemon = eggGroupList.ConvertAll(x => x.Pokemon);
                    }
                    else if (pokemonId == this.dataService.GetPokemon("Ditto").Id)
                    {
                        eggGroupList = this.dataService.GetAllBreedablePokemon(gameId).OrderBy(x => x.Pokemon.PokedexNumber).ThenBy(x => x.PokemonId).ToList();
                        eggGroupList.Remove(eggGroupList.Find(x => x.PokemonId == pokemonId));
                        pokemonList.AddRange(eggGroupList.ConvertAll(x => x.Pokemon));
                        originalPokemon = eggGroupList.ConvertAll(x => x.Pokemon);
                    }
                    else
                    {
                        eggGroupList = this.GetAllPokemonWithSpecificEggGroups((int)searchedEggGroupDetails.PrimaryEggGroupId, searchedEggGroupDetails.SecondaryEggGroupId);
                        List<PokemonEggGroupDetail> breedablePokemonList = this.dataService.GetAllBreedablePokemon(gameId);
                        if (eggGroupList.Any(x => x.Pokemon.Name == "Manaphy"))
                        {
                            eggGroupList.Remove(eggGroupList.Find(x => x.Pokemon.Name == "Manaphy"));
                        }

                        if (eggGroupList.Any(x => x.Pokemon.Name == "Phione"))
                        {
                            eggGroupList.Remove(eggGroupList.Find(x => x.Pokemon.Name == "Phione"));
                        }

                        eggGroupList = eggGroupList.Where(x => breedablePokemonList.Any(y => y.PokemonId == x.PokemonId)).ToList();
                        List<PokemonEggGroupDetail> finalEggGroupList = new List<PokemonEggGroupDetail>(eggGroupList);

                        foreach (var p in eggGroupList.Where(x => x.Pokemon.GenderRatioId == 10 || ((genderRatio.Id == 1 || genderRatio.Id == 9) && x.Pokemon.GenderRatioId == genderRatio.Id)).ToList())
                        {
                            finalEggGroupList.Remove(p);
                        }

                        eggGroupList = finalEggGroupList;
                        eggGroupList.Add(this.dataService.GetObjectByPropertyValue<PokemonEggGroupDetail>("Pokemon.Name", "Ditto", "Pokemon, Pokemon.GenderRatio, PrimaryEggGroup, SecondaryEggGroup"));
                        originalPokemon = eggGroupList.ConvertAll(x => x.Pokemon);
                        pokemonList.AddRange(originalPokemon);
                    }

                    (originalPokemon ??= eggGroupList.ConvertAll(x => x.Pokemon)).RemoveAll(x => !eggGroupList.Select(y => y.PokemonId).Contains(x.Id));

                    List<EggGroup> pokemonEggGroupList = new List<EggGroup>
                    {
                        searchedEggGroupDetails.PrimaryEggGroup,
                    };

                    if (searchedEggGroupDetails.SecondaryEggGroup != null)
                    {
                        pokemonEggGroupList.Add(searchedEggGroupDetails.SecondaryEggGroup);
                    }

                    Game game = this.dataService.GetObjectByPropertyValue<Game>("Id", gameId);
                    List<Game> gamesWithSameReleaseDate = this.dataService.GetObjects<Game>().Where(x => x.ReleaseDate == game.ReleaseDate).ToList();
                    if (gamesWithSameReleaseDate.Count() > 1)
                    {
                        game.Name = gamesWithSameReleaseDate.First().Name;
                        for (var i = 1; i < gamesWithSameReleaseDate.Count(); i++)
                        {
                            game.Name += string.Concat(" / ", gamesWithSameReleaseDate[i].Name);
                        }
                    }

                    EggGroupEvaluatorViewModel model = new EggGroupEvaluatorViewModel()
                    {
                        AllPokemonWithEggGroups = eggGroupList.OrderBy(x => x.Pokemon.PokedexNumber).ThenBy(x => x.PokemonId).ToList(),
                        AllPokemon = pokemonList.OrderBy(x => x.PokedexNumber).ThenBy(x => x.Id).ToList(),
                        AllOriginalPokemon = originalPokemon.OrderBy(x => x.PokedexNumber).ThenBy(x => x.Id).ToList(),
                        AppConfig = this.appConfig,
                        SearchedPokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form"),
                        SearchedGame = game,
                        PokemonEggGroups = pokemonEggGroupList,
                        GenerationId = this.dataService.GetObjects<Generation>().Last().Id,
                    };

                    return this.PartialView("_FillDayCareEvaluator", model);
                }
                catch (Exception e)
                {
                    if (!this.User.IsInRole("Owner") && e != null)
                    {
                        string commentBody;
                        if (e != null)
                        {
                            commentBody = string.Concat(e.GetType().ToString(), " error while grabbing pokemon by egg group.");
                        }
                        else
                        {
                            commentBody = "Unknown error while grabbing pokemon by egg group.";
                        }

                        commentBody = string.Concat(commentBody, " - Pokemon Id: {", string.Join(", ", pokemonId), "}");
                        commentBody = string.Concat(commentBody, " - Game Id: {", string.Join(", ", gameId), "}");
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

                        return this.RedirectToAction("Error", "Index");
                    }
                    else
                    {
                        return this.RedirectToAction("Index", "Home");
                    }
                }
            }
            else
            {
                return this.RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Gets a list of pokemon with a specific form or form group.
        /// </summary>
        /// <param name="formId">The form or form group's id.</param>
        /// <returns>The fill form evaluator shared view.</returns>
        [Route("get-pokemon-by-alternate-form")]
        public IActionResult GetPokemonByAlternateForm(int formId)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                List<Pokemon> pokemonFormList = new List<Pokemon>();
                Form form = this.dataService.GetObjectByPropertyValue<Form>("Id", formId, "FormGroup");
                if (form.FormGroup != null)
                {
                    List<Form> formList = this.dataService.GetObjects<Form>(whereProperty: "FormGroupId", wherePropertyValue: form.FormGroupId);
                    foreach (var f in formList)
                    {
                        pokemonFormList.AddRange(this.dataService.GetObjects<Pokemon>(includes: "Form", whereProperty: "FormId", wherePropertyValue: f.Id));
                    }
                }
                else
                {
                    pokemonFormList = this.dataService.GetObjects<Pokemon>(includes: "Form", whereProperty: "FormId", wherePropertyValue: form.Id);
                }

                FormEvaluatorViewModel model = new FormEvaluatorViewModel()
                {
                    AllPokemon = pokemonFormList.OrderBy(x => x.PokedexNumber).ThenBy(x => x.Id).ToList(),
                    AppConfig = this.appConfig,
                    GenerationId = this.dataService.GetObjects<Generation>().Last().Id,
                };

                return this.PartialView("_FillFormEvaluator", model);
            }
            else
            {
                return this.RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Gets a list of types that are available in the given game.
        /// </summary>
        /// <param name="gameId">The game's id.</param>
        /// <returns>The fill typing evaluator types shared view.</returns>
        [Route("get-types-by-game")]
        public IActionResult GrabTypingEvaluatorTypes(int gameId)
        {
            if (gameId == 0)
            {
                gameId = this.dataService.GetObjects<Game>().Last().Id;
            }

            Game game = this.dataService.GetObjectByPropertyValue<Game>("Id", gameId);
            List<Pokedex.DataAccess.Models.Type> model = this.dataService.GetObjects<DataAccess.Models.Type>("Name").Where(x => x.GenerationId <= game.GenerationId && x.Name != "Stellar").ToList();
            return this.PartialView("_FillTypingEvaluatorTypes", model);
        }

        /// <summary>
        /// Gets a list of regional dexes that are available in the given game.
        /// </summary>
        /// <param name="gameId">The game's id.</param>
        /// <returns>The fill typing evaluator regional dexes shared view.</returns>
        [Route("get-regional-dexes-by-game")]
        public IActionResult GrabTypingEvaluatorRegionalDexes(int gameId)
        {
            List<RegionalDex> model = this.dataService.GetObjects<RegionalDex>().Where(x => x.GameId == gameId).ToList();
            return this.PartialView("_FillTypingEvaluatorRegionalDexes", model);
        }

        /// <summary>
        /// Grabs the details about two pokemon given the games provided.
        /// </summary>
        /// <param name="firstPokemonId">The id of the first pokemon.</param>
        /// <param name="firstGameId">The game from which the first pokemon's details will be grabbed.</param>
        /// <param name="secondPokemonId">The id of the second pokemon.</param>
        /// <param name="secondGameId">The game from which the second pokemon's details will be grabbed.</param>
        /// <returns>The pokemon difference shared view.</returns>
        [Route("get-pokemon-differences")]
        public IActionResult GrabPokemonDifferences(int firstPokemonId, int firstGameId, int secondPokemonId, int secondGameId)
        {
            try
            {
                List<Pokemon> pokemonList = this.dataService.GetAllPokemon();
                PokemonViewModel firstPokemonDetails = this.dataService.GetPokemonDetails(pokemonList.Find(x => x.Id == firstPokemonId), this.appConfig);
                PokemonViewModel secondPokemonDetails = this.dataService.GetPokemonDetails(pokemonList.Find(x => x.Id == secondPokemonId), this.appConfig);
                List<Game> games = this.dataService.GetObjects<Game>();
                List<Game> selectedGames = new List<Game>
                {
                    games.Find(x => x.Id == firstGameId),
                    games.Find(x => x.Id == secondGameId),
                };

                List<PokemonViewModel> pokemonViewList = new List<PokemonViewModel>
                {
                    firstPokemonDetails,
                    secondPokemonDetails,
                };

                PokemonDifferenceSharedViewModel model = new PokemonDifferenceSharedViewModel()
                {
                    AllPokemon = pokemonViewList,
                    AllGames = selectedGames,
                    AppConfig = this.appConfig,
                };

                return this.PartialView("_FillPokemonDifferences", model);
            }
            catch (Exception e)
            {
                if (!this.User.IsInRole("Owner") && e != null)
                {
                    string commentBody;
                    if (e != null)
                    {
                        commentBody = string.Concat(e.GetType().ToString(), " error during the pokemon comparison.");
                    }
                    else
                    {
                        commentBody = "Unknown error during the pokemon comparison.";
                    }

                    commentBody = string.Concat(commentBody, " - First Pokemon Id: ", firstPokemonId);
                    commentBody = string.Concat(commentBody, " - First Game Id: ", firstGameId);
                    commentBody = string.Concat(commentBody, " - Second Pokemon Id: ", secondPokemonId);
                    commentBody = string.Concat(commentBody, " - Second Game Id: ", secondGameId);
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
            }

            return null;
        }

        /// <summary>
        /// Gets a list of abilities that are available in the given game.
        /// </summary>
        /// <param name="gameId">The game's id.</param>
        /// <returns>The fill ability evaluator abilities shared view.</returns>
        [Route("get-abilities-by-game")]
        public IActionResult GrabAbilityEvaluatorAbilities(int gameId)
        {
            List<Ability> model = new List<Ability>();
            if (gameId != 0)
            {
                Game game = this.dataService.GetObjectByPropertyValue<Game>("Id", gameId);
                List<Pokemon> pokemonList = this.dataService.GetObjects<PokemonGameDetail>(includes: "Pokemon", whereProperty: "GameId", wherePropertyValue: gameId).Select(x => x.Pokemon).ToList();
                pokemonList = pokemonList.Distinct().ToList();
                List<PokemonAbilityDetail> pokemonAbilities = this.dataService.GetObjects<PokemonAbilityDetail>(includes: "PrimaryAbility, SecondaryAbility, HiddenAbility").Where(x => x.GenerationId <= game.GenerationId).ToList();
                pokemonAbilities = pokemonAbilities.Where(x => pokemonList.Any(y => y.Id == x.PokemonId)).ToList();
                if (pokemonAbilities.ConvertAll(x => x.PrimaryAbility).Any(x => x != null))
                {
                    model.AddRange(pokemonAbilities.ConvertAll(x => x.PrimaryAbility).Where(x => x != null).ToList());
                }

                if (pokemonAbilities.ConvertAll(x => x.SecondaryAbility).Any(x => x != null))
                {
                    model.AddRange(pokemonAbilities.ConvertAll(x => x.SecondaryAbility).Where(x => x != null).ToList());
                }

                if (pokemonAbilities.ConvertAll(x => x.HiddenAbility).Any(x => x != null))
                {
                    model.AddRange(pokemonAbilities.ConvertAll(x => x.HiddenAbility).Where(x => x != null).ToList());
                }

                model = model.Distinct().ToList();
            }
            else
            {
                model = this.dataService.GetObjects<Ability>().ToList();
            }

            return this.PartialView("_FillAbilityEvaluatorAbilities", model.OrderBy(x => x.Name).ToList());
        }

        /// <summary>
        /// Gets a list of months that any pages were viewed in by the year.
        /// </summary>
        /// <param name="year">The year in question.</param>
        /// <param name="pokemonPageCheck">Checks whether or not this pertains to the pokemon page stat page.</param>
        /// <returns>The fill month in year shared view.</returns>
        [Route("get-month-by-year/{year:int}/{pokemonPageCheck}")]
        public IActionResult GrabMonthByYear(int year, string pokemonPageCheck)
        {
            DateTime month = new DateTime(year, 1, 1);
            DateTime finalMonth = new DateTime(year, 12, 1);
            if (DateTime.Now.Year == year && finalMonth.Month > DateTime.Now.Month)
            {
                finalMonth = DateTime.Now;
            }

            List<string> model = new List<string>()
            {
                month.ToString("MMMM"),
            };
            while (month.Month < finalMonth.Month)
            {
                month = month.AddMonths(1);
                model.Add(month.ToString("MMMM"));
            }

            return this.PartialView("_FillMonthInYear", model);
        }

        /// <summary>
        /// Gets a list of days that any pages were viewed in by the month and year.
        /// </summary>
        /// <param name="month">The month in question.</param>
        /// <param name="year">The year in question.</param>
        /// <param name="pokemonPageCheck">Checks whether or not this pertains to the pokemon page stat page.</param>
        /// <returns>The fill day in month shared view.</returns>
        [Route("get-day-by-month/{month}/{year:int}/{pokemonPageCheck}")]
        public IActionResult GrabDayByMonth(string month, int year, string pokemonPageCheck)
        {
            int monthInt = DateTime.ParseExact(month, "MMMM", CultureInfo.CurrentCulture).Month;
            DateTime day = new DateTime(year, monthInt, 1);
            DateTime finalDay = new DateTime(year, monthInt, DateTime.DaysInMonth(year, monthInt));
            if (DateTime.Now.Year == year && DateTime.Now.Month == monthInt && finalDay.Day > DateTime.Now.Day)
            {
                finalDay = DateTime.Now;
            }

            List<string> model = new List<string>()
            {
                day.ToString("dd"),
            };
            while (day.Day < finalDay.Day)
            {
                day = day.AddDays(1);
                model.Add(day.ToString("dd"));
            }

            return this.PartialView("_FillDayInMonth", model);
        }

        /// <summary>
        /// Gets a list of stats for any page that was viewed in by the day, month, and year.
        /// </summary>
        /// <param name="day">The day in question.</param>
        /// <param name="month">The month in question.</param>
        /// <param name="year">The year in question.</param>
        /// <param name="pokemonPageCheck">Checks whether or not this pertains to the pokemon page stat page.</param>
        /// <returns>The fill stats in date shared view.</returns>
        [Route("get-stats-by-date/{day:int}/{month}/{year:int}/{pokemonPageCheck}")]
        public IActionResult GrabStatsByDate(int day, string month, int year, string pokemonPageCheck)
        {
            if (pokemonPageCheck == "noPokemonPage")
            {
                List<PageStat> pageStats = this.dataService.GetObjects<PageStat>("VisitDate").Where(x => x.VisitDate.Year == year).ToList();
                if (month != "0")
                {
                    pageStats = pageStats.Where(x => x.VisitDate.ToString("MMMM") == month).ToList();
                    if (day != 0)
                    {
                        pageStats = pageStats.Where(x => x.VisitDate.Day == day).ToList();
                    }
                }

                pageStats = pageStats.Where(x => !x.Name.Contains("Pokemon Page -")).ToList();
                return this.PartialView("_FillStatsInDate", pageStats);
            }
            else
            {
                List<PokemonPageStat> pokemonPageStats = this.dataService.GetObjects<PokemonPageStat>("VisitDate", "Pokemon, Pokemon.Form, Form").Where(x => x.VisitDate.Year == year).ToList();
                if (month != "0")
                {
                    pokemonPageStats = pokemonPageStats.Where(x => x.VisitDate.ToString("MMMM") == month).ToList();
                    if (day != 0)
                    {
                        pokemonPageStats = pokemonPageStats.Where(x => x.VisitDate.Day == day).ToList();
                    }
                }

                List<Pokemon> pokemonList = pokemonPageStats.DistinctBy(x => x.PokemonId).Select(x => x.Pokemon).ToList();
                pokemonList.Where(x => x.IsAltForm).ForEach(x => x.Name = x.NameWithForm);

                PokemonPageStatsViewModel model = new PokemonPageStatsViewModel()
                {
                    PokemonList = pokemonList.OrderBy(x => x.PokedexNumber).ThenBy(x => x.Id).ToList(),
                    PageStatList = pokemonPageStats,
                    AppConfig = this.appConfig,
                    Generation = this.dataService.GetObjects<Generation>().Last(),
                };

                return this.PartialView("_FillPokemonStatsInDate", model);
            }
        }

        /// <summary>
        /// Gets and sorts a list of pokemon by their popularity on the site.
        /// </summary>
        /// <returns>The fill pokemon stats by popularity shared view.</returns>
        [Route("get-stats-by-popularity")]
        public IActionResult GrabStatsByPopularity()
        {
            List<PokemonPageStat> pokemonPageStats = this.dataService.GetObjects<PokemonPageStat>("VisitDate", "Pokemon, Form");
            List<Pokemon> allPokemon = this.dataService.GetObjects<Pokemon>("PokedexNumber, Id");
            List<Pokemon> unOrderedPokemonList = new List<Pokemon>();
            List<Pokemon> pokemonList = new List<Pokemon>();
            PokemonPageStat pageStat;
            Pokemon pokemon;
            int viewCount = 0;
            List<IGrouping<int, int>> pokemonNames = pokemonPageStats.Select(x => x.PokemonId).ToList().GroupBy(x => x).OrderByDescending(x => x.Count()).ToList();
            foreach (var p in pokemonNames)
            {
                pageStat = pokemonPageStats.First(x => x.PokemonId == p.Key);
                if (p.Count() != viewCount)
                {
                    viewCount = p.Count();
                    pokemonList = pokemonList.Concat(unOrderedPokemonList.OrderBy(x => x.PokedexNumber).ThenBy(x => x.Id).ToList()).ToList();
                    unOrderedPokemonList = new List<Pokemon>();
                }

                if (pageStat.FormId != null)
                {
                    pokemon = pageStat.Pokemon;
                    pokemon.Name = pokemon.NameWithForm;
                    unOrderedPokemonList.Add(pokemon);
                }
                else
                {
                    unOrderedPokemonList.Add(allPokemon.First(x => x.Id == p.Key));
                }
            }

            pokemonList = pokemonList.Concat(unOrderedPokemonList.OrderBy(x => x.PokedexNumber).ThenBy(x => x.Id).ToList()).ToList();

            PokemonPageStatsViewModel model = new PokemonPageStatsViewModel()
            {
                PokemonList = pokemonList,
                PageStatList = pokemonPageStats,
                AppConfig = this.appConfig,
                Generation = this.dataService.GetObjects<Generation>().Last(),
            };

            return this.PartialView("_FillPokemonStatsByPopularity", model);
        }

        /// <summary>
        /// Gets the type chart for the given typing and generation.
        /// </summary>
        /// <param name="primaryTypeId">The id of the primary type.</param>
        /// <param name="secondaryTypeId">The id of the secondary type.</param>
        /// <param name="generationId">The id of the generation.</param>
        /// <param name="teraType">The tera type of the pokemon. Used only if terastallized.</param>
        /// <returns>Returns the type effectiveness given the typing and generation.</returns>
        [Route("get-type-chart-typing")]
        public TypeEffectivenessViewModel GetTypeChartTyping(int primaryTypeId, int secondaryTypeId, int generationId, string teraType = "")
        {
            List<DataAccess.Models.Type> typeList = this.dataService.GetObjects<DataAccess.Models.Type>("Name");
            List<DataAccess.Models.Type> pokemonTypes = new List<DataAccess.Models.Type>();
            List<string> strongAgainst = new List<string>();
            List<string> superStrongAgainst = new List<string>();
            List<string> weakAgainst = new List<string>();
            List<string> superWeakAgainst = new List<string>();
            List<string> immuneTo = new List<string>();
            List<TypeChart> typeChart;
            string effectiveValue, attackType;
            if (generationId == 0)
            {
                generationId = this.dataService.GetObjects<Generation>().Last().Id;
            }

            pokemonTypes.Add(this.dataService.GetObjectByPropertyValue<DataAccess.Models.Type>("Id", primaryTypeId));

            if (secondaryTypeId != 0)
            {
                pokemonTypes.Add(this.dataService.GetObjectByPropertyValue<DataAccess.Models.Type>("Id", secondaryTypeId));
            }

            foreach (var type in pokemonTypes)
            {
                typeChart = this.dataService.GetObjects<TypeChart>(includes: "Attack, Defend", whereProperty: "DefendId", wherePropertyValue: type.Id);

                List<int> generations = typeChart.Select(x => x.GenerationId).Distinct().OrderByDescending(x => x).ToList();
                typeChart = typeChart.Where(x => x.GenerationId == generations.First(x => x <= generationId)).ToList();

                foreach (var t in typeList)
                {
                    if (typeChart.Exists(x => x.Attack.Name == t.Name))
                    {
                        attackType = t.Name;
                        effectiveValue = typeChart.Find(x => x.Attack.Name == attackType).Effective.ToString("0.####");
                        if (effectiveValue == "0")
                        {
                            strongAgainst.Remove(attackType);
                            weakAgainst.Remove(attackType);
                            immuneTo.Add(attackType);
                        }
                        else if (effectiveValue == "0.5" && immuneTo.Where(x => x == attackType).ToList().Count == 0)
                        {
                            if (strongAgainst.Exists(x => x == attackType))
                            {
                                strongAgainst.Remove(attackType);
                                superStrongAgainst.Add(string.Concat(attackType, " Quad"));
                            }
                            else if (weakAgainst.Exists(x => x == attackType))
                            {
                                weakAgainst.Remove(attackType);
                            }
                            else
                            {
                                strongAgainst.Add(attackType);
                            }
                        }
                        else if (effectiveValue == "2" && immuneTo.Where(x => x == attackType).ToList().Count == 0)
                        {
                            if (weakAgainst.Exists(x => x == attackType))
                            {
                                weakAgainst.Remove(attackType);
                                superWeakAgainst.Add(string.Concat(attackType, " Quad"));
                            }
                            else if (strongAgainst.Exists(x => x == attackType))
                            {
                                strongAgainst.Remove(attackType);
                            }
                            else
                            {
                                weakAgainst.Add(attackType);
                            }
                        }
                    }
                }
            }

            immuneTo.Sort();
            strongAgainst.Sort();
            superStrongAgainst.Sort();
            weakAgainst.Sort();
            superWeakAgainst.Sort();

            if (!string.IsNullOrEmpty(teraType) && generationId == 9)
            {
                weakAgainst.Add("Stellar");
            }

            TypeEffectivenessViewModel effectivenessChart = new TypeEffectivenessViewModel()
            {
                ImmuneTo = immuneTo,
                StrongAgainst = strongAgainst,
                WeakAgainst = weakAgainst,
                SuperStrongAgainst = superStrongAgainst,
                SuperWeakAgainst = superWeakAgainst,
                AppConfig = this.appConfig,
            };

            return effectivenessChart;
        }

        /// <summary>
        /// Marks down that a shiny pokemon was generated while randomizing a team.
        /// </summary>
        [Route("shiny-pokemon-found")]
        public void ShinyPokemonFound()
        {
            this.dataService.AddPageView("Shiny Pokemon found in Team Randomizer", false);
        }

        /// <summary>
        /// Grabs all of the games available for the selected pokemon.
        /// </summary>
        /// <param name="pokemonId">The selected pokemon Ids.</param>
        /// <returns>The list of available games.</returns>
        [Route("get-games-by-shiny-huntable-pokemon")]
        public List<Game> GetGamesByShinyHuntablePokemon(List<int> pokemonId)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return this.dataService.GetMultipleShinyHuntGames(pokemonId);
            }

            return null;
        }

        /// <summary>
        /// Grabs all of the games available for the selected pokemon.
        /// </summary>
        /// <param name="pokemonId">The selected pokemon's Id.</param>
        /// <returns>The list of available games.</returns>
        [Route("get-games-by-pokemon")]
        public List<Game> GetGamesByPokemon(int pokemonId)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                List<Game> pokemonGamesIn = this.dataService.GetObjects<PokemonGameDetail>("GameId", "Game", "PokemonId", pokemonId).ConvertAll(x => x.Game);
                List<Game> games = this.dataService.GetGamesGroupedByReleaseDate().Where(x => x.Id != 43).ToList();
                return games.Where(x => pokemonGamesIn.Any(y => y.Id == x.Id)).ToList();
            }

            return null;
        }

        /// <summary>
        /// Grabs all of the hunting methods available for the selected game.
        /// </summary>
        /// <param name="gameId">The selected game's Id.</param>
        /// <returns>The list of available hunting methods.</returns>
        [Route("get-hunting-methods")]
        public List<HuntingMethod> GetHuntingMethods(int gameId)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                List<HuntingMethod> huntingMethods = this.dataService.GetObjects<HuntingMethodGameDetail>("HuntingMethod.Name", "HuntingMethod", "GameId", gameId).ConvertAll(x => x.HuntingMethod);

                return huntingMethods;
            }

            return null;
        }

        /// <summary>
        /// Grabs all of the pokeballs available for the selected game.
        /// </summary>
        /// <param name="gameId">The selected game's Id.</param>
        /// <param name="huntingMethodId">The selected hunting method's Id.</param>
        /// <returns>The list of available pokeballs.</returns>
        [Route("get-shiny-hunt-pokeballs")]
        public List<Pokeball> GetShinyHuntPokeballs(int gameId, int huntingMethodId)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                List<Pokeball> pokeballs = this.dataService.GetPokeballs(gameId, huntingMethodId, this.User, this.appConfig);

                return pokeballs;
            }

            return null;
        }

        /// <summary>
        /// Grabs all of the marks available for the selected game.
        /// </summary>
        /// <param name="gameId">The selected game's Id.</param>
        /// <param name="huntingMethodId">The selected hunting method's Id.</param>
        /// <returns>The list of available marks.</returns>
        [Route("get-shiny-hunt-marks")]
        public List<Mark> GetShinyHuntMarks(int gameId, int huntingMethodId)
        {
            Game game = this.dataService.GetObjectByPropertyValue<Game>("Id", gameId);
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest" && huntingMethodId != 4 && huntingMethodId != 5 && huntingMethodId != 15)
            {
                List<MarkGameDetail> markGameDetails = this.dataService.GetObjects<MarkGameDetail>("Mark.Name", "Mark", "GameId", gameId);
                List<Mark> marks = null;
                if (markGameDetails.Count() > 0)
                {
                    marks = this.dataService.GetObjects<MarkGameDetail>("Mark.Name", "Mark", "GameId", gameId).ConvertAll(x => x.Mark);
                }

                return marks;
            }

            return null;
        }

        /// <summary>
        /// Toggles whether or not a user will see alternate forms on the shiny dex progression page upon visiting it.
        /// </summary>
        /// <param name="altFormToggle">Tells the method whether or not the user wants shiny alternate forms to be shown.</param>
        [Route("toggle-shiny-alt-forms")]
        public void ToggleShinyAltForms(string altFormToggle)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest" && this.User.Identity.Name != null)
            {
                User user = this.dataService.GetCurrentUser(this.User);
                user.ShowShinyAltForms = altFormToggle switch
                {
                    "hide" => false,
                    "show" => true,
                    _ => false,
                };
                this.dataService.UpdateObject<User>(user);
            }
        }

        /// <summary>
        /// Toggles whether or not a user will see gender differences on the shiny dex progression page upon visiting it.
        /// </summary>
        /// <param name="genderDifferenceToggle">Tells the method whether or not the user wants shiny gender differences to be shown.</param>
        [Route("toggle-shiny-gender-differences")]
        public void ToggleShinyGenderDifferences(string genderDifferenceToggle)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest" && this.User.Identity.Name != null)
            {
                User user = this.dataService.GetCurrentUser(this.User);
                user.HideShinyGenderDifferences = genderDifferenceToggle switch
                {
                    "hide" => true,
                    "show" => false,
                    _ => false,
                };
                this.dataService.UpdateObject<User>(user);
            }
        }

        /// <summary>
        /// Toggles whether or not a user will see pokemon already captured shiny on the shiny dex progression page upon visiting it.
        /// </summary>
        /// <param name="capturedShiniesToggle">Tells the method whether or not the user wants pokemon already captured shiny to be shown.</param>
        [Route("toggle-captured-shinies")]
        public void ToggleCapturedShinies(string capturedShiniesToggle)
        {
            if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest" && this.User.Identity.Name != null)
            {
                User user = this.dataService.GetCurrentUser(this.User);
                user.HideCapturedShinyPokemon = capturedShiniesToggle switch
                {
                    "hide" => true,
                    "show" => false,
                    _ => false,
                };
                this.dataService.UpdateObject<User>(user);
            }
        }

        /// <summary>
        /// Abandon a shiny hunt.
        /// </summary>
        /// <param name="shinyHuntId">The abandoned shiny hunt's Id.</param>
        [Route("abandon-shiny-hunt")]
        public void DeleteShinyHunt(int shinyHuntId)
        {
            ShinyHunt shinyHunt = this.dataService.GetObjectByPropertyValue<ShinyHunt>("Id", shinyHuntId);
            User user = this.dataService.GetCurrentUser(this.User);
            if (user != null && user.Id == shinyHunt.UserId)
            {
                List<ShinyHunt> phaseHunts = this.dataService.GetObjects<ShinyHunt>(whereProperty: "PhaseOfHuntId", wherePropertyValue: shinyHuntId);
                foreach (var sh in phaseHunts)
                {
                    if (sh.IsCaptured)
                    {
                        sh.PhaseOfHuntId = null;
                        this.dataService.UpdateObject(sh);
                    }
                    else
                    {
                        this.dataService.DeleteObject<ShinyHunt>(sh.Id);
                    }
                }

                this.dataService.DeleteObject<ShinyHunt>(shinyHuntId);
            }
        }

        /// <summary>
        /// Transport the user to their shiny hunt page.
        /// </summary>
        /// <param name="gameId">The id of the game used to search for specific completed shiny hunts.</param>
        /// <param name="isShared">The boolean determining if the complete shiny hunt page was shared.</param>
        /// <param name="username">The username used during the search in case the page was shared.</param>
        /// <param name="sortMethod">The method in which the completed hunts will be sorted upon request.</param>
        /// <returns>The shiny hunt page.</returns>
        [Route("completed-shiny-hunt-data/{gameId:int}/{isShared}/{username}/{sortMethod}")]
        public IActionResult CompletedShinyHuntData(int gameId, bool isShared, string username = "", string sortMethod = "Pokedex")
        {
            List<ShinyHunt> shinyHunts = new List<ShinyHunt>();
            if (isShared)
            {
                User user = this.dataService.GetObjectByPropertyValue<User>("Username", username);
                shinyHunts = this.dataService.GetObjects<ShinyHunt>("Game.GenerationId, Pokemon.PokedexNumber, PokemonId, Id", "User, Pokemon, Pokemon.Form, Game, HuntingMethod, Mark, Sweet, Pokeball, PhaseOfHunt, PhaseOfHunt.Pokemon, PhaseOfHunt.Pokemon.Form", "User.Username", user.Username).Where(x => (x.IsCaptured || (!x.IsCaptured && x.PhaseOfHuntId != null))).ToList();
            }
            else
            {
                shinyHunts = this.dataService.GetObjects<ShinyHunt>("Game.GenerationId, Pokemon.PokedexNumber, PokemonId, Id", "User, Pokemon, Pokemon.Form, Game, HuntingMethod, Mark, Sweet, Pokeball, PhaseOfHunt, PhaseOfHunt.Pokemon, PhaseOfHunt.Pokemon.Form", "User.Username", this.User.Identity.Name).Where(x => (x.IsCaptured || (!x.IsCaptured && x.PhaseOfHuntId != null))).ToList();
            }

            if (gameId != 0)
            {
                shinyHunts = shinyHunts.Where(x => x.GameId == gameId).ToList();
            }

            List<Game> gamesList = this.dataService.GetObjects<Game>("ReleaseDate, Id").Where(x => x.ReleaseDate <= DateTime.UtcNow).ToList();
            gamesList = gamesList.Where(x => shinyHunts.DistinctBy(x => x.Game).Any(y => y.Game.ReleaseDate == x.ReleaseDate)).ToList();
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

            if (sortMethod == "Pokedex")
            {
                shinyHunts = shinyHunts.OrderBy(x => x.Pokemon.PokedexNumber).ThenBy(x => x.PokemonId).ToList();
            }
            else
            {
                shinyHunts = shinyHunts.OrderBy(x => x.DateOfCapture).ThenBy(x => x.Id).ToList();
            }

            ShinyHuntsViewModel model = new ShinyHuntsViewModel()
            {
                AllShinyHunts = shinyHunts,
                EdittedGames = edittedGamesList.OrderBy(x => x.ReleaseDate).ThenBy(x => x.Id).ToList(),
                UnedittedGames = gamesList,
                IsShared = isShared,
                AppConfig = this.appConfig,
            };

            model.ShinyHuntCount = model.AllShinyHunts.Count();

            return this.PartialView("_FillCompletedShinyHunts", model);
        }

        private List<PokemonEggGroupDetail> GetAllPokemonWithSpecificEggGroups(int primaryEggGroupId, int? secondaryEggGroupId)
        {
            List<PokemonEggGroupDetail> pokemonList = this.dataService.GetObjects<PokemonEggGroupDetail>(includes: "Pokemon, Pokemon.GenderRatio, Pokemon.Form, PrimaryEggGroup, SecondaryEggGroup").OrderBy(x => x.GenerationId).GroupBy(x => new { x.PokemonId }).Select(x => x.LastOrDefault()).ToList();

            List<PokemonEggGroupDetail> finalPokemonList = pokemonList.Where(x => x.PrimaryEggGroupId == primaryEggGroupId || x.SecondaryEggGroupId == primaryEggGroupId).ToList();

            if (secondaryEggGroupId != null)
            {
                finalPokemonList.AddRange(pokemonList.Where(x => x.PrimaryEggGroupId == secondaryEggGroupId || x.SecondaryEggGroupId == secondaryEggGroupId).ToList());
            }

            return finalPokemonList.Distinct().ToList();
        }

        private List<PokemonAbilityDetail> GetPokemonByAbilityAndGame(int abilityId, int gameId)
        {
            List<PokemonAbilityDetail> pokemonList = new List<PokemonAbilityDetail>();
            if (gameId != 0)
            {
                Game game = this.dataService.GetObjectByPropertyValue<Game>("Id", gameId);
                pokemonList = this.dataService.GetObjects<PokemonAbilityDetail>(includes: "Pokemon, Pokemon.Game, Pokemon.Form")
                    .Where(x => x.Pokemon.Game.GenerationId <= game.GenerationId)
                    .Where(x => x.GenerationId <= game.GenerationId)
                    .OrderBy(x => x.GenerationId)
                    .GroupBy(x => new { x.PokemonId })
                    .Select(x => x.LastOrDefault())
                    .ToList();

                List<int> exclusionList = pokemonList.Select(x => x.PokemonId).Except(this.dataService.GetObjects<PokemonGameDetail>(includes: "Pokemon, Game", whereProperty: "GameId", wherePropertyValue: gameId).Select(x => x.PokemonId)).ToList();

                foreach (var pokemon in exclusionList)
                {
                    pokemonList.Remove(pokemonList.Find(x => x.PokemonId == pokemon));
                }
            }
            else
            {
                pokemonList = this.dataService.GetObjects<PokemonAbilityDetail>(includes: "Pokemon, Pokemon.Game")
                    .GroupBy(x => new { x.PokemonId })
                    .Select(x => x.LastOrDefault())
                    .ToList();
            }

            return pokemonList.Where(x => x.PrimaryAbilityId == abilityId || x.SecondaryAbilityId == abilityId || x.HiddenAbilityId == abilityId).OrderBy(x => x.Pokemon.PokedexNumber).ThenBy(x => x.PokemonId).ToList();
        }

        private string FillEVs(PokemonTeamEV evs)
        {
            string evString = string.Empty;
            if (evs.EVTotal == 0)
            {
                return "\nEVs: 1 HP / 1 Atk / 1 Def / 1 SpA / 1 SpD / 1 Spe";
            }
            else
            {
                if (evs.Health > 0)
                {
                    evString = string.Concat(evString, evs.Health.ToString(), " HP");
                }

                if (evs.Attack > 0)
                {
                    if (!string.IsNullOrEmpty(evString))
                    {
                        evString = string.Concat(evString, " / ");
                    }

                    evString = string.Concat(evString, evs.Attack.ToString(), " Atk");
                }

                if (evs.Defense > 0)
                {
                    if (!string.IsNullOrEmpty(evString))
                    {
                        evString = string.Concat(evString, " / ");
                    }

                    evString = string.Concat(evString, evs.Defense.ToString(), " Def");
                }

                if (evs.SpecialAttack > 0)
                {
                    if (!string.IsNullOrEmpty(evString))
                    {
                        evString = string.Concat(evString, " / ");
                    }

                    evString = string.Concat(evString, evs.SpecialAttack.ToString(), " SpA");
                }

                if (evs.SpecialDefense > 0)
                {
                    if (!string.IsNullOrEmpty(evString))
                    {
                        evString = string.Concat(evString, " / ");
                    }

                    evString = string.Concat(evString, evs.SpecialDefense.ToString(), " SpD");
                }

                if (evs.Speed > 0)
                {
                    if (!string.IsNullOrEmpty(evString))
                    {
                        evString = string.Concat(evString, " / ");
                    }

                    evString = string.Concat(evString, evs.Speed.ToString(), " Spe");
                }

                return string.Concat("\nEVs: ", evString);
            }
        }

        private string FillIVs(PokemonTeamIV ivs)
        {
            string ivString = string.Empty;
            if (ivs.Health < 31)
            {
                ivString = string.Concat(ivString, ivs.Health.ToString(), " HP");
            }

            if (ivs.Attack < 31)
            {
                if (!string.IsNullOrEmpty(ivString))
                {
                    ivString = string.Concat(ivString, " / ");
                }

                ivString = string.Concat(ivString, ivs.Attack.ToString(), " Atk");
            }

            if (ivs.Defense < 31)
            {
                if (!string.IsNullOrEmpty(ivString))
                {
                    ivString = string.Concat(ivString, " / ");
                }

                ivString = string.Concat(ivString, ivs.Defense.ToString(), " Def");
            }

            if (ivs.SpecialAttack < 31)
            {
                if (!string.IsNullOrEmpty(ivString))
                {
                    ivString = string.Concat(ivString, " / ");
                }

                ivString = string.Concat(ivString, ivs.SpecialAttack.ToString(), " SpA");
            }

            if (ivs.SpecialDefense < 31)
            {
                if (!string.IsNullOrEmpty(ivString))
                {
                    ivString = string.Concat(ivString, " / ");
                }

                ivString = string.Concat(ivString, ivs.SpecialDefense.ToString(), " SpD");
            }

            if (ivs.Speed < 31)
            {
                if (!string.IsNullOrEmpty(ivString))
                {
                    ivString = string.Concat(ivString, " / ");
                }

                ivString = string.Concat(ivString, ivs.Speed.ToString(), " Spe");
            }

            if (!string.IsNullOrEmpty(ivString))
            {
                ivString = string.Concat("\nIVs: ", ivString);
            }

            return ivString;
        }

        private string FillMoveset(PokemonTeamMoveset moveset)
        {
            string movesetString = string.Empty;
            if (!string.IsNullOrEmpty(moveset.FirstMove))
            {
                movesetString = string.Concat(movesetString, "\n- ", moveset.FirstMove);
            }

            if (!string.IsNullOrEmpty(moveset.SecondMove))
            {
                movesetString = string.Concat(movesetString, "\n- ", moveset.SecondMove);
            }

            if (!string.IsNullOrEmpty(moveset.ThirdMove))
            {
                movesetString = string.Concat(movesetString, "\n- ", moveset.ThirdMove);
            }

            if (!string.IsNullOrEmpty(moveset.FourthMove))
            {
                movesetString = string.Concat(movesetString, "\n- ", moveset.FourthMove);
            }

            return movesetString;
        }

        private string ExportPokemonTeam(List<int> pokemonIdList)
        {
            try
            {
                if (this.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    string pokemonTeam = string.Empty;
                    Pokemon pokemon;

                    for (var i = 0; i < pokemonIdList.Count; i++)
                    {
                        if (i != 0)
                        {
                            pokemonTeam = string.Concat(pokemonTeam, "\n\n");
                        }

                        pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonIdList[i], "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth");
                        string pokemonName = pokemon.Name;
                        if (pokemon.IsAltForm)
                        {
                            string pokemonForm = this.GetFormDetails(pokemon.Id);
                            pokemonName = string.Concat(pokemonName, "-", pokemonForm);
                        }

                        pokemonTeam = string.Concat(pokemonTeam, pokemonName);

                        pokemonTeam = string.Concat(pokemonTeam, "\nEVs: 1 HP / 1 Atk / 1 Def / 1 SpA / 1 SpD / 1 Spe");
                    }

                    return pokemonTeam;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                if (!this.User.IsInRole("Owner") && e != null)
                {
                    string commentBody;
                    if (e != null)
                    {
                        commentBody = string.Concat(e.GetType().ToString(), " error during the pokemon team's export.");
                    }
                    else
                    {
                        commentBody = "Unknown error during the pokemon team's export.";
                    }

                    commentBody = string.Concat(commentBody, " - Pokemon Id List: {", string.Join(", ", pokemonIdList), "}");
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

        private string GetFormDetails(int pokemonId)
        {
            string formDetails = string.Empty, itemName = string.Empty;
            Pokemon pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonId, "Form");
            formDetails = string.Concat(pokemon.Form.Name.Replace(' ', '-').Replace("Gigantamax", "Gmax"));
            List<FormItem> formItems = this.dataService.GetObjects<FormItem>();
            FormItem formItem = formItems.Find(x => x.PokemonId == pokemonId);
            if (formItem != null)
            {
                itemName = formItem.Name;
            }

            if (!string.IsNullOrEmpty(itemName))
            {
                formDetails = string.Concat(formDetails, " @ ", itemName);
            }

            return formDetails;
        }

        private List<Pokemon> FilterSpecialGroupings(List<Pokemon> pokemonList, List<string> selectedSpecialGroupings)
        {
            try
            {
                if (selectedSpecialGroupings.Count() > 0)
                {
                    List<string> specialGroupings = this.dataService.GetObjects<SpecialGrouping>().ConvertAll(x => x.Name);
                    foreach (var specialGrouping in specialGroupings.Except(selectedSpecialGroupings).ToList())
                    {
                        pokemonList = pokemonList.Where(x => x.SpecialGrouping.Name != specialGrouping).ToList();
                    }
                }

                return pokemonList;
            }
            catch (Exception e)
            {
                if (!this.User.IsInRole("Owner"))
                {
                    string commentBody;

                    if (e != null)
                    {
                        commentBody = string.Concat(e.GetType().ToString(), " error during team generation. More specifically while filtering special groupings.");
                    }
                    else
                    {
                        commentBody = "Unknown error during team generation. More specifically while filtering special groupings.";
                    }

                    commentBody = string.Concat(commentBody, " - Selected Special Groupings: {", string.Join(", ", selectedSpecialGroupings), "}");

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

                return new List<Pokemon>();
            }
        }

        private List<Pokemon> FilterForms(List<Pokemon> pokemonList, List<string> formList, Game game, bool onlyAltForms, bool multipleMegas, bool multipleGMax)
        {
            try
            {
                if (formList.Count() > 0)
                {
                    List<Pokemon> availableForms = new List<Pokemon>();
                    if (formList.Where(x => x != "Other").Count() > 0)
                    {
                        FormGroup formGroup;
                        List<Form> forms;
                        foreach (var formGroupName in formList.Where(x => x != "Other"))
                        {
                            formGroup = this.dataService.GetObjectByPropertyValue<FormGroup>("Name", formGroupName);
                            forms = this.dataService.GetObjects<Form>(whereProperty: "FormGroupId", wherePropertyValue: formGroup.Id);
                            availableForms.AddRange(pokemonList.Where(x => forms.Any(y => y.Id == x.FormId)));
                        }
                    }

                    if (formList.Contains("Other"))
                    {
                        List<Form> formsToRemove = this.dataService.GetObjects<Form>("Name").Where(x => !x.Randomizable).ToList();
                        availableForms.AddRange(pokemonList.Where(x => x.IsAltForm && !formsToRemove.Any(y => y.Id == x.FormId)));
                    }

                    availableForms = this.RemoveExtraPokemonForms(availableForms, multipleMegas, multipleGMax);
                    availableForms.ForEach(x => x.Name = x.NameWithForm);
                    pokemonList = pokemonList.Where(x => !x.IsAltForm).ToList();
                    pokemonList.AddRange(availableForms);
                }

                if (onlyAltForms)
                {
                    pokemonList = pokemonList.Where(x => x.IsAltForm).ToList();
                }
                else if (formList.Count() == 0)
                {
                    pokemonList = pokemonList.Where(x => !x.IsAltForm).ToList();
                }

                return pokemonList;
            }
            catch (Exception e)
            {
                if (!this.User.IsInRole("Owner"))
                {
                    string commentBody;

                    if (e != null)
                    {
                        commentBody = string.Concat(e.GetType().ToString(), " error during team generation. More specifically while filtering alternate forms.");
                    }
                    else
                    {
                        commentBody = "Unknown error during team generation. More specifically while filtering alternate forms.";
                    }

                    commentBody = string.Concat(commentBody, " - Only Alternate Forms: ", onlyAltForms);
                    commentBody = string.Concat(commentBody, " - Selected Forms: {", string.Join(", ", formList), "}");
                    commentBody = string.Concat(commentBody, " - Multiple Megas: ", multipleMegas);
                    commentBody = string.Concat(commentBody, " - Multiple Gigantamax: ", multipleGMax);

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

                return new List<Pokemon>();
            }
        }

        private List<Pokemon> RemoveExtraPokemonForms(List<Pokemon> pokemonList, bool multipleMegas, bool multipleGMax)
        {
            Random rnd = new Random();
            List<Pokemon> pumpkabooCount = pokemonList.Where(x => x.PokedexNumber == 710).ToList();
            while (pumpkabooCount.Count > 1)
            {
                pokemonList.Remove(pumpkabooCount[rnd.Next(pumpkabooCount.Count)]);
                pumpkabooCount = pokemonList.Where(x => x.PokedexNumber == 710).ToList();
            }

            List<Pokemon> gourgeistCount = pokemonList.Where(x => x.PokedexNumber == 711).ToList();
            while (gourgeistCount.Count > 1)
            {
                pokemonList.Remove(gourgeistCount[rnd.Next(gourgeistCount.Count)]);
                gourgeistCount = pokemonList.Where(x => x.PokedexNumber == 711).ToList();
            }

            if (!multipleMegas)
            {
                List<Pokemon> megaList = pokemonList.Where(x => x.Form.Name.Contains("Mega")).ToList();

                if (megaList.Count > 0)
                {
                    Pokemon mega = megaList[rnd.Next(megaList.Count)];
                    foreach (var p in megaList.Where(x => x.Id != mega.Id))
                    {
                        if (pokemonList.Exists(x => x.Id == p.Id))
                        {
                            pokemonList.Remove(pokemonList.Find(x => x.Id == p.Id));
                        }
                    }
                }
            }

            if (!multipleGMax)
            {
                List<Pokemon> gigantamaxList = pokemonList.Where(x => x.Form.Name.Contains("Gigantamax") || x.Form.Name == "Eternamax").ToList();

                if (gigantamaxList.Count > 0)
                {
                    Pokemon gigantamax = gigantamaxList[rnd.Next(gigantamaxList.Count)];
                    foreach (var p in gigantamaxList.Where(x => x.Id != gigantamax.Id))
                    {
                        if (pokemonList.Exists(x => x.Id == p.Id))
                        {
                            pokemonList.Remove(pokemonList.Find(x => x.Id == p.Id));
                        }
                    }
                }
            }

            return pokemonList;
        }

        private List<Pokemon> FilterEvolutions(List<Pokemon> pokemonList, List<string> evolutionList, Game game)
        {
            try
            {
                if (evolutionList.Count() != 0 && evolutionList.Count() < 4)
                {
                    List<Evolution> allEvolutions = this.dataService.GetObjects<Evolution>(includes: "PreevolutionPokemon, PreevolutionPokemon.Game, EvolutionPokemon, EvolutionPokemon.Game, EvolutionMethod");
                    if (game.Id != 0)
                    {
                        allEvolutions = allEvolutions.Where(x => x.PreevolutionPokemon.Game.ReleaseDate <= game.ReleaseDate && x.EvolutionPokemon.Game.ReleaseDate <= game.ReleaseDate).ToList();
                    }

                    if (game.Id == 16)
                    {
                        allEvolutions = allEvolutions.Where(x => x.GenerationId == 1 || x.PreevolutionPokemonId == 1339).ToList();
                    }

                    List<Pokemon> evolutions = new List<Pokemon>();
                    if (evolutionList.Contains("stage1Pokemon"))
                    {
                        foreach (var p in pokemonList)
                        {
                            if (allEvolutions.Exists(x => x.PreevolutionPokemonId == p.Id) && !allEvolutions.Exists(x => x.EvolutionPokemonId == p.Id))
                            {
                                evolutions.Add(p);
                            }
                        }
                    }

                    if (evolutionList.Contains("middleEvolution"))
                    {
                        foreach (var p in pokemonList)
                        {
                            if (allEvolutions.Exists(x => x.PreevolutionPokemonId == p.Id) && allEvolutions.Exists(x => x.EvolutionPokemonId == p.Id))
                            {
                                evolutions.Add(p);
                            }
                        }
                    }

                    if (evolutionList.Contains("onlyFullyEvolved"))
                    {
                        foreach (var p in pokemonList)
                        {
                            if (!allEvolutions.Exists(x => x.PreevolutionPokemonId == p.Id) && allEvolutions.Exists(x => x.EvolutionPokemonId == p.Id))
                            {
                                evolutions.Add(p);
                            }
                        }
                    }

                    if (evolutionList.Contains("noEvolutionLine"))
                    {
                        foreach (var p in pokemonList)
                        {
                            if (!allEvolutions.Exists(x => x.PreevolutionPokemonId == p.Id) && !allEvolutions.Exists(x => x.EvolutionPokemonId == p.Id))
                            {
                                evolutions.Add(p);
                            }
                        }
                    }

                    pokemonList = evolutions;
                }

                return pokemonList;
            }
            catch (Exception e)
            {
                if (!this.User.IsInRole("Owner"))
                {
                    string commentBody;

                    if (e != null)
                    {
                        commentBody = string.Concat(e.GetType().ToString(), " error during team generation. More specifically while filtering evolutions.");
                    }
                    else
                    {
                        commentBody = "Unknown error during team generation. More specifically while filtering evolutions.";
                    }

                    commentBody = string.Concat(commentBody, " - Selected Evolutions: {", string.Join(", ", evolutionList), "}");
                    commentBody = string.Concat(commentBody, " - Selected Game: ", game.Id);

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

                return new List<Pokemon>();
            }
        }

        private List<Pokemon> FilterTypes(List<Pokemon> pokemonList, List<int> types, Game game)
        {
            try
            {
                List<DataAccess.Models.Type> typeList = this.dataService.GetObjects<DataAccess.Models.Type>();
                int totalTypeCount = typeList.Count();
                if (game.GenerationId != 0)
                {
                    totalTypeCount = typeList.Where(x => x.GenerationId <= game.GenerationId).Count();
                }

                if (types.Count() > 0 && types.Count() < totalTypeCount)
                {
                    List<PokemonTypeDetail> pokemonTypeDetails = new List<PokemonTypeDetail>();
                    if (game.Id != 0)
                    {
                        pokemonTypeDetails = this.GetAllPokemonWithSpecificType(types, game.GenerationId, pokemonList);
                    }
                    else
                    {
                        pokemonTypeDetails = this.dataService.GetObjects<PokemonTypeDetail>("Pokemon.PokedexNumber, PokemonId", "Pokemon, Pokemon.Game, PrimaryType, SecondaryType").Where(x => pokemonList.Any(y => y.Id == x.PokemonId)).OrderBy(x => x.GenerationId).ThenBy(x => x.PokemonId).ToList();
                        pokemonTypeDetails = pokemonTypeDetails.GroupBy(x => new { x.PokemonId }).Select(x => x.LastOrDefault()).ToList();
                        pokemonTypeDetails = pokemonTypeDetails.Where(x => types.Exists(y => x.PrimaryTypeId == y) || types.Exists(y => x.SecondaryTypeId == y)).ToList();
                    }

                    pokemonList = pokemonList.Where(x => pokemonTypeDetails.Select(x => x.Pokemon).Any(y => y.Id == x.Id)).ToList();
                }

                return pokemonList;
            }
            catch (Exception e)
            {
                if (!this.User.IsInRole("Owner"))
                {
                    string commentBody;
                    List<DataAccess.Models.Type> allTypes = this.dataService.GetObjects<DataAccess.Models.Type>();

                    if (e != null)
                    {
                        commentBody = string.Concat(e.GetType().ToString(), " error during team generation. More specifically while filtering types.");
                    }
                    else
                    {
                        commentBody = "Unknown error during team generation. More specifically while filtering types.";
                    }

                    commentBody = string.Concat(commentBody, " - Selected Types: {", string.Join(", ", allTypes.Where(x => types.Any(y => y == x.Id)).Select(x => x.Name)), "}");
                    if (game.Id != 0)
                    {
                        commentBody = string.Concat(commentBody, " - Selected Game: ", game.Name);
                    }
                    else
                    {
                        commentBody = string.Concat(commentBody, " - No Selected Game");
                    }

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

                return new List<Pokemon>();
            }
        }

        private List<PokemonTypeDetail> GetAllPokemonWithSpecificTypes(int primaryTypeId, int secondaryTypeId, Game game, int regionalDexId)
        {
            List<PokemonTypeDetail> pokemonList = this.dataService.GetObjects<PokemonTypeDetail>("GenerationId", "Pokemon, Pokemon.Game, Pokemon.Form, PrimaryType, SecondaryType").Where(x => x.GenerationId <= game.GenerationId).GroupBy(x => new { x.PokemonId }).Select(x => x.LastOrDefault()).ToList();
            if (regionalDexId != 0)
            {
                List<RegionalDexEntry> regionalDexEntries = this.dataService.GetObjects<RegionalDexEntry>(whereProperty: "RegionalDexId", wherePropertyValue: regionalDexId);
                pokemonList = pokemonList.Where(x => regionalDexEntries.Any(y => y.PokemonId == x.PokemonId)).ToList();
            }

            if (game.Id != 0)
            {
                pokemonList = pokemonList.Where(x => x.GenerationId <= game.GenerationId).ToList();
            }

            if (secondaryTypeId != 0 && secondaryTypeId != 100)
            {
                pokemonList = pokemonList.Where(x => (x.PrimaryTypeId == primaryTypeId && x.SecondaryTypeId == secondaryTypeId) || (x.PrimaryTypeId == secondaryTypeId && x.SecondaryTypeId == primaryTypeId)).ToList();
            }
            else if (secondaryTypeId == 100)
            {
                pokemonList = pokemonList.Where(x => x.PrimaryTypeId == primaryTypeId || x.SecondaryTypeId == primaryTypeId).ToList();
            }
            else
            {
                pokemonList = pokemonList.Where(x => x.PrimaryTypeId == primaryTypeId && x.SecondaryType == null).ToList();
            }

            if (game.Id != 0)
            {
                List<int> exclusionList = pokemonList.Select(x => x.PokemonId).Except(this.dataService.GetObjects<PokemonGameDetail>(includes: "Pokemon, Game", whereProperty: "GameId", wherePropertyValue: game.Id).Select(x => x.PokemonId)).ToList();

                foreach (var pokemon in exclusionList)
                {
                    pokemonList.Remove(pokemonList.Find(x => x.PokemonId == pokemon));
                }
            }

            return pokemonList.OrderBy(x => x.Pokemon.PokedexNumber).ToList();
        }

        private List<PokemonTypeDetail> GetAllPokemonWithSpecificType(List<int> types, int generationId, List<Pokemon> allPokemon)
        {
            try
            {
                List<PokemonTypeDetail> pokemonList = this.dataService.GetObjects<PokemonTypeDetail>(includes: "Pokemon, Pokemon.Game, PrimaryType, SecondaryType").Where(x => allPokemon.Any(y => y.Id == x.PokemonId)).OrderBy(x => x.GenerationId).ToList();

                if (generationId != 0)
                {
                    pokemonList = pokemonList.Where(x => x.GenerationId <= generationId).ToList();

                    List<int> exclusionList = pokemonList.Select(x => x.PokemonId).Except(this.dataService.GetObjects<PokemonGameDetail>(includes: "Pokemon, Game, Game.Generation", whereProperty: "Game.GenerationId", wherePropertyValue: generationId).Select(x => x.PokemonId)).ToList();

                    foreach (var pokemon in exclusionList)
                    {
                        pokemonList.Remove(pokemonList.Find(x => x.PokemonId == pokemon));
                    }
                }

                pokemonList = pokemonList.GroupBy(x => new { x.PokemonId }).Select(x => x.LastOrDefault()).Where(x => types.Exists(y => x.PrimaryTypeId == y) || types.Exists(y => x.SecondaryTypeId == y)).ToList();

                return pokemonList.OrderBy(x => x.Pokemon.PokedexNumber).ToList();
            }
            catch (Exception e)
            {
                if (!this.User.IsInRole("Owner") && e != null)
                {
                    string commentBody;
                    List<DataAccess.Models.Type> allTypes = this.dataService.GetObjects<DataAccess.Models.Type>();

                    if (e != null)
                    {
                        commentBody = string.Concat(e.GetType().ToString(), " error during retrieval of pokemon's specific type.");
                    }
                    else
                    {
                        commentBody = "Unknown error during retrieval of pokemon's specific type.";
                    }

                    commentBody = string.Concat(commentBody, " - Selected Types: {", string.Join(", ", allTypes.Where(x => types.Any(y => y == x.Id)).Select(x => x.Name)), "}");
                    commentBody = string.Concat(commentBody, " - Generation Id: ", generationId);
                    commentBody = string.Concat(commentBody, " - Pokemon List: ", allPokemon.ToString());
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
            }

            return null;
        }

        private ExportPokemonViewModel ExportTeamString(PokemonTeam team)
        {
            List<PokemonTeamDetail> pokemonList = team.GrabPokemonTeamDetails;
            ExportPokemonViewModel pokemonTeam = new ExportPokemonViewModel
            {
                ExportString = string.Empty,
                TeamId = team.Id,
            };

            if (pokemonList.Count > 0)
            {
                pokemonTeam.ExportString = "=== ";

                if (team.Game != null)
                {
                    pokemonTeam.ExportString = string.Concat(pokemonTeam.ExportString, " [gen", team.Game.GenerationId, "] ");
                }

                pokemonTeam.ExportString = string.Concat(pokemonTeam.ExportString, team.PokemonTeamName, " ===\n\n");

                for (var i = 0; i < pokemonList.Count; i++)
                {
                    if (i != 0)
                    {
                        pokemonTeam.ExportString = string.Concat(pokemonTeam.ExportString, "\n\n");
                    }

                    pokemonTeam.ExportString = string.Concat(pokemonTeam.ExportString, this.FillUserPokemonTeam(pokemonList[i], team.GameId));
                }
            }

            return pokemonTeam;
        }

        private string FillUserPokemonTeam(PokemonTeamDetail pokemonTeamDetail, int? gameId)
        {
            try
            {
                int generationId = 0;
                if (gameId != null)
                {
                    generationId = this.dataService.GetObjectByPropertyValue<Game>("Id", gameId).GenerationId;
                }

                Pokemon pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonTeamDetail.PokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth");
                List<string> pokemonForm = new List<string>();
                string pokemonName = string.Empty;

                if (!string.IsNullOrEmpty(pokemonTeamDetail.Nickname))
                {
                    pokemonName = string.Concat(pokemonTeamDetail.Nickname, " (");
                }

                if (pokemon.Name.Contains(" (Male)"))
                {
                    pokemon.Name = pokemon.Name.Replace(" (Male)", "-M");
                }
                else if (pokemon.Name.Contains(" (Female)"))
                {
                    pokemon.Name = pokemon.Name.Replace(" (Female)", "-F");
                }

                pokemonName = string.Concat(pokemonName, pokemon.Name);
                if ((pokemon.Id != 1760 && pokemon.IsAltForm) || pokemon.Id == 1692)
                {
                    pokemonForm = this.GetUserFormDetails(pokemon.Id);
                    pokemonName = string.Concat(pokemonName, "-", (pokemonForm[0] == "Female") ? "F" : pokemonForm[0]);
                }

                if (!string.IsNullOrEmpty(pokemonTeamDetail.Nickname))
                {
                    pokemonName = string.Concat(pokemonName, ")");
                }

                if (!string.IsNullOrEmpty(pokemonTeamDetail.Gender) && generationId != 1)
                {
                    pokemonName = string.Concat(pokemonName, " (", pokemonTeamDetail.Gender.Substring(0, 1), ")");
                }

                if (pokemonForm.Count == 2)
                {
                    pokemonName = string.Concat(pokemonName, pokemonForm[1]);
                }
                else if (!string.IsNullOrEmpty(pokemonTeamDetail.BattleItemString) && generationId != 1)
                {
                    pokemonName = string.Concat(pokemonName, " @ ", pokemonTeamDetail.BattleItemString);
                }

                string pokemonTeamString = pokemonName;
                if (generationId != 1 && generationId != 2 && gameId != 37)
                {
                    if (pokemonTeamDetail.Ability != null)
                    {
                        pokemonTeamString = string.Concat(pokemonTeamString, "\nAbility: ", pokemonTeamDetail.Ability.Name);
                    }
                }

                if (pokemonTeamDetail.Level < 100)
                {
                    pokemonTeamString = string.Concat(pokemonTeamString, "\nLevel: ", pokemonTeamDetail.Level.ToString());
                }

                if (pokemonTeamDetail.IsShiny && generationId != 1)
                {
                    pokemonTeamString = string.Concat(pokemonTeamString, "\nShiny: Yes");
                }

                if ((gameId == 41 || gameId == 42) && pokemonTeamDetail.TeraType != null)
                {
                    pokemonTeamString = string.Concat(pokemonTeamString, "\nTera Type: ", pokemonTeamDetail.TeraType.Name);
                }

                if (generationId != 1 && pokemonTeamDetail.PokemonTeamMoveset.CheckMove("Frustration"))
                {
                    pokemonTeamString = string.Concat(pokemonTeamString, "\nHappiness: 0");
                }

                pokemonTeamString = string.Concat(pokemonTeamString, this.FillEVs(pokemonTeamDetail.PokemonTeamEV));

                if (pokemonTeamDetail.Nature != null && generationId != 1 && generationId != 2)
                {
                    pokemonTeamString = string.Concat(pokemonTeamString, "\n", pokemonTeamDetail.Nature.Name, " Nature");
                }

                pokemonTeamString = string.Concat(pokemonTeamString, this.FillIVs(pokemonTeamDetail.PokemonTeamIV), this.FillMoveset(pokemonTeamDetail.PokemonTeamMoveset));

                return pokemonTeamString;
            }
            catch (Exception e)
            {
                if (!this.User.IsInRole("Owner") && e != null)
                {
                    string commentBody;
                    if (e != null)
                    {
                        commentBody = string.Concat(e.GetType().ToString(), " error during filling the user's pokemon team.");
                    }
                    else
                    {
                        commentBody = "Unknown error during filling the user's pokemon team.";
                    }

                    commentBody = string.Concat(commentBody, " - Pokemon Team Detail Id: ", pokemonTeamDetail.Id);
                    commentBody = string.Concat(commentBody, " - Game Id: ", gameId.HasValue ? gameId.Value.ToString() : "Null");
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
            }

            return null;
        }

        private List<string> GetUserFormDetails(int pokemonId)
        {
            string form = string.Empty, itemName = string.Empty;
            List<string> formDetails = new List<string>();
            if (pokemonId == 1692)
            {
                form = "Four";
            }
            else
            {
                Pokemon pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonId, "Form");
                form = string.Concat(pokemon.Form.Name.Replace(' ', '-').Replace("Gigantamax", "Gmax"));
            }

            List<FormItem> formItems = this.dataService.GetObjects<FormItem>();
            FormItem formItem = formItems.Find(x => x.PokemonId == pokemonId);
            if (formItem != null)
            {
                itemName = string.Concat(" @ ", formItem.Name);
            }

            formDetails.Add(form);
            if (!string.IsNullOrEmpty(itemName))
            {
                formDetails.Add(itemName);
            }

            return formDetails;
        }
    }
}

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

namespace Pokedex.Controllers
{
    /// <summary>
    /// The class that handles all PokemonTeam related requests.
    /// </summary>
    [Authorize]
    [Route("")]
    public class PokemonTeamController : Controller
    {
        private readonly DataService dataService;

        private readonly AppConfig appConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="PokemonTeamController"/> class.
        /// </summary>
        /// <param name="appConfig">The application's configuration.</param>
        /// <param name="dataContext">The pokemon database context.</param>
        public PokemonTeamController(IOptions<AppConfig> appConfig, DataContext dataContext)
        {
            // Instantiate an instance of the data service.
            this.appConfig = appConfig.Value;
            this.dataService = new DataService(dataContext);
        }

        /// <summary>
        /// Creates a pokemon team.
        /// </summary>
        /// <returns>The create pokemon team page.</returns>
        [HttpGet]
        [Route("create_team")]
        public IActionResult CreateTeam()
        {
            CreatePokemonTeamViewModel model = new CreatePokemonTeamViewModel()
            {
                AllGames = this.dataService.GetObjects<Game>("ReleaseDate, Id").Where(x => x.ReleaseDate <= DateTime.Now).ToList(),
                UserId = this.dataService.GetCurrentUser(this.User).Id,
            };

            model.AllGames.Remove(model.AllGames.Find(x => x.Id == 43));

            return this.View(model);
        }

        /// <summary>
        /// Creates a pokemon team.
        /// </summary>
        /// <param name="pokemonTeam">The created pokemon team.</param>
        /// <returns>The user's pokemon team page.</returns>
        [HttpPost]
        [Route("create_team")]
        public IActionResult CreateTeam(CreatePokemonTeamViewModel pokemonTeam)
        {
            if (!this.ModelState.IsValid)
            {
                CreatePokemonTeamViewModel model = new CreatePokemonTeamViewModel()
                {
                    AllGames = this.dataService.GetObjects<Game>("ReleaseDate, Id").Where(x => x.ReleaseDate <= DateTime.Now).ToList(),
                    UserId = this.dataService.GetCurrentUser(this.User).Id,
                };

                return this.View(model);
            }

            this.dataService.AddObject(pokemonTeam);

            return this.RedirectToAction("PokemonTeams", "User");
        }

        /// <summary>
        /// Imports pokemon teams from Pokemon Showdown.
        /// </summary>
        /// <returns>The import pokemon teams page.</returns>
        [HttpGet]
        [Route("import_teams")]
        public IActionResult ImportTeams()
        {
            return this.View();
        }

        /// <summary>
        /// Imports pokemon teams from Pokemon Showdown.
        /// </summary>
        /// <param name="importedTeams">The Pokemon Showdown teama export string.</param>
        /// <returns>The user's pokemon team page.</returns>
        [HttpPost]
        [Route("import_teams")]
        public IActionResult ImportTeams(string importedTeams)
        {
            try
            {
                int userId = this.dataService.GetCurrentUser(this.User).Id;
                if (!this.ModelState.IsValid || string.IsNullOrEmpty(importedTeams))
                {
                    return this.View();
                }

                List<string> pokemonTeams = importedTeams.TrimStart().Split("\r\n===").ToList();
                for (var i = 1; i < pokemonTeams.Count; i++)
                {
                    pokemonTeams[i] = string.Concat("===", pokemonTeams[i]);
                }

                foreach (var p in pokemonTeams)
                {
                    this.CreateTeamFromImport(p, userId);
                }
            }
            catch (Exception e)
            {
                if (!this.User.IsInRole("Owner") && e != null)
                {
                    string commentBody;
                    if (e != null)
                    {
                        commentBody = string.Concat(e.GetType().ToString(), " error during the pokemon team's import.");
                    }
                    else
                    {
                        commentBody = "Unknown error during the pokemon team's import.";
                    }

                    commentBody = string.Concat(commentBody, " - Imported Team Text: {", string.Join(", ", importedTeams), "}");
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

            return this.RedirectToAction("PokemonTeams", "User");
        }

        /// <summary>
        /// Creates a pokemon for a user's pokemon team.
        /// </summary>
        /// <param name="pokemonTeamId">The pokemon team id.</param>
        /// <returns>The create pokemon for team page.</returns>
        [HttpGet]
        [Route("create_pokemon/{pokemonTeamId:int}")]
        public IActionResult CreatePokemon(int pokemonTeamId)
        {
            List<PokemonTeam> pokemonTeams = this.dataService.GetPokemonTeams(this.User.Identity.Name);
            if (pokemonTeams.Count < pokemonTeamId || pokemonTeams[pokemonTeamId - 1].SixthPokemonId != null)
            {
                return this.RedirectToAction("PokemonTeams", "User");
            }
            else
            {
                PokemonTeam pokemonTeam = pokemonTeams[pokemonTeamId - 1];
                List<Nature> natures = this.dataService.GetObjects<Nature>("Name", "RaisedStat, LoweredStat");
                foreach (var n in natures)
                {
                    if (n.RaisedStat != null && n.LoweredStat != null)
                    {
                        n.Name = string.Concat(n.Name, " (+", n.RaisedStat.Abbreviation, ", -", n.LoweredStat.Abbreviation, ")");
                    }
                }

                CreateTeamPokemonViewModel model = new CreateTeamPokemonViewModel()
                {
                    AllPokemon = this.FillPokemonList(pokemonTeam),
                    AllNatures = natures,
                    NatureId = this.dataService.GetObjectByPropertyValue<Nature>("Name", "Serious").Id,
                    AllTypes = this.dataService.GetObjects<DataAccess.Models.Type>("Name"),
                    GameId = pokemonTeam.GameId,
                    Level = 100,
                };

                return this.View(model);
            }
        }

        /// <summary>
        /// Creates a pokemon for a user's pokemon team.
        /// </summary>
        /// <param name="pokemonTeamDetail">The created pokemon.</param>
        /// <param name="pokemonTeamId">The pokemon team id.</param>
        /// <returns>The user's pokemon team page.</returns>
        [HttpPost]
        [Route("create_pokemon/{pokemonTeamId:int}")]
        public IActionResult CreatePokemon(CreateTeamPokemonViewModel pokemonTeamDetail, int pokemonTeamId)
        {
            if (!this.ModelState.IsValid)
            {
                List<Nature> natures = this.dataService.GetObjects<Nature>("Name", "RaisedStat, LoweredStat");
                foreach (var n in natures)
                {
                    if (n.RaisedStat != null && n.LoweredStat != null)
                    {
                        n.Name = string.Concat(n.Name, " (+", n.RaisedStat.Abbreviation, ", -", n.LoweredStat.Abbreviation, ")");
                    }
                }

                CreateTeamPokemonViewModel model = new CreateTeamPokemonViewModel()
                {
                    AllPokemon = this.FillPokemonList(this.GetPokemonTeamByIndex(pokemonTeamId)),
                    AllNatures = natures,
                    NatureId = this.dataService.GetObjectByPropertyValue<Nature>("Name", "Serious").Id,
                    AllTypes = this.dataService.GetObjects<DataAccess.Models.Type>("Name"),
                    GameId = pokemonTeamDetail.GameId,
                    Level = 100,
                };

                return this.View(model);
            }

            PokemonTeam pokemonTeam = this.GetPokemonTeamByIndex(pokemonTeamId);

            Pokemon pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonTeamDetail.PokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth");

            if (pokemon.GenderRatioId == 10 || (pokemonTeamDetail.Gender != "Male" && pokemonTeamDetail.Gender != "Female"))
            {
                pokemonTeamDetail.Gender = null;
            }

            int pokemonTeamDetailId = this.dataService.AddPokemonTeamDetail(pokemonTeamDetail);

            pokemonTeam.InsertPokemon(this.dataService.GetObjectByPropertyValue<PokemonTeamDetail>("Id", pokemonTeamDetailId));

            this.dataService.UpdateObject(pokemonTeam);

            return this.RedirectToAction("PokemonTeams", "User");
        }

        /// <summary>
        /// Bulk deletes teams from a user's pokemon team list.
        /// </summary>
        /// <returns>The Delete Teams page.</returns>
        [Route("delete_teams")]
        public IActionResult DeleteTeams()
        {
            List<PokemonTeam> pokemonTeams = this.dataService.GetPokemonTeams(this.User.Identity.Name);
            PokemonTeamsViewModel model = new PokemonTeamsViewModel()
            {
                AllPokemonTeams = pokemonTeams,
                AllGames = pokemonTeams.Where(x => x.Game != null).ToList().ConvertAll(x => x.Game).OrderBy(x => x.ReleaseDate).ThenBy(x => x.Id).DistinctBy(x => x.Id).ToList(),
                AppConfig = this.appConfig,
            };

            return this.View(model);
        }

        /// <summary>
        /// Edits a pokemon in a user's pokemon team.
        /// </summary>
        /// <param name="pokemonTeamId">The pokemon team id.</param>
        /// <param name="pokemonTeamDetailId">The team's pokemon id.</param>
        /// <returns>The edit pokemon in team page.</returns>
        [HttpGet]
        [Route("update_pokemon/{pokemonTeamId:int}/{pokemonTeamDetailId:int}")]
        public IActionResult EditPokemon(int pokemonTeamId, int pokemonTeamDetailId)
        {
            List<PokemonTeam> pokemonTeams = this.dataService.GetPokemonTeams(this.User.Identity.Name);
            if (pokemonTeams.Count < pokemonTeamId)
            {
                return this.RedirectToAction("PokemonTeams", "User");
            }
            else
            {
                PokemonTeam pokemonTeam = this.GetPokemonTeamByIndex(pokemonTeamId);
                PokemonTeamDetail pokemonTeamDetail = this.GetPokemonTeamDetail(pokemonTeam.GrabPokemonTeamDetailIds()[pokemonTeamDetailId - 1]);

                if (pokemonTeamDetail.Nature == null)
                {
                    pokemonTeamDetail.NatureId = this.dataService.GetObjectByPropertyValue<Nature>("Name", "Serious").Id;
                }

                List<Nature> natures = this.dataService.GetObjects<Nature>("Name", "RaisedStat, LoweredStat");
                foreach (var n in natures)
                {
                    if (n.RaisedStat != null && n.LoweredStat != null)
                    {
                        n.Name = string.Concat(n.Name, " (+", n.RaisedStat.Abbreviation, ", -", n.LoweredStat.Abbreviation, ")");
                    }
                }

                UpdateTeamPokemonViewModel model = new UpdateTeamPokemonViewModel()
                {
                    PokemonTeamDetail = pokemonTeamDetail,
                    AllPokemon = this.FillPokemonList(pokemonTeam),
                    AllNatures = natures,
                    AllTypes = this.dataService.GetObjects<DataAccess.Models.Type>("Name"),
                    AllAbilities = this.dataService.GetObjects<Ability>("Name"),
                    GameId = pokemonTeam.GameId,
                };

                return this.View(model);
            }
        }

        /// <summary>
        /// Edits a pokemon in a user's pokemon team.
        /// </summary>
        /// <param name="pokemonTeamDetail">The team's updated pokemon.</param>
        /// <returns>The user's pokemon team page.</returns>
        [HttpPost]
        [Route("update_pokemon/{pokemonTeamId:int}/{pokemonTeamDetailId:int}")]
        public IActionResult EditPokemon(PokemonTeamDetail pokemonTeamDetail)
        {
            PokemonTeam pokemonTeam = this.dataService.GetObjects<PokemonTeam>(includes: "FirstPokemon, SecondPokemon, ThirdPokemon, FourthPokemon, FifthPokemon, SixthPokemon, User")
                .Find(x => x.FirstPokemonId == pokemonTeamDetail.Id ||
                           x.SecondPokemonId == pokemonTeamDetail.Id ||
                           x.ThirdPokemonId == pokemonTeamDetail.Id ||
                           x.FourthPokemonId == pokemonTeamDetail.Id ||
                           x.FifthPokemonId == pokemonTeamDetail.Id ||
                           x.SixthPokemonId == pokemonTeamDetail.Id);

            if (!this.ModelState.IsValid)
            {
                List<Nature> natures = this.dataService.GetObjects<Nature>("Name", "RaisedStat, LoweredStat");
                foreach (var n in natures)
                {
                    if (n.RaisedStat != null && n.LoweredStat != null)
                    {
                        n.Name = string.Concat(n.Name, " (+", n.RaisedStat.Abbreviation, ", -", n.LoweredStat.Abbreviation, ")");
                    }
                }

                UpdateTeamPokemonViewModel model = new UpdateTeamPokemonViewModel()
                {
                    PokemonTeamDetail = pokemonTeamDetail,
                    AllPokemon = this.FillPokemonList(pokemonTeam),
                    AllNatures = natures,
                    AllTypes = this.dataService.GetObjects<DataAccess.Models.Type>("Name"),
                    AllAbilities = this.dataService.GetObjects<Ability>("Name"),
                    GameId = pokemonTeam.GameId,
                };

                return this.View(model);
            }

            Pokemon pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonTeamDetail.PokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth");

            if (pokemon.GenderRatioId == 10 || (pokemonTeamDetail.Gender != "Male" && pokemonTeamDetail.Gender != "Female"))
            {
                pokemonTeamDetail.Gender = null;
            }

            this.dataService.UpdateObject(pokemonTeamDetail);

            return this.RedirectToAction("PokemonTeams", "User");
        }

        /// <summary>
        /// Deletes the selected pokemon team.
        /// </summary>
        /// <param name="pokemonTeamId">The pokemon team id.</param>
        /// <returns>The delete pokemon team confirmation page.</returns>
        [HttpGet]
        [Route("delete_team/{pokemonTeamId:int}")]
        public IActionResult DeleteTeam(int pokemonTeamId)
        {
            List<PokemonTeam> pokemonTeams = this.dataService.GetPokemonTeams(this.User.Identity.Name);
            if (pokemonTeams.Count < pokemonTeamId)
            {
                return this.RedirectToAction("PokemonTeams", "User");
            }
            else
            {
                PokemonTeam pokemonTeam = this.GetPokemonTeamByIndex(pokemonTeamId);
                PokemonTeamViewModel model = new PokemonTeamViewModel()
                {
                    Id = pokemonTeam.Id,
                    PokemonTeamName = pokemonTeam.PokemonTeamName,
                    FirstPokemon = pokemonTeam.FirstPokemon,
                    SecondPokemon = pokemonTeam.SecondPokemon,
                    ThirdPokemon = pokemonTeam.ThirdPokemon,
                    FourthPokemon = pokemonTeam.FourthPokemon,
                    FifthPokemon = pokemonTeam.FifthPokemon,
                    SixthPokemon = pokemonTeam.SixthPokemon,
                    AppConfig = this.appConfig,
                };

                return this.View(model);
            }
        }

        /// <summary>
        /// Deletes the selected pokemon team.
        /// </summary>
        /// <param name="pokemonTeam">The pokemon team being deleted.</param>
        /// <returns>The user's pokemon teams page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_team/{pokemonTeamId:int}")]
        public IActionResult DeleteTeam(PokemonTeam pokemonTeam)
        {
            this.dataService.DeletePokemonTeam(pokemonTeam.Id, this.User, this.appConfig);

            return this.RedirectToAction("PokemonTeams", "User");
        }

        /// <summary>
        /// Edits the EVs of a pokemon.
        /// </summary>
        /// <param name="pokemonTeamId">The pokemon team's id.</param>
        /// <param name="pokemonTeamDetailId">The pokemon's id.</param>
        /// <returns>The update pokemon's EVs page.</returns>
        [HttpGet]
        [Route("update_ev/{pokemonTeamId:int}/{pokemonTeamDetailId:int}")]
        public IActionResult EditEV(int pokemonTeamId, int pokemonTeamDetailId)
        {
            List<PokemonTeam> pokemonTeams = this.dataService.GetPokemonTeams(this.User.Identity.Name);
            if (pokemonTeams.Count < pokemonTeamId)
            {
                return this.RedirectToAction("PokemonTeams", "User");
            }
            else
            {
                PokemonTeamEV pokemonEV = this.dataService.GetObjectByPropertyValue<PokemonTeamEV>("Id", (int)this.GetPokemonTeamByIndex(pokemonTeamId).GrabPokemonTeamDetails[pokemonTeamDetailId - 1].PokemonTeamEVId);
                PokemonTeamEVViewModel model = new PokemonTeamEVViewModel()
                {
                    Id = pokemonEV.Id,
                    Health = pokemonEV.Health,
                    Attack = pokemonEV.Attack,
                    Defense = pokemonEV.Defense,
                    SpecialAttack = pokemonEV.SpecialAttack,
                    SpecialDefense = pokemonEV.SpecialDefense,
                    Speed = pokemonEV.Speed,
                    PokemonId = pokemonEV.Id,
                };

                return this.View(model);
            }
        }

        /// <summary>
        /// Edits the EVs of a pokemon.
        /// </summary>
        /// <param name="pokemonTeamEV">The pokemon's EVs.</param>
        /// <returns>The user's pokemon team page.</returns>
        [HttpPost]
        [Route("update_ev/{pokemonTeamId:int}/{pokemonTeamDetailId:int}")]
        public IActionResult EditEV(PokemonTeamEVViewModel pokemonTeamEV)
        {
            if (!this.ModelState.IsValid)
            {
                PokemonTeamEV pokemon = this.dataService.GetObjectByPropertyValue<PokemonTeamEV>("Id", pokemonTeamEV.PokemonId);
                PokemonTeamEVViewModel model = new PokemonTeamEVViewModel()
                {
                    Id = pokemon.Id,
                    Health = pokemon.Health,
                    Attack = pokemon.Attack,
                    Defense = pokemon.Defense,
                    SpecialAttack = pokemon.SpecialAttack,
                    SpecialDefense = pokemon.SpecialDefense,
                    Speed = pokemon.Speed,
                    PokemonId = pokemonTeamEV.PokemonId,
                };

                return this.View(model);
            }
            else if (pokemonTeamEV.EVTotal > 510)
            {
                PokemonTeamEV pokemon = this.dataService.GetObjectByPropertyValue<PokemonTeamEV>("Id", pokemonTeamEV.PokemonId);
                PokemonTeamEVViewModel model = new PokemonTeamEVViewModel()
                {
                    Id = pokemon.Id,
                    Health = pokemon.Health,
                    Attack = pokemon.Attack,
                    Defense = pokemon.Defense,
                    SpecialAttack = pokemon.SpecialAttack,
                    SpecialDefense = pokemon.SpecialDefense,
                    Speed = pokemon.Speed,
                    PokemonId = pokemonTeamEV.PokemonId,
                };

                this.ModelState.AddModelError("EVTotal", "Total EVs max at 510.");
                return this.View(model);
            }

            this.dataService.UpdateObject(pokemonTeamEV);

            return this.RedirectToAction("PokemonTeams", "User");
        }

        /// <summary>
        /// Edits the moveset of a pokemon.
        /// </summary>
        /// <param name="pokemonTeamId">The pokemon team's id.</param>
        /// <param name="pokemonTeamDetailId">The pokemon's id.</param>
        /// <returns>The update pokemon's moveset page.</returns>
        [HttpGet]
        [Route("update_moveset/{pokemonTeamId:int}/{pokemonTeamDetailId:int}")]
        public IActionResult EditMoveset(int pokemonTeamId, int pokemonTeamDetailId)
        {
            List<PokemonTeam> pokemonTeams = this.dataService.GetPokemonTeams(this.User.Identity.Name);
            if (pokemonTeams.Count < pokemonTeamId)
            {
                return this.RedirectToAction("PokemonTeams", "User");
            }
            else
            {
                PokemonTeamMoveset pokemonMoveset = this.dataService.GetObjectByPropertyValue<PokemonTeamMoveset>("Id", (int)this.GetPokemonTeamByIndex(pokemonTeamId).GrabPokemonTeamDetails[pokemonTeamDetailId - 1].PokemonTeamMovesetId);
                PokemonTeamMovesetViewModel model = new PokemonTeamMovesetViewModel()
                {
                    Id = pokemonMoveset.Id,
                    FirstMove = pokemonMoveset.FirstMove,
                    SecondMove = pokemonMoveset.SecondMove,
                    ThirdMove = pokemonMoveset.ThirdMove,
                    FourthMove = pokemonMoveset.FourthMove,
                    PokemonId = pokemonMoveset.Id,
                };

                return this.View(model);
            }
        }

        /// <summary>
        /// Edits the moveset of a pokemon.
        /// </summary>
        /// <param name="pokemonTeamMoveset">The pokemon's moveset.</param>
        /// <returns>The user's pokemon team page.</returns>
        [HttpPost]
        [Route("update_moveset/{pokemonTeamId:int}/{pokemonTeamDetailId:int}")]
        public IActionResult EditMoveset(PokemonTeamMovesetViewModel pokemonTeamMoveset)
        {
            if (!this.ModelState.IsValid)
            {
                PokemonTeamMoveset pokemon = this.dataService.GetObjectByPropertyValue<PokemonTeamMoveset>("Id", pokemonTeamMoveset.PokemonId);
                PokemonTeamMovesetViewModel model = new PokemonTeamMovesetViewModel()
                {
                    Id = pokemon.Id,
                    FirstMove = pokemon.FirstMove,
                    SecondMove = pokemon.SecondMove,
                    ThirdMove = pokemon.ThirdMove,
                    FourthMove = pokemon.FourthMove,
                    PokemonId = pokemon.Id,
                };

                return this.View(model);
            }

            this.FormatMoveText(pokemonTeamMoveset);

            this.dataService.UpdateObject(pokemonTeamMoveset);

            return this.RedirectToAction("PokemonTeams", "User");
        }

        /// <summary>
        /// Edits the IVs of a pokemon.
        /// </summary>
        /// <param name="pokemonTeamId">The pokemon team's id.</param>
        /// <param name="pokemonTeamDetailId">The pokemon's id.</param>
        /// <returns>The update pokemon's IVs page.</returns>
        [HttpGet]
        [Route("update_iv/{pokemonTeamId:int}/{pokemonTeamDetailId:int}")]
        public IActionResult EditIV(int pokemonTeamId, int pokemonTeamDetailId)
        {
            List<PokemonTeam> pokemonTeams = this.dataService.GetPokemonTeams(this.User.Identity.Name);
            if (pokemonTeams.Count < pokemonTeamId)
            {
                return this.RedirectToAction("PokemonTeams", "User");
            }
            else
            {
                PokemonTeamIV pokemonIV = this.dataService.GetObjectByPropertyValue<PokemonTeamIV>("Id", (int)this.GetPokemonTeamByIndex(pokemonTeamId).GrabPokemonTeamDetails[pokemonTeamDetailId - 1].PokemonTeamIVId);
                PokemonTeamIVViewModel model = new PokemonTeamIVViewModel()
                {
                    Id = pokemonIV.Id,
                    Health = pokemonIV.Health,
                    Attack = pokemonIV.Attack,
                    Defense = pokemonIV.Defense,
                    SpecialAttack = pokemonIV.SpecialAttack,
                    SpecialDefense = pokemonIV.SpecialDefense,
                    Speed = pokemonIV.Speed,
                    PokemonId = pokemonIV.Id,
                };

                return this.View(model);
            }
        }

        /// <summary>
        /// Edits the IVs of a pokemon.
        /// </summary>
        /// <param name="pokemonTeamIV">The pokemon's IVs.</param>
        /// <returns>The user's pokemon team page.</returns>
        [HttpPost]
        [Route("update_iv/{pokemonTeamId:int}/{pokemonTeamDetailId:int}")]
        public IActionResult EditIV(PokemonTeamIVViewModel pokemonTeamIV)
        {
            if (!this.ModelState.IsValid)
            {
                PokemonTeamEV pokemon = this.dataService.GetObjectByPropertyValue<PokemonTeamEV>("Id", pokemonTeamIV.PokemonId);
                PokemonTeamIVViewModel model = new PokemonTeamIVViewModel()
                {
                    Id = pokemon.Id,
                    Health = pokemon.Health,
                    Attack = pokemon.Attack,
                    Defense = pokemon.Defense,
                    SpecialAttack = pokemon.SpecialAttack,
                    SpecialDefense = pokemon.SpecialDefense,
                    Speed = pokemon.Speed,
                    PokemonId = pokemonTeamIV.PokemonId,
                };

                return this.View(model);
            }

            this.dataService.UpdateObject(pokemonTeamIV);

            return this.RedirectToAction("PokemonTeams", "User");
        }

        /// <summary>
        /// Deletes a pokemon from a pokemon team.
        /// </summary>
        /// <param name="pokemonTeamId">The id of the pokemon team being modified.</param>
        /// <param name="pokemonTeamDetailId">The id of the pokemon being deleted.</param>
        /// <returns>The delete pokemon team pokemon page.</returns>
        [HttpGet]
        [Route("delete_team_pokemon/{pokemonTeamId:int}/{pokemonTeamDetailId:int}")]
        public IActionResult DeletePokemon(int pokemonTeamId, int pokemonTeamDetailId)
        {
            List<PokemonTeam> pokemonTeams = this.dataService.GetPokemonTeams(this.User.Identity.Name);
            if (pokemonTeams.Count < pokemonTeamId)
            {
                return this.RedirectToAction("PokemonTeams", "User");
            }
            else
            {
                PokemonTeam pokemonTeam = this.GetPokemonTeamByIndex(pokemonTeamId);
                PokemonTeamDetail pokemonTeamDetail = this.GetPokemonTeamDetail(pokemonTeam.GrabPokemonTeamDetailIds()[pokemonTeamDetailId - 1]);
                PokemonTeamDetailViewModel model = new PokemonTeamDetailViewModel()
                {
                    Id = pokemonTeamDetail.Id,
                    Pokemon = pokemonTeamDetail.Pokemon,
                    AppConfig = this.appConfig,
                };

                return this.View(model);
            }
        }

        /// <summary>
        /// Deletes a pokemon from a pokemon team.
        /// </summary>
        /// <param name="pokemonTeamDetail">The pokemon being deleted.</param>
        /// <returns>The user's pokemon team page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_team_pokemon/{pokemonTeamId:int}/{pokemonTeamDetailId:int}")]
        public IActionResult DeletePokemon(PokemonTeamDetail pokemonTeamDetail)
        {
            this.DeletePokemonTeamDetail(pokemonTeamDetail.Id);

            return this.RedirectToAction("PokemonTeams", "User");
        }

        /// <summary>
        /// Edits the pokemon team name or selected game.
        /// </summary>
        /// <param name="pokemonTeamId">The pokemon team id.</param>
        /// <returns>The update pokemon team page.</returns>
        [HttpGet]
        [Route("update_team/{pokemonTeamId:int}")]
        public IActionResult EditTeam(int pokemonTeamId)
        {
            PokemonTeam pokemonTeam = this.GetPokemonTeamByIndex(pokemonTeamId);
            this.FillPokemonTeam(pokemonTeam);

            UpdatePokemonTeamViewModel model = new UpdatePokemonTeamViewModel()
            {
                Id = pokemonTeam.Id,
                PokemonTeamName = pokemonTeam.PokemonTeamName,
                GameId = pokemonTeam.GameId,
                UserId = pokemonTeam.UserId,
                AllGames = this.GetAvailableGames(pokemonTeam),
            };

            return this.View(model);
        }

        /// <summary>
        /// Edits the pokemon team name or selected game.
        /// </summary>
        /// <param name="newPokemonTeam">The updated pokemon team.</param>
        /// <returns>The user's pokemon team page.</returns>
        [HttpPost]
        [Route("update_team/{pokemonTeamId:int}")]
        public IActionResult EditTeam(PokemonTeam newPokemonTeam)
        {
            PokemonTeam originalPokemonTeam = this.dataService.GetObjectByPropertyValue<PokemonTeam>("Id", newPokemonTeam.Id);
            if (!this.ModelState.IsValid)
            {
                UpdatePokemonTeamViewModel model = new UpdatePokemonTeamViewModel()
                {
                    Id = originalPokemonTeam.Id,
                    PokemonTeamName = originalPokemonTeam.PokemonTeamName,
                    GameId = originalPokemonTeam.GameId,
                    UserId = originalPokemonTeam.UserId,
                    AllGames = this.GetAvailableGames(newPokemonTeam),
                };

                return this.View(model);
            }

            if (originalPokemonTeam.PokemonTeamName != newPokemonTeam.PokemonTeamName)
            {
                originalPokemonTeam.PokemonTeamName = newPokemonTeam.PokemonTeamName;
            }

            if (originalPokemonTeam.GameId != newPokemonTeam.GameId)
            {
                originalPokemonTeam.GameId = newPokemonTeam.GameId;
            }

            this.dataService.UpdateObject(originalPokemonTeam);

            return this.RedirectToAction("PokemonTeams", "User");
        }

        /// <summary>
        /// Creates a pokemon list from a selected pokemon team.
        /// </summary>
        /// <returns>The pokemon list.</returns>
        private List<Pokemon> FillPokemonList(PokemonTeam pokemonTeam)
        {
            if (pokemonTeam.GameId != null)
            {
                return this.dataService.GetPokemonWithFormNames((int)pokemonTeam.GameId);
            }
            else
            {
                return this.dataService.GetPokemonWithFormNames();
            }
        }

        /// <summary>
        /// Creates a pokemon team from a string in Pokemon Showndown's export format.
        /// </summary>
        /// <param name="importedTeam">The pokemon string in Pokemon Showdown's export format.</param>
        /// <param name="userId">The id of the user.</param>
        private void CreateTeamFromImport(string importedTeam, int userId)
        {
            string teamName = importedTeam.Split("===\r\n")[0];
            string pokemonString;
            if (teamName.IndexOf("===") != -1)
            {
                teamName = string.Concat(teamName, "===");
                int teamNameStart = teamName.IndexOf("] ");
                if (teamNameStart == -1)
                {
                    teamNameStart = 4;
                }
                else
                {
                    teamNameStart += 2;
                }

                int teamNameTo = teamName.LastIndexOf(" ===");
                teamName = teamName.Substring(teamNameStart, teamNameTo - teamNameStart);
                pokemonString = string.Concat("\r\n", importedTeam.Split("===\r\n")[1]);
            }
            else
            {
                teamName = "Import From Pokemon Showdown";
                pokemonString = importedTeam;
            }

            PokemonTeam pokemonTeam = new PokemonTeam()
            {
                PokemonTeamName = teamName,
                UserId = userId,
            };
            List<string> pokemonList = pokemonString.Split("\r\n\r\n").ToList();
            List<string> filteredPokemonList = new List<string>();
            foreach (var p in pokemonList)
            {
                if (!string.IsNullOrEmpty(p) && filteredPokemonList.Count < 6)
                {
                    filteredPokemonList.Add(p);
                }
            }

            // Used to ensure that there are no issues when creating the pokemon detail.
            List<PokemonTeamDetailViewModel> pokemonDetailList = new List<PokemonTeamDetailViewModel>();

            foreach (var p in filteredPokemonList)
            {
                pokemonDetailList.Add(this.CreatePokemonDetailFromImport(p));
            }

            foreach (var p in pokemonDetailList)
            {
                if (p.EVs != null)
                {
                    this.dataService.AddObject(p.EVs);
                    p.PokemonTeamEVId = p.EVs.Id;
                }

                if (p.IVs != null)
                {
                    this.dataService.AddObject(p.IVs);
                    p.PokemonTeamIVId = p.IVs.Id;
                }

                if (p.Moveset != null)
                {
                    this.dataService.AddObject(p.Moveset);
                    p.PokemonTeamMovesetId = p.Moveset.Id;
                }

                int pokemonId = this.dataService.AddPokemonTeamDetail(p);
                pokemonTeam.InsertPokemon(this.GetPokemonTeamDetail(pokemonId));
            }

            string generationId = importedTeam.Split("===\r\n")[0];
            this.FillPokemonTeam(pokemonTeam);
            List<Game> availableGames = this.GetAvailableGames(pokemonTeam);
            if (availableGames.Count() > 0)
            {
                if (generationId.IndexOf("[gen") != -1)
                {
                    int generationStart = generationId.IndexOf(" [");
                    int generationEnd = generationId.IndexOf("] ");
                    generationId = generationId.Substring(generationStart, generationEnd - generationStart);
                    int genId = Convert.ToInt32(System.Text.RegularExpressions.Regex.Replace(generationId, "[^0-9]+", string.Empty));
                    if (availableGames.Find(x => x.Id == genId) != null)
                    {
                        pokemonTeam.GameId = availableGames.Last(x => x.GenerationId == genId).Id;
                    }
                    else
                    {
                        pokemonTeam.GameId = availableGames.Last().Id;
                    }
                }
                else
                {
                    Game newestGame = availableGames.Last();
                    pokemonTeam.GameId = newestGame.Id;
                }
            }

            if (pokemonTeam.FirstPokemon != null)
            {
                pokemonTeam.FirstPokemon = null;
            }

            if (pokemonTeam.SecondPokemon != null)
            {
                pokemonTeam.SecondPokemon = null;
            }

            if (pokemonTeam.ThirdPokemon != null)
            {
                pokemonTeam.ThirdPokemon = null;
            }

            if (pokemonTeam.FourthPokemon != null)
            {
                pokemonTeam.FourthPokemon = null;
            }

            if (pokemonTeam.FifthPokemon != null)
            {
                pokemonTeam.FifthPokemon = null;
            }

            if (pokemonTeam.SixthPokemon != null)
            {
                pokemonTeam.SixthPokemon = null;
            }

            this.dataService.AddObject(pokemonTeam);
        }

        /// <summary>
        /// Creates a pokemon for the imported team.
        /// </summary>
        /// <param name="importedPokemon">The pokemon string from the Pokemon Showdown export string.</param>
        /// <returns>The created PokemonTeamDetail object.</returns>
        private PokemonTeamDetailViewModel CreatePokemonDetailFromImport(string importedPokemon)
        {
            try
            {
                PokemonTeamDetailViewModel pokemonTeamDetail = new PokemonTeamDetailViewModel();
                string pokemonName = importedPokemon.Split("\r\n")[0];
                string remainingImportedText = importedPokemon.Replace(string.Concat(pokemonName, "\r\n"), string.Empty);
                pokemonName = pokemonName.Trim();

                // Held item converter.
                if (pokemonName.IndexOf(" @ ") != -1)
                {
                    string itemName = pokemonName.Split(" @ ")[1];
                    pokemonTeamDetail.BattleItemString = itemName;

                    pokemonName = pokemonName.Split(string.Concat(" @ ", itemName))[0];
                }

                // Gender converter.
                int genderCheckStart = pokemonName.LastIndexOf('(');
                if (genderCheckStart != -1 && pokemonName.Substring(genderCheckStart + 2, 1) == ")")
                {
                    string genderInitial = pokemonName.Substring(genderCheckStart + 1, 1);
                    if (genderInitial == "M")
                    {
                        pokemonTeamDetail.Gender = "Male";
                    }
                    else if (genderInitial == "F")
                    {
                        pokemonTeamDetail.Gender = "Female";
                    }

                    pokemonName = pokemonName.Split(string.Concat(" (", genderInitial, ")"))[0];
                }

                // Nickname converter.
                if (pokemonName.IndexOf("(") != -1)
                {
                    pokemonTeamDetail.Nickname = pokemonName.Substring(0, pokemonName.LastIndexOf("(") - 1);
                    pokemonName = pokemonName.Replace(string.Concat(pokemonTeamDetail.Nickname, " ("), string.Empty);
                    pokemonName = pokemonName.Replace(")", string.Empty);
                }

                // Pokemon converter.
                Pokemon pokemon;

                // Converts Pokemon Showdown's apostrophe to the database's apostrophe.
                pokemonName = pokemonName.Replace('’', '\'');

                // Used to check for alternate form
                if ((pokemonName.LastIndexOf('-') != -1 && pokemonName != "Maushold-Four") || pokemonName == "Maushold")
                {
                    if (pokemonName.Contains("Gmax"))
                    {
                        pokemonName = pokemonName.Replace("-Gmax", "-Gigantamax");
                    }
                    else if (pokemonName == "Meowstic-F" || pokemonName == "Indeedee-F")
                    {
                        pokemonName = pokemonName.Replace("-F", "-Female");
                    }
                    else if (pokemonName == "Maushold")
                    {
                        pokemonName = string.Concat(pokemonName, "-Family-of-Three");
                    }

                    List<Form> forms = this.dataService.GetObjects<Form>("Name");
                    foreach (var f in forms)
                    {
                        f.Name = f.Name.Replace(' ', '-');
                    }

                    string formName = pokemonName.Remove(0, pokemonName.IndexOf('-') + 1);

                    Form form = forms.Find(x => x.Name == formName);

                    if (form != null)
                    {
                        pokemon = this.dataService.GetObjects<Pokemon>(whereProperty: "Name", wherePropertyValue: pokemonName.Replace(string.Concat("-", form.Name), string.Empty)).Find(x => x.FormId == form.Id);
                    }
                    else
                    {
                        pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Name", pokemonName);
                    }
                }
                else if (pokemonName == "Maushold-Four")
                {
                    pokemon = this.dataService.GetPokemon("Maushold");
                }
                else
                {
                    pokemon = this.dataService.GetPokemon(pokemonName);
                }

                pokemonTeamDetail.PokemonId = pokemon.Id;

                // Ability converter.
                Ability ability;
                if (remainingImportedText.Contains("Ability: ") && !remainingImportedText.Contains("Ability: No Ability"))
                {
                    string abilityName = remainingImportedText.Split("\r\n")[0];
                    remainingImportedText = remainingImportedText.Replace(string.Concat(abilityName, "\r\n"), string.Empty);
                    abilityName = abilityName.Split("Ability: ")[1].Trim();
                    ability = this.dataService.GetObjectByPropertyValue<Ability>("Name", abilityName);
                }
                else
                {
                    ability = this.dataService.GetAbilitiesForPokemon(pokemon.Id, this.dataService.GetObjects<Game>("ReleaseDate, Id").Where(x => x.ReleaseDate <= DateTime.Now).Last().Id, this.User, this.appConfig)[0];
                }

                pokemonTeamDetail.AbilityId = ability.Id;

                // Level converter.
                if (remainingImportedText.Contains("Level:"))
                {
                    string pokemonLevel = remainingImportedText.Split("\r\n")[0];
                    remainingImportedText = remainingImportedText.Replace(string.Concat(pokemonLevel, "\r\n"), string.Empty);
                    pokemonLevel = pokemonLevel.Trim();
                    pokemonTeamDetail.Level = Convert.ToByte(pokemonLevel.Substring(pokemonLevel.IndexOf(':') + 2, pokemonLevel.Length - (pokemonLevel.IndexOf(':') + 2)));
                    if (pokemonTeamDetail.Level == 0)
                    {
                        pokemonTeamDetail.Level = 1;
                    }
                    else if (pokemonTeamDetail.Level > 100 || string.Compare(pokemonTeamDetail.Level.ToString(), pokemonLevel.Substring(pokemonLevel.IndexOf(':') + 2)) != 0)
                    {
                        pokemonTeamDetail.Level = 100;
                    }
                }
                else
                {
                    pokemonTeamDetail.Level = 100;
                }

                // Shiny converter.
                if (remainingImportedText.Contains("Shiny: Yes"))
                {
                    remainingImportedText = remainingImportedText.Replace(string.Concat(remainingImportedText.Split("\r\n")[0], "\r\n"), string.Empty);
                    pokemonTeamDetail.IsShiny = true;
                }

                // Happiness converter.
                if (remainingImportedText.Contains("Tera Type:"))
                {
                    string teraType = remainingImportedText.Split("\r\n")[0];
                    remainingImportedText = remainingImportedText.Replace(string.Concat(teraType, "\r\n"), string.Empty);
                    teraType = teraType.Trim();
                    teraType = teraType.Substring(teraType.IndexOf(':') + 2, teraType.Length - (teraType.IndexOf(':') + 2));
                    pokemonTeamDetail.TeraTypeId = this.dataService.GetObjectByPropertyValue<DataAccess.Models.Type>("Name", teraType).Id;
                }

                // EV converter.
                if (remainingImportedText.Contains("EVs:"))
                {
                    string evs = remainingImportedText.Split("\r\n")[0];
                    remainingImportedText = remainingImportedText.Replace(string.Concat(evs, "\r\n"), string.Empty);
                    PokemonTeamEV pokemonEVs = new PokemonTeamEV();
                    if (evs.Contains("HP"))
                    {
                        string health = evs.Substring(evs.IndexOf("HP") - 4, 3);
                        health = health.Replace(":", string.Empty).Replace("/", string.Empty).Trim();
                        pokemonEVs.Health = Convert.ToByte(health);
                    }

                    if (evs.Contains("Atk"))
                    {
                        string attack = evs.Substring(evs.IndexOf("Atk") - 4, 3);
                        attack = attack.Replace(":", string.Empty).Replace("/", string.Empty).Trim();
                        pokemonEVs.Attack = Convert.ToByte(attack);
                    }

                    if (evs.Contains("Def"))
                    {
                        string defense = evs.Substring(evs.IndexOf("Def") - 4, 3);
                        defense = defense.Replace(":", string.Empty).Replace("/", string.Empty).Trim();
                        pokemonEVs.Defense = Convert.ToByte(defense);
                    }

                    if (evs.Contains("SpA"))
                    {
                        string specialAttack = evs.Substring(evs.IndexOf("SpA") - 4, 3);
                        specialAttack = specialAttack.Replace(":", string.Empty).Replace("/", string.Empty).Trim();
                        pokemonEVs.SpecialAttack = Convert.ToByte(specialAttack);
                    }

                    if (evs.Contains("SpD"))
                    {
                        string specialDefense = evs.Substring(evs.IndexOf("SpD") - 4, 3);
                        specialDefense = specialDefense.Replace(":", string.Empty).Replace("/", string.Empty).Trim();
                        pokemonEVs.SpecialDefense = Convert.ToByte(specialDefense);
                    }

                    if (evs.Contains("Spe"))
                    {
                        string speed = evs.Substring(evs.IndexOf("Spe") - 4, 3);
                        speed = speed.Replace(":", string.Empty).Replace("/", string.Empty).Trim();
                        pokemonEVs.Speed = Convert.ToByte(speed);
                    }

                    pokemonTeamDetail.EVs = pokemonEVs;
                }

                // Nature converter.
                if (remainingImportedText.Contains("Nature"))
                {
                    string natureName = remainingImportedText.Split("\r\n")[0];
                    remainingImportedText = remainingImportedText.Replace(string.Concat(natureName, "\r\n"), string.Empty);
                    natureName = natureName.Replace("Nature", string.Empty).Trim();
                    Nature nature = this.dataService.GetObjectByPropertyValue<Nature>("Name", natureName);
                    pokemonTeamDetail.NatureId = nature.Id;
                }
                else
                {
                    pokemonTeamDetail.NatureId = this.dataService.GetObjectByPropertyValue<Nature>("Name", "Serious").Id;
                }

                // IV converter.
                if (remainingImportedText.Contains("IVs:"))
                {
                    string ivs = remainingImportedText.Split("\r\n")[0];
                    remainingImportedText = remainingImportedText.Replace(string.Concat(ivs, "\r\n"), string.Empty);
                    PokemonTeamIV pokemonIVs = new PokemonTeamIV();
                    if (ivs.Contains("HP"))
                    {
                        string health = ivs.Substring(ivs.IndexOf("HP") - 3, 2).Trim();
                        pokemonIVs.Health = Convert.ToByte(health);
                    }

                    if (ivs.Contains("Atk"))
                    {
                        string health = ivs.Substring(ivs.IndexOf("Atk") - 3, 2).Trim();
                        pokemonIVs.Attack = Convert.ToByte(health);
                    }

                    if (ivs.Contains("Def"))
                    {
                        string health = ivs.Substring(ivs.IndexOf("Def") - 3, 2).Trim();
                        pokemonIVs.Defense = Convert.ToByte(health);
                    }

                    if (ivs.Contains("SpA"))
                    {
                        string health = ivs.Substring(ivs.IndexOf("SpA") - 3, 2).Trim();
                        pokemonIVs.SpecialAttack = Convert.ToByte(health);
                    }

                    if (ivs.Contains("SpD"))
                    {
                        string health = ivs.Substring(ivs.IndexOf("SpD") - 3, 2).Trim();
                        pokemonIVs.SpecialDefense = Convert.ToByte(health);
                    }

                    if (ivs.Contains("Spe"))
                    {
                        string health = ivs.Substring(ivs.IndexOf("Spe") - 3, 2).Trim();
                        pokemonIVs.Speed = Convert.ToByte(health);
                    }

                    pokemonTeamDetail.IVs = pokemonIVs;
                }

                // Moveset converter.
                if (remainingImportedText.Contains("- "))
                {
                    List<string> moves = remainingImportedText.Split("\r\n").ToList();
                    List<string> validMoves = new List<string>();
                    string move;
                    PokemonTeamMoveset moveset = new PokemonTeamMoveset();

                    foreach (var m in moves)
                    {
                        if (!string.IsNullOrEmpty(m) && validMoves.Count < 4)
                        {
                            validMoves.Add(m);
                        }
                    }

                    foreach (var m in validMoves)
                    {
                        move = m.Substring(2, m.Length - 2).Trim();
                        moveset.FourthMove = move;
                        moveset = this.SortMoveset(moveset);
                    }

                    pokemonTeamDetail.Moveset = moveset;
                }

                return pokemonTeamDetail;
            }
            catch (Exception e)
            {
                if (!this.User.IsInRole("Owner") && e != null)
                {
                    string commentBody;
                    if (e != null)
                    {
                        commentBody = string.Concat(e.GetType().ToString(), " error during team.");
                    }
                    else
                    {
                        commentBody = "Unknown error during team import.";
                    }

                    Comment comment = new Comment()
                    {
                        Name = string.Concat(commentBody, " - Imported Text: ", importedPokemon),
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

        private void FillPokemonTeam(PokemonTeam pokemonTeam)
        {
            if (pokemonTeam.FirstPokemonId != null)
            {
                pokemonTeam.FirstPokemon = this.GetPokemonTeamDetail((int)pokemonTeam.FirstPokemonId);
            }

            if (pokemonTeam.SecondPokemonId != null)
            {
                pokemonTeam.SecondPokemon = this.GetPokemonTeamDetail((int)pokemonTeam.SecondPokemonId);
            }

            if (pokemonTeam.ThirdPokemonId != null)
            {
                pokemonTeam.ThirdPokemon = this.GetPokemonTeamDetail((int)pokemonTeam.ThirdPokemonId);
            }

            if (pokemonTeam.FourthPokemonId != null)
            {
                pokemonTeam.FourthPokemon = this.GetPokemonTeamDetail((int)pokemonTeam.FourthPokemonId);
            }

            if (pokemonTeam.FifthPokemonId != null)
            {
                pokemonTeam.FifthPokemon = this.GetPokemonTeamDetail((int)pokemonTeam.FifthPokemonId);
            }

            if (pokemonTeam.SixthPokemonId != null)
            {
                pokemonTeam.SixthPokemon = this.GetPokemonTeamDetail((int)pokemonTeam.SixthPokemonId);
            }
        }

        /// <summary>
        /// Deletes all of the details for a pokemon team detail.
        /// </summary>
        /// <param name="id">The id of the pokemon team detail variable.</param>
        private void DeletePokemonTeamDetail(int id)
        {
            PokemonTeamDetail pokemonTeamDetail = this.dataService.GetObjectByPropertyValue<PokemonTeamDetail>("Id", id);
            PokemonTeam pokemonTeam = this.dataService.GetObjects<PokemonTeam>()
                .Find(x => x.FirstPokemonId == pokemonTeamDetail.Id ||
                           x.SecondPokemonId == pokemonTeamDetail.Id ||
                           x.ThirdPokemonId == pokemonTeamDetail.Id ||
                           x.FourthPokemonId == pokemonTeamDetail.Id ||
                           x.FifthPokemonId == pokemonTeamDetail.Id ||
                           x.SixthPokemonId == pokemonTeamDetail.Id);

            if (pokemonTeam != null)
            {
                this.RemovePokemonFromTeam(pokemonTeam, pokemonTeamDetail);
            }

            int? evId = pokemonTeamDetail.PokemonTeamEVId;
            int? ivId = pokemonTeamDetail.PokemonTeamIVId;
            int? movesetId = pokemonTeamDetail.PokemonTeamMovesetId;
            this.dataService.DeleteObject<PokemonTeamDetail>(pokemonTeamDetail.Id);

            if (evId != null)
            {
                this.dataService.DeleteObject<PokemonTeamEV>((int)evId);
            }

            if (ivId != null)
            {
                this.dataService.DeleteObject<PokemonTeamIV>((int)ivId);
            }

            if (movesetId != null)
            {
                this.dataService.DeleteObject<PokemonTeamMoveset>((int)movesetId);
            }
        }

        /// <summary>
        /// Removes a pokemon from the team and then shifts all of the pokemon up in the team to fill in any empty spaces.
        /// </summary>
        /// <param name="team">The team that the pokemon is being removed from are being shifted.</param>
        /// <param name="teamDetail">The pokemon being removed.</param>
        private void RemovePokemonFromTeam(PokemonTeam team, PokemonTeamDetail teamDetail)
        {
            if (team.FirstPokemonId == teamDetail.Id)
            {
                team.FirstPokemonId = null;
            }
            else if (team.SecondPokemonId == teamDetail.Id)
            {
                team.SecondPokemonId = null;
            }
            else if (team.ThirdPokemonId == teamDetail.Id)
            {
                team.ThirdPokemonId = null;
            }
            else if (team.FourthPokemonId == teamDetail.Id)
            {
                team.FourthPokemonId = null;
            }
            else if (team.FifthPokemonId == teamDetail.Id)
            {
                team.FifthPokemonId = null;
            }
            else if (team.SixthPokemonId == teamDetail.Id)
            {
                team.SixthPokemonId = null;
            }

            // Shifts the pokemon up in the team to fill empty space.
            if (team.FirstPokemonId == null && team.SecondPokemonId != null)
            {
                team.FirstPokemonId = team.SecondPokemonId;
                team.SecondPokemonId = null;
            }

            if (team.SecondPokemonId == null && team.ThirdPokemonId != null)
            {
                team.SecondPokemonId = team.ThirdPokemonId;
                team.ThirdPokemonId = null;
            }

            if (team.ThirdPokemonId == null && team.FourthPokemonId != null)
            {
                team.ThirdPokemonId = team.FourthPokemonId;
                team.FourthPokemonId = null;
            }

            if (team.FourthPokemonId == null && team.FifthPokemonId != null)
            {
                team.FourthPokemonId = team.FifthPokemonId;
                team.FifthPokemonId = null;
            }

            if (team.FifthPokemonId == null && team.SixthPokemonId != null)
            {
                team.FifthPokemonId = team.SixthPokemonId;
                team.SixthPokemonId = null;
            }

            this.dataService.UpdateObject(team);
        }

        /// <summary>
        /// Sorts the movesets to ensure no empty spaces.
        /// </summary>
        /// <param name="moveset">The moveset that is being sorted.</param>
        /// <returns>The sorted moveset.</returns>
        private PokemonTeamMoveset SortMoveset(PokemonTeamMoveset moveset)
        {
            // First Sort
            if (string.IsNullOrEmpty(moveset.FirstMove) && !string.IsNullOrEmpty(moveset.SecondMove))
            {
                moveset.FirstMove = moveset.SecondMove;
                moveset.SecondMove = null;
            }

            if (string.IsNullOrEmpty(moveset.SecondMove) && !string.IsNullOrEmpty(moveset.ThirdMove))
            {
                moveset.SecondMove = moveset.ThirdMove;
                moveset.ThirdMove = null;
            }

            if (string.IsNullOrEmpty(moveset.ThirdMove) && !string.IsNullOrEmpty(moveset.FourthMove))
            {
                moveset.ThirdMove = moveset.FourthMove;
                moveset.FourthMove = null;
            }

            // Second Sort
            if (string.IsNullOrEmpty(moveset.FirstMove) && !string.IsNullOrEmpty(moveset.SecondMove))
            {
                moveset.FirstMove = moveset.SecondMove;
                moveset.SecondMove = null;
            }

            if (string.IsNullOrEmpty(moveset.SecondMove) && !string.IsNullOrEmpty(moveset.ThirdMove))
            {
                moveset.SecondMove = moveset.ThirdMove;
                moveset.ThirdMove = null;
            }

            // Third Sort.
            if (string.IsNullOrEmpty(moveset.FirstMove) && !string.IsNullOrEmpty(moveset.SecondMove))
            {
                moveset.FirstMove = moveset.SecondMove;
                moveset.SecondMove = null;
            }

            return moveset;
        }

        private List<Game> GetAvailableGames(PokemonTeam pokemonTeam)
        {
            List<Game> availableGames = new List<Game>();
            if (pokemonTeam.FirstPokemonId != null)
            {
                availableGames = this.dataService.GetObjects<PokemonGameDetail>("Game.GenerationId, GameId, Id", "Pokemon, Pokemon.Game, Game", "PokemonId", pokemonTeam.FirstPokemon.PokemonId).Select(x => x.Game).Where(x => x.ReleaseDate <= DateTime.Now).ToList();
            }

            if (pokemonTeam.SecondPokemonId != null)
            {
                availableGames = availableGames.Where(x => this.dataService.GetObjects<PokemonGameDetail>("Game.GenerationId, GameId, Id", "Pokemon, Pokemon.Game, Game", "PokemonId", pokemonTeam.SecondPokemon.PokemonId).Select(y => y.Game).Where(r => r.ReleaseDate <= DateTime.Now).Any(z => z.Id == x.Id)).ToList();
            }

            if (pokemonTeam.ThirdPokemonId != null)
            {
                availableGames = availableGames.Where(x => this.dataService.GetObjects<PokemonGameDetail>("Game.GenerationId, GameId, Id", "Pokemon, Pokemon.Game, Game", "PokemonId", pokemonTeam.ThirdPokemon.PokemonId).Select(y => y.Game).Where(r => r.ReleaseDate <= DateTime.Now).Any(z => z.Id == x.Id)).ToList();
            }

            if (pokemonTeam.FourthPokemonId != null)
            {
                availableGames = availableGames.Where(x => this.dataService.GetObjects<PokemonGameDetail>("Game.GenerationId, GameId, Id", "Pokemon, Pokemon.Game, Game", "PokemonId", pokemonTeam.FourthPokemon.PokemonId).Select(y => y.Game).Where(r => r.ReleaseDate <= DateTime.Now).Any(z => z.Id == x.Id)).ToList();
            }

            if (pokemonTeam.FifthPokemonId != null)
            {
                availableGames = availableGames.Where(x => this.dataService.GetObjects<PokemonGameDetail>("Game.GenerationId, GameId, Id", "Pokemon, Pokemon.Game, Game", "PokemonId", pokemonTeam.FifthPokemon.PokemonId).Select(y => y.Game).Where(r => r.ReleaseDate <= DateTime.Now).Any(z => z.Id == x.Id)).ToList();
            }

            if (pokemonTeam.SixthPokemonId != null)
            {
                availableGames = availableGames.Where(x => this.dataService.GetObjects<PokemonGameDetail>("Game.GenerationId, GameId, Id", "Pokemon, Pokemon.Game, Game", "PokemonId", pokemonTeam.SixthPokemon.PokemonId).Select(y => y.Game).Where(r => r.ReleaseDate <= DateTime.Now).Any(z => z.Id == x.Id)).ToList();
            }

            if (availableGames.Count() <= 0)
            {
                availableGames = this.dataService.GetObjects<Game>("ReleaseDate, Id").Where(x => x.ReleaseDate <= DateTime.Now).ToList();
                availableGames.Remove(availableGames.Find(x => x.Id == 43));
            }

            return availableGames.OrderBy(x => x.ReleaseDate).ThenBy(x => x.Id).ToList();
        }

        private void FormatMoveText(PokemonTeamMovesetViewModel moveset)
        {
            TextInfo info = CultureInfo.CurrentCulture.TextInfo;
            if (!string.IsNullOrEmpty(moveset.FirstMove))
            {
                moveset.FirstMove = info.ToTitleCase(moveset.FirstMove);
            }

            if (!string.IsNullOrEmpty(moveset.SecondMove))
            {
                moveset.SecondMove = info.ToTitleCase(moveset.SecondMove);
            }

            if (!string.IsNullOrEmpty(moveset.ThirdMove))
            {
                moveset.ThirdMove = info.ToTitleCase(moveset.ThirdMove);
            }

            if (!string.IsNullOrEmpty(moveset.FourthMove))
            {
                moveset.FourthMove = info.ToTitleCase(moveset.FourthMove);
            }
        }

        private PokemonTeamDetail GetPokemonTeamDetail(int id)
        {
            return this.dataService.GetObjects<PokemonTeamDetail>(includes: "Pokemon, Pokemon.Game.Generation, Ability, PokemonTeamEV, PokemonTeamIV, PokemonTeamMoveset, Nature")
                .Find(x => x.Id == id);
        }

        private PokemonTeam GetPokemonTeamByIndex(int id)
        {
            return this.dataService.GetPokemonTeams(this.User.Identity.Name)[id - 1];
        }
    }
}

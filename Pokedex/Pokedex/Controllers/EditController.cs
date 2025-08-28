using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MoreLinq;
using Pokedex.DataAccess.Models;
using Pokedex.Models;
using System.Collections.Generic;
using System.Linq;

namespace Pokedex.Controllers
{
    /// <summary>
    /// The class that handles the editing of the database.
    /// </summary>
    [Authorize(Roles = "Owner")]
    [Route("admin")]
    public class EditController : Controller
    {
        private readonly DataService dataService;

        private readonly AppConfig appConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditController"/> class.
        /// </summary>
        /// <param name="appConfig">The configuration for the application.</param>
        /// <param name="dataContext">The data context.</param>
        public EditController(IOptions<AppConfig> appConfig, DataContext dataContext)
        {
            this.appConfig = appConfig.Value;
            this.dataService = new DataService(dataContext);
        }

        [Authorize(Roles = "Owner")]
        [HttpGet]
        [Route("edit_user/{id:int}")]
        public new IActionResult User(int id)
        {
            User model = this.dataService.GetObjectByPropertyValue<User>("Id", id);

            return this.View(model);
        }

        [Authorize(Roles = "Owner")]
        [HttpPost]
        [Route("edit_user/{id:int}")]
        public new IActionResult User(User user)
        {
            if (!this.ModelState.IsValid)
            {
                User model = this.dataService.GetObjectByPropertyValue<User>("Username", user.Username);

                return this.View(model);
            }

            if (user.IsOwner && !user.IsTester)
            {
                user.IsTester = true;
            }

            this.dataService.UpdateObject(user);

            return this.RedirectToAction("Users", "Owner");
        }

        [Route("edit_type_effectiveness/{id:int}/{genId:int}")]
        public IActionResult TypeEffectiveness(int id, int genId)
        {
            EditTypeChartViewModel model = new EditTypeChartViewModel()
            {
                TypeChart = this.dataService.GetTypeChartByDefendType(id, genId),
                Types = this.dataService.GetObjects<Type>("Name").Where(x => x.GenerationId <= genId).ToList(),
                TypeId = id,
                GenerationId = genId,
            };

            return this.View(model);
        }

        [Route("game_availability/{pokemonId:int}")]
        public IActionResult PokemonGameDetails(int pokemonId)
        {
            Pokemon pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form");
            if (pokemon.IsAltForm)
            {
                pokemon.Name = pokemon.NameWithForm;
            }

            PokemonGameViewModel model = new PokemonGameViewModel()
            {
                Pokemon = pokemon,
                PokemonGameDetails = this.dataService.GetObjects<PokemonGameDetail>("Game.GenerationId, GameId, Id", "Pokemon, Pokemon.Game, Game", "PokemonId", pokemonId),
                AllGames = this.dataService.GetObjects<Game>("ReleaseDate, Id").Where(x => x.ReleaseDate >= pokemon.Game.ReleaseDate).ToList(),
            };

            return this.View(model);
        }

        [Route("edit_hunting_method_game_availability/{huntingMethodId:int}")]
        public IActionResult HuntingMethodGameDetail(int huntingMethodId)
        {
            HuntingMethodGameViewModel model = new HuntingMethodGameViewModel()
            {
                HuntingMethod = this.dataService.GetObjectByPropertyValue<HuntingMethod>("Id", huntingMethodId),
                HuntingMethodGameDetails = this.dataService.GetObjects<HuntingMethodGameDetail>("Game.GenerationId, GameId, Id", "HuntingMethod, Game", "HuntingMethodId", huntingMethodId),
                AllGames = this.dataService.GetObjects<Game>("ReleaseDate, Id"),
            };

            return this.View(model);
        }

        /// <summary>
        /// Grabs all games to mark what games the specified form group is limited to.
        /// </summary>
        /// <param name="id">The form group's id.</param>
        /// <returns>The page to specify limited games list.</returns>
        [Route("edit_form_group_games/{id:int}")]
        public IActionResult FormGroupGameDetails(int id)
        {
            FormGroupGameDetailViewModel model = new FormGroupGameDetailViewModel()
            {
                FormGroup = this.dataService.GetObjectByPropertyValue<FormGroup>("Id", id),
                AllFormGroupGameDetails = this.dataService.GetObjects<FormGroupGameDetail>(whereProperty: "FormGroupId", wherePropertyValue: id),
                AllGames = this.dataService.GetObjects<Game>("ReleaseDate, Id"),
            };

            return this.View(model);
        }

        [HttpGet]
        [Route("edit_game/{id:int}")]
        public IActionResult Game(int id)
        {
            Game game = this.dataService.GetObjectByPropertyValue<Game>("Id", id);
            GameViewModel model = new GameViewModel()
            {
                Id = game.Id,
                Name = game.Name,
                ReleaseDate = game.ReleaseDate,
                GenerationId = game.GenerationId,
                IsBreedingPossible = game.IsBreedingPossible,
                GameColor = game.GameColor,
                AllGenerations = this.dataService.GetObjects<Generation>(),
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_game/{id:int}")]
        public IActionResult Game(Game game, List<int> regionIds)
        {
            if (!this.ModelState.IsValid)
            {
                Game oldGame = this.dataService.GetObjectByPropertyValue<Game>("Id", game.Id);
                GameViewModel model = new GameViewModel()
                {
                    Id = oldGame.Id,
                    Name = oldGame.Name,
                    ReleaseDate = oldGame.ReleaseDate,
                    GenerationId = oldGame.GenerationId,
                    GameColor = oldGame.GameColor,
                    AllGenerations = this.dataService.GetObjects<Generation>(),
                };

                return this.View(model);
            }

            this.dataService.UpdateObject(game);

            return this.RedirectToAction("Games", "Owner");
        }

        [Route("edit_game_availability/{id:int}")]
        public IActionResult GameAvailability(int id)
        {
            Game game = this.dataService.GetObjectByPropertyValue<Game>("Id", id);
            List<Pokemon> pokemonList = this.dataService.GetAllPokemon();
            pokemonList.Where(x => x.IsAltForm).ToList().ForEach(x => x.Name = x.NameWithForm);
            if (id != 43)
            {
                pokemonList = pokemonList.Where(x => x.Game.ReleaseDate <= game.ReleaseDate).ToList();
            }

            EditGameAvailabilityViewModel model = new EditGameAvailabilityViewModel()
            {
                Game = game,
                Games = this.dataService.GetObjects<Game>("ReleaseDate, Id"),
                PokemonList = pokemonList,
                GameAvailability = this.dataService.GetObjects<PokemonGameDetail>(includes: "Pokemon, Game"),
            };

            return this.View(model);
        }

        [Route("edit_regional_dex_entries/{id:int}")]
        public IActionResult RegionalDexEntry(int id)
        {
            RegionalDex regionalDex = this.dataService.GetObjectByPropertyValue<RegionalDex>("Id", id);
            List<Pokemon> availablePokemon = this.dataService.GetObjects<PokemonGameDetail>(includes: "Pokemon, Pokemon.Game, Pokemon.Game.Generation").Where(x => x.GameId == regionalDex.GameId).Select(x => x.Pokemon).ToList();
            availablePokemon = availablePokemon.DistinctBy(x => x.Name).ToList();

            EditRegionalDexEntriesViewModel model = new EditRegionalDexEntriesViewModel()
            {
                RegionalDex = regionalDex,
                PokemonList = availablePokemon,
                RegionalDexEntries = this.dataService.GetObjects<RegionalDexEntry>(includes: "Pokemon, RegionalDex"),
            };

            return this.View(model);
        }

        [Route("edit_mark_game_availability/{id:int}")]
        public IActionResult MarkGameAvailability(int id)
        {
            Game game = this.dataService.GetObjectByPropertyValue<Game>("Id", id);
            EditMarkGameViewModel model = new EditMarkGameViewModel()
            {
                Game = game,
                AllMarks = this.dataService.GetObjects<Mark>().Where(x => x.GenerationId <= game.GenerationId).ToList(),
                MarkGameDetails = this.dataService.GetObjects<MarkGameDetail>(includes: "Game, Mark", whereProperty: "GameId", wherePropertyValue: id),
            };

            return this.View(model);
        }

        [Route("edit_pokeball_game_availability/{id:int}")]
        public IActionResult PokeballGameAvailability(int id)
        {
            Game game = this.dataService.GetObjectByPropertyValue<Game>("Id", id);
            EditPokeballGameViewModel model = new EditPokeballGameViewModel()
            {
                Game = game,
                AllPokeballs = this.dataService.GetObjects<Pokeball>().Where(x => x.GenerationId <= game.GenerationId).ToList(),
                PokeballGameDetails = this.dataService.GetObjects<PokeballGameDetail>(includes: "Game, Pokeball", whereProperty: "GameId", wherePropertyValue: id),
            };

            return this.View(model);
        }

        [HttpGet]
        [Route("edit_pokemon/{id:int}")]
        public IActionResult Pokemon(int id)
        {
            BasePokemonViewModel model = new BasePokemonViewModel
            {
                Pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", id, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form"),
                AllGames = this.dataService.GetObjects<Game>("ReleaseDate, Id"),
                AllClassifications = this.dataService.GetObjects<Classification>("Name"),
                AllBaseHappinesses = this.dataService.GetObjects<BaseHappiness>("Happiness"),
                AllCaptureRates = this.dataService.GetObjects<CaptureRate>("CatchRate"),
                AllEggCycles = this.dataService.GetObjects<EggCycle>("CycleCount"),
                AllExperienceGrowths = this.dataService.GetObjects<ExperienceGrowth>("Name"),
                AllSpecialGroupings = this.dataService.GetObjects<SpecialGrouping>(),
                AllGenderRatios = new List<GenderRatioViewModel>(),
            };

            foreach (GenderRatio genderRatio in this.dataService.GetObjects<GenderRatio>())
            {
                GenderRatioViewModel viewModel = new GenderRatioViewModel()
                {
                    Id = genderRatio.Id,
                };

                if (genderRatio.MaleRatio == genderRatio.FemaleRatio && genderRatio.MaleRatio == 0)
                {
                    viewModel.GenderRatioString = "Gender Unknown";
                }
                else if (genderRatio.MaleRatio == genderRatio.FemaleRatio && genderRatio.MaleRatio == 100)
                {
                    viewModel.GenderRatioString = "Gender Not Revealed";
                }
                else if (genderRatio.FemaleRatio == 0)
                {
                    viewModel.GenderRatioString = string.Concat(genderRatio.MaleRatio, "% Male");
                }
                else if (genderRatio.MaleRatio == 0)
                {
                    viewModel.GenderRatioString = string.Concat(genderRatio.FemaleRatio, "% Female");
                }
                else
                {
                    viewModel.GenderRatioString = string.Concat(genderRatio.MaleRatio, "% Male / ", genderRatio.FemaleRatio, "% Female");
                }

                model.AllGenderRatios.Add(viewModel);
                model.AllGenderRatios.OrderBy(x => x.GenderRatioString).ToList();
            }

            if (model.Pokemon.IsAltForm)
            {
                model.Name = model.NameWithForm;
            }

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_pokemon/{id:int}")]
        public IActionResult Pokemon(Pokemon pokemon)
        {
            if (!this.ModelState.IsValid)
            {
                BasePokemonViewModel model = new BasePokemonViewModel()
                {
                    Pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemon.Id, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form"),
                    AllGames = this.dataService.GetObjects<Game>("ReleaseDate, Id"),
                };

                if (!pokemon.IsAltForm)
                {
                    model.AllBaseHappinesses = this.dataService.GetObjects<BaseHappiness>("Happiness");
                    model.AllClassifications = this.dataService.GetObjects<Classification>("Name");
                    model.AllCaptureRates = this.dataService.GetObjects<CaptureRate>("CatchRate");
                    model.AllEggCycles = this.dataService.GetObjects<EggCycle>("CycleCount");
                    model.AllExperienceGrowths = this.dataService.GetObjects<ExperienceGrowth>("Name");
                    model.AllGenderRatios = new List<GenderRatioViewModel>();
                    model.AllSpecialGroupings = this.dataService.GetObjects<SpecialGrouping>();

                    foreach (GenderRatio genderRatio in this.dataService.GetObjects<GenderRatio>())
                    {
                        GenderRatioViewModel viewModel = new GenderRatioViewModel()
                        {
                            Id = genderRatio.Id,
                        };

                        if (genderRatio.MaleRatio == genderRatio.FemaleRatio && genderRatio.MaleRatio == 0)
                        {
                            viewModel.GenderRatioString = "Gender Unknown";
                        }
                        else if (genderRatio.MaleRatio == genderRatio.FemaleRatio && genderRatio.MaleRatio == 100)
                        {
                            viewModel.GenderRatioString = "Gender Not Revealed";
                        }
                        else if (genderRatio.FemaleRatio == 0)
                        {
                            viewModel.GenderRatioString = string.Concat(genderRatio.MaleRatio, "% Male");
                        }
                        else if (genderRatio.MaleRatio == 0)
                        {
                            viewModel.GenderRatioString = string.Concat(genderRatio.FemaleRatio, "% Female");
                        }
                        else
                        {
                            viewModel.GenderRatioString = string.Concat(genderRatio.MaleRatio, "% Male / ", genderRatio.FemaleRatio, "% Female");
                        }

                        model.AllGenderRatios.Add(viewModel);
                        model.AllGenderRatios.OrderBy(x => x.GenderRatioString).ToList();
                    }
                }
                else
                {
                    model.Name = model.NameWithForm;
                }

                return this.View(model);
            }
            else if (this.dataService.GetObjectByPropertyValue<Pokemon>("PokedexNumber", pokemon.PokedexNumber) != null && this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemon.Id, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth").PokedexNumber != pokemon.PokedexNumber)
            {
                BasePokemonViewModel model = new BasePokemonViewModel()
                {
                    Pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemon.Id, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form"),
                    AllGames = this.dataService.GetObjects<Game>("ReleaseDate, Id"),
                };

                if (!pokemon.IsAltForm)
                {
                    model.AllBaseHappinesses = this.dataService.GetObjects<BaseHappiness>("Happiness");
                    model.AllClassifications = this.dataService.GetObjects<Classification>("Name");
                    model.AllCaptureRates = this.dataService.GetObjects<CaptureRate>("CatchRate");
                    model.AllEggCycles = this.dataService.GetObjects<EggCycle>("CycleCount");
                    model.AllExperienceGrowths = this.dataService.GetObjects<ExperienceGrowth>("Name");
                    model.AllGenderRatios = new List<GenderRatioViewModel>();
                    model.AllSpecialGroupings = this.dataService.GetObjects<SpecialGrouping>();

                    foreach (GenderRatio genderRatio in this.dataService.GetObjects<GenderRatio>())
                    {
                        GenderRatioViewModel viewModel = new GenderRatioViewModel()
                        {
                            Id = genderRatio.Id,
                        };

                        if (genderRatio.MaleRatio == genderRatio.FemaleRatio && genderRatio.MaleRatio == 0)
                        {
                            viewModel.GenderRatioString = "Gender Unknown";
                        }
                        else if (genderRatio.MaleRatio == genderRatio.FemaleRatio && genderRatio.MaleRatio == 100)
                        {
                            viewModel.GenderRatioString = "Gender Not Revealed";
                        }
                        else if (genderRatio.FemaleRatio == 0)
                        {
                            viewModel.GenderRatioString = string.Concat(genderRatio.MaleRatio, "% Male");
                        }
                        else if (genderRatio.MaleRatio == 0)
                        {
                            viewModel.GenderRatioString = string.Concat(genderRatio.FemaleRatio, "% Female");
                        }
                        else
                        {
                            viewModel.GenderRatioString = string.Concat(genderRatio.MaleRatio, "% Male / ", genderRatio.FemaleRatio, "% Female");
                        }

                        model.AllGenderRatios.Add(viewModel);
                        model.AllGenderRatios.OrderBy(x => x.GenderRatioString).ToList();
                    }
                }
                else
                {
                    model.Name = model.NameWithForm;
                }

                this.ModelState.AddModelError("PokedexNumber", "A pokemon with a this pokedex number already exists.");

                return this.View(model);
            }

            Pokemon oldPokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemon.Id, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth");
            List<Pokemon> altForms = this.dataService.GetObjects<Pokemon>("Game.ReleaseDate, PokedexNumber, Id", "Form", "OriginalFormId", pokemon.Id);
            if (oldPokemon.PokedexNumber != pokemon.PokedexNumber)
            {
                foreach (var p in altForms)
                {
                    if (p.PokedexNumber != pokemon.PokedexNumber)
                    {
                        p.PokedexNumber = pokemon.PokedexNumber;
                        this.dataService.UpdateObject(p);
                    }
                }
            }

            if (oldPokemon.SpecialGroupingId != pokemon.SpecialGroupingId)
            {
                foreach (var p in altForms)
                {
                    if (p.SpecialGroupingId != pokemon.SpecialGroupingId)
                    {
                        p.SpecialGroupingId = pokemon.SpecialGroupingId;
                        this.dataService.UpdateObject(p);
                    }
                }
            }

            this.dataService.UpdateObject(pokemon);

            return this.RedirectToAction("Pokemon", "Owner");
        }

        [HttpGet]
        [Route("edit_official_pokemon_image/{id:int}")]
        public IActionResult OfficialPokemonImage(int id)
        {
            Pokemon model = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", id, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form");

            if (model.IsAltForm)
            {
                model.Name = model.NameWithForm;
            }

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_official_pokemon_image/{id:int}")]
        public IActionResult OfficialPokemonImage(Pokemon pokemon, int id, IFormFile fileUpload, string urlUpload)
        {
            if (!this.ModelState.IsValid && pokemon.Name.Length <= 25)
            {
                Pokemon model = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", id, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form");

                if (model.IsAltForm)
                {
                    model.Name = model.NameWithForm;
                }

                return this.View(model);
            }
            else if (fileUpload == null && string.IsNullOrEmpty(urlUpload))
            {
                Pokemon model = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", id, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form");

                if (model.IsAltForm)
                {
                    model.Name = model.NameWithForm;
                }

                this.ModelState.AddModelError("Picture", "An image is needed to update.");
                return this.View(model);
            }
            else if ((fileUpload?.FileName.Contains(".png") == false) || (!string.IsNullOrEmpty(urlUpload) && !urlUpload.Contains(".png")))
            {
                Pokemon model = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", id, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form");

                if (model.IsAltForm)
                {
                    model.Name = model.NameWithForm;
                }

                this.ModelState.AddModelError("Picture", "The image must be in the .png format.");
                return this.View(model);
            }

            this.dataService.UploadImages(fileUpload, urlUpload, id.ToString(), this.appConfig, ImageType.Official);

            return this.RedirectToAction("Pokemon", "Owner");
        }

        [HttpGet]
        [Route("edit_home_pokemon_image/{id:int}")]
        public IActionResult HomePokemonImage(int id)
        {
            Pokemon model = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", id, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form");

            if (model.IsAltForm)
            {
                model.Name = model.NameWithForm;
            }

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_home_pokemon_image/{id:int}")]
        public IActionResult HomePokemonImage(Pokemon pokemon, int id, IFormFile fileUpload, string urlUpload)
        {
            if (!this.ModelState.IsValid && pokemon.Name.Length <= 25)
            {
                Pokemon model = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", id, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form");

                if (model.IsAltForm)
                {
                    model.Name = model.NameWithForm;
                }

                return this.View(model);
            }
            else if (fileUpload == null && string.IsNullOrEmpty(urlUpload))
            {
                Pokemon model = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", id, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form");

                if (model.IsAltForm)
                {
                    model.Name = model.NameWithForm;
                }

                this.ModelState.AddModelError("Picture", "An image is needed to update.");
                return this.View(model);
            }
            else if ((fileUpload?.FileName.Contains(".png") == false) || (!string.IsNullOrEmpty(urlUpload) && !urlUpload.Contains(".png")))
            {
                Pokemon model = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", id, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form");

                if (model.IsAltForm)
                {
                    model.Name = model.NameWithForm;
                }

                this.ModelState.AddModelError("Picture", "The image must be in the .png format.");
                return this.View(model);
            }

            this.dataService.UploadImages(fileUpload, urlUpload, id.ToString(), this.appConfig, ImageType.Home);
            this.dataService.UpdateImageBools(id, ImageType.Home);

            return this.RedirectToAction("Pokemon", "Owner");
        }

        [HttpGet]
        [Route("edit_shiny_pokemon_image/{id:int}")]
        public IActionResult ShinyPokemonImage(int id)
        {
            Pokemon model = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", id, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form");

            if (model.IsAltForm)
            {
                model.Name = model.NameWithForm;
            }

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_shiny_pokemon_image/{id:int}")]
        public IActionResult ShinyPokemonImage(Pokemon pokemon, int id, IFormFile fileUpload, string urlUpload)
        {
            if (!this.ModelState.IsValid && pokemon.Name.Length <= 25)
            {
                Pokemon model = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", id, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form");

                if (model.IsAltForm)
                {
                    model.Name = model.NameWithForm;
                }

                return this.View(model);
            }
            else if (fileUpload == null && string.IsNullOrEmpty(urlUpload))
            {
                Pokemon model = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", id, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form");

                if (model.IsAltForm)
                {
                    model.Name = model.NameWithForm;
                }

                this.ModelState.AddModelError("Picture", "An image is needed to update.");
                return this.View(model);
            }
            else if ((fileUpload?.FileName.Contains(".png") == false) || (!string.IsNullOrEmpty(urlUpload) && !urlUpload.Contains(".png")))
            {
                Pokemon model = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", id, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form");

                if (model.IsAltForm)
                {
                    model.Name = model.NameWithForm;
                }

                this.ModelState.AddModelError("Picture", "The image must be in the .png format.");
                return this.View(model);
            }

            this.dataService.UploadImages(fileUpload, urlUpload, id.ToString(), this.appConfig, ImageType.Shiny);
            this.dataService.UpdateImageBools(id, ImageType.Shiny);

            return this.RedirectToAction("Pokemon", "Owner");
        }

        [HttpGet]
        [Route("edit_pokeball_image/{id:int}")]
        public IActionResult PokeballImage(int id)
        {
            Pokeball model = this.dataService.GetObjectByPropertyValue<Pokeball>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_pokeball_image/{id:int}")]
        public IActionResult PokeballImage(int id, IFormFile fileUpload, string urlUpload)
        {
            if (!this.ModelState.IsValid)
            {
                Pokeball model = this.dataService.GetObjectByPropertyValue<Pokeball>("Id", id);

                return this.View(model);
            }
            else if (fileUpload == null && string.IsNullOrEmpty(urlUpload))
            {
                Pokeball model = this.dataService.GetObjectByPropertyValue<Pokeball>("Id", id);

                this.ModelState.AddModelError("Picture", "An image is needed to update.");
                return this.View(model);
            }
            else if ((fileUpload?.FileName.Contains(".png") == false) || (!string.IsNullOrEmpty(urlUpload) && !urlUpload.Contains(".png")))
            {
                Pokeball model = this.dataService.GetObjectByPropertyValue<Pokeball>("Id", id);

                this.ModelState.AddModelError("Picture", "The image must be in the .png format.");
                return this.View(model);
            }

            this.dataService.UploadImages(fileUpload, urlUpload, id.ToString(), this.appConfig, ImageType.Pokeball);

            return this.RedirectToAction("Pokeballs", "Owner");
        }

        [HttpGet]
        [Route("edit_mark_image/{id:int}")]
        public IActionResult MarkImage(int id)
        {
            Mark model = this.dataService.GetObjectByPropertyValue<Mark>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_mark_image/{id:int}")]
        public IActionResult MarkImage(int id, IFormFile fileUpload, string urlUpload)
        {
            if (!this.ModelState.IsValid)
            {
                Mark model = this.dataService.GetObjectByPropertyValue<Mark>("Id", id);

                return this.View(model);
            }
            else if (fileUpload == null && string.IsNullOrEmpty(urlUpload))
            {
                Mark model = this.dataService.GetObjectByPropertyValue<Mark>("Id", id);

                this.ModelState.AddModelError("Picture", "An image is needed to update.");
                return this.View(model);
            }
            else if ((fileUpload?.FileName.Contains(".png") == false) || (!string.IsNullOrEmpty(urlUpload) && !urlUpload.Contains(".png")))
            {
                Mark model = this.dataService.GetObjectByPropertyValue<Mark>("Id", id);

                this.ModelState.AddModelError("Picture", "The image must be in the .png format.");
                return this.View(model);
            }

            this.dataService.UploadImages(fileUpload, urlUpload, id.ToString(), this.appConfig, ImageType.Mark);

            return this.RedirectToAction("Marks", "Owner");
        }

        [HttpGet]
        [Route("edit_sweet_image/{id:int}")]
        public IActionResult SweetImage(int id)
        {
            Sweet model = this.dataService.GetObjectByPropertyValue<Sweet>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_sweet_image/{id:int}")]
        public IActionResult SweetImage(int id, IFormFile fileUpload, string urlUpload)
        {
            if (!this.ModelState.IsValid)
            {
                Sweet model = this.dataService.GetObjectByPropertyValue<Sweet>("Id", id);

                return this.View(model);
            }
            else if (fileUpload == null && string.IsNullOrEmpty(urlUpload))
            {
                Sweet model = this.dataService.GetObjectByPropertyValue<Sweet>("Id", id);

                this.ModelState.AddModelError("Picture", "An image is needed to update.");
                return this.View(model);
            }
            else if ((fileUpload?.FileName.Contains(".png") == false) || (!string.IsNullOrEmpty(urlUpload) && !urlUpload.Contains(".png")))
            {
                Sweet model = this.dataService.GetObjectByPropertyValue<Sweet>("Id", id);

                this.ModelState.AddModelError("Picture", "The image must be in the .png format.");
                return this.View(model);
            }

            this.dataService.UploadImages(fileUpload, urlUpload, id.ToString(), this.appConfig, ImageType.Sweet);

            return this.RedirectToAction("Sweets", "Owner");
        }

        /// <summary>
        /// Adds a gender difference.
        /// </summary>
        /// <param name="pokemonId">The pokemon's id.</param>
        /// <returns>The view to add a gender difference to a pokemon.</returns>
        [HttpGet]
        [Route("add_gender_difference/{pokemonId:int}")]
        public IActionResult GenderDifferenceImage(int pokemonId)
        {
            Pokemon model = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form");

            if (model.IsAltForm)
            {
                model.Name = model.NameWithForm;
            }

            return this.View(model);
        }

        /// <summary>
        /// Adds a gender difference.
        /// </summary>
        /// <param name="pokemon">The pokemon getting a gender difference.</param>
        /// <param name="isFemale">True if Female is selected. False if Male is selected.</param>
        /// <param name="officialFileUpload">The original image used for the gender difference (File).</param>
        /// <param name="officialUrlUpload">The original image used for the gender difference (URL to file).</param>
        /// <param name="shinyFileUpload">The shiny image used for the gender difference (File).</param>
        /// <param name="shinyUrlUpload">The shiny image used for the gender difference (URL to file).</param>
        /// <param name="homeFileUpload">The home image used for the gender difference (File).</param>
        /// <param name="homeUrlUpload">The home image used for the gender difference (URL to file).</param>
        /// <returns>The view to the pokemon admin page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_gender_difference/{pokemonId:int}")]
        public IActionResult GenderDifferenceImage(Pokemon pokemon, bool isFemale, IFormFile officialFileUpload, string officialUrlUpload, IFormFile shinyFileUpload, string shinyUrlUpload, IFormFile homeFileUpload, string homeUrlUpload)
        {
            if (!this.ModelState.IsValid && pokemon.Name.Length <= 25)
            {
                Pokemon model = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemon.Id, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form");

                if (model.IsAltForm)
                {
                    model.Name = model.NameWithForm;
                }

                return this.View(model);
            }
            else if (officialFileUpload == null && string.IsNullOrEmpty(officialUrlUpload) && shinyFileUpload == null && string.IsNullOrEmpty(shinyUrlUpload) && homeFileUpload == null && string.IsNullOrEmpty(homeUrlUpload))
            {
                Pokemon model = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemon.Id, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form");

                if (model.IsAltForm)
                {
                    model.Name = model.NameWithForm;
                }

                this.ModelState.AddModelError("Picture", "An image is needed to update.");
                return this.View(model);
            }
            else if ((officialFileUpload?.FileName.Contains(".png") == false) || (!string.IsNullOrEmpty(officialUrlUpload) && !officialUrlUpload.Contains(".png")))
            {
                Pokemon model = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemon.Id, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form");

                if (model.IsAltForm)
                {
                    model.Name = model.NameWithForm;
                }

                this.ModelState.AddModelError("Picture", "The image must be in the .png format.");
                return this.View(model);
            }
            else if ((shinyFileUpload?.FileName.Contains(".png") == false) || (!string.IsNullOrEmpty(shinyUrlUpload) && !shinyUrlUpload.Contains(".png")))
            {
                Pokemon model = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemon.Id, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form");

                if (model.IsAltForm)
                {
                    model.Name = model.NameWithForm;
                }

                this.ModelState.AddModelError("Picture", "The image must be in the .png format.");
                return this.View(model);
            }
            else if ((officialFileUpload?.FileName.Contains(".png") == false) || (!string.IsNullOrEmpty(homeUrlUpload) && !homeUrlUpload.Contains(".png")))
            {
                Pokemon model = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemon.Id, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form");

                if (model.IsAltForm)
                {
                    model.Name = model.NameWithForm;
                }

                this.ModelState.AddModelError("Picture", "The image must be in the .png format.");
                return this.View(model);
            }

            string fileName = pokemon.Id.ToString();
            if (isFemale)
            {
                fileName += "-f";
            }
            else
            {
                fileName += "-m";
            }

            if (officialFileUpload != null || !string.IsNullOrEmpty(officialUrlUpload))
            {
                this.dataService.UploadImages(officialFileUpload, officialUrlUpload, fileName, this.appConfig, ImageType.GenderDifference);
            }

            if (homeFileUpload != null || !string.IsNullOrEmpty(homeUrlUpload))
            {
                this.dataService.UploadImages(homeFileUpload, homeUrlUpload, fileName, this.appConfig, ImageType.GenderDifferenceHome);
            }

            if (shinyFileUpload != null || !string.IsNullOrEmpty(shinyUrlUpload))
            {
                this.dataService.UploadImages(shinyFileUpload, shinyUrlUpload, fileName, this.appConfig, ImageType.GenderDifferenceShiny);
            }

            return this.RedirectToAction("Pokemon", "Owner");
        }

        [HttpGet]
        [Route("edit_typing/{pokemonId:int}/{generationId:int}")]
        public IActionResult Typing(int pokemonId, int generationId)
        {
            PokemonTypeDetail typeDetail = this.dataService.GetObjects<PokemonTypeDetail>(includes: "Pokemon, PrimaryType, SecondaryType, Generation", whereProperty: "PokemonId", wherePropertyValue: pokemonId).Find(x => x.GenerationId == generationId);
            PokemonTypingViewModel model = new PokemonTypingViewModel()
            {
                Id = typeDetail.Id,
                AllTypes = this.dataService.GetObjects<Type>("Name"),
                PokemonId = typeDetail.PokemonId,
                Pokemon = typeDetail.Pokemon,
                PrimaryTypeId = typeDetail.PrimaryTypeId,
                SecondaryTypeId = typeDetail.SecondaryTypeId,
                GenerationId = typeDetail.GenerationId,
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_typing/{id:int}/{generationId:int}")]
        public IActionResult Typing(PokemonTypeDetail pokemonTypeDetail)
        {
            if (!this.ModelState.IsValid)
            {
                PokemonTypeDetail typeDetail = this.dataService.GetObjects<PokemonTypeDetail>(includes: "Pokemon, PrimaryType, SecondaryType, Generation", whereProperty: "PokemonId", wherePropertyValue: pokemonTypeDetail.PokemonId).Find(x => x.GenerationId == pokemonTypeDetail.GenerationId);
                PokemonTypingViewModel model = new PokemonTypingViewModel()
                {
                    Id = typeDetail.Id,
                    AllTypes = this.dataService.GetObjects<Type>("Name"),
                    PokemonId = typeDetail.PokemonId,
                    Pokemon = typeDetail.Pokemon,
                    PrimaryTypeId = typeDetail.PrimaryTypeId,
                    SecondaryTypeId = typeDetail.SecondaryTypeId,
                    GenerationId = typeDetail.GenerationId,
                };

                return this.View(model);
            }

            if (pokemonTypeDetail.PrimaryTypeId == pokemonTypeDetail.SecondaryTypeId)
            {
                pokemonTypeDetail.SecondaryTypeId = null;
            }

            this.dataService.UpdateObject(pokemonTypeDetail);

            return this.RedirectToAction("Pokemon", "Owner");
        }

        [HttpGet]
        [Route("edit_abilities/{pokemonId:int}/{generationId:int}")]
        public IActionResult Abilities(int pokemonId, int generationId)
        {
            PokemonAbilityDetail abilityDetail = this.dataService.GetObjects<PokemonAbilityDetail>(includes: "Pokemon, PrimaryAbility, SecondaryAbility, HiddenAbility", whereProperty: "PokemonId", wherePropertyValue: pokemonId).Find(x => x.GenerationId == generationId);
            PokemonAbilitiesViewModel model = new PokemonAbilitiesViewModel()
            {
                Id = abilityDetail.Id,
                AllAbilities = this.dataService.GetObjects<Ability>("Name"),
                PokemonId = abilityDetail.PokemonId,
                Pokemon = abilityDetail.Pokemon,
                PrimaryAbilityId = abilityDetail.PrimaryAbilityId,
                SecondaryAbilityId = abilityDetail.SecondaryAbilityId,
                HiddenAbilityId = abilityDetail.HiddenAbilityId,
                GenerationId = generationId,
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_abilities/{pokemonId:int}/{generationId:int}")]
        public IActionResult Abilities(PokemonAbilityDetail pokemonAbilityDetail)
        {
            if (!this.ModelState.IsValid)
            {
                PokemonAbilityDetail abilityDetail = this.dataService.GetObjects<PokemonAbilityDetail>(includes: "Pokemon, PrimaryAbility, SecondaryAbility, HiddenAbility", whereProperty: "PokemonId", wherePropertyValue: pokemonAbilityDetail.PokemonId).Find(x => x.GenerationId == pokemonAbilityDetail.GenerationId);
                PokemonAbilitiesViewModel model = new PokemonAbilitiesViewModel()
                {
                    Id = abilityDetail.Id,
                    AllAbilities = this.dataService.GetObjects<Ability>("Name"),
                    PokemonId = abilityDetail.PokemonId,
                    Pokemon = abilityDetail.Pokemon,
                    PrimaryAbilityId = abilityDetail.PrimaryAbilityId,
                    SecondaryAbilityId = abilityDetail.SecondaryAbilityId,
                    HiddenAbilityId = abilityDetail.HiddenAbilityId,
                    GenerationId = abilityDetail.GenerationId,
                };

                return this.View(model);
            }

            this.dataService.UpdateObject(pokemonAbilityDetail);

            return this.RedirectToAction("Pokemon", "Owner");
        }

        [HttpGet]
        [Route("edit_egg_groups/{pokemonId:int}/{generationId:int}")]
        public IActionResult EggGroups(int pokemonId, int generationId)
        {
            PokemonEggGroupDetail eggGroupDetail = this.dataService.GetObjects<PokemonEggGroupDetail>(includes: "Pokemon, PrimaryEggGroup, SecondaryEggGroup", whereProperty: "PokemonId", wherePropertyValue: pokemonId).Find(x => x.GenerationId == generationId);
            PokemonEggGroupsViewModel model = new PokemonEggGroupsViewModel()
            {
                Id = eggGroupDetail.Id,
                AllEggGroups = this.dataService.GetObjects<EggGroup>("Name"),
                PokemonId = eggGroupDetail.PokemonId,
                Pokemon = eggGroupDetail.Pokemon,
                PrimaryEggGroupId = eggGroupDetail.PrimaryEggGroupId,
                SecondaryEggGroupId = eggGroupDetail.SecondaryEggGroupId,
                GenerationId = generationId,
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_egg_groups/{pokemonId:int}/{generationId:int}")]
        public IActionResult EggGroups(PokemonEggGroupDetail pokemonEggGroupDetail)
        {
            if (!this.ModelState.IsValid)
            {
                PokemonEggGroupDetail eggGroupDetail = this.dataService.GetObjects<PokemonEggGroupDetail>(includes: "Pokemon, PrimaryEggGroup, SecondaryEggGroup", whereProperty: "PokemonId", wherePropertyValue: pokemonEggGroupDetail.PokemonId).Find(x => x.GenerationId == pokemonEggGroupDetail.GenerationId);
                PokemonEggGroupsViewModel model = new PokemonEggGroupsViewModel()
                {
                    Id = eggGroupDetail.Id,
                    AllEggGroups = this.dataService.GetObjects<EggGroup>("Name"),
                    PokemonId = eggGroupDetail.PokemonId,
                    Pokemon = eggGroupDetail.Pokemon,
                    PrimaryEggGroupId = eggGroupDetail.PrimaryEggGroupId,
                    SecondaryEggGroupId = eggGroupDetail.SecondaryEggGroupId,
                    GenerationId = eggGroupDetail.GenerationId,
                };

                return this.View(model);
            }

            this.dataService.UpdateObject(pokemonEggGroupDetail);

            return this.RedirectToAction("Pokemon", "Owner");
        }

        [HttpGet]
        [Route("edit_base_stats/{pokemonId:int}/{generationId:int}")]
        public IActionResult BaseStats(int pokemonId, int generationId)
        {
            BaseStat model = this.dataService.GetPokemonBaseStats(pokemonId, generationId);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_base_stats/{pokemonId:int}/{generationId:int}")]
        public IActionResult BaseStats(BaseStat baseStat)
        {
            if (!this.ModelState.IsValid)
            {
                BaseStat model = this.dataService.GetPokemonBaseStats(baseStat.PokemonId, baseStat.GenerationId);

                return this.View(model);
            }

            this.dataService.UpdateObject(baseStat);

            return this.RedirectToAction("Pokemon", "Owner");
        }

        [HttpGet]
        [Route("edit_ev_yields/{pokemonId:int}/{generationId:int}")]
        public IActionResult EVYields(int pokemonId, int generationId)
        {
            EVYield model = this.dataService.GetPokemonEVYields(pokemonId, generationId);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_ev_yields/{pokemonId:int}/{generationId:int}")]
        public IActionResult EVYields(EVYield evYield)
        {
            if (!this.ModelState.IsValid)
            {
                EVYield model = this.dataService.GetPokemonEVYields(evYield.PokemonId, evYield.GenerationId);

                return this.View(model);
            }

            this.dataService.UpdateObject(evYield);

            return this.RedirectToAction("Pokemon", "Owner");
        }

        [HttpGet]
        [Route("edit_type/{id:int}")]
        public IActionResult Type(int id)
        {
            Type type = this.dataService.GetObjectByPropertyValue<Type>("Id", id);
            TypeGenerationViewModel model = new TypeGenerationViewModel()
            {
                Name = type.Name,
                GenerationId = type.GenerationId,
                AllGenerations = this.dataService.GetObjects<Generation>(),
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_type/{id:int}")]
        public IActionResult Type(Type type)
        {
            if (!this.ModelState.IsValid)
            {
                Type newType = this.dataService.GetObjectByPropertyValue<Type>("Id", type.Id);
                TypeGenerationViewModel model = new TypeGenerationViewModel()
                {
                    Name = newType.Name,
                    GenerationId = newType.GenerationId,
                    AllGenerations = this.dataService.GetObjects<Generation>(),
                };

                return this.View(model);
            }

            this.dataService.UpdateObject(type);

            return this.RedirectToAction("Types", "Owner");
        }

        [HttpGet]
        [Route("edit_egg_cycle/{id:int}")]
        public IActionResult EggCycle(int id)
        {
            EggCycle model = this.dataService.GetObjectByPropertyValue<EggCycle>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_egg_cycle/{id:int}")]
        public IActionResult EggCycle(EggCycle eggCycle)
        {
            if (!this.ModelState.IsValid)
            {
                EggCycle model = this.dataService.GetObjectByPropertyValue<EggCycle>("Id", eggCycle.Id);

                return this.View(model);
            }

            this.dataService.UpdateObject(eggCycle);

            return this.RedirectToAction("EggCycles", "Owner");
        }

        [HttpGet]
        [Route("edit_experience_growth/{id:int}")]
        public IActionResult ExperienceGrowth(int id)
        {
            ExperienceGrowth model = this.dataService.GetObjectByPropertyValue<ExperienceGrowth>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_experience_growth/{id:int}")]
        public IActionResult ExperienceGrowth(ExperienceGrowth experienceGrowth)
        {
            if (!this.ModelState.IsValid)
            {
                ExperienceGrowth model = this.dataService.GetObjectByPropertyValue<ExperienceGrowth>("Id", experienceGrowth.Id);

                return this.View(model);
            }

            this.dataService.UpdateObject(experienceGrowth);

            return this.RedirectToAction("ExperienceGrowths", "Owner");
        }

        [HttpGet]
        [Route("edit_gender_ratio/{id:int}")]
        public IActionResult GenderRatio(int id)
        {
            GenderRatio model = this.dataService.GetObjectByPropertyValue<GenderRatio>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_gender_ratio/{id:int}")]
        public IActionResult GenderRatio(GenderRatio genderRatio)
        {
            if (!this.ModelState.IsValid)
            {
                GenderRatio model = this.dataService.GetObjectByPropertyValue<GenderRatio>("Id", genderRatio.Id);

                return this.View(model);
            }

            this.dataService.UpdateObject(genderRatio);

            return this.RedirectToAction("GenderRatios", "Owner");
        }

        [HttpGet]
        [Route("edit_egg_group/{id:int}")]
        public IActionResult EggGroup(int id)
        {
            EggGroup model = this.dataService.GetObjectByPropertyValue<EggGroup>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_egg_group/{id:int}")]
        public IActionResult EggGroup(EggGroup eggGroup)
        {
            if (!this.ModelState.IsValid)
            {
                EggGroup model = this.dataService.GetObjectByPropertyValue<EggGroup>("Id", eggGroup.Id);

                return this.View(model);
            }

            this.dataService.UpdateObject(eggGroup);

            return this.RedirectToAction("EggGroups", "Owner");
        }

        [HttpGet]
        [Route("edit_form_item/{id:int}")]
        public IActionResult FormItem(int id)
        {
            FormItem item = this.dataService.GetObjectByPropertyValue<FormItem>("Id", id);
            FormItemViewModel model = new FormItemViewModel()
            {
                PokemonId = item.PokemonId,
                Name = item.Name,
            };

            List<Pokemon> altForms = this.dataService.GetObjects<Pokemon>(includes: "Form").Where(x => x.IsAltForm && x.Form.NeedsItem).ToList();
            altForms.Remove(altForms.Find(x => x.Name == "Rayquaza"));
            altForms.ForEach(x => x.Name = x.NameWithForm);
            model.AllPokemon = altForms;

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_form_item/{id:int}")]
        public IActionResult FormItem(FormItem formItem)
        {
            if (!this.ModelState.IsValid)
            {
                FormItem item = this.dataService.GetObjectByPropertyValue<FormItem>("Id", formItem.Id);
                FormItemViewModel model = new FormItemViewModel()
                {
                    PokemonId = item.PokemonId,
                    Name = item.Name,
                };

                List<Pokemon> altForms = this.dataService.GetObjects<Pokemon>(includes: "Form").Where(x => x.IsAltForm && x.Form.NeedsItem).ToList();
                altForms.Remove(altForms.Find(x => x.Name == "Rayquaza"));
                altForms.ForEach(x => x.Name = x.NameWithForm);
                model.AllPokemon = altForms;

                return this.View(model);
            }

            this.dataService.UpdateObject(formItem);

            return this.RedirectToAction("FormItems", "Owner");
        }

        [HttpGet]
        [Route("edit_form_group/{id:int}")]
        public IActionResult FormGroup(int id)
        {
            FormGroup model = this.dataService.GetObjectByPropertyValue<FormGroup>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_form_group/{id:int}")]
        public IActionResult FormGroup(FormGroup formGroup)
        {
            if (!this.ModelState.IsValid)
            {
                FormGroup model = this.dataService.GetObjectByPropertyValue<FormGroup>("Id", formGroup.Id);

                return this.View(model);
            }

            this.dataService.UpdateObject(formGroup);

            return this.RedirectToAction("FormGroups", "Owner");
        }

        [HttpGet]
        [Route("edit_form/{id:int}")]
        public IActionResult Form(int id)
        {
            Form form = this.dataService.GetObjectByPropertyValue<Form>("Id", id);
            FormModelViewModel model = new FormModelViewModel()
            {
                Id = form.Id,
                Name = form.Name,
                Randomizable = form.Randomizable,
                FormGroupId = form.FormGroupId,
                NeedsItem = form.NeedsItem,
                OnlyDuringBattle = form.OnlyDuringBattle,
                AllFormGroups = this.dataService.GetObjects<FormGroup>(),
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_form/{id:int}")]
        public IActionResult Form(Form form)
        {
            if (!this.ModelState.IsValid)
            {
                Form oldForm = this.dataService.GetObjectByPropertyValue<Form>("Id", form.Id);
                FormModelViewModel model = new FormModelViewModel()
                {
                    Id = oldForm.Id,
                    Name = oldForm.Name,
                    Randomizable = oldForm.Randomizable,
                    FormGroupId = oldForm.FormGroupId,
                    NeedsItem = oldForm.NeedsItem,
                    OnlyDuringBattle = oldForm.OnlyDuringBattle,
                    AllFormGroups = this.dataService.GetObjects<FormGroup>(),
                };

                return this.View(model);
            }

            if (form.OnlyWithTera && !form.OnlyDuringBattle)
            {
                form.OnlyDuringBattle = true;
            }

            this.dataService.UpdateObject(form);

            return this.RedirectToAction("Forms", "Owner");
        }

        [HttpGet]
        [Route("edit_evolution_method/{id:int}")]
        public IActionResult EvolutionMethod(int id)
        {
            EvolutionMethod model = this.dataService.GetObjectByPropertyValue<EvolutionMethod>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_evolution_method/{id:int}")]
        public IActionResult EvolutionMethod(EvolutionMethod evolutionMethod)
        {
            if (!this.ModelState.IsValid)
            {
                EvolutionMethod model = this.dataService.GetObjectByPropertyValue<EvolutionMethod>("Id", evolutionMethod.Id);

                return this.View(model);
            }

            this.dataService.UpdateObject(evolutionMethod);

            return this.RedirectToAction("EvolutionMethods", "Owner");
        }

        [HttpGet]
        [Route("edit_capture_rate/{id:int}")]
        public IActionResult CaptureRate(int id)
        {
            CaptureRate model = this.dataService.GetObjectByPropertyValue<CaptureRate>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_capture_rate/{id:int}")]
        public IActionResult CaptureRate(CaptureRate captureRate)
        {
            if (!this.ModelState.IsValid)
            {
                CaptureRate model = this.dataService.GetObjectByPropertyValue<CaptureRate>("Id", captureRate.Id);

                return this.View(model);
            }

            this.dataService.UpdateObject(captureRate);

            return this.RedirectToAction("CaptureRates", "Owner");
        }

        [HttpGet]
        [Route("edit_base_happiness/{id:int}")]
        public IActionResult BaseHappiness(int id)
        {
            BaseHappiness model = this.dataService.GetObjectByPropertyValue<BaseHappiness>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_base_happiness/{id:int}")]
        public IActionResult BaseHappiness(BaseHappiness baseHappiness)
        {
            if (!this.ModelState.IsValid)
            {
                BaseHappiness model = this.dataService.GetObjectByPropertyValue<BaseHappiness>("Id", baseHappiness.Id);

                return this.View(model);
            }

            this.dataService.UpdateObject(baseHappiness);

            return this.RedirectToAction("BaseHappinesses", "Owner");
        }

        [HttpGet]
        [Route("edit_pokemon_capture_rate/{pokemonId:int}/{generationId:int}")]
        public IActionResult CaptureRates(int pokemonId, int generationId)
        {
            PokemonCaptureRateDetail captureRate = this.GetPokemonWithCaptureRatesFromGenerationId(pokemonId, generationId);
            PokemonCaptureRateViewModel model = new PokemonCaptureRateViewModel()
            {
                Id = captureRate.Id,
                PokemonId = captureRate.PokemonId,
                GenerationId = captureRate.GenerationId,
                CaptureRateId = captureRate.CaptureRateId,
                AllCaptureRates = this.dataService.GetObjects<CaptureRate>("CatchRate"),
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_pokemon_capture_rate/{id:int}/{generationId:int}")]
        public IActionResult CaptureRates(PokemonCaptureRateDetail pokemonCaptureRate)
        {
            if (!this.ModelState.IsValid)
            {
                PokemonCaptureRateDetail captureRate = this.GetPokemonWithCaptureRatesFromGenerationId(pokemonCaptureRate.PokemonId, pokemonCaptureRate.GenerationId);
                PokemonCaptureRateViewModel model = new PokemonCaptureRateViewModel()
                {
                    Id = captureRate.Id,
                    PokemonId = captureRate.PokemonId,
                    GenerationId = captureRate.GenerationId,
                    CaptureRateId = captureRate.CaptureRateId,
                    AllCaptureRates = this.dataService.GetObjects<CaptureRate>("CatchRate"),
                };

                return this.View(model);
            }

            this.dataService.UpdateObject(pokemonCaptureRate);

            return this.RedirectToAction("Pokemon", "Owner");
        }

        [HttpGet]
        [Route("edit_pokemon_base_happiness/{pokemonId:int}/{generationId:int}")]
        public IActionResult BaseHappinesses(int pokemonId, int generationId)
        {
            PokemonBaseHappinessDetail bassHappiness = this.GetPokemonWithBaseHappinesssFromGenerationId(pokemonId, generationId);
            PokemonBaseHappinessViewModel model = new PokemonBaseHappinessViewModel()
            {
                Id = bassHappiness.Id,
                PokemonId = bassHappiness.PokemonId,
                GenerationId = bassHappiness.GenerationId,
                BaseHappinessId = bassHappiness.BaseHappinessId,
                AllBaseHappinesses = this.dataService.GetObjects<BaseHappiness>("Happiness"),
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_pokemon_base_happiness/{id:int}/{generationId:int}")]
        public IActionResult BaseHappinesses(PokemonBaseHappinessDetail pokemonBaseHappiness)
        {
            if (!this.ModelState.IsValid)
            {
                PokemonBaseHappinessDetail baseHappiness = this.GetPokemonWithBaseHappinesssFromGenerationId(pokemonBaseHappiness.PokemonId, pokemonBaseHappiness.GenerationId);
                PokemonBaseHappinessViewModel model = new PokemonBaseHappinessViewModel()
                {
                    Id = baseHappiness.Id,
                    PokemonId = baseHappiness.PokemonId,
                    GenerationId = baseHappiness.GenerationId,
                    BaseHappinessId = baseHappiness.BaseHappinessId,
                    AllBaseHappinesses = this.dataService.GetObjects<BaseHappiness>("Happiness"),
                };

                return this.View(model);
            }

            this.dataService.UpdateObject(pokemonBaseHappiness);

            return this.RedirectToAction("Pokemon", "Owner");
        }

        [HttpGet]
        [Route("edit_nature/{id:int}")]
        public IActionResult Nature(int id)
        {
            Nature nature = this.dataService.GetObjectByPropertyValue<Nature>("Id", id);
            NatureStatViewModel model = new NatureStatViewModel(nature)
            {
                AllStats = this.dataService.GetObjects<Stat>(),
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_nature/{id:int}")]
        public IActionResult Nature(Nature nature)
        {
            if (!this.ModelState.IsValid)
            {
                NatureStatViewModel model = new NatureStatViewModel(nature)
                {
                    AllStats = this.dataService.GetObjects<Stat>(),
                };

                return this.View(model);
            }

            this.dataService.UpdateObject(nature);

            return this.RedirectToAction("Natures", "Owner");
        }

        [HttpGet]
        [Route("edit_regional_dex/{id:int}")]
        public IActionResult RegionalDex(int id)
        {
            RegionalDex nature = this.dataService.GetObjectByPropertyValue<RegionalDex>("Id", id);
            RegionalDexViewModel model = new RegionalDexViewModel(nature)
            {
                AllGames = this.dataService.GetGamesGroupedByReleaseDate(),
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_regional_dex/{id:int}")]
        public IActionResult RegionalDex(RegionalDex regionalDex)
        {
            if (!this.ModelState.IsValid)
            {
                RegionalDexViewModel model = new RegionalDexViewModel(regionalDex)
                {
                    AllGames = this.dataService.GetGamesGroupedByReleaseDate(),
                };

                return this.View(model);
            }

            this.dataService.UpdateObject(regionalDex);

            return this.RedirectToAction("RegionalDexes", "Owner");
        }

        /// <summary>
        /// The page used to modify a stat in the database.
        /// </summary>
        /// <param name="id">The id of the stat being modified.</param>
        /// <returns>The stat editting page.</returns>
        [HttpGet]
        [Route("edit_stat/{id:int}")]
        public IActionResult Stat(int id)
        {
            Stat model = this.dataService.GetObjectByPropertyValue<Stat>("Id", id);

            return this.View(model);
        }

        /// <summary>
        /// The page used to modify a stat in the database.
        /// </summary>
        /// <param name="stat">The stat being modified.</param>
        /// <returns>The stats page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_stat/{id:int}")]
        public IActionResult Stat(Stat stat)
        {
            if (!this.ModelState.IsValid)
            {
                Stat model = this.dataService.GetObjectByPropertyValue<Stat>("Id", stat.Id);

                return this.View(model);
            }

            this.dataService.UpdateObject(stat);

            return this.RedirectToAction("Stats", "Owner");
        }

        [HttpGet]
        [Route("edit_pokeball/{id:int}")]
        public IActionResult Pokeball(int id)
        {
            Pokeball pokeball = this.dataService.GetObjectByPropertyValue<Pokeball>("Id", id);
            PokeballViewModel model = new PokeballViewModel()
            {
                Id = pokeball.Id,
                Name = pokeball.Name,
                GenerationId = pokeball.GenerationId,
                AllGenerations = this.dataService.GetObjects<Generation>(),
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_pokeball/{id:int}")]
        public IActionResult Pokeball(Pokeball pokeball)
        {
            if (!this.ModelState.IsValid)
            {
                Pokeball model = this.dataService.GetObjectByPropertyValue<Pokeball>("Id", pokeball.Id);

                return this.View(model);
            }

            this.dataService.UpdateObject(pokeball);

            return this.RedirectToAction("Pokeballs", "Owner");
        }

        [HttpGet]
        [Route("edit_mark/{id:int}")]
        public IActionResult Mark(int id)
        {
            Mark mark = this.dataService.GetObjectByPropertyValue<Mark>("Id", id);
            MarkViewModel model = new MarkViewModel()
            {
                Id = mark.Id,
                Name = mark.Name,
                NameAddition = mark.NameAddition,
                GenerationId = mark.GenerationId,
                AllGenerations = this.dataService.GetObjects<Generation>().Where(x => x.Id >= 8).ToList(),
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_mark/{id:int}")]
        public IActionResult Mark(Mark mark)
        {
            if (!this.ModelState.IsValid)
            {
                Mark model = this.dataService.GetObjectByPropertyValue<Mark>("Id", mark.Id);

                return this.View(model);
            }

            this.dataService.UpdateObject(mark);

            return this.RedirectToAction("Marks", "Owner");
        }

        [HttpGet]
        [Route("edit_sweet/{id:int}")]
        public IActionResult Sweet(int id)
        {
            Sweet model = this.dataService.GetObjectByPropertyValue<Sweet>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_sweet/{id:int}")]
        public IActionResult Sweet(Sweet sweet)
        {
            if (!this.ModelState.IsValid)
            {
                Sweet model = this.dataService.GetObjectByPropertyValue<Sweet>("Id", sweet.Id);

                return this.View(model);
            }

            this.dataService.UpdateObject(sweet);

            return this.RedirectToAction("Sweets", "Owner");
        }

        [HttpGet]
        [Route("edit_hunting_method/{id:int}")]
        public IActionResult HuntingMethod(int id)
        {
            HuntingMethod model = this.dataService.GetObjectByPropertyValue<HuntingMethod>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_hunting_method/{id:int}")]
        public IActionResult HuntingMethod(HuntingMethod huntingMethod)
        {
            if (!this.ModelState.IsValid)
            {
                HuntingMethod model = this.dataService.GetObjectByPropertyValue<HuntingMethod>("Id", huntingMethod.Id);

                return this.View(model);
            }

            this.dataService.UpdateObject(huntingMethod);

            return this.RedirectToAction("HuntingMethods", "Owner");
        }

        [HttpGet]
        [Route("edit_special_grouping/{id:int}")]
        public IActionResult SpecialGrouping(int id)
        {
            SpecialGrouping model = this.dataService.GetObjectByPropertyValue<SpecialGrouping>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_special_grouping/{id:int}")]
        public IActionResult SpecialGrouping(SpecialGrouping specialGrouping)
        {
            if (!this.ModelState.IsValid)
            {
                SpecialGrouping model = this.dataService.GetObjectByPropertyValue<SpecialGrouping>("Id", specialGrouping.Id);

                return this.View(model);
            }

            this.dataService.UpdateObject(specialGrouping);

            return this.RedirectToAction("SpecialGroupings", "Owner");
        }

        [HttpGet]
        [Route("edit_classification/{id:int}")]
        public IActionResult Classification(int id)
        {
            Classification model = this.dataService.GetObjectByPropertyValue<Classification>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_classification/{id:int}")]
        public IActionResult Classification(Classification classification)
        {
            if (!this.ModelState.IsValid)
            {
                Classification model = this.dataService.GetObjectByPropertyValue<Classification>("Id", classification.Id);

                return this.View(model);
            }

            if (classification.Name.Contains("pokemon"))
            {
                classification.Name = classification.Name.Replace("pokemon", "Pokémon");
            }
            else if (classification.Name.Contains("Pokemon"))
            {
                classification.Name = classification.Name.Replace("Pokemon", "Pokémon");
            }
            else if (classification.Name.Contains("pokémon"))
            {
                classification.Name = classification.Name.Replace("pokémon", "Pokémon");
            }
            else if (!classification.Name.Contains("Pokémon"))
            {
                classification.Name = string.Concat(classification.Name.Trim(), " Pokémon");
            }

            this.dataService.UpdateObject(classification);

            return this.RedirectToAction("Classifications", "Owner");
        }

        [HttpGet]
        [Route("edit_ability/{id:int}")]
        public IActionResult Ability(int id)
        {
            Ability ability = this.dataService.GetObjectByPropertyValue<Ability>("Id", id);
            AbilityAdminViewModel model = new AbilityAdminViewModel()
            {
                Id = ability.Id,
                Name = ability.Name,
                Description = ability.Description,
                GenerationId = ability.GenerationId,
                AllGenerations = this.dataService.GetObjects<Generation>(),
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_ability/{id:int}")]
        public IActionResult Ability(Ability ability)
        {
            if (!this.ModelState.IsValid)
            {
                Ability model = this.dataService.GetObjectByPropertyValue<Ability>("Id", ability.Id);

                return this.View(model);
            }

            this.dataService.UpdateObject(ability);

            return this.RedirectToAction("Abilities", "Owner");
        }

        [Route("edit_preevolutions/{pokemonId:int}/{generationId:int}")]
        public IActionResult Preevolutions(int pokemonId, int generationId)
        {
            List<Evolution> preevolutions = this.dataService.GetObjects<Evolution>(includes: "PreevolutionPokemon, PreevolutionPokemon.Form").Where(x => x.EvolutionPokemonId == pokemonId && x.GenerationId == generationId).ToList();
            if (preevolutions.Count() > 1)
            {
                preevolutions.Where(x => x.PreevolutionPokemon.IsAltForm).ForEach(x => x.PreevolutionPokemon.Name = x.PreevolutionPokemon.NameWithForm);
                EditEvolutionViewModel model = new EditEvolutionViewModel()
                {
                    AllEvolutions = preevolutions,
                    AppConfig = this.appConfig,
                };

                return this.View(model);
            }
            else
            {
                return this.RedirectToAction("Evolution", new { preEvolutionPokemonId = preevolutions.First().PreevolutionPokemonId, evolutionPokemonId = preevolutions.First().EvolutionPokemonId, generationId });
            }
        }

        [HttpGet]
        [Route("edit_evolution/{preevolutionPokemonId:int}/{evolutionPokemonId:int}/{generationId:int}")]
        public IActionResult Evolution(int preEvolutionPokemonId, int evolutionPokemonId, int generationId)
        {
            Evolution preEvolution = this.dataService.GetObjects<Evolution>(includes: "PreevolutionPokemon, PreevolutionPokemon.Game, EvolutionPokemon, EvolutionPokemon.Game, EvolutionMethod").Find(x => x.PreevolutionPokemonId == preEvolutionPokemonId && x.EvolutionPokemonId == evolutionPokemonId && x.GenerationId == generationId);
            EvolutionViewModel model = new EvolutionViewModel()
            {
                AllEvolutionMethods = this.dataService.GetObjects<EvolutionMethod>("Name"),
                Id = preEvolution.Id,
                EvolutionDetails = preEvolution.EvolutionDetails,
                EvolutionMethodId = preEvolution.EvolutionMethodId,
                EvolutionMethod = preEvolution.EvolutionMethod,
                PreevolutionPokemon = preEvolution.PreevolutionPokemon,
                PreevolutionPokemonId = preEvolution.PreevolutionPokemonId,
                EvolutionPokemonId = preEvolution.EvolutionPokemonId,
                EvolutionPokemon = preEvolution.EvolutionPokemon,
                GenerationId = preEvolution.GenerationId,
            };

            List<Pokemon> pokemonList = this.dataService.GetObjects<Pokemon>("PokedexNumber, Id", "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form");
            pokemonList.Where(x => x.IsAltForm).ForEach(x => x.Name = x.NameWithForm);

            model.AllPokemon = pokemonList;

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_evolution/{preevolutionPokemonId:int}/{evolutionPokemonId:int}/{generationId:int}")]
        public IActionResult Evolution(EvolutionViewModel evolution)
        {
            if (!this.ModelState.IsValid)
            {
                Evolution preEvolution = this.dataService.GetObjects<Evolution>(includes: "PreevolutionPokemon, PreevolutionPokemon.Game, EvolutionPokemon, EvolutionPokemon.Game, EvolutionMethod").Find(x => x.PreevolutionPokemonId == evolution.PreevolutionPokemonId && x.EvolutionPokemonId == evolution.EvolutionPokemonId && x.GenerationId == evolution.GenerationId);
                EvolutionViewModel model = new EvolutionViewModel()
                {
                    AllEvolutionMethods = this.dataService.GetObjects<EvolutionMethod>("Name"),
                    Id = evolution.Id,
                    EvolutionDetails = preEvolution.EvolutionDetails,
                    EvolutionMethodId = preEvolution.EvolutionMethodId,
                    EvolutionMethod = preEvolution.EvolutionMethod,
                    PreevolutionPokemon = preEvolution.PreevolutionPokemon,
                    PreevolutionPokemonId = preEvolution.PreevolutionPokemonId,
                    EvolutionPokemonId = preEvolution.EvolutionPokemonId,
                    EvolutionPokemon = preEvolution.EvolutionPokemon,
                    GenerationId = preEvolution.GenerationId,
                };

                List<Pokemon> pokemonList = this.dataService.GetObjects<Pokemon>("PokedexNumber, Id", "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form");
                pokemonList.Where(x => x.IsAltForm).ForEach(x => x.Name = x.NameWithForm);

                model.AllPokemon = pokemonList;

                return this.View(model);
            }

            this.dataService.UpdateObject(evolution);

            return this.RedirectToAction("Pokemon", "Owner");
        }

        [Route("edit_alternate_forms/{pokemonId:int}")]
        public IActionResult AltForms(int pokemonId)
        {
            Pokemon originalPokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, SpecialGrouping");
            List<Pokemon> pokemonList = this.dataService.GetObjects<Pokemon>(includes: "Game, Form, SpecialGrouping").Where(x => x.OriginalFormId == pokemonId).ToList();
            pokemonList.Where(x => x.IsAltForm).ForEach(x => x.Name = x.NameWithForm);

            AllAdminPokemonViewModel allPokemon = new AllAdminPokemonViewModel()
            {
                AllPokemon = pokemonList,
                AllAltForms = this.dataService.GetObjects<PokemonFormDetail>(includes: "AltFormPokemon, AltFormPokemon.Game, OriginalPokemon, OriginalPokemon.Game, Form"),
                AllEvolutions = this.dataService.GetObjects<Evolution>(includes: "PreevolutionPokemon, PreevolutionPokemon.Game, EvolutionPokemon, EvolutionPokemon.Game, EvolutionMethod"),
                AllTypings = this.dataService.GetObjects<PokemonTypeDetail>("PokemonId", "Pokemon, PrimaryType, SecondaryType"),
                AllAbilities = this.dataService.GetObjects<PokemonAbilityDetail>(includes: "Pokemon, PrimaryAbility, SecondaryAbility, HiddenAbility"),
                AllEggGroups = this.dataService.GetObjects<PokemonEggGroupDetail>(includes: "Pokemon, PrimaryEggGroup, SecondaryEggGroup"),
                AllBaseStats = this.dataService.GetObjects<BaseStat>(includes: "Pokemon"),
                AllEVYields = this.dataService.GetObjects<EVYield>(includes: "Pokemon"),
                AllPokemonCaptureRates = this.dataService.GetAllPokemonWithCaptureRates(),
                AllPokemonBaseHappinesses = this.dataService.GetAllPokemonWithBaseHappinesses(),
            };

            DropdownViewModel model = new DropdownViewModel()
            {
                AllPokemon = allPokemon,
                AppConfig = this.appConfig,
            };

            return this.View(model);
        }

        [HttpGet]
        [Route("edit_alternate_form/{pokemonId:int}")]
        public IActionResult AltFormsForm(int pokemonId)
        {
            Pokemon pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonId);
            AlternateFormsFormViewModel model = new AlternateFormsFormViewModel()
            {
                Pokemon = pokemon,
                AllForms = this.dataService.GetObjects<Form>("Name"),
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_alternate_form/{pokemonId:int}")]
        public IActionResult AltFormsForm(AlternateFormsFormViewModel altFormsForm)
        {
            Pokemon updatedPokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", altFormsForm.Pokemon.Id);
            if (!this.ModelState.IsValid)
            {
                AlternateFormsFormViewModel model = new AlternateFormsFormViewModel()
                {
                    Pokemon = updatedPokemon,
                    AllForms = this.dataService.GetObjects<Form>("Name"),
                };

                return this.View(model);
            }

            updatedPokemon.FormId = altFormsForm.Pokemon.FormId;
            this.dataService.UpdateObject(updatedPokemon);

            return this.RedirectToAction("AltForms", "Edit", new { pokemonId = updatedPokemon.OriginalFormId });
        }

        private PokemonCaptureRateDetail GetPokemonWithCaptureRatesFromGenerationId(int pokemonId, int generationId)
        {
            return this.dataService.GetObjects<PokemonCaptureRateDetail>(includes: "Pokemon, CaptureRate, Generation").First(x => x.Pokemon.Id == pokemonId && x.Generation.Id == generationId);
        }

        private PokemonBaseHappinessDetail GetPokemonWithBaseHappinesssFromGenerationId(int pokemonId, int generationId)
        {
            return this.dataService.GetObjects<PokemonBaseHappinessDetail>(includes: "Pokemon, BaseHappiness, Generation").First(x => x.Pokemon.Id == pokemonId && x.Generation.Id == generationId);
        }
    }
}

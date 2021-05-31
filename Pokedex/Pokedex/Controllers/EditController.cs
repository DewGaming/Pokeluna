using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pokedex.DataAccess.Models;
using Pokedex.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Pokedex.Controllers
{
    [Authorize(Roles = "Admin,Owner")]
    [Route("admin")]
    public class EditController : Controller
    {
        private readonly DataService dataService;

        private readonly AppConfig appConfig;

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
            User model = this.dataService.GetObjectById<User>(id);

            return this.View(model);
        }

        [Authorize(Roles = "Owner")]
        [HttpPost]
        [Route("edit_user/{id:int}")]
        public new IActionResult User(User user)
        {
            if (!this.ModelState.IsValid)
            {
                User model = this.dataService.GetUserWithUsername(user.Username);

                return this.View(model);
            }

            this.dataService.UpdateUser(user);

            return this.RedirectToAction("Users", "Owner");
        }

        [Route("edit_type_effectiveness/{id:int}/{genId:int}")]
        public IActionResult TypeEffectiveness(int id, int genId)
        {
            EditTypeChartViewModel model = new EditTypeChartViewModel()
            {
                TypeChart = this.dataService.GetTypeChartByDefendType(id, genId),
                Types = this.dataService.GetTypes().Where(x => x.GenerationId <= genId).ToList(),
                TypeId = id,
                GenerationId = genId,
            };

            return this.View(model);
        }

        [Route("game_availability/{pokemonId:int}")]
        public IActionResult PokemonGameDetails(int pokemonId)
        {
            Pokemon pokemon = this.dataService.GetPokemonById(pokemonId);
            if (this.dataService.CheckIfAltForm(pokemonId))
            {
                pokemon.Name = this.dataService.GetAltFormWithFormName(pokemon.Id).Name;
            }

            PokemonGameViewModel model = new PokemonGameViewModel()
            {
                Pokemon = pokemon,
                PokemonGameDetails = this.dataService.GetPokemonGameDetails(pokemonId),
                AllGames = this.dataService.GetGames().Where(x => x.ReleaseDate >= pokemon.Game.ReleaseDate).ToList(),
            };

            return this.View(model);
        }

        [HttpGet]
        [Route("edit_generation/{id:int}")]
        public IActionResult Generation(int id)
        {
            Generation model = this.dataService.GetObjectById<Generation>(id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_generation/{id:int}")]
        public IActionResult Generation(Generation generation)
        {
            if (!this.ModelState.IsValid)
            {
                Generation model = this.dataService.GetObjectById<Generation>(generation.Id);

                return this.View(model);
            }

            this.dataService.UpdateGeneration(generation);

            return this.RedirectToAction("Generations", "Admin");
        }

        [HttpGet]
        [Route("edit_game/{id:int}")]
        public IActionResult Game(int id)
        {
            Game game = this.dataService.GetObjectById<Game>(id);
            GameViewModel model = new GameViewModel()
            {
                Id = game.Id,
                Name = game.Name,
                Abbreviation = game.Abbreviation,
                ReleaseDate = game.ReleaseDate,
                GenerationId = game.GenerationId,
                AllGenerations = this.dataService.GetObjects<Generation>(),
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_game/{id:int}")]
        public IActionResult Game(Game game)
        {
            if (!this.ModelState.IsValid)
            {
                Game oldGame = this.dataService.GetObjectById<Game>(game.Id);
                GameViewModel model = new GameViewModel()
                {
                    Id = oldGame.Id,
                    Name = oldGame.Name,
                    Abbreviation = oldGame.Abbreviation,
                    ReleaseDate = oldGame.ReleaseDate,
                    GenerationId = oldGame.GenerationId,
                    AllGenerations = this.dataService.GetObjects<Generation>(),
                };

                return this.View(model);
            }

            this.dataService.UpdateGame(game);

            return this.RedirectToAction("Games", "Admin");
        }

        [Route("edit_game_availability/{id:int}")]
        public IActionResult GameAvailability(int id)
        {
            Game game = this.dataService.GetObjectById<Game>(id);
            List<Pokemon> pokemonList = this.dataService.GetAllPokemonWithIncompleteWithFormNames().Where(x => x.Game.ReleaseDate <= game.ReleaseDate).ToList();
            EditGameAvailabilityViewModel model = new EditGameAvailabilityViewModel()
            {
                Game = game,
                Games = this.dataService.GetGames(),
                PokemonList = pokemonList,
                GameAvailability = this.dataService.GetAllPokemonGameDetails(),
            };

            return this.View(model);
        }

        [HttpGet]
        [Route("edit_battle_item/{id:int}")]
        public IActionResult BattleItem(int id)
        {
            List<Pokemon> pokemonList = this.dataService.GetAllPokemon();
            List<Pokemon> altFormsList = this.dataService.GetAllAltForms().ConvertAll(x => x.AltFormPokemon);
            foreach (var p in pokemonList.Where(x => altFormsList.Any(y => y.Id == x.Id)))
            {
                p.Name = this.dataService.GetAltFormWithFormName(p.Id).Name;
            }

            BattleItem battleItem = this.dataService.GetObjectById<BattleItem>(id);
            BattleItemViewModel model = new BattleItemViewModel()
            {
                Id = battleItem.Id,
                Name = battleItem.Name,
                GenerationId = battleItem.GenerationId,
                AllGenerations = this.dataService.GetObjects<Generation>().Where(x => x.Id >= 1).ToList(),
                AllPokemon = pokemonList,
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_battle_item/{id:int}")]
        public IActionResult BattleItem(BattleItem battleItem)
        {
            if (!this.ModelState.IsValid)
            {
                List<Pokemon> pokemonList = this.dataService.GetAllPokemon();
                List<Pokemon> altFormsList = this.dataService.GetAllAltForms().ConvertAll(x => x.AltFormPokemon);
                foreach (var p in pokemonList.Where(x => altFormsList.Any(y => y.Id == x.Id)))
                {
                    p.Name = this.dataService.GetAltFormWithFormName(p.Id).Name;
                }

                BattleItem item = this.dataService.GetObjectById<BattleItem>(battleItem.Id);
                BattleItemViewModel model = new BattleItemViewModel()
                {
                    Id = item.Id,
                    Name = item.Name,
                    GenerationId = item.GenerationId,
                    AllGenerations = this.dataService.GetObjects<Generation>().Where(x => x.Id >= 1).ToList(),
                    AllPokemon = pokemonList,
                };

                return this.View(model);
            }

            this.dataService.UpdateBattleItem(battleItem);

            return this.RedirectToAction("BattleItems", "Admin");
        }

        [HttpGet]
        [Route("edit_pokemon/{id:int}")]
        public IActionResult Pokemon(int id)
        {
            BasePokemonViewModel model = new BasePokemonViewModel()
            {
                Pokemon = this.dataService.GetPokemonById(id),
                AllGames = this.dataService.GetGames(),
                AllClassifications = this.dataService.GetClassifications(),
            };

            model.AllBaseHappinesses = this.dataService.GetBaseHappinesses();
            model.AllCaptureRates = this.dataService.GetCaptureRates();
            model.AllEggCycles = this.dataService.GetEggCycles();
            model.AllExperienceGrowths = this.dataService.GetExperienceGrowths();
            model.AllGenderRatios = new List<GenderRatioViewModel>();

            foreach (GenderRatio genderRatio in this.dataService.GetObjects<GenderRatio>())
            {
                GenderRatioViewModel viewModel = new GenderRatioViewModel()
                {
                    Id = genderRatio.Id,
                };

                if (genderRatio.MaleRatio == genderRatio.FemaleRatio && genderRatio.MaleRatio == 0)
                {
                    viewModel.GenderRatioString = "Genderless";
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
            }

            if (this.dataService.CheckIfAltForm(id))
            {
                model.Name = string.Concat(model.Pokemon.Name, " (", this.dataService.GetPokemonFormName(id), ")");
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
                    Pokemon = this.dataService.GetPokemonById(pokemon.Id),
                    AllGames = this.dataService.GetGames(),
                };

                if (!this.dataService.CheckIfAltForm(pokemon.Id))
                {
                    model.AllBaseHappinesses = this.dataService.GetBaseHappinesses();
                    model.AllClassifications = this.dataService.GetClassifications();
                    model.AllCaptureRates = this.dataService.GetCaptureRates();
                    model.AllEggCycles = this.dataService.GetEggCycles();
                    model.AllExperienceGrowths = this.dataService.GetExperienceGrowths();
                    model.AllGenderRatios = new List<GenderRatioViewModel>();

                    foreach (GenderRatio genderRatio in this.dataService.GetObjects<GenderRatio>())
                    {
                        GenderRatioViewModel viewModel = new GenderRatioViewModel()
                        {
                            Id = genderRatio.Id,
                        };

                        if (genderRatio.MaleRatio == genderRatio.FemaleRatio && genderRatio.MaleRatio == 0)
                        {
                            viewModel.GenderRatioString = "Genderless";
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
                    }
                }
                else
                {
                    model.Name = string.Concat(model.Pokemon.Name, " (", this.dataService.GetPokemonFormName(pokemon.Id), ")");
                }

                return this.View(model);
            }
            else if (this.dataService.GetPokemonByPokedexNumber(pokemon.PokedexNumber) != null && this.dataService.GetPokemonById(pokemon.Id).PokedexNumber != pokemon.PokedexNumber)
            {
                BasePokemonViewModel model = new BasePokemonViewModel()
                {
                    Pokemon = this.dataService.GetPokemonById(pokemon.Id),
                    AllGames = this.dataService.GetGames(),
                };

                if (!this.dataService.CheckIfAltForm(pokemon.Id))
                {
                    model.AllBaseHappinesses = this.dataService.GetBaseHappinesses();
                    model.AllClassifications = this.dataService.GetClassifications();
                    model.AllCaptureRates = this.dataService.GetCaptureRates();
                    model.AllEggCycles = this.dataService.GetEggCycles();
                    model.AllExperienceGrowths = this.dataService.GetExperienceGrowths();
                    model.AllGenderRatios = new List<GenderRatioViewModel>();

                    foreach (GenderRatio genderRatio in this.dataService.GetObjects<GenderRatio>())
                    {
                        GenderRatioViewModel viewModel = new GenderRatioViewModel()
                        {
                            Id = genderRatio.Id,
                        };

                        if (genderRatio.MaleRatio == genderRatio.FemaleRatio && genderRatio.MaleRatio == 0)
                        {
                            viewModel.GenderRatioString = "Genderless";
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
                    }
                }
                else
                {
                    model.Name = string.Concat(model.Pokemon.Name, " (", this.dataService.GetPokemonFormName(pokemon.Id), ")");
                }

                this.ModelState.AddModelError("PokedexNumber", "A pokemon with a this pokedex number already exists.");

                return this.View(model);
            }

            Pokemon oldPokemon = this.dataService.GetPokemonById(pokemon.Id);
            List<Pokemon> altForms = this.dataService.GetAltFormsNoIncludes(pokemon.Id);
            if (oldPokemon.PokedexNumber != pokemon.PokedexNumber)
            {
                foreach (var p in altForms)
                {
                    if (p.PokedexNumber != pokemon.PokedexNumber)
                    {
                        p.PokedexNumber = pokemon.PokedexNumber;
                        this.dataService.UpdatePokemon(p);
                    }
                }
            }

            this.dataService.UpdatePokemon(pokemon);

            return this.RedirectToAction("Pokemon", "Admin");
        }

        [HttpGet]
        [Route("edit_pokemon_image/{id:int}")]
        public IActionResult PokemonImage(int id)
        {
            Pokemon model = this.dataService.GetPokemonById(id);

            if (this.dataService.CheckIfAltForm(id))
            {
                model.Name = string.Concat(model.Name, " (", this.dataService.GetPokemonFormName(id), ")");
            }

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_pokemon_image/{id:int}")]
        public async Task<IActionResult> PokemonImage(Pokemon pokemon, int id, IFormFile fileUpload, string urlUpload)
        {
            if (!this.ModelState.IsValid && pokemon.Name.Length <= 25)
            {
                Pokemon model = this.dataService.GetPokemonById(id);

                if (this.dataService.CheckIfAltForm(id))
                {
                    model.Name = string.Concat(model.Name, " (", this.dataService.GetPokemonFormName(id), ")");
                }

                return this.View(model);
            }
            else if (fileUpload == null && string.IsNullOrEmpty(urlUpload))
            {
                Pokemon model = this.dataService.GetPokemonById(id);

                if (this.dataService.CheckIfAltForm(id))
                {
                    model.Name = string.Concat(model.Name, " (", this.dataService.GetPokemonFormName(id), ")");
                }

                this.ModelState.AddModelError("Picture", "An image is needed to update.");
                return this.View(model);
            }
            else if ((fileUpload?.FileName.Contains(".png") == false) || (!string.IsNullOrEmpty(urlUpload) && !urlUpload.Contains(".png")))
            {
                Pokemon model = this.dataService.GetPokemonById(id);

                if (this.dataService.CheckIfAltForm(id))
                {
                    model.Name = string.Concat(model.Name, " (", this.dataService.GetPokemonFormName(id), ")");
                }

                this.ModelState.AddModelError("Picture", "The image must be in the .png format.");
                return this.View(model);
            }

            IFormFile upload;

            if (fileUpload == null)
            {
                WebRequest webRequest = WebRequest.CreateHttp(urlUpload);

                using WebResponse webResponse = webRequest.GetResponse();
                Stream stream = webResponse.GetResponseStream();
                MemoryStream memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);

                upload = new FormFile(memoryStream, 0, memoryStream.Length, "image", "image.png");
            }
            else
            {
                upload = fileUpload;
            }

            upload = this.dataService.TrimImage(upload);

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(string.Concat(this.appConfig.FTPUrl, this.appConfig.PokemonImageFTPUrl, id.ToString(), ".png"));
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(this.appConfig.FTPUsername, this.appConfig.FTPPassword);

            using (Stream requestStream = request.GetRequestStream())
            {
                await upload.CopyToAsync(requestStream).ConfigureAwait(false);
            }

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                System.Console.WriteLine($"Upload File Complete, status {response.StatusDescription}");
            }

            upload = this.dataService.FormatFavIcon(upload);

            request = (FtpWebRequest)WebRequest.Create(string.Concat(this.appConfig.FTPUrl, this.appConfig.FaviconImageFTPUrl, id.ToString(), ".png"));
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(this.appConfig.FTPUsername, this.appConfig.FTPPassword);

            using (Stream requestStream = request.GetRequestStream())
            {
                await upload.CopyToAsync(requestStream).ConfigureAwait(false);
            }

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                System.Console.WriteLine($"Upload File Complete, status {response.StatusDescription}");
            }

            return this.RedirectToAction("Pokemon", "Admin");
        }

        [HttpGet]
        [Route("edit_shiny_pokemon_image/{id:int}")]
        public IActionResult ShinyPokemonImage(int id)
        {
            Pokemon model = this.dataService.GetPokemonById(id);

            if (this.dataService.CheckIfAltForm(id))
            {
                model.Name = string.Concat(model.Name, " (", this.dataService.GetPokemonFormName(id), ")");
            }

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_shiny_pokemon_image/{id:int}")]
        public async Task<IActionResult> ShinyPokemonImage(Pokemon pokemon, int id, IFormFile fileUpload, string urlUpload)
        {
            if (!this.ModelState.IsValid && pokemon.Name.Length <= 25)
            {
                Pokemon model = this.dataService.GetPokemonById(id);

                if (this.dataService.CheckIfAltForm(id))
                {
                    model.Name = string.Concat(model.Name, " (", this.dataService.GetPokemonFormName(id), ")");
                }

                return this.View(model);
            }
            else if (fileUpload == null && string.IsNullOrEmpty(urlUpload))
            {
                Pokemon model = this.dataService.GetPokemonById(id);

                if (this.dataService.CheckIfAltForm(id))
                {
                    model.Name = string.Concat(model.Name, " (", this.dataService.GetPokemonFormName(id), ")");
                }

                this.ModelState.AddModelError("Picture", "An image is needed to update.");
                return this.View(model);
            }
            else if ((fileUpload?.FileName.Contains(".png") == false) || (!string.IsNullOrEmpty(urlUpload) && !urlUpload.Contains(".png")))
            {
                Pokemon model = this.dataService.GetPokemonById(id);

                if (this.dataService.CheckIfAltForm(id))
                {
                    model.Name = string.Concat(model.Name, " (", this.dataService.GetPokemonFormName(id), ")");
                }

                this.ModelState.AddModelError("Picture", "The image must be in the .png format.");
                return this.View(model);
            }

            IFormFile upload;

            if (fileUpload == null)
            {
                WebRequest webRequest = WebRequest.CreateHttp(urlUpload);

                using WebResponse webResponse = webRequest.GetResponse();
                Stream stream = webResponse.GetResponseStream();
                MemoryStream memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);

                upload = new FormFile(memoryStream, 0, memoryStream.Length, "image", "image.png");
            }
            else
            {
                upload = fileUpload;
            }

            upload = this.dataService.TrimImage(upload);

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(string.Concat(this.appConfig.FTPUrl, this.appConfig.ShinyPokemonImageFTPUrl, id.ToString(), ".png"));
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(this.appConfig.FTPUsername, this.appConfig.FTPPassword);

            using (Stream requestStream = request.GetRequestStream())
            {
                await upload.CopyToAsync(requestStream).ConfigureAwait(false);
            }

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                System.Console.WriteLine($"Upload File Complete, status {response.StatusDescription}");
            }

            return this.RedirectToAction("Pokemon", "Admin");
        }

        [HttpGet]
        [Route("edit_typing/{pokemonId:int}/{generationId:int}")]
        public IActionResult Typing(int pokemonId, int generationId)
        {
            PokemonTypeDetail typeDetail = this.dataService.GetPokemonWithTypes(pokemonId).Find(x => x.GenerationId == generationId);
            PokemonTypingViewModel model = new PokemonTypingViewModel()
            {
                Id = typeDetail.Id,
                AllTypes = this.dataService.GetTypes(),
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
                PokemonTypeDetail typeDetail = this.dataService.GetPokemonWithTypes(pokemonTypeDetail.PokemonId).Find(x => x.GenerationId == pokemonTypeDetail.GenerationId);
                PokemonTypingViewModel model = new PokemonTypingViewModel()
                {
                    Id = typeDetail.Id,
                    AllTypes = this.dataService.GetTypes(),
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

            this.dataService.UpdatePokemonTypeDetail(pokemonTypeDetail);

            return this.RedirectToAction("Pokemon", "Admin");
        }

        [HttpGet]
        [Route("edit_abilities/{pokemonId:int}/{generationId:int}")]
        public IActionResult Abilities(int pokemonId, int generationId)
        {
            PokemonAbilityDetail abilityDetail = this.dataService.GetPokemonWithAbilities(pokemonId).Find(x => x.GenerationId == generationId);
            PokemonAbilitiesViewModel model = new PokemonAbilitiesViewModel()
            {
                Id = abilityDetail.Id,
                AllAbilities = this.dataService.GetAbilities(),
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
                PokemonAbilityDetail abilityDetail = this.dataService.GetPokemonWithAbilities(pokemonAbilityDetail.PokemonId).Find(x => x.GenerationId == pokemonAbilityDetail.GenerationId);
                PokemonAbilitiesViewModel model = new PokemonAbilitiesViewModel()
                {
                    Id = abilityDetail.Id,
                    AllAbilities = this.dataService.GetAbilities(),
                    PokemonId = abilityDetail.PokemonId,
                    Pokemon = abilityDetail.Pokemon,
                    PrimaryAbilityId = abilityDetail.PrimaryAbilityId,
                    SecondaryAbilityId = abilityDetail.SecondaryAbilityId,
                    HiddenAbilityId = abilityDetail.HiddenAbilityId,
                    GenerationId = abilityDetail.GenerationId,
                };

                return this.View(model);
            }

            this.dataService.UpdatePokemonAbilityDetail(pokemonAbilityDetail);

            return this.RedirectToAction("Pokemon", "Admin");
        }

        [HttpGet]
        [Route("edit_egg_groups/{pokemonId:int}/{generationId:int}")]
        public IActionResult EggGroups(int pokemonId, int generationId)
        {
            PokemonEggGroupDetail eggGroupDetail = this.dataService.GetPokemonWithEggGroups(pokemonId).Find(x => x.GenerationId == generationId);
            PokemonEggGroupsViewModel model = new PokemonEggGroupsViewModel()
            {
                Id = eggGroupDetail.Id,
                AllEggGroups = this.dataService.GetEggGroups(),
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
                PokemonEggGroupDetail eggGroupDetail = this.dataService.GetPokemonWithEggGroups(pokemonEggGroupDetail.PokemonId).Find(x => x.GenerationId == pokemonEggGroupDetail.GenerationId);
                PokemonEggGroupsViewModel model = new PokemonEggGroupsViewModel()
                {
                    Id = eggGroupDetail.Id,
                    AllEggGroups = this.dataService.GetEggGroups(),
                    PokemonId = eggGroupDetail.PokemonId,
                    Pokemon = eggGroupDetail.Pokemon,
                    PrimaryEggGroupId = eggGroupDetail.PrimaryEggGroupId,
                    SecondaryEggGroupId = eggGroupDetail.SecondaryEggGroupId,
                    GenerationId = eggGroupDetail.GenerationId,
                };

                return this.View(model);
            }

            this.dataService.UpdatePokemonEggGroupDetail(pokemonEggGroupDetail);

            return this.RedirectToAction("Pokemon", "Admin");
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

            this.dataService.UpdateBaseStat(baseStat);

            return this.RedirectToAction("Pokemon", "Admin");
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

            this.dataService.UpdateEVYield(evYield);

            return this.RedirectToAction("Pokemon", "Admin");
        }

        [HttpGet]
        [Route("edit_type/{id:int}")]
        public IActionResult Type(int id)
        {
            Type type = this.dataService.GetObjectById<Type>(id);
            TypeGenerationViewModel model = new TypeGenerationViewModel()
            {
                Name = type.Name,
                GenerationId = type.GenerationId,
                AllGenerations = this.dataService.GetGenerationsForRazor(),
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
                Type newType = this.dataService.GetObjectById<Type>(type.Id);
                TypeGenerationViewModel model = new TypeGenerationViewModel()
                {
                    Name = newType.Name,
                    GenerationId = newType.GenerationId,
                    AllGenerations = this.dataService.GetGenerationsForRazor(),
                };

                return this.View(model);
            }

            this.dataService.UpdateType(type);

            return this.RedirectToAction("Types", "Admin");
        }

        [HttpGet]
        [Route("edit_egg_cycle/{id:int}")]
        public IActionResult EggCycle(int id)
        {
            EggCycle model = this.dataService.GetObjectById<EggCycle>(id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_egg_cycle/{id:int}")]
        public IActionResult EggCycle(EggCycle eggCycle)
        {
            if (!this.ModelState.IsValid)
            {
                EggCycle model = this.dataService.GetObjectById<EggCycle>(eggCycle.Id);

                return this.View(model);
            }

            this.dataService.UpdateEggCycle(eggCycle);

            return this.RedirectToAction("EggCycles", "Admin");
        }

        [HttpGet]
        [Route("edit_experience_growth/{id:int}")]
        public IActionResult ExperienceGrowth(int id)
        {
            ExperienceGrowth model = this.dataService.GetObjectById<ExperienceGrowth>(id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_experience_growth/{id:int}")]
        public IActionResult ExperienceGrowth(ExperienceGrowth experienceGrowth)
        {
            if (!this.ModelState.IsValid)
            {
                ExperienceGrowth model = this.dataService.GetObjectById<ExperienceGrowth>(experienceGrowth.Id);

                return this.View(model);
            }

            this.dataService.UpdateExperienceGrowth(experienceGrowth);

            return this.RedirectToAction("ExperienceGrowths", "Admin");
        }

        [HttpGet]
        [Route("edit_gender_ratio/{id:int}")]
        public IActionResult GenderRatio(int id)
        {
            GenderRatio model = this.dataService.GetObjectById<GenderRatio>(id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_gender_ratio/{id:int}")]
        public IActionResult GenderRatio(GenderRatio genderRatio)
        {
            if (!this.ModelState.IsValid)
            {
                GenderRatio model = this.dataService.GetObjectById<GenderRatio>(genderRatio.Id);

                return this.View(model);
            }

            this.dataService.UpdateGenderRatio(genderRatio);

            return this.RedirectToAction("GenderRatios", "Admin");
        }

        [HttpGet]
        [Route("edit_egg_group/{id:int}")]
        public IActionResult EggGroup(int id)
        {
            EggGroup model = this.dataService.GetObjectById<EggGroup>(id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_egg_group/{id:int}")]
        public IActionResult EggGroup(EggGroup eggGroup)
        {
            if (!this.ModelState.IsValid)
            {
                EggGroup model = this.dataService.GetObjectById<EggGroup>(eggGroup.Id);

                return this.View(model);
            }

            this.dataService.UpdateEggGroup(eggGroup);

            return this.RedirectToAction("EggGroups", "Admin");
        }

        [HttpGet]
        [Route("edit_form_item/{id:int}")]
        public IActionResult FormItem(int id)
        {
            FormItem item = this.dataService.GetObjectById<FormItem>(id);
            FormItemViewModel model = new FormItemViewModel()
            {
                PokemonId = item.PokemonId,
                Name = item.Name,
            };

            List<Pokemon> altForms = this.dataService.GetPokemonFormDetails().Where(x => x.Form.NeedsItem).ToList().ConvertAll(x => x.AltFormPokemon);

            altForms.Remove(altForms.Find(x => x.Name == "Rayquaza"));

            foreach (var p in altForms)
            {
                p.Name = string.Concat(p.Name, " (", this.dataService.GetPokemonFormName(p.Id), ")");
            }

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
                FormItem item = this.dataService.GetObjectById<FormItem>(formItem.Id);
                FormItemViewModel model = new FormItemViewModel()
                {
                    PokemonId = item.PokemonId,
                    Name = item.Name,
                };

                List<Pokemon> altForms = this.dataService.GetPokemonFormDetails().Where(x => x.Form.NeedsItem).ToList().ConvertAll(x => x.AltFormPokemon);

                altForms.Remove(altForms.Find(x => x.Name == "Rayquaza"));

                foreach (var p in altForms)
                {
                    p.Name = string.Concat(p.Name, " (", this.dataService.GetPokemonFormName(p.Id), ")");
                }

                model.AllPokemon = altForms;

                return this.View(model);
            }

            this.dataService.UpdateFormItem(formItem);

            return this.RedirectToAction("FormItems", "Admin");
        }

        [HttpGet]
        [Route("edit_form/{id:int}")]
        public IActionResult Form(int id)
        {
            Form model = this.dataService.GetObjectById<Form>(id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_form/{id:int}")]
        public IActionResult Form(Form form)
        {
            if (!this.ModelState.IsValid)
            {
                Form model = this.dataService.GetObjectById<Form>(form.Id);

                return this.View(model);
            }

            this.dataService.UpdateForm(form);

            return this.RedirectToAction("Forms", "Admin");
        }

        [HttpGet]
        [Route("edit_evolution_method/{id:int}")]
        public IActionResult EvolutionMethod(int id)
        {
            EvolutionMethod model = this.dataService.GetObjectById<EvolutionMethod>(id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_evolution_method/{id:int}")]
        public IActionResult EvolutionMethod(EvolutionMethod evolutionMethod)
        {
            if (!this.ModelState.IsValid)
            {
                EvolutionMethod model = this.dataService.GetObjectById<EvolutionMethod>(evolutionMethod.Id);

                return this.View(model);
            }

            this.dataService.UpdateEvolutionMethod(evolutionMethod);

            return this.RedirectToAction("EvolutionMethods", "Admin");
        }

        [HttpGet]
        [Route("edit_capture_rate/{id:int}")]
        public IActionResult CaptureRate(int id)
        {
            CaptureRate model = this.dataService.GetObjectById<CaptureRate>(id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_capture_rate/{id:int}")]
        public IActionResult CaptureRate(CaptureRate captureRate)
        {
            if (!this.ModelState.IsValid)
            {
                CaptureRate model = this.dataService.GetObjectById<CaptureRate>(captureRate.Id);

                return this.View(model);
            }

            this.dataService.UpdateCaptureRate(captureRate);

            return this.RedirectToAction("CaptureRates", "Admin");
        }

        [HttpGet]
        [Route("edit_base_happiness/{id:int}")]
        public IActionResult BaseHappiness(int id)
        {
            BaseHappiness model = this.dataService.GetObjectById<BaseHappiness>(id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_base_happiness/{id:int}")]
        public IActionResult BaseHappiness(BaseHappiness baseHappiness)
        {
            if (!this.ModelState.IsValid)
            {
                BaseHappiness model = this.dataService.GetObjectById<BaseHappiness>(baseHappiness.Id);

                return this.View(model);
            }

            this.dataService.UpdateBaseHappiness(baseHappiness);

            return this.RedirectToAction("BaseHappinesses", "Admin");
        }

        [HttpGet]
        [Route("edit_pokeball/{id:int}")]
        public IActionResult Pokeball(int id)
        {
            Pokeball pokeball = this.dataService.GetPokeball(id);
            PokeballAdminViewModel model = new PokeballAdminViewModel()
            {
                AllGenerations = this.dataService.GetObjects<Generation>(),
                Id = pokeball.Id,
                Name = pokeball.Name,
                GenerationId = pokeball.GenerationId,
                Generation = pokeball.Generation,
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
                Pokeball newPokeball = this.dataService.GetPokeball(pokeball.Id);
                PokeballAdminViewModel model = new PokeballAdminViewModel()
                {
                    AllGenerations = this.dataService.GetObjects<Generation>(),
                    Id = newPokeball.Id,
                    Name = newPokeball.Name,
                    GenerationId = newPokeball.GenerationId,
                    Generation = newPokeball.Generation,
                };

                return this.View(model);
            }

            this.dataService.UpdatePokeball(pokeball);

            return this.RedirectToAction("Pokeballs", "Admin");
        }

        [HttpGet]
        [Route("edit_status/{id:int}")]
        public IActionResult Status(int id)
        {
            Status model = this.dataService.GetObjectById<Status>(id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_status/{id:int}")]
        public IActionResult Status(Status status)
        {
            if (!this.ModelState.IsValid)
            {
                Status model = this.dataService.GetObjectById<Status>(status.Id);

                return this.View(model);
            }

            this.dataService.UpdateStatus(status);

            return this.RedirectToAction("Statuses", "Admin");
        }

        [HttpGet]
        [Route("edit_pokemon_capture_rate/{pokemonId:int}/{generationId:int}")]
        public IActionResult CaptureRates(int pokemonId, int generationId)
        {
            PokemonCaptureRateDetail captureRate = this.dataService.GetPokemonWithCaptureRatesFromGenerationId(pokemonId, generationId);
            PokemonCaptureRateViewModel model = new PokemonCaptureRateViewModel()
            {
                Id = captureRate.Id,
                PokemonId = captureRate.PokemonId,
                GenerationId = captureRate.GenerationId,
                AllCaptureRates = this.dataService.GetCaptureRates(),
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
                PokemonCaptureRateDetail captureRate = this.dataService.GetPokemonWithCaptureRatesFromGenerationId(pokemonCaptureRate.PokemonId, pokemonCaptureRate.GenerationId);
                PokemonCaptureRateViewModel model = new PokemonCaptureRateViewModel()
                {
                    Id = captureRate.Id,
                    PokemonId = captureRate.PokemonId,
                    GenerationId = captureRate.GenerationId,
                    AllCaptureRates = this.dataService.GetCaptureRates(),
                };

                return this.View(model);
            }

            this.dataService.UpdatePokemonCaptureRateDetail(pokemonCaptureRate);

            return this.RedirectToAction("Pokemon", "Admin");
        }

        [HttpGet]
        [Route("edit_pokeball_catch_modifier_detail/{id:int}")]
        public IActionResult PokeballCatchModifierDetail(int id)
        {
            PokeballCatchModifierDetail model = this.dataService.GetPokeballCatchModifierDetail(id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_pokeball_catch_modifier_detail/{id:int}")]
        public IActionResult PokeballCatchModifierDetail(PokeballCatchModifierDetail pokeballCatchModifierDetail)
        {
            if (!this.ModelState.IsValid)
            {
                PokeballCatchModifierDetail model = this.dataService.GetPokeballCatchModifierDetail(pokeballCatchModifierDetail.Id);

                return this.View(model);
            }

            this.dataService.UpdatePokeballCatchModifierDetail(pokeballCatchModifierDetail);

            return this.RedirectToAction("Pokeballs", "Admin");
        }

        [Route("pokeball_catch_modifier/{pokeballId:int}")]
        public IActionResult PokeballCatchModifiers(int pokeballId)
        {
            PokeballViewModel model = new PokeballViewModel()
            {
                PokeballId = pokeballId,
                AllPokeballs = this.dataService.GetPokeballs(),
                AllCatchModifiers = this.dataService.GetPokeballCatchModifierDetails(),
            };

            return this.View(model);
        }

        [HttpGet]
        [Route("edit_nature/{id:int}")]
        public IActionResult Nature(int id)
        {
            Nature model = this.dataService.GetObjectById<Nature>(id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_nature/{id:int}")]
        public IActionResult Nature(Nature nature)
        {
            if (!this.ModelState.IsValid)
            {
                Nature model = this.dataService.GetObjectById<Nature>(nature.Id);

                return this.View(model);
            }

            this.dataService.UpdateNature(nature);

            return this.RedirectToAction("Natures", "Admin");
        }

        [HttpGet]
        [Route("edit_legendary_type/{id:int}")]
        public IActionResult LegendaryType(int id)
        {
            LegendaryType model = this.dataService.GetObjectById<LegendaryType>(id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_legendary_type/{id:int}")]
        public IActionResult LegendaryType(LegendaryType legendaryType)
        {
            if (!this.ModelState.IsValid)
            {
                LegendaryType model = this.dataService.GetObjectById<LegendaryType>(legendaryType.Id);

                return this.View(model);
            }

            this.dataService.UpdateLegendaryType(legendaryType);

            return this.RedirectToAction("LegendaryTypes", "Admin");
        }

        [HttpGet]
        [Route("edit_classification/{id:int}")]
        public IActionResult Classification(int id)
        {
            Classification model = this.dataService.GetObjectById<Classification>(id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_classification/{id:int}")]
        public IActionResult Classification(Classification classification)
        {
            if (!this.ModelState.IsValid)
            {
                Classification model = this.dataService.GetObjectById<Classification>(classification.Id);

                return this.View(model);
            }

            if (classification.Name.Contains("pokemon"))
            {
                classification.Name = classification.Name.Replace("pokemon", "Pokemon");
            }
            else if (!classification.Name.Contains("Pokemon") && !classification.Name.Contains("pokemon"))
            {
                classification.Name = string.Concat(classification.Name.Trim(), " Pokemon");
            }

            this.dataService.UpdateClassification(classification);

            return this.RedirectToAction("Classifications", "Admin");
        }

        [HttpGet]
        [Route("edit_ability/{id:int}")]
        public IActionResult Ability(int id)
        {
            Ability model = this.dataService.GetObjectById<Ability>(id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_ability/{id:int}")]
        public IActionResult Ability(Ability ability)
        {
            if (!this.ModelState.IsValid)
            {
                Ability model = this.dataService.GetObjectById<Ability>(ability.Id);

                return this.View(model);
            }

            this.dataService.UpdateAbility(ability);

            return this.RedirectToAction("Abilities", "Admin");
        }

        [HttpGet]
        [Route("edit_comment_page/{id:int}")]
        public IActionResult CommentPage(int id)
        {
            CommentPage model = this.dataService.GetObjectById<CommentPage>(id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_comment_page/{id:int}")]
        public IActionResult CommentPage(CommentPage page)
        {
            if (!this.ModelState.IsValid)
            {
                CommentPage model = this.dataService.GetObjectById<CommentPage>(page.Id);

                return this.View(model);
            }

            this.dataService.UpdateCommentPage(page);

            return this.RedirectToAction("CommentPages", "Admin");
        }

        [HttpGet]
        [Route("edit_comment_category/{id:int}")]
        public IActionResult CommentCategory(int id)
        {
            CommentCategory model = this.dataService.GetObjectById<CommentCategory>(id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_comment_category/{id:int}")]
        public IActionResult CommentCategory(CommentCategory category)
        {
            if (!this.ModelState.IsValid)
            {
                CommentCategory model = this.dataService.GetObjectById<CommentCategory>(category.Id);

                return this.View(model);
            }

            this.dataService.UpdateCommentCategory(category);

            return this.RedirectToAction("CommentCategories", "Admin");
        }

        [HttpGet]
        [Route("edit_evolution/{pokemonId:int}")]
        public IActionResult Evolution(int pokemonId)
        {
            Evolution preEvolution = this.dataService.GetEvolutions().Find(x => x.EvolutionPokemonId == pokemonId);
            EvolutionViewModel model = new EvolutionViewModel()
            {
                AllEvolutionMethods = this.dataService.GetEvolutionMethods(),
                Id = preEvolution.Id,
                EvolutionDetails = preEvolution.EvolutionDetails,
                EvolutionMethodId = preEvolution.EvolutionMethodId,
                EvolutionMethod = preEvolution.EvolutionMethod,
                PreevolutionPokemon = preEvolution.PreevolutionPokemon,
                PreevolutionPokemonId = preEvolution.PreevolutionPokemonId,
                EvolutionPokemonId = preEvolution.EvolutionPokemonId,
                EvolutionPokemon = preEvolution.EvolutionPokemon,
            };

            List<Pokemon> pokemonList = this.dataService.GetAllPokemonIncludeIncomplete().Where(x => x.Id != pokemonId).ToList();
            List<Pokemon> altFormsList = this.dataService.GetAllAltForms().ConvertAll(x => x.AltFormPokemon);
            foreach (var pokemon in pokemonList.Where(x => altFormsList.Any(y => y.Id == x.Id)))
            {
                pokemon.Name = string.Concat(pokemon.Name, " (", this.dataService.GetPokemonFormName(pokemon.Id), ")");
            }

            model.AllPokemon = pokemonList;

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_evolution/{pokemonId:int}")]
        public IActionResult Evolution(EvolutionViewModel evolution)
        {
            if (!this.ModelState.IsValid)
            {
                Evolution preEvolution = this.dataService.GetEvolutions().Find(x => x.EvolutionPokemonId == evolution.EvolutionPokemonId);
                EvolutionViewModel model = new EvolutionViewModel()
                {
                    AllEvolutionMethods = this.dataService.GetEvolutionMethods(),
                    Id = evolution.Id,
                    EvolutionDetails = preEvolution.EvolutionDetails,
                    EvolutionMethodId = preEvolution.EvolutionMethodId,
                    EvolutionMethod = preEvolution.EvolutionMethod,
                    PreevolutionPokemon = preEvolution.PreevolutionPokemon,
                    PreevolutionPokemonId = preEvolution.PreevolutionPokemonId,
                    EvolutionPokemonId = preEvolution.EvolutionPokemonId,
                    EvolutionPokemon = preEvolution.EvolutionPokemon,
                };

                List<Pokemon> pokemonList = this.dataService.GetAllPokemon().Where(x => x.Id != evolution.EvolutionPokemonId).ToList();
                List<Pokemon> altFormsList = this.dataService.GetAllAltForms().ConvertAll(x => x.AltFormPokemon);
                foreach (var pokemon in pokemonList.Where(x => altFormsList.Any(y => y.Id == x.Id)))
                {
                    pokemon.Name = string.Concat(pokemon.Name, " (", this.dataService.GetPokemonFormName(pokemon.Id), ")");
                }

                model.AllPokemon = pokemonList;

                return this.View(model);
            }

            this.dataService.UpdateEvolution(evolution);

            return this.RedirectToAction("Pokemon", "Admin");
        }

        [Route("edit_alternate_forms/{pokemonId:int}")]
        public IActionResult AltForms(int pokemonId)
        {
            Pokemon originalPokemon = this.dataService.GetPokemonById(pokemonId);
            List<Pokemon> altFormList = this.dataService.GetAllAltForms().Select(x => x.AltFormPokemon).Where(x => x.Name == originalPokemon.Name).ToList();
            foreach (var pokemon in altFormList)
            {
                pokemon.Name = string.Concat(pokemon.Name, " (", this.dataService.GetPokemonFormName(pokemon.Id), ")");
            }

            AllAdminPokemonViewModel allPokemon = new AllAdminPokemonViewModel()
            {
                AllPokemon = altFormList,
                AllAltForms = this.dataService.GetAllAltForms(),
                AllEvolutions = this.dataService.GetEvolutions(),
                AllTypings = this.dataService.GetAllPokemonWithTypesAndIncomplete(),
                AllAbilities = this.dataService.GetAllPokemonWithAbilitiesAndIncomplete(),
                AllEggGroups = this.dataService.GetAllPokemonWithEggGroupsAndIncomplete(),
                AllBaseStats = this.dataService.GetBaseStatsWithIncomplete(),
                AllEVYields = this.dataService.GetEVYieldsWithIncomplete(),
                AllLegendaryDetails = this.dataService.GetAllPokemonWithLegendaryTypes(),
                AllPokemonCaptureRates = this.dataService.GetAllPokemonWithCaptureRates(),
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
            PokemonFormDetail pokemonForm = this.dataService.GetPokemonFormDetailByAltFormId(pokemonId);
            AlternateFormsFormViewModel model = new AlternateFormsFormViewModel()
            {
                PokemonFormDetail = pokemonForm,
                AllForms = this.dataService.GetForms(),
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit_alternate_form/{pokemonId:int}")]
        public IActionResult AltFormsForm(PokemonFormDetail pokemonFormDetail)
        {
            if (!this.ModelState.IsValid)
            {
                PokemonFormDetail pokemonForm = this.dataService.GetPokemonFormDetailByAltFormId(pokemonFormDetail.AltFormPokemonId);
                AlternateFormsFormViewModel model = new AlternateFormsFormViewModel()
                {
                    PokemonFormDetail = pokemonForm,
                    AllForms = this.dataService.GetForms(),
                };

                return this.View(model);
            }

            this.dataService.UpdatePokemonFormDetail(pokemonFormDetail);

            return this.RedirectToAction("AltForms", "Edit", new { pokemonId = pokemonFormDetail.OriginalPokemonId });
        }
    }
}

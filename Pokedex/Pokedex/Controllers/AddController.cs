using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MoreLinq;
using Pokedex.DataAccess.Models;
using Pokedex.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Pokedex.Controllers
{
    /// <summary>
    /// The class that handles all additions to the database.
    /// </summary>
    [Authorize(Roles = "Owner")]
    [Route("admin")]
    public class AddController : Controller
    {
        private readonly DataService dataService;

        private readonly AppConfig appConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddController"/> class.
        /// </summary>
        /// <param name="appConfig">The application's configuration.</param>
        /// <param name="dataContext">The pokemon database's context.</param>
        public AddController(IOptions<AppConfig> appConfig, DataContext dataContext)
        {
            // Instantiate an instance of the data service.
            this.appConfig = appConfig.Value;
            this.dataService = new DataService(dataContext);
        }

        /// <summary>
        /// Adds an evolution for a pokemon.
        /// </summary>
        /// <param name="pokemonId">The pokemon's Id.</param>
        /// <param name="generationId">The generation's Id.</param>
        /// <returns>The view to add the pokemon's evolution.</returns>
        [HttpGet]
        [Route("add_evolution/{pokemonId:int}/{generationId:int}")]
        public IActionResult Evolution(int pokemonId, int generationId)
        {
            Pokemon evolutionPokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth");
            EvolutionViewModel model = new EvolutionViewModel()
            {
                AllEvolutionMethods = this.dataService.GetObjects<EvolutionMethod>("Name"),
                EvolutionPokemon = evolutionPokemon,
                EvolutionPokemonId = evolutionPokemon.Id,
                GenerationId = generationId,
            };

            List<Pokemon> pokemonList = this.dataService.GetObjects<Pokemon>("PokedexNumber, Id", "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form").Where(x => x.Id != pokemonId).ToList();
            pokemonList.Where(x => x.IsAltForm).ForEach(x => x.Name = x.NameWithForm);
            model.AllPokemon = pokemonList;

            return this.View(model);
        }

        /// <summary>
        /// Adds an evolution for a pokemon.
        /// </summary>
        /// <param name="evolution">The generated evolution.</param>
        /// <returns>The admin pokemon view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_evolution/{pokemonId:int}/{generationId:int}")]
        public IActionResult Evolution(Evolution evolution)
        {
            if (!this.ModelState.IsValid)
            {
                EvolutionViewModel model = new EvolutionViewModel()
                {
                    AllEvolutionMethods = this.dataService.GetObjects<EvolutionMethod>("Name"),
                    EvolutionPokemon = evolution.EvolutionPokemon,
                    EvolutionPokemonId = evolution.EvolutionPokemon.Id,
                    GenerationId = evolution.GenerationId,
                };
                List<Pokemon> pokemonList = this.dataService.GetObjects<Pokemon>("PokedexNumber, Id", "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form").Where(x => x.Id != evolution.EvolutionPokemonId).ToList();
                pokemonList.Where(x => x.IsAltForm).ForEach(x => x.Name = x.NameWithForm);
                model.AllPokemon = pokemonList;

                return this.View(model);
            }

            this.dataService.AddObject(evolution);

            return this.RedirectToAction("ReviewPokemon", "Owner", new { pokemonId = evolution.EvolutionPokemonId });
        }

        /// <summary>
        /// Adds a generation.
        /// </summary>
        /// <returns>The view to add the generation.</returns>
        [Route("add_generation")]
        public IActionResult Generation()
        {
            this.dataService.AddObject(new DataAccess.Models.Generation());

            return this.RedirectToAction("Generations", "Owner");
        }

        /// <summary>
        /// Adds a form item.
        /// </summary>
        /// <returns>The view to add the form item.</returns>
        [HttpGet]
        [Route("add_form_item")]
        public IActionResult FormItem()
        {
            FormItemViewModel model = new FormItemViewModel();
            List<Pokemon> altForms = this.dataService.GetObjects<Pokemon>(includes: "Form").Where(x => x.IsAltForm && x.Form.NeedsItem).ToList();

            foreach (var p in this.dataService.GetFormItems().Select(x => x.Pokemon))
            {
                altForms.Remove(altForms.Find(x => x.PokedexNumber == p.PokedexNumber));
            }

            altForms.Remove(altForms.Find(x => x.Name == "Rayquaza"));
            altForms.ForEach(x => x.Name = x.NameWithForm);
            model.AllPokemon = altForms;

            return this.View(model);
        }

        /// <summary>
        /// Adds a form item.
        /// </summary>
        /// <param name="formItem">The form item being added.</param>
        /// <returns>The view to the form item admin page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_form_item")]
        public IActionResult FormItem(FormItem formItem)
        {
            if (!this.ModelState.IsValid)
            {
                List<Pokemon> pokemonList = this.dataService.GetObjects<Pokemon>(includes: "Game, Form").Where(x => x.IsAltForm).OrderBy(x => x.PokedexNumber).ToList();
                pokemonList.ForEach(x => x.Name = x.NameWithForm);

                FormItemViewModel model = new FormItemViewModel()
                {
                    AllPokemon = pokemonList,
                };

                return this.View(model);
            }

            this.dataService.AddObject(formItem);

            return this.RedirectToAction("FormItems", "Owner");
        }

        /// <summary>
        /// Adds a form group.
        /// </summary>
        /// <returns>The view to add the form group.</returns>
        [HttpGet]
        [Route("add_form_group")]
        public IActionResult FormGroup()
        {
            return this.View();
        }

        /// <summary>
        /// Adds a form group.
        /// </summary>
        /// <param name="formGroup">The form group being added.</param>
        /// <returns>The view to the form group admin page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_form_group")]
        public IActionResult FormGroup(FormGroup formGroup)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View();
            }

            this.dataService.AddObject(formGroup);

            return this.RedirectToAction("FormGroups", "Owner");
        }

        /// <summary>
        /// Adds a type.
        /// </summary>
        /// <returns>The view to add the type.</returns>
        [HttpGet]
        [Route("add_type")]
        public IActionResult Type()
        {
            TypeGenerationViewModel model = new TypeGenerationViewModel()
            {
                AllGenerations = this.dataService.GetObjects<Generation>(),
            };

            return this.View(model);
        }

        /// <summary>
        /// Adds a type.
        /// </summary>
        /// <param name="type">The type being added.</param>
        /// <returns>The view for the type admin page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_type")]
        public IActionResult Type(DataAccess.Models.Type type)
        {
            if (!this.ModelState.IsValid)
            {
                TypeGenerationViewModel model = new TypeGenerationViewModel()
                {
                    AllGenerations = this.dataService.GetObjects<Generation>(),
                };

                return this.View(model);
            }

            this.dataService.AddObject(type);

            return this.RedirectToAction("Types", "Owner");
        }

        [HttpGet]
        [Route("add_egg_cycle")]
        public IActionResult EggCycle()
        {
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_egg_cycle")]
        public IActionResult EggCycle(EggCycle eggCycle)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View();
            }

            this.dataService.AddObject(eggCycle);

            return this.RedirectToAction("EggCycles", "Owner");
        }

        [HttpGet]
        [Route("add_experience_growth")]
        public IActionResult ExperienceGrowth()
        {
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_experience_growth")]
        public IActionResult ExperienceGrowth(ExperienceGrowth experienceGrowth)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View();
            }

            this.dataService.AddObject(experienceGrowth);

            return this.RedirectToAction("ExperienceGrowths", "Owner");
        }

        [HttpGet]
        [Route("add_gender_ratio")]
        public IActionResult GenderRatio()
        {
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_gender_ratio")]
        public IActionResult GenderRatio(GenderRatio genderRatio)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View();
            }

            this.dataService.AddObject(genderRatio);

            return this.RedirectToAction("GenderRatios", "Owner");
        }

        [HttpGet]
        [Route("add_special_grouping")]
        public IActionResult SpecialGrouping()
        {
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_special_grouping")]
        public IActionResult SpecialGrouping(SpecialGrouping specialGrouping)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View();
            }

            this.dataService.AddObject(specialGrouping);

            return this.RedirectToAction("SpecialGroupings", "Owner");
        }

        [HttpGet]
        [Route("add_regional_dex")]
        public IActionResult RegionalDex()
        {
            RegionalDexViewModel model = new RegionalDexViewModel()
            {
                AllGames = this.dataService.GetGamesGroupedByReleaseDate(),
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_regional_dex")]
        public IActionResult RegionalDex(RegionalDex regionalDex)
        {
            if (!this.ModelState.IsValid)
            {
                RegionalDexViewModel model = new RegionalDexViewModel()
                {
                    AllGames = this.dataService.GetGamesGroupedByReleaseDate(),
                };

                return this.View(model);
            }

            this.dataService.AddObject(regionalDex);

            return this.RedirectToAction("RegionalDexes", "Owner");
        }

        [HttpGet]
        [Route("add_egg_group")]
        public IActionResult EggGroup()
        {
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_egg_group")]
        public IActionResult EggGroup(EggGroup eggGroup)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View();
            }

            this.dataService.AddObject(eggGroup);

            return this.RedirectToAction("EggGroups", "Owner");
        }

        [HttpGet]
        [Route("add_classification")]
        public IActionResult Classification()
        {
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_classification")]
        public IActionResult Classification(Classification classification)
        {
            List<Classification> classifications = this.dataService.GetObjects<Classification>();
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            classification.Name = textInfo.ToTitleCase(classification.Name);

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

            if (!this.ModelState.IsValid || classifications.Find(x => x.Name == classification.Name) != null)
            {
                return this.View();
            }

            this.dataService.AddObject(classification);

            return this.RedirectToAction("Classifications", "Owner");
        }

        [HttpGet]
        [Route("add_nature")]
        public IActionResult Nature()
        {
            NatureStatViewModel model = new NatureStatViewModel()
            {
                AllStats = this.dataService.GetObjects<Stat>(),
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_nature")]
        public IActionResult Nature(Nature nature)
        {
            if (!this.ModelState.IsValid)
            {
                NatureStatViewModel model = new NatureStatViewModel()
                {
                    AllStats = this.dataService.GetObjects<Stat>(),
                };

                return this.View(model);
            }

            this.dataService.AddObject(nature);

            return this.RedirectToAction("Natures", "Owner");
        }

        /// <summary>
        /// The page used to create a new stat in the database.
        /// </summary>
        /// <returns>The page to create the stat.</returns>
        [HttpGet]
        [Route("add_stat")]
        public IActionResult Stat()
        {
            return this.View();
        }

        /// <summary>
        /// The page used to create a new stat in the database.
        /// </summary>
        /// <param name="stat">The stat being craated.</param>
        /// <returns>The stats page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_stat")]
        public IActionResult Stat(Stat stat)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View();
            }

            this.dataService.AddObject(stat);

            return this.RedirectToAction("Stats", "Owner");
        }

        [HttpGet]
        [Route("add_pokeball")]
        public IActionResult Pokeball()
        {
            PokeballViewModel model = new PokeballViewModel()
            {
                AllGenerations = this.dataService.GetObjects<Generation>(),
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_pokeball")]
        public IActionResult Pokeball(Pokeball pokeball, IFormFile officialUpload, string officialUrlUpload)
        {
            if (!this.ModelState.IsValid)
            {
                PokeballViewModel model = new PokeballViewModel()
                {
                    AllGenerations = this.dataService.GetObjects<Generation>(),
                };

                return this.View(model);
            }

            this.dataService.AddObject(pokeball);

            this.dataService.UploadImages(officialUpload, officialUrlUpload, pokeball.Id.ToString(), this.appConfig, ImageType.Pokeball);

            return this.RedirectToAction("Pokeballs", "Owner");
        }

        [HttpGet]
        [Route("add_mark")]
        public IActionResult Mark()
        {
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_mark")]
        public IActionResult Mark(Mark mark, IFormFile officialUpload, string officialUrlUpload)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View();
            }

            this.dataService.AddObject(mark);

            this.dataService.UploadImages(officialUpload, officialUrlUpload, mark.Id.ToString(), this.appConfig, ImageType.Mark);

            return this.RedirectToAction("Marks", "Owner");
        }

        [HttpGet]
        [Route("add_sweet")]
        public IActionResult Sweet()
        {
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_sweet")]
        public IActionResult Sweet(Sweet sweet, IFormFile officialUpload, string officialUrlUpload)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View();
            }

            this.dataService.AddObject(sweet);

            this.dataService.UploadImages(officialUpload, officialUrlUpload, sweet.Id.ToString(), this.appConfig, ImageType.Sweet);

            return this.RedirectToAction("Sweets", "Owner");
        }

        [HttpGet]
        [Route("add_hunting_method")]
        public IActionResult HuntingMethod()
        {
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_hunting_method")]
        public IActionResult HuntingMethod(HuntingMethod huntingMethod)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View();
            }

            this.dataService.AddObject(huntingMethod);

            return this.RedirectToAction("HuntingMethods", "Owner");
        }

        [HttpGet]
        [Route("add_form")]
        public IActionResult Form()
        {
            FormModelViewModel model = new FormModelViewModel()
            {
                AllFormGroups = this.dataService.GetObjects<FormGroup>(),
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_form")]
        public IActionResult Form(Form form)
        {
            if (!this.ModelState.IsValid)
            {
                FormModelViewModel model = new FormModelViewModel()
                {
                    AllFormGroups = this.dataService.GetObjects<FormGroup>(),
                };

                return this.View(model);
            }

            this.dataService.AddObject(form);

            return this.RedirectToAction("Forms", "Owner");
        }

        [HttpGet]
        [Route("add_evolution_method")]
        public IActionResult EvolutionMethod()
        {
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_evolution_method")]
        public IActionResult EvolutionMethod(EvolutionMethod evolutionMethod)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View();
            }

            this.dataService.AddObject(evolutionMethod);

            return this.RedirectToAction("EvolutionMethods", "Owner");
        }

        [HttpGet]
        [Route("add_capture_rate")]
        public IActionResult CaptureRate()
        {
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_capture_rate")]
        public IActionResult CaptureRate(CaptureRate captureRate)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View();
            }

            this.dataService.AddObject(captureRate);

            return this.RedirectToAction("CaptureRates", "Owner");
        }

        [HttpGet]
        [Route("add_ability")]
        public IActionResult Ability()
        {
            AbilityAdminViewModel model = new AbilityAdminViewModel()
            {
                AllGenerations = this.dataService.GetObjects<Generation>(),
            };
            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_ability")]
        public IActionResult Ability(Ability ability)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View();
            }

            this.dataService.AddObject(ability);

            return this.RedirectToAction("Abilities", "Owner");
        }

        [HttpGet]
        [Route("add_base_happiness")]
        public IActionResult BaseHappiness()
        {
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_base_happiness")]
        public IActionResult BaseHappiness(BaseHappiness baseHappiness)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View();
            }

            this.dataService.AddObject(baseHappiness);

            return this.RedirectToAction("BaseHappinesses", "Owner");
        }

        [HttpGet]
        [Route("add_pokemon_capture_rate/{pokemonId:int}/{generationId:int}")]
        public IActionResult CaptureRates(int pokemonId, int generationId)
        {
            PokemonCaptureRateViewModel model = new PokemonCaptureRateViewModel()
            {
                PokemonId = pokemonId,
                GenerationId = generationId,
                AllCaptureRates = this.dataService.GetObjects<CaptureRate>("CatchRate"),
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_pokemon_capture_rate/{pokemonId:int}/{generationId:int}")]
        public IActionResult CaptureRates(PokemonCaptureRateDetail pokemonCaptureRate)
        {
            if (!this.ModelState.IsValid)
            {
                PokemonCaptureRateViewModel model = new PokemonCaptureRateViewModel()
                {
                    PokemonId = pokemonCaptureRate.PokemonId,
                    GenerationId = pokemonCaptureRate.GenerationId,
                    AllCaptureRates = this.dataService.GetObjects<CaptureRate>("CatchRate"),
                };

                return this.View(model);
            }

            this.dataService.AddObject(pokemonCaptureRate);

            if (this.dataService.GetObjects<PokemonEggGroupDetail>(includes: "Pokemon, PrimaryEggGroup, SecondaryEggGroup", whereProperty: "PokemonId", wherePropertyValue: pokemonCaptureRate.PokemonId).Count() == 0 && !this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonCaptureRate.PokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth").IsComplete)
            {
                return this.RedirectToAction("EggGroups", "Add", new { pokemonId = pokemonCaptureRate.PokemonId, generationId = pokemonCaptureRate.GenerationId });
            }
            else
            {
                return this.RedirectToAction("Pokemon", "Owner");
            }
        }

        [HttpGet]
        [Route("add_pokemon_base_happiness/{pokemonId:int}/{generationId:int}")]
        public IActionResult BaseHappinesses(int pokemonId, int generationId)
        {
            PokemonBaseHappinessViewModel model = new PokemonBaseHappinessViewModel()
            {
                PokemonId = pokemonId,
                GenerationId = generationId,
                AllBaseHappinesses = this.dataService.GetObjects<BaseHappiness>("Happiness"),
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_pokemon_base_happiness/{pokemonId:int}/{generationId:int}")]
        public IActionResult BaseHappinesses(PokemonBaseHappinessDetail pokemonBaseHappiness)
        {
            if (!this.ModelState.IsValid)
            {
                PokemonBaseHappinessViewModel model = new PokemonBaseHappinessViewModel()
                {
                    PokemonId = pokemonBaseHappiness.PokemonId,
                    GenerationId = pokemonBaseHappiness.GenerationId,
                    AllBaseHappinesses = this.dataService.GetObjects<BaseHappiness>("Happiness"),
                };

                return this.View(model);
            }

            this.dataService.AddObject(pokemonBaseHappiness);

            if (this.dataService.GetObjects<PokemonCaptureRateDetail>(includes: "Pokemon, CaptureRate", whereProperty: "PokemonId", wherePropertyValue: pokemonBaseHappiness.PokemonId).Count() == 0 && !this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonBaseHappiness.PokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth").IsComplete)
            {
                return this.RedirectToAction("CaptureRates", "Add", new { pokemonId = pokemonBaseHappiness.PokemonId, generationId = pokemonBaseHappiness.GenerationId });
            }
            else
            {
                return this.RedirectToAction("Pokemon", "Owner");
            }
        }

        [HttpGet]
        [Route("add_pokemon")]
        public IActionResult Pokemon()
        {
            BasePokemonViewModel model = new BasePokemonViewModel()
            {
                AllBaseHappinesses = this.dataService.GetObjects<BaseHappiness>("Happiness"),
                AllClassifications = this.dataService.GetObjects<Classification>("Name"),
                AllCaptureRates = this.dataService.GetObjects<CaptureRate>("CatchRate"),
                AllEggCycles = this.dataService.GetObjects<EggCycle>("CycleCount"),
                AllExperienceGrowths = this.dataService.GetObjects<ExperienceGrowth>("Name"),
                AllGenderRatios = new List<GenderRatioViewModel>(),
                AllSpecialGroupings = this.dataService.GetObjects<SpecialGrouping>(),
                AllGames = this.dataService.GetObjects<Game>("ReleaseDate, Id"),
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

            model.GameId = this.dataService.GetObjects<Game>("ReleaseDate, Id").Last().Id;

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_pokemon")]
        public IActionResult Pokemon(BasePokemonViewModel newPokemon, IFormFile officialUpload, string officialUrlUpload)
        {
            if (!this.ModelState.IsValid)
            {
                BasePokemonViewModel model = new BasePokemonViewModel()
                {
                    AllBaseHappinesses = this.dataService.GetObjects<BaseHappiness>("Happiness"),
                    AllClassifications = this.dataService.GetObjects<Classification>("Name"),
                    AllCaptureRates = this.dataService.GetObjects<CaptureRate>("CatchRate"),
                    AllEggCycles = this.dataService.GetObjects<EggCycle>("CycleCount"),
                    AllExperienceGrowths = this.dataService.GetObjects<ExperienceGrowth>("Name"),
                    AllGenderRatios = new List<GenderRatioViewModel>(),
                    AllSpecialGroupings = this.dataService.GetObjects<SpecialGrouping>(),
                    AllGames = this.dataService.GetObjects<Game>("ReleaseDate, Id"),
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

                return this.View(model);
            }
            else if (this.dataService.GetObjectByPropertyValue<Pokemon>("PokedexNumber", newPokemon.PokedexNumber) != null)
            {
                BasePokemonViewModel model = new BasePokemonViewModel()
                {
                    AllBaseHappinesses = this.dataService.GetObjects<BaseHappiness>("Happiness"),
                    AllClassifications = this.dataService.GetObjects<Classification>("Name"),
                    AllCaptureRates = this.dataService.GetObjects<CaptureRate>("CatchRate"),
                    AllEggCycles = this.dataService.GetObjects<EggCycle>("CycleCount"),
                    AllExperienceGrowths = this.dataService.GetObjects<ExperienceGrowth>("Name"),
                    AllGenderRatios = new List<GenderRatioViewModel>(),
                    AllSpecialGroupings = this.dataService.GetObjects<SpecialGrouping>(),
                    AllGames = this.dataService.GetObjects<Game>("ReleaseDate, Id"),
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

                this.ModelState.AddModelError("Pokedex Number", "Pokedex Number already exists.");
                return this.View(model);
            }

            Game game = this.dataService.GetObjectByPropertyValue<Game>("Id", newPokemon.GameId);

            this.dataService.AddObject(newPokemon);

            this.dataService.UploadImages(officialUpload, officialUrlUpload, newPokemon.Id.ToString(), this.appConfig, ImageType.Official);

            this.dataService.AddObject(new PokemonGameDetail()
            {
                PokemonId = newPokemon.Id,
                GameId = newPokemon.GameId,
            });

            return this.RedirectToAction("Typing", "Add", new { pokemonId = newPokemon.Id, generationId = game.GenerationId });
        }

        [HttpGet]
        [Route("add_alternate_form/{pokemonId:int}")]
        public IActionResult AltForm(int pokemonId)
        {
            Pokemon originalPokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth");
            List<Game> games = this.dataService.GetObjects<Game>("ReleaseDate, Id");

            AlternateFormViewModel model = new AlternateFormViewModel
            {
                AllForms = this.dataService.GetObjects<Form>("Name"),
                AllClassifications = this.dataService.GetObjects<Classification>("Name"),
                AllGames = games.Where(x => x.ReleaseDate >= originalPokemon.Game.ReleaseDate).ToList(),
                OriginalPokemon = originalPokemon,
                OriginalPokemonId = originalPokemon.Id,
                GameId = games.Last().Id,
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_alternate_form/{pokemonId:int}")]
        public IActionResult AltForm(AlternateFormViewModel pokemon, IFormFile officialUpload, string officialUrlUpload)
        {
            if (!this.ModelState.IsValid)
            {
                Pokemon originalPokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemon.OriginalPokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth");
                List<Game> games = this.dataService.GetObjects<Game>("ReleaseDate, Id");

                AlternateFormViewModel model = new AlternateFormViewModel()
                {
                    AllForms = this.dataService.GetObjects<Form>("Name"),
                    AllClassifications = this.dataService.GetObjects<Classification>("Name"),
                    AllGames = games.Where(x => x.ReleaseDate >= originalPokemon.Game.ReleaseDate).ToList(),
                    OriginalPokemon = originalPokemon,
                    OriginalPokemonId = originalPokemon.Id,
                    GameId = games.Last().Id,
                };

                return this.View(model);
            }

            List<PokemonFormDetail> originalPokemonForms = this.dataService.GetObjects<PokemonFormDetail>("AltFormPokemonId", "Form, OriginalPokemon, AltFormPokemon", "OriginalPokemonId", pokemon.OriginalPokemonId);

            foreach (var p in originalPokemonForms)
            {
                if (p.FormId == pokemon.FormId)
                {
                    Pokemon originalPokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemon.OriginalPokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth");
                    List<Game> games = this.dataService.GetObjects<Game>("ReleaseDate, Id").Where(x => x.ReleaseDate >= originalPokemon.Game.ReleaseDate).ToList();

                    AlternateFormViewModel model = new AlternateFormViewModel()
                    {
                        AllForms = this.dataService.GetObjects<Form>("Name"),
                        AllClassifications = this.dataService.GetObjects<Classification>("Name"),
                        AllGames = games,
                        OriginalPokemon = originalPokemon,
                        OriginalPokemonId = originalPokemon.Id,
                    };

                    this.ModelState.AddModelError("Alternate Form Name", "Original Pokemon already has an alternate form of this type.");
                    return this.View(model);
                }
            }

            Pokemon alternatePokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemon.OriginalPokemonId);

            alternatePokemon.Id = 0;
            alternatePokemon.PokedexNumber = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemon.OriginalPokemonId).PokedexNumber;
            alternatePokemon.Height = pokemon.Height;
            alternatePokemon.Weight = pokemon.Weight;
            alternatePokemon.GameId = pokemon.GameId;
            alternatePokemon.ClassificationId = pokemon.ClassificationId;
            alternatePokemon.IsComplete = false;

            this.dataService.AddObject(alternatePokemon);

            this.dataService.UploadImages(officialUpload, officialUrlUpload, alternatePokemon.Id.ToString(), this.appConfig, ImageType.Official);

            PokemonEggGroupDetail eggGroups = this.dataService.GetObjects<PokemonEggGroupDetail>(includes: "Pokemon, PrimaryEggGroup, SecondaryEggGroup", whereProperty: "PokemonId", wherePropertyValue: pokemon.OriginalPokemonId).Last();
            PokemonEggGroupDetail alternatePokemonEggGroups = new PokemonEggGroupDetail()
            {
                PrimaryEggGroupId = eggGroups.PrimaryEggGroupId,
                SecondaryEggGroupId = eggGroups.SecondaryEggGroupId,
                PokemonId = alternatePokemon.Id,
                GenerationId = this.dataService.GetObjectByPropertyValue<Game>("Id", alternatePokemon.GameId).GenerationId,
            };
            this.dataService.AddObject(alternatePokemonEggGroups);

            PokemonFormDetail alternateForm = new PokemonFormDetail()
            {
                OriginalPokemonId = pokemon.OriginalPokemonId,
                AltFormPokemonId = alternatePokemon.Id,
                FormId = pokemon.FormId,
            };
            this.dataService.AddObject(alternateForm);

            this.dataService.AddObject(new PokemonGameDetail()
            {
                PokemonId = alternatePokemon.Id,
                GameId = alternatePokemon.GameId,
            });

            return this.RedirectToAction("Typing", "Add", new { pokemonId = alternatePokemon.Id, generationId = alternatePokemonEggGroups.GenerationId });
        }

        [HttpGet]
        [Route("add_typing/{pokemonId:int}/{generationId:int}")]
        public IActionResult Typing(int pokemonId, int generationId)
        {
            PokemonTypingViewModel model = new PokemonTypingViewModel()
            {
                AllTypes = this.dataService.GetObjects<DataAccess.Models.Type>("Name"),
                PokemonId = pokemonId,
                Pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth"),
                GenerationId = generationId,
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_typing/{pokemonId:int}/{generationId:int}")]
        public IActionResult Typing(PokemonTypingViewModel typing)
        {
            if (!this.ModelState.IsValid)
            {
                PokemonTypingViewModel model = new PokemonTypingViewModel()
                {
                    AllTypes = this.dataService.GetObjects<DataAccess.Models.Type>("Name"),
                    PokemonId = typing.PokemonId,
                    Pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", typing.PokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth"),
                    GenerationId = typing.GenerationId,
                };

                return this.View(model);
            }

            this.dataService.AddObject(typing);

            if (this.dataService.GetObjects<PokemonAbilityDetail>(includes: "Pokemon, PrimaryAbility, SecondaryAbility, HiddenAbility", whereProperty: "PokemonId", wherePropertyValue: typing.PokemonId).Count() == 0 && !this.dataService.GetObjectByPropertyValue<Pokemon>("Id", typing.PokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth").IsComplete)
            {
                return this.RedirectToAction("Abilities", "Add", new { pokemonId = typing.PokemonId, generationId = typing.GenerationId });
            }
            else
            {
                return this.RedirectToAction("Pokemon", "Owner");
            }
        }

        [HttpGet]
        [Route("add_abilities/{pokemonId:int}/{generationId:int}")]
        public IActionResult Abilities(int pokemonId, int generationId)
        {
            PokemonAbilitiesViewModel model = new PokemonAbilitiesViewModel()
            {
                AllAbilities = this.dataService.GetObjects<Ability>("Name"),
                PokemonId = pokemonId,
                Pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form"),
                GenerationId = generationId,
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_abilities/{pokemonId:int}/{generationId:int}")]
        public IActionResult Abilities(PokemonAbilitiesViewModel abilities)
        {
            if (!this.ModelState.IsValid)
            {
                PokemonAbilitiesViewModel model = new PokemonAbilitiesViewModel()
                {
                    AllAbilities = this.dataService.GetObjects<Ability>("Name"),
                    PokemonId = abilities.PokemonId,
                    Pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", abilities.PokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form"),
                    GenerationId = abilities.GenerationId,
                };

                return this.View(model);
            }

            this.dataService.AddObject(abilities);

            if (this.dataService.GetObjects<PokemonBaseHappinessDetail>(includes: "Pokemon, BaseHappiness", whereProperty: "PokemonId", wherePropertyValue: abilities.PokemonId).Count() == 0 && !this.dataService.GetObjectByPropertyValue<Pokemon>("Id", abilities.PokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth").IsComplete)
            {
                return this.RedirectToAction("BaseHappinesses", "Add", new { pokemonId = abilities.PokemonId, generationId = abilities.GenerationId });
            }
            else if (abilities.Pokemon.IsAltForm && this.dataService.GetPokemonBaseStats(abilities.PokemonId, abilities.GenerationId) == null && !this.dataService.GetObjectByPropertyValue<Pokemon>("Id", abilities.PokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth").IsComplete)
            {
                return this.RedirectToAction("BaseStats", "Add", new { pokemonId = abilities.PokemonId, generationId = abilities.GenerationId });
            }
            else
            {
                return this.RedirectToAction("Pokemon", "Owner");
            }
        }

        [HttpGet]
        [Route("add_egg_groups/{pokemonId:int}/{generationId:int}")]
        public IActionResult EggGroups(int pokemonId, int generationId)
        {
            PokemonEggGroupsViewModel model = new PokemonEggGroupsViewModel()
            {
                AllEggGroups = this.dataService.GetObjects<EggGroup>("Name"),
                PokemonId = pokemonId,
                Pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth"),
                GenerationId = generationId,
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_egg_groups/{pokemonId:int}/{generationId:int}")]
        public IActionResult EggGroups(PokemonEggGroupsViewModel eggGroups)
        {
            if (!this.ModelState.IsValid)
            {
                PokemonEggGroupsViewModel model = new PokemonEggGroupsViewModel()
                {
                    AllEggGroups = this.dataService.GetObjects<EggGroup>("Name"),
                    PokemonId = eggGroups.PokemonId,
                    Pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", eggGroups.PokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth"),
                    GenerationId = eggGroups.GenerationId,
                };

                return this.View(model);
            }

            this.dataService.AddObject(eggGroups);

            if (this.dataService.GetPokemonBaseStats(eggGroups.PokemonId, eggGroups.GenerationId) == null && !this.dataService.GetObjectByPropertyValue<Pokemon>("Id", eggGroups.PokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth").IsComplete)
            {
                return this.RedirectToAction("BaseStats", "Add", new { pokemonId = eggGroups.PokemonId, generationId = eggGroups.GenerationId });
            }
            else
            {
                return this.RedirectToAction("Pokemon", "Owner");
            }
        }

        [HttpGet]
        [Route("add_base_stats/{pokemonId:int}/{generationId:int}")]
        public IActionResult BaseStats(int pokemonId, int generationId)
        {
            BaseStat model = new BaseStat()
            {
                PokemonId = pokemonId,
                Pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth"),
                GenerationId = generationId,
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_base_stats/{pokemonId:int}/{generationId:int}")]
        public IActionResult BaseStats(BaseStat baseStat)
        {
            if (!this.ModelState.IsValid)
            {
                BaseStat model = new BaseStat()
                {
                    PokemonId = baseStat.PokemonId,
                    Pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", baseStat.PokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth"),
                    GenerationId = baseStat.GenerationId,
                };

                return this.View(model);
            }

            this.dataService.AddObject(baseStat);

            if (this.dataService.GetPokemonEVYields(baseStat.PokemonId, baseStat.GenerationId) == null && !this.dataService.GetObjectByPropertyValue<Pokemon>("Id", baseStat.PokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth").IsComplete)
            {
                return this.RedirectToAction("EVYields", "Add", new { pokemonId = baseStat.PokemonId, generationId = baseStat.GenerationId });
            }
            else
            {
                return this.RedirectToAction("Pokemon", "Owner");
            }
        }

        [HttpGet]
        [Route("add_ev_yields/{pokemonId:int}/{generationId:int}")]
        public IActionResult EVYields(int pokemonId, int generationId)
        {
            EVYield model = new EVYield()
            {
                PokemonId = pokemonId,
                Pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", pokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth"),
                GenerationId = generationId,
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add_ev_yields/{pokemonId:int}/{generationId:int}")]
        public IActionResult EVYields(EVYield evYield)
        {
            if (!this.ModelState.IsValid)
            {
                EVYield model = new EVYield()
                {
                    PokemonId = evYield.PokemonId,
                    Pokemon = this.dataService.GetObjectByPropertyValue<Pokemon>("Id", evYield.PokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth"),
                };

                return this.View(model);
            }

            this.dataService.AddObject(evYield);

            if (this.User.IsInRole("Owner") && !this.dataService.GetObjectByPropertyValue<Pokemon>("Id", evYield.PokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth").IsComplete)
            {
                return this.RedirectToAction("Evolution", "Add", new { pokemonId = evYield.PokemonId, generationId = evYield.GenerationId });
            }
            else
            {
                return this.RedirectToAction("Pokemon", "Owner");
            }
        }
    }
}

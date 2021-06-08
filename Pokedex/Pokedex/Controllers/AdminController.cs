using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pokedex.DataAccess.Models;
using Pokedex.Models;
using System.Collections.Generic;
using System.Linq;

namespace Pokedex.Controllers
{
    [Authorize(Roles = "Admin,Owner")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly DataService dataService;

        public AdminController(DataContext dataContext)
        {
            this.dataService = new DataService(dataContext);
        }

        [Route("pokemon")]
        public IActionResult Pokemon()
        {
            List<int> model = this.dataService.GetGenerationsFromPokemonWithIncomplete();

            return this.View(model);
        }

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

        [Route("regions")]
        public IActionResult Regions()
        {
            RegionViewModel model = new RegionViewModel()
            {
                AllRegions = this.dataService.GetObjects<Region>(includes: "Generation"),
            };

            return this.View(model);
        }

        [Route("games")]
        public IActionResult Games()
        {
            AdminGameViewModel model = new AdminGameViewModel()
            {
                AllGames = this.dataService.GetObjects<Game>("ReleaseDate, Id").OrderBy(x => x.ReleaseDate).ToList(),
                AllPokemon = this.dataService.GetAllPokemonIncludeIncomplete(),
            };

            return this.View(model);
        }

        [Route("comment_categories")]
        public IActionResult CommentCategories()
        {
            CommentViewModel model = new CommentViewModel()
            {
                AllComments = this.dataService.GetComments(),
                AllCategories = this.dataService.GetObjects<CommentCategory>(),
                AllPages = this.dataService.GetCommentPages(),
            };

            return this.View(model);
        }

        [Route("comment_pages")]
        public IActionResult CommentPages()
        {
            CommentViewModel model = new CommentViewModel()
            {
                AllComments = this.dataService.GetComments(),
                AllCategories = this.dataService.GetObjects<CommentCategory>(),
                AllPages = this.dataService.GetCommentPages(),
            };

            return this.View(model);
        }

        [Route("type")]
        public IActionResult Types()
        {
            TypeViewModel model = new TypeViewModel()
            {
                AllTypes = this.dataService.GetObjects<Type>("Name"),
                AllPokemon = this.dataService.GetAllPokemonWithTypesAndIncomplete(),
            };

            return this.View(model);
        }

        [Route("egg_cycle")]
        public IActionResult EggCycles()
        {
            EggCycleViewModel model = new EggCycleViewModel()
            {
                AllEggCycles = this.dataService.GetObjects<EggCycle>("CycleCount"),
                AllPokemon = this.dataService.GetAllPokemonIncludeIncomplete(),
            };

            return this.View(model);
        }

        [Route("experience_growth")]
        public IActionResult ExperienceGrowths()
        {
            ExperienceGrowthViewModel model = new ExperienceGrowthViewModel()
            {
                AllExperienceGrowths = this.dataService.GetObjects<ExperienceGrowth>("Name"),
                AllPokemon = this.dataService.GetAllPokemonIncludeIncomplete(),
            };

            return this.View(model);
        }

        [Route("gender_ratio")]
        public IActionResult GenderRatios()
        {
            GenderRatiosViewModel model = new GenderRatiosViewModel()
            {
                AllGenderRatios = this.dataService.GetObjects<GenderRatio>(),
                AllPokemon = this.dataService.GetAllPokemonIncludeIncomplete(),
            };

            return this.View(model);
        }

        [Route("form_items")]
        public IActionResult FormItems()
        {
            List<FormItem> model = this.dataService.GetFormItems();

            return this.View(model);
        }

        [Route("statuses")]
        public IActionResult Statuses()
        {
            List<Status> model = this.dataService.GetObjects<Status>("Name");

            return this.View(model);
        }

        [Route("pokeballs")]
        public IActionResult Pokeballs()
        {
            PokeballViewModel model = new PokeballViewModel()
            {
                AllPokeballs = this.dataService.GetPokeballs(),
                AllCatchModifiers = this.dataService.GetPokeballCatchModifierDetails(),
            };

            return this.View(model);
        }

        [Route("ability")]
        public IActionResult Abilities()
        {
            AbilityViewModel model = new AbilityViewModel()
            {
                AllAbilities = this.dataService.GetObjects<Ability>("GenerationId, Name"),
                AllPokemon = this.dataService.GetAllPokemonWithAbilitiesAndIncomplete(),
            };

            return this.View(model);
        }

        [Route("legendary_type")]
        public IActionResult LegendaryTypes()
        {
            LegendaryTypeViewModel model = new LegendaryTypeViewModel()
            {
                AllLegendaryTypes = this.dataService.GetObjects<LegendaryType>("Type"),
                AllPokemon = this.dataService.GetAllPokemonWithLegendaryTypesAndIncomplete(),
            };

            return this.View(model);
        }

        [Route("form")]
        public IActionResult Forms()
        {
            FormViewModel model = new FormViewModel()
            {
                AllForms = this.dataService.GetObjects<Form>("Name"),
                AllPokemon = this.dataService.GetPokemonFormDetails(),
            };

            return this.View(model);
        }

        [Route("egg_group")]
        public IActionResult EggGroups()
        {
            EggGroupViewModel model = new EggGroupViewModel()
            {
                AllEggGroups = this.dataService.GetObjects<EggGroup>("Name"),
                AllPokemon = this.dataService.GetAllPokemonWithEggGroupsAndIncomplete(),
            };

            return this.View(model);
        }

        [Route("evolution_methods")]
        public IActionResult EvolutionMethods()
        {
            EvolutionMethodViewModel model = new EvolutionMethodViewModel()
            {
                AllEvolutionMethods = this.dataService.GetObjects<EvolutionMethod>("Name"),
                AllEvolutions = this.dataService.GetEvolutions(),
            };

            return this.View(model);
        }

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

        [Route("base_happinesses")]
        public IActionResult BaseHappinesses()
        {
            BaseHappinessViewModel model = new BaseHappinessViewModel()
            {
                AllBaseHappinesses = this.dataService.GetObjects<BaseHappiness>("Happiness"),
                AllPokemon = this.dataService.GetAllPokemonIncludeIncomplete(),
            };

            return this.View(model);
        }

        [Route("classification")]
        public IActionResult Classifications()
        {
            ClassificationViewModel model = new ClassificationViewModel()
            {
                AllClassifications = this.dataService.GetObjects<Classification>("Name"),
                AllPokemon = this.dataService.GetAllPokemonWithClassificationsAndIncomplete(),
            };

            return this.View(model);
        }

        [Route("nature")]
        public IActionResult Natures()
        {
            List<Nature> model = this.dataService.GetObjects<Nature>("Name");

            return this.View(model);
        }

        [Route("battle_item")]
        public IActionResult BattleItems()
        {
            List<Pokemon> pokemonList = this.dataService.GetAllPokemon();
            List<Pokemon> altFormsList = this.dataService.GetAllAltForms().ConvertAll(x => x.AltFormPokemon);
            foreach (var p in pokemonList.Where(x => altFormsList.Any(y => y.Id == x.Id)))
            {
                p.Name = this.dataService.GetAltFormWithFormName(p.Id).Name;
            }

            BattleItemViewModel model = new BattleItemViewModel()
            {
                AllBattleItems = this.dataService.GetBattleItems(),
                AllPokemonTeamDetails = this.dataService.GetPokemonTeamDetails(),
                AllPokemon = pokemonList,
            };

            return this.View(model);
        }
    }
}

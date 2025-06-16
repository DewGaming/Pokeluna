using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pokedex.DataAccess.Models;
using System.Collections.Generic;

namespace Pokedex.Controllers
{
    /// <summary>
    /// The class that handles the deletion of objects from the database.
    /// </summary>
    [Authorize(Roles = "Owner")]
    [Route("admin")]
    public class DeleteController : Controller
    {
        private readonly DataService dataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteController"/> class.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        public DeleteController(DataContext dataContext)
        {
            // Instantiate an instance of the data service.
            this.dataService = new DataService(dataContext);
        }

        [HttpGet]
        [Route("delete_generation/{id:int}")]
        public IActionResult Generation(int id)
        {
            Generation model = this.dataService.GetObjectByPropertyValue<Generation>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_generation/{id:int}")]
        public IActionResult Generation(Generation generation)
        {
            this.dataService.DeleteObject<Generation>(generation.Id);

            return this.RedirectToAction("Generations", "Owner");
        }

        [HttpGet]
        [Route("delete_pageStat/{id:int}")]
        public IActionResult PageStat(int id)
        {
            PageStat model = this.dataService.GetObjectByPropertyValue<PageStat>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_pageStat/{id:int}")]
        public IActionResult PageStat(PageStat pageStat)
        {
            this.dataService.DeleteObject<PageStat>(pageStat.Id);

            return this.RedirectToAction("PageStats", "Owner");
        }

        [HttpGet]
        [Route("delete_game/{id:int}")]
        public IActionResult Game(int id)
        {
            Game model = this.dataService.GetObjectByPropertyValue<Game>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_game/{id:int}")]
        public IActionResult Game(Game game)
        {
            this.dataService.DeleteObject<Game>(game.Id);

            return this.RedirectToAction("Games", "Owner");
        }

        [HttpGet]
        [Route("delete_type/{id:int}")]
        public IActionResult Type(int id)
        {
            Type model = this.dataService.GetObjectByPropertyValue<Type>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_type/{id:int}")]
        public IActionResult Type(Type type)
        {
            List<TypeChart> typeCharts = this.dataService.GetObjects<TypeChart>("AttackId, DefendId", "Attack, Defend", "DefendId", type.Id);
            typeCharts.AddRange(this.dataService.GetObjects<TypeChart>("AttackId, DefendId", "Attack, Defend", "AttackId", type.Id));
            foreach (var t in typeCharts)
            {
                this.dataService.DeleteObject<TypeChart>(t.Id);
            }

            this.dataService.DeleteObject<Type>(type.Id);

            return this.RedirectToAction("Types", "Owner");
        }

        [HttpGet]
        [Route("delete_nature/{id:int}")]
        public IActionResult Nature(int id)
        {
            Nature model = this.dataService.GetObjectByPropertyValue<Nature>("Id", id, "RaisedStat, LoweredStat");

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_nature/{id:int}")]
        public IActionResult Nature(Nature nature)
        {
            this.dataService.DeleteObject<Nature>(nature.Id);

            return this.RedirectToAction("Natures", "Owner");
        }

        [HttpGet]
        [Route("delete_regional_dex/{id:int}")]
        public IActionResult RegionalDex(int id)
        {
            RegionalDex model = this.dataService.GetObjectByPropertyValue<RegionalDex>("Id", id, "Game");

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_regional_dex/{id:int}")]
        public IActionResult RegionalDex(RegionalDex regionalDex)
        {
            this.dataService.DeleteObject<RegionalDex>(regionalDex.Id);

            return this.RedirectToAction("RegionalDexes", "Owner");
        }

        /// <summary>
        /// The page used to delete a stat from the database.
        /// </summary>
        /// <param name="id">The id of the stat being deleted.</param>
        /// <returns>The page to delete the stat.</returns>
        [HttpGet]
        [Route("delete_stat/{id:int}")]
        public IActionResult Stat(int id)
        {
            Stat model = this.dataService.GetObjectByPropertyValue<Stat>("Id", id);

            return this.View(model);
        }

        /// <summary>
        /// The page used to delete a stat from the database.
        /// </summary>
        /// <param name="stat">The stat being deleted.</param>
        /// <returns>The stats page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_stat/{id:int}")]
        public IActionResult Stat(Stat stat)
        {
            this.dataService.DeleteObject<Stat>(stat.Id);

            return this.RedirectToAction("Stats", "Owner");
        }

        [HttpGet]
        [Route("delete_mark/{id:int}")]
        public IActionResult Mark(int id)
        {
            Mark model = this.dataService.GetObjectByPropertyValue<Mark>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_mark/{id:int}")]
        public IActionResult Mark(Mark mark)
        {
            this.dataService.DeleteObject<Mark>(mark.Id);

            return this.RedirectToAction("Marks", "Owner");
        }

        [HttpGet]
        [Route("delete_sweet/{id:int}")]
        public IActionResult Sweet(int id)
        {
            Sweet model = this.dataService.GetObjectByPropertyValue<Sweet>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_sweet/{id:int}")]
        public IActionResult Sweet(Sweet sweet)
        {
            this.dataService.DeleteObject<Sweet>(sweet.Id);

            return this.RedirectToAction("Sweets", "Owner");
        }

        [HttpGet]
        [Route("delete_pokeball/{id:int}")]
        public IActionResult Pokeball(int id)
        {
            Pokeball model = this.dataService.GetObjectByPropertyValue<Pokeball>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_pokeball/{id:int}")]
        public IActionResult Pokeball(Pokeball pokeball)
        {
            this.dataService.DeleteObject<Pokeball>(pokeball.Id);

            return this.RedirectToAction("Pokeballs", "Owner");
        }

        [HttpGet]
        [Route("delete_hunting_method/{id:int}")]
        public IActionResult HuntingMethod(int id)
        {
            HuntingMethod model = this.dataService.GetObjectByPropertyValue<HuntingMethod>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_hunting_method/{id:int}")]
        public IActionResult HuntingMethod(HuntingMethod huntingMethod)
        {
            this.dataService.DeleteObject<HuntingMethod>(huntingMethod.Id);

            return this.RedirectToAction("HuntingMethods", "Owner");
        }

        [HttpGet]
        [Route("delete_egg_cycle/{id:int}")]
        public IActionResult EggCycle(int id)
        {
            EggCycle model = this.dataService.GetObjectByPropertyValue<EggCycle>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_egg_cycle/{id:int}")]
        public IActionResult EggCycle(EggCycle eggCycle)
        {
            this.dataService.DeleteObject<EggCycle>(eggCycle.Id);

            return this.RedirectToAction("EggCycles", "Owner");
        }

        [HttpGet]
        [Route("delete_experience_growth/{id:int}")]
        public IActionResult ExperienceGrowth(int id)
        {
            ExperienceGrowth model = this.dataService.GetObjectByPropertyValue<ExperienceGrowth>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_experience_growth/{id:int}")]
        public IActionResult ExperienceGrowth(ExperienceGrowth experienceGrowth)
        {
            this.dataService.DeleteObject<ExperienceGrowth>(experienceGrowth.Id);

            return this.RedirectToAction("ExperienceGrowths", "Owner");
        }

        [HttpGet]
        [Route("delete_gender_ratio/{id:int}")]
        public IActionResult GenderRatio(int id)
        {
            GenderRatio model = this.dataService.GetObjectByPropertyValue<GenderRatio>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_gender_ratio/{id:int}")]
        public IActionResult GenderRatio(GenderRatio genderRatio)
        {
            this.dataService.DeleteObject<GenderRatio>(genderRatio.Id);

            return this.RedirectToAction("GenderRatios", "Owner");
        }

        [HttpGet]
        [Route("delete_capture_rate/{id:int}")]
        public IActionResult CaptureRate(int id)
        {
            CaptureRate model = this.dataService.GetObjectByPropertyValue<CaptureRate>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_capture_rate/{id:int}")]
        public IActionResult CaptureRate(CaptureRate captureRate)
        {
            this.dataService.DeleteObject<CaptureRate>(captureRate.Id);

            return this.RedirectToAction("CaptureRates", "Owner");
        }

        [HttpGet]
        [Route("delete_base_happiness/{id:int}")]
        public IActionResult BaseHappiness(int id)
        {
            BaseHappiness model = this.dataService.GetObjectByPropertyValue<BaseHappiness>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_base_happiness/{id:int}")]
        public IActionResult BaseHappiness(BaseHappiness baseHappiness)
        {
            this.dataService.DeleteObject<BaseHappiness>(baseHappiness.Id);

            return this.RedirectToAction("BaseHappinesses", "Owner");
        }

        [HttpGet]
        [Route("delete_evolution_method/{id:int}")]
        public IActionResult EvolutionMethod(int id)
        {
            EvolutionMethod model = this.dataService.GetObjectByPropertyValue<EvolutionMethod>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_evolution_method/{id:int}")]
        public IActionResult EvolutionMethod(EvolutionMethod evolutionMethod)
        {
            this.dataService.DeleteObject<EvolutionMethod>(evolutionMethod.Id);

            return this.RedirectToAction("EvolutionMethods", "Owner");
        }

        [HttpGet]
        [Route("delete_ability/{id:int}")]
        public IActionResult Ability(int id)
        {
            Ability model = this.dataService.GetObjectByPropertyValue<Ability>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_ability/{id:int}")]
        public IActionResult Ability(Ability ability)
        {
            this.dataService.DeleteObject<Ability>(ability.Id);

            return this.RedirectToAction("Abilities", "Owner");
        }

        [HttpGet]
        [Route("delete_form/{id:int}")]
        public IActionResult Form(int id)
        {
            Form model = this.dataService.GetObjectByPropertyValue<Form>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_form/{id:int}")]
        public IActionResult Form(Form form)
        {
            this.dataService.DeleteObject<Form>(form.Id);

            return this.RedirectToAction("Forms", "Owner");
        }

        [HttpGet]
        [Route("delete_special_grouping/{id:int}")]
        public IActionResult SpecialGrouping(int id)
        {
            SpecialGrouping model = this.dataService.GetObjectByPropertyValue<SpecialGrouping>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_special_grouping/{id:int}")]
        public IActionResult SpecialGrouping(SpecialGrouping specialGrouping)
        {
            this.dataService.DeleteObject<SpecialGrouping>(specialGrouping.Id);

            return this.RedirectToAction("SpecialGroupings", "Owner");
        }

        [HttpGet]
        [Route("delete_comment/{id:int}")]
        public IActionResult Comment(int id)
        {
            Comment comment = this.dataService.GetObjectByPropertyValue<Comment>("Id", id);
            this.dataService.DeleteObject<Comment>(comment.Id);

            return this.RedirectToAction("Comments", "Owner");
        }

        [HttpGet]
        [Route("delete_egg_group/{id:int}")]
        public IActionResult EggGroup(int id)
        {
            EggGroup model = this.dataService.GetObjectByPropertyValue<EggGroup>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_egg_group/{id:int}")]
        public IActionResult EggGroup(EggGroup eggGroup)
        {
            this.dataService.DeleteObject<EggGroup>(eggGroup.Id);

            return this.RedirectToAction("EggGroups", "Owner");
        }

        [HttpGet]
        [Route("delete_form_item/{id:int}")]
        public IActionResult FormItem(int id)
        {
            FormItem model = this.dataService.GetObjectByPropertyValue<FormItem>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_form_item/{id:int}")]
        public IActionResult FormItem(FormItem formItem)
        {
            this.dataService.DeleteObject<FormItem>(formItem.Id);

            return this.RedirectToAction("FormItems", "Owner");
        }

        [HttpGet]
        [Route("delete_form_group/{id:int}")]
        public IActionResult FormGroup(int id)
        {
            FormGroup model = this.dataService.GetObjectByPropertyValue<FormGroup>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_form_group/{id:int}")]
        public IActionResult FormGroup(FormGroup formGroup)
        {
            this.dataService.DeleteObject<FormGroup>(formGroup.Id);

            return this.RedirectToAction("FormGroups", "Owner");
        }

        [HttpGet]
        [Route("delete_classification/{id:int}")]
        public IActionResult Classification(int id)
        {
            Classification model = this.dataService.GetObjectByPropertyValue<Classification>("Id", id);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("delete_classification/{id:int}")]
        public IActionResult Classification(Classification classification)
        {
            this.dataService.DeleteObject<Classification>(classification.Id);

            return this.RedirectToAction("Classifications", "Owner");
        }
    }
}

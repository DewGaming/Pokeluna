using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MoreLinq;
using Pokedex.DataAccess.Models;
using Pokedex.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Mail;

namespace Pokedex
{
    /// <summary>
    /// The class that is used to represent methods to grab, modify, or delete data from the DataContext database.
    /// </summary>
    public class DataService
    {
        private readonly DataContext dataContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataService"/> class.
        /// </summary>
        /// <param name="dataContext">The data context for the main database.</param>
        public DataService(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        /// <summary>
        /// Records a page's view.
        /// </summary>
        /// <param name="pageName">The name of the page the view is being counted for.</param>
        /// <param name="isOwner">A check to ensure the user is not the owner.</param>
        public void AddPageView(string pageName, bool isOwner)
        {
            if (!isOwner)
            {
                PageStat pageStat = this.GetObjects<PageStat>().Find(x => x.Name == pageName);

                this.AddObject(new PageStat() { Name = pageName, VisitTime = DateTime.Now.ToUniversalTime(), VisitDate = DateTime.Now.ToUniversalTime() });
            }
        }

        /// <summary>
        /// Records a page's view.
        /// </summary>
        /// <param name="pokemonId">The id of the pokemon the view is being counted for.</param>
        /// <param name="formId">The id of the form the view is being counted for. Optional.</param>
        /// <param name="isOwner">A check to ensure the user is not the owner.</param>
        public void AddPokemonPageView(int pokemonId, int formId, bool isOwner)
        {
            if (!isOwner)
            {
                if (formId == 0)
                {
                    this.AddObject(new PokemonPageStat() { PokemonId = pokemonId, VisitTime = DateTime.Now.ToUniversalTime(), VisitDate = DateTime.Now.ToUniversalTime() });
                }
                else
                {
                    this.AddObject(new PokemonPageStat() { PokemonId = pokemonId, FormId = formId, VisitTime = DateTime.Now.ToUniversalTime(), VisitDate = DateTime.Now.ToUniversalTime() });
                }
            }
        }

        /// <summary>
        /// Returns a list of objects from the passed-through TEntity class.
        /// </summary>
        /// <typeparam name="TEntity">The generic type parameter.</typeparam>
        /// <param name="orderedProperty">The property that is used if an order is needed. Separate properties by commas for multiples. I.E. "Id, Name".</param>
        /// <param name="includes">The property that is used if an include is needed. Separate includes by commas for multiples. I.E. "Game, Ability".</param>
        /// <param name="whereProperty">The property the object will be searched for.</param>
        /// <param name="wherePropertyValue">The property's value.</param>
        /// <returns>Returns the list of the class requested.</returns>
        public List<TEntity> GetObjects<TEntity>(string orderedProperty = "", string includes = "", string whereProperty = "", object wherePropertyValue = null)
            where TEntity : class
        {
            IQueryable<TEntity> objects = this.dataContext.Set<TEntity>();

            if (!string.IsNullOrEmpty(includes))
            {
                foreach (var i in includes.Split(", "))
                {
                    objects = objects.Include(i);
                }
            }

            if (!string.IsNullOrEmpty(orderedProperty))
            {
                objects = objects.OrderBy(orderedProperty);
            }

            if (!string.IsNullOrEmpty(whereProperty) && wherePropertyValue != null)
            {
                string searchString = string.Concat("x => x.", whereProperty, " == ");

                if (wherePropertyValue is string)
                {
                    searchString = string.Concat(searchString, '"', wherePropertyValue, '"');
                }
                else
                {
                    searchString = string.Concat(searchString, wherePropertyValue);
                }

                try
                {
                    return objects.Where(searchString).ToList();
                }
                catch
                {
                    return objects.ToList();
                }
            }
            else
            {
                return objects.ToList();
            }
        }

        /// <summary>
        /// Returns an object based off the passed-through TEntity class and the variable we are finding.
        /// </summary>
        /// <typeparam name="TEntity">The generic type parameter.</typeparam>
        /// <param name="property">The property the object will be searched for.</param>
        /// <param name="propertyValue">The property's value.</param>
        /// <param name="includes">The property that is used if an include is needed. Separate includes by commas for multiples. I.E. "Game, Ability".</param>
        /// <returns>Returns the object with the correct class and id.</returns>
        public TEntity GetObjectByPropertyValue<TEntity>(string property, object propertyValue, string includes = "")
            where TEntity : class
        {
            IQueryable<TEntity> objects = this.dataContext.Set<TEntity>();

            if (!string.IsNullOrEmpty(includes))
            {
                foreach (var i in includes.Split(", "))
                {
                    objects = objects.Include(i);
                }
            }

            string searchString = string.Concat("x => x.", property, " == ");
            if (propertyValue is string)
            {
                searchString = string.Concat(searchString, '"', propertyValue, '"');
            }
            else
            {
                searchString = string.Concat(searchString, propertyValue);
            }

            try
            {
                return objects.First(searchString);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Creates an entity in the database and saves the changes.
        /// </summary>
        /// <typeparam name="TEntity">The generic type parameter.</typeparam>
        /// <param name="entity">The entity being added.</param>
        public void AddObject<TEntity>(TEntity entity)
            where TEntity : class
        {
            this.dataContext.Set<TEntity>().Add(entity);
            this.dataContext.SaveChanges();
        }

        /// <summary>
        /// Updates an entity in the database and saves the changes.
        /// </summary>
        /// <typeparam name="TEntity">The generic type parameter.</typeparam>
        /// <param name="entity">The updated entity.</param>
        public void UpdateObject<TEntity>(TEntity entity)
            where TEntity : class
        {
            try
            {
                this.dataContext.Set<TEntity>().Update(entity);
                this.dataContext.SaveChanges();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Delets an entity in the database and saves the changes.
        /// </summary>
        /// <typeparam name="TEntity">The generic type parameter.</typeparam>
        /// <param name="id">The id of the entity to be deleted.</param>
        public void DeleteObject<TEntity>(int id)
            where TEntity : class
        {
            var entity = this.GetObjectByPropertyValue<TEntity>("Id", id);
            this.dataContext.Set<TEntity>().Remove(entity);
            this.dataContext.SaveChanges();
        }

        /// <summary>
        /// Gets the list of form items with form names added to it.
        /// </summary>
        /// <returns>The list of formatted form items.</returns>
        public List<FormItem> GetFormItems()
        {
            List<FormItem> formItemList = this.GetObjects<FormItem>("Pokemon.PokedexNumber", "Pokemon, Pokemon.Form");
            formItemList.ForEach(x => x.Pokemon.Name = x.Pokemon.NameWithForm);

            return formItemList;
        }

        /// <summary>
        /// Gets the list of pre-evolutions with form names added to it.
        /// </summary>
        /// <param name="pokemonId">The id of the evolved pokemon.</param>
        /// <returns>The list of formatted pre evolutions.</returns>
        public List<Evolution> GetPreEvolution(int pokemonId)
        {
            List<Evolution> preEvolution = this.GetObjects<Evolution>(includes: "PreevolutionPokemon, PreevolutionPokemon.Game, PreevolutionPokemon.Form, EvolutionPokemon, EvolutionPokemon.Game, EvolutionMethod", whereProperty: "EvolutionPokemon.Id", wherePropertyValue: pokemonId);
            preEvolution.Where(x => x.PreevolutionPokemon.IsAltForm).ForEach(x => x.PreevolutionPokemon.Name = x.PreevolutionPokemon.NameWithForm);

            return preEvolution;
        }

        /// <summary>
        /// Gets all of the evolutions available for a pokemon by their id.
        /// </summary>
        /// <param name="pokemonId">The preevolution's id.</param>
        /// <returns>The list of all available evolutions.</returns>
        public List<Evolution> GetPokemonEvolutions(int pokemonId)
        {
            List<Evolution> evolutions = this.GetObjects<Evolution>("EvolutionPokemon.PokedexNumber, EvolutionPokemon.Id",  "PreevolutionPokemon, PreevolutionPokemon.Game, EvolutionPokemon, EvolutionPokemon.Game, EvolutionPokemon.Form, EvolutionMethod", "PreevolutionPokemon.Id", pokemonId);
            evolutions.Where(x => x.EvolutionPokemon.IsAltForm).ForEach(x => x.EvolutionPokemon.Name = x.EvolutionPokemon.NameWithForm);

            return evolutions;
        }

        /// <summary>
        /// Gets the pokemon by the name.
        /// </summary>
        /// <param name="name">The name of the pokemon.</param>
        /// <returns>Returns the requested pokemon.</returns>
        public Pokemon GetPokemon(string name)
        {
            return this.GetObjectByPropertyValue<Pokemon>("Name", name);
        }

        /// <summary>
        /// Gets all of the details required for admin actions.
        /// </summary>
        /// <returns>Returns all the details required for admins.</returns>
        public AllAdminPokemonViewModel GetAllAdminPokemonDetails()
        {
            return new AllAdminPokemonViewModel()
            {
                AllAltForms = this.GetObjects<PokemonFormDetail>(includes: "AltFormPokemon, AltFormPokemon.Game, OriginalPokemon, OriginalPokemon.Game, Form"),
                AllEvolutions = this.GetObjects<Evolution>(includes: "PreevolutionPokemon, PreevolutionPokemon.Game, EvolutionPokemon, EvolutionPokemon.Game, EvolutionMethod, Generation"),
                AllTypings = this.GetObjects<PokemonTypeDetail>("PokemonId", "Pokemon, PrimaryType, SecondaryType"),
                AllAbilities = this.GetObjects<PokemonAbilityDetail>(includes: "Pokemon, PrimaryAbility, SecondaryAbility, HiddenAbility"),
                AllEggGroups = this.GetObjects<PokemonEggGroupDetail>(includes: "Pokemon, PrimaryEggGroup, SecondaryEggGroup"),
                AllBaseStats = this.GetObjects<BaseStat>(includes: "Pokemon"),
                AllEVYields = this.GetObjects<EVYield>(includes: "Pokemon"),
                AllPokemonCaptureRates = this.GetAllPokemonWithCaptureRates(),
                AllPokemonBaseHappinesses = this.GetAllPokemonWithBaseHappinesses(),
            };
        }

        /// <summary>
        /// Gets a list of all pokemon.
        /// </summary>
        /// <returns>Returns the list of all pokemon.</returns>
        public List<Pokemon> GetAllPokemon()
        {
            return this.GetObjects<Pokemon>("PokedexNumber, Id", "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth, Form, SpecialGrouping, BaseHappinesses, BaseHappinesses.BaseHappiness, BaseStats, EVYields, Typings.PrimaryType, Typings.SecondaryType, Typings.Generation, Abilities.PrimaryAbility, Abilities.SecondaryAbility, Abilities.HiddenAbility, EggGroups.PrimaryEggGroup, EggGroups.SecondaryEggGroup, CaptureRates.CaptureRate, CaptureRates.Generation");
        }

        /// <summary>
        /// Gets all of the details for a pokemon.
        /// </summary>
        /// <param name="pokemon">The pokemon needing their details.</param>
        /// <param name="appConfig">The application's configuration.</param>
        /// <param name="form">The alternate form if it applies.</param>
        /// <returns>Returns the pokemon's details.</returns>
        public PokemonViewModel GetPokemonDetails(Pokemon pokemon, AppConfig appConfig, Form form = null)
        {
            List<Game> games = this.GetObjects<PokemonGameDetail>("Game.ReleaseDate, GameId, Id", "Pokemon, Pokemon.Game, Pokemon.Form, Pokemon.SpecialGrouping, Game", "PokemonId", pokemon.Id).ConvertAll(x => x.Game).Where(x => x.ReleaseDate <= DateTime.UtcNow).ToList();
            games.Remove(games.Find(x => x.Id == 43));

            PokemonViewModel pokemonViewModel = new PokemonViewModel()
            {
                Pokemon = pokemon,
                BaseHappinesses = pokemon.BaseHappinesses.OrderByDescending(x => x.GenerationId).ToList(),
                BaseStats = pokemon.BaseStats.OrderByDescending(x => x.GenerationId).ToList(),
                EVYields = pokemon.EVYields.OrderByDescending(x => x.GenerationId).ToList(),
                Typings = pokemon.Typings.OrderByDescending(x => x.GenerationId).ToList(),
                Abilities = pokemon.Abilities.OrderByDescending(x => x.GenerationId).ToList(),
                EggGroups = pokemon.EggGroups.OrderByDescending(x => x.GenerationId).ToList(),
                CaptureRates = pokemon.CaptureRates.OrderByDescending(x => x.GenerationId).ToList(),
                PreEvolutions = this.GetPreEvolution(pokemon.Id),
                Evolutions = this.GetPokemonEvolutions(pokemon.Id),
                Effectiveness = this.GetTypeChartPokemon(pokemon.Id),
                SurroundingPokemon = this.GetSurroundingPokemon(pokemon.PokedexNumber),
                RegionalDexes = this.GetObjects<RegionalDex>(includes: "Game"),
                RegionalDexEntries = this.GetObjects<RegionalDexEntry>(includes: "Pokemon", whereProperty: "PokemonId", wherePropertyValue: pokemon.Id),
                GamesAvailableIn = games,
                HasShiny = false,
                HasHome = false,
                HasFemaleGenderDifference = false,
                HasMaleGenderDifference = false,
                AppConfig = appConfig,
            };

            if (pokemon.IsAltForm)
            {
                pokemonViewModel.Pokemon.Name = pokemon.NameWithForm;
            }

            HttpWebRequest webRequest;
            HttpWebResponse imageRequest;
            try
            {
                if (pokemon.HasShinyArtwork)
                {
                    pokemonViewModel.HasShiny = true;
                }
                else
                {
                    webRequest = (HttpWebRequest)WebRequest.Create(string.Concat(appConfig.WebUrl, appConfig.ShinyPokemonImageUrl, pokemon.Id, ".png"));
                    imageRequest = (HttpWebResponse)webRequest.GetResponse();
                    if (imageRequest.StatusCode == HttpStatusCode.OK)
                    {
                        pokemonViewModel.HasShiny = true;
                        this.UpdateImageBools(pokemon.Id, ImageType.Shiny);
                    }
                }
            }
            catch
            {
            }

            try
            {
                if (pokemon.HasHomeArtwork)
                {
                    pokemonViewModel.HasHome = true;
                }
                else
                {
                    webRequest = (HttpWebRequest)WebRequest.Create(string.Concat(appConfig.WebUrl, appConfig.HomePokemonImageUrl, pokemon.Id, ".png"));
                    imageRequest = (HttpWebResponse)webRequest.GetResponse();
                    if (imageRequest.StatusCode == HttpStatusCode.OK)
                    {
                        pokemonViewModel.HasHome = true;
                        this.UpdateImageBools(pokemon.Id, ImageType.Home);
                    }
                }
            }
            catch
            {
            }

            if (pokemon.HasGenderDifference)
            {
                // Female Gender Difference Check
                try
                {
                    webRequest = (HttpWebRequest)WebRequest.Create(string.Concat(appConfig.WebUrl, appConfig.GenderDifferenceGridImageUrl, pokemon.Id, "-f.png"));
                    imageRequest = (HttpWebResponse)webRequest.GetResponse();
                    if (imageRequest.StatusCode == HttpStatusCode.OK)
                    {
                        pokemonViewModel.HasFemaleGenderDifference = true;
                    }
                }
                catch
                {
                }

                // Male Gender Difference Check
                try
                {
                    if (!pokemonViewModel.HasFemaleGenderDifference)
                    {
                        webRequest = (HttpWebRequest)WebRequest.Create(string.Concat(appConfig.WebUrl, appConfig.GenderDifferenceGridImageUrl, pokemon.Id, "-m.png"));
                        imageRequest = (HttpWebResponse)webRequest.GetResponse();
                        if (imageRequest.StatusCode == HttpStatusCode.OK)
                        {
                            pokemonViewModel.HasMaleGenderDifference = true;
                        }
                    }
                }
                catch
                {
                }
            }

            return pokemonViewModel;
        }

        /// <summary>
        /// Gets a list of all pokemon with form names for alternate forms.
        /// </summary>
        /// <param name="gameId">The game's id. Optional.</param>
        /// <param name="noBattleOnlyForms">Whether or not battle only forms will be excluded. Optional.</param>
        /// <returns>Returns the list of all pokemon.</returns>
        public List<Pokemon> GetPokemonWithFormNames(int gameId = 0, bool noBattleOnlyForms = false)
        {
            Game game = this.GetObjectByPropertyValue<Game>("Id", gameId);
            List<Pokemon> pokemonList = new List<Pokemon>();
            if (gameId != 0)
            {
                if (game.GenerationId == 1)
                {
                    // gameId is overwritten with the ID for Pokemon Gold.
                    gameId = 2;
                }

                pokemonList = this.GetObjects<PokemonGameDetail>(includes: "Pokemon, Pokemon.GenderRatio, Pokemon.Game, Pokemon.Form", whereProperty: "GameId", wherePropertyValue: gameId).ConvertAll(x => x.Pokemon).ToList();
                if (game.GenerationId == 1)
                {
                    // Celebi is removed as it can't be hunted in Generation 1.
                    pokemonList.Remove(pokemonList.Find(x => x.PokedexNumber == 251));
                }
            }
            else
            {
                pokemonList = this.GetObjects<Pokemon>(includes: "GenderRatio, Game, Form");
            }

            if (noBattleOnlyForms)
            {
                pokemonList = pokemonList.Where(x => !x.IsAltForm || (!x.Form.OnlyDuringBattle && !x.Form.FusionForm)).ToList();
            }

            pokemonList.Where(x => x.IsAltForm).ToList().ForEach(x => x.Name = x.NameWithForm);

            return pokemonList.OrderBy(x => x.PokedexNumber).ThenBy(x => x.Id).ToList();
        }

        /// <summary>
        /// Gets a list of all pokemon.
        /// </summary>
        /// <param name="gameId">The game's id. Optional.</param>
        /// <returns>Returns the list of all pokemon.</returns>
        public List<Pokemon> GetNonBattlePokemonWithFormNames(int gameId = 0)
        {
            List<Pokemon> pokemonList = this.GetPokemonWithFormNames(gameId, true);

            if (gameId != 0)
            {
                // Gets evolutions that are possible in other games.
                Game game = this.GetObjectByPropertyValue<Game>("Id", gameId);
                List<Evolution> evolutions = this.GetObjects<Evolution>("EvolutionPokemon.PokedexNumber, EvolutionPokemonId", "EvolutionPokemon, EvolutionPokemon.Game, EvolutionPokemon.Form, PreevolutionPokemon, PreevolutionPokemon.Game, PreevolutionPokemon.Form");
                List<Pokemon> futureEvolutions = evolutions.Where(x => pokemonList.Any(y => y.Id == x.PreevolutionPokemonId)).ToList().ConvertAll(x => x.EvolutionPokemon);
                futureEvolutions = futureEvolutions.Distinct().ToList();
                futureEvolutions.Where(x => x.IsAltForm).ToList().ForEach(x => x.Name = x.NameWithForm);
                pokemonList.AddRange(futureEvolutions);
            }

            return pokemonList.Distinct().OrderBy(x => x.PokedexNumber).ThenBy(x => x.Id).ToList();
        }

        /// <summary>
        /// Gets a list of all pokemon that are able to be bred in the day care.
        /// </summary>
        /// <param name="gameId">The game's id. Defaults to 0 when no game id is given.</param>
        /// <returns>The list of breedable pokemon.</returns>
        public List<PokemonEggGroupDetail> GetAllBreedablePokemon(int gameId = 0)
        {
            List<Pokemon> battleOnlyForms = this.GetObjects<Pokemon>(includes: "Form", whereProperty: "Form.OnlyDuringBattle", wherePropertyValue: true);
            List<PokemonEggGroupDetail> eggGroupDetails = this.GetObjects<PokemonEggGroupDetail>(includes: "Pokemon, Pokemon.Form, PrimaryEggGroup, SecondaryEggGroup");
            List<Pokemon> unbreedablePokemon = this.GetAllPokemon().Where(x => !eggGroupDetails.Any(y => y.PokemonId == x.Id && y.PrimaryEggGroupId != 15)).ToList();

            eggGroupDetails = eggGroupDetails.Where(x => !battleOnlyForms.Any(y => y.Id == x.PokemonId)).ToList();
            eggGroupDetails = eggGroupDetails.Where(x => !unbreedablePokemon.Any(y => y.Id == x.PokemonId)).ToList();
            eggGroupDetails = eggGroupDetails.Where(x => x.Pokemon.IsComplete).ToList();

            if (gameId != 0)
            {
                Game game = this.GetObjectByPropertyValue<Game>("Id", gameId);
                List<Pokemon> gameAvailability = this.GetObjects<PokemonGameDetail>(includes: "Pokemon", whereProperty: "GameId", wherePropertyValue: game.Id).ConvertAll(y => y.Pokemon);
                eggGroupDetails = eggGroupDetails.Where(x => gameAvailability.Any(z => z.Id == x.PokemonId)).ToList();
            }

            eggGroupDetails.Where(x => x.Pokemon.IsAltForm).ForEach(x => x.Pokemon.Name = x.Pokemon.NameWithForm);
            eggGroupDetails = eggGroupDetails.OrderBy(x => x.Pokemon.Name).ToList();

            return eggGroupDetails;
        }

        /// <summary>
        /// Gets a list of all pokemon, alongside all of their possible typings.
        /// </summary>
        /// <returns>Returns the pokemon type detail list.</returns>
        public List<PokemonTypeDetail> GetAllPokemonWithTypes()
        {
            List<PokemonTypeDetail> pokemonList = this.GetObjects<PokemonTypeDetail>("Pokemon.PokedexNumber, PokemonId", "Pokemon, Pokemon.Game, PrimaryType, SecondaryType").Where(x => !x.Pokemon.IsAltForm).ToList();
            List<int> pokemonIds = pokemonList.Select(x => x.PokemonId).Distinct().ToList();

            return pokemonList.OrderBy(x => x.GenerationId).GroupBy(x => new { x.PokemonId }).Select(x => x.LastOrDefault()).ToList();
        }

        /// <summary>
        /// Gets the abilities of a pokemon. This is will then be cut down to the abilities of either the latest game, or of the game specified.
        /// </summary>
        /// <param name="pokemonId">The pokemon id.</param>
        /// <param name="gameId">The game id.</param>
        /// <param name="user">The user.</param>
        /// <param name="appConfig">The application config.</param>
        /// <returns>Returns the list of abilities.</returns>
        public List<Ability> GetAbilitiesForPokemon(int pokemonId, int gameId, System.Security.Claims.ClaimsPrincipal user, AppConfig appConfig)
        {
            try
            {
                if (gameId == 0)
                {
                    List<Game> games = this.GetObjects<PokemonGameDetail>("Game.ReleaseDate, Game.Id", "Game", "PokemonId", pokemonId).Select(x => x.Game).ToList();
                    games = games.Where(x => x.Id != 37).ToList();
                    gameId = games.Where(x => x.ReleaseDate <= DateTime.Now).Last().Id;
                }

                List<Ability> abilityList = new List<Ability>();
                if (gameId != 1 && gameId != 21 && gameId != 20 && gameId != 2 && gameId != 22 && gameId != 23 && gameId != 37)
                {
                    Game game = this.GetObjectByPropertyValue<Game>("Id", gameId);
                    List<PokemonAbilityDetail> availableAbilityDetails = this.GetObjects<PokemonAbilityDetail>(includes: "Pokemon, PrimaryAbility, SecondaryAbility, HiddenAbility", whereProperty: "PokemonId", wherePropertyValue: pokemonId).OrderByDescending(x => x.GenerationId).ToList();
                    PokemonAbilityDetail pokemonAbilityDetail = availableAbilityDetails.Find(x => x.GenerationId <= game.GenerationId);

                    abilityList.Add(pokemonAbilityDetail.PrimaryAbility);
                    if (pokemonAbilityDetail.SecondaryAbility != null)
                    {
                        abilityList.Add(pokemonAbilityDetail.SecondaryAbility);
                    }

                    if (pokemonAbilityDetail.HiddenAbility != null)
                    {
                        abilityList.Add(pokemonAbilityDetail.HiddenAbility);
                    }
                }

                return abilityList;
            }
            catch (Exception e)
            {
                if (!user.IsInRole("Owner"))
                {
                    string commentBody;
                    Game game = this.GetObjectByPropertyValue<Game>("Id", gameId);
                    Pokemon pokemon = this.GetObjectByPropertyValue<Pokemon>("Id", pokemonId);

                    if (e != null)
                    {
                        commentBody = string.Concat(e.GetType().ToString(), " error while grabbing pokemon abilities.");
                    }
                    else
                    {
                        commentBody = "Unknown error while grabbing pokemon abilities.";
                    }

                    if (game != null)
                    {
                        commentBody = string.Concat(commentBody, " - Selected Game: ", game.Name);
                    }
                    else
                    {
                        commentBody = string.Concat(commentBody, " - Selected Game: ", gameId);
                    }

                    if (pokemon != null)
                    {
                        commentBody = string.Concat(commentBody, " - Selected Pokemon: ", pokemon.Name, " (ID: ", pokemon.Id, ")");
                    }
                    else
                    {
                        commentBody = string.Concat(commentBody, " - Selected Pokemon: ", pokemonId);
                    }

                    Comment comment = new Comment()
                    {
                        Name = commentBody,
                    };

                    if (user.Identity.Name != null)
                    {
                        comment.CommentorId = this.GetObjectByPropertyValue<User>("Username", user.Identity.Name).Id;
                    }

                    this.AddObject(comment);
                    this.EmailComment(appConfig, comment);
                }

                return null;
            }
        }

        /// <summary>
        /// Gets a list of all pokemon, alongside their capture rates.
        /// </summary>
        /// <returns>Returns the list of pokemon capture rate details.</returns>
        public List<PokemonCaptureRateDetail> GetAllPokemonWithCaptureRates()
        {
            return this.GetObjects<PokemonCaptureRateDetail>(includes: "Pokemon, CaptureRate, Generation")
                .OrderByDescending(x => x.GenerationId)
                .ToList();
        }

        /// <summary>
        /// Gets a list of all pokemon, alongside their base happinesses.
        /// </summary>
        /// <returns>Returns the list of pokemon base happiness details.</returns>
        public List<PokemonBaseHappinessDetail> GetAllPokemonWithBaseHappinesses()
        {
            return this.GetObjects<PokemonBaseHappinessDetail>(includes: "Pokemon, BaseHappiness, Generation")
                .OrderByDescending(x => x.GenerationId)
                .ToList();
        }

        /// <summary>
        /// Gets the base stats of a pokemon depending on the generation.
        /// </summary>
        /// <param name="pokemonId">The id of the pokemon.</param>
        /// <param name="generationId">The id of the generation.</param>
        /// <returns>The base stats of the pokemon by generation.</returns>
        public BaseStat GetPokemonBaseStats(int pokemonId, int generationId)
        {
            return this.GetObjects<BaseStat>(includes: "Pokemon")
                .ToList()
                .Find(x => x.PokemonId == pokemonId && x.GenerationId == generationId);
        }

        /// <summary>
        /// Gets the ev yield of a pokemon depending on the generation.
        /// </summary>
        /// <param name="pokemonId">The id of the pokemon.</param>
        /// <param name="generationId">The id of the generation.</param>
        /// <returns>The ev yield of the pokemon by generation.</returns>
        public EVYield GetPokemonEVYields(int pokemonId, int generationId)
        {
            return this.GetObjects<EVYield>(includes: "Pokemon")
                .ToList()
                .Find(x => x.PokemonId == pokemonId && x.GenerationId == generationId);
        }

        /// <summary>
        /// Gets the type chart by the defending type.
        /// </summary>
        /// <param name="typeId">The type's id.</param>
        /// <param name="generationId">The generation's id.</param>
        /// <returns>The type chart by the defending type.</returns>
        public List<TypeChart> GetTypeChartByDefendType(int typeId, int generationId)
        {
            return this.GetObjects<TypeChart>("AttackId, DefendId", "Attack, Defend")
                .Where(x => x.DefendId == typeId && x.GenerationId == generationId)
                .ToList();
        }

        /// <summary>
        /// Gets the type chart of a particular pokemon.
        /// </summary>
        /// <param name="pokemonId">The id of the pokemon.</param>
        /// <returns>The view model used for the pokemon's type chart.</returns>
        public List<PokemonTypeChartViewModel> GetTypeChartPokemon(int pokemonId)
        {
            List<DataAccess.Models.Type> typeList = this.GetObjects<DataAccess.Models.Type>("Name");
            List<PokemonTypeDetail> pokemonTypes = this.GetObjects<PokemonTypeDetail>(includes: "Pokemon, PrimaryType, SecondaryType, Generation", whereProperty: "PokemonId", wherePropertyValue: pokemonId);
            List<TypeChart> typeChart = this.GetObjects<TypeChart>("AttackId, DefendId", "Attack, Defend");
            List<TypeChart> primaryTypeChart = new List<TypeChart>();
            List<TypeChart> secondaryTypeChart = new List<TypeChart>();
            List<PokemonTypeChartViewModel> pokemonTypeCharts = new List<PokemonTypeChartViewModel>();

            foreach (var t in pokemonTypes)
            {
                if (t.SecondaryType != null)
                {
                    secondaryTypeChart = typeChart.Where(x => x.DefendId == t.SecondaryTypeId).ToList();
                }

                primaryTypeChart = typeChart.Where(x => x.DefendId == t.PrimaryTypeId).ToList();

                if (secondaryTypeChart.Count > 0)
                {
                    primaryTypeChart.AddRange(secondaryTypeChart);
                }

                List<TypeChart> finalTypeChart = new List<TypeChart>();
                finalTypeChart.AddRange(primaryTypeChart);

                foreach (var i in primaryTypeChart.Where(x => x.Effective == 0))
                {
                    finalTypeChart.RemoveAll(x => x.AttackId == i.AttackId && x.Effective != 0 && x.GenerationId == i.GenerationId);
                }

                pokemonTypeCharts.Add(new PokemonTypeChartViewModel()
                {
                    TypeChart = finalTypeChart,
                    Generation = t.Generation,
                });
            }

            return pokemonTypeCharts;
        }

        /// <summary>
        /// Adds a pokemon team detail to the database, and ensures that there is an entry for the pokemon's EV, IV, and moveset.
        /// </summary>
        /// <param name="pokemonTeamDetail">The pokemon team detail that is being added.</param>
        /// <returns>The id of the new pokemon team detail.</returns>
        public int AddPokemonTeamDetail(PokemonTeamDetail pokemonTeamDetail)
        {
            if (pokemonTeamDetail.PokemonTeamEVId == null)
            {
                PokemonTeamEV pokemonTeamEV = new PokemonTeamEV();
                this.AddObject(pokemonTeamEV);
                pokemonTeamDetail.PokemonTeamEVId = pokemonTeamEV.Id;
            }

            if (pokemonTeamDetail.PokemonTeamIVId == null)
            {
                PokemonTeamIV pokemonTeamIV = new PokemonTeamIV();
                this.AddObject(pokemonTeamIV);
                pokemonTeamDetail.PokemonTeamIVId = pokemonTeamIV.Id;
            }

            if (pokemonTeamDetail.PokemonTeamMovesetId == null)
            {
                PokemonTeamMoveset pokemonTeamMoveset = new PokemonTeamMoveset();
                this.AddObject(pokemonTeamMoveset);
                pokemonTeamDetail.PokemonTeamMovesetId = pokemonTeamMoveset.Id;
            }

            this.AddObject(pokemonTeamDetail);
            return pokemonTeamDetail.Id;
        }

        /// <summary>
        /// Deletes a pokemon team.
        /// </summary>
        /// <param name="id">The id of the team being deleted.</param>
        /// <param name="user">The user that called this method. Only used if there is an error.</param>
        /// <param name="appConfig">The application's config. Only used if there is an error.</param>
        public void DeletePokemonTeam(int id, System.Security.Claims.ClaimsPrincipal user, AppConfig appConfig)
        {
            int teamDetailId = 0;
            List<int> pokemonTeamDetailIds = new List<int>();

            try
            {
                PokemonTeam pokemonTeam = this.GetObjectByPropertyValue<PokemonTeam>("Id", id);
                pokemonTeamDetailIds = pokemonTeam.GrabPokemonTeamDetailIds();
                this.DeleteObject<PokemonTeam>(pokemonTeam.Id);
            }
            catch (Exception e)
            {
                if (!user.IsInRole("Owner") && e != null)
                {
                    string commentBody;
                    if (e != null)
                    {
                        commentBody = string.Concat(e.GetType().ToString(), " error during the pokemon team's deletion.");
                    }
                    else
                    {
                        commentBody = "Unknown error during the pokemon team's deletion.";
                    }

                    commentBody = string.Concat(commentBody, " - Deleted Team Id: {", id, "}");
                    if (pokemonTeamDetailIds.Count() > 0)
                    {
                        commentBody = string.Concat(commentBody, " - Deleted Team Detail Id(s): {", string.Join(", ", pokemonTeamDetailIds), "}");
                    }

                    Comment comment = new Comment()
                    {
                        Name = commentBody,
                    };

                    if (user.Identity.Name != null)
                    {
                        comment.CommentorId = this.GetObjectByPropertyValue<User>("Username", user.Identity.Name).Id;
                    }

                    this.AddObject(comment);
                    this.EmailComment(appConfig, comment);
                }
            }

            if (pokemonTeamDetailIds.Count() > 0)
            {
                try
                {
                    foreach (var p in pokemonTeamDetailIds)
                    {
                        teamDetailId = p;
                        this.DeleteObject<PokemonTeamDetail>(teamDetailId);
                    }
                }
                catch (Exception e)
                {
                    if (!user.IsInRole("Owner") && e != null)
                    {
                        string commentBody;
                        if (e != null)
                        {
                            commentBody = string.Concat(e.GetType().ToString(), " error during the pokemon team detail's deletion.");
                        }
                        else
                        {
                            commentBody = "Unknown error during the pokemon team's deletion.";
                        }

                        commentBody = string.Concat(commentBody, " - Deleted Team Id: {", id, "}");
                        commentBody = string.Concat(commentBody, " - Deleted Team Detail Id: {", string.Join(", ", teamDetailId), "}");

                        Comment comment = new Comment()
                        {
                            Name = commentBody,
                        };

                        if (user.Identity.Name != null)
                        {
                            comment.CommentorId = this.GetObjectByPropertyValue<User>("Username", user.Identity.Name).Id;
                        }

                        this.AddObject(comment);
                        this.EmailComment(appConfig, comment);
                    }
                }
            }
        }

        /// <summary>
        /// Gets a list of all of the pokemon teams a user has, sorted with no games first, then by the game itself.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <returns>A list of sorted pokemon teams.</returns>
        public List<PokemonTeam> GetPokemonTeams(string username)
        {
            List<PokemonTeam> pokemonTeams = new List<PokemonTeam>();
            List<PokemonTeam> teams = this.GetObjects<PokemonTeam>("Id", "User, Game, FirstPokemon, FirstPokemon.Pokemon, SecondPokemon, SecondPokemon.Pokemon, ThirdPokemon, ThirdPokemon.Pokemon, FourthPokemon, FourthPokemon.Pokemon, FifthPokemon, FifthPokemon.Pokemon, SixthPokemon, SixthPokemon.Pokemon", "User.Username", username);
            if (teams.Count() > 0)
            {
                List<PokemonTeam> teamsWithGames = teams.Where(x => x.Game != null).OrderBy(x => x.Game.ReleaseDate).ThenBy(x => x.GameId).ThenBy(x => x.Id).ToList();
                List<PokemonTeam> teamsWithoutGames = teams.Where(x => x.Game == null).OrderBy(x => x.Id).ToList();
                pokemonTeams = teamsWithoutGames;
                pokemonTeams.AddRange(teamsWithGames);
            }

            return pokemonTeams;
        }

        /// <summary>
        /// Gets a list of huntable pokemon.
        /// </summary>
        /// <param name="gameId">The game's id. Defaults to 0.</param>
        /// <returns>A list of pokemon huntable.</returns>
        public List<Pokemon> GetHuntablePokemon(int gameId = 0)
        {
            List<Pokemon> pokemonList = this.GetNonBattlePokemonWithFormNames(gameId).Where(x => !x.IsShinyLocked).ToList();
            if (gameId != 0)
            {
                Game game = this.GetObjectByPropertyValue<Game>("Id", gameId);

                // Gets extra forms made available in future generations.
                if (game.GenerationId == 3)
                {
                    pokemonList.Remove(pokemonList.Find(x => x.Name.Contains("Deoxys")));
                    List<Pokemon> deoxysList = this.GetObjects<Pokemon>(includes: "GenderRatio, Form").Where(x => x.Name == "Deoxys").ToList();
                    deoxysList.Where(x => x.IsAltForm).ToList().ForEach(x => x.Name = x.NameWithForm);
                    pokemonList.AddRange(deoxysList);
                }
                else if (game.GenerationId == 6)
                {
                    pokemonList.Remove(pokemonList.Find(x => x.Name.Contains("Zygarde")));
                    List<Pokemon> zygardeList = this.GetObjects<Pokemon>(includes: "GenderRatio, Form").Where(x => x.Name == "Zygarde" && (!x.IsAltForm || x.Form.Name != "Complete")).ToList();
                    zygardeList.Where(x => x.IsAltForm).ToList().ForEach(x => x.Name = x.NameWithForm);
                    pokemonList.AddRange(zygardeList);
                }
            }

            // Removes Minior's Shields Up form
            pokemonList.Remove(pokemonList.Find(x => x.Id == 1302));

            return pokemonList.OrderBy(x => x.PokedexNumber).ThenBy(x => x.Id).ToList();
        }

        /// <summary>
        /// Gets a list of marks.
        /// </summary>
        /// <param name="gameId">The game's id. Defaults to 0.</param>
        /// <returns>A list of marks.</returns>
        public List<Mark> GetMarks(int gameId = 0)
        {
            List<Mark> marks = new List<Mark>();
            if (gameId == 0)
            {
                marks = this.GetObjects<Mark>("Name");
            }
            else
            {
                marks = this.GetObjects<Mark>("Name", whereProperty: "GameId", wherePropertyValue: gameId);
            }

            return marks;
        }

        /// <summary>
        /// Gets the list of games possible to shiny hunt in and formats it for the shiny hunt pages.
        /// </summary>
        /// <param name="pokemonId">The pokemon's id.</param>
        /// <returns>The list of shiny huntable games.</returns>
        public List<Game> GetMultipleShinyHuntGames(List<int> pokemonId)
        {
            List<Game> huntableGames = new List<Game>();
            if (pokemonId.Exists(x => x == 0))
            {
                huntableGames = this.GetGamesGroupedByReleaseDate().ToList();
            }
            else
            {
                List<Pokemon> pokemonList = this.GetObjects<Pokemon>(includes: "Game").Where(x => pokemonId.Any(y => y == x.Id)).ToList();
                List<PokemonGameDetail> pokemonGameDetails = this.GetObjects<PokemonGameDetail>("GameId", "Game");
                List<Game> gamesAvailable = this.GetGamesGroupedByReleaseDate().ToList();
                List<Game> pokemonGamesIn = new List<Game>();
                List<Game> possibleGames = new List<Game>();

                foreach (var p in pokemonList)
                {
                    pokemonGamesIn = pokemonGameDetails.Where(x => x.PokemonId == p.Id).ToList().ConvertAll(x => x.Game);
                    possibleGames = gamesAvailable.Where(x => pokemonGamesIn.Any(y => y.Id == x.Id)).ToList();

                    // Gets other games for extra forms introduced in future generations.
                    if (p.Name == "Deoxys")
                    {
                        possibleGames = possibleGames.Where(x => x.GenerationId >= 4).ToList();
                        possibleGames.AddRange(gamesAvailable.Where(x => x.GenerationId == 3 && x.Id != 39 && x.Id != 40));
                    }
                    else if (p.Name == "Zygarde")
                    {
                        possibleGames = possibleGames.Where(x => x.GenerationId >= 7).ToList();
                        possibleGames.AddRange(gamesAvailable.Where(x => x.GenerationId == 6));
                    }

                    if (p.Game.GenerationId <= 2 && p.PokedexNumber != 251)
                    {
                        possibleGames.InsertRange(0, gamesAvailable.Where(x => x.GenerationId == 1));
                    }

                    // Gets other games for evolutions introduced in future generations.
                    List<Evolution> evolutionList = this.GetObjects<Evolution>(includes: "PreevolutionPokemon");
                    Evolution evolution = evolutionList.Find(x => x.EvolutionPokemonId == p.Id);
                    int preevolution = p.Id;
                    while (evolution != null && evolution.PreevolutionPokemon != null)
                    {
                        preevolution = evolution.PreevolutionPokemonId;
                        evolution = evolutionList.Find(x => x.EvolutionPokemonId == preevolution);
                    }

                    if (preevolution != p.Id)
                    {
                        pokemonGamesIn = pokemonGameDetails.Where(x => x.PokemonId == preevolution).ToList().ConvertAll(x => x.Game);
                        gamesAvailable = gamesAvailable.Where(x => pokemonGamesIn.Any(y => y.Id == x.Id)).ToList();
                        possibleGames.AddRange(gamesAvailable.Where(x => !possibleGames.Any(y => y.Id == x.Id)).ToList());
                    }

                    if (huntableGames.Count() == 0)
                    {
                        huntableGames = possibleGames;
                    }
                    else
                    {
                        huntableGames = huntableGames.Where(x => possibleGames.Any(y => y.Id == x.Id)).ToList();
                    }
                }
            }

            return huntableGames.OrderByDescending(x => x.GenerationId).ThenByDescending(x => x.Id).ToList();
        }

        /// <summary>
        /// Gets the list of games possible to shiny hunt in and formats it for the shiny hunt pages.
        /// </summary>
        /// <param name="pokemonId">The pokemon's id.</param>
        /// <returns>The list of shiny huntable games.</returns>
        public List<Game> GetShinyHuntGames(int? pokemonId)
        {
            List<Game> possibleGames = new List<Game>();
            if (pokemonId == null)
            {
                possibleGames = this.GetGamesGroupedByReleaseDate().ToList();
            }
            else
            {
                List<Pokemon> pokemonList = this.GetObjects<Pokemon>(includes: "Game");
                Pokemon pokemon = pokemonList.Find(x => x.Id == (int)pokemonId);
                List<PokemonGameDetail> pokemonGameDetails = this.GetObjects<PokemonGameDetail>("GameId", "Game");
                List<Game> pokemonGamesIn = pokemonGameDetails.Where(x => x.PokemonId == pokemon.Id).ToList().ConvertAll(x => x.Game);
                List<Game> gamesAvailable = this.GetGamesGroupedByReleaseDate().ToList();
                possibleGames = gamesAvailable.Where(x => pokemonGamesIn.Any(y => y.Id == x.Id)).ToList();

                // Gets other games for extra forms introduced in future generations.
                if (pokemon.Name == "Deoxys")
                {
                    possibleGames = possibleGames.Where(x => x.GenerationId >= 4).ToList();
                    possibleGames.AddRange(gamesAvailable.Where(x => x.GenerationId == 3 && x.Id != 39 && x.Id != 40));
                }
                else if (pokemon.Name == "Zygarde")
                {
                    possibleGames = possibleGames.Where(x => x.GenerationId >= 7).ToList();
                    possibleGames.AddRange(gamesAvailable.Where(x => x.GenerationId == 6));
                }

                if (pokemon.Game.GenerationId <= 2 && pokemon.PokedexNumber != 251)
                {
                    possibleGames.InsertRange(0, gamesAvailable.Where(x => x.GenerationId == 1));
                }

                // Gets other games for evolutions introduced in future generations.
                List<Evolution> evolutionList = this.GetObjects<Evolution>(includes: "PreevolutionPokemon");
                Evolution evolution = evolutionList.Find(x => x.EvolutionPokemonId == pokemon.Id);
                int preevolution = pokemon.Id;
                while (evolution != null && evolution.PreevolutionPokemon != null)
                {
                    preevolution = evolution.PreevolutionPokemonId;
                    evolution = evolutionList.Find(x => x.EvolutionPokemonId == preevolution);
                }

                if (preevolution != pokemon.Id)
                {
                    pokemonGamesIn = pokemonGameDetails.Where(x => x.PokemonId == preevolution).ToList().ConvertAll(x => x.Game);
                    gamesAvailable = gamesAvailable.Where(x => pokemonGamesIn.Any(y => y.Id == x.Id)).ToList();
                    possibleGames.AddRange(gamesAvailable.Where(x => !possibleGames.Any(y => y.Id == x.Id)).ToList());
                }
            }

            return possibleGames.OrderBy(x => x.GenerationId).ThenBy(x => x.Id).ToList();
        }

        /// <summary>
        /// Gets the list of games grouped by release date.
        /// </summary>
        /// <returns>The list of games grouped by release date.</returns>
        public List<Game> GetGamesGroupedByReleaseDate()
        {
            List<Game> gamesList = new List<Game>();
            List<Game> selectableGames = new List<Game>();
            gamesList = this.GetObjects<Game>("ReleaseDate, Id").Where(x => x.ReleaseDate <= DateTime.Now).ToList();
            List<Game> uniqueGames = gamesList.OrderBy(x => x.ReleaseDate).ThenBy(x => x.Id).DistinctBy(y => y.ReleaseDate).ToList();
            for (var i = 0; i < uniqueGames.Count; i++)
            {
                if (uniqueGames[i].Name == "Fire Red")
                {
                    selectableGames.Add(uniqueGames[i]);
                    selectableGames.Add(this.GetObjectByPropertyValue<Game>("Name", "Leaf Green"));
                }
                else if (i == uniqueGames.Count - 1)
                {
                    selectableGames.Add(new Game()
                    {
                        Id = uniqueGames[i].Id,
                        Name = string.Join(" / ", gamesList.Where(x => x.ReleaseDate >= uniqueGames[i].ReleaseDate).Select(x => x.Name)),
                        GenerationId = uniqueGames[i].GenerationId,
                        IsBreedingPossible = uniqueGames[i].IsBreedingPossible,
                    });
                }
                else
                {
                    List<Game> games = gamesList.Where(x => x.ReleaseDate >= uniqueGames[i].ReleaseDate && x.ReleaseDate < uniqueGames[i + 1].ReleaseDate && !selectableGames.Any(y => y.ReleaseDate == x.ReleaseDate)).ToList();
                    if (games.Count == 0)
                    {
                        selectableGames.Add(uniqueGames[i]);
                    }
                    else
                    {
                        selectableGames.Add(new Game()
                        {
                            Id = uniqueGames[i].Id,
                            Name = string.Join(" / ", games.ConvertAll(x => x.Name)),
                            GenerationId = uniqueGames[i].GenerationId,
                            IsBreedingPossible = uniqueGames[i].IsBreedingPossible,
                        });
                    }
                }
            }

            return selectableGames;
        }

        /// <summary>
        /// Gets a list of pokeballs available for shiny hunts.
        /// </summary>
        /// <param name="gameId">The game the shiny hunt is taking place in.</param>
        /// <param name="huntingMethodId">The type of shiny hunt.</param>
        /// <param name="user">The user.</param>
        /// <param name="appConfig">The application config.</param>
        /// <returns>The available pokeballs.</returns>
        public List<Pokeball> GetPokeballs(int gameId, int huntingMethodId, System.Security.Claims.ClaimsPrincipal user, AppConfig appConfig)
        {
            try
            {
                Game game = this.GetObjectByPropertyValue<Game>("Id", gameId);
                HuntingMethod huntingMethod = this.GetObjectByPropertyValue<HuntingMethod>("Id", huntingMethodId);
                List<Pokeball> selectablePokeballs = new List<Pokeball>();
                switch (huntingMethod.Name)
                {
                    case "Egg Hatching":
                    case "Masuda Method":
                        if (game.GenerationId <= 5)
                        {
                            selectablePokeballs = this.GetObjects<Pokeball>(whereProperty: "Id", wherePropertyValue: 1);
                        }

                        break;
                    case "Event":
                    case "HOME Pokedex Completion":
                        if (game.GenerationId <= 3)
                        {
                            selectablePokeballs = this.GetObjects<Pokeball>(whereProperty: "Id", wherePropertyValue: 1);
                        }
                        else
                        {
                            selectablePokeballs = this.GetObjects<Pokeball>(whereProperty: "Id", wherePropertyValue: 24);
                        }

                        break;
                    default:
                        selectablePokeballs = this.GetObjects<PokeballGameDetail>("Pokeball.Name", "Pokeball", "GameId", gameId).Select(x => x.Pokeball).ToList();
                        break;
                }

                return selectablePokeballs;
            }
            catch (Exception e)
            {
                if (!user.IsInRole("Owner"))
                {
                    string commentBody;
                    Game game = this.GetObjectByPropertyValue<Game>("Id", gameId);
                    HuntingMethod huntingMethod = this.GetObjectByPropertyValue<HuntingMethod>("Id", huntingMethodId);

                    if (e != null)
                    {
                        commentBody = string.Concat(e.GetType().ToString(), " error while grabbing shiny hunting pokeballs.");
                    }
                    else
                    {
                        commentBody = "Unknown error while grabbing shiny hunting pokeballs.";
                    }

                    if (game != null)
                    {
                        commentBody = string.Concat(commentBody, " - Selected Game: ", game.Name);
                    }
                    else
                    {
                        commentBody = string.Concat(commentBody, " - Selected Game: ", gameId);
                    }

                    if (huntingMethod != null)
                    {
                        commentBody = string.Concat(commentBody, " - Selected Hunting Method: ", huntingMethod.Name, " (ID: ", huntingMethod.Id, ")");
                    }
                    else
                    {
                        commentBody = string.Concat(commentBody, " - Selected Hunting Method: ", huntingMethodId);
                    }

                    Comment comment = new Comment()
                    {
                        Name = commentBody,
                    };

                    if (user.Identity.Name != null)
                    {
                        comment.CommentorId = this.GetObjectByPropertyValue<User>("Username", user.Identity.Name).Id;
                    }

                    this.AddObject(comment);
                    this.EmailComment(appConfig, comment);
                }

                return null;
            }
        }

        /// <summary>
        /// Creates a list of strings for the available genders of a pokemon.
        /// </summary>
        /// <param name="pokemonId">The pokemon's id.</param>
        /// <param name="isShinyHunt">Whether or not this method is used for a shiny hunt. Only used to determine what no gender is listed as.</param>
        /// <returns>A list of genders in string form.</returns>
        public List<string> GrabGenders(int? pokemonId, bool isShinyHunt)
        {
            List<string> genders = new List<string>();
            if (pokemonId == 0)
            {
                genders.Add("Gender Unknown");
            }
            else
            {
                GenderRatio genderRatio = this.GetObjectByPropertyValue<Pokemon>("Id", pokemonId, "EggCycle, GenderRatio, Classification, Game, Game.Generation, ExperienceGrowth").GenderRatio;
                if (genderRatio.MaleRatio == 0 && genderRatio.FemaleRatio == 0)
                {
                    if (isShinyHunt)
                    {
                        genders.Add("Gender Unknown");
                    }
                    else
                    {
                        genders.Add("None");
                    }
                }
                else if (genderRatio.MaleRatio == 0)
                {
                    genders.Add("Female");
                }
                else if (genderRatio.FemaleRatio == 0)
                {
                    genders.Add("Male");
                }
                else
                {
                    genders.Add("No Specific Gender");
                    genders.Add("Male");
                    genders.Add("Female");
                }
            }

            return genders;
        }

        /// <summary>
        /// Uploades a given image to the pokemon images folder.
        /// </summary>
        /// <param name="fileUpload">The file being uploaded.</param>
        /// <param name="urlUpload">The URL of an image if no file is provided.</param>
        /// <param name="fileName">The name the image will have on the server.</param>
        /// <param name="appConfig">The application config.</param>
        /// <param name="imageType">What type of image is being uploaded.</param>
        public async void UploadImages(IFormFile fileUpload, string urlUpload, string fileName, AppConfig appConfig, ImageType imageType)
        {
            string imageUrlPath = string.Empty;
            string iconUrlPath = string.Empty;
            string gridUrlPath = string.Empty;
            IFormFile upload;
            IFormFile resizedUpload;

            switch (imageType)
            {
                case ImageType.Official:
                    imageUrlPath = appConfig.OfficialPokemonImageFTPUrl;
                    iconUrlPath = appConfig.IconImageFTPUrl;
                    gridUrlPath = appConfig.GridImageFTPUrl;
                    break;
                case ImageType.Home:
                    imageUrlPath = appConfig.HomePokemonImageFTPUrl;
                    break;
                case ImageType.Shiny:
                    imageUrlPath = appConfig.ShinyPokemonImageFTPUrl;
                    gridUrlPath = appConfig.ShinyGridImageFTPUrl;
                    break;
                case ImageType.Pokeball:
                    imageUrlPath = appConfig.OfficialPokeballImageFTPUrl;
                    break;
                case ImageType.Mark:
                    imageUrlPath = appConfig.OfficialMarkImageFTPUrl;
                    break;
                case ImageType.Sweet:
                    imageUrlPath = appConfig.OfficialSweetImageFTPUrl;
                    break;
                case ImageType.GenderDifference:
                    imageUrlPath = appConfig.GenderDifferencePokemonImageFTPUrl;
                    gridUrlPath = appConfig.GenderDifferenceGridImageFTPUrl;
                    break;
                case ImageType.GenderDifferenceHome:
                    imageUrlPath = appConfig.GenderDifferenceHomePokemonImageFTPUrl;
                    break;
                case ImageType.GenderDifferenceShiny:
                    imageUrlPath = appConfig.GenderDifferenceShinyPokemonImageFTPUrl;
                    gridUrlPath = appConfig.GenderDifferenceShinyGridImageFTPUrl;
                    break;
            }

            if (fileUpload == null && urlUpload != null)
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

            if (upload != null)
            {
                if (imageType == ImageType.Pokeball || imageType == ImageType.Mark)
                {
                    resizedUpload = this.TrimImage(upload, 50, 50, true);
                }
                else
                {
                    resizedUpload = this.TrimImage(upload, 350, 350);
                }

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(string.Concat(appConfig.FTPUrl, imageUrlPath, fileName, ".png"));
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(appConfig.FTPUsername, appConfig.FTPPassword);

                using (Stream requestStream = request.GetRequestStream())
                {
                    await resizedUpload.CopyToAsync(requestStream).ConfigureAwait(false);
                }

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    Console.WriteLine($"Upload File Complete, status {response.StatusDescription}");
                }

                if (!string.IsNullOrEmpty(gridUrlPath))
                {
                    IFormFile gridUpload = this.TrimImage(upload, 150, 150);

                    request = (FtpWebRequest)WebRequest.Create(string.Concat(appConfig.FTPUrl, gridUrlPath, fileName, ".png"));
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.Credentials = new NetworkCredential(appConfig.FTPUsername, appConfig.FTPPassword);

                    using (Stream requestStream = request.GetRequestStream())
                    {
                        await gridUpload.CopyToAsync(requestStream).ConfigureAwait(false);
                    }

                    using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                    {
                        Console.WriteLine($"Upload File Complete, status {response.StatusDescription}");
                    }
                }

                if (!string.IsNullOrEmpty(iconUrlPath))
                {
                    IFormFile iconUpload = this.TrimImage(upload, 64, 64, true);

                    request = (FtpWebRequest)WebRequest.Create(string.Concat(appConfig.FTPUrl, iconUrlPath, fileName, ".png"));
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.Credentials = new NetworkCredential(appConfig.FTPUsername, appConfig.FTPPassword);

                    using (Stream requestStream = request.GetRequestStream())
                    {
                        await iconUpload.CopyToAsync(requestStream).ConfigureAwait(false);
                    }

                    using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                    {
                        Console.WriteLine($"Upload File Complete, status {response.StatusDescription}");
                    }
                }
            }
            else
            {
                WebClient webRequest = new WebClient
                {
                    Credentials = new NetworkCredential(appConfig.FTPUsername, appConfig.FTPPassword),
                };

                byte[] file = webRequest.DownloadData(string.Concat(appConfig.WebUrl, "/images/general/tempPhoto.png"));

                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(string.Concat(appConfig.FTPUrl, imageUrlPath, fileName, ".png"));
                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                ftpRequest.Credentials = new NetworkCredential(appConfig.FTPUsername, appConfig.FTPPassword);

                using (Stream requestStream = ftpRequest.GetRequestStream())
                {
                    requestStream.Write(file, 0, file.Length);
                }

                using FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
                Console.WriteLine($"Upload File Complete, status {response.StatusDescription}");
            }
        }

        /// <summary>
        /// Gets the User class for the current user using the ClaimsPrincipal object.
        /// </summary>
        /// <param name="user">The ClaimsPrincipal object of the current user.</param>
        /// <returns>The current user's User object.</returns>
        public User GetCurrentUser(System.Security.Claims.ClaimsPrincipal user)
        {
            return this.GetObjectByPropertyValue<User>("Username", user.Identity.Name);
        }

        /// <summary>
        /// Sends an email with a given comment.
        /// </summary>
        /// <param name="appConfig">The configuration for the application.</param>
        /// <param name="comment">The email's message.</param>
        public void EmailComment(AppConfig appConfig, Comment comment)
        {
            try
            {
                if (comment.CommentorId != 1)
                {
                    MailAddress fromAddress = new MailAddress(appConfig.SenderEmailAddress, "Pokemon Database Website");
                    MailAddress toAddress = new MailAddress(appConfig.ReceiverEmailAddress, "Pokemon Database Email");
                    MailMessage message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = string.Concat("New Comment for ", appConfig.AppName),
                        Body = "Comment",
                    };

                    if (comment.CommentorId != null)
                    {
                        message.Body += string.Concat(" by ", this.GetObjectByPropertyValue<User>("Id", (int)comment.CommentorId).Username);
                    }
                    else
                    {
                        message.Body += string.Concat(" by Anonymous User");
                    }

                    message.Body += string.Concat(": ", comment.Name);

                    SmtpClient smtp = new SmtpClient()
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(fromAddress.Address, appConfig.SenderEmailAddressPassword),
                    };
                    smtp.Send(message);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Email could not be sent. ", (ex.InnerException != null) ? ex.InnerException.ToString() : ex.Message);
            }
        }

        /// <summary>
        /// Updates the boolean fields of a pokemon to mark an image as available.
        /// </summary>
        /// <param name="pokemonId">The Id of the pokemon that will be updated.</param>
        /// <param name="imageType">The type of image that is available. Options are Shiny or ThreeD.</param>
        public void UpdateImageBools(int pokemonId, ImageType imageType)
        {
            Pokemon pokemon = this.GetObjectByPropertyValue<Pokemon>("Id", pokemonId);
            switch (imageType)
            {
                case ImageType.Shiny:
                    pokemon.HasShinyArtwork = true;
                    break;
                case ImageType.Home:
                    pokemon.HasHomeArtwork = true;
                    break;
                default:
                    return;
            }

            this.UpdateObject(pokemon);
        }

        /// <summary>
        /// Returns the two pokemon that surround the searched pokemon.
        /// </summary>
        /// <param name="pokedexNumber">The viewed Pokemon.</param>
        /// <returns>Returns the two pokemon who's pokedex number surround this pokemon's pokedex number.</returns>
        private List<Pokemon> GetSurroundingPokemon(int pokedexNumber)
        {
            List<Pokemon> pokemonList = this.GetAllPokemon().OrderBy(x => x.PokedexNumber).ThenBy(x => x.Id).ToList();
            Pokemon priorPokemon = pokemonList.FirstOrDefault(x => x.PokedexNumber == (pokedexNumber - 1));
            Pokemon nextPokemon = pokemonList.FirstOrDefault(x => x.PokedexNumber == (pokedexNumber + 1));

            if (priorPokemon == null)
            {
                if (pokemonList.First().PokedexNumber != pokedexNumber)
                {
                    priorPokemon = pokemonList.Last(x => x.PokedexNumber < pokedexNumber);
                }
                else
                {
                    priorPokemon = pokemonList.Last();
                }
            }

            if (nextPokemon == null)
            {
                if (pokemonList.Last().PokedexNumber != pokedexNumber)
                {
                    nextPokemon = pokemonList.First(x => x.PokedexNumber > pokedexNumber);
                }
                else
                {
                    nextPokemon = pokemonList.First();
                }
            }

            List<Pokemon> surroundingPokemon = new List<Pokemon>
            {
                priorPokemon,
                nextPokemon,
            };

            return surroundingPokemon;
        }

        /// <summary>
        /// Formats the image for use as a main page image.
        /// </summary>
        /// <param name="file">The file being formatted.</param>
        /// <param name="width">The new width of the image. Defaults is 0.</param>
        /// <param name="height">The new height of the image. Defaults is 0.</param>
        /// <param name="icon">Whether or not this image is an icon. Default is false.</param>
        /// <returns>The formatted file.</returns>
        private IFormFile TrimImage(IFormFile file, int width = 0, int height = 0, bool icon = false)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                file.CopyTo(ms);
                byte[] fileBytes = ms.ToArray();
                using MagickImage image = new MagickImage(fileBytes);
                if (!icon)
                {
                    image.Trim();
                }

                image.Resize(new MagickGeometry(Convert.ToUInt32(width), Convert.ToUInt32(height)));
                MemoryStream strm = new MemoryStream();
                image.ResetPage();
                if (icon)
                {
                    image.Quality = 75;
                }
                else
                {
                    image.Quality = 100;
                }

                image.Settings.SetDefine(MagickFormat.WebP, "lossless", true);
                image.Settings.SetDefine(MagickFormat.WebP, "method", "6");
                image.Write(strm, MagickFormat.Png);
                file = new FormFile(strm, 0, strm.Length, file.Name, file.FileName);
            }

            return file;
        }
    }
}

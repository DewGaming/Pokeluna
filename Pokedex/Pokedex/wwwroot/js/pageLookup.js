var currentGeneration = 0, sortMethod = "Pokedex", orderGameId, orderIsShared, orderUsername;

function lookupPage(pageName) {
  $('.active').each(function () {
    $(this).removeClass('active');
  })

  $('.' + pageName).addClass('active');
  $('#' + pageName).addClass('active');
}

function lookupTypeChart(generation) {
  $('.active').each(function () {
    $(this).removeClass('active');
  })

  $('button#Generation' + generation + ', .grid-row#Generation' + generation).addClass('active');
}

function lookupPokemon(pageName, iconLink, pokemonName, generation) {
  $('.active').each(function () {
    $(this).removeClass('active');
  })

  var imageUsed = $('.current').attr('class').split(' ')[0];

  pokemonName = pokemonName.replace("&apos;", "\'");

  while ($('.generation' + generation + '#' + pageName).length == 0) {
    generation -= 1
    if (generation == 0) {
      generation = Number($('#LatestGenerationId').val())
    }
  }

  $('button#Generation' + generation + ', .generations#' + pageName + ', .generation' + generation + '#' + pageName).addClass('active');

  if (!$('.active .pokemonImage>div').hasClass(imageUsed)) {
    $('.current').each(function () {
      $(this).removeClass('current');
    })

    $('.pokemonImage .official').each(function () {
      $(this).addClass('current');
    })

    $('.pokemonImageButtons button').each(function () {
      $(this).removeClass('hidden');
    })

    $('.pokemonImageButtons .officialButton').each(function () {
      $(this).addClass('hidden');
    })
  
    if ($('.genderSign.default').hasClass('hidden')) {
      $('.genderSign.default').removeClass('hidden');
      $('.genderSign.difference').addClass('hidden');
    }
  }

  $('.tabIcon').attr("href", iconLink);
  $('.pageTitle').text(pokemonName + " | Pokéluna");
}

function lookupAllPokemon() {
  $('.pokemonList > .grid-container').empty();
  $('.pokemonList').removeClass('active');

  $('.pageButtons').show();
  $('.incompletePokemon').show();
  $('.missingThreeD').show();
  $('.missingShiny').show();
  $('.allPokemon').hide();
};

function lookupIncompletePokemon() {
  $('button').each(function () {
    $(this).removeClass('active');
  });

  $('.pokemonList').removeClass('active');
  $('.pokemonList > .grid-container').empty();

  $('.grid-container').load('/get-incomplete-pokemon-admin/', function () {
    $.each($('.grid-container .pokemonName'), function (index, item) {
      if ($(item).text().includes('_')) {
        $(item).text($(item).text().replace('_', ' '));
      }
    });

    $('.pokemonList').addClass('active');
  });

  $('.pageButtons').hide();
  $('.incompletePokemon').hide();
  $('.missingThreeD').show();
  $('.missingShiny').show();
  $('.allPokemon').show();
};

function lookupMissingThreeD() {
  $('button').each(function () {
    $(this).removeClass('active');
  });

  $('.pokemonList').removeClass('active');
  $('.pokemonList > .grid-container').empty();

  $('.grid-container').load('/get-missing-threed-pokemon-admin/', function () {
    $.each($('.grid-container .pokemonName'), function (index, item) {
      if ($(item).text().includes('_')) {
        $(item).text($(item).text().replace('_', ' '));
      }
    });

    $('.pokemonList').addClass('active');
  });

  $('.pageButtons').hide();
  $('.incompletePokemon').show();
  $('.missingThreeD').hide();
  $('.missingShiny').show();
  $('.allPokemon').show();
};

function lookupMissingShiny() {
  $('button').each(function () {
    $(this).removeClass('active');
  });

  $('.pokemonList').removeClass('active');
  $('.pokemonList > .grid-container').empty();

  $('.grid-container').load('/get-missing-shiny-pokemon-admin/', function () {
    $.each($('.grid-container .pokemonName'), function (index, item) {
      if ($(item).text().includes('_')) {
        $(item).text($(item).text().replace('_', ' '));
      }
    });

    $('.pokemonList').addClass('active');
  });

  $('.pageButtons').hide();
  $('.incompletePokemon').show();
  $('.missingThreeD').show();
  $('.missingShiny').hide();
  $('.allPokemon').show();
};

function lookupGeneration(generationId) {
  if (currentGeneration != generationId) {
    currentGeneration = generationId;
  }

  $(".overlay").fadeIn(300);

  $('button').each(function () {
    $(this).removeClass('active');
  });

  $('.pokemonList').removeClass('active');
  $('.pokemonList > .grid-container').empty();
  $('button#Generation' + generationId).addClass('active');

  $('.pokemonList').load('/get-pokemon-by-generation/' + generationId, function () {
    $.each($('.grid-container .pokemonName'), function (index, item) {
      if ($(item).text().includes('_')) {
        $(item).text($(item).text().replace('_', ' '));
      }
    });

    $('.pokemonList').addClass('active');

    setTimeout(function () {
      $(".overlay").fadeOut(300);
    });
  });
}

function lookupAvailableGame(gameId) {
  if (!$('.active').is($('#Game' + gameId))) {
    $(".overlay").fadeIn(300);

    $('button').each(function () {
      $(this).removeClass('active');
    });

    $('.pokemonCount').remove();
    $('.updateButton').remove();

    $('.pokemonList').removeClass('active');
    $('.pokemonList > .grid-container').empty();
    $('button#Game' + gameId).addClass('active');

    $('.grid-container').load('/get-available-pokemon-by-game/' + gameId, function () {
      $('.totalPokemon').prepend($('<h5>').addClass('pokemonCount').text('Total Available Pokémon: ' + $('.grid-container').children().length));
      $('.updater').append($('<a>').attr('href', 'admin/edit_game_availability/' + gameId).addClass('updateButton btn btn-primary').text('Update Game Availability'));
      $('.pokemonList').addClass('active');

      setTimeout(function () {
        $(".overlay").fadeOut(300);
      });
    });
  }
}

function lookupAdminGeneration(generationId) {
  if (!$('.active').is($('#Generation' + generationId))) {
    $('button').each(function () {
      $(this).removeClass('active');
    });

    $('.pokemonList').removeClass('active');
    $('.pokemonList > .grid-container').empty();
    $('button#Generation' + generationId).addClass('active');

    $('.grid-container').load('/get-pokemon-by-generation-admin/' + generationId, function () {
      $.each($('.grid-container .pokemonName'), function (index, item) {
        if ($(item).text().includes('_')) {
          $(item).text($(item).text().replace('_', ' '));
        }
      });

      $('.pokemonList').addClass('active');
    });
  }
}

function lookupMoveTypes(typeId) {
  if (!$('.active').is($('#Type' + typeId))) {
    $('button').each(function () {
      $(this).removeClass('active');
    });

    $('.page').each(function () {
      $(this).removeClass('active');
    });

    $('button#Type' + typeId).addClass('active');

    $('.type' + typeId).addClass('active');
  }
}

function lookupGames(gameId) {
  if (!$('.active').is($('#Game' + gameId))) {
    $('button').each(function () {
      $(this).removeClass('active');
    });

    $('.page').each(function () {
      $(this).removeClass('active');
    });

    $('button#Game' + gameId).addClass('active');

    $('.game' + gameId).addClass('active');
  }
}

function lookupYear(year) {
  $('.year button').each(function () {
    $(this).removeClass('active');
  });

  $('.year.pageStatList').removeClass('active');
  $('.year.pageStatList').empty();
  $('button#Year' + year).addClass('active');

  $('.year.pageStatList').load('/get-month-by-year/' + year + '/' + $('.pokemonPageCheck').attr('id'), function () {
    $('.year.pageStatList').addClass('active');
  });
}

function lookupMonth(month) {
  $('.month button').each(function () {
    $(this).removeClass('active');
  });

  $('.month.pageStatList').removeClass('active');
  $('.month.pageStatList').empty();
  $('button#Month' + month).addClass('active');

  $('.month.pageStatList').load('/get-day-by-month/' + month + '/' + $('.pageButtons.year .active').attr('id').replace('Year', '') + '/' + $('.pokemonPageCheck').attr('id'), function () {
    $('.month.pageStatList').addClass('active');
  });
}

function lookupYearStats(month) {
  $(".overlay").fadeIn(300);

  $('.month button').each(function () {
    $(this).removeClass('active');
  });

  var monthString = month;
  if (month < 10) {
    monthString = '0' + month;
  }

  $('.month.pageStatList').removeClass('active');
  $('.day.pageStatList').empty();
  $('button#Month' + monthString).addClass('active');

  $('.month.pageStatList').load('/get-stats-by-date/' + 0 + '/' + 0 + '/' + $('.pageButtons.year .active').attr('id').replace('Year', '') + '/' + $('.pokemonPageCheck').attr('id'), function () {
    $('.month.pageStatList').addClass('active');
    $(".overlay").fadeOut(300);
  });
}

function lookupDay(day) {
  $(".overlay").fadeIn(300);

  $('.day button').each(function () {
    $(this).removeClass('active');
  });

  var dayString = day;
  if (day < 10) {
    dayString = '0' + day;
  }

  $('.day.pageStatList').removeClass('active');
  $('.day.pageStatList').empty();
  $('button#Day' + dayString).addClass('active');

  $('.day.pageStatList').load('/get-stats-by-date/' + day + '/' + $('.pageButtons.month .active').attr('id').replace('Month', '') + '/' + $('.pageButtons.year .active').attr('id').replace('Year', '') + '/' + $('.pokemonPageCheck').attr('id'), function () {
    $('.day.pageStatList').addClass('active');
    $(".overlay").fadeOut(300);
  });
}

function lookupPopularity() {
  $(".overlay").fadeIn(300);

  $('.year button').each(function () {
    $(this).removeClass('active');
  });

  $('.year.pageStatList').removeClass('active');
  $('.year.pageStatList').empty();
  $('button#Popularity').addClass('active');

  $('.year.pageStatList').load('/get-stats-by-popularity/', function () {
    $('.year.pageStatList').addClass('active');
    $(".overlay").fadeOut(300);
  });
}

function lookupShinyDex(gameId) {
  $(".overlay").fadeIn(300);

  $('.shinyGame button').each(function () {
    $(this).removeClass('active');
  });

  $('.shinyGame.shinyDexList').removeClass('active');
  $('.shinyGame.shinyDexList').empty();
  $('button#Game' + gameId).addClass('active');

  $('.shinyGame.shinyDexList').load('/get-shiny-progress/' + gameId, function () {
    $('.shinyGame.shinyDexList').addClass('active');
    $(".overlay").fadeOut(300);
  });
}

function orderHunts(orderMethod) {
  sortMethod = orderMethod;
  if (orderMethod == "Pokedex") {
    $('.sortByPokedex').addClass('hide');
    $('.sortByDate').removeClass('hide');
  } else {
    $('.sortByPokedex').removeClass('hide');
    $('.sortByDate').addClass('hide');
  }

  if ($('.gamesList .active').length > 0) {
    lookupCompletedShinyHunts(orderGameId, orderIsShared, orderUsername);
  }
}

function lookupCompletedShinyHunts(gameId, isShared, username) {
  $(".overlay").fadeIn(300);
  orderGameId = gameId;
  orderIsShared = isShared;
  orderUsername = username;

  $('button').each(function () {
    $(this).removeClass('active');
  });

  $('.pokemonList').removeClass('active');
  $('.pokemonList > .grid-container').empty();
  $('button#Game' + gameId).addClass('active');

  $('.pokemonList').load('/completed-shiny-hunt-data/' + gameId + '/' + isShared + '/' + username + '/' + sortMethod, function () {
    $('.pokemonList').addClass('active');
    setTimeout(function () {
      $(".overlay").fadeOut(300);
    });
  });
}
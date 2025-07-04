var checkTypings = function () {
    $('.secondaryList option').each(function () {
        if (!$(this).is(':visible')) {
            $(this).css('display', 'block');
        }
    });

    $('.secondaryList option[value="' + $('.primaryList > select').val() + '"]').css('display', 'none');

    if ($('.primaryList > select').val() == '0' && $('.secondaryList > select').val() != '0') {
        if ($('.secondaryList > select').val() == '100') {
            $('.primaryList > select').val('0');
        }
        else {
            $('.primaryList > select').val($('.secondaryList > select').val());
        }

        $('.secondaryList > select').val('0');
    }
    else if ($('.primaryList > select').val() == $('.secondaryList > select').val()) {
        $('.secondaryList > select').val('0');
    }

    if ($('.primaryList > select').val() != '0' && $('.secondaryList > select').val() != '100') {
        $('.effectivenessChart').empty();

        $('.effectivenessChart').load('/get-typing-evaluator-chart/', { 'primaryTypeId': $('.primaryList > select').val(), 'secondaryTypeId': $('.secondaryList > select').val(), 'gameId': $('.gameList > select').val() }, function () {
            if ($('.typing-table-strong').children().length > 0) {
                $(".StrongAgainst").css("display", "block");
            }
            else {
                $(".StrongAgainst").css("display", "none");
            }

            if ($('.typing-table-weak').children().length > 0) {
                $(".WeakAgainst").css("display", "block");
            }
            else {
                $(".WeakAgainst").css("display", "none");
            }

            if ($('.typing-table-immune').children().length > 0) {
                $(".ImmuneTo").css("display", "block");
            }
            else {
                $(".ImmuneTo").css("display", "none");
            }

            $('.effectivenessChart').css('display', 'flex');
        });
    }
    else {
        $('.effectivenessChart').css('display', 'none');
    }
}, grabPokemon = function () {
    checkTypings();
    $(".overlay").fadeIn(300);
    if ($('.primaryList > select').val() != '') {
        $('.pokemonWithTyping').css('display', 'none');
        $('.effectivenessChart').css('display', 'none');
        $('.pokemonList').empty();
        $('.pokemonList').load('/get-pokemon-by-typing/', { 'primaryTypeId': $('.primaryList > select').val(), 'secondaryTypeId': $('.secondaryList > select').val(), 'gameId': $('.gameList > select').val(), 'regionalDexId': $('.regionalDexList > select').val(), 'specificTyping': false }, function () {
            if ($('.pokemonList').children().length > 0) {
                $('.pokemonWithTyping').css('display', 'block');
            }
            else {
                $('.pokemonWithTyping').css('display', 'none');
            }

            if ($('.secondaryList > select').val() == '') {
                $('.effectivenessChart').css('display', 'flex');
            }

            setTimeout(function () {
                $(".overlay").fadeOut(300);
            });
        });
    }
    else {
        $('.pokemonWithTyping').css('display', 'none');

        setTimeout(function () {
            $(".overlay").fadeOut(300);
        });
    }
}, grabPokemonSpecificTyping = function () {
    checkTypings();
    $(".overlay").fadeIn(300);
    if ($('.primaryList > select').val() != '') {
        $('.pokemonWithTyping').css('display', 'none');
        $('.effectivenessChart').css('display', 'none');
        $('.pokemonList').empty();
        $('.pokemonList').load('/get-pokemon-by-typing/', { 'primaryTypeId': $('.primaryList > select').val(), 'secondaryTypeId': $('.secondaryList > select').val(), 'gameId': $('.gameList > select').val(), 'regionalDexId': $('.regionalDexList > select').val(), 'specificTyping': true }, function () {
            if ($('.pokemonList').children().length > 0) {
                $('.pokemonWithTyping').css('display', 'block');
            }
            else {
                $('.pokemonWithTyping').css('display', 'none');
            }

            if ($('.secondaryList > select').val() == '') {
                $('.effectivenessChart').css('display', 'flex');
            }

            setTimeout(function () {
                $(".overlay").fadeOut(300);
            });
        });
    }
    else {
        $('.pokemonWithTyping').css('display', 'none');

        setTimeout(function () {
            $(".overlay").fadeOut(300);
        });
    }
}, grabTypes = function (gameId) {
    var primaryTypeId = $('.primaryList > select').val(), secondaryTypeId = $('.secondaryList > select').val();
    $('.typeLists').load('/get-types-by-game/', { 'gameId': gameId }, function () {
        if ($('.primaryList option[value=' + primaryTypeId + ']').length != 0) {
            $('.primaryList > select').val(primaryTypeId);
        }
        else {
            $('.primaryList > select').val(0);
        }

        if ($('.secondaryList option[value=' + secondaryTypeId + ']').length != 0) {
            $('.secondaryList > select').val(secondaryTypeId);
        }
        else {
            $('.secondaryList > select').val(0);
        }
    });
}, grabRegionalDexes = function (gameId) {
    if (gameId != 0) {
        $('.regionalDexList').load('/get-regional-dexes-by-game/', { 'gameId': gameId }, function () {
            $('.regionalDexList > select').val(0);
            $('.regionalDexList.hide').removeClass('hide')
        });
    } else {
        $('.regionalDexList:not(.hide)').addClass('hide')
    }
};

$(function () {
    $('.gameSelectList').select2();
    $('.primaryTypeSelectList').select2();
    $('.secondaryTypeSelectList').select2();
    grabTypes($('.gameList > select').val());
    grabRegionalDexes($('.gameList > select').val());
});

$(".gameSelectList").on('change', function () {
    grabTypes($('.gameList > select').val());
    grabRegionalDexes($('.gameList > select').val());
});
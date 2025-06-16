var pokemonList = [], grabGames = function (gameId, pokemonIds) {
    $(".overlay").fadeIn(300);

    $.ajax({
        url: '/get-games-by-shiny-huntable-pokemon/',
        method: "POST",
        data: { 'pokemonId': pokemonIds }
    })
        .done(function (data) {
            $('#GameId').empty();
            $.each(data, function (index, item) {
                $('#GameId').append($('<option>').val(item.id).text(item.name));
            });

            if (gameId !== "") {
                $('#GameId option').each(function () {
                    if (this.value == gameId) {
                        $('#GameId').val(gameId);
                    }
                });
            }

            $('.hide').not('.gameSpecific').not('.pokemonSpecific').not('.pokemonShinyImage').not('.isCaptured').each(function () {
                $(this).removeClass('hide');
            })

            grabHuntingMethod($('#HuntingMethodId').val())

            $(".overlay").fadeOut(300);
        })
        .fail(function () {
            if (isLocalhost) {
                alert("Failed to grab games!");
            }

            $(".overlay").fadeOut(300);
        });
}, refreshGenders = function () {
    var gender = $('#Gender').val();
    $.ajax({
        url: '/get-pokemon-genders/',
        method: "POST",
        data: { 'pokemonId': $('#PokemonId').val(), 'isShinyHunt': true }
    })
        .done(function (data) {
            $('#Gender').empty();
            $.each(data, function (index, item) {
                if (item == "None") {
                    item = "Gender Unknown"
                }
                $('#Gender').append($('<option>').text(item));
            });

            if (data.find(x => x == gender)) {
                $('#Gender').val(gender);
            }
        })
        .fail(function () {
            if (isLocalhost) {
                alert("Failed to grab genders!");
            }
        });
}, checkSweets = function () {
    if ($('#SweetId').length) {
        sweetId = $('#SweetId').val();
        $.ajax({
            url: '/get-pokemon-sweets/',
            method: "POST",
            data: { 'pokemonId': $('#PokemonId').val() }
        })
            .done(function (data) {
                $('#SweetId').empty();
                if ($('select').hasClass('preferredSweet')) {
                    $('#SweetId').append($('<option>').val("").text("No Preferred Sweet"));
                } else {
                    $('#SweetId').append($('<option>').val("").text("No Sweet"));
                }

                $.each(data, function (index, item) {
                    $('#SweetId').append($('<option>').val(item.id).text(item.name));
                });

                if (sweetId != "" && sweetId != null && data.find(x => x.id = sweetId)) {
                    $('#SweetId option').each(function () {
                        if (this.value == sweetId) {
                            $('#SweetId').val(sweetId);
                        }
                    });
                }

                if (data.length > 0) {
                    if ($('.sweets').hasClass('hide')) {
                        $('.sweets').removeClass('hide');
                    }
                } else {
                    if (!$('.sweets').hasClass('hide')) {
                        $('.sweets').addClass('hide');
                    }
                }
            })
            .fail(function () {
                if (isLocalhost) {
                    alert("Failed to grab sweets!");
                }
            });
    }
}, grabHuntingMethod = function (huntingMethodId) {
    $.ajax({
        url: '/get-hunting-methods/',
        method: "POST",
        data: { 'gameId': $('#GameId').val() }
    })
        .done(function (data) {
            $('#HuntingMethodId').empty();
            $.each(data, function (index, item) {
                $('#HuntingMethodId').append($('<option>').val(item.id).text(item.name));
            });

            if (huntingMethodId != "" && huntingMethodId != null && data.find(x => x.id = huntingMethodId)) {
                $('#HuntingMethodId option').each(function () {
                    if (this.value == huntingMethodId) {
                        $('#HuntingMethodId').val(huntingMethodId);
                    }
                });
            } else {
                $('#HuntingMethodId').val(1);
            }

            checkFunctions();
        })
        .fail(function () {
            if (isLocalhost) {
                alert("Failed to grab hunting methods!");
            }
        });
}, checkShinyCharm = function () {
    $.ajax({
        url: '/check-shiny-charm/',
        method: "POST",
        data: { 'gameId': $('#GameId').val(), 'huntingMethodId': $('#HuntingMethodId').val() }
    })
        .done(function (data) {
            if (data) {
                if ($('.shinyCharmCheckbox').hasClass('hide')) {
                    $('.shinyCharmCheckbox').removeClass('hide');
                }
            } else {
                if (!$('.shinyCharmCheckbox').hasClass('hide')) {
                    $('.shinyCharmCheckbox').addClass('hide');
                    $('#HasShinyCharm').prop('checked', false)
                }
            }
        })
        .fail(function () {
            if (isLocalhost) {
                alert("Failed to check shiny charm!");
            }
        });
}, checkAlpha = function () {
    if ($('#GameId').val() == 37) {
        if ($('.alphaCheckbox').hasClass('hide')) {
            $('.alphaCheckbox').removeClass('hide');
        }
    } else {
        if (!$('.alphaCheckbox').hasClass('hide')) {
            $('.alphaCheckbox').addClass('hide');
            $('#IsAlpha').prop('checked', false)
        }
    }
}, checkSparklingPower = function () {
    $.ajax({
        url: '/check-sparkling-power/',
        method: "POST",
        data: { 'gameId': $('#GameId').val(), 'huntingMethodId': $('#HuntingMethodId').val() }
    })
        .done(function (data) {
            if (data) {
                if ($('.sparklingPower').hasClass('hide')) {
                    $('.sparklingPower').removeClass('hide');
                }
            } else {
                if (!$('.sparklingPower').hasClass('hide')) {
                    $('.sparklingPower').addClass('hide');
                    $('#SparklingPowerLevel').val(0)
                }
            }
        })
        .fail(function () {
            if (isLocalhost) {
                alert("Failed to check sparkling power!");
            }
        });
}, checkLure = function () {
    var gameId = $('#GameId').val(), huntingMethodId = $('#HuntingMethodId').val()
    if ((gameId == 16 || gameId == 28) && (huntingMethodId == 1 || huntingMethodId == 16)) {
        if ($('.lureCheckbox').hasClass('hide')) {
            $('.lureCheckbox').removeClass('hide');
        }
    } else {
        if (!$('.lureCheckbox').hasClass('hide')) {
            $('.lureCheckbox').addClass('hide');
            $('#UsingLures').prop('checked', false)
        }
    }
}, checkMarks = function () {
    if ($('#MarkId').length) {
        var gameId = $('#GameId').val(), markId = $('#MarkId').val();
        $.ajax({
            url: '/get-shiny-hunt-marks/',
            method: "POST",
            data: { 'gameId': gameId, 'huntingMethodId': $('#HuntingMethodId').val() }
        })
            .done(function (data) {
                $('#MarkId').empty();
                if ($('select').hasClass('preferredMark')) {
                    $('#MarkId').append($('<option>').val("").text("No Preferred Mark"));
                } else {
                    $('#MarkId').append($('<option>').val("").text("No Mark"));
                }

                $.each(data, function (index, item) {
                    $('#MarkId').append($('<option>').val(item.id).text(item.name));
                });

                if (markId != "" && markId != null && data.find(x => x.id = markId)) {
                    $('#MarkId option').each(function () {
                        if (this.value == markId) {
                            $('#MarkId').val(markId);
                        }
                    });
                }

                if (data != null) {
                    if ($('.marks').hasClass('hide')) {
                        $('.marks').removeClass('hide');
                    }
                } else {
                    if (!$('.marks').hasClass('hide')) {
                        $('.marks').addClass('hide');
                    }
                }
            })
            .fail(function () {
                if (isLocalhost) {
                    alert("Failed to grab marks!");
                }
            });
    }
}, checkPokeballs = function () {
    if ($('#PokeballId').length) {
        var gameId = $('#GameId').val(), pokeballId = $('#PokeballId').val();
        $.ajax({
            url: '/get-shiny-hunt-pokeballs/',
            method: "POST",
            data: { 'gameId': gameId, 'huntingMethodId': $('#HuntingMethodId').val() }
        })
            .done(function (data) {
                $('#PokeballId').empty();
                $('#PokeballId').append($('<option>').val("").text("No Specific Pokeball"));

                $.each(data, function (index, item) {
                    $('#PokeballId').append($('<option>').val(item.id).text(item.name));
                });

                if (pokeballId != "" && pokeballId != null && data.find(x => x.id = pokeballId)) {
                    $('#PokeballId option').each(function () {
                        if (this.value == pokeballId) {
                            $('#PokeballId').val(pokeballId);
                        }
                    });
                }
            })
            .fail(function () {
                if (isLocalhost) {
                    alert("Failed to grab pokeballs!");
                }
            });
    }
}, checkCapturedStatus = function () {
        if ($('#IsCaptured').is(':checked')) {
            $.each($('.captureRequired:not(.gameSpecific):not(.pokemonSpecific)'), function (index, item) {
                $(this).removeClass('hide');
            });
        } else {
            $.each($('.captureRequired'), function (index, item) {
                $(this).addClass('hide');
            });
        }
}, checkFunctions = function () {
    checkLure();
    checkAlpha();
    checkMarks();
    checkPokeballs();
    checkShinyCharm();
    checkSparklingPower();
    checkCapturedStatus();
}, refreshEvents = function () {
    $('.pokemonShinyImage').unbind("click");
    $('.pokemonShinyImage').on("click", function () {
        if ($('.container>div').hasClass('startShinyHunt')) {
            var pokemonId = $(this).attr('id')
            if ($('#PokemonIds option[value="' + pokemonId + '"]') !== "undefined" && $('.select2-selection__choice[title="' + pokemonName + '"] .select2-selection__choice__remove') !== "undefined") {
                var pokemonName = $('#PokemonIds option[value="' + pokemonId + '"]').html();
                $('.select2-selection__choice[title="' + pokemonName + '"] .select2-selection__choice__remove').trigger('click');
            }
        }
        $(this).remove()
    });
}

$(document).ready(function () {
    if ($('.container>div').hasClass('startShinyHunt')) {
        $('#PokemonIds').select2({
            multiple: true,
        }).on('select2:unselecting', function() {
            $(this).data('unselecting', true);
        }).on('select2:opening', function(e) {
            if ($(this).data('unselecting')) {
                $(this).removeData('unselecting');
                e.preventDefault();
            }
        });
        $('.select2-selection__choice__remove').each(function () {
            $(this).trigger('click');
        })
    } else {
        if ($('#PokemonId').val() !== "") {
            grabGames($('#GameId').val(), $("#PokemonId").val());
            grabHuntingMethod($('#HuntingMethodId').val(), $("#PokemonId").val());
            checkSweets();
            $('.pokemonShinyImage').removeClass('hide').prop('src', $('.webUrl').prop('name') + $('.shinyUrl').prop('name') + $('#PokemonId').val() + '.png');
        }
        $("#PokemonId").select2();
    }

    $("#HuntingMethodId").select2();
    $("#PokeballId").select2();
    $("#SweetId").select2();
    $("#MarkId").select2();

    if (!$('.container>div').hasClass('completeShinyHunt')) {
        $("#GameId").select2();
    }
});

$('#GameId').on('change', function () {
    grabHuntingMethod($('#HuntingMethodId').val());
});

$('#PokemonId').on('change', function () {
    $('.pokemonShinyImage').prop('src', $('.webUrl').prop('name') + $('.shinyUrl').prop('name') + $('#PokemonId').val() + '.png');
    $('.pokemonShinyImage.hide').removeClass('hide');
    $('#PokemonId option[value=""]').remove();
    grabGames($('#GameId').val(), $('#PokemonId').val());
    refreshGenders();
    checkSweets();
});

$('#PokemonIds').on('change', function () {
    if ($('#PokemonIds').val().length > 0) {
        if (pokemonList.length < $('#PokemonIds').val().length) {
            var pokemon = $('#PokemonIds').val().filter(x => !pokemonList.includes(x))[0];
            $('.shinyHuntImages').append($('<img>').addClass('shadowed pokemonShinyImage').attr('id', pokemon).prop('alt', 'Shiny Pokemon Image').prop('loading', 'lazy').prop('src', $('.webUrl').prop('name') + $('.shinyUrl').prop('name') + pokemon + '.png'));
            refreshEvents();
        } else if (pokemonList.length > $('#PokemonIds').val().length) {
            var pokemonId = pokemonList.filter(x => !$('#PokemonIds').val().includes(x))[0];
            if ($('.pokemonShinyImage#' + pokemonId) !== "undefined") {
                $('.pokemonShinyImage#' + pokemonId).remove();
            }
        }
        grabGames($('#GameId').val(), $('#PokemonIds').val());
    } else {
        $('.pokemonShinyImage').addClass('hide');
        $('.form-group.row').addClass('hide');
        $('.form-group.row').first().removeClass('hide');
    }
    pokemonList = $('#PokemonIds').val();
});

$('#HuntingMethodId').on('change', function () {
    checkFunctions();
});

$('.addShinyHunt').on('click', function () {
    $(".overlay").fadeIn(300);
});

$('#IsCaptured').on('change', function() {
    checkFunctions();
})
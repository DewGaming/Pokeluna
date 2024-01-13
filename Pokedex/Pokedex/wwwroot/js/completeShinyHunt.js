var checkShinyCharm = function () {
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
    var gameId = $('#GameId').val()
    if (gameId == 37) {
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
                $('#MarkId').append($('<option>'));
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
}, checkHOMETransfer = function () {
    if ($('#GameId').val() == 43) {
        $.ajax({
            url: '/go-transfer-without-symbol/',
            method: "POST",
            data: { 'pokemonId': $('#PokemonId').val() }
        })
            .done(function (data) {
                if (data) {
                    if ($('.directHOMETransferCheckbox').hasClass('hide')) {
                        $('.directHOMETransferCheckbox').removeClass('hide');
                    }
                } else {
                    if (!$('.directHOMETransferCheckbox').hasClass('hide')) {
                        $('.directHOMETransferCheckbox').addClass('hide');
                        $('#DirectHOMETransfer').prop('checked', false)
                    }
                }
            })
            .fail(function () {
                if (isLocalhost) {
                    alert("Failed to check HOME Transfer!");
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
                $.each(data, function (index, item) {
                    $('#PokeballId').append($('<option>').val(item.id).text(item.name));
                });

                if (pokeballId != "" && pokeballId != null && data.find(x => x.id = pokeballId)) {
                    $('#PokeballId option').each(function () {
                        if (this.value == pokeballId) {
                            $('#PokeballId').val(pokeballId);
                        }
                    });
                } else if (gameId == 37) {
                    $('#PokeballId').val(11);
                } else {
                    $('#PokeballId').val(1);
                }
            })
            .fail(function () {
                if (isLocalhost) {
                    alert("Failed to grab pokeballs!");
                }
            });
    }
}, checkSweets = function () {
    $.ajax({
        url: '/get-pokemon-sweets/',
        method: "POST",
        data: { 'pokemonId': $('#PokemonId').val() }
    })
        .done(function (data) {
            $('#SweetId').empty();
            $.each(data, function (index, item) {
                $('#SweetId').append($('<option>').val(item.id).text(item.name));
            });

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
}, checkFunctions = function () {
    checkLure();
    checkAlpha();
    checkMarks();
    checkSweets();
    checkPokeballs();
    checkShinyCharm();
    checkHOMETransfer();
    checkSparklingPower();
}

$(document).ready(function () {
    checkFunctions();
    var image = new Image();
    image.src = $('.webUrl').prop('name') + $('.shinyUrl').prop('name') + $('#PokemonId').val() + '.png';
    image.onload = function () {
        if (this.width > 0) {
            $('.pokemonShinyImage').prop('src', $('.webUrl').prop('name') + $('.shinyUrl').prop('name') + $('#PokemonId').val() + '.png');
        }
    }
    image.onerror = function () {
        $('.pokemonShinyImage').prop('src', $('.webUrl').prop('name') + $('.officialUrl').prop('name') + $('#PokemonId').val() + '.png');
    }
    $("#HuntingMethodId").select2();
    $("#PokeballId").select2();
    $("#SweetId").select2();
    $("#MarkId").select2();
});
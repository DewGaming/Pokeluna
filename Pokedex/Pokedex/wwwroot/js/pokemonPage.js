var officialRender = function() {
    $('.current').removeClass('current');
    if ($('.genderSign:not(.hidden)').hasClass('default') || !$('.genderSign').length) {
        $('.official').addClass('current');
    } else {
        $('.genderDifferenceOfficial').addClass('current');
    }
    $('.pokemonImageButtons>.hidden').removeClass('hidden');
    $('.pokemonImageButtons .officialButton').addClass('hidden');
}, homeRender = function() {
    $('.current').removeClass('current');
    if ($('.genderSign:not(.hidden)').hasClass('default') || !$('.genderSign').length) {
        $('.home').addClass('current');
    } else {
        $('.genderDifferenceHome').addClass('current');
    }
    $('.pokemonImageButtons>.hidden').removeClass('hidden');
    $('.pokemonImageButtons .homeButton').addClass('hidden');
}, shinyRender = function() {
    $('.current').removeClass('current');
    if ($('.genderSign:not(.hidden)').hasClass('default') || !$('.genderSign').length) {
        $('.shiny').addClass('current');
    } else {
        $('.genderDifferenceShiny').addClass('current');
    }
    $('.pokemonImageButtons>.hidden').removeClass('hidden');
    $('.pokemonImageButtons .shinyButton').addClass('hidden');
}, differenceRender = function() {
    if ($('.official').hasClass('current') || $('.genderDifferenceOfficial').hasClass('current')) {
        if ($('.official').hasClass('current')) {
            $('.official').removeClass('current');
            $('.genderDifferenceOfficial').addClass('current');
        } else {
            $('.official').addClass('current');
            $('.genderDifferenceOfficial').removeClass('current');
        }
    } else if ($('.shiny').hasClass('current') || $('.genderDifferenceShiny').hasClass('current')) {
        if ($('.shiny').hasClass('current')) {
            $('.shiny').removeClass('current');
            $('.genderDifferenceShiny').addClass('current');
        } else {
            $('.shiny').addClass('current');
            $('.genderDifferenceShiny').removeClass('current');
        }
    } else if ($('.home').hasClass('current') || $('.genderDifferenceHome').hasClass('current')) {
        if ($('.home').hasClass('current')) {
            $('.home').removeClass('current');
            $('.genderDifferenceHome').addClass('current');
        } else {
            $('.home').addClass('current');
            $('.genderDifferenceHome').removeClass('current');
        }
    }

    if ($('.genderSign.default').hasClass('hidden')) {
        $('.genderSign.default').removeClass('hidden');
        $('.genderSign.difference').addClass('hidden');
    } else {
        $('.genderSign.difference').removeClass('hidden');
        $('.genderSign.default').addClass('hidden');
    }
}, grabTypingChart = function (primaryType, generationId) {
    $('.page.active .effectivenessChart').load('/get-typing-evaluator-chart/', { 'primaryTypeId': primaryType, 'secondaryTypeId': '0', 'generationId': generationId, 'teraType': $('.teraTypeSelectList').find(":selected").html() }, function () {
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
    });
}, grabTypingChartByPokemon = function (pokemonId, generationId) {
    $('.page.active .effectivenessChart').load('/get-typing-evaluator-chart-by-pokemon/', { 'pokemonId': pokemonId, 'generationId': generationId }, function () {
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
    });
}, grabStellarChart = function (pokemonId, generationId) {
    $('.page.active .effectivenessChart').load('/get-stellar-typing-chart/', { 'pokemonId': pokemonId, 'generationId': generationId, 'teraType': 'stellar' }, function () {
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
    });
}, grabOgerponTeraAbility = function (pokemonId) {
    $.ajax({
        url: '/grab-ogerpon-tera-ability/',
        method: "POST",
        data: { 'pokemonId': pokemonId }
    })
        .done(function (data) {
            $('.page.active .abilities .cursorHelp').html(data.name);
            $('.page.active .abilities .cursorHelp').attr('title', 'Ability Name: ' + data.name + ' Description: ' + data.description);
        });
}, grabOgerponRegularAbility = function (pokemonId) {
    $.ajax({
        url: '/grab-ogerpon-regular-ability/',
        method: "POST",
        data: { 'pokemonId': pokemonId }
    })
        .done(function (data) {
            $('.page.active .abilities .cursorHelp').html(data.name);
            $('.page.active .abilities .cursorHelp').attr('title', 'Ability Name: ' + data.name + ' Description: ' + data.description);
        });
}

$(document).ready(function () {
    if ($('#TerapagosStellar').length) {
        $('#TerapagosStellar.page .teraTypeSelectList').val(99);
    }

    if ($('#TerapagosTerastal').length) {
        $('#TerapagosTerastal.page .teraTypeSelectList').val(0);
    }

    $('.teraTypeSelectList').select2();
})

$('span[title]').on('click', function() {
    alert($(this).attr('title'))
})

$('.teraTypeSelectList').on('change', function () {
    var generationId = $('#' + $('.page.active').attr('id') + '.page.active').attr('class').split(' ')[1].split('generation')[1];
    var teraType = $(this).val();
    var currentImage = $('.page.active .pokemonImage .current').attr("class").split(' ')[0];
    var pokemonId = $('.page.active .pokemonImage .current img').attr("id");
    if (teraType == 99) {
        if (pokemonId == 1770) {
            $('#TerapagosTerastal.page.active .teraTypeSelectList').val(0);
            $('#TerapagosTerastal.page').removeClass('active');
            $('#TerapagosTerastal.generations').removeClass('active');
            $('#TerapagosStellar.page').addClass('active');
            $('#TerapagosStellar.generations').addClass('active');
        } else {
            grabStellarChart(pokemonId, generationId);
        } 
    } else if (teraType != 0) {
        grabTypingChart(teraType, generationId);
    } else {
        if (pokemonId == 1882) {
            $('#TerapagosStellar.page.active .teraTypeSelectList').val(99);
            $('#TerapagosStellar.page').removeClass('active');
            $('#TerapagosStellar.generations').removeClass('active');
            $('#TerapagosTerastal.page').addClass('active');
            $('#TerapagosTerastal.generations').addClass('active');
        } else {
            grabTypingChartByPokemon(pokemonId, generationId);
        }
    }

    if (pokemonId == '741' || pokemonId == '1301') {
        if (teraType == 99) {
            teraType = 0;
        }
        $('.pokemonPicture .pokemonImage').load('/get-pokemon-images-with-type/', { 'pokemonId': pokemonId, 'typeId': teraType, 'currentImage': currentImage });
    } else if ($('#Pokemon_Name').val().indexOf('Ogerpon') >= 0) {
        if (teraType != 0) {
            grabOgerponTeraAbility(pokemonId);
        } else {
            grabOgerponRegularAbility(pokemonId);
        }
    }
});
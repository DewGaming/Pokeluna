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
}

$(document).ready(function () {
    if ($('#Ogerpon').length) {
        $('#Ogerpon.page .teraTypeSelectList').val(0);
    }

    if ($('#OgerponWellspringMask').length) {
        $('#OgerponWellspringMask.page .teraTypeSelectList').val(0);
    }

    if ($('#OgerponHearthflameMask').length) {
        $('#OgerponHearthflameMask.page .teraTypeSelectList').val(0);
    }

    if ($('#OgerponCornerstoneMask').length) {
        $('#OgerponCornerstoneMask.page .teraTypeSelectList').val(0);
    }

    if ($('#TerapagosTerastal').length) {
        $('#TerapagosTerastal.page .teraTypeSelectList').val(0);
    }
    
    if ($('#OgerponTealMaskTerastallized').length) {
        $('#OgerponTealMaskTerastallized.page .teraTypeSelectList').val(1);
    }

    if ($('#OgerponWellspringMaskTerastallized').length) {
        $('#OgerponWellspringMaskTerastallized.page .teraTypeSelectList').val(3);
    }

    if ($('#OgerponHearthflameMaskTerastallized').length) {
        $('#OgerponHearthflameMaskTerastallized.page .teraTypeSelectList').val(4);
    }

    if ($('#OgerponCornerstoneMaskTerastallized').length) {
        $('#OgerponCornerstoneMaskTerastallized.page .teraTypeSelectList').val(18);
    }

    if ($('#TerapagosStellar').length) {
        $('#TerapagosStellar.page .teraTypeSelectList').val(99);
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
        if (pokemonId == 1768) {
            $('#Ogerpon.page.active .teraTypeSelectList').val(0);
            $('#Ogerpon.page').removeClass('active');
            $('#Ogerpon.generations').removeClass('active');
            $('#OgerponTealMaskTerastallized.page').addClass('active');
            $('#OgerponTealMaskTerastallized.generations').addClass('active');
        } else if (pokemonId == 1879) {
            $('#OgerponWellspringMask.page.active .teraTypeSelectList').val(0);
            $('#OgerponWellspringMask.page').removeClass('active');
            $('#OgerponWellspringMask.generations').removeClass('active');
            $('#OgerponWellspringMaskTerastallized.page').addClass('active');
            $('#OgerponWellspringMaskTerastallized.generations').addClass('active');
        } else if (pokemonId == 1880) {
            $('#OgerponHearthflameMask.page.active .teraTypeSelectList').val(0);
            $('#OgerponHearthflameMask.page').removeClass('active');
            $('#OgerponHearthflameMask.generations').removeClass('active');
            $('#OgerponHearthflameMaskTerastallized.page').addClass('active');
            $('#OgerponHearthflameMaskTerastallized.generations').addClass('active');
        } else if (pokemonId == 1881) {
            $('#OgerponCornerstoneMask.page.active .teraTypeSelectList').val(0);
            $('#OgerponCornerstoneMask.page').removeClass('active');
            $('#OgerponCornerstoneMask.generations').removeClass('active');
            $('#OgerponCornerstoneMaskTerastallized.page').addClass('active');
            $('#OgerponCornerstoneMaskTerastallized.generations').addClass('active');
        } else {
            grabTypingChart(teraType, generationId);
        }
    } else {
        if (pokemonId == 1882) {
            $('#TerapagosStellar.page.active .teraTypeSelectList').val(99);
            $('#TerapagosStellar.page').removeClass('active');
            $('#TerapagosStellar.generations').removeClass('active');
            $('#TerapagosTerastal.page').addClass('active');
            $('#TerapagosTerastal.generations').addClass('active');
        } else if (pokemonId == 1890) {
            $('#OgerponTealMaskTerastallized.page.active .teraTypeSelectList').val(1);
            $('#OgerponTealMaskTerastallized.page').removeClass('active');
            $('#OgerponTealMaskTerastallized.generations').removeClass('active');
            $('#Ogerpon.page').addClass('active');
            $('#Ogerpon.generations').addClass('active');
        } else if (pokemonId == 1891) {
            $('#OgerponWellspringMaskTerastallized.page.active .teraTypeSelectList').val(3);
            $('#OgerponWellspringMaskTerastallized.page').removeClass('active');
            $('#OgerponWellspringMaskTerastallized.generations').removeClass('active');
            $('#OgerponWellspringMask.page').addClass('active');
            $('#OgerponWellspringMask.generations').addClass('active');
        } else if (pokemonId == 1892) {
            $('#OgerponHearthflameMaskTerastallized.page.active .teraTypeSelectList').val(4);
            $('#OgerponHearthflameMaskTerastallized.page').removeClass('active');
            $('#OgerponHearthflameMaskTerastallized.generations').removeClass('active');
            $('#OgerponHearthflameMask.page').addClass('active');
            $('#OgerponHearthflameMask.generations').addClass('active');
        } else if (pokemonId == 1893) {
            $('#OgerponCornerstoneMaskTerastallized.page.active .teraTypeSelectList').val(18);
            $('#OgerponCornerstoneMaskTerastallized.page').removeClass('active');
            $('#OgerponCornerstoneMaskTerastallized.generations').removeClass('active');
            $('#OgerponCornerstoneMask.page').addClass('active');
            $('#OgerponCornerstoneMask.generations').addClass('active');
        } else {
            grabTypingChartByPokemon(pokemonId, generationId);
        }
    }

    if (pokemonId == '741' || pokemonId == '1301') {
        if (teraType == 99) {
            teraType = 0;
        }
        $('.pokemonPicture .pokemonImage').load('/get-pokemon-images-with-type/', { 'pokemonId': pokemonId, 'typeId': teraType, 'currentImage': currentImage });
    }
});
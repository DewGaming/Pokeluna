var abilityId = 0, gender = "Empty", grabAbilities = function () {
    $.ajax({
        url: '/get-pokemon-abilities/',
        method: "POST",
        data: { 'pokemonId': $('#PokemonTeamDetail_PokemonId').val(), 'gameId': $('#GameId').val() }
    })
        .done(function (data) {
            $('#PokemonTeamDetail_AbilityId').empty();
            $.each(data, function (index, item) {
                $('#PokemonTeamDetail_AbilityId').append($('<option>').val(item.id).text(item.name));
                if (item.id == abilityId)
                {
                    $('#PokemonTeamDetail_AbilityId option:contains("' + item.name + '")').attr("selected", "selected");
                }
            });
        })
        .fail(function () {
            if (isLocalhost) {
                alert("Failed to grab abilities!");
            }
        });
}, refreshGenders = function () {
    $.ajax({
        url: '/get-pokemon-genders/',
        method: "POST",
        data: { 'pokemonId': $('#PokemonTeamDetail_PokemonId').val(), 'isShinyHunt': false }
    })
        .done(function (data) {
            $('#PokemonTeamDetail_Gender').empty();
            $.each(data, function (index, item) {
                $('#PokemonTeamDetail_Gender').append($('<option>').text(item));
                if (item == gender)
                {
                    $('#PokemonTeamDetail_Gender option:contains("' + item + '")').attr("selected", "selected");
                }
            });
            if ($.inArray("None", data) !== -1 && data.length == 1) {
                $('#PokemonTeamDetail_Gender').parent().hide();
            }
            else {
                $('#PokemonTeamDetail_Gender').parent().show();
            }
        })
        .fail(function () {
            if (isLocalhost) {
                alert("Failed to grab genders!");
            }
        });
}

$(document).ready(function () {
    abilityId = $('#PokemonTeamDetail_AbilityId').val();
    gender = $('#PokemonTeamDetail_Gender').val();
    refreshGenders();
    grabAbilities();
    $("#PokemonTeamDetail_PokemonId").select2();
});

$('#PokemonTeamDetail_PokemonId').on('change', function () {
    abilityId = $('#PokemonTeamDetail_AbilityId').val();
    gender = $('#PokemonTeamDetail_Gender').val();
    refreshGenders();
    grabAbilities();
});

$('#PokemonTeamDetail_Gender').on('change', function () {
    if ($('#PokemonTeamDetail_PokemonId').val() == "678") {
        abilityId = 0;
        gender = "";
        grabAbilities();
    }
});
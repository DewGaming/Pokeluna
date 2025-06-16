var grabAbilities = function () {
    $.ajax({
        url: '/get-pokemon-abilities/',
        method: "POST",
        data: { 'pokemonId': $('#PokemonId').val() }
    })
        .done(function (data) {
            $('#AbilityId').empty();
            $.each(data, function (index, item) {
                $('#AbilityId').append($('<option>').val(item.id).text(item.name));
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
        data: { 'pokemonId': $('#PokemonId').val(), 'isShinyHunt': false }
    })
        .done(function (data) {
            $('#Gender').empty();
            $.each(data, function (index, item) {
                $('#Gender').append($('<option>').text(item));
            });
            if ($.inArray("None", data) !== -1 && data.length == 1) {
                $('#Gender').parent().hide();
            }
            else {
                $('#Gender').parent().show();
            }
        })
        .fail(function () {
            if (isLocalhost) {
                alert("Failed to grab genders!");
            }
        });
}

$(document).ready(function () {
    $('#Gender').parent().hide();
    $("#PokemonId").select2();
});

$('#PokemonId').on('change', function () {
    refreshGenders();
    grabAbilities();
});

$('#Gender').on('change', function () {
    if ($('#PokemonId').val() == "678") {
        grabAbilities();
    }
});
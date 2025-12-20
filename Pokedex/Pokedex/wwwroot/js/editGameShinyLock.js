function updateGameShinyLocks(gameId) {
    var pokemon = [];
    $('.gameShinyLock input').each(function () {
        if ($(this).is(':checked')) {
            pokemon.push(this.value);
        }
    });

    $(".overlay").fadeIn(300);

    $.ajax({
        url: '/update-game-shiny-lock/',
        method: "POST",
        data: { "gameId": gameId, "pokemonList": pokemon }
    })
        .done(function (data) {
            window.location = data;
        })
        .fail(function () {
            alert("Update Failed!");

            $(".overlay").fadeOut(300);
        });
}
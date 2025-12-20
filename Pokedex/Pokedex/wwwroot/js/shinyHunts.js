"use strict";

var isShared, adjustIncrements = function (shinyHuntId) {
    var currentIncrements = $('.Hunt' + shinyHuntId + ' .increments').html();
    var increments = prompt("Increment Amount", currentIncrements);
    if ($.isNumeric(increments)) {
        if (increments < 1) {
            increments = 1;
        }
        $('.Hunt' + shinyHuntId + ' .increments').html(increments);
        $.ajax({
            url: '/set-shiny-hunt-increments/',
            method: "POST",
            data: { "shinyHuntId": shinyHuntId, "increments": increments }
        });
    } else if (increments != null) {
        alert("Entered Phases Need to be a Number")
    }
}, adjustEncountersManually = function (shinyHuntId) {
    var currentEncounters = $('.Hunt' + shinyHuntId + ' .encounters').html();
    var encounters = prompt("Total Number of Encounters", currentEncounters);
    if ($.isNumeric(encounters)) {
        if (encounters < 0) {
            encounters = 0;
        }
        $('.Hunt' + shinyHuntId + ' .encounters').html(encounters);
        $.ajax({
            url: '/set-shiny-hunt-encounters/',
            method: "POST",
            data: { "shinyHuntId": shinyHuntId, "encounters": encounters }
        });
    } else if (encounters != null) {
        alert("Entered Encounters Need to be a Number")
    }
}, updatePinStatus = function (shinyHuntId, pinned) {
    if (pinned) {
        $('.Hunt' + shinyHuntId).addClass('HuntGamePin');
        $('.Hunt' + shinyHuntId).removeClass('hide');
        $('.Hunt' + shinyHuntId + ' .pin').addClass('pinned');
        $('.Hunt' + shinyHuntId + ' .pinned').removeClass('pin');
        if ($('.pinnedHunts').hasClass('hide')) {
            $('.pinnedHunts').removeClass('hide');
        }
    } else {
        $('.Hunt' + shinyHuntId).removeClass('HuntGamePin');
        $('.Hunt' + shinyHuntId + ' .pinned').addClass('pin');
        $('.Hunt' + shinyHuntId + ' .pin').removeClass('pinned');
        if ($('.pinnedHunts').hasClass('active')) {
            $('.Hunt' + shinyHuntId).addClass('hide');
        }
    }

    if ($('.HuntGamePin').length <= 0) {
        $('.pinnedHunts').addClass('hide');
        $('#Game0').trigger('click');
    }
}

$('.encounterIncrement.pointer').on('click', function () {
    adjustIncrements($(this).prop('id'));
});

$('.currentEncounters.pointer').on('click', function () {
    adjustEncountersManually($(this).prop('id'));
});

$('.pokemonNotes[title]').on('click', function () {
    alert($(this).attr('title'))
})

function lookupHuntsInGame(element, gameId) {
    if (!$('.active').is($('#Game' + gameId))) {
        $('button').each(function () {
            $(this).removeClass('active');
        });

        $('.pokemonList').removeClass('active');

        $('.shadowed.hide').each(function () {
            $(this).removeClass('hide');
        });

        if (!$(element).hasClass('incompleteAllGames') && !$(element).hasClass('completeAllGames')) {
            $('div.shadowed').not('.HuntGame' + gameId).each(function () {
                $(this).addClass('hide');
            });
        }

        if (!$(element).hasClass('incompleteAllGames') && !$(element).hasClass('pinnedHunts') && !$(element).hasClass('completeAllGames')) {
            $('.gameHuntedIn').each(function () {
                $(this).addClass('hide');
            });
        } else {
            $('.gameHuntedIn').each(function () {
                $(this).removeClass('hide');
            });
        }

        if (gameId != '0') {
            $('.shiniesFoundCount').html($('.completedHunts .HuntGame' + gameId).length)
        } else {
            $('.shiniesFoundCount').html($('.completedHunts .grid-container').children().length)
        }

        $('button#Game' + gameId).addClass('active');
        $('.pokemonList').addClass('active');
        $('.active.hide').removeClass('active');
    }
}

function checkIfShared(sharedBool) {
    isShared = sharedBool;
}

function incrementEncounter(shinyHuntId) {
    var currentEncounters = parseInt($('.Hunt' + shinyHuntId + ' .encounters').html());
    var incrementAmount = parseInt($('.Hunt' + shinyHuntId + ' .increments').html());
    $('.Hunt' + shinyHuntId + ' .encounters').html(currentEncounters + incrementAmount);
    $.ajax({
        url: '/set-shiny-hunt-encounters/',
        method: "POST",
        data: { "shinyHuntId": shinyHuntId, "encounters": currentEncounters + incrementAmount }
    });
}

function togglePin(shinyHuntId) {
    $.ajax({
        url: '/toggle-hunt-pin/',
        method: "POST",
        data: { "shinyHuntId": shinyHuntId }
    })
    .done(function (data) {
        updatePinStatus(shinyHuntId, data);
    });
}

function abandonHunt(shinyHuntId, pokemonName) {
    var typedName = prompt("Abandoning a Hunt is not reverisble. To confirm, please type the pokemon's name:\r\n\r\n" + pokemonName);
    typedName = typedName.toLowerCase();
    if (typedName == "flabebe") {
        typedName = typedName.replace("flabebe", "flabébé");
    }

    if (typedName == pokemonName.toLowerCase()) {
        $.ajax({
            url: '/abandon-shiny-hunt/',
            data: { "shinyHuntId": shinyHuntId }
        })
            .done(function () {
                $('.Hunt' + shinyHuntId).remove();
            });
    }
}

function giveSharableLink(username) {
    var url = window.location.href + '/' + username.toLowerCase();
    if (navigator.clipboard) {
        navigator.clipboard.writeText(url)
            .then(() => {
                if (window.confirm('Sharable link has been copied to your clipboard. If you would like to see this page for yourself, press OK. Otherwise, press Cancel.')) {
                    window.open(url, '_blank');
                };
            })

        console.log(url);
    }
}
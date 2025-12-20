var generationId = 0;

function updatePercentage() {
    var uncaptured, total;

    if (generationId == '0') {
        uncaptured = $('.completedHunts .grid-container').children().not('.hide').not('.uncaptured').length;
        total = $('.completedHunts .grid-container').children().not('.hide').length;
        $('.capturedCount').html(uncaptured);
        $('.totalCount').html(total);
    } else {
        uncaptured = $('.completedHunts.generation' + generationId + ' .grid-container').children().not('.hide').not('.uncaptured').length;
        total = $('.completedHunts.generation' + generationId + ' .grid-container').children().not('.hide').length;
        $('.capturedCount').html(uncaptured)
        $('.totalCount').html(total)
    }

    $('.capturedPercentage').html((100 * (uncaptured / total)).toFixed(2));
}

function hideCaptured() {
    $('.shadowed:not(.uncaptured)').each(function () {
        $(this).addClass('hide');
    });

    $.ajax({
        url: '/toggle-captured-shinies/',
        method: "POST",
        data: { 'capturedShiniesToggle': 'hide' }
    })
        .done(function () {
            $('.hideCapturedButton').each(function () {
                $(this).addClass('hide');
            });

            $('.showCapturedButton').each(function () {
                $(this).removeClass('hide');
            });
        })
        .fail(function () {
            alert("Failed to toggle captured shinies!");
        });

    updatePercentage();
}

function showCaptured() {
    $('.captured:not(.altForm):not(.femaleGenderDifference)').each(function () {
        $(this).removeClass('hide');
    });
    
    if ($('.hideGenderDifferenceButton').hasClass('hide')) {
        $('.maleGenderDifference .captureTotal').each(function () {
            $(this).addClass('hide');
        })

        $('.shadowed.uncaptured:not(.totalCaught0)').each(function () {
            $(this).removeClass('uncaptured');
            $(this).addClass('captured');
        })
        
        $('.shadowed:not(.uncaptured) .captureCompleteTotal').each(function () {
            $(this).removeClass('hide');
        })
    }

    if ($('.showAltFormsButton').hasClass('hide')) {
        if ($('.showGenderDifferenceButton').hasClass('hide')) {
            $('.captured').each(function () {
                $(this).removeClass('hide');
            });
        } else {
            $('.captured.altForm:not(.femaleGenderDifference)').each(function () {
                $(this).removeClass('hide');
            });
        }
    } else if ($('.showGenderDifferenceButton').hasClass('hide')) {
        $('.captured.femaleGenderDifference:not(.altForm)').each(function () {
            $(this).removeClass('hide');
        });
    }

    $.ajax({
        url: '/toggle-captured-shinies/',
        method: "POST",
        data: { 'capturedShiniesToggle': 'show' }
    })
        .done(function () {
            $('.showCapturedButton').each(function () {
                $(this).addClass('hide');
            });

            $('.hideCapturedButton').each(function () {
                $(this).removeClass('hide');
            });
        })
        .fail(function () {
            alert("Failed to toggle captured shinies!");
        });

    updatePercentage();
}

function hideAltForms() {
    $('.altForm').each(function () {
        $(this).addClass('hide');
    });

    $.ajax({
        url: '/toggle-shiny-alt-forms/',
        method: "POST",
        data: { 'altFormToggle': 'hide' }
    })
        .done(function () {
            $('.hideAltFormsButton').each(function () {
                $(this).addClass('hide');
            });

            $('.showAltFormsButton').each(function () {
                $(this).removeClass('hide');
            });
        })
        .fail(function () {
            alert("Failed to toggle shiny alt forms!");
        });

    updatePercentage();
}

function showAltForms() {
    $('.altForm:not(.captured):not(.femaleGenderDifference)').each(function () {
        $(this).removeClass('hide');
    });

    if ($('.showCapturedButton').hasClass('hide')) {
        if ($('.showGenderDifferenceButton').hasClass('hide')) {
            $('.altForm').each(function () {
                $(this).removeClass('hide');
            });
        } else {
            $('.captured.altForm:not(.femaleGenderDifference)').each(function () {
                $(this).removeClass('hide');
            });
        }
    } else if ($('.showGenderDifferenceButton').hasClass('hide')) {
        $('.altForm.femaleGenderDifference:not(.captured)').each(function () {
            $(this).removeClass('hide');
        });
    }

    $.ajax({
        url: '/toggle-shiny-alt-forms/',
        method: "POST",
        data: { 'altFormToggle': 'show' }
    })
        .done(function () {
            $('.showAltFormsButton').each(function () {
                $(this).addClass('hide');
            });

            $('.hideAltFormsButton').each(function () {
                $(this).removeClass('hide');
            });
        })
        .fail(function () {
            alert("Failed to toggle shiny alt forms!");
        });

    updatePercentage();
}

function hideGenderDifferences() {
    $('.femaleGenderDifference').each(function () {
        $(this).addClass('hide');
    });

    if ($('.showCapturedButton').hasClass('hide')) {
        $('.maleGenderDifference .captureTotal').each(function () {
            $(this).addClass('hide');
        })

        $('.shadowed.uncaptured:not(.totalCaught0)').each(function () {
            $(this).removeClass('uncaptured');
            $(this).addClass('captured');
        })
        
        $('.shadowed:not(.uncaptured) .captureCompleteTotal').each(function () {
            $(this).removeClass('hide');
        })
    }

    $('.bonusImages').each(function () {
        $(this).addClass('hide');
    })

    $.ajax({
        url: '/toggle-shiny-gender-differences/',
        method: "POST",
        data: { 'genderDifferenceToggle': 'hide' }
    })
        .done(function () {
            $('.hideGenderDifferenceButton').each(function () {
                $(this).addClass('hide');
            });

            $('.showGenderDifferenceButton').each(function () {
                $(this).removeClass('hide');
            });
        })
        .fail(function () {
            alert("Failed to toggle shiny gender differences!");
        });

    updatePercentage();
}

function showGenderDifferences() {
    $('.femaleGenderDifference:not(.captured):not(.altForm)').each(function () {
        $(this).removeClass('hide');
    });

    if ($('.showCapturedButton').hasClass('hide')) {
        if ($('.showAltFormsButton').hasClass('hide')) {
            $('.femaleGenderDifference').each(function () {
                $(this).removeClass('hide');
            });
        } else {
            $('.captured.femaleGenderDifference:not(.altForm)').each(function () {
                $(this).removeClass('hide');
            });
        }
    } else if ($('.showAltFormsButton').hasClass('hide')) {
        $('.altForm.femaleGenderDifference:not(.captured)').each(function () {
            $(this).removeClass('hide');
        });
    }

    $('.maleGenderDifference .captureTotal').each(function () {
        $(this).removeClass('hide');
    })

    $('.individualCaught0').each(function () {
        $(this).addClass('uncaptured');
        $(this).removeClass('captured');
    })

    $('.captureCompleteTotal').each(function () {
        $(this).addClass('hide');
    })

    $('.bonusImages').each(function () {
        $(this).removeClass('hide');
    })

    $.ajax({
        url: '/toggle-shiny-gender-differences/',
        method: "POST",
        data: { 'genderDifferenceToggle': 'show' }
    })
        .done(function () {
            $('.showGenderDifferenceButton').each(function () {
                $(this).addClass('hide');
            });

            $('.hideGenderDifferenceButton').each(function () {
                $(this).removeClass('hide');
            });
        })
        .fail(function () {
            alert("Failed to toggle shiny gender differences!");
        });

    updatePercentage();
}

function lookupShinyGeneration(generation) {
    generationId = generation;
    $('.shinyDexList .active').each(function () {
        $(this).removeClass('active');
    });

    $('button#Generation' + generationId).addClass('active');

    if (generationId != 0) {
        $('.page.generation' + generationId).each(function () {
            $(this).addClass('active');
        });

        $('.generationHeaders').each(function () {
            $(this).hide();
        });
    } else {
        $('.page').each(function () {
            $(this).addClass('active');
        });

        $('.generationHeaders').each(function () {
            $(this).show();
        });
    }

    updatePercentage();
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

$(function () {
    if ($('.showCapturedButton').hasClass('hide') && $('.hideGenderDifferenceButton').hasClass('hide')) {
        $('.maleGenderDifference .captureTotal').each(function () {
            $(this).addClass('hide');
        })

        $('.shadowed.uncaptured:not(.totalCaught0)').each(function () {
            $(this).removeClass('uncaptured');
            $(this).addClass('captured');
        })
        
        $('.shadowed:not(.uncaptured) .captureCompleteTotal').each(function () {
            $(this).removeClass('hide');
        })
    }
    updatePercentage();
});
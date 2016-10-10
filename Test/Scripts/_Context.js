var xhub;
var pcodepattern = /^(1[3-9]|2\d|3[0-2])[ABCDEGHJPRTX]\d\dR?( (I ?|\d)\d{3})?$/i;
var lotpattern = /^(F|B)?\d[0-3]\d\d[ABCD][1-8]00[0-6]$/i;

$.fn.founded = function (isfound) {
    if ($(this).is('input')) {
        $(this).css('background-color', isfound ? '#CBFFC0' : 'pink');
        $(this).tooltip(isfound ? 'disable' : 'enable');
        return $(this);
    }

    if (isfound) {
        $(this).removeClass('text-danger').addClass('text-info');
        $(this).tooltip('enable');
    }
    else {
        $(this).removeClass('text-info').addClass('text-danger');
        $(this).tooltip('disable');
    }
    return $(this);
}

$(function () {
    $('#contextform div.form-group').attr('class', 'form-group');

    var lot = $('input#Context_LotNum');
    var prd = $('input#Context_Product');
    var prdi = $('#producticon');
    var loti = $('#loticon');

    xhub = $.connection.contextHub;

    function ctxUpdate(ctx) {
        console.log(ctx);
        prd.val(ctx.Product);
        lot.val(ctx.LotNum);

        $('input#Context_ConnectionId').val(ctx.ConnectionId);
        $('input#Context_ProductCodeId').val(ctx.ProductCodeId);
        $('input#Context_SampleId').val(ctx.SampleId);

        if (prd.val().trim().length > 0) {
            prdi.founded(ctx.ProductCodeId != 0);
            prd.founded(ctx.ProductCodeId != 0);
        }

        if (lot.val().trim().length > 0) {
            loti.founded(ctx.SampleId != 0);
            lot.founded(ctx.SampleId != 0);
        }
        if (typeof ctxstats === "function")
            xhub.server.stats().done(ctxstats);                     // uses payload function if it exists
    };
    $.connection.hub.start().done(function () {
        if (typeof ctxstats != 'undefined' && $('input#Context_ProductCodeId').val() != 0) {
            $('.jumbotron').hide();
            swim('#lanes');
        }
        $(document).trigger('contextready');
    })
    .fail(function (error) {
        console.log('hub error ' + error);
    });
    $.connection.hub.disconnected(function () {
        setTimeout(function () {
            $.connection.hub.start();
        }, 1000); // Re-start connection after 1 second
    });
    $('input#Context_Product').on('change', function (e) {
        console.log('change fired');
        if (!pcodepattern.test($(this).val())) {
            $(this).founded(false);
            prdi.founded(false);
        }
        else {
            $(this).val($(this).val().toUpperCase());
            xhub.server.byProduct($(this).val()).done(ctxUpdate);
        }
    });
    $('input#Context_LotNum').on('change', function (e) {
        if (!lotpattern.test($(this).val())) {
            $(this).founded(false);
            loti.founded(false);
            console.log('pattern no match');
        }
        else {
            $(this).val($(this).val().toUpperCase());
            console.log('pattern matched');
            xhub.server.byLotNum($(this).val()).done(ctxUpdate);
        }
    });
    $('i#producticon').on('click', function (e) {
        console.log('ip');
        xhub.server.clear().done(ctxUpdate);
    });
    $('i#loticon').on('click', function (e) {
        console.log('il');
        xhub.server.clearSample().done(ctxUpdate);
    });
});

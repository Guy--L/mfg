var connectionId;
$(function () {
    chrome.serial.getDevices(function (devices) {

        for (var i = 0; i < devices.length; i++) {
            $('select#portList').append('<option value="' + devices[i].path + '">' + devices[i].path + '</option>');
        }
    });

    // ui hook
    $('button#open').click(function () {
        var clicks = $(this).data('clicks');

        if (!clicks) {
            var port = $('select#portList').val();
            chrome.serial.connect(port, { bitrate: 9600 }, function (info) {
                connectionId = info.connectionId;
                $("button#open").html("Close Port");
                console.log('Connection opened with id: ' + connectionId + ', Bitrate: ' + info.bitrate);
            });
        } else {
            chrome.serial.disconnect(connectionId, function (result) {
                $("button#open").html("Open Port");
                console.log('Connection with id: ' + connectionId + ' closed');
            });
        }

        $(this).data("clicks", !clicks);
    });
});
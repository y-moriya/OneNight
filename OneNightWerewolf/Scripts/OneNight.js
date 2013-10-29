function showUpdateTimer() {
    var lefttimeSpan = document.getElementById("lefttime");
    if (!lefttimeSpan.innerHTML.match(/([0-9]+)分 ([0-9]+)秒/)) {
        return;
    }
    min = parseInt(RegExp.$1);
    sec = parseInt(RegExp.$2);
    sec -= 1;
    if (sec == -1) {
        sec = 59;
        min -= 1;
        if (min == -1) {
            document.location.reload();
            return;
        }
    }
    lefttimeSpan.innerHTML = min + '分 ' + sec + '秒';
    setTimeout('showUpdateTimer();', 1000);
}
setTimeout('showUpdateTimer();', 1000);

$(function () {
    if ($("#entry").length != 0) {
        window.name = "";
    }
    if ($("#hPlayerId").length == 0) {
        $("#connection").html("接続なし");
        return;
    }

    var connection = $.hubConnection();
    var game = connection.createHubProxy("game");
    var gameId = $("#hGameId").val();
    var playerId = $("#hPlayerId").val();
    var playerName = $("#hPlayerName").val();
    var phase = $("#hPhase").val();
    var max = $("#hMaxMessageId").val();
    var MAX_INT = Math.pow(2, 53);

    //game.on("Recieve", function (type, name, msg, date) {
    //    $("#messages").find('tbody > tr:first').before('<tr class="' + type + '"><td class="name">' + name +
    //                                        ' > </td><td class="content">' + msg +
    //                                        '</td><td class="time">' + date + '</td></tr>');
    //});

    game.on("UpdateMax", function (currentMax) {
        max = currentMax;
    });

    game.on("WriteMsg", function (str, currentMax) {
        var json = JSON.parse(str);
        for (var i in json) {
            var content = '<div class="content"><div class="header"><img class="icon" src="';
            content += json[i].IconUri.replace('~', 'http://' + location.host);
            content += '"><strong class="name">';
            content += json[i].Name;
            content += '</strong>';
            if (json[i].UserName) {
                content += '<span class="username"> @';
                content += json[i].UserName;
                content += '</span>';
            }
            content += '<span class="time">';
            content += json[i].Date;
            content += '</span></div><p class="';
            content += json[i].Type;
            content += '">';
            content += json[i].Content;
            content += '</p></div>';

            $("#msg").find('div:first').before(content);
        }
        max = currentMax;
    });

    game.on("Reload", function (message) {
        if (message != "") {
            alert(message);
        }
        document.location.reload();
    });

    game.on("Toggle", function (commited) {
        if (commited) {
            $("#commit").val("時間を進めない");
        } else {
            $("#commit").val("時間を進める");
        }
    });

    game.on("Info", function (players) {
        $("#players").html(players);
    });

    game.on("Adjust", function (lefttime) {
        $("#lefttime").html(lefttime);
    });

    game.on("Error", function () {
        $("#connection").html("エラー");
    });

    $("#send").click(function () {
        var text = $("#Content").val();
        $("#Content").val("");
        game.invoke("SendMessage", gameId, playerId, text);
        game.invoke("GetMessages", gameId, playerId, max);
    });

    $("#start").click(function () {
        game.invoke("Start", gameId);
    });

    $("#commit").click(function () {
        game.invoke("Commit", gameId, playerId);
    });

    $("#exit").click(function () {
        game.invoke("Exit", gameId, playerId, playerName);
    });

    // リロードでも退出してしまう。
    //$(window).bind('beforeunload', function (event) {
    //    game.invoke("Exit", gameId, playerId, playerName);
    //});

    setInterval(function () {
        game.invoke("CheckUpdate", gameId, phase);
        game.invoke("GetMessages", gameId, playerId, max);
        max = MAX_INT;
    }, 10000);

    $("#SendMessageForm").keypress(function (ev) {
        if ((ev.which && ev.which === 13) || (ev.keyCode && ev.keyCode === 13)) {
            var text = $("#Content").val();
            $("#Content").val("");
            game.invoke("SendMessage", gameId, playerId, text);
            game.invoke("GetMessages", gameId, playerId, max);
            max = MAX_INT;
            return false;
        } else {
            return true;
        }
    });

    connection.start(function () {
        $("#send").prop("disabled", false);
        $("#start").prop("disabled", false);
        $("#commit").prop("disabled", false);
        $("#connection").html("正常");

        game.invoke("Join", gameId, playerName, (window.name == window.location.href));
        window.name = window.location.href;
    });

    connection.error(function (error) {
        $("#connection").html("エラー");
    });
})
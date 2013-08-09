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
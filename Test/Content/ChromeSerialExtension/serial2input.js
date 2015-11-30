chrome.runtime.onMessage.addListener(function (request, sender, sendResponse) {
	if(request.action == "countCourses") {
		var authors = $('.author a');
		authors = $.makeArray(authors);
		var hist = {};
		authors.map(function (a) {
		if (a.title in hist)
			hist[a.title]++;
		else
			hist[a.title] = 1;
		});
		
		for (var a in hist) {
			var aName = '"' + a + '"';
			var links = $('.author a[title=' + aName + ']');
			$.each(links, function (i, v) {
				v.innerText = a + ' - ' + hist[a];
			});
		}
	}
	
});

chrome.runtime.sendMessage({action: "show"});


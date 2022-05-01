var notifications = $("#notif-col");

function NotifySucc(text) {
	$('<div/>', {
		text: text,
		class: 'p-2 bg-success notification m-2',
		
	}).hide().appendTo(notifications).slideDown().delay(5000).fadeOut("slow", "linear");
}

function NotifyBad(text) {
	$('<div/>', {
		text: text,
		class: 'p-2 bg-danger notification m-2',

	}).hide().appendTo(notifications).slideDown().delay(5000).fadeOut("slow", "linear");
}


function UpdateOnline() {
	fetch(`/api/u/Online`, {
		method: "PUT",
		credentials: "same-origin",
	});
}
UpdateOnline();

window.setInterval(UpdateOnline, 60000);

function _ajax_request(url, data, callback, type, method) {
	if (jQuery.isFunction(data)) {
		callback = data;
		data = {};
	}
	return jQuery.ajax({
		type: method,
		url: url,
		data: data,
		success: callback,
		dataType: type
		});
}

jQuery.extend({
	put: function(url, data, callback, type) {
		return _ajax_request(url, data, callback, type, 'PUT');
	},
	delete_: function(url, data, callback, type) {
		return _ajax_request(url, data, callback, type, 'DELETE');
	}
});

function build(nr) {
	return '<option value="' + nr + '">' + nr + '</option>';
}

$(function() {
	var area = $("#text");
	var element = $('<select id="nr"></select>');
	$.get("notepad", function(result) {
		var lines = [""]
		if(result.indexOf("\r\n") != -1) {
			lines = result.split("\r\n");
		} else if(result.indexOf("\n") != -1) {
			lines = result.split("\n");
		} else {
			lines = result.split("\r");
		}
		var length = lines.length;
		for(var i = 0; i < length; i++) {
			var line = lines[i];
			$(build(line))
				.appendTo(element);
		}
		element
			.appendTo("form");
	});
	var text = $("#text");
	
	$("#load").click(function() {
		$.get(
			"/notepad/" + element.val(),
			function(data) {
				text.html(data);
			}
		);
	});
	$("#save").click(function() {
		$.put(
			"/notepad/" + element.val(),
			text.html()
		);
	});
	var savenew = $("#savenew").click(function() {
		$.post(
			"/notepad/",
			text.html(),
			function(nr) {
				$(build(nr))
					.appendTo(element);
			})
	});
	$("#new").click(function() {
		text.html("");
		savenew.click();
	});
});
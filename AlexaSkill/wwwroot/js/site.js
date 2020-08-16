var deviceId = "amzn1.ask.device.AFBC3NEJJ6JCOHJEDWFQ6DB3XXIZAIQ6HWR2JDPGAIABRRFCMWNDNNUUT2X4CC3HCRBFH3DSADVNHPJMVSMAIYICZVD6SH3FGCL4TORMF4T3GTO5N3RHGFM6XKOVQHDJRKD6GAGKV5NQRLARWWYQCVILTUC57SNY67RLE7T3FZ3C4CDNLVKCC";
var apiEndpoint = "https://api.amazonalexa.com";
var applicationId = "amzn1.ask.skill.bd47a0c1-f7ec-46a6-80de-0f04fab85ebd";

var postData = {
	"request": {
		"type": ""
	},
	"context": {
		"System": {
			"user": {
				"accessToken": ""
			},
			"device": {
				"deviceId": deviceId
			},
			"apiEndpoint": apiEndpoint,
			"apiAccessToken": "",
			"application": {
				"applicationId": applicationId
			}
		}
	}
};

var intentData = {
	"name": "[intent name]",
	"confirmationStatus": "NONE",
	"slots": {}
};

function ajaxCallPOST(endpointUri, jsondata, error, success) {
	$.ajax({
		url: endpointUri,
		type: "POST",
		data: jsondata,
		contentType: "application/json; charset=utf-8",
		success: success,
		error: error
	});
}

function ajaxError(error) {
	if (error.responseText.error) {
		console.log(JSON.stringify(error.responseText.error));
	}
}

function postForm(x, y) {
	postData.request.type = x;
	postData.context.System.apiAccessToken = $("#apiAccessToken").val();
	postData.context.System.user.accessToken = $("#graphAccessToken").val();

	if (y != null) {
		postData.request["intent"] = {};
		postData.request.intent = JSON.parse(JSON.stringify(intentData));
		postData.request.intent.name = y;
	}

	console.log("REQUEST: " + JSON.stringify(postData))

	ajaxCallPOST("/", JSON.stringify(postData), ajaxError, function (data) {
		console.log("RESPONSE: " + data);
	});
}
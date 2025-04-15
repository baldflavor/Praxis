"use strict";

/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	Generic javascript utility functions

	ver 18
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
let praxis = {
	commandLock: [],

	domLoadFunctions: [],


	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	* Handles the ready state change of an XmlHttpRequest
	*
	* xmlhttp: XmlHttp request firing the event
	* successCallback: method to call when request status is 200
	* failCallback: method to call when request is not 200
	* */
	_ajaxOnReadyStateChange: function (xmlhttp, successCallback, failCallback) {
		if (xmlhttp.readyState === 4) {
			let responseText = xmlhttp.responseText;
			let statusCode = xmlhttp.status;
			let headerDict = {};
			let headers = xmlhttp.getAllResponseHeaders().split('\r\n');
			for (let header of headers) {
				let cIndex = header.indexOf(':');
				let key = header.substring(0, cIndex);
				let value = header.substring(cIndex + 2);
				headerDict[key] = value;
			}

			if (xmlhttp.status === 200 || xmlhttp.status === 201 || xmlhttp.status === 202 || xmlhttp.status === 204 || xmlhttp.status === 206) {
				if (successCallback)
					successCallback(responseText, statusCode, headerDict);
			}
			else {
				if (failCallback)
					failCallback(responseText, statusCode, headerDict);
			}
		}
	},

	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	* Add to the internal list of functions held in `domLoadFunctions` that can be executed using code similar to:
	* `document.addEventListener("DOMContentLoaded", (event) => praxis.onDOMLoaded());`
	*
	* func: Code to push to the stack of functions/code executed when the DOM is loaded
	* */
	addDOMLoadFunction: function (func) {
		this.domLoadFunctions.push(func);
	},


	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	*	A Callback method for ajax failures. Also will handle authentication errors
	*	
	*	response: What is returned from the server
	*	statusCode: The status code of the error
	* */
	ajaxFailure: function (response, statusCode) {
		if (statusCode === 403) {
			alert("Authentication Error. Please try logging in.");
			console.debug(response);
			window.close();
		}
		else {
			alert("A critical failure has occurred processing your request. Please contact technical support.");
			console.debug(response);
			window.location.reload();
		}
	},


	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	Appends a new element to the parent element.

	parent: element that the new one will be appended to
	childTag: the tag (e.g. 'li', 'div') used to create the new element
	textContent: optional textContent to set on the new element

	returns: the newly created element
	* */
	addCreateElem: function (parent, childTag, textContent) {
		let newElem = document.createElement(childTag);
		parent.appendChild(newElem);

		if (textContent)
			praxis.addTextNode(newElem, textContent);

		return newElem;
	},


	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	Appends a text node to the passed element with the given text
	
	elem: element to add the text node to
	text: the textual content to add to the element
	* */
	addTextNode: function (elem, text) {
		elem.appendChild(document.createTextNode(text));
	},


	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	*	Checks to see if a given command is locked
	*	
	*	name: The name of the command to be checked
	*	
	*	returns: true if the commandLock contains the given name or false if it does not contain the given name
	* */
	checkCommandLock: function (name) {
		if (praxis.commandLock.indexOf(name) > -1)
			return true;
		else
			return false;
	},


	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	Builds an encoded query string using the target object for it's payload of data
	
	target: Object that will be used to create the an encoded querystring
	url: If passed, then the query string will be added to the end of it for return
	
	Returns:
		A string suitable as a query string, or a full url depending on the passed arguments
	* */
	createQueryStringUrl: function (target, url) {
		let ret = [];
		for (let t in target)
			ret.push(`${encodeURIComponent(t)}=${encodeURIComponent(target[t])}`);
		let queryString = ret.join('&');

		if (url) {
			let delimchar = url.includes("?") ? "&" : "?";
			return `${url}${delimchar}${queryString}`;
		}
		else {
			return queryString;
		}
	},



	dateToString: function (date) {
		let padStr = (int) => (int < 10) ? "0" + int : "" + int;
		return `${padStr(date.getFullYear())}-${padStr(1 + date.getMonth())}-${padStr(date.getDate())} ${padStr(date.getHours())}:${padStr(date.getMinutes())}:${padStr(date.getSeconds())}`;
	},


	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	*	Disables a given command by adding the name to the commandLock array
	*	
	*	name: The name of the command to be checked
	* */
	disableCommand: function (name) {
		if (praxis.commandLock.indexOf(name) < 0)
			praxis.commandLock.push(name);
	},


	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	*	Enables a given command by removing the name from the commandLock array
	*
	*	name: The name of the command to be checked
	* */
	enableCommand: function (name) {
		var index = praxis.commandLock.indexOf(name);
		if (index > -1)
			praxis.commandLock.splice(index, 1);
	},


	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	* Performs an Ajax GET to a server endpoint
	* The success and fail callbacks are both provIded with arguments of the following:
	* 	*response text
	* 	*status code
	* 	*header dictionary
	* 
	* data: Object to be used as query string parameters to *url
	* url: Target location of the request
	* successCallback: method to call when request status is 200
	* failCallback: method to call when request is not 200
	* */
	getAjax: function (data, url, successCallback, failCallback) {
		let fullUrl;

		if (data)
			fullUrl = praxis.createQueryStringUrl(data, url);
		else
			fullUrl = url;

		let xmlhttp = new XMLHttpRequest();
		xmlhttp.open("GET", fullUrl, true);
		xmlhttp.onreadystatechange = () => praxis._ajaxOnReadyStateChange(xmlhttp, successCallback, failCallback);
		xmlhttp.send();
	},

	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	Escapes text so that potentially dangerous characters are replaced with HTML code equivalents

	target: string to escape
	* */
	htmlEncode: function (target) {
		return target
			.replace(/&/g, '&amp;')
			.replace(/"/g, '&quot;')
			.replace(/'/g, '&#39;')
			.replace(/</g, '&lt;')
			.replace(/>/g, '&gt;');
	},


	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	Unescapes text so that HTML code characters escaped above are revereted to their natural text representation

	target: string to unescape
	* */
	htmlDecode: function (target) {
		return target
			.replace(/&quot;/g, '"')
			.replace(/&#39;/g, "'")
			.replace(/&lt;/g, '<')
			.replace(/&gt;/g, '>')
			.replace(/&amp;/g, '&');
	},


	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	Determines whether the passed (string) value is not of a substantial value

	target: string to test

	returns: true if the passed string does not have an substantial value, otherwise false
	* */
	isNullOrWhiteSpace: function (target) {
		return !target || target.trim().length < 1;
	},


	jsonDateStr: function (str, blankZeroDate) {
		let miliseconds = Date.parse(str);
		if (blankZeroDate === true && miliseconds === -62135579038000)
			return "";

		return this.dateToString(new Date(miliseconds));
	},

	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	* Executes each function held in `domLoadFunctions`. This code should Ideally be executed in a similar fashion to:
	* `document.addEventListener("DOMContentLoaded", (event) => praxis.onDOMLoaded());`
	* */
	onDOMLoaded: function () {
		for (let fu of this.domLoadFunctions) {
			fu();
		}
	},


	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	*	Performs an Ajax POST to a server endpoint
	*	The success and fail callbacks are both provIded with arguments of the following:
	*		*response text
	*		*status code
	*		*header dictionary
	* 
	*	 data: Object to be serialzed as JSON or sent as FormData to the endpoint
	*	 url: Target location of the request
	*	 successCallback: method to call when request status is 200
	*	 failCallback: method to call when request is not 200
	*	 sendAsFormData: if set to true, will post [data] as FormData
	* */
	postAjax: function (data, url, successCallback, failCallback, sendAsFormData) {
		let xmlhttp = new XMLHttpRequest();
		xmlhttp.open("POST", url, true);
		xmlhttp.onreadystatechange = xmlhttp.onreadystatechange = () => praxis._ajaxOnReadyStateChange(xmlhttp, successCallback, failCallback);

		let dataToSend = null;
		if (data) {
			if (sendAsFormData === true) {
				dataToSend = new FormData();
				for (let d in data)
					dataToSend.set(d, data[d]);
			}
			else {
				xmlhttp.setRequestHeader("Content-Type", "application/json;charset=UTF-8");
				dataToSend = JSON.stringify(data);
			}
		}

		xmlhttp.send(dataToSend);
	},


	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	* Prevents using the back button by overwriting the push state of the current location in history	
	* */
	preventBackNav() {
		history.pushState(null, null, document.location.href);
		window.addEventListener('popstate', function () {
			history.pushState(null, null, document.location.href);
		});
	},


	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	Removes all children from the passed target element
	
	target: element to remove all children from
	* */
	removeChildren: function (target) {
		while (target.hasChildNodes()) {
			target.removeChild(target.firstChild);
		}
	},


	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	*	Remove an object from an array
	* 
	*	array: The array to search and remove from
	*	key: The property to match on
	*	target: The value to search/match on
	*	
	*	returns: The removed object
	* */
	removeFromArray(array, key, target) {
		let index = array.map(function (e) { return e[key]; }).indexOf(target);

		if (index === -1)
			return null;

		let value = array[index];
		array.splice(index, 1);

		return value;
	},


	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	*	Handles submission of a form to the server
	*
	*	url: The URL to post the form data too
	*	formId: The Id of the form tag
	*	successCallBack: A callback method to be called if the form passes valIdation
	*	failureCallback: A callback method if the server returns an error
	*	successCallbackArg: OPTIONAL - pass additional argument or callback to the secessfull call back
	* */
	submitForm: function (url, formId, successCallback, failureCallback, successCallbackArg) {
		let form = document.getElementById(formId);
		let data = new FormData(form);

		let xmlhttp = new XMLHttpRequest();
		xmlhttp.open("POST", url, true);
		xmlhttp.onreadystatechange = function () {
			if (xmlhttp.readyState === 4) {
				if (xmlhttp.status === 200) {
					if (successCallback)
						successCallback(xmlhttp.responseText, successCallbackArg);
				} else {
					if (failureCallback)
						failureCallback(xmlhttp.responseText);
				}
			}
		};

		xmlhttp.send(data);
	},


	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	*	Handles valIdation results of a form and displays error messages
		*		*Response from server needs to be a <JsonExResult<DataTransport<***>> object
	*
	*	responseJson: The response from the server that has been parsed into JSON
	*	formId: The Id of the form tag
	*	valIdationId: OPTIONAl - The Id of the form valIdation ul tag
	*	errorClass: The name of the errorClass to added to form fields that had an error
	* */
	valIdateForm: function (responseJson, formId, valIdationId, errorClass) {
		let valSummaryList = document.getElementById(valIdationId);

		if (valSummaryList !== null) {
			valSummaryList.style.display = "none";
			praxis.removeChildren(valSummaryList);
		}

		let form = document.getElementById(formId);
		let errorElements = form.getElementsByClassName(errorClass);
		while (errorElements.length > 0) {
			errorElements[0].classList.remove(errorClass);
		}

		if (responseJson.ValIdationFailures !== null && responseJson.ValIdationFailures.length > 0) {
			let valIdationResults = responseJson.ValIdationFailures;

			for (let result of valIdationResults) {
				let formPropName = "";

				if (result.MemberNames[0]) {
					formPropName = result.MemberNames[0];
				}

				//	Adds error class to form elements
				let memberNames = result.MemberNames;
				for (let memberName of memberNames) {
					let element = document.getElementById(memberName);

					if (element !== null) {
						element.classList.add(errorClass);
						formPropName = memberName;
					}
				}

				//	Adds an list item to the valIdation summary list element
				if (valSummaryList !== null) {
					let li = praxis.addCreateElem(valSummaryList, "li");
					li.classList.add(errorClass);

					let message = result.ErrorMessage;
					if (formPropName.length > 0) {
						message = `${formPropName} - ${message}`;
					}

					praxis.addTextNode(li, message);

					valSummaryList.style.display = "block";
				}
			}
			return false;
		}

		return true;
	}
};
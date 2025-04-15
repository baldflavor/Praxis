"use strict";


/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	Generic javascript for web storage database functions

	ver 1
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
let praxisWS = {
	db: null,	// The object containing the indexed database
	logUrl: null,	// url endpoint for loging all errors generated
	isDebug: true,	// In debug mode or not


	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
		Creates a new entry in the browsers indexed db

		objStoreName: The name of the object store in the database
		data: The JSON object to be stored
		callback(optional): A callback method to be called on sucesses
	* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
	create: function (objStoreName, data, callback) {
		let req = praxisWS.db.transaction([objStoreName], "readwrite").objectStore(objStoreName).add(data);

		req.onsuccess = function (event) {
			if (callback)
				callback();
		};

		req.onerror = function (event) {
			praxisWS.log(event);
		};
	},


	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
		Creates a new object store in the database

		objStoreName: The name of the object store
		keyId: The name of the memeber variable to be used as the Id
		callback(optional): A callback method to be called on sucesses
	* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
	createObjectStore: function (objStoreName, keyId, callback) {
		praxisWS.db.createObjectStore(objStoreName, { keypath: keyId });

		if (callback)
			calback();
	},


	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
		Delete an entry in the browsers indexed db

		objStoreName: The name of the object store in the database
		id: The Id of the entry to be removed
		callback(optional): A callback method to be called on sucesses
	* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
	delete: function (objStoreName, id, callback) {
		let req = praxisWS.db.transaction([objStoreName], "readwrite")
			.objectStore(objStoreName)
			.delete(id);

		req.onsuccess = function (event) {
			if (callback)
				callback();
		};

		req.onerror = function (event) {
			praxisWS.log(event);
		};
	},


	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
		Gets a JSON object from the browsers indexed db

		objStoreName: The name of the object store in the database
		id: The Id of the json object to be retrived
		callback(optional): A callback method to be called on sucesses

		returns: A JSON object
	* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
	get: function (objStoreName, id, callback) {
		let tran = praxisWS.db.transaction([objStoreName]);
		let objStore = tran.objectStore(objStoreName);
		let req = objStore.get(id);

		req.onerror = function (event) {
			praxisWS.log(event);
		};

		req.onsuccess = function (event) {
			if (callback)
				callback(req.result);
			else
				return req.result;
		};
	},


	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
		Gets all JSON object from an object store

		objStoreName: The name of the object store in the database
		callback(optional): A callback method to be called on sucesses

		returns: An array of JSON objects
	* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
	getAll: function (objStoreName, callback) {
		var tran = praxisWS.db.transaction([objStoreName]);
		var objStore = tran.objectStore(objStoreName);
		var items = [];

		tran.oncomplete = function () {
			if (callback) {
				callback(items);
			}
			else {
				return items;
			}
		};

		var cursorReq = objStore.openCursor();

		cursorReq.onerror = function (event) {
			praxisWS.log(event);
		};

		cursorReq.onsuccess = function (event) {
			var cursor = event.target.result;
			if (cursor) {
				items.push(cursor.value);
				cursor.continue();
			}
		};
	},

	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
		intializes the javascript and the index database

		dbName: The name for the database
		objStoreNames: An array containing the object store names for the db schema
		keyIds: An array containing the name of the key path id. MUST match the order of the objectStoreNames
		callback(optional): A callback method to be called on sucesses
	* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
	initalize: function (dbName, objStoreNames, keyIds, callback) {
		if (!window.indexedDB) {
			praxisWS.log("This browser doesn't support a stable sersion of IndexedDB", 0);
		}

		let req = window.indexedDB.open(dbName);

		req.onerror = function (event) {
			praxisWS.log(event);
		};

		req.onsuccess = function (event) {
			praxisWS.db = req.result;

			if (callback)
				callback();
		};

		req.onupgradeneeded = function (event) {
			let db = event.target.result;
			for (let i in objStoreNames) {
				let osn = objStoreNames[i];
				db.createObjectStore(osn, { keyPath: keyIds[i] });
			}
		};
	},


	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	TODO: NOT FINISHED 
		Logs a message

		message (string): target content to be logged
		errorCode (int): possible error code if the message is describing an error (otherwise undefined)
	* */
	log: function (message, errorCode) {
		//31205 = JWT expiration. Not.. "really" an error
		//if (errorCode === 31205) {
		//	return;
		//}

		if (praxisWS.isDebug) {
			console.info(message);
		}

		// came from praxis ws
		//if (typeof message === String) {
		//	praxis.postAjax({ "message": message, "errorCode": errorCode }, praxisWS.logUrl, null, function (response) { console.error(response); });
		//}
	},

	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
		Update an indexed db entry

		objStoreName: The name for a new the object store 
		data: The json object to be update
		callback(optional): A callback method to be called on sucesses
	* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
	update: function (objStoreName, data, callback) {
		let tran = praxisWS.db.transaction([objStoreName], "readwrite");
		let objStore = tran.objectStore(objStoreName);
		let req = objStore.put(data);

		req.onerror = function (event) {
			praxisWS.log(event);
		};

		req.onsuccess = function (event) {
			if (req.result) {
				if (callback)
					callback(req.result);
			}

			else {
				praxisWS.log(event);
			}
		};
	}
};
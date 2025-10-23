var ezgoAnalytics = {
    dataArray: [],
    dataArrayBuffer: [],
    elementsToTrack: document.querySelectorAll('[data-tracking="enabled"]'),
    analyticApplication: "CMS",
    maxDataLogs: 1,
    sending: false,

    init: function (moduleName) {
        ezgoAnalytics.moduleName = moduleName;
        ezgoAnalytics.elementsToTrack.forEach(function (ele) {
            ele.addEventListener("click", function () { ezgoAnalytics.logClickEvent(ele) });
        });
        //for (let i = 0; i < ezgoAnalytics.elementsToTrack.length; i++) {
        //    ezgoAnalytics.elementsToTrack[i].addEventListener("click", function () { logClickEvent(ezgoAnalytics.elementsToTrack[i]) });
        //}
    },

    logClickEvent: function (element) {
        let analyticDateUtc = new Date().toISOString();
        let analyticType = "click";
        let analyticName = element.getAttribute('data-tracking-name');
        let analyticModule = element.getAttribute('data-tracking-module');
        let customProperties = element.getAttribute('data-tracking-storage');

        let dataEntry = new DataEntry(analyticDateUtc, analyticType, analyticName, analyticModule);
        dataEntry.addCustomProperties(customProperties);

        ezgoAnalytics.logData(dataEntry);
    },

    logData: function (dataLog) {

        if (ezgoAnalytics.dataArray.length < ezgoAnalytics.maxDataLogs) {
            ezgoAnalytics.dataArray.push(dataLog);
        } else {
            ezgoAnalytics.dataArrayBuffer.push(dataLog);
        }

        if (ezgoAnalytics.dataArray.length >= ezgoAnalytics.maxDataLogs) {
            ezgoAnalytics.sendData();
        }
    },

    //Use to start a log of type Time_Span
    //returns a DataEntry object to be provided to dataSpanEnd
    //customProperties format [name]:[value]; [name]:[value] without the []
    dataSpanStart: function (name, module, customProperties) {
        let now = new Date().toISOString();
        let dataEntry = new DataEntry(now, "Time_Span", name, module);

        dataEntry.DateStartUtc = now;
        dataEntry.addCustomProperties(customProperties);
        return dataEntry;
    },

    //Used to end the log of type Time_Span and log the data
    //Provide the DateEntry object that was returned by dataSpanStart
    //customProperties format [name]:[value]; [name]:[value] without the []
    dataSpanEnd: function (dataEntry, customProperties) {
        dataEntry.DateEndUtc = new Date().toISOString();
        dataEntry.addCustomProperties(customProperties);

        ezgoAnalytics.logData(dataEntry)
    },

    //use to log any non-standard single event
    //customProperties format [name]:[value]; [name]:[value] without the []
    customLog: function (type, name, module, customProperties) {
        let dataEntry = new DataEntry(new Date().toISOString(), type, name, module);
        dataEntry.addCustomProperties(customProperties);
        ezgoAnalytics.logData(dataEntry);
    },

    sendData: function () {
        ezgoAnalytics.sending = true;
        let endpoint = '/analytics/logdata';
        let xhr = new XMLHttpRequest();

        xhr.onreadystatechange = function () {
            if (this.readyState == 4) {
                if (this.status == 200) {
                    ezgoAnalytics.dataArray.length = 0;
                    if (ezgoAnalytics.dataArrayBuffer.length > 0) {
                        ezgoAnalytics.dataArray.push(...ezgoAnalytics.dataArrayBuffer.splice(0, ezgoAnalytics.maxDataLogs));
                    }
                } else if (this.status == 503) {
                    console.log("logging is disabled");
                }
                ezgoAnalytics.sending = false;
            }
        };

        xhr.open("POST", endpoint);
        xhr.setRequestHeader('Content-Type', 'application/json');
        xhr.send(JSON.stringify(ezgoAnalytics.dataArray));
    }
}

window.addEventListener("beforeunload", function (e) {
    if (ezgoAnalytics.dataArray.length > 0 && !ezgoAnalytics.sending) {
        ezgoAnalytics.sendData();
    }
});

ezgoAnalyticsSeparator = ";";

class DataEntry {

    constructor(analyticDateUtc, analyticType, analyticName, analyticModule) {
        this.DateUtc = analyticDateUtc;
        this.AnalyticType = analyticType;
        this.Name = analyticName;
        this.Application = ezgoAnalytics.analyticApplication;
        this.ModuleType = analyticModule ? analyticModule : ezgoAnalytics.moduleName;
    }

    //customProperties format [name]:[value]; [name]:[value] without the []
    addCustomProperties(customProperties) {
        const customPropertiesArray = customProperties ? customProperties.split(ezgoAnalyticsSeparator) : [];

        //add custom properties
        for (let i = 0; i < customPropertiesArray.length; i++) {
            const property = customPropertiesArray[i].trim().split(":");
            let name = property[0].trim();
            let value = property[1].trim();

            this[name] = value;
        }
    }
}
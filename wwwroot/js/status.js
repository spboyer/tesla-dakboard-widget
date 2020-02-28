"use strict";

let connection = new signalR.HubConnectionBuilder()
    .withUrl("/statusHub")
    .configureLogging(signalR.LogLevel.Trace)
    .build();

connection.on("SendStatus", function (cars) {
    console.log(cars[0].displayName);

    var output = "";
    cars.forEach(function (car) {
        var carStatus = `<div style="text-align:center; display:inline-block">
        <div class="vehicle">
            <button type="button" style="width:150px; margin-bottom: 5px; opacity: 0.7" class="btn btn-secondary">${car.displayName}</button>
            <div>
                <img src="/img/Tesla_Model_3.png">
                    </div>
                <div class="vehicle-battery">${car.batteryRange.toFixed(0)} mi (<span class="vehicle-battery-percentage">${car.batteryLevel.toString()} %</span>)</div>
                <div class="vehicle-odometer">${formatNumber(car.odometer.toFixed(0))}</div>
            </div>
            </div><br /><br />`

        output += carStatus;
    })

    document.getElementById("car-status").innerHTML = output;
   
});

function formatNumber(num) {
    return num.toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1,')
}

connection.start().then(function () {
//    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

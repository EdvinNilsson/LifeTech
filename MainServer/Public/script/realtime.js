function httpGet(url, responseCallback) {
    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function() {
        if (xhr.readyState === XMLHttpRequest.DONE) {
            responseCallback(xhr.responseText, xhr.status);
        }
    };
    xhr.open('GET', url, true);
    xhr.send();
}

var sensorNames = {};
var sensorUnits;

var run;

function distinct(value, index, self) {
    return self.indexOf(value) === index;
}

function updateGraph() {
    var sensors = myChart.data.datasets.filter(c => !c.hidden).map(c => c.sensorId).filter(distinct);
    if (sensors.length === 0) { setTimeout(() => updateGraph(), 1000); return; }
    httpGet('/realtid/get-latest-sensor-value?sensors=' + sensors.join(','), function (response, statusCode) {
        myChart.data.labels.push(new Date().toLocaleString());
        var shift = myChart.data.labels.length > 60;
        myChart.data.datasets.forEach(value => {
            if (shift) value.data.shift();
            value.data.push(NaN);
        });

        if (statusCode === 200) {
            response.split(' ').forEach(value => {
                value = value.split(',');
                try {
                    var arr = myChart.data.datasets.find(c => c.sensorId === value[0] && c.sensorType === value[1]).data;
                    arr[arr.length - 1] = parseFloat(value[2]);
                } catch (e) {}
            });
        }
        if (shift) myChart.data.labels.shift();
        
        myChart.update({
            duration: 300,
            easing: 'easeInOutSine'
        });
        if (typeof run === 'function') { run(); run = null; }

        if (statusCode === 200 || statusCode === 408)
            updateGraph();
        else
            setTimeout(() => { updateGraph(); }, 1000);
    });
}

var htmlChart = document.getElementById('myChart');
var ctx = htmlChart.getContext('2d');
var myChart = new Chart(ctx, {
    type: 'line',
    data: {},
    options: {
        scales: {
            yAxes: [{
                ticks: {
                    beginAtZero: true,
                    callback: (value, index, values) => value.toLocaleString()
                }
            }]
        },
        tooltips: {
            callbacks: {
                label: (tooltipItem, data) => tooltipItem.yLabel.toLocaleString() + ' ' + sensorUnits[data.datasets[tooltipItem.datasetIndex].sensorType]
            }
        },
        legend: {
            onClick: function (e, legendItem) {
                var index = legendItem.datasetIndex;
                var sensorId = myChart.data.datasets[index].sensorId;
                var sensorType = myChart.data.datasets[index].sensorType;
                var state = myChart.data.datasets[index].hidden;
                myChart.data.datasets[index].hidden = !state;
                myChart.update();
                if (!state) return;

                run = function () {
                    httpGet('/realtid/get-sensordata?sensors=' + sensorId, function (response, statusCode) {
                        if (statusCode === 200) {
                            var data = JSON.parse(response.split('|')[1]);
                            myChart.data.datasets[index].data = data[sensorId][sensorType].map(c => c == 0 ? NaN : c);
                        }
                        myChart.update();
                    });
                };
            }
        },
        maintainAspectRatio: false
    }
});

function getParameterByName(name, url) {
    if (!url) url = window.location.href;
    name = name.replace(/[\[\]]/g, '\\$&');
    var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)'), results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, ' '));
}

function generateColor(value) {
    var hash = 23;
    for (var i = 0; i < 4; ++i) {
        hash = hash * 31 + value << i;
    }
    return '#' + (Math.abs(hash) % 16777215).toString(16).padStart(6, '0');
}

httpGet('/realtid/get-sensor-names-units', function (response, statusCode) {
    if (statusCode !== 200) return;

    var parts = response.split('#');
    parts[0].split('|').forEach(sensor => {
        var subParts = sensor.split(',');
        sensorNames[subParts[0]] = subParts[1];
    });

    sensorUnits = parts[1].split(',');
    var senasteParam = getParameterByName('senaste');
    
    var freq = 1;
    switch (senasteParam) {
        case 'timmen':
            freq = 60;
            break;
        case 'dygnet':
            freq = 300;
            break;
        case 'veckan':
            freq = 1800;
            break;
        case 'all':
            freq = 3600;
            break;
    }

    var senaste = senasteParam ? '&senaste=' + senasteParam : '';
    httpGet('/realtid/get-sensordata?sensors=' + Object.keys(sensorNames).join(',') + senaste, function(response, statusCode) {
        if (statusCode !== 200) return;
        var parts = response.split('|');
        var date = new Date((parts[0] - freq) * 1000);
        var data = JSON.parse(parts[1]);
        for (const [key, value] of Object.entries(data)) {
            for (const [key2, value2] of Object.entries(value)) {
                var color = generateColor(key + key2 * 89);
                myChart.data.datasets.push({
                    label: sensorNames[key] + ' (' + sensorUnits[key2] + ')',
                    data: value2.map(c => c == 0 ? NaN : c),
                    sensorId: key,
                    sensorType: key2,
                    fill: false,
                    backgroundColor: color,
                    borderColor: color + 'aa'
                });
                var samples = value2.length;
            }
        }
        myChart.data.labels = Array(samples).fill().map(() => {
            date.setSeconds(date.getSeconds() + freq); return date.toLocaleString();
        });
        myChart.update();
        if (freq === 1) updateGraph();
    });
});
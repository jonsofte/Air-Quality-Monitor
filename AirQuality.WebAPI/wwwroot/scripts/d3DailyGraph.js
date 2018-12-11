// Oppsett
var inputData;
var latestDataValue;
var redValueLine = 60;
var yellowValueLine = 30;

var margin = { top: 15, right: 15, bottom: 20, left: 30 },
    width = parseInt(d3.select('#d3graphDaily').style('width'));
    width = width - 20 - margin.left - margin.right;

if (width < 760) {
    height = (400 - margin.top - margin.bottom) * (width / 760);
}
else {
    height = 400 - margin.top - margin.bottom;
}

var parseTime = d3.timeParse("%Y-%m-%dT%H:%M:%S.%LZ");
var formatTime = d3.timeFormat("%Y-%m-%d %H:%M:%S");

// Definere skala
var x = d3.scaleTime().range([0, width]);
var y = d3.scaleLinear().range([height, 0]);

// Definere PM10 linje
var valuelinePM100 = d3.line()
    .x(function (d) { return x(d.readDateTime); })
    .y(function (d) { return y(d.pM100); });

// Definere PM2.5 linje
var valuelinePM025 = d3.line()
    .x(function (d) { return x(d.readDateTime); })
    .y(function (d) { return y(d.pM025); });

var svg = d3.select("#graphDaily")
    .attr("width", width + margin.left + margin.right)
    .attr("height", height + margin.top + margin.bottom)
    .append("g")
    .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

// Clip Path
svg.append("defs").append("clipPath")
    .attr("id", "clip")
    .append("rect")
    .attr("width", (width))
    .attr("height", (height));

// Funksjon for X-akse
function make_x_gridlines() {
    return d3.axisBottom(x)
        .ticks(24);
}

// Funksjon for Y-akse
function make_y_gridlines() {
    return d3.axisLeft(y).ticks(10)
}

function parsedataPoint(d) {
    d.readDateTime = parseTime(d.readDateTime);
    d.pM010 = +d.pM010;
    d.pM025 = +d.pM025;
    d.pM100 = +d.pM100;
    return d;
}

// Hent og parse data
d3.json("/api/airquality", function (error, dailyData) {
    if (error) {
        console.error(error);
    }

    inputData = dailyData;
    inputData.forEach(function (d) {
        d.readDateTime = parseTime(d.readDateTime);
        d.pM010 = +d.pM010;
        d.pM025 = +d.pM025;
        d.pM100 = +d.pM100;
    });

    latestDataValue = inputData[inputData.length - 1]

    // Domene-verdier for mapping av input
    x.domain(d3.extent(inputData, function (d) { return d.readDateTime; }));
    y.domain([0, d3.max(inputData, function (d) { return d.pM100; }) * 1.05]);

    //Fargebånd i bakgrunn

    var yellowPos = y(redValueLine) < 0 ? 0 : y(redValueLine);
    var yellowHeight = y(yellowValueLine) - (y(redValueLine) < 0 ? 0 : y(redValueLine));
    if (yellowHeight < 0) yellowHeight = 0;
    var greenHeight = y(yellowValueLine) < 0 ? 0 : y(yellowValueLine);

    svg.append("svg:rect")
        .attr("class", "bands red")
        .style("fill", "red")
        .style("opacity", "0.03")
        .attr('width', width)
        .attr('height', yellowPos);

    svg.append("svg:rect")
        .attr("class", "bands yellow")
        .style("fill", "yellow")
        .style("opacity", "0.03")
        .attr('width', width)
        .attr('height', yellowHeight)
        .attr("transform", "translate(0," + yellowPos + ")");

    svg.append("svg:rect")
        .attr("class", "bands green")
        .style("fill", "green")
        .style("opacity", "0.03")
        .attr('width', width)
        .attr('height', height - greenHeight)
        .attr("transform", "translate(0," + greenHeight + ")");

    // X grid
    svg.append("g")
        .attr("class", "gridx")
        .attr("transform", "translate(0," + height + ")")
        .call(make_x_gridlines()
            .tickSize(-height)
            .tickFormat("")
        )

    // Y grid
    svg.append("g")
        .attr("class", "gridy")
        .call(make_y_gridlines()
            .tickSize(-width)
            .tickFormat("")
        )

    // Linje for PM10-verdier
    svg.append("g")
        .attr("clip-path", "url(#clip)")
        .append("path")
           .data([inputData])
            .attr("class", "linePM100")
            .attr("d", valuelinePM100);

    // Linje for PM2.5-verdier
    svg.append("g")
        .attr("clip-path", "url(#clip)")
        .append("path")
            .data([inputData])
            .attr("class", "linePM025")
            .attr("d", valuelinePM025);

    // Legg til X-akse
    svg.append("g")
        .attr("class", "x axis")
        .attr("transform", "translate(0," + height + ")")
        .call(d3.axisBottom(x).tickFormat(d3.timeFormat("%H:%M")))
        .style("font-family", "Montserrat");

    // Legg til Y-akse
    svg.append("g")
        .attr("class", "y axis")
        .call(d3.axisLeft(y))
        .style("font-family", "Montserrat");

    var mouseG = svg.append("g")
        .attr("class", "mouse-over-effects");

    //  Vertikal linje for peker
    mouseG.append("path")
        .attr("class", "mouse-line")
        .style("stroke", "black")
        .style("stroke-width", "1px")
        .style("opacity", "0.5");

    mouseG.append('svg:rect') // For håndtering av peker
        .attr('width', width)
        .attr('height', height)
        .attr('fill', 'none')
        .attr('pointer-events', 'all')
        .on('mouseout', function () {    // Skjul linje
            d3.select(".mouse-line")
                .style("opacity", "0");
            d3.select("#panelLastDayStatDiv").attr("class", "hidden");
        })
        .on('mouseover', function () { // Vis linje
            d3.select(".mouse-line")
                .style("opacity", "0.5");
        })
        .on('mousemove', function () { // Flytt linje
            var mouse = d3.mouse(this);

            d3.select(".mouse-line")
                .attr("d", function () {
                    var d = "M" + mouse[0] + "," + height;
                    d += " " + mouse[0] + "," + 0;
                    return d;
                });

            var beginning = 0,

                // Finn verdi for PM10
                line = d3.select(".linePM100");
            end = line.node().getTotalLength(),
                target = null;

            while (true) {
                target = Math.floor((beginning + end) / 2);
                pos = line.node().getPointAtLength(target);
                if ((target === end || target === beginning) && pos.x !== mouse[0]) {
                    break;
                }
                if (pos.x > mouse[0]) end = target;
                else if (pos.x < mouse[0]) beginning = target;
                else break;
            }

            var pm100value = y.invert(pos.y);
            var beginning = 0,
                line = d3.select(".linePM025");
            end = line.node().getTotalLength(),
                target = null;
            while (true) {
                target = Math.floor((beginning + end) / 2);
                pos = line.node().getPointAtLength(target);
                if ((target === end || target === beginning) && pos.x !== mouse[0]) {
                    break;
                }
                if (pos.x > mouse[0]) end = target;
                else if (pos.x < mouse[0]) beginning = target;
                else break;
            }
            var pm025value = y.invert(pos.y);
            d3.select("#panelLastDayStatDiv").attr("class", "");

            //console.log("PM100: " + pm100value.toFixed(2) + " PM025: " + pm025value.toFixed(2));

            d3.select("#LastDayTimeValue").text(formatTime(x.invert(pos.x)));
            d3.select("#LastDayPM025Value").text(pm025value.toFixed(0));
            d3.select("#LastDayPM100Value").text(pm100value.toFixed(0));
        });

    d3.select("#panelPM025Value").text(latestDataValue.pM025);
    d3.select("#panelPM100Value").text(latestDataValue.pM100);
    d3.select("#panelLastUpdatedTime").text(formatTime(latestDataValue.readDateTime));    
    d3.select("#panelLatestValuesDiv").attr("class", "");
});

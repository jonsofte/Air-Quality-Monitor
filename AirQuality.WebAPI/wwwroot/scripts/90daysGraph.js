// Oppsett
var ninetyDaysinputData;
var ninetyDaysLatestDataValue;

var ninetyDaysRedValueLine = 60;
var ninetyDaysYellowValueLine = 30;

var ninetyDaysMargin = { top: 15, right: 15, bottom: 20, left: 30 },
    ninetyDaysWidth = parseInt(d3.select('#d3graph90days').style('width'));
    ninetyDaysWidth = ninetyDaysWidth - 20 - ninetyDaysMargin.left - ninetyDaysMargin.right;

if (ninetyDaysWidth < 760) {
    ninetyDaysHeight = (400 - ninetyDaysMargin.top - ninetyDaysMargin.bottom) * (ninetyDaysWidth / 760);
}
else {
    ninetyDaysHeight = 400 - ninetyDaysMargin.top - ninetyDaysMargin.bottom;
}

var ninetyDaysParseTime = d3.timeParse("%Y-%m-%dT%H:%M:%SZ");
var ninetyDaysFormatTime = d3.timeFormat("%Y-%m-%d");

var locale = d3.timeFormatLocale({
    "dateTime": "%Y-%m-%dT%H:%M:%SZ",
    "date": "%Y-%m-%d",
    "time": "%H:%M:%SZ",
    "periods": ["AM", "PM"],
    "days": ["Søndag", "Mandag", "Tirsdag", "Onsdag", "Torsdag", "Fredag", "Lørdag"],
    "shortDays": ["Søn", "Man", "Tir", "Ons", "Tor", "Fre", "Lør"],
    "months": ["Januar", "Februar", "Mars", "April", "Mai", "Juni", "Juli", "August", "September", "Oktober", "November", "Desember"],
    "shortMonths": ["Jan", "Feb", "Mar", "Apr", "Mai", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Des"]
});

var formatMillisecond = locale.format(".%L"),
    formatSecond = locale.format(":%S"),
    formatMinute = locale.format("%I:%M"),
    formatHour = locale.format(""),
    formatDay = locale.format("%a %d"),
    formatWeek = locale.format("%b %d"),
    formatMonth = locale.format("%B"),
    formatYear = locale.format("%Y");

function multiFormat(date) {
    return (d3.timeSecond(date) < date ? formatMillisecond
        : d3.timeMinute(date) < date ? formatSecond
        : d3.timeHour(date) < date ? formatMinute
        : d3.timeDay(date) < date ? formatHour
        : d3.timeMonth(date) < date ? (d3.timeWeek(date) < date ? formatDay : formatWeek)
        : d3.timeYear(date) < date ? formatMonth
        : formatYear)(date);
}


// Definere skala
var ninetyDaysX = d3.scaleTime().range([0, ninetyDaysWidth]);
var ninetyDaysY = d3.scaleLinear().range([ninetyDaysHeight, 0]);
var ninetyDaysXBand = d3.scaleBand().range([0, ninetyDaysWidth]).padding(0.2);

// Definere PM10 linje
var ninetyDaysValuelinePM025 = d3.line()
    .curve(d3.curveCardinal)
    .x(function (d) { return ninetyDaysX(d3.timeHour.offset(d.readDateTime, 12)); })
    .y(function (d) { return ninetyDaysY(d.avgPM100); });

var ninetyDaysSvg = d3.select("#graph90days")
    .attr("width", ninetyDaysWidth + ninetyDaysMargin.left + ninetyDaysMargin.right)
    .attr("height", ninetyDaysHeight + ninetyDaysMargin.top + ninetyDaysMargin.bottom)
    .append("g")
    .attr("transform", "translate(" + ninetyDaysMargin.left + "," + ninetyDaysMargin.top + ")");

// Funksjon for X-akse
function ninetyDaysMake_x_gridlines() {
    return d3.axisBottom(ninetyDaysX)
        .ticks(14)
        .tickFormat(multiFormat);
}

// Funksjon for Y-akse
function ninetyDaysMake_y_gridlines() {
    return d3.axisLeft(ninetyDaysY).ticks(10)
}

// Hent og parse data
d3.json("/api/AirQualityDayLog", function (error, ninetyData) {
    if (error) throw error;

    ninetyDaysinputData = ninetyData;

    ninetyDaysinputData.forEach(function (d) {
        d.readDateTime = ninetyDaysParseTime(d.readDateTime);
    });

    // Antall dager fra første til eldste gyldige verdi i array
    var ninetyDaysnumberOfBars = d3.timeDays(ninetyDaysinputData[0].readDateTime, ninetyDaysinputData[ninetyDaysinputData.length - 1].readDateTime).length;;
    var ninetyDaysBarWidth = ninetyDaysWidth / ninetyDaysnumberOfBars;

    ninetyDaysLatestDataValue = ninetyDaysinputData[ninetyDaysinputData.length - 1]

    // Domene-verdier for mapping av input
    ninetyDaysX.domain([d3.min(ninetyDaysinputData, function (d) { return d.readDateTime; })
        , d3.max(ninetyDaysinputData, function (d) { return d3.timeDay.offset(d.readDateTime, 1); })]);
    ninetyDaysXBand.domain([d3.min(ninetyDaysinputData, function (d) { return d.readDateTime; })
        , d3.extent(ninetyDaysinputData, function (d) { return d3.timeDay.offset(d.readDateTime, 1); })]);
    ninetyDaysY.domain([0, d3.max(ninetyDaysinputData, function (d) { return d.maxPM100; }) * 1.05]);

    //Fargebånd i bakgrunn

    var ninetyDaysYellowPos = ninetyDaysY(ninetyDaysRedValueLine) < 0 ? 0 : ninetyDaysY(ninetyDaysRedValueLine);
    var ninetyDaysYellowHeight = ninetyDaysY(ninetyDaysYellowValueLine) - (ninetyDaysY(ninetyDaysRedValueLine) < 0 ? 0 : ninetyDaysY(ninetyDaysRedValueLine));
    if (ninetyDaysYellowHeight < 0) ninetyDaysYellowHeight = 0;
    var ninetyDaysGreenHeight = ninetyDaysY(ninetyDaysYellowValueLine) < 0 ? 0 : ninetyDaysY(ninetyDaysYellowValueLine);

    ninetyDaysSvg.append("svg:rect")
        .attr("class", "ninetydays bands red")
        .style("fill", "red")
        .style("opacity", "0.03")
        .attr('width', ninetyDaysWidth)
        .attr('height', ninetyDaysYellowPos);

    ninetyDaysSvg.append("svg:rect")
        .attr("class", "ninetydays bands yellow")
        .style("fill", "yellow")
        .style("opacity", "0.03")
        .attr('width', ninetyDaysWidth)
        .attr('height', ninetyDaysYellowHeight)
        .attr("transform", "translate(0," + ninetyDaysYellowPos + ")");

    ninetyDaysSvg.append("svg:rect")
        .attr("class", "ninetydays bands green")
        .style("fill", "green")
        .style("opacity", "0.03")
        .attr('width', ninetyDaysWidth)
        .attr('height', ninetyDaysHeight - ninetyDaysY(30))
        .attr("transform", "translate(0," + ninetyDaysY(30) + ")");

    // X grid
    ninetyDaysSvg.append("g")
        .attr("class", "ninetydays gridx")
        .attr("transform", "translate(0," + ninetyDaysHeight + ")")
        .call(ninetyDaysMake_x_gridlines()
            .tickSize(-ninetyDaysHeight)
            .tickFormat("")
        )

    // Y grid
    ninetyDaysSvg.append("g")
        .attr("class", "ninetydays gridy")
        .call(ninetyDaysMake_y_gridlines()
            .tickSize(-ninetyDaysWidth)
            .tickFormat("")
        )

    ninetyDaysSvg.selectAll(".ninetydays.bar")
        .data(ninetyDaysinputData)
        .enter().append("rect")
        .attr("class", "ninetydays bar")
        .attr("x", function (d) { return ninetyDaysX(d.readDateTime); })
        .attr("width", ninetyDaysBarWidth - 1)
        .attr("y", function (d) { return ninetyDaysY(d.maxPM100); })
        .attr("height", function (d) { return (ninetyDaysY(d.minPM100) - ninetyDaysY(d.maxPM100)); });


    // Linje for PM2.5-verdier
    ninetyDaysSvg.append("path")
        .data([ninetyDaysinputData])
        .attr("class", "ninetydays linePM025")
        .attr("d", ninetyDaysValuelinePM025);

    // Legg til X-akse
    ninetyDaysSvg.append("g")
        .attr("class", "ninetydays x axis")
        .attr("transform", "translate(0," + ninetyDaysHeight + ")")
        .call(ninetyDaysMake_x_gridlines())
        .style("font-family", "Montserrat");

    // Legg til Y-akse
    ninetyDaysSvg.append("g")
        .attr("class", "ninetydays y axis")
        .call(d3.axisLeft(ninetyDaysY))
        .style("font-family", "Montserrat");

    var ninetyDaysMouseG = ninetyDaysSvg.append("g")
        .attr("class", "ninetydays mouse-over-effects");

    //  Vertikal linje for peker
    ninetyDaysMouseG.append("path")
        .attr("class", "ninetydays mouse-line")
        .style("stroke", "black")
        .style("stroke-width", "1px")
        .style("opacity", "0.5");

    ninetyDaysMouseG.append('svg:rect') // For håndtering av koordinater fra peker
        .attr('width', ninetyDaysWidth - 1)
        .attr('height', ninetyDaysHeight)
        .attr('fill', 'none')
        .attr('pointer-events', 'all')
        .on('mouseout', function () {
            d3.selectAll(".ninetydays.bar.selected").attr("class", "ninetydays bar");
            d3.select("#panel90DaysStatDiv").attr("class", "hidden");
        })
        .on('mousemove', function () { // Flytt linje
            var mouse = d3.mouse(this);

            if (mouse[0] > 1) {
                d3.select("#panel90DaysStatDiv").attr("class", "");
                d3.selectAll(".ninetydays.bar.selected").attr("class", "ninetydays bar");
                var bar = d3.selectAll(".ninetydays.bar").filter(function (d) { return (d.readDateTime <= ninetyDaysX.invert(mouse[0]) && d3.timeDay.offset(d.readDateTime, 1) >= ninetyDaysX.invert(mouse[0])) });
                bar.attr("class", "ninetydays bar selected");
                //console.log("Date:" + bar.data()[0].readDateTime + "Avg:" + bar.data()[0].avgPM25);
                if (bar.data().length > 0) {
                    d3.select("#NinetyDaysTimeValue").text(ninetyDaysFormatTime(bar.data()[0].readDateTime));
                    d3.select("#NinetyDaysMaxValue").text(bar.data()[0].maxPM100.toFixed(0));
                    d3.select("#NinetyDaysMinValue").text(bar.data()[0].minPM100.toFixed(0));
                    d3.select("#NinetyDaysAvgValue").text(bar.data()[0].avgPM100.toFixed(1));
                }
            }
        });
});



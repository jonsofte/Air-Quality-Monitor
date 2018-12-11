// Oppsett
var weekly_inputData;
var weekly_latestDataValue;
var weekly_redValueLine = 60;
var weekly_yellowValueLine = 30;

var weekly_margin = { top: 15, right: 15, bottom: 20, left: 30 },
    weekly_width = parseInt(d3.select('#d3graphWeekly').style('width'));
    weekly_width = weekly_width - 20 - weekly_margin.left - weekly_margin.right;

if (weekly_width < 760) {
    weekly_height = (400 - weekly_margin.top - weekly_margin.bottom) * (weekly_width / 760);
}
else {
    weekly_height = 400 - weekly_margin.top - weekly_margin.bottom;
}

var weekly_parseTime = d3.timeParse("%Y-%m-%dT%H:%M:%S");
var weekly_formatTime = d3.timeFormat("%Y-%m-%d");

// Localization

var locale = d3.timeFormatLocale({
    "dateTime": "%Y-%m-%dT%H:%M:%S",
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
var weekly_x = d3.scaleTime().range([0, weekly_width]);
var weekly_y = d3.scaleLinear().range([weekly_height, 0]);
var weekly_xBand = d3.scaleBand().range([0, weekly_width]).padding(0.2);

// Definere PM2.5 linje
var weekly_valuelinePM025 = d3.line()
    .curve(d3.curveCatmullRom)
    .x(function (d) { return weekly_x(d3.timeMinute.offset(d.readDateTime, 30)); })
    .y(function (d) { return weekly_y(d.avgPM100); });

var weekly_svg = d3.select("#graphWeekly")
    .attr("width", weekly_width + weekly_margin.left + weekly_margin.right)
    .attr("height", weekly_height + weekly_margin.top + weekly_margin.bottom)
    .append("g")
    .attr("transform", "translate(" + weekly_margin.left + "," + weekly_margin.top + ")");

// Funksjon for X-akse
function weekly_make_x_gridlines() {
    return d3.axisBottom(weekly_x)
        .ticks(14)
        .tickFormat(multiFormat);
}

// Funksjon for Y-akse
function weekly_make_y_gridlines() {
    return d3.axisLeft(weekly_y).ticks(10)
}

// Hent og parse data
d3.json("/api/AirQualityHourLog", function (error, weekdata) {
    if (error) throw error;

    weekly_inputData = weekdata;

    weekly_inputData.forEach(function (d) {
        d.readDateTime = weekly_parseTime(d.readDateTime);
    });

    // Antall timer fra første til eldste gyldige verdi i array
    var weekly_numberOfBars = d3.timeHours(weekly_inputData[0].readDateTime, weekly_inputData[weekly_inputData.length - 1].readDateTime).length;
    var weekly_barWidth = weekly_width / weekly_numberOfBars;
    weekly_latestDataValue = weekly_inputData[weekly_inputData.length - 1]

    // Domene-verdier for mapping av input
    weekly_x.domain([d3.min(weekly_inputData, function (d) { return d.readDateTime; })
        , d3.max(weekly_inputData, function (d) { return d3.timeHour.offset(d.readDateTime, 1); })]);
    weekly_xBand.domain([d3.min(weekly_inputData, function (d) { return d.readDateTime; })
        , d3.extent(weekly_inputData, function (d) { return d3.timeHour.offset(d.readDateTime, 1); })]);
    weekly_y.domain([0, d3.max(weekly_inputData, function (d) { return d.maxPM100; }) * 1.05]);

    //Fargebånd i bakgrunn

    var weekly_yellowPos = weekly_y(weekly_redValueLine) < 0 ? 0 : weekly_y(weekly_redValueLine);
    var weekly_yellowHeight = weekly_y(weekly_yellowValueLine) - (weekly_y(weekly_redValueLine) < 0 ? 0 : weekly_y(weekly_redValueLine));
    if (weekly_yellowHeight < 0) weekly_yellowHeight = 0;
    var weekly_greenHeight = weekly_y(weekly_yellowValueLine) < 0 ? 0 : weekly_y(weekly_yellowValueLine);

    weekly_svg.append("svg:rect")
        .attr("class", "weekly bands red")
        .style("fill", "red")
        .style("opacity", "0.03")
        .attr('width', weekly_width)
        .attr('height', weekly_yellowPos);

    weekly_svg.append("svg:rect")
        .attr("class", "weekly bands yellow")
        .style("fill", "yellow")
        .style("opacity", "0.03")
        .attr('width', weekly_width)
        .attr('height', weekly_yellowHeight)
        .attr("transform", "translate(0," + weekly_yellowPos + ")");

    weekly_svg.append("svg:rect")
        .attr("class", "weekly bands green")
        .style("fill", "green")
        .style("opacity", "0.03")
        .attr('width', weekly_width)
        .attr('height', weekly_height - weekly_y(30))
        .attr("transform", "translate(0," + weekly_y(30) + ")");

    // X grid
    weekly_svg.append("g")
        .attr("class", "weekly gridx")
        .attr("transform", "translate(0," + weekly_height + ")")
        .call(weekly_make_x_gridlines()
            .tickSize(-weekly_height)
            .tickFormat("")
        )
        

    // Y grid
    weekly_svg.append("g")
        .attr("class", "weekly gridy")
        .call(weekly_make_y_gridlines()
            .tickSize(-weekly_width)
            .tickFormat("")
        )

    weekly_svg.selectAll(".bar")
        .data(weekly_inputData)
        .enter().append("rect")
        .attr("class", "weekly bar")
        .attr("x", function (d) { return weekly_x(d.readDateTime); })
        .attr("width", weekly_barWidth - 1)
        .attr("y", function (d) { return weekly_y(d.maxPM100); })
        .attr("height", function (d) { return (weekly_y(d.minPM100) - weekly_y(d.maxPM100)); });

    // Linje for PM2.5-verdier
    weekly_svg.append("path")
        .data([weekly_inputData])
        .attr("class", "weekly linePM025")
        .attr("d", weekly_valuelinePM025);

    // Legg til X-akse
    weekly_svg.append("g")
        .attr("class", "weekly x axis")
        .attr("transform", "translate(0," + weekly_height + ")")
        .call(weekly_make_x_gridlines())
        .style("font-family", "Montserrat");

    // Legg til Y-akse
    weekly_svg.append("g")
        .attr("class", "weekly y axis")
        .call(d3.axisLeft(weekly_y))
        .style("font-family", "Montserrat");

    var weekly_mouseG = weekly_svg.append("g")
        .attr("class", "mouse-over-effects");

    //  Vertikal linje for peker
    weekly_mouseG.append("path")
        .attr("class", "weekly mouse-line")
        .style("stroke", "black")
        .style("stroke-width", "1px")
        .style("opacity", "0.5");

    weekly_mouseG.append('svg:rect') // For håndtering av koordinater fra peker
        .attr('width', weekly_width - 1)
        .attr('height', weekly_height)
        .attr('fill', 'none')
        .attr('pointer-events', 'all')
        .on('mouseout', function () {
            d3.selectAll(".weekly.bar.selected").attr("class", "weekly bar");
            d3.select("#panelWeeklyStatDiv").attr("class", "hidden");
        })
        .on('mousemove', function () { // Flytt linje
            var weekly_mouse = d3.mouse(this);
            if (weekly_mouse[0] > 1) {
                d3.select("#panelWeeklyStatDiv").attr("class", "");
                d3.selectAll(".weekly.bar.selected").attr("class", "weekly bar");
                var weekly_bar = d3.selectAll(".weekly.bar").filter(function (d) { return (d.readDateTime <= weekly_x.invert(weekly_mouse[0]) && d3.timeHour.offset(d.readDateTime, 1) >= weekly_x.invert(weekly_mouse[0])) });
                weekly_bar.attr("class", "weekly bar selected");
                //console.log("Date:" + weekly_bar.data()[0].readDateTime + "Avg:" + weekly_bar.data()[0].avgPM25);
                // Bare oppdater verdier dersom det finnes element i array
                if (weekly_bar.data().length > 0) {
                    d3.select("#WeeklyTimeValue").text(formatTime(weekly_bar.data()[0].readDateTime));
                    d3.select("#WeeklyMaxValue").text(weekly_bar.data()[0].maxPM100.toFixed(0));
                    d3.select("#WeeklyMinValue").text(weekly_bar.data()[0].minPM100.toFixed(0));
                    d3.select("#WeeklyAvgValue").text(weekly_bar.data()[0].avgPM100.toFixed(1));
                }
            }
        });
});


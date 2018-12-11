//Skalere graf ved resize av skjerm
d3.select(window).on('resize', resize);

function resize() {

    weekly_width = parseInt(d3.select('#d3graphWeekly').style('width'));
    weekly_width = weekly_width - 20 - weekly_margin.left - weekly_margin.right;

    // Antall timer fra første til eldste gyldige verdi i array
    var weekly_numberOfBars = d3.timeHours(weekly_inputData[0].readDateTime, weekly_inputData[weekly_inputData.length - 1].readDateTime).length;
    var weekly_barWidth = weekly_width / weekly_numberOfBars;

    if (weekly_width < 760) {
        weekly_height = (400 - weekly_margin.top - weekly_margin.bottom) * (weekly_width / 760);
    }
    else {
        weekly_height = 400 - weekly_margin.top - weekly_margin.bottom;
    }

    d3.select("#graphWeekly").style("height", weekly_height + weekly_margin.top + weekly_margin.bottom);

    weekly_x = d3.scaleTime().range([0, weekly_width]);
    weekly_y = d3.scaleLinear().range([weekly_height, 0]);
    weekly_xBand = d3.scaleBand().range([0, weekly_width]).padding(0.2);

    // Domene-verdier for mapping av input
    weekly_x.domain([d3.min(weekly_inputData, function (d) { return d.readDateTime; })
        , d3.max(weekly_inputData, function (d) { return d3.timeHour.offset(d.readDateTime, 1); })]);
    weekly_xBand.domain([d3.min(weekly_inputData, function (d) { return d.readDateTime; })
        , d3.extent(weekly_inputData, function (d) { return d3.timeHour.offset(d.readDateTime, 1); })]);
    weekly_y.domain([0, d3.max(weekly_inputData, function (d) { return d.maxPM100; }) * 1.05]);

    d3.select("#graphWeekly").attr("width", weekly_width + weekly_margin.left + weekly_margin.right);

    weekly_yellowPos = weekly_y(weekly_redValueLine) < 0 ? 0 : weekly_y(weekly_redValueLine);
    weekly_yellowHeight = weekly_y(weekly_yellowValueLine) - (weekly_y(weekly_redValueLine) < 0 ? 0 : weekly_y(weekly_redValueLine));
    if (weekly_yellowHeight < 0) weekly_yellowHeight = 0;
    weekly_greenHeight = weekly_y(weekly_yellowValueLine) < 0 ? 0 : weekly_y(weekly_yellowValueLine);

    d3.select(".weekly.bands.red")
        .attr('height', weekly_yellowPos);
    d3.select(".weekly.bands.yellow")
        .attr('height', weekly_yellowHeight)
        .attr("transform", "translate(0," + weekly_yellowPos + ")");
    d3.select(".weekly.bands.green")
        .attr('height', weekly_height - weekly_greenHeight)
        .attr("transform", "translate(0," + weekly_greenHeight + ")");

    weekly_svg.select('.weekly.x.axis')
        .attr("transform", "translate(0," + weekly_height + ")")
        .call(d3.axisBottom(weekly_x)
            .ticks(14)
            .tickFormat(multiFormat));

    weekly_svg.select('.weekly.y.axis')
        .call(d3.axisLeft(weekly_y));

    weekly_svg.select('.weekly.gridy')
        .call(weekly_make_y_gridlines()
            .tickSize(-weekly_width)
            .tickFormat(""));

    weekly_svg.select('.weekly.gridx')
        .attr("transform", "translate(0," + weekly_height + ")")
        .call(weekly_make_x_gridlines()
            .tickSize(-weekly_height)
            .tickFormat(""));

    weekly_svg.select('.weekly.linePM025')
        .attr("d", weekly_valuelinePM025);

    weekly_svg.selectAll(".weekly.bar")
        .data(weekly_inputData)
        .attr("x", function (d) { return weekly_x(d.readDateTime); })
        .attr("width", weekly_barWidth - 1)
        .attr("y", function (d) { return weekly_y(d.maxPM100); })
        .attr("height", function (d) { return (weekly_y(d.minPM100) - weekly_y(d.maxPM100)); });

    weekly_svg.selectAll('.weekly.bands')
        .attr('width', weekly_width);

    weekly_svg.select('.mouse-over-effects > rect')
        .attr('width', weekly_width)
        .attr('height', weekly_height);

    width = parseInt(d3.select('#d3graphDaily').style('width'));
    width = width - 20 - margin.left - margin.right;

    if (width < 760) {
        height = (400 - margin.top - margin.bottom) * (width / 760);
    }
    else {
        height = 400 - margin.top - margin.bottom;
    }

    d3.select("#graphDaily").style("height", height + margin.top + margin.bottom);

    d3.select(".mouse-line")
        .style("opacity", "0");

    d3.select('#clip  > rect')
        .attr("width", (width))
        .attr("height", (height));

    x = d3.scaleTime().range([0, width]);
    y = d3.scaleLinear().range([height, 0]);

    x.domain(d3.extent(inputData, function (d) { return d.readDateTime; }));
    y.domain([0, d3.max(inputData, function (d) { return d.pM100; }) * 1.05]);

    d3.select("#graphDaily").attr("width", width + margin.left + margin.right);

    yellowPos = y(redValueLine) < 0 ? 0 : y(redValueLine);
    yellowHeight = y(yellowValueLine) - (y(redValueLine) < 0 ? 0 : y(redValueLine));
    if (yellowHeight < 0) yellowHeight = 0;
    greenHeight = y(yellowValueLine) < 0 ? 0 : y(yellowValueLine);

    d3.select(".bands.red")
        .attr('height', yellowPos);
    d3.select(".bands.yellow")
        .attr('height', yellowHeight)
        .attr("transform", "translate(0," + yellowPos + ")");
    d3.select(".bands.green")
        .attr('height', height - greenHeight)
        .attr("transform", "translate(0," + greenHeight + ")");

    svg.select('.x.axis')
        .attr("transform", "translate(0," + height + ")")
        .call(d3.axisBottom(x).tickFormat(d3.timeFormat("%H:%M")));

    svg.select('.y.axis')
        .call(d3.axisLeft(y));

    svg.select('.gridy')
        .call(make_y_gridlines()
            .tickSize(-width)
            .tickFormat(""));

    svg.select('.gridx')
        .attr("transform", "translate(0," + height + ")")
        .call(make_x_gridlines()
            .tickSize(-height)
            .tickFormat(""));

    svg.select('.linePM100')
        .attr("transform", null)
        .attr("d", valuelinePM100);

    svg.select('.linePM025')
        .attr("transform", null)
        .attr("d", valuelinePM025);

    svg.selectAll('.bands')
        .attr('width', width);

    svg.select('.mouse-over-effects > rect')
        .attr('width', width)
        .attr('height', height);

    ninetyDaysWidth = parseInt(d3.select('#d3graph90days').style('width'));
    ninetyDaysWidth = ninetyDaysWidth - 20 - ninetyDaysMargin.left - ninetyDaysMargin.right;

    // Antall dager fra første til eldste gyldige verdi i array
    var ninetyDaysnumberOfBars = d3.timeDays(ninetyDaysinputData[0].readDateTime, ninetyDaysinputData[ninetyDaysinputData.length - 1].readDateTime).length;
    var ninetyDaysBarWidth = ninetyDaysWidth / ninetyDaysnumberOfBars;

    if (ninetyDaysWidth < 760) {
        ninetyDaysHeight = (400 - ninetyDaysMargin.top - ninetyDaysMargin.bottom) * (ninetyDaysWidth / 760);
    }
    else {
        ninetyDaysHeight = 400 - ninetyDaysMargin.top - ninetyDaysMargin.bottom;
    }

    ninetyDaysX = d3.scaleTime().range([0, ninetyDaysWidth]);
    ninetyDaysY = d3.scaleLinear().range([ninetyDaysHeight, 0]);
    ninetyDaysXBand = d3.scaleBand().range([0, ninetyDaysWidth]).padding(0.2);

    // Domene-verdier for mapping av input
    ninetyDaysX.domain([d3.min(ninetyDaysinputData, function (d) { return d.readDateTime; })
        , d3.max(ninetyDaysinputData, function (d) { return d3.timeDay.offset(d.readDateTime, 1); })]);
    ninetyDaysXBand.domain([d3.min(ninetyDaysinputData, function (d) { return d.readDateTime; })
        , d3.extent(ninetyDaysinputData, function (d) { return d3.timeDay.offset(d.readDateTime, 1); })]);
    ninetyDaysY.domain([0, d3.max(ninetyDaysinputData, function (d) { return d.maxPM100; }) * 1.05]);

    d3.select("#graph90days").style("height", ninetyDaysHeight + ninetyDaysMargin.top + ninetyDaysMargin.bottom);
    d3.select("#graph90days").attr("width", ninetyDaysWidth + ninetyDaysMargin.left + ninetyDaysMargin.right);

    yellowPos = ninetyDaysY(ninetyDaysRedValueLine) < 0 ? 0 : ninetyDaysY(ninetyDaysRedValueLine);
    yellowHeight = ninetyDaysY(ninetyDaysYellowValueLine) - (ninetyDaysY(ninetyDaysRedValueLine) < 0 ? 0 : ninetyDaysY(ninetyDaysRedValueLine));
    if (yellowHeight < 0) _yellowHeight = 0;
    greenHeight = ninetyDaysY(ninetyDaysYellowValueLine) < 0 ? 0 : ninetyDaysY(ninetyDaysYellowValueLine);

    d3.select(".ninetydays.bands.red")
        .attr('height', yellowPos);
    d3.select(".ninetydays.bands.yellow")
        .attr('height', yellowHeight)
        .attr("transform", "translate(0," + yellowPos + ")");
    d3.select(".ninetydays.bands.green")
        .attr('height', ninetyDaysHeight - greenHeight)
        .attr("transform", "translate(0," + greenHeight + ")");

    ninetyDaysSvg.select('.ninetydays.x.axis')
        .attr("transform", "translate(0," + ninetyDaysHeight + ")")
        .call(d3.axisBottom(ninetyDaysX)
            .tickFormat(multiFormat));//.tickFormat(d3.timeFormat("%H:%M")));

    ninetyDaysSvg.select('.ninetydays.y.axis')
        .call(d3.axisLeft(ninetyDaysY));

    ninetyDaysSvg.select('.ninetydays.gridy')
        .call(ninetyDaysMake_y_gridlines()
            .tickSize(-ninetyDaysWidth)
            .tickFormat(""));

    ninetyDaysSvg.select('.ninetydays.gridx')
        .attr("transform", "translate(0," + ninetyDaysHeight + ")")
        .call(ninetyDaysMake_x_gridlines()
            .tickSize(-ninetyDaysHeight)
            .tickFormat(""));

    ninetyDaysSvg.select('.ninetydays.linePM025')
        .attr("d", ninetyDaysValuelinePM025);

    ninetyDaysSvg.selectAll(".ninetydays.bar")
        .data(ninetyDaysinputData)
        .attr("x", function (d) { return ninetyDaysX(d.readDateTime); })
        .attr("width", ninetyDaysBarWidth - 1)
        .attr("y", function (d) { return ninetyDaysY(d.maxPM100); })
        .attr("height", function (d) { return (ninetyDaysY(d.minPM100) - ninetyDaysY(d.maxPM100)); });

    ninetyDaysSvg.selectAll('.ninetydays.bands')
        .attr('width', ninetyDaysWidth);

    ninetyDaysSvg.select('.mouse-over-effects > rect')
        .attr('width', ninetyDaysWidth)
        .attr('height', ninetyDaysHeight);
};

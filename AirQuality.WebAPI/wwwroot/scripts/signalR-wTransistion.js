
var connection = new signalR.HubConnectionBuilder()
    .withUrl('/livedatastream')
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.start().catch(function (error) {
        console.error(error.message);
});  

connection.on("PointMeasurement", (values) => {

    width = parseInt(d3.select('#d3graphDaily').style('width'));
    width = width - 20 - margin.left - margin.right;

    if (width < 760) {
        height = (400 - margin.top - margin.bottom) * (width / 760);
    }
    else {
        height = 400 - margin.top - margin.bottom;
    }

    const encodedMsg = "Received values: " + values;
    //console.log(encodedMsg);

    inputData.push(parsedataPoint(JSON.parse(values)));

    latestDataValue = inputData[inputData.length - 1];
    d3.select("#panelPM025Value").text(latestDataValue.pM025);
    d3.select("#panelPM100Value").text(latestDataValue.pM100);
    d3.select("#panelLastUpdatedTime").text(formatTime(latestDataValue.readDateTime));
    d3.select("#panelLastValuesInfo").style("color", "orangered");

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
        //.attr("transform", null)
        .attr("d", valuelinePM100);

    svg.select('.linePM025')
        //.attr("transform", null)
        .attr("d", valuelinePM025);

    svg.selectAll('.bands')
        .attr('width', width);

    svg.select('.mouse-over-effects > rect')
        .attr('width', width)
        .attr('height', height);

    inputData.shift();

    d3.select("#panelLastValuesInfo").transition().duration(10000).style("color", "black");
    
});


var connection = new signalR.HubConnectionBuilder()
    .withUrl('/livedatastream')
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.start().catch(function (error) {
        console.error(error.message);
});  

connection.on("PointMeasurement", (values) => {

    const encodedMsg = "Received values: " + values;
    //console.log(encodedMsg);

    width = parseInt(d3.select('#d3graphDaily').style('width'));
    width = width - 20 - margin.left - margin.right;
    if (width < 760) {
        height = (400 - margin.top - margin.bottom) * (width / 760);
    }
    else {
        height = 400 - margin.top - margin.bottom;
    } 


    inputData.push(parsedataPoint(JSON.parse(values)));
    latestDataValue = inputData[inputData.length - 1];

    d3.select("#panelPM025Value").text(latestDataValue.pM025);
    d3.select("#panelPM100Value").text(latestDataValue.pM100);
    d3.select("#panelLastUpdatedTime").text(formatTime(latestDataValue.readDateTime));
    d3.select("#panelLastValuesInfo").style("color", "orangered");

    x.domain(d3.extent(inputData, function (d) { return d.readDateTime; }));
    y.domain([0, d3.max(inputData, function (d) { return d.pM100; }) * 1.05]);

    var t = d3.transition().duration(0);
    //.ease(d3.easeLinear);

    var xdiff = (x(inputData[inputData.length - 1].readDateTime)) - (x(inputData[inputData.length - 2].readDateTime));
    var newYValue = d3.max(inputData, function (d) { return d.pM100; });   
    var ydiff = 0;

    //var currentYMax = d3.max(inputData, function (d) { return d.pM100; });
    //if (newYValue > currentYMax) ydiff = y(newYValue) - y(currentYMax);
   
    d3.select('.linePM100')
        .attr("transform", null)
        .transition(t)
        .attr("d", valuelinePM100);
//        .attr("transform", "translate(" + -xdiff + "," + -ydiff + ")");

    d3.select('.linePM025')
        .attr("transform", null)
        .transition(t)
        .attr("d", valuelinePM025);
//        .attr("transform", "translate(" + -xdiff + "," + -ydiff + ")");

    yellowPos = y(redValueLine) < 0 ? 0 : y(redValueLine);
    yellowHeight = y(yellowValueLine) - (y(redValueLine) < 0 ? 0 : y(redValueLine));
    if (yellowHeight < 0) yellowHeight = 0;
    greenHeight = y(yellowValueLine) < 0 ? 0 : y(yellowValueLine);

    d3.select(".bands.red")
        .transition(t)
        .attr('height', yellowPos);
        
    d3.select(".bands.yellow")
        .transition(t)
        .attr('height', yellowHeight)
        .attr("transform", "translate(0," + yellowPos + ")");
        
    d3.select(".bands.green")
        .transition(t)
        .attr('height', height - greenHeight)
        .attr("transform", "translate(0," + greenHeight + ")");

    d3.select('.x.axis')
        .attr("transform", "translate(0," + height + ")")
        .transition(t)
        .call(d3.axisBottom(x).tickFormat(d3.timeFormat("%H:%M")));

    d3.select('.y.axis')
        .transition(t)
        .call(d3.axisLeft(y));

    d3.select('.gridy')
        .transition(t)
        .call(make_y_gridlines()
            .tickSize(-width)
            .tickFormat(""));

    d3.select('.gridx')
        .transition(t)
        .attr("transform", "translate(0," + height + ")")
        .call(make_x_gridlines()
            .tickSize(-height)
            .tickFormat(""));

    inputData.shift();

    d3.select("#panelLastValuesInfo").transition().duration(10000).style("color", "black");

    
});

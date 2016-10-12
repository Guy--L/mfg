var Chart = function (opts) {
    // load in arguments from config object
    this.data = opts.data;
    this.spec = opts.limits;
    this.element = opts.element;
    this.bins = opts.bins;
    this.width = opts.width;
    this.height = opts.height;
    this.color = "steelblue" //default color

    // create the chart
    this.draw();
}

Chart.prototype.draw = function () {
    // define width, height and paddings
    this.padding = 20;
    //this.width = this.element.offsetWidth - (2 * this.padding);
    //this.height = (this.width / 2) - (2 * this.padding);

    // set up parent element and SVG
    this.element.innerHTML = '';
    var svg = d3.select(this.element).append('g');
    svg.attr('width', this.width + (2 * this.padding));
    svg.attr('height', this.height + (2 * this.padding));

    // we'll actually be appending to a <g> element
    this.plot = svg.append('g')
        .attr("transform", "translate(" + this.padding + "," + this.padding + ")");

    // create the other stuff
    this.createXScale();
    this.generateHist(this.bins); //convert data to histogram layout
    this.createYScale(); //make the y scale based on that hist data.
    this.drawHist(); //actually draw the histogram
    this.addAxes(); //draw the axes
    //this.setColor(this.color) //color the chart.
}

Chart.prototype.createXScale = function () {
    // shorthand to save typing later
    var w = this.width,
        raw_data = this.data;

    var stt = calcMeanSdVar(raw_data);

    var extent = [stt.mean - 2 * stt.deviation, stt.mean + 2 * stt.deviation];

    this.safedata = raw_data.filter(function (d) { return d >= extent[0] && d <= extent[1]; });
    this.xScale = d3.scale.linear()
        .domain(extent)
        .range([0, w]);
}

Chart.prototype.generateHist = function () {

    // Generate a histogram w/ uniformly-spaced bins.
    this.hist_data = d3.layout.histogram()
        .bins(this.xScale.ticks(this.bins))
        (this.safedata);
}

Chart.prototype.createYScale = function () {
    this.yScale = d3.scale.linear()
        .domain([0, d3.max(this.hist_data, function (d) { return d.y; })])
        .range([this.height, 0]);
}

Chart.prototype.addAxes = function () {

    var h = this.height,
        w = this.width;

    var xAxis = d3.svg.axis()
        .scale(this.xScale)
        .orient("bottom");

    this.plot.append("g")
        .attr("class", "x axis")
        .attr("transform", "translate(0," + h + ")")
        .call(xAxis)
            .selectAll("text")
            .attr("transform", "translate( 0 ," + -4 + ")");
}

Chart.prototype.updateAxes = function () {

    var h = this.height,
        w = this.width;

    var xAxis = d3.svg.axis()
        .scale(this.xScale)
        .orient("bottom");

    this.plot.select(".x.axis").transition()
        .attr("transform", "translate(0," + h + ")")
        .call(xAxis)
            .selectAll("text")
            .attr("transform", "translate( 0 ," + -4 + ")");
}

Chart.prototype.drawHist = function () {
    //minumum observed data, needed for calculating bin width
    //var stdm = calcMeanSdVar(this.data);

    //this.min = stdm.mean - 2 * stdm.deviation;

    this.min = d3.min(this.safedata);
    if (typeof this.min === 'undefined')
        return;

    var h = this.height,
        w = this.width,
        x = this.xScale,
        y = this.yScale
    hist_data = this.hist_data,
    bin_width = x(this.min + hist_data[0].dx) - x(this.min);

    // A formatter for counts.
    var formatCount = d3.format(",.0f");

    var hist = this.plot.selectAll(".bar")
        .data(this.hist_data)

    hist.transition()
        .attr("transform", function (d) { return "translate(" + x(d.x) + "," + y(d.y) + ")"; })
        .each(function () {
            d3.select(this).select("rect") //update the bars
                .transition()
                .attr("x", 1)
                .attr("width", bin_width - 1)
                .attr("height", function (d) { return h - y(d.y); });
            // .attr("fill", this.color || "steelblue");

            d3.select(this).select("text") //update the text
                .transition()
                .attr("x", bin_width / 2)
                .attr("y", -14)
                .text(function (d) { return d.y != 0 ? formatCount(d.y) : ""; });
        });

    hist.enter().append("g")
        .attr("class", "bar")
        .attr("transform", function (d) { return "translate(" + x(d.x) + "," + y(d.y) + ")"; })
        .each(function () {
            var r = d3.select(this);
            r.append("rect") //draw the bars
                .attr("x", 1)
                .attr("width", bin_width - 1)
                .attr("height", function (d) { return h - y(d.y); });

            // .attr("fill", this.color || "steelblue");

            r.append("text") //draw the text
                .attr("dy", ".75em")
                .attr("y", -14)
                .attr("x", bin_width / 2)
                .attr("text-anchor", "middle")
                .attr("fill", "#fff;")
                .text(function (d) { return formatCount(d.y); });
        });

    hist.exit()
        .remove()

    this.plot.selectAll('.limitlinechart').remove();
    this.plot.selectAll('.limitlinechart')
        .data(this.spec).enter()
        .append('line')
        .attr('class', function (d) { return 'limitlinechart ' + d.cls; })
        //.attr('clip-path', 'url(#clip)')
        .attr('level', function (d) { return d.level; })
        .attr('x1', function (d) { return x(+d.level); })
        .attr('x2', function (d) { return x(+d.level); })
        .attr('y1', 0-this.padding*2)
        .attr('y2', h+this.padding*2);
}

// the following are "public methods"
// which can be used by code outside of this file

Chart.prototype.setColor = function (newColor) {

    this.plot.selectAll('rect')
        .style('fill', newColor);

    // store for use when redrawing
    this.color = newColor;
}

Chart.prototype.setData = function (newData, limit) {
    this.data = newData;
    this.spec = limit;

    this.createXScale();
    this.generateHist(this.bins);
    this.createYScale();
    this.updateAxes();
    this.drawHist();
   // this.setColor(this.color);
}

// quick statistical calculation for purposes of this chart

function calcMeanSdVar(values) {
    var r = { mean: 0, variance: 0, deviation: 0 }, t = values.length;
    for (var m, s = 0, l = t; l--; s += values[l]);
    for (m = r.mean = s / t, l = t, s = 0; l--; s += Math.pow(values[l] - m, 2));
    return r.deviation = Math.sqrt(r.variance = s / t), r;
}
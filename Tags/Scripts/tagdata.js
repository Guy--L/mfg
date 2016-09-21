
/// Chart (indata)
///   Name
///   exportName
///   Runs
///   Series
///     Name  
///     Tag
///     Specs
///         LoLo
///         Lo
///         Aim
///         Hi
///         HiHi
///     Data

function reDate(p) {
    p.t = new Date(p.t);
    return p;
}

function parseData(ind) {
    ind.mint = Number.MAX_VALUE;
    ind.maxt = Number.MIN_VALUE;
    ind.miny = Number.MAX_VALUE;
    ind.maxy = Number.MIN_VALUE;

    ind.series = ind.Series.map(function(d){
        var s = {};
        s.Name = d.Name;
        s.Tag = d.Tag;

        s.Specs = d.Specs.map(function(d){
            return d.map(reDate);
        });
        s.Data = d.Data.map(function (d) {
            ind.mint = (ind.mint > d.t) ? d.t : ind.mint;
            ind.maxt = (ind.maxt < d.t) ? d.t : ind.maxt;
            ind.miny = (ind.miny > d.y) ? d.y : ind.miny;
            ind.maxy = (ind.maxy < d.y) ? d.y : ind.maxy;

            d.t = new Date(d.t);
            return d;
        });
        return s;
    });
    return ind;
}

function tagdata(id, rawdata) {

    var margin = { top: 20, right: 20, bottom: 30, left: 40 },
    width = 960 - margin.left - margin.right,
    height = 500 - margin.top - margin.bottom;

    d3.select("div#"+id)
       .append("div")
       .classed("svg-container", true) //container class to make it responsive
       .append("svg")
       //responsive SVG needs these 2 attributes and no width and height attr
       .attr("preserveAspectRatio", "xMinYMin meet")
       .attr("viewBox", "0 0 " + width + " " + height)
       //class to make it responsive
       .classed("svg-content-responsive", true);
    
    data = parseData(rawdata);

    var x = d3.time.scale()
        .domain([data.mint, data.maxt])
        .range([0, width]);

    var y = d3.scale.linear()
        .domain([data.miny, data.maxy])
        .range([height, 0]);

    var xAxis = d3.svg.axis()
        .scale(x)
        .orient("bottom")
        .tickSize(-height);

    var yAxis = d3.svg.axis()
        .scale(y)
        .orient("left")
        .ticks(5)
        .tickSize(-width);

    var zoom = d3.behavior.zoom()
        .x(x)
        .y(y)
        .scaleExtent([1, 10])
        .on("zoom", zoomed);

    var svg = d3.select(id).append("svg")
        .attr("width", width + margin.left + margin.right)
        .attr("height", height + margin.top + margin.bottom)
      .append("g")
        .attr("transform", "translate(" + margin.left + "," + margin.top + ")")
        .call(zoom);

    svg.append("rect")
        .attr("width", width)
        .attr("height", height);

    svg.append("g")
        .attr("class", "x axis")
        .attr("transform", "translate(0," + height + ")")
        .call(xAxis);

    svg.append("g")
        .attr("class", "y axis")
        .call(yAxis);

    d3.select("button").on("click", reset);

    function zoomed() {
        svg.select(".x.axis").call(xAxis);
        svg.select(".y.axis").call(yAxis);
    }

    function reset() {
        d3.transition().duration(750).tween("zoom", function () {
            var ix = d3.interpolate(x.domain(), [data.mint, data.maxt]),
                iy = d3.interpolate(y.domain(), [data.miny, data.maxy]);
            return function (t) {
                zoom.x(x.domain(ix(t))).y(y.domain(iy(t)));
                zoomed();
            };
        });
    }

}
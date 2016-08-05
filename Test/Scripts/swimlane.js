function swim(id) {

    if ($(id).children().length > 0)
        d3.select(id).selectAll('svg').remove();
     
    xhub.server.runsEver().done(function (rets) {

        if (typeof rets === 'undefined')
            return;

        if (rets.length == 0)
            return;

        var details;
        var data = parseData(rets),
            items = data.items,
            lanes = data.lanes,
            now = new Date();

        var margin = { top: 20, right: 15, bottom: 15, left: 60 }
            , width = 960 - margin.left - margin.right
            , height = 500 - margin.top - margin.bottom
            , miniHeight = lanes.length * 12 + 50
            , mainHeight = height - miniHeight - 50;

        var x = d3.time.scale()
            .domain([d3.time.sunday(d3.min(items, function (d) { return d.begin; })),
                        d3.max(items, function (d) { return d.end; })])
            .range([0, width]);
        var x1 = d3.time.scale().range([0, width]);

        var ext = d3.extent(lanes, function (d) { return d.id; });
        var y1 = d3.scale.linear().domain([ext[0], ext[1] + 1]).range([0, mainHeight]);
        var y2 = d3.scale.linear().domain([ext[0], ext[1] + 1]).range([0, miniHeight]);

        //d3.select(id).remove();


        var chart = d3.select(id)
            .append('svg:svg')
            .attr('width', width + margin.right + margin.left)
            .attr('height', height + margin.top + margin.bottom)
            .attr('class', 'chart');

        chart.append('defs').append('clipPath')
            .attr('id', 'clip')
            .append('rect')
                .attr('width', width)
                .attr('height', mainHeight);

        var main = chart.append('g')
            .attr('transform', 'translate(' + margin.left + ',' + margin.top + ')')
            .attr('width', width)
            .attr('height', mainHeight)
            .attr('class', 'main');

        var mini = chart.append('g')
            .attr('transform', 'translate(' + margin.left + ',' + (mainHeight + 60) + ')')
            .attr('width', width)
            .attr('height', miniHeight)
            .attr('class', 'mini');

        // draw the lanes for the main chart
        main.append('g').selectAll('.laneLines')
            .data(lanes)
            .enter().append('line')
            .attr('x1', 0)
            .attr('y1', function (d) { return d3.round(y1(d.id)) + 0.5; })
            .attr('x2', width)
            .attr('y2', function (d) { return d3.round(y1(d.id)) + 0.5; })
            .attr('stroke', function (d) { return d.label === '' ? 'white' : 'lightgray' });

        main.append('g').selectAll('.laneText')
            .data(lanes)
            .enter().append('text')
            .text(function (d) { return d.label; })
            .attr('x', -10)
            .attr('y', function (d) { return y1(d.id + .5); })
            .attr('dy', '0.5ex')
            .attr('text-anchor', 'end')
            .attr('class', 'laneText');

        // draw the lanes for the mini chart
        mini.append('g').selectAll('.laneLines')
            .data(lanes)
            .enter().append('line')
            .attr('x1', 0)
            .attr('y1', function (d) { return d3.round(y2(d.id)) + 0.5; })
            .attr('x2', width)
            .attr('y2', function (d) { return d3.round(y2(d.id)) + 0.5; })
            .attr('stroke', function (d) { return d.label === '' ? 'white' : 'lightgray' });

        mini.append('g').selectAll('.laneText')
            .data(lanes)
            .enter().append('text')
            .text(function (d) { return d.label; })
            .attr('x', -10)
            .attr('y', function (d) { return y2(d.id + .5); })
            .attr('dy', '0.5ex')
            .attr('text-anchor', 'end')
            .attr('class', 'laneText');

        // draw the x axis
        var xDateAxis = d3.svg.axis()
            .scale(x)
            .orient('bottom')
            .ticks(d3.time.mondays, (x.domain()[1] - x.domain()[0]) > 15552e6 ? 2 : 1)
            .tickFormat(d3.time.format('%d'))
            .tickSize(6, 0, 0);

        var x1DateAxis = d3.svg.axis()
            .scale(x1)
            .orient('bottom')
            .ticks(d3.time.days, 1)
            .tickFormat(d3.time.format('%a %d'))
            .tickSize(6, 0, 0);

        var xMonthAxis = d3.svg.axis()
            .scale(x)
            .orient('top')
            .ticks(d3.time.months, 1)
            .tickFormat(d3.time.format('%b %Y'))
            .tickSize(15, 0, 0);

        var x1MonthAxis = d3.svg.axis()
            .scale(x1)
            .orient('top')
            .ticks(d3.time.mondays, 1)
            .tickFormat(d3.time.format('%b - Week %W'))
            .tickSize(15, 0, 0);

        main.append('g')
            .attr('transform', 'translate(0,' + mainHeight + ')')
            .attr('class', 'main axis date')
            .call(x1DateAxis);

        main.append('g')
            .attr('transform', 'translate(0,0.5)')
            .attr('class', 'main axis month')
            .call(x1MonthAxis)
            .selectAll('text')
                .attr('dx', 5)
                .attr('dy', 12);

        mini.append('g')
            .attr('transform', 'translate(0,' + miniHeight + ')')
            .attr('class', 'axis date')
            .call(xDateAxis);

        mini.append('g')
            .attr('transform', 'translate(0,0.5)')
            .attr('class', 'axis month')
            .call(xMonthAxis)
            .selectAll('text')
                .attr('dx', 5)
                .attr('dy', 12);

        // draw a line representing today's date
        main.append('line')
            .attr('y1', 0)
            .attr('y2', mainHeight)
            .attr('class', 'main todayLine')
            .attr('clip-path', 'url(#clip)');

        mini.append('line')
            .attr('x1', x(now) + 0.5)
            .attr('y1', 0)
            .attr('x2', x(now) + 0.5)
            .attr('y2', miniHeight)
            .attr('class', 'todayLine');

        // draw the items
        var itemRects = main.append('g')
            .attr('clip-path', 'url(#clip)');

        var offset = .5 * y2(1) + 0.5;

        mini.append('g').selectAll('miniItems')
            .data(items)
            .enter().append('line')
            .attr('class', function (d) { return 'miniItem ' + d.class; })
            .attr('id', function (d) { return 'miniItem_' + d.id; })
            .attr('x1', function (d) { return x(d.begin); })
            .attr('x2', function (d) { return x(d.end); })
            .attr('y1', function (d) { return y2(d.laneid) + offset; })
            .attr('y2', function (d) { return y2(d.laneid) + offset; });

        // invisible hit area to move around the selection window
        mini.append('rect')
            .attr('pointer-events', 'painted')
            .attr('width', width)
            .attr('height', miniHeight)
            .attr('visibility', 'hidden')
            .on('mouseup', moveBrush);

        // draw the selection area
        var brush = d3.svg.brush()
            .x(x)
            .extent([d3.time.monday(now), d3.time.saturday.ceil(now)])
            .on("brush", display);

        mini.append('g')
            .attr('class', 'x brush')
            .call(brush)
            .selectAll('rect')
                .attr('y', 1)
                .attr('height', miniHeight - 1);

        mini.selectAll('rect.background').remove();
        display();

        function display() {

            var rects, labels, lines
              , minExtent = d3.time.day(brush.extent()[0])
              , maxExtent = d3.time.day(brush.extent()[1])
              , visItems = items.filter(function (d) { return d.begin < maxExtent && d.end > minExtent });

            mini.select('.brush').call(brush.extent([minExtent, maxExtent]));
            x1.domain([minExtent, maxExtent]);

            // julian date would be %j after %a %d and %b %e
            if ((maxExtent - minExtent) > 1468800000) {
                x1DateAxis.ticks(d3.time.mondays, 1).tickFormat(d3.time.format('%a %d'))
                x1MonthAxis.ticks(d3.time.mondays, 1).tickFormat(d3.time.format('%b - Week %W'))
            }
            else if ((maxExtent - minExtent) > 172800000) {
                x1DateAxis.ticks(d3.time.days, 1).tickFormat(d3.time.format('%a %d'))
                x1MonthAxis.ticks(d3.time.mondays, 1).tickFormat(d3.time.format('%b - Week %W'))
            }
            else {
                x1DateAxis.ticks(d3.time.hours, 4).tickFormat(d3.time.format('%I %p'))
                x1MonthAxis.ticks(d3.time.days, 1).tickFormat(d3.time.format('%b %e'))
            }
            //x1Offset.range([0, x1(d3.time.day.ceil(now) - x1(d3.time.day.floor(now)))]);

            // shift the today line
            main.select('.main.todayLine')
                .attr('x1', x1(now) + 0.5)
                .attr('x2', x1(now) + 0.5);

            // update the axis
            main.select('.main.axis.date').call(x1DateAxis);
            main.select('.main.axis.month').call(x1MonthAxis)
                .selectAll('text')
                    .attr('dx', 5)
                    .attr('dy', 12);

            // update the item rects
            rects = itemRects.selectAll('g')
                .data(visItems, function (d) { return d.id; })
                .attr('transform', function (d) { return 'translate(0,' + (y1(d.laneid) + .1 * y1(1) + 0.5) + ')'; })

            rects.selectAll('rect')
                .attr('x', function (d) { return x1(d.begin); })
                .attr('width', function (d) { return x1(d.end) - x1(d.begin); });

            //rects.selectAll('line')
                //.attr()

            // add new item rects
            var newr = rects.enter().append('g')
                .attr('transform', function (d) { return 'translate(0,' + (y1(d.laneid) + .1 * y1(1) + 0.5) + ')'; });

            newr.append('rect')
                .attr('id', function (d) { return d.id; })
                .attr('x', function (d) { return x1(d.begin); })
                .attr('y', 0)
                .attr('width', function (d) { return x1(d.end) - x1(d.begin); })
                .attr('height', .8 * y1(1))
                .attr('class', function (d) { return 'mainItem ' + d.class; })
                .on('click', function (d) {
                    var r = d3.select(this);
                    r.classed('chosen', !r.classed('chosen'));
                    d.class = r.attr('class');
                    var s = mini.select('#miniItem_' + d.id);
                    s.classed('chosen', !s.classed('chosen'));
                    console.log('run begin: ' + d.begin + ' - ' + d.end);
                    rundetail(d, d3.select(this.parentNode));
                });

            // remove item rects out of window
            var delr = rects.exit();
            delr.selectAll('rect').remove();
            delr.remove();

            console.log('display completed');
        }

        var marker = ['belowspec', 'belowctl', 'inrange', 'abovectl', 'abovespec'];

        function rundetail(item, rungroup) {
            if (item.hasOwnProperty('details'))
                return;

            console.log('getting detail');

            xhub.server.runDetail(item.lane, item.start, item.stop).done(function (details) {
                for (var i = 0; i < details.length; i++) {
                    var seq = details[i].series;
                    if (!seq.length || seq.length < 1)
                        break;

                    seq[0].time0 = new Date(seq[0].epoch - (seq[1].epoch - seq[0].epoch));
                    seq[0].time1 = new Date(seq[0].epoch);

                    for (var j = 1; j < seq.length; j++) {
                        seq[j].time1 = new Date(seq[j].epoch);
                        seq[j].time0 = seq[j - 1].time1;
                    }
                }
                item.details = details;

                var trace = rungroup.append('g')
                    .attr('class', 'detail')

                var ysplit = d3.scale.ordinal()
                    .domain(d3.range(item.details.length))
                    .rangeRoundBands([0, .8 * y1(1)]);

                console.log(ysplit.rangeBand());
                console.log(ysplit(0) + ', ' + ysplit(1) + ', ' + ysplit(2));

                var detail = trace.selectAll('g')
                    .data(item.details).enter().append('g')
                    .attr('ysplit', function (d, i) { return ysplit(i);})
                    .attr('transform', function (d, i) { return 'translate(0,' + ysplit(i) + ')'; });

                detail.selectAll('line')
                    .data(function (d) { return d.series; })
                    .enter().append('line')
                    .attr('x1', function (d) { return x1(d.time0); })
                    .attr('y1', function (d, i, j) { return ysplit(j); })
                    .attr('x2', function (d) { return x1(d.time1); })
                    .attr('y2', function (d, i, j) { return ysplit(j); })
                    .attr('stroke-width', ysplit.rangeBand())
                    .attr('class', function (d) { return marker[d.control + 2]; });

                console.log('detail displayed');
            }).fail(function (msg) {
                console.log('rundetail fail');
                console.log(msg);
            });
        }

        function moveBrush() {
            var origin = d3.mouse(this)
              , point = x.invert(origin[0])
              , halfExtent = (brush.extent()[1].getTime() - brush.extent()[0].getTime()) / 2
              , begin = new Date(point.getTime() - halfExtent)
              , end = new Date(point.getTime() + halfExtent);

            brush.extent([begin, end]);
            display();
        }

        // generates a single path for each item class in the mini display
        // ugly - but draws mini 2x faster than append lines or line generator
        // is there a better way to do a bunch of lines as a single path with d3?
        function getPaths() {
            var paths = {}, d, offset = .5 * y2(1) + 0.5, result = [];
            for (var i = 0; i < items.length; i++) {
                d = items[i];
                if (!paths[d.class]) paths[d.class] = '';
                paths[d.class] += ['M', x(d.begin), (y2(d.laneid) + offset), 'H', x(d.end)].join(' ');
            }

            for (var className in paths) {
                result.push({ class: className, path: paths[className] });
            }

            return result;
        }
    });
}

function addToLane(chart, item) {
    var name = item.Lane;

    if (!chart.lanes[name])
        chart.lanes[name] = [];

    var lane = chart.lanes[name];

    var sublane = 0;
    //while (isOverlapping(item, lane[sublane]))
    //    sublane++;

    if (!lane[sublane]) {
        lane[sublane] = [];
    }

    lane[sublane].push(item);
};

function isOverlapping(item, lane) {
    if (lane) {
        for (var i = 0; i < lane.length; i++) {
            var t = lane[i];
            if (item.begin < t.end && item.end > t.begin) {
                return true;
            }
        }
    }
    return false;
};

function parseData(data) {
    var i = 0, length = data.length, node;
    chart = { lanes: {} };

    for (i; i < length; i++) {
        var item = data[i];
        item.begin = new Date(item.start);
        item.end = new Date(item.stop);
        addToLane(chart, item);
    }
    return collapseLanes(chart);
};

function collapseLanes(chart) {
    var lanes = [], items = [], laneId = 0;
    var now = new Date();

    for (var laneName in chart.lanes) {
        var lane = chart.lanes[laneName];

        for (var i = 0; i < lane.length; i++) {
            var subLane = lane[i];

            lanes.push({
                id: laneId,
                label: i === 0 ? laneName : ''
            });

            for (var j = 0; j < subLane.length; j++) {

                var item = subLane[j];

                items.push({
                    id: item.LineTxId,
                    laneid: laneId,
                    lane: laneName,
                    stamp: item.Stamp,
                    endstamp: item.EndStamp,
                    begin: item.begin,
                    end: item.end,
                    start: item.start,
                    stop: item.stop,
                    class: item.LineTxId == 0 ? 'lot' : (item.end > now ? 'future' : 'past'),
                    samples: item.samples,
                    productcode: item.ProductCode,
                    productspec: item.ProductSpec,
                    productid: item.ProductCodeId,
                    series: item.series
                });
            }

            laneId++;
        }
    }

    return { lanes: lanes, items: items };
}


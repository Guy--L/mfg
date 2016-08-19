function limitrender(limit) {
    return [
        { 'level': limit.LoLo, 'cls': 'outofspec' },
        { 'level': limit.Lo, 'cls': 'outofcontrol' },
        { 'level': limit.Aim, 'cls': 'target' },
        { 'level': limit.Hi, 'cls': 'outofcontrol' },
        { 'level': limit.HiHi, 'cls': 'outofspec' }
    ];
}

function alllines(id) {

    if ($(id).children().length > 0)
        d3.select(id).selectAll('svg').remove();
     
    xhub.server.runsAll().done(function (rets) {
        var gelcolors = ['#1b9e77', '#d95f02', '#7570b3', '#e7298a', '#66a61e', '#e6ab02', '#a6761d', '#666666'];
        var geltypes = 'ABCDEGJP'.split('');

        var gels = d3.scale.ordinal().domain(geltypes).range(gelcolors);

        if (typeof rets === 'undefined')
            return;

        if (rets.length == 0)
            return;

        var details;
        var data = parseData(rets),
            items = data.items,
            lanes = data.lanes,
            now = new Date(),
            focal = now;

        if (rets[rets.length - 1].LineTxId == 0) {
            focal = new Date(rets[rets.length - 1].start);
            console.log('focal set to ' + focal);
        }

        var margin = { top: 20, right: 15, bottom: 15, left: 60 }
            , width = 960 - margin.left - margin.right
            , height = 800 - margin.top - margin.bottom
            , allHeight = lanes.length * 20 + 50;

        var x = d3.time.scale()
            .domain([d3.time.sunday(d3.min(items, function (d) { return d.begin; })),
                        d3.max(items, function (d) { return d.end; })])
            .range([0, width]);

        var ext = d3.extent(lanes, function (d) { return d.id; });
        var y = d3.scale.linear().domain([ext[0], ext[1] + 1]).range([0, allHeight]);

        //d3.select(id).remove();

        var graph = d3.select(id)
            .append('svg:svg')
            .attr('width', width + margin.right + margin.left)
            .attr('height', height + margin.top + margin.bottom)
            .attr('class', 'chart');

        graph.append('defs').append('clipPath')
            .attr('id', 'clip')
            .append('rect')
                .attr('width', width)
                .attr('height', allHeight);

        var main = graph.append('g')
            .attr('transform', 'translate(' + margin.left + ',' + margin.top + ')')
            .attr('width', width)
            .attr('height', allHeight)
            .attr('class', 'main');

        // draw the lanes for the main graph
        main.append('g').selectAll('.laneLines')
            .data(lanes)
            .enter().append('line')
            .attr('x1', 0)
            .attr('y1', function (d) { return d3.round(y(d.id)) + 0.5; })
            .attr('x2', width)
            .attr('y2', function (d) { return d3.round(y(d.id)) + 0.5; })
            .attr('stroke', function (d) { return d.label === '' ? 'white' : 'lightgray' });

        main.append('g').selectAll('.laneText')
            .data(lanes)
            .enter().append('text')
            .text(function (d) { return d.label; })
            .attr('x', -10)
            .attr('y', function (d) { return y(d.id + .5); })
            .attr('dy', '0.5ex')
            .attr('text-anchor', 'end')
            .attr('class', 'laneText');

        var xDateAxis = d3.svg.axis()
            .scale(x)
            .orient('bottom')
            .ticks(d3.time.years, 1)
            .tickFormat(d3.time.format('%Y'))
            .tickSize(6, 0, 0);

        var xMonthAxis = d3.svg.axis()
            .scale(x)
            .orient('top')
            .ticks(d3.time.months, 1)
            .tickFormat(d3.time.format('%b'))
            .tickSize(15, 0, 0);

        main.append('g')
            .attr('transform', 'translate(0,' + allHeight + ')')
            .attr('class', 'main axis date')
            .call(xDateAxis);

        main.append('g')
            .attr('transform', 'translate(0,0.5)')
            .attr('class', 'main axis month')
            .call(xMonthAxis)
            .selectAll('text')
                .attr('dx', 5)
                .attr('dy', 12);

        var div = d3.select(id).append('div')
            .attr('class', 'runtip')
            .style('opacity', 0);

        var loading = graph
            .append('text')
            .text('loading...')
            .attr('x', width / 2)
            .attr('y', height / 2)
            .attr('text-anchor', 'middle')
            .attr('dominant-baseline', 'central')
            .attr('font-size', 26)
            .style('opacity', 0);

        var offset = .5 * y(1) + 0.5;

        function drill(d) {
            var r = d3.select(this);
            main.selectAll('.allItem')
                .on('click', null)
                .on('mouseover', null)
                .on('mouseout', null);

            //r.classed('chosen', !r.classed('chosen'));
            r.classed('chosen', true);
            d.class = r.attr('class');
            div.transition()
                .duration(500)
                .style("opacity", 0);
            loading
                .style('opacity', .9)
                .attr('x', x(d.begin))
                .attr('y', y(d.laneid));
            rundetail(d, d3.select(this.parentNode));
        }

        var formatrTime = d3.time.format("%y/%m/%d %H:%M");
        var formatsTime = d3.time.format("%m/%d %H:%M");

        function runtip(d) {
            var hi = d3.select(this);
            var hiColor = d3.rgb(gels(d.geltype)).brighter(2);
            var text = formatrTime(d.begin)
                + " - " + formatsTime(d.end) + '<br />(' + d.id + ')<br />'
                + d.productcode + ' ' + d.productspec + '<br />' + (d.comment==null?'':d.comment);
            hi.style('stroke', hiColor);
            var hi2Color = d3.rgb(hiColor).brighter(2);
            div.transition()
                .duration(200)
                .style('opacity', .9)
                .style('background', hi2Color)
                .style('border-color', hiColor);
            div.html(text)
                .style("left", x(d.begin) + margin.left + 50 + "px")
                .style("top", (d3.event.pageY - 70) - margin.top + "px");
        }

        function unruntip(d) {
            var hi = d3.select(this);
            hi.style('stroke', gels(d.geltype));
            div.transition()
                .duration(500)
                .style("opacity", 0);
        }

        main.append('g').selectAll('.allItem')
            .data(items)
            .enter().append('line')
            .attr('class', 'allItem')
            .attr('id', function (d) { return 'allItem_' + d.id; })
            .attr('x1', function (d) { return x(d.begin); })
            .attr('x2', function (d) { return x(d.end); })
            .attr('y1', function (d) { return y(d.laneid) + offset; })
            .attr('y2', function (d) { return y(d.laneid) + offset; })
            .style('stroke', function (d) { return gels(d.geltype); })
            .style('stroke-width', function (d) { return d.productwidth / 2; })
            .attr('title', function (d) { return d.productcode + ' ' + d.productspec })
            .on('click', drill)
            .on("mouseover", runtip)
            .on("mouseout", unruntip);

        main.append('text')
            .attr('x', -25)
            .attr('id', 'topexit')
            .attr('class', 'reicon')
            .attr('text-anchor', 'middle')
            .attr('dominant-baseline', 'central')
            .text('\uf057')
            .on('click', function () {
                graph.remove();
                $(document).trigger('historyexit');
            });

        $(document).on('detailexit', function (e) {
            main.attr('opacity', 1);
            d3.select('#clip').select('rect').attr('height', allHeight);
            main.selectAll('.allItem')
                .on('click', drill)
                .on('mouseover', runtip)
                .on('mouseout', unruntip);
        });

        function detailenter(item) {
            main.attr('opacity', 0);
            specview(item);
            $('li.download').show();
        }

        function rundetail(item, rungroup) {
            if (!item.hasOwnProperty('details')) {
                xhub.server.runDetail(item.productcode, item.productspec, item.lane, item.start, item.stop).done(function (details) {
                    item.details = details; 

                    detailenter(item);

                    loading.style('opacity', 0);
                }).fail(function (msg) {
                    console.log('rundetail fail');
                    console.log(msg);
                    main.selectAll('.allItem')
                        .on('click', drill)
                        .on('mouseover', runtip)
                        .on('mouseout', unruntip);
                    loading.text('failed to load')
                        .attr('stroke', 'red');
                });
            } else {
                detailenter(item);
                loading.style('opacity', 0);
            }
        }


        function limitextent(limit) {
            var bottom = limit.Aim - limit.LoLo;
            bottom = limit.Aim - 1.2 * bottom;
            var top = limit.HiHi - limit.Aim;
            top = limit.Aim + 1.2 * top;
            return [top, bottom];
        }

        function specview(data) {

            $('#Context_Product').val(data.productcode + ' ' + data.productspec);
            var oldtitle = $('#linestitle').html();
            $('#linestitle').html('<h3>Detail of ' + data.lane + ' between ' + data.stamp.replace('T', ' ') + ' and ' + data.endstamp.replace('T', ' ') + '</h3>');

            var channel = 0;
            var lanes = data.details;
            var bottomHeight = lanes.length * 6 + 50;
            var topHeight = height - bottomHeight - 50 - 300;
            var statHeight = height - bottomHeight - topHeight - 100;
           
            var xb = d3.time.scale()
                .domain([data.begin, data.end])
                .range([0, width]);
            var xt = d3.time.scale()
                .range([0, width]);

            var ext = d3.extent(lanes, function (d) { return d.id; });
            var yb = d3.scale.linear().domain([ext[0], ext[1] + 1]).range([0, bottomHeight]);

            var yt;
            var ytTagAxis;

            var top = graph.append('g')
                .attr('transform', 'translate(' + margin.left + ',' + margin.top + ')')
                .attr('width', width)
                .attr('height', topHeight)
                .attr('class', 'top');

            var bottom = graph.append('g')
                .attr('transform', 'translate(' + margin.left + ',' + (topHeight + 60) + ')')
                .attr('width', width)
                .attr('height', bottomHeight)
                .attr('class', 'bottom');

            var stat = graph.append('g')
                .attr('transform', 'translate(' + margin.left + ',' + (topHeight + bottomHeight + 100) + ')')
                .attr('width', width)
                .attr('height', statHeight)
                .attr('class', 'stat');

            // draw the lanes for the mini graph
            bottom.append('g').selectAll('.laneLines')
                .data(lanes)
                .enter().append('line')
                .attr('x1', 0)
                .attr('y1', function (d) { return d3.round(yb(d.id)) + 0.5; })
                .attr('x2', width)
                .attr('y2', function (d) { return d3.round(yb(d.id)) + 0.5; })
                .attr('stroke', function (d) { return d.label === '' ? 'white' : 'lightgray' });

            bottom.append('g').selectAll('.laneText')
                .data(lanes)
                .enter().append('text')
                .text(function (d) { return d.Label; })
                .attr('x', -10)
                .attr('y', function (d) { return yb(d.id + .5); })
                .attr('dy', '0.5ex')
                .attr('text-anchor', 'end')
                .attr('class', function (d, i) { return 'laneText' + (i == 0 ? ' chosen' : ''); })
                .on('click', function (d, i) {
                    var r = d3.select(this);
                    var chosen = r.classed('chosen');
                    if (chosen) return;

                    d3.select(this.parentNode).selectAll('.laneText').classed('chosen', false);
                    r.classed('chosen', true);

                    channel = i;
                    switched();
                });

            // draw the x axis
            var xbDateAxis = d3.svg.axis()
                .scale(xb)
                .orient('bottom')
                .ticks(d3.time.mondays, (xb.domain()[1] - xb.domain()[0]) > 15552e6 ? 2 : 1)
                .tickFormat(d3.time.format('%d'))
                .tickSize(6, 0, 0);

            var xtDateAxis = d3.svg.axis()
                .scale(xt)
                .orient('bottom')
                .ticks(d3.time.days, 1)
                .tickFormat(d3.time.format('%a %d'))
                .tickSize(6, 0, 0);

            var xbMonthAxis = d3.svg.axis()
                .scale(xb)
                .orient('top')
                .ticks(d3.time.months, 1)
                .tickFormat(d3.time.format('%b %Y'))
                .tickSize(15, 0, 0);

            var xtMonthAxis = d3.svg.axis()
                .scale(xt)
                .orient('top')
                .ticks(d3.time.mondays, 1)
                .tickFormat(d3.time.format('%b - Week %W'))
                .tickSize(15, 0, 0);

            top.append('g')
                .attr('transform', 'translate(0,' + topHeight + ')')
                .attr('class', 'main axis date')
                .call(xtDateAxis);

            d3.select('#clip').select('rect').attr('height', topHeight);
            var topClip = top.append('g')
                .attr('clip-path', 'url(#clip)');

            var dataline = d3.svg.line()
                .x(function (d) { return xt(new Date(d.epoch)); })
                .y(function (d) { return yt(+d.dvalue); });

            var chart = new Chart({
                element: document.querySelector('.stat'),
                data: lanes[channel].series.map(function (d) { return d.dvalue; }),
                limits: limitrender(lanes[channel].limit),
                bins: 20,
                width: width,
                height: statHeight
            });

            switched();
            top.append('g')
                .attr('transform', 'translate(-1.5,0)')
                .attr('class', 'main axis value')
                .call(ytTagAxis);

            top.append('g')
                .attr('transform', 'translate(0,0.5)')
                .attr('class', 'main axis month')
                .call(xtMonthAxis)
                .selectAll('text')
                    .attr('dx', 5)
                    .attr('dy', 12);

            top.append('text')
                .attr('id', 'exit')
                .attr('x', -25)
                .attr('class', 'reicon')
                .attr('text-anchor', 'middle')
                .attr('dominant-baseline', 'central')
                .text(function (d) { return '\uf057'; })
                .on('click', function () {
                    bottom.remove();
                    top.remove();
                    stat.remove();
                    $('#linestitle').html(oldtitle);
                    $(document).trigger('detailexit');
                });

            bottom.append('g')
                .attr('transform', 'translate(0,' + bottomHeight + ')')
                .attr('class', 'axis date')
                .call(xbDateAxis);

            bottom.append('g')
                .attr('transform', 'translate(0,0.5)')
                .attr('class', 'axis month')
                .call(xbMonthAxis)
                .selectAll('text')
                    .attr('dx', 5)
                    .attr('dy', 12);

            var trace = bottom.selectAll('.trace')
                .data(lanes)
              .enter().append('g')
                .attr('transform', function (d, i) { return 'translate(0,' + (yb(i) + 0.5 * yb(1)) + ')'; })
                .attr('class', 'trace');

            var tracepaint = ['outofspec', 'outofcontrol', 'target', 'outofcontrol', 'outofspec'];

            trace.selectAll('brun')
                .data(function (d) { return d.series; })
              .enter().append('line')
                .attr('class', function (d) { return 'brun ' + tracepaint[d.ctrl + 2]; })
                .attr('id', function (d) { return 'brun_' + d.prdid; })
                .attr('x1', function (d, i, j) {
                    return xb(i == 0 ? new Date(lanes[j].series[0].epoch) : new Date(lanes[j].series[i - 1].epoch));
                })
                .attr('x2', function (d) { return xb(new Date(d.epoch)); })
                .attr('y1', 0)
                .attr('y2', 0);

            // invisible hit area to move around the selection window
            bottom.append('rect')
                .attr('pointer-events', 'painted')
                .attr('width', width)
                .attr('height', bottomHeight)
                .attr('visibility', 'hidden')
                .on('mouseup', moveBrushd);

            // draw the selection area
            var brushd = d3.svg.brush()
                .x(xb)
                .extent([data.begin, data.end])
                .on('brush', brushed);

            bottom.append('g')
                .attr('class', 'x brushd')
                .call(brushd)
                .selectAll('rect')
                    .attr('y', 1)
                    .attr('height', bottomHeight - 1);

            bottom.selectAll('rect.background').remove();

            $('#getdetail').on('click', function (e) {
                $('input#Product').val(data.productcode + ' ' + data.productspec);
                $('input#Start').val(brushd.extent()[0].getTime());
                $('input#End').val(brushd.extent()[1].getTime());
            });

            brushed();

            function switched() {
                var limit = lanes[channel].limit;

                yt = d3.scale.linear()
                    .domain(limitextent(limit))
                    .range([0, topHeight]);

                ytTagAxis = d3.svg.axis()
                    .scale(yt)
                    .orient('left')

                top.select('.main.axis.value').call(ytTagAxis);

                top.selectAll('.dataline').remove();
                top.append('path')
                    .datum(lanes[channel].series)
                    .attr('d', dataline)
                    .attr('class', 'dataline')
                    .attr('clip-path', 'url(#clip)');

                //topClip.selectAll('.dot').remove();

                //topClip.selectAll('.dot')
                //     .data(lanes[channel].series, function (d) { return d.prdid; })
                //   .enter().append('circle')
                //     .attr('class', 'dot')
                //     .attr('r', 1)
                //     .attr('cx', function (d) { return xt(new Date(d.epoch)); })
                //     .attr('cy', function (d) { return yt(+d.dvalue); });

                top.selectAll('.limitline').remove();
                top.selectAll('.limitline')
                    .data(limitrender(limit)).enter()
                    .append('line')
                    .attr('class', function (d) { return 'limitline ' + d.cls; })
                    .attr('clip-path', 'url(#clip)')
                    .attr('level', function (d) { return d.level; })
                    .attr('x1', 0)
                    .attr('x2', width)
                    .attr('y1', function (d) { return yt(+d.level); })
                    .attr('y2', function (d) { return yt(+d.level); });

                chart.setData(lanes[channel].series.map(function (d) { return d.dvalue; }), limitrender(lanes[channel].limit));
            }

            function brushed() {
                xt.domain(brushd.empty() ? xb.domain() : brushd.extent());

                top.selectAll('.dataline').attr('d', dataline);

                //topClip.selectAll('.dot')
                //     .attr('cx', function (d) { return xt(new Date(d.epoch)); })
                //     .attr('cy', function (d) { return yt(+d.dvalue); });

                var
                    minExtent = brushd.extent()[0]
                  , maxExtent = brushd.extent()[1];

                bottom.select('.brushd').call(brushd.extent([minExtent, maxExtent]));

                // julian date would be %j after %a %d and %b %e
                if ((maxExtent - minExtent) > 1468800000) {
                    xtDateAxis.ticks(d3.time.mondays, 1).tickFormat(d3.time.format('%a %d'))
                    xtMonthAxis.ticks(d3.time.mondays, 1).tickFormat(d3.time.format('%b - Week %W'))
                }
                else if ((maxExtent - minExtent) > 172800000) {
                    xtDateAxis.ticks(d3.time.days, 1).tickFormat(d3.time.format('%a %d'))
                    xtMonthAxis.ticks(d3.time.mondays, 1).tickFormat(d3.time.format('%b - Week %W'))
                }
                else {
                    xtDateAxis.ticks(d3.time.hours, 4).tickFormat(d3.time.format('%I %p'))
                    xtMonthAxis.ticks(d3.time.days, 1).tickFormat(d3.time.format('%b %e'))
                }
                top.select('.main.axis.date').call(xtDateAxis);
                top.select('.main.axis.month').call(xtMonthAxis)
                    .selectAll('text')
                        .attr('dx', 5)
                        .attr('dy', 12);
            }

            function moveBrushd() {
                var origin = d3.mouse(this)
                  , point = xb.invert(origin[0])
                  , halfExtent = (brushd.extent()[1].getTime() - brushd.extent()[0].getTime()) / 2
                  , begin = new Date(point.getTime() - halfExtent)
                  , end = new Date(point.getTime() + halfExtent);

                brushd.extent([begin, end]);
                brushed();
            }
        }

        $('#linesloading').hide();
    }).fail(function (e) {
        $('#linesloading').val('Failed to load history');
        console.log('fail');
        console.log(e);
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
                    productwidth: item.ProductWidth,
                    geltype: item.GelType,
                    series: item.series,
                    comment: item.Comment,
                    message: false
                });
            }

            laneId++;
        }
    }

    return { lanes: lanes, items: items };
}



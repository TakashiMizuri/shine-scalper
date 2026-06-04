import { useEffect, useRef } from 'react';
import {
  CandlestickSeries,
  ColorType,
  createChart,
  createSeriesMarkers,
  LineSeries,
  type IChartApi,
  type ISeriesApi,
  type CandlestickData,
  type Time,
} from 'lightweight-charts';
import type { CandleDto, FillDto, LevelDto } from '../types';

interface Props {
  candles: CandleDto[];
  levels: LevelDto[];
  trades: FillDto[];
  interval: string;
  liveCandle?: CandleDto | null;
}

export function ChartPanel({ candles, levels, trades, interval, liveCandle }: Props) {
  const containerRef = useRef<HTMLDivElement>(null);
  const chartRef = useRef<IChartApi | null>(null);
  const seriesRef = useRef<ISeriesApi<'Candlestick'> | null>(null);
  const zoneLinesRef = useRef<ISeriesApi<'Line'>[]>([]);

  useEffect(() => {
    if (!containerRef.current) return;

    const chart = createChart(containerRef.current, {
      layout: {
        background: { type: ColorType.Solid, color: '#09090b' },
        textColor: '#a1a1aa',
      },
      grid: {
        vertLines: { color: '#27272a' },
        horzLines: { color: '#27272a' },
      },
      width: containerRef.current.clientWidth,
      height: 480,
    });

    const series = chart.addSeries(CandlestickSeries, {
      upColor: '#22c55e',
      downColor: '#ef4444',
      borderVisible: false,
      wickUpColor: '#22c55e',
      wickDownColor: '#ef4444',
    });

    chartRef.current = chart;
    seriesRef.current = series;

    const onResize = () => {
      if (containerRef.current)
        chart.applyOptions({ width: containerRef.current.clientWidth });
    };
    window.addEventListener('resize', onResize);

    return () => {
      window.removeEventListener('resize', onResize);
      chart.remove();
      chartRef.current = null;
      seriesRef.current = null;
      zoneLinesRef.current = [];
    };
  }, []);

  useEffect(() => {
    const series = seriesRef.current;
    const chart = chartRef.current;
    if (!series || !chart) return;

    const toBar = (c: CandleDto): CandlestickData<Time> => ({
      time: c.time as Time,
      open: c.open,
      high: c.high,
      low: c.low,
      close: c.close,
    });

    const data: CandlestickData<Time>[] = candles.map(toBar);
    if (liveCandle && liveCandle.interval === interval) {
      const t = liveCandle.time as Time;
      const idx = data.findIndex((d) => d.time === t);
      const point = toBar(liveCandle);
      if (idx >= 0) data[idx] = point;
      else data.push(point);
    }

    data.sort((a, b) => (a.time as number) - (b.time as number));
    series.setData(data);

    zoneLinesRef.current.forEach((l) => chart.removeSeries(l));
    zoneLinesRef.current = [];

    levels
      .filter((l) => !l.invalidated)
      .forEach((level) => {
        const color = level.type === 'Resistance' ? '#f97316' : '#3b82f6';
        for (const price of [level.lowerBound, level.upperBound, level.center]) {
          const line = chart.addSeries(LineSeries, {
            color,
            lineWidth: 1,
            lineStyle: price === level.center ? 2 : 0,
            priceLineVisible: false,
            lastValueVisible: false,
          });
          const start = data[0]?.time ?? (liveCandle?.time as Time);
          const end = data[data.length - 1]?.time ?? start;
          line.setData([
            { time: start, value: price },
            { time: end, value: price },
          ]);
          zoneLinesRef.current.push(line);
        }
      });

    const markers = trades.map((t) => ({
      time: (Math.floor(new Date(t.timestampUtc).getTime() / 1000)) as Time,
      position: t.isEntry
        ? ('belowBar' as const)
        : ('aboveBar' as const),
      color: t.side === 'Long' ? '#22c55e' : '#ef4444',
      shape: t.isEntry ? ('arrowUp' as const) : ('arrowDown' as const),
      text: t.isEntry ? 'IN' : 'OUT',
    }));

    createSeriesMarkers(series, markers);
  }, [candles, levels, trades, interval, liveCandle]);

  return (
    <div className="rounded-lg border border-zinc-800 bg-zinc-900/50 p-2">
      <div ref={containerRef} className="w-full" />
    </div>
  );
}

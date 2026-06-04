import { useCallback, useEffect, useState } from 'react';
import { fetchCandles, fetchHealth, fetchLevels, fetchPositions, fetchTrades } from './api';
import { ChartPanel } from './components/ChartPanel';
import { useTradingHub } from './hooks/useTradingHub';
import type { CandleDto, FillDto, HealthDto, LevelDto, PositionDto } from './types';

export default function App() {
  const [health, setHealth] = useState<HealthDto | null>(null);
  const [timeframe, setTimeframe] = useState('5');
  const [candles, setCandles] = useState<CandleDto[]>([]);
  const [levels, setLevels] = useState<LevelDto[]>([]);
  const [openPositions, setOpenPositions] = useState<PositionDto[]>([]);
  const [recentPositions, setRecentPositions] = useState<PositionDto[]>([]);
  const [trades, setTrades] = useState<FillDto[]>([]);

  const hub = useTradingHub();

  const refresh = useCallback(async () => {
    const [h, c, l, p, t] = await Promise.all([
      fetchHealth(),
      fetchCandles(timeframe),
      fetchLevels(),
      fetchPositions(),
      fetchTrades(),
    ]);
    setHealth(h);
    setCandles(c);
    setLevels(l);
    setOpenPositions(p.open);
    setRecentPositions(p.recent);
    setTrades(t);
  }, [timeframe]);

  useEffect(() => {
    void refresh();
    const id = window.setInterval(() => void refresh(), 30_000);
    return () => clearInterval(id);
  }, [refresh]);

  useEffect(() => {
    if (hub.levels.length) setLevels(hub.levels);
  }, [hub.levels]);

  const displayLevels = hub.levels.length ? hub.levels : levels;
  const lastPrice = hub.lastPrice || health?.lastPrice || 0;

  return (
    <div className="min-h-screen p-4 md:p-6 max-w-[1600px] mx-auto">
      <header className="flex flex-wrap items-center justify-between gap-4 mb-6">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Shine Scalper</h1>
          <p className="text-zinc-400 text-sm">
            {health?.symbol ?? 'SOLUSDT'} · {health?.tradingMode ?? 'Paper'} mode
          </p>
        </div>
        <div className="flex items-center gap-3 text-sm">
          <span
            className={`px-2 py-1 rounded ${hub.connected ? 'bg-emerald-900/50 text-emerald-300' : 'bg-red-900/50 text-red-300'}`}
          >
            SignalR {hub.connected ? 'on' : 'off'}
          </span>
          <span
            className={`px-2 py-1 rounded ${health?.marketDataConnected ? 'bg-emerald-900/50 text-emerald-300' : 'bg-amber-900/50 text-amber-300'}`}
          >
            Bybit {health?.marketDataConnected ? 'on' : 'off'}
          </span>
          <span className="text-zinc-300 font-mono">
            {lastPrice > 0 ? lastPrice.toFixed(4) : '—'}
          </span>
          <select
            value={timeframe}
            onChange={(e) => setTimeframe(e.target.value)}
            className="bg-zinc-800 border border-zinc-700 rounded px-2 py-1"
          >
            <option value="5">5m</option>
            <option value="1">1m</option>
          </select>
        </div>
      </header>

      <div className="grid grid-cols-1 xl:grid-cols-4 gap-4">
        <div className="xl:col-span-3">
          <ChartPanel
            candles={candles}
            levels={displayLevels}
            trades={[...hub.trades, ...trades]}
            interval={timeframe}
            liveCandle={hub.candlePatch?.interval === timeframe ? hub.candlePatch : null}
          />
        </div>

        <div className="space-y-4">
          <Panel title="Open positions">
            {openPositions.length === 0 && hub.positions.filter((p) => p.status === 'Open').length === 0 ? (
              <p className="text-zinc-500 text-sm">No open positions</p>
            ) : (
              <ul className="space-y-2 text-sm">
                {(hub.positions.filter((p) => p.status === 'Open').length
                  ? hub.positions.filter((p) => p.status === 'Open')
                  : openPositions
                ).map((p) => (
                  <li key={p.id} className="border border-zinc-800 rounded p-2">
                    <div className="font-medium">
                      {p.side} @ {p.entryPrice.toFixed(4)}
                    </div>
                    <div className="text-zinc-400">
                      SL {p.stopLoss.toFixed(4)} · qty {p.remainingQuantity.toFixed(4)}
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </Panel>

          <Panel title="Levels">
            <ul className="space-y-1 text-xs max-h-48 overflow-auto">
              {displayLevels.map((l) => (
                <li key={l.id} className="text-zinc-400">
                  <span className={l.type === 'Resistance' ? 'text-orange-400' : 'text-blue-400'}>
                    {l.type}
                  </span>{' '}
                  {l.lowerBound.toFixed(2)} – {l.upperBound.toFixed(2)} ({l.touches})
                </li>
              ))}
            </ul>
          </Panel>

          <Panel title="Recent trades">
            <ul className="space-y-1 text-xs max-h-40 overflow-auto">
              {[...hub.trades, ...trades].slice(0, 15).map((t) => (
                <li key={t.id}>
                  {t.isEntry ? 'ENTRY' : 'EXIT'} {t.side} {t.price.toFixed(4)} x{t.quantity.toFixed(3)}
                </li>
              ))}
            </ul>
          </Panel>

          <Panel title="History">
            <ul className="space-y-1 text-xs max-h-32 overflow-auto">
              {recentPositions.slice(0, 8).map((p) => (
                <li key={p.id} className="text-zinc-500">
                  {p.status} {p.side} PnL {p.realizedPnL.toFixed(2)}
                </li>
              ))}
            </ul>
          </Panel>
        </div>
      </div>
    </div>
  );
}

function Panel({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <section className="rounded-lg border border-zinc-800 bg-zinc-900/50 p-3">
      <h2 className="text-sm font-medium text-zinc-300 mb-2">{title}</h2>
      {children}
    </section>
  );
}

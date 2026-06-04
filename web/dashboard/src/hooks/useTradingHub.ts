import { useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import type { CandleDto, FillDto, LevelDto, PositionDto } from '../types';

export function useTradingHub() {
  const [connected, setConnected] = useState(false);
  const [lastPrice, setLastPrice] = useState(0);
  const [levels, setLevels] = useState<LevelDto[]>([]);
  const [candlePatch, setCandlePatch] = useState<CandleDto | null>(null);
  const [positions, setPositions] = useState<PositionDto[]>([]);
  const [trades, setTrades] = useState<FillDto[]>([]);
  const connRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    const conn = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/trading')
      .withAutomaticReconnect()
      .build();

    conn.on('ticker', (p: { price: number }) => setLastPrice(p.price));
    conn.on('levels', (p: { zones: LevelDto[] }) => setLevels(p.zones ?? []));
    conn.on('candle', (c: CandleDto) => setCandlePatch(c));
    conn.on('position', (p: PositionDto) => {
      setPositions((prev) => {
        const idx = prev.findIndex((x) => x.id === p.id);
        if (idx >= 0) {
          const next = [...prev];
          next[idx] = p;
          return next;
        }
        return [p, ...prev].slice(0, 20);
      });
    });
    conn.on('trade', (t: FillDto) => {
      setTrades((prev) => [t, ...prev].slice(0, 50));
    });

    conn
      .start()
      .then(() => setConnected(true))
      .catch(() => setConnected(false));

    conn.onreconnected(() => setConnected(true));
    conn.onclose(() => setConnected(false));

    connRef.current = conn;
    return () => {
      void conn.stop();
    };
  }, []);

  return { connected, lastPrice, levels, candlePatch, positions, trades };
}

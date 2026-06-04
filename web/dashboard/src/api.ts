import type { CandleDto, FillDto, HealthDto, LevelDto, PositionDto } from './types';

const base = '';

export async function fetchHealth(): Promise<HealthDto> {
  const r = await fetch(`${base}/api/health`);
  return r.json();
}

export async function fetchCandles(interval: string, limit = 500): Promise<CandleDto[]> {
  const r = await fetch(`${base}/api/candles?interval=${interval}&limit=${limit}`);
  return r.json();
}

export async function fetchLevels(): Promise<LevelDto[]> {
  const r = await fetch(`${base}/api/levels`);
  return r.json();
}

export async function fetchPositions(): Promise<{ open: PositionDto[]; recent: PositionDto[] }> {
  const r = await fetch(`${base}/api/positions`);
  return r.json();
}

export async function fetchTrades(limit = 50): Promise<FillDto[]> {
  const r = await fetch(`${base}/api/trades?limit=${limit}`);
  return r.json();
}

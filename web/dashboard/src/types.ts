export interface CandleDto {
  symbol: string;
  interval: string;
  time: number;
  open: number;
  high: number;
  low: number;
  close: number;
  volume: number;
  isClosed: boolean;
}

export interface LevelDto {
  id: string;
  symbol: string;
  center: number;
  lowerBound: number;
  upperBound: number;
  type: string;
  touches: number;
  invalidated: boolean;
}

export interface PositionDto {
  id: string;
  symbol: string;
  side: string;
  status: string;
  entryPrice: number;
  stopLoss: number;
  quantity: number;
  remainingQuantity: number;
  realizedPnL: number;
  takeProfitPrices: number[];
  openedAtUtc: string;
  closedAtUtc?: string;
}

export interface FillDto {
  id: string;
  positionId: string;
  symbol: string;
  side: string;
  isEntry: boolean;
  price: number;
  quantity: number;
  fee: number;
  partialTpIndex?: number;
  timestampUtc: string;
}

export interface HealthDto {
  status: string;
  tradingMode: string;
  marketDataConnected: boolean;
  lastPrice: number;
  symbol: string;
}

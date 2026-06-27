export interface CostUsageResponse {
  windowStartUtc: string;
  windowEndUtc: string;
  studentContext: string;
  metricName: string;
  aggregationStatus: string;
  usageDelta: number | null;
  ratePerHour: number | null;
  baselineSampleTimestampUtc: string | null;
  baselineSampleValue: number | null;
  endingSampleTimestampUtc: string | null;
  endingSampleValue: number | null;
  notesOrReason: string | null;
}

export interface CostTrendResponse {
  windowStartUtc: string;
  windowEndUtc: string;
  bucketGranularityHours: number;
  studentContext: string;
  metricName: string;
  buckets: CostTrendBucket[];
}

export interface CostTrendBucket {
  bucketStartUtc: string;
  bucketEndUtc: string;
  costValue: number;
}

export interface TimeRangeOption {
  key: string;
  label: string;
  days: number;
}

export interface PanelState<T> {
  status: "loading" | "success" | "incomplete" | "empty" | "error";
  data: T | null;
  error?: string;
}

"use client";

import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from "recharts";
import type { PanelState, CostTrendResponse, CostTrendBucket } from "@/types/cost-usage-dashboard";

interface CostOverTimePanelProps {
  state: PanelState<CostTrendResponse>;
}

function formatDate(iso: string): string {
  const d = new Date(iso);
  return d.toLocaleDateString(undefined, { month: "short", day: "numeric" });
}

function chartData(buckets: CostTrendBucket[]): { date: string; cost: number }[] {
  return buckets.map((b) => ({
    date: formatDate(b.bucketStartUtc),
    cost: Math.round(b.costValue * 100) / 100,
  }));
}

export default function CostOverTimePanel({ state }: CostOverTimePanelProps) {
  return (
    <div className="bg-white dark:bg-zinc-900 rounded-xl border border-zinc-200 dark:border-zinc-800 p-6">
      <div className="flex items-center gap-3 mb-4">
        <h2 className="text-sm font-semibold text-zinc-500 dark:text-zinc-400 uppercase tracking-wide">
          Cost Over Time
        </h2>
        {state.status === "incomplete" && (
          <span className="text-xs bg-yellow-100 dark:bg-yellow-900/30 text-yellow-700 dark:text-yellow-400 px-2 py-0.5 rounded">
            Partial data
          </span>
        )}
      </div>

      {state.status === "loading" && (
        <div className="h-64 flex items-center justify-center">
          <div className="w-8 h-8 border-2 border-blue-500 border-t-transparent rounded-full animate-spin" />
        </div>
      )}

      {state.status === "error" && (
        <div className="h-64 flex items-center justify-center">
          <p className="text-red-500 text-sm">Failed to load trend data</p>
        </div>
      )}

      {(state.status === "success" || state.status === "incomplete") && state.data && (
        <div className="h-64">
          {state.data.buckets.length === 0 ? (
            <div className="h-full flex items-center justify-center">
              <p className="text-zinc-400 text-sm">No activity in this period</p>
            </div>
          ) : (
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={chartData(state.data.buckets)} margin={{ top: 8, right: 8, left: 0, bottom: 8 }}>
                <CartesianGrid strokeDasharray="3 3" className="stroke-zinc-200 dark:stroke-zinc-700" />
                <XAxis
                  dataKey="date"
                  tick={{ fontSize: 12 }}
                  className="text-zinc-500"
                />
                <YAxis
                  tick={{ fontSize: 12 }}
                  className="text-zinc-500"
                  tickFormatter={(v) => `$${Number(v).toFixed(2)}`}
                />
                <Tooltip
                  formatter={(value) => [`$${Number(value).toFixed(2)}`, "Cost"]}
                  contentStyle={{
                    backgroundColor: "var(--tooltip-bg, #fff)",
                    border: "1px solid #e4e4e7",
                    borderRadius: "8px",
                    fontSize: "13px",
                  }}
                />
                <Bar dataKey="cost" fill="#3b82f6" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          )}
        </div>
      )}

      {state.status === "empty" && (
        <div className="h-64 flex items-center justify-center">
          <p className="text-zinc-400 text-sm">No trend data available</p>
        </div>
      )}
    </div>
  );
}

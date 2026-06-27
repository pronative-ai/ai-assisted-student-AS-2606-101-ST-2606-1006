"use client";

import type { PanelState, CostUsageResponse } from "@/types/cost-usage-dashboard";

interface TotalCostPanelProps {
  state: PanelState<CostUsageResponse>;
}

function formatCost(value: number | null): string {
  if (value === null) return "—";
  return `$${value.toFixed(2)}`;
}

export default function TotalCostPanel({ state }: TotalCostPanelProps) {
  return (
    <div className="bg-white dark:bg-zinc-900 rounded-xl border border-zinc-200 dark:border-zinc-800 p-6">
      <h2 className="text-sm font-semibold text-zinc-500 dark:text-zinc-400 uppercase tracking-wide">
        Total Cost
      </h2>

      <div className="mt-3">
        {state.status === "loading" && (
          <div className="h-10 flex items-center">
            <div className="w-8 h-8 border-2 border-blue-500 border-t-transparent rounded-full animate-spin" />
          </div>
        )}

        {state.status === "error" && (
          <div className="h-10 flex items-center">
            <p className="text-red-500 text-sm">Failed to load</p>
          </div>
        )}

        {(state.status === "success" || state.status === "incomplete") && state.data && (
          <div className="flex items-baseline gap-3">
            <span className="text-3xl font-bold tabular-nums">
              {formatCost(state.data.usageDelta)}
            </span>
            {state.status === "incomplete" && (
              <span className="text-xs bg-yellow-100 dark:bg-yellow-900/30 text-yellow-700 dark:text-yellow-400 px-2 py-0.5 rounded">
                Incomplete
              </span>
            )}
          </div>
        )}

        {state.status === "empty" && (
          <p className="text-zinc-400 text-sm">No cost data for this period</p>
        )}

        {state.data?.notesOrReason && (
          <p className="mt-2 text-xs text-zinc-400 italic">{state.data.notesOrReason}</p>
        )}
      </div>
    </div>
  );
}

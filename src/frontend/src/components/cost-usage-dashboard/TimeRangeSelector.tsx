"use client";

import type { TimeRangeOption } from "@/types/cost-usage-dashboard";

const TIME_RANGES: TimeRangeOption[] = [
  { key: "7d", label: "Last 7 days", days: 7 },
  { key: "14d", label: "Last 14 days", days: 14 },
  { key: "30d", label: "Last 30 days", days: 30 },
];

interface TimeRangeSelectorProps {
  selected: string;
  onChange: (key: string, start: string, end: string) => void;
}

export default function TimeRangeSelector({ selected, onChange }: TimeRangeSelectorProps) {
  const handleChange = (key: string) => {
    const option = TIME_RANGES.find((r) => r.key === key);
    if (!option) return;
    const end = new Date();
    const start = new Date(end.getTime() - option.days * 24 * 60 * 60 * 1000);
    onChange(key, start.toISOString(), end.toISOString());
  };

  return (
    <div className="flex gap-2 items-center">
      <span className="text-sm font-medium text-zinc-600 dark:text-zinc-400">Time Range:</span>
      <div className="flex gap-1">
        {TIME_RANGES.map((range) => (
          <button
            key={range.key}
            onClick={() => handleChange(range.key)}
            className={`px-3 py-1.5 text-sm rounded-md transition-colors ${
              selected === range.key
                ? "bg-blue-600 text-white"
                : "bg-zinc-100 dark:bg-zinc-800 text-zinc-700 dark:text-zinc-300 hover:bg-zinc-200 dark:hover:bg-zinc-700"
            }`}
          >
            {range.label}
          </button>
        ))}
      </div>
    </div>
  );
}

export { TIME_RANGES };

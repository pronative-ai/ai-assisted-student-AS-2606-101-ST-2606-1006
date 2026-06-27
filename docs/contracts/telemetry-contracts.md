# Telemetry Contracts — OpenCode Cost Usage

**Intent ID:** `intent-2606-101-1006-0002`
**Parent Intent:** `intent-2606-101-1006-0001`

## Supported Signals

Only the following signals are accepted by the ingestion pipeline:

| Signal Kind | Signal Name | Description |
|------------|-------------|-------------|
| Metric | `opencode.cost.usage` | Cumulative counter (USD per completed message) |
| Log | `api_request` | OpenCode API request event |
| Log | `api_error` | OpenCode API error event |

All other signals are ignored or rejected.

## Transport

- **Enabled:** OTLP HTTP/protobuf (`POST /v1/metrics`, `POST /v1/logs`)
- **Disabled:** OTLP gRPC (not implemented in MVP)
- Requests over a disabled transport will not be accepted.

## Ingestion Endpoints

### POST /v1/metrics

Accepts metric telemetry for `opencode.cost.usage`.

**Request body format** (line-delimited, pipe-separated):
```
metric_name|cumulative_value|timestamp_utc|resource_attributes|metric_attributes
```

**Response:**
```json
{
  "message": "Metrics processed",
  "accepted": 1,
  "rejected": 0
}
```

### POST /v1/logs

Accepts log events for `api_request` and `api_error`.

**Request body format** (line-delimited, pipe-separated):
```
event_name|timestamp_utc|body|attributes|resource_attributes
```

**Response:**
```json
{
  "message": "Logs processed",
  "accepted": 1,
  "rejected": 0
}
```

## Metric Sample Contract (`opencode.cost.usage`)

```json
{
  "id": "string (guid)",
  "partitionKey": "string (default: 'student-default')",
  "studentContext": "string (default: 'student-default')",
  "signalKind": "metric",
  "signalName": "opencode.cost.usage",
  "sampleTimestampUtc": "2026-01-01T10:00:00Z",
  "cumulativeValue": 12.0,
  "resourceAttributes": "string (JSON, optional)",
  "metricAttributes": "string (JSON, optional)",
  "sourceTransport": "otlp_http_protobuf",
  "ingestedAtUtc": "2026-01-01T10:00:01Z"
}
```

## Log Event Contracts

### api_request

```json
{
  "id": "string (guid)",
  "partitionKey": "string (default: 'student-default')",
  "studentContext": "string (default: 'student-default')",
  "signalKind": "log",
  "eventName": "api_request",
  "eventTimestampUtc": "2026-01-01T10:00:00Z",
  "body": "string (JSON, optional)",
  "attributes": "string (JSON, optional)",
  "resourceAttributes": "string (JSON, optional)",
  "sourceTransport": "otlp_http_protobuf",
  "ingestedAtUtc": "2026-01-01T10:00:01Z"
}
```

### api_error

```json
{
  "id": "string (guid)",
  "partitionKey": "string (default: 'student-default')",
  "studentContext": "string (default: 'student-default')",
  "signalKind": "log",
  "eventName": "api_error",
  "eventTimestampUtc": "2026-01-01T10:00:00Z",
  "body": "string (JSON, optional)",
  "attributes": "string (JSON, optional)",
  "resourceAttributes": "string (JSON, optional)",
  "sourceTransport": "otlp_http_protobuf",
  "ingestedAtUtc": "2026-01-01T10:00:01Z"
}
```

## Read-Only Aggregation API

### GET /api/opencode/cost-usage?start={iso8601}&end={iso8601}

**Parameters:**
- `start` (required) — ISO 8601 UTC start of query window
- `end` (required) — ISO 8601 UTC end of query window

**Validation rules:**
- Both `start` and `end` are required
- `start` must be earlier than `end`
- Invalid dates return 400 Bad Request

**Response (200 OK):**
```json
{
  "windowStartUtc": "2026-01-01T10:00:00Z",
  "windowEndUtc": "2026-01-01T11:00:00Z",
  "studentContext": "student-default",
  "metricName": "opencode.cost.usage",
  "aggregationStatus": "complete",
  "usageDelta": 6.5,
  "ratePerHour": 6.5,
  "baselineSampleTimestampUtc": "2026-01-01T10:00:00Z",
  "baselineSampleValue": 12.0,
  "endingSampleTimestampUtc": "2026-01-01T11:00:00Z",
  "endingSampleValue": 18.5,
  "notesOrReason": null
}
```

**Aggregation status values:**
| Status | Meaning |
|--------|---------|
| `complete` | Delta computed from baseline and ending samples |
| `incomplete_missing_baseline` | No baseline sample at or before window start |
| `no_samples_in_window` | No telemetry found in or before the window |
| `anomaly_counter_decrease` | Counter decreased between baseline and ending (possible reset) |
| `invalid_range` | Start >= end or missing parameters |

**Edge cases:**
- Zero data: returns `aggregationStatus: "no_samples_in_window"` with `usageDelta: 0`
- Missing baseline: returns `aggregationStatus: "incomplete_missing_baseline"` with `usageDelta: null`
- Counter decrease: returns `aggregationStatus: "anomaly_counter_decrease"` with `usageDelta: null`

## Data Store

- **Datastore:** Cosmos DB
- **Database:** `opencode-telemetry`
- **Containers:** `metric-samples` (for `opencode.cost.usage`), `log-events` (for `api_request`, `api_error`)
- **Partition key:** `/partitionKey` (single-user = `student-default`)

## Configuration & Secrets

- Cosmos DB connection string, Key Vault URI, and Application Insights connection string are sourced from Key Vault-backed configuration.
- No secrets are hard-coded in committed files.

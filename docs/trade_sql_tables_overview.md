# Trade SQL Tables Overview

This document explains the main SQL tables used to store trade data imported from the CSV files.

## 1. trade table

- Combines imports, exports, and domestic CSV files into a single SQL table.
- One row per trade flow record.
- Key columns (example):
  - `trade_id` – integer ID for each row.
  - `year` – 2019 or 2022.
  - `country_code` – country (e.g., US, IN, CN).
  - `flow_type` – imports, exports, or domestic.
  - `partner_country` – trading partner.
  - `industry_code` – industry/category.
  - `amount` – trade value (numeric).

The exact column names should match the CSV headers from the trade-data repo.

## 2. tradeid_lookup table

- Used to create smaller, consistent `trade_id` values instead of large GUIDs.
- Columns:
  - `country_code`
  - `source_tradeid` – original ID from the CSV or source.
  - `trade_id` – integer ID we generate and store.

This table lets us:
- Keep `trade_id` small and efficient.
- Map back to the original source IDs if needed.
- Avoid collisions when combining data from different sources.

## Future steps

- Use these definitions to write SQL scripts that create the tables.
- Connect the .NET import code so it reads CSV files and inserts rows into `trade` and `tradeid_lookup`.

# SQLite Trade Quickstart (Beginner Path)

## Why this document
The main project uses .NET 8 + PostgreSQL (Azure/local) to import trade CSV files.
This guide provides a **minimal, beginner-friendly path** to understand the data flow
using SQLite and a single CSV file.

## Small data slice (intentionally minimal)
- Year: 2019
- Country: IN (India)
- Trade flow: imports
- File: trade.csv

## CSV source
From the trade-data repository:

trade-data/year/2019/IN/imports/trade.csv

## Goal
Validate the pipeline end-to-end:

CSV → SQL table → basic queries

before scaling to PostgreSQL and multi-country imports.

## High-level steps
1. Locate or download the `trade.csv` file
2. Create a local SQLite database (`trade.db`)
3. Create a table using CSV headers
4. Import CSV rows into the table
5. Run basic validation queries:
   - total row count
   - check NULLs in key columns
   - sample top rows

## Notes
- This guide is for learning and validation only.
- The same logic later maps to PostgreSQL import used by the main system.

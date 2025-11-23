-- =============================================
-- Script: 001_CreateTradeTable.sql
-- Description: Creates the main trade table that consolidates ALL trade data
--              from all countries, years, and tradeflow types (imports, exports, domestic)
-- Author: Generated for ModelEarth Trade Import System
-- Date: 2025-11-22
-- =============================================

-- Drop table if exists (for development - remove in production)
-- DROP TABLE IF EXISTS public.trade CASCADE;

-- Create the main trade table
CREATE TABLE IF NOT EXISTS public.trade (
    trade_id BIGSERIAL PRIMARY KEY,           -- Auto-incrementing ID
    year SMALLINT NOT NULL,                   -- 2019 or 2022
    region1 CHAR(2) NOT NULL,                 -- Source country code (e.g., 'US', 'IN', 'CN')
    region2 CHAR(2) NOT NULL,                 -- Destination country code
    industry1 TEXT NOT NULL,                  -- Source industry
    industry2 TEXT NOT NULL,                  -- Destination industry
    amount NUMERIC(20,4) NOT NULL,            -- Trade amount
    tradeflow_type VARCHAR(20) NOT NULL,      -- 'imports', 'exports', or 'domestic'
    source_file VARCHAR(255),                 -- Track which CSV this came from (e.g., '2022/US/imports/trade.csv')
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_trade_year ON public.trade(year);
CREATE INDEX IF NOT EXISTS idx_trade_regions ON public.trade(region1, region2);
CREATE INDEX IF NOT EXISTS idx_trade_tradeflow ON public.trade(tradeflow_type);
CREATE INDEX IF NOT EXISTS idx_trade_year_region1_tradeflow ON public.trade(year, region1, tradeflow_type);

-- Create comment on table
COMMENT ON TABLE public.trade IS 'Consolidated trade data from all countries, years, and tradeflow types';
COMMENT ON COLUMN public.trade.trade_id IS 'Auto-incrementing unique identifier';
COMMENT ON COLUMN public.trade.year IS 'Trade data year (2019, 2022, etc.)';
COMMENT ON COLUMN public.trade.region1 IS 'Source country/region code (2-letter)';
COMMENT ON COLUMN public.trade.region2 IS 'Destination country/region code (2-letter)';
COMMENT ON COLUMN public.trade.tradeflow_type IS 'Type of trade flow: imports, exports, or domestic';
COMMENT ON COLUMN public.trade.source_file IS 'Original CSV file path for tracking';

-- Create updated_at trigger function if it doesn't exist
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create trigger to automatically update updated_at
DROP TRIGGER IF EXISTS update_trade_updated_at ON public.trade;
CREATE TRIGGER update_trade_updated_at
    BEFORE UPDATE ON public.trade
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- Grant permissions (adjust as needed for your environment)
-- GRANT SELECT, INSERT, UPDATE, DELETE ON public.trade TO your_app_user;
-- GRANT USAGE, SELECT ON SEQUENCE public.trade_trade_id_seq TO your_app_user;

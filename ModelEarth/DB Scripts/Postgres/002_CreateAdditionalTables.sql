-- =============================================
-- Script: 002_CreateAdditionalTables.sql
-- Description: Creates the additional 8 tables for trade data
--              (trade_employment, trade_factor, trade_impact, trade_material, trade_resource, and 3 BEA tables)
-- Author: Generated for ModelEarth Trade Import System
-- Date: 2025-11-22
-- =============================================

-- =============================================
-- 1. trade_employment table
-- =============================================
CREATE TABLE IF NOT EXISTS public.trade_employment (
    id BIGSERIAL PRIMARY KEY,
    year SMALLINT NOT NULL,
    region1 CHAR(2) NOT NULL,
    region2 CHAR(2) NOT NULL,
    industry1 TEXT NOT NULL,
    industry2 TEXT NOT NULL,
    employment_value NUMERIC(20,4) NOT NULL,
    tradeflow_type VARCHAR(20) NOT NULL,
    source_file VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_trade_employment_year ON public.trade_employment(year);
CREATE INDEX IF NOT EXISTS idx_trade_employment_regions ON public.trade_employment(region1, region2);
CREATE INDEX IF NOT EXISTS idx_trade_employment_tradeflow ON public.trade_employment(tradeflow_type);

COMMENT ON TABLE public.trade_employment IS 'Employment impacts from trade data';

-- =============================================
-- 2. trade_factor table
-- =============================================
CREATE TABLE IF NOT EXISTS public.trade_factor (
    id BIGSERIAL PRIMARY KEY,
    year SMALLINT NOT NULL,
    region1 CHAR(2) NOT NULL,
    region2 CHAR(2) NOT NULL,
    industry1 TEXT NOT NULL,
    industry2 TEXT NOT NULL,
    factor_value NUMERIC(20,4) NOT NULL,
    tradeflow_type VARCHAR(20) NOT NULL,
    source_file VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_trade_factor_year ON public.trade_factor(year);
CREATE INDEX IF NOT EXISTS idx_trade_factor_regions ON public.trade_factor(region1, region2);
CREATE INDEX IF NOT EXISTS idx_trade_factor_tradeflow ON public.trade_factor(tradeflow_type);

COMMENT ON TABLE public.trade_factor IS 'Production factor data from trade';

-- =============================================
-- 3. trade_impact table
-- =============================================
CREATE TABLE IF NOT EXISTS public.trade_impact (
    id BIGSERIAL PRIMARY KEY,
    year SMALLINT NOT NULL,
    region1 CHAR(2) NOT NULL,
    region2 CHAR(2) NOT NULL,
    industry1 TEXT NOT NULL,
    industry2 TEXT NOT NULL,
    impact_value NUMERIC(20,4) NOT NULL,
    tradeflow_type VARCHAR(20) NOT NULL,
    source_file VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_trade_impact_year ON public.trade_impact(year);
CREATE INDEX IF NOT EXISTS idx_trade_impact_regions ON public.trade_impact(region1, region2);
CREATE INDEX IF NOT EXISTS idx_trade_impact_tradeflow ON public.trade_impact(tradeflow_type);

COMMENT ON TABLE public.trade_impact IS 'Economic impact data from trade';

-- =============================================
-- 4. trade_material table
-- =============================================
CREATE TABLE IF NOT EXISTS public.trade_material (
    id BIGSERIAL PRIMARY KEY,
    year SMALLINT NOT NULL,
    region1 CHAR(2) NOT NULL,
    region2 CHAR(2) NOT NULL,
    industry1 TEXT NOT NULL,
    industry2 TEXT NOT NULL,
    material_value NUMERIC(20,4) NOT NULL,
    tradeflow_type VARCHAR(20) NOT NULL,
    source_file VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_trade_material_year ON public.trade_material(year);
CREATE INDEX IF NOT EXISTS idx_trade_material_regions ON public.trade_material(region1, region2);
CREATE INDEX IF NOT EXISTS idx_trade_material_tradeflow ON public.trade_material(tradeflow_type);

COMMENT ON TABLE public.trade_material IS 'Material flow data from trade';

-- =============================================
-- 5. trade_resource table
-- =============================================
CREATE TABLE IF NOT EXISTS public.trade_resource (
    id BIGSERIAL PRIMARY KEY,
    year SMALLINT NOT NULL,
    region1 CHAR(2) NOT NULL,
    region2 CHAR(2) NOT NULL,
    industry1 TEXT NOT NULL,
    industry2 TEXT NOT NULL,
    resource_value NUMERIC(20,4) NOT NULL,
    tradeflow_type VARCHAR(20) NOT NULL,
    source_file VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_trade_resource_year ON public.trade_resource(year);
CREATE INDEX IF NOT EXISTS idx_trade_resource_regions ON public.trade_resource(region1, region2);
CREATE INDEX IF NOT EXISTS idx_trade_resource_tradeflow ON public.trade_resource(tradeflow_type);

COMMENT ON TABLE public.trade_resource IS 'Resource usage data from trade';

-- =============================================
-- 6-8. BEA (Bureau of Economic Analysis) tables
-- Note: The actual table structures will depend on the BEA CSV file formats
-- These are placeholder structures - update based on actual BEA data
-- =============================================

CREATE TABLE IF NOT EXISTS public.bea_table1 (
    id BIGSERIAL PRIMARY KEY,
    year SMALLINT NOT NULL,
    region1 CHAR(2) NOT NULL,
    region2 CHAR(2) NOT NULL,
    industry1 TEXT NOT NULL,
    industry2 TEXT NOT NULL,
    bea_value NUMERIC(20,4) NOT NULL,
    tradeflow_type VARCHAR(20) NOT NULL,
    source_file VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_bea_table1_year ON public.bea_table1(year);
CREATE INDEX IF NOT EXISTS idx_bea_table1_regions ON public.bea_table1(region1, region2);
CREATE INDEX IF NOT EXISTS idx_bea_table1_tradeflow ON public.bea_table1(tradeflow_type);

COMMENT ON TABLE public.bea_table1 IS 'BEA (Bureau of Economic Analysis) data - table 1 (structure TBD)';

CREATE TABLE IF NOT EXISTS public.bea_table2 (
    id BIGSERIAL PRIMARY KEY,
    year SMALLINT NOT NULL,
    region1 CHAR(2) NOT NULL,
    region2 CHAR(2) NOT NULL,
    industry1 TEXT NOT NULL,
    industry2 TEXT NOT NULL,
    bea_value NUMERIC(20,4) NOT NULL,
    tradeflow_type VARCHAR(20) NOT NULL,
    source_file VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_bea_table2_year ON public.bea_table2(year);
CREATE INDEX IF NOT EXISTS idx_bea_table2_regions ON public.bea_table2(region1, region2);
CREATE INDEX IF NOT EXISTS idx_bea_table2_tradeflow ON public.bea_table2(tradeflow_type);

COMMENT ON TABLE public.bea_table2 IS 'BEA (Bureau of Economic Analysis) data - table 2 (structure TBD)';

CREATE TABLE IF NOT EXISTS public.bea_table3 (
    id BIGSERIAL PRIMARY KEY,
    year SMALLINT NOT NULL,
    region1 CHAR(2) NOT NULL,
    region2 CHAR(2) NOT NULL,
    industry1 TEXT NOT NULL,
    industry2 TEXT NOT NULL,
    bea_value NUMERIC(20,4) NOT NULL,
    tradeflow_type VARCHAR(20) NOT NULL,
    source_file VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_bea_table3_year ON public.bea_table3(year);
CREATE INDEX IF NOT EXISTS idx_bea_table3_regions ON public.bea_table3(region1, region2);
CREATE INDEX IF NOT EXISTS idx_bea_table3_tradeflow ON public.bea_table3(tradeflow_type);

COMMENT ON TABLE public.bea_table3 IS 'BEA (Bureau of Economic Analysis) data - table 3 (structure TBD)';

-- Create triggers for updated_at (reuse function from 001_CreateTradeTable.sql)
DROP TRIGGER IF EXISTS update_trade_employment_updated_at ON public.trade_employment;
CREATE TRIGGER update_trade_employment_updated_at
    BEFORE UPDATE ON public.trade_employment
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_trade_factor_updated_at ON public.trade_factor;
CREATE TRIGGER update_trade_factor_updated_at
    BEFORE UPDATE ON public.trade_factor
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_trade_impact_updated_at ON public.trade_impact;
CREATE TRIGGER update_trade_impact_updated_at
    BEFORE UPDATE ON public.trade_impact
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_trade_material_updated_at ON public.trade_material;
CREATE TRIGGER update_trade_material_updated_at
    BEFORE UPDATE ON public.trade_material
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_trade_resource_updated_at ON public.trade_resource;
CREATE TRIGGER update_trade_resource_updated_at
    BEFORE UPDATE ON public.trade_resource
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_bea_table1_updated_at ON public.bea_table1;
CREATE TRIGGER update_bea_table1_updated_at
    BEFORE UPDATE ON public.bea_table1
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_bea_table2_updated_at ON public.bea_table2;
CREATE TRIGGER update_bea_table2_updated_at
    BEFORE UPDATE ON public.bea_table2
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_bea_table3_updated_at ON public.bea_table3;
CREATE TRIGGER update_bea_table3_updated_at
    BEFORE UPDATE ON public.bea_table3
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

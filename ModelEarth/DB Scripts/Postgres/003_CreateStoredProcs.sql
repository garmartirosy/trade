-- =============================================
-- Script: 003_CreateStoredProcs.sql
-- Description: Creates PostgreSQL functions for import operations
-- Author: Generated for ModelEarth Trade Import System
-- Date: 2025-11-22
-- =============================================

-- =============================================
-- Function: clear_year_data
-- Description: Deletes all data for a specific year from all trade tables
-- Parameters: p_year - The year to clear (e.g., 2019, 2022)
-- Returns: Total number of rows deleted across all tables
-- =============================================
CREATE OR REPLACE FUNCTION clear_year_data(p_year SMALLINT)
RETURNS TABLE(
    table_name TEXT,
    rows_deleted INTEGER
) AS $$
DECLARE
    trade_deleted INTEGER;
    employment_deleted INTEGER;
    factor_deleted INTEGER;
    impact_deleted INTEGER;
    material_deleted INTEGER;
    resource_deleted INTEGER;
    bea1_deleted INTEGER;
    bea2_deleted INTEGER;
    bea3_deleted INTEGER;
BEGIN
    -- Delete from trade table
    DELETE FROM public.trade WHERE year = p_year;
    GET DIAGNOSTICS trade_deleted = ROW_COUNT;

    -- Delete from trade_employment table
    DELETE FROM public.trade_employment WHERE year = p_year;
    GET DIAGNOSTICS employment_deleted = ROW_COUNT;

    -- Delete from trade_factor table
    DELETE FROM public.trade_factor WHERE year = p_year;
    GET DIAGNOSTICS factor_deleted = ROW_COUNT;

    -- Delete from trade_impact table
    DELETE FROM public.trade_impact WHERE year = p_year;
    GET DIAGNOSTICS impact_deleted = ROW_COUNT;

    -- Delete from trade_material table
    DELETE FROM public.trade_material WHERE year = p_year;
    GET DIAGNOSTICS material_deleted = ROW_COUNT;

    -- Delete from trade_resource table
    DELETE FROM public.trade_resource WHERE year = p_year;
    GET DIAGNOSTICS resource_deleted = ROW_COUNT;

    -- Delete from BEA tables
    DELETE FROM public.bea_table1 WHERE year = p_year;
    GET DIAGNOSTICS bea1_deleted = ROW_COUNT;

    DELETE FROM public.bea_table2 WHERE year = p_year;
    GET DIAGNOSTICS bea2_deleted = ROW_COUNT;

    DELETE FROM public.bea_table3 WHERE year = p_year;
    GET DIAGNOSTICS bea3_deleted = ROW_COUNT;

    -- Return results
    RETURN QUERY SELECT 'trade'::TEXT, trade_deleted;
    RETURN QUERY SELECT 'trade_employment'::TEXT, employment_deleted;
    RETURN QUERY SELECT 'trade_factor'::TEXT, factor_deleted;
    RETURN QUERY SELECT 'trade_impact'::TEXT, impact_deleted;
    RETURN QUERY SELECT 'trade_material'::TEXT, material_deleted;
    RETURN QUERY SELECT 'trade_resource'::TEXT, resource_deleted;
    RETURN QUERY SELECT 'bea_table1'::TEXT, bea1_deleted;
    RETURN QUERY SELECT 'bea_table2'::TEXT, bea2_deleted;
    RETURN QUERY SELECT 'bea_table3'::TEXT, bea3_deleted;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION clear_year_data IS 'Deletes all trade data for a specific year across all 9 tables';

-- =============================================
-- Function: get_import_statistics
-- Description: Gets statistics about imported data for a specific year
-- Parameters: p_year - The year to get statistics for
-- Returns: Table with counts by region and tradeflow type
-- =============================================
CREATE OR REPLACE FUNCTION get_import_statistics(p_year SMALLINT)
RETURNS TABLE(
    region1 CHAR(2),
    tradeflow_type VARCHAR(20),
    trade_count BIGINT,
    employment_count BIGINT,
    factor_count BIGINT,
    impact_count BIGINT,
    material_count BIGINT,
    resource_count BIGINT,
    total_amount NUMERIC
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        t.region1,
        t.tradeflow_type,
        COUNT(DISTINCT t.trade_id) as trade_count,
        COUNT(DISTINCT te.id) as employment_count,
        COUNT(DISTINCT tf.id) as factor_count,
        COUNT(DISTINCT ti.id) as impact_count,
        COUNT(DISTINCT tm.id) as material_count,
        COUNT(DISTINCT tr.id) as resource_count,
        SUM(t.amount) as total_amount
    FROM public.trade t
    LEFT JOIN public.trade_employment te ON t.year = te.year AND t.region1 = te.region1 AND t.tradeflow_type = te.tradeflow_type
    LEFT JOIN public.trade_factor tf ON t.year = tf.year AND t.region1 = tf.region1 AND t.tradeflow_type = tf.tradeflow_type
    LEFT JOIN public.trade_impact ti ON t.year = ti.year AND t.region1 = ti.region1 AND t.tradeflow_type = ti.tradeflow_type
    LEFT JOIN public.trade_material tm ON t.year = tm.year AND t.region1 = tm.region1 AND t.tradeflow_type = tm.tradeflow_type
    LEFT JOIN public.trade_resource tr ON t.year = tr.year AND t.region1 = tr.region1 AND t.tradeflow_type = tr.tradeflow_type
    WHERE t.year = p_year
    GROUP BY t.region1, t.tradeflow_type
    ORDER BY t.region1, t.tradeflow_type;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION get_import_statistics IS 'Returns import statistics by region and tradeflow type for a specific year';

-- =============================================
-- Function: get_table_counts
-- Description: Gets row counts for all trade tables for a specific year
-- Parameters: p_year - The year to count (optional, NULL for all years)
-- Returns: Table with row counts per table
-- =============================================
CREATE OR REPLACE FUNCTION get_table_counts(p_year SMALLINT DEFAULT NULL)
RETURNS TABLE(
    table_name TEXT,
    row_count BIGINT,
    year_filter SMALLINT
) AS $$
BEGIN
    IF p_year IS NULL THEN
        -- Count all rows
        RETURN QUERY SELECT 'trade'::TEXT, COUNT(*)::BIGINT, NULL::SMALLINT FROM public.trade;
        RETURN QUERY SELECT 'trade_employment'::TEXT, COUNT(*)::BIGINT, NULL::SMALLINT FROM public.trade_employment;
        RETURN QUERY SELECT 'trade_factor'::TEXT, COUNT(*)::BIGINT, NULL::SMALLINT FROM public.trade_factor;
        RETURN QUERY SELECT 'trade_impact'::TEXT, COUNT(*)::BIGINT, NULL::SMALLINT FROM public.trade_impact;
        RETURN QUERY SELECT 'trade_material'::TEXT, COUNT(*)::BIGINT, NULL::SMALLINT FROM public.trade_material;
        RETURN QUERY SELECT 'trade_resource'::TEXT, COUNT(*)::BIGINT, NULL::SMALLINT FROM public.trade_resource;
        RETURN QUERY SELECT 'bea_table1'::TEXT, COUNT(*)::BIGINT, NULL::SMALLINT FROM public.bea_table1;
        RETURN QUERY SELECT 'bea_table2'::TEXT, COUNT(*)::BIGINT, NULL::SMALLINT FROM public.bea_table2;
        RETURN QUERY SELECT 'bea_table3'::TEXT, COUNT(*)::BIGINT, NULL::SMALLINT FROM public.bea_table3;
    ELSE
        -- Count rows for specific year
        RETURN QUERY SELECT 'trade'::TEXT, COUNT(*)::BIGINT, p_year FROM public.trade WHERE year = p_year;
        RETURN QUERY SELECT 'trade_employment'::TEXT, COUNT(*)::BIGINT, p_year FROM public.trade_employment WHERE year = p_year;
        RETURN QUERY SELECT 'trade_factor'::TEXT, COUNT(*)::BIGINT, p_year FROM public.trade_factor WHERE year = p_year;
        RETURN QUERY SELECT 'trade_impact'::TEXT, COUNT(*)::BIGINT, p_year FROM public.trade_impact WHERE year = p_year;
        RETURN QUERY SELECT 'trade_material'::TEXT, COUNT(*)::BIGINT, p_year FROM public.trade_material WHERE year = p_year;
        RETURN QUERY SELECT 'trade_resource'::TEXT, COUNT(*)::BIGINT, p_year FROM public.trade_resource WHERE year = p_year;
        RETURN QUERY SELECT 'bea_table1'::TEXT, COUNT(*)::BIGINT, p_year FROM public.bea_table1 WHERE year = p_year;
        RETURN QUERY SELECT 'bea_table2'::TEXT, COUNT(*)::BIGINT, p_year FROM public.bea_table2 WHERE year = p_year;
        RETURN QUERY SELECT 'bea_table3'::TEXT, COUNT(*)::BIGINT, p_year FROM public.bea_table3 WHERE year = p_year;
    END IF;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION get_table_counts IS 'Returns row counts for all trade tables, optionally filtered by year';

-- =============================================
-- Function: get_distinct_countries
-- Description: Gets list of distinct countries that have data for a specific year
-- Parameters: p_year - The year to check
-- Returns: Table with distinct country codes
-- =============================================
CREATE OR REPLACE FUNCTION get_distinct_countries(p_year SMALLINT)
RETURNS TABLE(
    country_code CHAR(2),
    tradeflow_count INTEGER,
    total_trade_records BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        t.region1 as country_code,
        COUNT(DISTINCT t.tradeflow_type)::INTEGER as tradeflow_count,
        COUNT(*)::BIGINT as total_trade_records
    FROM public.trade t
    WHERE t.year = p_year
    GROUP BY t.region1
    ORDER BY t.region1;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION get_distinct_countries IS 'Returns distinct countries with data for a specific year';

-- =============================================
-- Example usage:
-- =============================================
-- Clear all 2022 data:
--   SELECT * FROM clear_year_data(2022);
--
-- Get import statistics for 2022:
--   SELECT * FROM get_import_statistics(2022);
--
-- Get row counts for 2022:
--   SELECT * FROM get_table_counts(2022);
--
-- Get row counts for all years:
--   SELECT * FROM get_table_counts();
--
-- Get distinct countries for 2022:
--   SELECT * FROM get_distinct_countries(2022);

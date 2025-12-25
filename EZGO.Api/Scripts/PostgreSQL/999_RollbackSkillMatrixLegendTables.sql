-- =============================================
-- Rollback Skills Matrix Legend Configuration Tables for PostgreSQL
-- Use this script to remove all Skills Matrix Legend related objects
-- WARNING: This will delete all legend configuration data!
-- Version: 1.0
-- =============================================

-- Drop function first
DROP FUNCTION IF EXISTS create_default_skill_matrix_legend(INTEGER);

-- Drop indexes (will be dropped with tables, but explicit for clarity)
DROP INDEX IF EXISTS ix_skill_matrix_legend_item_skill_type_order;
DROP INDEX IF EXISTS ix_skill_matrix_legend_item_config_id;
DROP INDEX IF EXISTS uq_skill_matrix_legend_item_config_skill_level;
DROP INDEX IF EXISTS ix_skill_matrix_legend_config_company_id;

-- Drop tables (items first due to foreign key)
DROP TABLE IF EXISTS skill_matrix_legend_item CASCADE;
DROP TABLE IF EXISTS skill_matrix_legend_configuration CASCADE;

-- Confirm rollback
DO $$
BEGIN
    RAISE NOTICE 'Skills Matrix Legend tables have been dropped successfully';
END $$;

-- =============================================
-- Rollback Skills Matrix Legend Configuration for PostgreSQL
-- Use this script to remove all Skills Matrix Legend related objects
-- WARNING: This will delete all legend configuration data!
-- Version: 1.0
-- =============================================

-- Drop stored procedures/functions first
DROP FUNCTION IF EXISTS sp_get_skill_matrix_legend_full(INTEGER);
DROP FUNCTION IF EXISTS sp_get_skill_matrix_legend_configuration(INTEGER);
DROP FUNCTION IF EXISTS sp_get_skill_matrix_legend_items(INTEGER);
DROP FUNCTION IF EXISTS sp_insert_skill_matrix_legend_configuration(INTEGER, INTEGER, INTEGER);
DROP FUNCTION IF EXISTS sp_insert_skill_matrix_legend_item(INTEGER, INTEGER, VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, INTEGER, INTEGER, VARCHAR, BOOLEAN);
DROP FUNCTION IF EXISTS sp_update_skill_matrix_legend_configuration(INTEGER, INTEGER, INTEGER);
DROP FUNCTION IF EXISTS sp_update_skill_matrix_legend_item(INTEGER, INTEGER, VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, INTEGER, INTEGER, VARCHAR);
DROP FUNCTION IF EXISTS sp_delete_skill_matrix_legend_items_by_company(INTEGER);
DROP FUNCTION IF EXISTS sp_delete_skill_matrix_legend_configuration(INTEGER);
DROP FUNCTION IF EXISTS sp_exists_skill_matrix_legend_configuration(INTEGER);
DROP FUNCTION IF EXISTS sp_get_skill_matrix_legend_version(INTEGER);
DROP FUNCTION IF EXISTS sp_get_skill_matrix_legend_configuration_id(INTEGER);
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
    RAISE NOTICE 'Skills Matrix Legend tables and stored procedures have been dropped successfully';
END $$;

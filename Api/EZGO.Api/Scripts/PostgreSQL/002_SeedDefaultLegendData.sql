-- =============================================
-- Seed Default Skills Matrix Legend Data for PostgreSQL
-- This script inserts default legend configuration for a company
-- Usage: Replace {company_id} with the actual company ID
-- Version: 1.0
-- =============================================

-- Function to create default legend configuration for a company
CREATE OR REPLACE FUNCTION create_default_skill_matrix_legend(p_company_id INTEGER)
RETURNS INTEGER AS $$
DECLARE
    v_config_id INTEGER;
BEGIN
    -- Check if configuration already exists
    SELECT id INTO v_config_id
    FROM skill_matrix_legend_configuration
    WHERE company_id = p_company_id;

    IF v_config_id IS NOT NULL THEN
        RAISE NOTICE 'Configuration already exists for company %', p_company_id;
        RETURN v_config_id;
    END IF;

    -- Insert configuration
    INSERT INTO skill_matrix_legend_configuration (company_id, version, created_at)
    VALUES (p_company_id, 1, NOW())
    RETURNING id INTO v_config_id;

    -- Insert mandatory skills
    INSERT INTO skill_matrix_legend_item
        (configuration_id, skill_level_id, skill_type, label, description, icon_color, background_color, sort_order, icon_class, is_default, created_at)
    VALUES
        (v_config_id, 1, 'mandatory', 'Masters the skill', 'User has mastered this mandatory skill', '#008000', '#DDF7DD', 1, 'thumbsup', TRUE, NOW()),
        (v_config_id, 2, 'mandatory', 'Almost expired', 'Skill certification is about to expire', '#FFA500', '#FFF0D4', 2, 'warning', TRUE, NOW()),
        (v_config_id, 3, 'mandatory', 'Expired', 'Skill certification has expired', '#CB0000', '#FFEAEA', 3, 'thumbsdown', TRUE, NOW());

    -- Insert operational skills
    INSERT INTO skill_matrix_legend_item
        (configuration_id, skill_level_id, skill_type, label, description, icon_color, background_color, sort_order, score_value, is_default, created_at)
    VALUES
        (v_config_id, 1, 'operational', 'Doesn''t know the theory', 'User does not have theoretical knowledge', '#CB0000', '#FFEAEA', 1, 1, TRUE, NOW()),
        (v_config_id, 2, 'operational', 'Knows the theory', 'User has theoretical knowledge', '#FF4500', '#FFE4DA', 2, 2, TRUE, NOW()),
        (v_config_id, 3, 'operational', 'Is able to apply this in the standard situations', 'User can apply skill in standard conditions', '#FFA500', '#FFF0D4', 3, 3, TRUE, NOW()),
        (v_config_id, 4, 'operational', 'Is able to apply this in the non-standard conditions', 'User can apply skill in non-standard conditions', '#8DA304', '#F2F5DD', 4, 4, TRUE, NOW()),
        (v_config_id, 5, 'operational', 'Can educate others', 'User can train and educate other team members', '#008000', '#DDF7DD', 5, 5, TRUE, NOW());

    RAISE NOTICE 'Created default legend configuration with ID % for company %', v_config_id, p_company_id;
    RETURN v_config_id;
END;
$$ LANGUAGE plpgsql;

-- Comment on function
COMMENT ON FUNCTION create_default_skill_matrix_legend(INTEGER) IS 'Creates default Skills Matrix Legend configuration for a company if it does not exist';

-- Example usage (uncomment and modify company_id as needed):
-- SELECT create_default_skill_matrix_legend(1);
-- SELECT create_default_skill_matrix_legend(2);

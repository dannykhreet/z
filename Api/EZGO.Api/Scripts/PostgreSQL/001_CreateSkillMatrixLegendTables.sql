-- Skills Matrix Legend Table and Stored Procedures

-- Single table for legend items (per company)
CREATE TABLE IF NOT EXISTS skill_matrix_legend_item (
    id SERIAL PRIMARY KEY,
    company_id INTEGER NOT NULL,
    skill_level_id INTEGER NOT NULL,
    skill_type VARCHAR(20) NOT NULL,
    label VARCHAR(255) NULL,
    description VARCHAR(500) NULL,
    icon_color VARCHAR(7) NULL,
    background_color VARCHAR(7) NULL,
    sort_order INTEGER NOT NULL,
    score_value INTEGER NULL,
    icon_class VARCHAR(50) NULL,
    is_default BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NULL,
    created_by INTEGER NULL,
    updated_by INTEGER NULL
);

-- Stored Procedures

DROP FUNCTION IF EXISTS public.get_skill_matrix_legend_items(int4, varchar);

CREATE OR REPLACE FUNCTION public.get_skill_matrix_legend_items(_company_id integer, _skill_type varchar(20))
 RETURNS TABLE(id integer, company_id integer, skill_level_id integer, skill_type varchar(20), label varchar(255), description varchar(500), icon_color varchar(7), background_color varchar(7), sort_order integer, score_value integer, icon_class varchar(50), is_default boolean, created_at timestamp with time zone, updated_at timestamp with time zone, created_by integer, updated_by integer)
 LANGUAGE plpgsql
 STABLE
AS $function$
BEGIN
    RETURN QUERY
    SELECT
        i.id,
        i.company_id,
        i.skill_level_id,
        i.skill_type,
        i.label,
        i.description,
        i.icon_color,
        i.background_color,
        i.sort_order,
        i.score_value,
        i.icon_class,
        i.is_default,
        i.created_at,
        i.updated_at,
        i.created_by,
        i.updated_by
    FROM skill_matrix_legend_item i
    WHERE i.company_id = _company_id
      AND i.skill_type = _skill_type
    ORDER BY i.sort_order;
END$function$
;


DROP FUNCTION IF EXISTS public.check_skill_matrix_legend_exists(int4);

CREATE OR REPLACE FUNCTION public.check_skill_matrix_legend_exists(_company_id integer)
 RETURNS boolean
 LANGUAGE plpgsql
 STABLE
AS $function$
BEGIN
    RETURN EXISTS (
        SELECT 1 FROM skill_matrix_legend_item WHERE company_id = _company_id
    );
END$function$
;


DROP FUNCTION IF EXISTS public.insert_skill_matrix_legend_item(int4, int4, varchar, varchar, varchar, varchar, varchar, int4, int4, varchar, bool, int4);

CREATE OR REPLACE FUNCTION public.insert_skill_matrix_legend_item(_company_id integer, _skill_level_id integer, _skill_type varchar(20), _label varchar(255), _description varchar(500), _icon_color varchar(7), _background_color varchar(7), _sort_order integer, _score_value integer, _icon_class varchar(50), _is_default boolean, _created_by integer)
 RETURNS integer
 LANGUAGE plpgsql
AS $function$
DECLARE
    v_id integer;
BEGIN
    INSERT INTO skill_matrix_legend_item (company_id, skill_level_id, skill_type, label, description, icon_color, background_color, sort_order, score_value, icon_class, is_default, created_at, created_by)
    VALUES (_company_id, _skill_level_id, _skill_type, _label, _description, _icon_color, _background_color, _sort_order, _score_value, _icon_class, _is_default, NOW(), _created_by)
    RETURNING id INTO v_id;
    RETURN v_id;
END$function$
;


DROP FUNCTION IF EXISTS public.update_skill_matrix_legend_item(int4, int4, varchar, varchar, varchar, varchar, varchar, int4, int4, varchar, int4);

CREATE OR REPLACE FUNCTION public.update_skill_matrix_legend_item(_company_id integer, _skill_level_id integer, _skill_type varchar(20), _label varchar(255), _description varchar(500), _icon_color varchar(7), _background_color varchar(7), _sort_order integer, _score_value integer, _icon_class varchar(50), _updated_by integer)
 RETURNS boolean
 LANGUAGE plpgsql
AS $function$
BEGIN
    UPDATE skill_matrix_legend_item
    SET label = _label,
        description = _description,
        icon_color = _icon_color,
        background_color = _background_color,
        sort_order = _sort_order,
        score_value = _score_value,
        icon_class = _icon_class,
        is_default = FALSE,
        updated_at = NOW(),
        updated_by = _updated_by
    WHERE company_id = _company_id
      AND skill_level_id = _skill_level_id
      AND skill_type = _skill_type;
    RETURN FOUND;
END$function$
;

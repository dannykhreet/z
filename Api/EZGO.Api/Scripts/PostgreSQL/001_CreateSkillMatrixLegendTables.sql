-- Skills Matrix Legend Tables and Stored Procedures

-- Tables
CREATE TABLE IF NOT EXISTS skill_matrix_legend_configuration (
    id SERIAL PRIMARY KEY,
    company_id INTEGER NOT NULL UNIQUE,
    version INTEGER NOT NULL DEFAULT 1,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NULL,
    created_by INTEGER NULL,
    updated_by INTEGER NULL
);

CREATE TABLE IF NOT EXISTS skill_matrix_legend_item (
    id SERIAL PRIMARY KEY,
    configuration_id INTEGER NOT NULL REFERENCES skill_matrix_legend_configuration(id) ON DELETE CASCADE,
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
    updated_at TIMESTAMP WITH TIME ZONE NULL
);

-- Stored Procedures

DROP FUNCTION IF EXISTS public.get_skill_matrix_legend_configuration(int4);

CREATE OR REPLACE FUNCTION public.get_skill_matrix_legend_configuration(_company_id integer)
 RETURNS TABLE(id integer, company_id integer, version integer, created_at timestamp with time zone, updated_at timestamp with time zone, created_by integer, updated_by integer)
 LANGUAGE plpgsql
 STABLE
AS $function$
BEGIN
    RETURN QUERY
    SELECT
        c.id,
        c.company_id,
        c.version,
        c.created_at,
        c.updated_at,
        c.created_by,
        c.updated_by
    FROM skill_matrix_legend_configuration c
    WHERE c.company_id = _company_id;
END$function$
;


DROP FUNCTION IF EXISTS public.get_skill_matrix_legend_items(int4, varchar);

CREATE OR REPLACE FUNCTION public.get_skill_matrix_legend_items(_configuration_id integer, _skill_type varchar(20))
 RETURNS TABLE(id integer, configuration_id integer, skill_level_id integer, skill_type varchar(20), label varchar(255), description varchar(500), icon_color varchar(7), background_color varchar(7), sort_order integer, score_value integer, icon_class varchar(50), is_default boolean, created_at timestamp with time zone, updated_at timestamp with time zone)
 LANGUAGE plpgsql
 STABLE
AS $function$
BEGIN
    RETURN QUERY
    SELECT
        i.id,
        i.configuration_id,
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
        i.updated_at
    FROM skill_matrix_legend_item i
    WHERE i.configuration_id = _configuration_id
      AND i.skill_type = _skill_type
    ORDER BY i.sort_order;
END$function$
;


DROP FUNCTION IF EXISTS public.insert_skill_matrix_legend_configuration(int4, int4, int4);

CREATE OR REPLACE FUNCTION public.insert_skill_matrix_legend_configuration(_company_id integer, _version integer, _created_by integer)
 RETURNS integer
 LANGUAGE plpgsql
AS $function$
DECLARE
    v_id integer;
BEGIN
    INSERT INTO skill_matrix_legend_configuration (company_id, version, created_at, created_by)
    VALUES (_company_id, _version, NOW(), _created_by)
    RETURNING id INTO v_id;
    RETURN v_id;
END$function$
;


DROP FUNCTION IF EXISTS public.update_skill_matrix_legend_configuration(int4, int4);

CREATE OR REPLACE FUNCTION public.update_skill_matrix_legend_configuration(_company_id integer, _updated_by integer)
 RETURNS boolean
 LANGUAGE plpgsql
AS $function$
BEGIN
    UPDATE skill_matrix_legend_configuration
    SET updated_at = NOW(),
        updated_by = _updated_by
    WHERE company_id = _company_id;
    RETURN FOUND;
END$function$
;


DROP FUNCTION IF EXISTS public.insert_skill_matrix_legend_item(int4, int4, varchar, varchar, varchar, varchar, varchar, int4, int4, varchar, bool);

CREATE OR REPLACE FUNCTION public.insert_skill_matrix_legend_item(_configuration_id integer, _skill_level_id integer, _skill_type varchar(20), _label varchar(255), _description varchar(500), _icon_color varchar(7), _background_color varchar(7), _sort_order integer, _score_value integer, _icon_class varchar(50), _is_default boolean)
 RETURNS integer
 LANGUAGE plpgsql
AS $function$
DECLARE
    v_id integer;
BEGIN
    INSERT INTO skill_matrix_legend_item (configuration_id, skill_level_id, skill_type, label, description, icon_color, background_color, sort_order, score_value, icon_class, is_default, created_at)
    VALUES (_configuration_id, _skill_level_id, _skill_type, _label, _description, _icon_color, _background_color, _sort_order, _score_value, _icon_class, _is_default, NOW())
    RETURNING id INTO v_id;
    RETURN v_id;
END$function$
;


DROP FUNCTION IF EXISTS public.update_skill_matrix_legend_item(int4, int4, varchar, varchar, varchar, varchar, varchar, int4, int4, varchar);

CREATE OR REPLACE FUNCTION public.update_skill_matrix_legend_item(_company_id integer, _skill_level_id integer, _skill_type varchar(20), _label varchar(255), _description varchar(500), _icon_color varchar(7), _background_color varchar(7), _sort_order integer, _score_value integer, _icon_class varchar(50))
 RETURNS boolean
 LANGUAGE plpgsql
AS $function$
BEGIN
    UPDATE skill_matrix_legend_item i
    SET label = _label,
        description = _description,
        icon_color = _icon_color,
        background_color = _background_color,
        sort_order = _sort_order,
        score_value = _score_value,
        icon_class = _icon_class,
        is_default = FALSE,
        updated_at = NOW()
    FROM skill_matrix_legend_configuration c
    WHERE i.configuration_id = c.id
      AND c.company_id = _company_id
      AND i.skill_level_id = _skill_level_id
      AND i.skill_type = _skill_type;
    RETURN FOUND;
END$function$
;

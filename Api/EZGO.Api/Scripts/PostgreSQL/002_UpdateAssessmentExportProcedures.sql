-- Assessment Export Stored Procedures Update
-- This script updates existing export procedures and creates a new one for assessment instruction tags
--
-- Changes:
-- 1. export_data_assessments_overview: Add Assessor(s) column, change Started fields from created_at to start_at
-- 2. export_data_assessmentitems_overview: Add Assessors column, change Started fields from created_at to start_at
-- 3. export_data_assessmentinstructionitems_overview: Replace Started/Done with MarkedOn (completed_at)
-- 4. export_data_assessments_tags_overview: Add AssessmentId, filter only completed assessments
-- 5. NEW: export_data_assessmentinstructions_tags_overview: Tags for assessment instructions

-- =============================================================================
-- 1. UPDATE export_data_assessments_overview
-- Add "Assessor(s)" column between Assessor and Assessee
-- Change "Started..." fields from created_at to start_at
-- =============================================================================
DROP FUNCTION public.export_data_assessments_overview(int4, timestamp, timestamp);

CREATE OR REPLACE FUNCTION public.export_data_assessments_overview(_companyid integer, _starttimestamp timestamp without time zone DEFAULT NULL::timestamp without time zone, _endtimestamp timestamp without time zone DEFAULT NULL::timestamp without time zone)
 RETURNS TABLE("AssessmentID" integer, "AssessmentTemplateID" integer, "Assessment Name" character varying, "NrOfAssessmentInstructions" integer, "AreaID" integer, "AreaName" character varying, "Assessor" character varying,"Assessor(s)" text, "Assessee" character varying, "TotalScore" integer, "AverageScore" double precision, "StartedDateTime" timestamp without time zone, "StartedDate" date, "StartedTime" character varying, "StartedWeekDay" integer, "StartedWeekNr" integer, "StartedMonth" integer, "StartedYear" integer, "DoneDateTime" timestamp without time zone, "DoneDate" date, "DoneTime" character varying, "DoneWeekDay" integer, "DoneWeekNr" integer, "DoneMonth" integer, "DoneYear" integer)
 LANGUAGE plpgsql
 STABLE
AS $function$
DECLARE
	_timezone varchar;
BEGIN
	SELECT get_timezone_with_company(_companyid) INTO _timezone;
	RETURN QUERY
	SELECT
		AM.id AS AssessmentID,--AssessmentID
		AMT.id AS AssessmentTemplateID,--AssessmentTemplateID
		AMT.name AS "Assessment Name", --Assessment Name
		(SELECT COUNT(AMSI.id) FROM assessment_skillinstructions AMSI WHERE AMSI.assessment_id = AM.id AND AMSI.is_active = true)::int AS NrOfAssessmentInstructions, -- NrOfAssessmentInstructions
		AMT.area_id AS AreaID, --AreaID
		CA.name AS AreaName, --AreaName

		CONCAT(PUA.first_name, ' ',PUA.last_name)::varchar AS Assessor, --Assessor
	 	COALESCE((
			SELECT string_agg(DISTINCT TRIM(CONCAT(u.first_name, ' ', u.last_name)), ', ' ORDER BY TRIM(CONCAT(u.first_name, ' ', u.last_name)))
			FROM assessment_skillinstruction_items asii
			JOIN profiles_user u ON u.id = asii.assessor_id AND u.company_id = _companyid
			WHERE asii.company_id = _companyid
				AND asii.assessment_id = AM.id
				AND asii.assessor_id IS NOT NULL
		), '') AS "Assessor(s)",
		CONCAT(PUC.first_name, ' ',PUC.last_name)::varchar AS Assessee,--Assessee

		AM.total_score AS TotalScore, --TotalScore
		(AM.calculated_score)::float AS AverageScore,--AverageScore

		date_trunc('second', timezone(_timezone, ASI_START.start_at::timestamptz)::timestamp) AS StartedDateTime, --StartedDateTime
		timezone(_timezone, ASI_START.start_at::timestamptz)::date AS StartedDate, --StartedDate
		TO_CHAR(timezone(_timezone, ASI_START.start_at::timestamptz), 'HH24:MI')::varchar AS StartedTime, --StartedTime
		TO_CHAR(timezone(_timezone, ASI_START.start_at::timestamptz), 'D')::int AS StartedWeekDay, --StartedWeekDay
		TO_CHAR(timezone(_timezone, ASI_START.start_at::timestamptz), 'IW')::int As StartedWeekNr, --StartedWeekNr
		TO_CHAR(timezone(_timezone, ASI_START.start_at::timestamptz), 'MM')::int As StartedMonth, --StartedMonth
		TO_CHAR(timezone(_timezone, ASI_START.start_at::timestamptz), 'YYYY')::int As StartedYear, --StartedYear

		date_trunc('second', timezone(_timezone, AM.completed_at::timestamptz)::timestamp) AS DoneDateTime, --DoneDateTime
		timezone(_timezone, AM.completed_at::timestamptz)::date AS DoneDate, --DoneDate
		TO_CHAR(timezone(_timezone, AM.completed_at::timestamptz), 'HH24:MI')::varchar AS DoneTime, --DoneTime
		TO_CHAR(timezone(_timezone, AM.completed_at::timestamptz), 'D')::int AS DoneWeekDay, --DoneWeekDay
		TO_CHAR(timezone(_timezone, AM.completed_at::timestamptz), 'IW')::int As DoneWeekNr, --DoneWeekNr
		TO_CHAR(timezone(_timezone, AM.completed_at::timestamptz), 'MM')::int As DoneMonth, --DoneMonth
		TO_CHAR(timezone(_timezone, AM.completed_at::timestamptz), 'YYYY')::int As DoneYear --DoneYear
		FROM assessments AM
		INNER JOIN assessment_templates AMT ON AMT.id = AM.assessment_template_id
		INNER JOIN companies_area CA ON CA.id = AMT.area_id
		LEFT JOIN profiles_user PUC ON PUC.id = AM.completed_for_id AND PUC.company_id = _companyid
		LEFT JOIN profiles_user PUA ON PUA.id = AM.assessor_id AND PUA.company_id = _companyid
		LEFT JOIN LATERAL (
			SELECT MIN(ASI.start_at)::timestamp AS start_at
			FROM assessment_skillinstructions ASI
			WHERE ASI.assessment_id = AM.id
			  AND ASI.company_id = _companyid
			  AND ASI.is_active = true
		) AS ASI_START ON true
		WHERE AMT.company_id = _companyid AND AM.company_id = _companyid AND CA.company_id = _companyid AND AM.is_completed = true
		AND (AM.completed_at >= timezone(_timezone, _starttimestamp::timestamp) OR _starttimestamp IS NULL)
		AND (AM.completed_at <= timezone(_timezone, _endtimestamp::timestamp) OR _endtimestamp IS NULL)
		ORDER BY AMT.name, AM.completed_at DESC;
END$function$
;


-- =============================================================================
-- 2. UPDATE export_data_assessmentitems_overview (ASSESSMENT INSTRUCTIONS)
-- Add "Assessors" column
-- Change "Started..." fields from created_at to start_at
-- =============================================================================
 DROP FUNCTION public.export_data_assessmentitems_overview(int4, timestamp, timestamp);

CREATE OR REPLACE FUNCTION public.export_data_assessmentitems_overview(_companyid integer, _starttimestamp timestamp without time zone DEFAULT NULL::timestamp without time zone, _endtimestamp timestamp without time zone DEFAULT NULL::timestamp without time zone)
 RETURNS TABLE("AssessmentID" integer, "AssessmentTemplateID" integer, "Assessment Name" character varying, "NrOfInstructionItems" integer, "AreaID" integer, "AreaName" character varying, "Assessor" character varying, "Assessors" text, "Assessee" character varying, "StartedDateTime" timestamp without time zone, "StartedDate" date, "StartedTime" character varying, "StartedWeekDay" integer, "StartedWeekNr" integer, "StartedMonth" integer, "StartedYear" integer, "DoneDateTime" timestamp without time zone, "DoneDate" date, "DoneTime" character varying, "DoneWeekDay" integer, "DoneWeekNr" integer, "DoneMonth" integer, "DoneYear" integer, "InstructionID" integer, "InstructionTemplateID" integer, "AssessmentInstructionName" character varying, "TotalScore" integer, "AverageScore" double precision)
 LANGUAGE plpgsql
 STABLE
AS $function$
DECLARE
	_timezone varchar;
BEGIN
	SELECT get_timezone_with_company(_companyid) INTO _timezone;
	RETURN QUERY
	SELECT
		AM.id AS AssessmentID,--AssessmentID
		AMT.id AS AssessmentTemplateID,--AssessmentTemplateID
		AMT.name AS "Assessment Name", --Assessment Name
		((SELECT COUNT(AMSII.id) FROM assessment_skillinstruction_items AMSII WHERE AMSII.assessment_skillinstruction_id = AMSI.id AND AMSII.is_active = true))::int AS NrOfInstructionItems, -- NrOfInstructionItems
		AMT.area_id AS AreaID, --AreaID
		CA.name AS AreaName, --AreaName

		CONCAT(PUA.first_name, ' ',PUA.last_name)::varchar AS Assessor, --Assessor

		COALESCE((
			SELECT string_agg(DISTINCT TRIM(CONCAT(u.first_name, ' ', u.last_name)), ', ' ORDER BY TRIM(CONCAT(u.first_name, ' ', u.last_name)))
			FROM assessment_skillinstruction_items asii
			JOIN profiles_user u ON u.id = asii.assessor_id AND u.company_id = _companyid
			WHERE asii.company_id = _companyid
				AND asii.assessment_skillinstruction_id = AMSI.id
				AND asii.assessor_id IS NOT NULL
		), '') AS Assessors,
		CONCAT(PUC.first_name, ' ',PUC.last_name)::varchar AS Assessee,--Assessee

		date_trunc('second', timezone(_timezone, AMSI.start_at::timestamptz)::timestamp) AS StartedDateTime, --StartedDateTime
		timezone(_timezone, AMSI.start_at::timestamptz)::date AS StartedDate, --StartedDate
		TO_CHAR(timezone(_timezone, AMSI.start_at::timestamptz), 'HH24:MI')::varchar AS StartedTime, --StartedTime
		TO_CHAR(timezone(_timezone, AMSI.start_at::timestamptz), 'D')::int AS StartedWeekDay, --StartedWeekDay
		TO_CHAR(timezone(_timezone, AMSI.start_at::timestamptz), 'IW')::int As StartedWeekNr, --StartedWeekNr
		TO_CHAR(timezone(_timezone, AMSI.start_at::timestamptz), 'MM')::int As StartedMonth, --StartedMonth
		TO_CHAR(timezone(_timezone, AMSI.start_at::timestamptz), 'YYYY')::int As StartedYear, --StartedYear

		date_trunc('second', timezone(_timezone, AMSI.completed_at::timestamptz)::timestamp) AS DoneDateTime, --DoneDateTime
		timezone(_timezone, AMSI.completed_at::timestamptz)::date AS DoneDate, --DoneDate
		TO_CHAR(timezone(_timezone, AMSI.completed_at::timestamptz), 'HH24:MI')::varchar AS DoneTime, --DoneTime
		TO_CHAR(timezone(_timezone, AMSI.completed_at::timestamptz), 'D')::int AS DoneWeekDay, --DoneWeekDay
		TO_CHAR(timezone(_timezone, AMSI.completed_at::timestamptz), 'IW')::int As DoneWeekNr, --DoneWeekNr
		TO_CHAR(timezone(_timezone, AMSI.completed_at::timestamptz), 'MM')::int As DoneMonth, --DoneMonth
		TO_CHAR(timezone(_timezone, AMSI.completed_at::timestamptz), 'YYYY')::int As DoneYear, --DoneYear
		AMSI.id AS InstructionID, --InstructionID
		ATSI.workinstruction_template_id AS InstructionTemplateID, --InstructionTemplateID
		WIT.name AS AssessmentInstructionName, --AssessmentInstructionName
		AMSI.total_score AS TotalScore, --TotalScore
		(ROUND((AMSI.total_score / CASE WHEN ((SELECT COUNT(AMSII.id) FROM assessment_skillinstruction_items AMSII WHERE AMSII.assessment_skillinstruction_id = AMSI.id)) != 0 THEN ((SELECT COUNT(AMSII.id) FROM assessment_skillinstruction_items AMSII WHERE AMSII.assessment_skillinstruction_id = AMSI.id)::float) ELSE 1 END
		)::numeric, 2))::float AS AverageScore--AverageScore
	FROM assessment_skillinstructions AMSI
		INNER JOIN assessments AM ON AM.id = AMSI.assessment_id
		INNER JOIN assessment_templates AMT ON AMT.id = AMSI.assessment_template_id
		INNER JOIN companies_area CA ON CA.id = AMT.area_id
		INNER JOIN assessment_template_skillinstructions ATSI ON ATSI.id = AMSI.assessment_template_skillinstruction_id
		INNER JOIN workinstruction_templates WIT ON WIT.id = ATSI.workinstruction_template_id
		LEFT JOIN profiles_user PUC ON PUC.id = AMSI.completed_for_id AND PUC.company_id = _companyid
		LEFT JOIN profiles_user PUA ON PUA.id = AM.assessor_id AND PUA.company_id = _companyid
	WHERE AMT.company_id = _companyid AND AM.company_id = _companyid AND CA.company_id = _companyid AND AM.is_completed = true
		AND AM.is_active AND AMSI.is_active
		AND (AM.completed_at >= timezone(_timezone, _starttimestamp::timestamp) OR _starttimestamp IS NULL)
		AND (AM.completed_at <= timezone(_timezone, _endtimestamp::timestamp) OR _endtimestamp IS NULL)
		ORDER BY AMT.name, AM.completed_at DESC;
END$function$
;


-- =============================================================================
-- 3. UPDATE export_data_assessmentinstructionitems_overview
-- Replace "Started..." and "Done..." fields with single "MarkedOn" field (completed_at)
-- =============================================================================
DROP FUNCTION public.export_data_assessmentinstructionitems_overview(int4, timestamp, timestamp);

CREATE OR REPLACE FUNCTION public.export_data_assessmentinstructionitems_overview(_companyid integer, _starttimestamp timestamp without time zone DEFAULT NULL::timestamp without time zone, _endtimestamp timestamp without time zone DEFAULT NULL::timestamp without time zone)
 RETURNS TABLE("AssessmentID" integer, "AssessmentTemplateID" integer, "Assessment Name" character varying, "InstructionID" integer, "InstructionTemplateID" integer, "AssessmentInstructionName" character varying, "AreaID" integer, "AreaName" character varying, "Assessor" character varying, "Assessee" character varying, "MarkedOnDateTime" timestamp without time zone, "MarkedOnDate" date, "MarkedOnTime" character varying, "MarkedOnWeekDay" integer, "MarkedOnWeekNr" integer, "MarkedOnMonth" integer, "MarkedOnYear" integer, "ItemID" integer, "ItemTemplateID" integer, "ItemName" character varying, "Score" integer)
 LANGUAGE plpgsql
 STABLE
AS $function$
DECLARE
	_timezone varchar;
BEGIN
	SELECT get_timezone_with_company(_companyid) INTO _timezone;
	RETURN QUERY
	SELECT
		AM.id AS AssessmentID,--AssessmentID
		AMT.id AS AssessmentTemplateID,--AssessmentTemplateID
		AMT.name AS "Assessment Name", --Assessment Name
		AMSI.id AS InstructionID, --InstructionID
		ATSI.workinstruction_template_id AS InstructionTemplateID, --InstructionTemplateID
		WIT.name AS AssessmentInstructionName, --AssessmentInstruction Name
		AMT.area_id AS AreaID, --AreaID
		CA.name AS AreaName, --AreaName

		CONCAT(PUA.first_name, ' ',PUA.last_name)::varchar AS Assessor, --Assessor
		CONCAT(PUC.first_name, ' ',PUC.last_name)::varchar AS Assessee,--Assessee


		date_trunc('second', timezone(_timezone, AMSII.completed_at::timestamptz)::timestamp) AS MarkedOnDateTime, --MarkedOnDateTime
		timezone(_timezone, AMSII.completed_at::timestamptz)::date AS MarkedOnDate, --MarkedOnDate
		TO_CHAR(timezone(_timezone, AMSII.completed_at::timestamptz), 'HH24:MI')::varchar AS MarkedOnTime, --MarkedOnTime
		TO_CHAR(timezone(_timezone, AMSII.completed_at::timestamptz), 'D')::int AS MarkedOnWeekDay, --MarkedOnWeekDay
		TO_CHAR(timezone(_timezone, AMSII.completed_at::timestamptz), 'IW')::int As MarkedOnWeekNr, --MarkedOnWeekNr
		TO_CHAR(timezone(_timezone, AMSII.completed_at::timestamptz), 'MM')::int As MarkedOnMonth, --MarkedOnMonth
		TO_CHAR(timezone(_timezone, AMSII.completed_at::timestamptz), 'YYYY')::int As MarkedOnYear, --MarkedOnYear

		AMSII.id AS ItemID, --ItemID
		AMSII.workinstruction_template_item_id AS ItemTemplateID, --ItemTemplateID
		WITI.name AS ItemName, --ItemName
		AMSII.score AS Score
	FROM assessment_skillinstruction_items AMSII
		INNER JOIN assessment_skillinstructions AMSI ON AMSI.id = AMSII.assessment_skillinstruction_id
		INNER JOIN assessments AM ON AM.id = AMSI.assessment_id
		INNER JOIN assessment_templates AMT ON AMT.id = AMSI.assessment_template_id
		INNER JOIN companies_area CA ON CA.id = AMT.area_id
		INNER JOIN assessment_template_skillinstructions ATSI ON ATSI.id = AMSI.assessment_template_skillinstruction_id
		INNER JOIN workinstruction_templates WIT ON WIT.id = ATSI.workinstruction_template_id
		INNER JOIN workinstruction_template_items WITI ON AMSII.workinstruction_template_item_id = WITI.id
		LEFT JOIN profiles_user PUC ON PUC.id = AMSI.completed_for_id AND PUC.company_id = _companyid
		LEFT JOIN profiles_user PUA ON PUA.id = AM.assessor_id AND PUA.company_id = _companyid
	WHERE AMT.company_id = _companyid AND AM.company_id = _companyid AND CA.company_id = _companyid AND AM.is_completed = true
		AND AM.is_active AND AMSI.is_active AND AMSII.is_active
		AND (AM.completed_at >= timezone(_timezone, _starttimestamp::timestamp) OR _starttimestamp IS NULL)
		AND (AM.completed_at <= timezone(_timezone, _endtimestamp::timestamp) OR _endtimestamp IS NULL)
		ORDER BY AMT.name, AM.completed_at DESC;
END$function$
;


-- =============================================================================
-- 4. UPDATE export_data_assessments_tags_overview
-- Add AssessmentId column
-- Filter to only return tags for COMPLETED assessments
-- =============================================================================
DROP FUNCTION public.export_data_assessments_tags_overview(int4, timestamp, timestamp);

CREATE OR REPLACE FUNCTION public.export_data_assessments_tags_overview(_companyid integer, _starttimestamp timestamp without time zone DEFAULT NULL::timestamp without time zone, _endtimestamp timestamp without time zone DEFAULT NULL::timestamp without time zone)
 RETURNS TABLE("AssessmentId" integer, "TagId" integer, "TagName" character varying, "IsSystemTag" boolean, "IsHoldingTag" boolean, "TagGroupId" integer, "TagGroupName" character varying, "AssessmentTemplateId" integer, "AssessmentTemplateName" character varying, "AreaId" integer, "AreaName" character varying, "StartedDateTime" timestamp without time zone, "DoneDateTime" timestamp without time zone)
 LANGUAGE plpgsql
 STABLE
AS $function$
DECLARE
	_timezone varchar;
BEGIN
	SELECT get_timezone_with_company(_companyid) INTO _timezone;
	RETURN QUERY
	SELECT
		A."id" AS "AssessmentId",
		T."id" AS "TagId",
		T."name" AS "TagName",
		T.is_system_tag AS "IsSystemTag",
		T.is_holding_tag AS "IsHoldingTag",
		TG."id" AS "TagGroupId",
		TG."name" AS "TagGroupName",
		AT."id" AS "AssessmentTemplateId",
		AT."name" AS "AssessmentTemplateName",
		AT.area_id AS "AreaId",
		CA."name" AS "AreaName",
		date_trunc('second', timezone(_timezone, A.created_at::timestamptz)::timestamp) AS StartedDateTime,
		date_trunc('second', timezone(_timezone, A.completed_at::timestamptz)::timestamp) AS DoneDateTime
	FROM
		tags_tag T
		INNER JOIN tags_tag_relation TR ON TR.tag_id = T."id" AND TR.company_id = _companyid AND TR.assessment_id IS NOT NULL
		INNER JOIN tags_taggroup_tags TGT ON TGT.tag_id = T."id"
		INNER JOIN tags_taggroup TG ON TG."id" = TGT.taggroup_id
		INNER JOIN assessments A ON A.id = TR.assessment_id AND A.company_id = _companyid
		INNER JOIN assessment_templates AT ON AT."id" = A.assessment_template_id AND AT.company_id = _companyid
		INNER JOIN companies_area CA ON CA."id" = AT.area_id AND CA.company_id = _companyid
	WHERE
		T.is_active = TRUE
		AND A.is_active
		AND A.is_completed = TRUE
		AND (A.completed_at >= timezone(_timezone, _starttimestamp::timestamp) OR _starttimestamp IS NULL)
		AND (A.completed_at <= timezone(_timezone, _endtimestamp::timestamp) OR _endtimestamp IS NULL)
	ORDER BY AT.name, T."name" ASC;
END$function$
;


-- =============================================================================
-- 5. NEW: export_data_assessmentinstructions_tags_overview
-- New tab for assessment instruction tags (based on assessment tags structure)
-- Fields: InstructionTemplateId, AssessmentName, AssessmentId, InstructionName, InstructionId + tag fields
-- =============================================================================
-- DROP FUNCTION public.export_data_assessmentinstructions_tags_overview(int4, timestamp, timestamp);

CREATE OR REPLACE FUNCTION public.export_data_assessmentinstructions_tags_overview(_companyid integer, _starttimestamp timestamp without time zone DEFAULT NULL::timestamp without time zone, _endtimestamp timestamp without time zone DEFAULT NULL::timestamp without time zone)
 RETURNS TABLE("InstructionTemplateId" integer, "AssessmentName" character varying, "AssessmentId" integer, "InstructionName" character varying, "InstructionId" integer, "TagId" integer, "TagName" character varying, "IsSystemTag" boolean, "IsHoldingTag" boolean, "TagGroupId" integer, "TagGroupName" character varying, "AssessmentTemplateId" integer, "AssessmentTemplateName" character varying, "AreaId" integer, "AreaName" character varying, "StartedDateTime" timestamp without time zone, "DoneDateTime" timestamp without time zone)
 LANGUAGE plpgsql
 STABLE
AS $function$
DECLARE
	_timezone varchar;
BEGIN
	SELECT get_timezone_with_company(_companyid) INTO _timezone;
	RETURN QUERY
	SELECT
		ATSI.workinstruction_template_id AS "InstructionTemplateId",
		AT."name" AS "AssessmentName",
		A."id" AS "AssessmentId",
		WIT."name" AS "InstructionName",
		AMSI."id" AS "InstructionId",
		T."id" AS "TagId",
		T."name" AS "TagName",
		T.is_system_tag AS "IsSystemTag",
		T.is_holding_tag AS "IsHoldingTag",
		TG."id" AS "TagGroupId",
		TG."name" AS "TagGroupName",
		AT."id" AS "AssessmentTemplateId",
        AT."name" AS "AssessmentTemplateName",
		AT.area_id AS "AreaId",
		CA."name" AS "AreaName",
        date_trunc('second', timezone(_timezone, A.created_at::timestamptz)::timestamp) AS StartedDateTime,
        date_trunc('second', timezone(_timezone, A.completed_at::timestamptz)::timestamp) AS DoneDateTime
	FROM
		tags_tag T
		INNER JOIN tags_tag_relation TR ON TR.tag_id = T."id" AND TR.company_id = _companyid AND TR.assessment_id IS NOT NULL
		INNER JOIN tags_taggroup_tags TGT ON TGT.tag_id = T."id"
		INNER JOIN tags_taggroup TG ON TG."id" = TGT.taggroup_id
		INNER JOIN assessments A ON A.id = TR.assessment_id AND A.company_id = _companyid
		INNER JOIN assessment_templates AT ON AT."id" = A.assessment_template_id AND AT.company_id = _companyid
		INNER JOIN companies_area CA ON CA."id" = AT.area_id AND CA.company_id = _companyid
		INNER JOIN assessment_skillinstructions AMSI ON AMSI.assessment_id = A.id AND AMSI.company_id = _companyid AND AMSI.is_active = true
		INNER JOIN assessment_template_skillinstructions ATSI ON ATSI.id = AMSI.assessment_template_skillinstruction_id
		INNER JOIN workinstruction_templates WIT ON WIT.id = ATSI.workinstruction_template_id
	WHERE
		T.is_active = TRUE
		AND A.is_active
		AND A.is_completed = TRUE
		AND (A.completed_at >= timezone(_timezone, _starttimestamp::timestamp) OR _starttimestamp IS NULL)
		AND (A.completed_at <= timezone(_timezone, _endtimestamp::timestamp) OR _endtimestamp IS NULL)
	ORDER BY AT.name, WIT.name, T."name" ASC;
END$function$
;

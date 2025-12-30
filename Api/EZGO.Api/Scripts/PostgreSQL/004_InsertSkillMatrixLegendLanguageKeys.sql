-- =============================================
-- Script: 004_InsertSkillMatrixLegendLanguageKeys.sql
-- Description: Insert translation keys for Skill Matrix Legend feature
-- Supported languages: en_us, en_gb, nl_nl, pl_pl, de_de
-- =============================================

-- Setting category translations
INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5001, 'sml001a1b2c3d4e5f6g7h85001', 'CMS_SETTING_ENABLE_SKILL_MATRIX_TITLE', 2, 'Round down', 'Round down', 'Afronden naar beneden', 'Zaokrąglij w dół', 'Abrunden');

INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5002, 'sml002a1b2c3d4e5f6g7h85002', 'CMS_SETTING_ENABLE_SKILL_MATRIX_DESCRIPTION', 2, 'The score from an assessment will always be rounded down.', 'The score from an assessment will always be rounded down.', 'De score van een beoordeling wordt altijd naar beneden afgerond.', 'Wynik z oceny będzie zawsze zaokrąglany w dół.', 'Die Punktzahl aus einer Bewertung wird immer abgerundet.');

INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5003, 'sml003a1b2c3d4e5f6g7h85003', 'CMS_SETTING_DISABLE_SKILL_MATRIX_TITLE', 2, 'Standard rounding', 'Standard rounding', 'Standaard afronding', 'Standardowe zaokrąglanie', 'Standardrundung');

INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5004, 'sml004a1b2c3d4e5f6g7h85004', 'CMS_SETTING_DISABLE_SKILL_MATRIX_DESCRIPTION', 2, 'Scores below .5 round down, scores of .5 or above round up.', 'Scores below .5 round down, scores of .5 or above round up.', 'Scores onder .5 worden afgerond naar beneden, scores van .5 of hoger worden afgerond naar boven.', 'Wyniki poniżej 0,5 są zaokrąglane w dół, wyniki 0,5 lub wyższe są zaokrąglane w górę.', 'Werte unter 0,5 werden abgerundet, Werte ab 0,5 werden aufgerundet.');

INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5005, 'sml005a1b2c3d4e5f6g7h85005', 'CMS_SETTING_CUSTOMIZE_LEGEND_TITLE', 2, 'Customize skill color codes and skill level text', 'Customise skill colour codes and skill level text', 'Pas vaardigheidskleurcodes en vaardigheidsniveautekst aan', 'Dostosuj kody kolorów umiejętności i tekst poziomu umiejętności', 'Fähigkeitsfarbcodes und Fähigkeitsstufentext anpassen');

INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5006, 'sml006a1b2c3d4e5f6g7h85006', 'CMS_SETTING_MANDATORY_SKILLS_TITLE', 2, 'Mandatory skills', 'Mandatory skills', 'Verplichte vaardigheden', 'Umiejętności obowiązkowe', 'Pflichtfähigkeiten');

INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5007, 'sml007a1b2c3d4e5f6g7h85007', 'CMS_SETTING_OPERATIONAL_SKILLS_TITLE', 2, 'Operational skills', 'Operational skills', 'Operationele vaardigheden', 'Umiejętności operacyjne', 'Operative Fähigkeiten');

INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5008, 'sml008a1b2c3d4e5f6g7h85008', 'CMS_SETTING_CUSTOMIZE_LEGEND_ITEM_TITLE', 2, 'Customize skill level text and color codes for', 'Customise skill level text and colour codes for', 'Pas vaardigheidsniveautekst en kleurcodes aan voor', 'Dostosuj tekst poziomu umiejętności i kody kolorów dla', 'Fähigkeitsstufentext und Farbcodes anpassen für');

INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5009, 'sml009a1b2c3d4e5f6g7h85009', 'CMS_SETTING_LEVEL_TEXT_LABEL', 2, 'Level text', 'Level text', 'Niveautekst', 'Tekst poziomu', 'Stufentext');

INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5010, 'sml010a1b2c3d4e5f6g7h85010', 'CMS_SETTING_COLOR_OF_NUMBER_LABEL', 2, 'Color of the number', 'Colour of the number', 'Kleur van het nummer', 'Kolor numeru', 'Farbe der Zahl');

INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5011, 'sml011a1b2c3d4e5f6g7h85011', 'CMS_SETTING_COLOR_OF_BACKGROUND_LABEL', 2, 'Color of the background', 'Colour of the background', 'Kleur van de achtergrond', 'Kolor tła', 'Hintergrundfarbe');

-- Skills category translations (skill levels)
INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5012, 'sml012a1b2c3d4e5f6g7h85012', 'CMS_SKILLS_VALUE_MASTERS_THE_SKILL', 2, 'Masters the skill', 'Masters the skill', 'Beheerst de vaardigheid', 'Opanował umiejętność', 'Beherrscht die Fähigkeit');

INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5013, 'sml013a1b2c3d4e5f6g7h85013', 'CMS_SKILLS_VALUE_ALMOST_EXPIRED', 2, 'Almost expired', 'Almost expired', 'Bijna verlopen', 'Prawie wygasło', 'Fast abgelaufen');

INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5014, 'sml014a1b2c3d4e5f6g7h85014', 'CMS_SKILLS_VALUE_EXPIRED', 2, 'Expired', 'Expired', 'Verlopen', 'Wygasło', 'Abgelaufen');

INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5015, 'sml015a1b2c3d4e5f6g7h85015', 'CMS_SKILLS_VALUE_NO_KNOW_THE_THEORY', 2, 'Doesn''t know the theory', 'Doesn''t know the theory', 'Kent de theorie niet', 'Nie zna teorii', 'Kennt die Theorie nicht');

INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5016, 'sml016a1b2c3d4e5f6g7h85016', 'CMS_SKILLS_VALUE_KNOW_THE_THEORY', 2, 'Knows the theory', 'Knows the theory', 'Kent de theorie', 'Zna teorię', 'Kennt die Theorie');

INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5017, 'sml017a1b2c3d4e5f6g7h85017', 'CMS_SKILLS_VALUE_STANDARD_SITUATIONS', 2, 'Is able to apply this in the standard situations', 'Is able to apply this in the standard situations', 'Is in staat dit toe te passen in standaardsituaties', 'Potrafi zastosować to w standardowych sytuacjach', 'Kann dies in Standardsituationen anwenden');

INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5018, 'sml018a1b2c3d4e5f6g7h85018', 'CMS_SKILLS_VALUE_NON_STANDARD_CONDITIONS', 2, 'Is able to apply this in the non-standard conditions', 'Is able to apply this in the non-standard conditions', 'Is in staat dit toe te passen in niet-standaard omstandigheden', 'Potrafi zastosować to w niestandardowych warunkach', 'Kann dies unter nicht standardmäßigen Bedingungen anwenden');

INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5019, 'sml019a1b2c3d4e5f6g7h85019', 'CMS_SKILLS_VALUE_CAN_EDUCATE_OTHERS', 2, 'Can educate others', 'Can educate others', 'Kan anderen opleiden', 'Może szkolić innych', 'Kann andere ausbilden');

-- Shared button translations
INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5020, 'sml020a1b2c3d4e5f6g7h85020', 'CMS_SHARED_BTN_CLOSE', 2, 'Close', 'Close', 'Sluiten', 'Zamknij', 'Schließen');

INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5021, 'sml021a1b2c3d4e5f6g7h85021', 'CMS_SHARED_BTN_CONFIRM', 2, 'Confirm', 'Confirm', 'Bevestigen', 'Potwierdź', 'Bestätigen');

-- Company setting notification translations
INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5022, 'sml022a1b2c3d4e5f6g7h85022', 'CMS_COMPANY_SETTING_NOTIFY_SKILL_MATRIX_CHANGE_DONE', 2, 'Skill matrix setting updated correctly', 'Skill matrix setting updated correctly', 'Vaardigheidsmatrix-instelling correct bijgewerkt', 'Ustawienie matrycy umiejętności zaktualizowane poprawnie', 'Fähigkeitsmatrix-Einstellung erfolgreich aktualisiert');

INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5023, 'sml023a1b2c3d4e5f6g7h85023', 'CMS_COMPANY_SETTING_LEGEND_UPDATED_SUCCESSFULLY', 2, 'Legend configuration updated successfully', 'Legend configuration updated successfully', 'Legenda-configuratie succesvol bijgewerkt', 'Konfiguracja legendy zaktualizowana pomyślnie', 'Legendenkonfiguration erfolgreich aktualisiert');

INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5024, 'sml024a1b2c3d4e5f6g7h85024', 'CMS_COMPANY_SETTING_LEGEND_UPDATE_FAILED', 2, 'Failed to update legend configuration', 'Failed to update legend configuration', 'Het bijwerken van de legenda-configuratie is mislukt', 'Nie udało się zaktualizować konfiguracji legendy', 'Aktualisierung der Legendenkonfiguration fehlgeschlagen');

-- View Matrix Legend button (shown on Skills Matrix page)
INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5025, 'sml025a1b2c3d4e5f6g7h85025', 'CMS_SKILLS_VIEW_MATRIX_LEGEND', 2, 'View matrix legend', 'View matrix legend', 'Bekijk matrixlegenda', 'Wyświetl legendę matrycy', 'Matrixlegende anzeigen');

-- Legend Modal Title
INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5026, 'sml026a1b2c3d4e5f6g7h85026', 'CMS_SKILLS_MATRIX_LEGEND_TITLE', 2, 'Skills Matrix Legend', 'Skills Matrix Legend', 'Vaardigheidsmatrix Legenda', 'Legenda matrycy umiejętności', 'Fähigkeitsmatrix Legende');

-- Reset to defaults button
INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5027, 'sml027a1b2c3d4e5f6g7h85027', 'CMS_SETTING_RESET_TO_DEFAULTS', 2, 'Reset to defaults', 'Reset to defaults', 'Herstel naar standaardwaarden', 'Przywróć ustawienia domyślne', 'Auf Standard zurücksetzen');

INSERT INTO resource_languages (id, resource_guid, resource_key, type, en_us, en_gb, nl_nl, pl_pl, de_de)
VALUES(5028, 'sml028a1b2c3d4e5f6g7h85028', 'CMS_COMPANY_SETTING_LEGEND_RESET_SUCCESSFULLY', 2, 'Legend configuration reset to defaults', 'Legend configuration reset to defaults', 'Legenda-configuratie teruggezet naar standaardwaarden', 'Konfiguracja legendy przywrócona do ustawień domyślnych', 'Legendenkonfiguration auf Standard zurückgesetzt');

# Database Migration Instructions

## Migration: 20260110_add_action_sorting_and_priority.sql

### What This Migration Does

1. **Adds `priority` column** to `actions_action` table
   - Type: `integer`
   - Default: `3` (Normal priority)
   - Values: 1=Critical, 2=High, 3=Normal, 4=Low
   - Creates index `idx_actions_action_priority` for performance

2. **Updates existing stored procedure** `get_actions_v3` with the following changes:
   - **Added parameters:**
     - `_sortby` (text, default: 'duedate') - determines which column to sort by
     - `_sortdirection` (text, default: 'asc') - determines sort direction (asc/desc)
   - **Added to return table:**
     - `priority` (integer) - action priority level
   - **Updated ORDER BY clause:** Replaced fixed ordering with dynamic sorting that supports:
     - `id`: Sort by action ID
     - `name` or `description`: Sort by action description
     - `duedate`: Sort by due date (default)
     - `startdate`: Sort by created_at date
     - `modifiedat` or `modificationdate`: Sort by modified_at timestamp
     - `areaname` or `area`: Sort by assigned area name
     - `username` or `user`: Sort by assigned user name
     - `lastcommentdate` or `recentchat`: Sort by most recent comment date
     - `priority`: Sort by priority level (1=Critical, 2=High, 3=Normal, 4=Low)
   - **Backwards compatible** with old parameter names
   - **NULL handling:** All NULL values sorted to end (NULLS LAST)
   - **Secondary sorting:** Maintains consistent ordering by is_resolved, due_date, modified_at

### What This Migration Does NOT Change

- All existing filtering logic remains unchanged
- All existing WHERE clauses remain unchanged
- All existing parameters remain with same defaults
- Backwards compatible - existing calls will work without any changes

### How to Run

#### Option 1: Using psql command line
```bash
psql -U your_username -d your_database -f Database/Migrations/20260110_add_action_sorting_and_priority.sql
```

#### Option 2: Using pgAdmin or database client
1. Open pgAdmin or your preferred PostgreSQL client
2. Connect to your database
3. Open Query Tool
4. Load the file: `Database/Migrations/20260110_add_action_sorting_and_priority.sql`
5. Execute the script

#### Option 3: Copy and paste
Copy the entire contents of the migration file and execute in your database client.

### Post-Migration Steps

1. **Grant permissions** to your application user:
   ```sql
   GRANT EXECUTE ON FUNCTION public.get_actions_v3 TO your_app_user;
   ```

2. **Verify the migration**:
   ```sql
   -- Check priority column
   SELECT column_name, data_type, column_default
   FROM information_schema.columns
   WHERE table_name = 'actions_action' AND column_name = 'priority';

   -- Check stored procedures
   SELECT routine_name, routine_type
   FROM information_schema.routines
   WHERE routine_name = 'get_actions_v3'
     AND routine_schema = 'public';
   ```

3. **Test the sorting**:
   ```sql
   -- Test ascending priority sort
   SELECT id, description, priority, due_date
   FROM get_actions_v3(
       _companyid := 1,
       _sortby := 'priority',
       _sortdirection := 'asc',
       _limit := 10
   );

   -- Test descending due date sort
   SELECT id, description, priority, due_date
   FROM get_actions_v3(
       _companyid := 1,
       _sortby := 'duedate',
       _sortdirection := 'desc',
       _limit := 10
   );

   -- Test last comment date sort
   SELECT id, description, lastcommentdate
   FROM get_actions_v3(
       _companyid := 1,
       _sortby := 'lastcommentdate',
       _sortdirection := 'desc',
       _limit := 10
   );
   ```

### Supported Sort Columns

| Sort By Value | Column | Description |
|---------------|--------|-------------|
| id | id | Action ID |
| name | description | Action description/name |
| description | description | Action description/name |
| duedate | due_date | Due date (default) |
| startdate | created_at | Action creation date |
| modifiedat | modified_at | Last modification timestamp |
| modificationdate | modified_at | Last modification timestamp (alias) |
| areaname | area.name | Assigned area name |
| area | area.name | Assigned area name (alias) |
| username | user.name | Assigned user name |
| user | user.name | Assigned user name (alias) |
| lastcommentdate | comment.modified_at | Most recent comment date |
| recentchat | comment.modified_at | Most recent comment date (alias) |
| priority | priority | Priority level (1=Critical, 2=High, 3=Normal, 4=Low) |

### Rollback (if needed)

```sql
-- Remove priority column (WARNING: data loss)
ALTER TABLE public.actions_action DROP COLUMN IF EXISTS priority;

-- Remove index
DROP INDEX IF EXISTS public.idx_actions_action_priority;
```

**Note:** After rollback, you'll need to restore the original `get_actions_v3` function from your backup or previous version.

### Notes

- Migration is **idempotent** - safe to run multiple times
- Default priority for existing actions is `3` (Normal)
- The script includes verification checks and will output success/failure messages
- NULLS are handled with `NULLS LAST` to ensure consistent sorting
- All existing filtering parameters and logic remain unchanged
- Default sort behavior: duedate ascending (same as before if no sort parameters provided)

### Support

For issues or questions, check the commit history or contact the development team.

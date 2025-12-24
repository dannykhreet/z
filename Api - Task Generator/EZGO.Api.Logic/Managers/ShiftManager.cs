using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EEZGO.Api.Utils.Data;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.General;
using EZGO.Api.Settings;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Utils.Cache;
using EZGO.Api.Utils.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Npgsql;

//TODO sort methods, rename all append methods in same structure.

namespace EZGO.Api.Logic.Managers
{
    /// <summary>
    /// ShiftManager; The ShiftManager contains all logic for retrieving and setting shifts.
    /// Shifts are linked to a company. A company can have 0 or more shifts. Shifts can currently not overlap. 
    /// A shift has a start and end date and is based on a certain day in the week. 
    /// Depending on the template type a shift is used to determine when a certain tasks needs to be finished/done.
    /// </summary>
    public class ShiftManager : BaseManager<ShiftManager>, IShiftManager
    {
        #region - privates -
        private readonly IMemoryCache _cache;
        private readonly IDatabaseAccessHelper _manager;
        private readonly IAreaBasicManager _areaBasicManager;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly IDataAuditing _dataAuditing;
        #endregion

        #region - constructor(s) -
        public ShiftManager(IDatabaseAccessHelper manager, IAreaBasicManager areaManager, IDataAuditing dataAuditing, IConfigurationHelper configurationHelper, ILogger<ShiftManager> logger, IMemoryCache memoryCache) : base(logger)
        {
            _cache = memoryCache;
            _manager = manager;
            _areaBasicManager = areaManager;
            _configurationHelper = configurationHelper;
            _dataAuditing = dataAuditing;
        }
        #endregion

        #region - public methods -
        /// <summary>
        /// GetShiftsAsync; Get shifts for a company.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="filters">Filters that can be used for filtering the data. Depending on implementation, filters can be done within the stored procedures or afterwards.</param>
        /// <returns>List of shifts.</returns>
        public async Task<List<Shift>> GetShiftsAsync(int companyId, ShiftFilters? filters = null) {
            var output = new List<Shift>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                using (dr = await _manager.GetDataReader("get_shifts", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var shift = CreateOrFillShiftFromReader(dr);
                        output.Add(shift);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ShiftManager.GetShiftsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (filters.HasValue && filters.Value.HasFilters())
            {
                output = (await FilterShifts(companyId: companyId, filters: filters.Value, nonFilteredCollection: output)).ToList();
            }

            return output;
        }

        /// <summary>
        /// GetShiftAsync; Get a single shift.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="shiftId">ShiftId (DB: companies_shifts.id)</param>
        /// <returns>A single shift item.</returns>
        public async Task<Shift> GetShiftAsync(int companyId, int shiftId) {
            var shift = new Shift();

            NpgsqlDataReader dr = null;


            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_id", shiftId));

                using (dr = await _manager.GetDataReader("get_shift", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        CreateOrFillShiftFromReader(dr, shift: shift);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ShiftManager.GetShiftAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if(shift.Id > 0)
            {
                return shift;
            }
            return null;
        }

        /// <summary>
        /// GetShiftAsync; Get a single shift based on a timestamp and companyId.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="date">Date (Date for getting the shift)</param>
        /// <returns>A single shift item.</returns>
        public async Task<Shift> GetShiftByTimestampAsync(int companyId, DateTime? timestamp)
        {
            timestamp ??= DateTime.Now;

            var shift = new Shift();

            NpgsqlDataReader dr = null;


            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_timestamp", timestamp));

                using (dr = await _manager.GetDataReader("get_shift_by_timestamp", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        CreateOrFillShiftFromReader(dr, shift: shift);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ShiftManager.GetShiftByDateTimeAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (shift.Id > 0)
            {
                return shift;
            }
            return null;
        }

        /// <summary>
        /// Asynchronously retrieves the start and end timestamps of a shift based on a specified offset from a given
        /// timestamp.
        /// </summary>
        /// <remarks>The method calculates the target shift by applying the specified offset to the
        /// current shift determined by the given timestamp. It handles week transitions and adjusts the target shift's
        /// week accordingly.</remarks>
        /// <param name="companyId">The identifier of the company for which the shift information is requested.</param>
        /// <param name="timestamp">The reference timestamp from which the shift offset is calculated. If null, the current date and time are
        /// used.</param>
        /// <param name="shiftOffset">The number of shifts to offset from the current shift. Positive values move forward in time, while negative
        /// values move backward.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see
        /// cref="ShiftTimestamps"/> object with the start and end times of the target shift.</returns>
        public async Task<ShiftTimestamps> GetShiftTimestampsByOffsetAsync(int companyId, DateTime? timestamp, int shiftOffset)
        {
            ShiftTimestamps result = null;

            timestamp ??= DateTime.Now;

            Shift currentShift = await GetShiftByTimestampAsync(companyId: companyId, timestamp: timestamp);

            if (currentShift == null) return result;

            List<Shift> companyShifts = await GetShiftsAsync(companyId: companyId);

            bool movingForward = shiftOffset >= 0;
            int shiftCount = companyShifts.Count;
            int deltaWeek = Math.Abs(shiftOffset) / shiftCount;
            int deltaShifts = Math.Abs(shiftOffset) % shiftCount;
            int currentShiftIndex = companyShifts.FindIndex(x => x.Id == currentShift.Id);
            int targetShiftIndex = currentShiftIndex;

            if (movingForward)
            {
                if(currentShiftIndex + deltaShifts >= shiftCount)
                {
                    deltaWeek += 1;
                    targetShiftIndex = (currentShiftIndex + deltaShifts) - shiftCount;
                }
                else
                {
                    targetShiftIndex = currentShiftIndex + deltaShifts;
                }
            }
            else //moving backward
            {
                if(currentShiftIndex - deltaShifts < 0)
                {
                    deltaWeek += 1;
                    targetShiftIndex = shiftCount - (deltaShifts - currentShiftIndex);
                }
                else
                {
                    targetShiftIndex = currentShiftIndex - deltaShifts;
                }
                deltaWeek = -deltaWeek;
            }

            Shift targetShift = companyShifts[targetShiftIndex];
            DateTime targetWeek = timestamp.Value.AddDays(deltaWeek * 7);
            result = new ShiftTimestamps();
            result.Start = ISOWeek.ToDateTime(year: ISOWeek.GetYear(targetWeek), week: ISOWeek.GetWeekOfYear(targetWeek), dayOfWeek: (DayOfWeek)targetShift.Weekday+1);
            result.End = result.Start;
            result.Start = result.Start.Add(TimeSpan.Parse(targetShift.Start));
            result.End = result.End.Add(TimeSpan.Parse(targetShift.End));

            if(result.Start > result.End)
            {
                result.End = result.End.AddDays(1);
            }

            return result;

        }

        /// <summary>
        /// Retrieves the shift, day and week timestamps for a specified company and timestamp.
        /// </summary>
        /// <remarks>If no data is found for the specified timestamp, default values are returned: - The
        /// day starts at midnight and ends at 11:59:59.9999999 PM of the same day. - The week starts on Monday and ends
        /// on Sunday.</remarks>
        /// <param name="companyId">The unique identifier of the company for which the shift, day and week timestamps are retrieved.</param>
        /// <param name="timestamp">The point in time for which the shift day and week timestamps are calculated. If <see langword="null"/>, the
        /// current date and time are used.</param>
        /// <returns>A <see cref="ShiftDayWeekTimestamps"/> object containing the start and end times for the shift, day and week
        /// corresponding to the specified timestamp.</returns>
        public async Task<ShiftDayWeekTimestamps> GetShiftDayWeekTimesByTimestamp(int companyId, DateTime? timestamp)
        {
            ShiftDayWeekTimestamps result = null;
            timestamp ??= DateTime.Now;

            NpgsqlDataReader dr = null;


            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_timestamp", timestamp));

                using (dr = await _manager.GetDataReader("get_shift_day_week_times_by_timestamp", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        result = CreateOrFillShiftDayWeekTimestampsFromReader(dr, result);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ShiftManager.getShiftDayWeekTimesByTimestamp(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (result == null)
            {
                result = new ShiftDayWeekTimestamps()
                {
                    DayStart = timestamp.Value.Date,
                    DayEnd = timestamp.Value.Date.AddDays(1).AddTicks(-1),
                    WeekStart = timestamp.Value.Date.AddDays(-((int)timestamp.Value.DayOfWeek - 1 < 0 ? 6 : (int)timestamp.Value.DayOfWeek - 1)),
                };
                result.WeekEnd = result.WeekStart.Date.AddDays(7).AddTicks(-1);
            }

            return result;
        }

        /// <summary>
        /// AddShiftAsync; Add a Shift to the database.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="shift">Shift object, containing all relevant Shift data to add.(DB: companies_shift)</param>
        /// <returns>The identity of the table (DB: companies_shift.id)</returns>
        public async Task<int> AddShiftAsync(int companyId, int userId, Shift shift)
        {
            shift = ShiftValidators.ValidateAndSetDefaults(shift);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromShift(shift: shift, companyId: companyId));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_shift", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.companies_shift.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.companies_shift.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added shift.");
            }
            //remove cache based on key for possible shift cache with company.
            CacheHelpers.ResetCacheByKeyByKeyStart(memoryCache: _cache, CacheHelpers.GenerateCacheKey(CacheSettings.CacheKeyCompany, companyId));

            return possibleId;

        }

        /// <summary>
        /// ChangeShiftAsync; Change a Shift in the database.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="shiftId">ShiftId, id of the object in the database that needs to be updated. (DB: companies_shift.id) </param>
        /// <param name="shift">Shift object containing all data needed for updating the database. (DB: companies_shift)</param>
        /// <returns>true / false depending on outcome. If more then 1 row is updated, true is returned in all other cases false is returned.</returns>
        public async Task<bool> ChangeShiftAsync(int companyId, int userId, int shiftId, Shift shift)
        {
            shift = ShiftValidators.ValidateAndSetDefaults(shift);

            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.companies_shift.ToString(), shiftId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromShift(shift: shift, companyId: companyId, shiftId:shiftId));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_shift", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.companies_shift.ToString(), shiftId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.companies_shift.ToString(), objectId: shiftId, userId: userId, companyId: companyId, description: "Changed shift.");
            }

            //remove cache based on key for possible shift cache with company.
            CacheHelpers.ResetCacheByKeyByKeyStart(memoryCache: _cache, CacheHelpers.GenerateCacheKey(CacheSettings.CacheKeyCompany, companyId));

            return rowseffected > 0;
        }

        /// <summary>
        /// SetShiftActiveAsync; Set Shift active/inactive based on ShiftId.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="shiftId">ShiftId (DB: companies_shift.id)</param>
        /// <param name="isActive">true / false -> default true is selected, for setting a Shift to inactive, set parameter to false.</param>
        /// <returns>true / false depending on outcome. If more then 1 row is updated, true is returned in all other cases false.</returns>
        public async Task<bool> SetShiftActiveAsync(int companyId, int userId, int shiftId, bool isActive = true)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.companies_shift.ToString(), shiftId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", shiftId));
            parameters.Add(new NpgsqlParameter("@_active", isActive));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_shift_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.companies_shift.ToString(), shiftId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.companies_shift.ToString(), objectId: shiftId, userId: userId, companyId: companyId, description: "Changed shift active state.");
            }
            //remove cache based on key for possible shift cache with company.
            CacheHelpers.ResetCacheByKeyByKeyStart(memoryCache: _cache, CacheHelpers.GenerateCacheKey(CacheSettings.CacheKeyCompany, companyId));

            return (rowseffected > 0);
        }
        #endregion

        #region - private methods Filters -
        /// <summary>
        /// FilterShifts; FilterShifts is the primary filter method for filtering shifts. Within this method the specific filters are determined based on the supplied Shift object.
        /// Filtering is done based on cascading filters, meaning, the first filter is applied, which results in a filtered collection.
        /// On that filtered collection the second filter is applied which results in a filtered-filtered collection.
        /// This will continue until all filters are applied.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="filters">ShiftFilters, depending on the values certain filters will be applies.</param>
        /// <param name="nonFilteredCollection">List of non filtered Shift objects.</param>
        /// <returns>A filtered list of Shift objects.</returns>
        private async Task<IList<Shift>> FilterShifts(int companyId, ShiftFilters filters, IList<Shift> nonFilteredCollection)
        {
            var filtered = nonFilteredCollection;
            if (filters.AreaId.HasValue)
            {
                if (filters.FilterAreaType.HasValue)
                {
                    filtered = await FilterShiftsOnArea(companyId: companyId, areaId: filters.AreaId.Value, filterType: filters.FilterAreaType.Value, shifts: filtered);
                }
                else
                {
                    filtered = await FilterShiftsOnArea(areaId: filters.AreaId.Value, shifts: filtered);
                }
            }
            if (filters.Day.HasValue)
            {
                filtered = await FilterShiftsOnDay(day: filters.Day.Value, shifts: filtered);
            }
            return filtered;
        }

        /// <summary>
        /// FilterShiftsOnArea; Filter a Shifts collection on AreaId.
        /// </summary>
        /// <param name="areaId">AreaId ( DB: companies_shift.area_id)</param>
        /// <param name="shifts">Collection of items to be filtered.</param>
        /// <returns>A filtered set of items.</returns>
        private async Task<IList<Shift>> FilterShiftsOnArea(int areaId, IList<Shift> shifts)
        {

            shifts = shifts.Where(x => x.AreaId == areaId).ToList();
            await Task.CompletedTask; //used for making method async executable.
            return shifts;
        }


        /// <summary>
        /// FilterShiftsOnArea; Filter a Shift collection on AreaId and a FilterAreaType. Depending on type a recursive filter is being used based on the children of a Area.
        /// </summary>
        /// <param name="areaId">AreaId ( DB: companies_shift.area_id)</param>
        /// <param name="filterType">FilterAreaTypeEnum, type based on Single, RootToLeaf and LeafToRoot filtering.</param>
        /// <param name="shifts">Collection of items to be filtered.</param>
        /// <returns>A filtered set of items.</returns>
        private async Task<IList<Shift>> FilterShiftsOnArea(int companyId, int areaId, FilterAreaTypeEnum filterType, IList<Shift> shifts)
        {
            var areas = await _areaBasicManager.GetAreasBasicByStartAreaAsync(companyId: companyId, areaId: areaId, areaFilterType: filterType);
            if (areas == null || areas.Count == 0)
            {
                areas.Add(new Models.Basic.AreaBasic() { Id = areaId, Name = "" });
            }
            //get data
            shifts = shifts.Where(x => x.AreaId.HasValue && areas.Select(a => a.Id).Contains(x.AreaId.Value)).ToList();
            return shifts;
            // return nonFilteredCollection;
        }

        /// <summary>
        /// FilterShiftsOnDay; Filter a Shifts collection on Day.
        /// </summary>
        /// <param name="day">Day ( DB: companies_shift.day)</param>
        /// <param name="shifts">Collection of items to be filtered.</param>
        /// <returns>A filtered set of items.</returns>
        private async Task<IList<Shift>> FilterShiftsOnDay(int day, IList<Shift> shifts)
        {

            shifts = shifts.Where(x => x.Day == day).ToList();
            await Task.CompletedTask; //used for making method async executable.
            return shifts;
        }
        #endregion

        #region - private methods -
        /// <summary>
        /// CreateOrFillShiftFromReader; creates and fills a Shift object from a DataReader.
        /// NOTE! intended for use with the action comment stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="shift">Shift object containing all data needed for updating the database. (DB: companies_shift)</param>
        /// <returns>A filled Shift object.</returns>
        private Shift CreateOrFillShiftFromReader(NpgsqlDataReader dr, Shift shift = null)
        {
            if (shift == null) shift = new Shift();

            shift.Id = Convert.ToInt32(dr["id"]);

            if (dr.HasColumn("day") && dr["day"] != DBNull.Value)
            {
                shift.Day = Convert.ToInt32(dr["day"]);
            }

            shift.Weekday = Convert.ToInt32(dr["weekday"]);

            if (dr.HasColumn("area_id") && dr["area_id"] != DBNull.Value)
            {
                shift.AreaId = Convert.ToInt32(dr["area_id"]);
            }
            shift.Start = dr["start"].ToString();
            shift.End = dr["end"].ToString();
            if (dr.HasColumn("shiftnr"))
            {
                if (dr["shiftnr"] != DBNull.Value)
                {
                    shift.ShiftNr = Convert.ToInt32(dr["shiftnr"]);
                }
            }

            return shift;
        }

        /// <summary>
        /// GetNpgsqlParametersFromShift; Creates a list of NpgsqlParameters, and fills it based on the supplied Shift object.
        /// NOTE! intended for use with the action stored procedures within the database.
        /// </summary>
        /// <param name="shift">The supplied Shift object, containing all data.</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="shiftId">ShiftId (DB: companies_shift.id)</param>
        /// <returns>A list of NpgsqlParameter parameters.</returns>
        private List<NpgsqlParameter> GetNpgsqlParametersFromShift(Shift shift, int companyId, int shiftId = 0)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (shiftId > 0) parameters.Add(new NpgsqlParameter("@_id", shiftId));

            var start = new NpgsqlParameter("@_start", Convert.ToDateTime(shift.Start).TimeOfDay);
            start.DbType = System.Data.DbType.Time;
            var end = new NpgsqlParameter("@_end", Convert.ToDateTime(shift.End).TimeOfDay);
            end.DbType = System.Data.DbType.Time;

            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_day", Convert.ToInt16(shift.Day)));
            parameters.Add(start);
            parameters.Add(end);
            if (shift.AreaId.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_areaid", shift.AreaId));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_areaid", DBNull.Value));
            }
            parameters.Add(new NpgsqlParameter("@_weekday", Convert.ToInt16(shift.Weekday)));

            return parameters;
        }

        private ShiftDayWeekTimestamps CreateOrFillShiftDayWeekTimestampsFromReader(NpgsqlDataReader dr, ShiftDayWeekTimestamps result)
        {
            result ??= new ShiftDayWeekTimestamps();

            result.ShiftStart = Convert.ToDateTime(dr["shift_start"]);
            result.ShiftEnd = Convert.ToDateTime(dr["shift_end"]);
            result.DayStart = Convert.ToDateTime(dr["day_start"]);
            result.DayEnd = Convert.ToDateTime(dr["day_end"]);
            result.WeekStart = Convert.ToDateTime(dr["week_start"]);
            result.WeekEnd = Convert.ToDateTime(dr["week_end"]);

            return result;
        }
        #endregion

            #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            var listEx = new List<Exception>();
            try
            {
                listEx.AddRange(this.Exceptions);
                listEx.AddRange(_areaBasicManager.GetPossibleExceptions());
            }
            catch (Exception ex)
            {
                //error occurs with errors, return only this error
                listEx.Add(ex);
            }
            return listEx;
        }
        #endregion


    }
}

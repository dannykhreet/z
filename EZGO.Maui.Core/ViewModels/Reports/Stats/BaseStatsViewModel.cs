using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Reports;
using EZGO.Maui.Core.Models.Reports;

namespace EZGO.Maui.Core.ViewModels.Reports.Stats
{
    public abstract class BaseStatsViewModel : NotifyPropertyChanged, IDisposable
    {
        private bool disposedValue;

        protected IReportService reportService;
        private IServiceScope scope;

        public bool IsRefreshing { get; set; }

        public BaseStatsViewModel(bool isRefreshing)
        {
            IsRefreshing = isRefreshing;
            scope = App.Container.CreateScope();
            reportService = scope.ServiceProvider.GetService<IReportService>();
        }

        public abstract Task FillStats(List<TaskStats> statsList, List<ReportsCount> reportsCounts);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    scope.Dispose();
                }

                reportService = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

using System;

namespace WebApp.Models.Pdf
{
    public class CompletedTasksPdfHeaderModel
    {
        public string FilterType { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string HeaderText { get; set; }

        public int HeaderHeight
        {
            get { return 16; }
        }
    }
}

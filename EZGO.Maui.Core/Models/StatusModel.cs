namespace EZGO.Maui.Core.Models
{
    public class StatusModel<T>
    {
        public string Color { get; set; }
        public T Status { get; set; }
        public int ItemNumber { get; set; }
        public bool IsSelected { get; set; } = false;
        public int Percentage { get; set; }

        public StatusModel(T status, string color)
        {
            Status = status;
            Color = color;
        }
    }
}

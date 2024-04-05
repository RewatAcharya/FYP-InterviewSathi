namespace InterviewSathi.Web.ViewModels
{
    public class RadialChart
    {
        public decimal TotalCount { get; set; }
        public decimal TotalCurrent { get; set; }
        public bool hasIncreased { get; set; }
        public int[] Series { get; set; }
    }
}

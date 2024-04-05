namespace InterviewSathi.Web.ViewModels
{
    public class LineChart
    {
        public List<ChartData> Series { get; set; }
        public string[] Categories { get; set; }
    }

    public class ChartData
    {
        public string Name { get; set; }
        public int[] Data { get; set; }
    }
}

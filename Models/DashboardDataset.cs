using System;
using System.Collections.Generic;
using System.Text;

namespace MocSaude.Models
{
    public class DashboardDataset
    {
        public List<Dictionary<String, object>> Rows { get; set; } = new();
        public List<ChartPoint> ChartData { get; set; } = new();
        public String TableName { get; set; }
        public String GroupBy { get; set; }
        public String Aggregate { get; set; }
        public String AggFunc { get; set; }
    }
}

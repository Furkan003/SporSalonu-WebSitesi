namespace NfaSporSalonu.Models.DTOs;

public class ProgressReportDto
{
    public string MemberName { get; set; } = null!;
    public int TotalMeasurements { get; set; }
    public DateTime? FirstMeasurementDate { get; set; }
    public DateTime? LastMeasurementDate { get; set; }

    // Vücut Metrik Değişimleri
    public BodyMetricChangeDto? WeightChange { get; set; }
    public BodyMetricChangeDto? WaistChange { get; set; }
    public BodyMetricChangeDto? BicepChange { get; set; }
    public BodyMetricChangeDto? ChestChange { get; set; }

    // Hesaplanan Oranlar
    public decimal? InitialBmi { get; set; }
    public decimal? CurrentBmi { get; set; }
    public decimal? InitialWaistToHeightRatio { get; set; }
    public decimal? CurrentWaistToHeightRatio { get; set; }

    // Analiz Sonucu
    public string Summary { get; set; } = null!;
    public List<string> Insights { get; set; } = new();
}

public class BodyMetricChangeDto
{
    public string MetricName { get; set; } = null!;
    public decimal? InitialValue { get; set; }
    public decimal? CurrentValue { get; set; }
    public decimal? Difference { get; set; }
    public decimal? PercentageChange { get; set; }
    public string Direction { get; set; } = null!; // "increase", "decrease", "stable"
}

public class MeasurementTrendDto
{
    public DateTime? Date { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Waist { get; set; }
    public decimal? Bicep { get; set; }
    public decimal? Chest { get; set; }
    public decimal? Bmi { get; set; }
    public decimal? WaistToHeightRatio { get; set; }
}

public class AnalyticsTrendsResponse
{
    public List<MeasurementTrendDto> Trends { get; set; } = new();
    public ChartDataDto ChartData { get; set; } = null!;
}

public class ChartDataDto
{
    public List<string> Labels { get; set; } = new();
    public List<ChartDatasetDto> Datasets { get; set; } = new();
}

public class ChartDatasetDto
{
    public string Label { get; set; } = null!;
    public List<decimal?> Data { get; set; } = new();
    public string BorderColor { get; set; } = null!;
    public string BackgroundColor { get; set; } = null!;
}

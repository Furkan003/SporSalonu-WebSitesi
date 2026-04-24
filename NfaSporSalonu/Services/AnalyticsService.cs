using Microsoft.EntityFrameworkCore;
using NfaSporSalonu.Models;
using NfaSporSalonu.Models.DTOs;

namespace NfaSporSalonu.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly NfaSporSalonuDbContext _context;

    public AnalyticsService(NfaSporSalonuDbContext context)
    {
        _context = context;
    }

    public async Task<ProgressReportDto> GetProgressReport(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        var measurements = await _context.MemberMeasurements
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.MeasurementDate)
            .ToListAsync();

        var report = new ProgressReportDto
        {
            MemberName = $"{user.FirstName} {user.LastName}",
            TotalMeasurements = measurements.Count,
            Insights = new List<string>()
        };

        if (measurements.Count < 2)
        {
            report.Summary = "Karşılaştırma yapabilmek için en az 2 ölçüm kaydı gereklidir.";
            return report;
        }

        var first = measurements.First();
        var last = measurements.Last();

        report.FirstMeasurementDate = first.MeasurementDate;
        report.LastMeasurementDate = last.MeasurementDate;

        // ─── Vücut Metrik Değişimleri ───
        report.WeightChange = CalculateChange("Kilo (kg)", first.Weight, last.Weight);
        report.WaistChange = CalculateChange("Bel (cm)", first.Waist, last.Waist);
        report.BicepChange = CalculateChange("Bicep (cm)", first.Bicep, last.Bicep);
        report.ChestChange = CalculateChange("Göğüs (cm)", first.Chest, last.Chest);

        // ─── BMI Hesaplaması ───
        if (first.Weight.HasValue && first.Height.HasValue && first.Height > 0)
        {
            var heightM = first.Height.Value / 100m;
            report.InitialBmi = Math.Round(first.Weight.Value / (heightM * heightM), 2);
        }
        if (last.Weight.HasValue && last.Height.HasValue && last.Height > 0)
        {
            var heightM = last.Height.Value / 100m;
            report.CurrentBmi = Math.Round(last.Weight.Value / (heightM * heightM), 2);
        }

        // ─── Bel/Boy Oranı ───
        if (first.Waist.HasValue && first.Height.HasValue && first.Height > 0)
            report.InitialWaistToHeightRatio = Math.Round(first.Waist.Value / first.Height.Value, 4);
        if (last.Waist.HasValue && last.Height.HasValue && last.Height > 0)
            report.CurrentWaistToHeightRatio = Math.Round(last.Waist.Value / last.Height.Value, 4);

        // ─── İçgörüler (Insights) ───
        GenerateInsights(report);

        // ─── Özet ───
        report.Summary = GenerateSummary(report);

        return report;
    }

    public async Task<AnalyticsTrendsResponse> GetMeasurementTrends(int userId)
    {
        var measurements = await _context.MemberMeasurements
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.MeasurementDate)
            .ToListAsync();

        var trends = measurements.Select(m =>
        {
            decimal? bmi = null;
            decimal? waistToHeight = null;

            if (m.Weight.HasValue && m.Height.HasValue && m.Height > 0)
            {
                var hm = m.Height.Value / 100m;
                bmi = Math.Round(m.Weight.Value / (hm * hm), 2);
            }
            if (m.Waist.HasValue && m.Height.HasValue && m.Height > 0)
                waistToHeight = Math.Round(m.Waist.Value / m.Height.Value, 4);

            return new MeasurementTrendDto
            {
                Date = m.MeasurementDate,
                Weight = m.Weight,
                Waist = m.Waist,
                Bicep = m.Bicep,
                Chest = m.Chest,
                Bmi = bmi,
                WaistToHeightRatio = waistToHeight
            };
        }).ToList();

        // ─── Chart.js uyumlu veri ───
        var chartData = new ChartDataDto
        {
            Labels = trends.Select(t => t.Date?.ToString("dd.MM.yyyy") ?? "").ToList(),
            Datasets = new List<ChartDatasetDto>
            {
                new()
                {
                    Label = "Kilo (kg)",
                    Data = trends.Select(t => t.Weight).ToList(),
                    BorderColor = "rgba(54, 162, 235, 1)",
                    BackgroundColor = "rgba(54, 162, 235, 0.2)"
                },
                new()
                {
                    Label = "Bel (cm)",
                    Data = trends.Select(t => t.Waist).ToList(),
                    BorderColor = "rgba(255, 99, 132, 1)",
                    BackgroundColor = "rgba(255, 99, 132, 0.2)"
                },
                new()
                {
                    Label = "Bicep (cm)",
                    Data = trends.Select(t => t.Bicep).ToList(),
                    BorderColor = "rgba(75, 192, 192, 1)",
                    BackgroundColor = "rgba(75, 192, 192, 0.2)"
                },
                new()
                {
                    Label = "Göğüs (cm)",
                    Data = trends.Select(t => t.Chest).ToList(),
                    BorderColor = "rgba(153, 102, 255, 1)",
                    BackgroundColor = "rgba(153, 102, 255, 0.2)"
                },
                new()
                {
                    Label = "BMI",
                    Data = trends.Select(t => t.Bmi).ToList(),
                    BorderColor = "rgba(255, 159, 64, 1)",
                    BackgroundColor = "rgba(255, 159, 64, 0.2)"
                }
            }
        };

        return new AnalyticsTrendsResponse
        {
            Trends = trends,
            ChartData = chartData
        };
    }

    // ─── YARDIMCI METOTLAR ───

    private static BodyMetricChangeDto? CalculateChange(string name, decimal? initial, decimal? current)
    {
        if (!initial.HasValue || !current.HasValue)
            return null;

        var diff = current.Value - initial.Value;
        var pct = initial.Value != 0
            ? Math.Round((diff / initial.Value) * 100, 2)
            : 0;

        return new BodyMetricChangeDto
        {
            MetricName = name,
            InitialValue = initial,
            CurrentValue = current,
            Difference = Math.Round(diff, 2),
            PercentageChange = pct,
            Direction = diff > 0 ? "increase" : diff < 0 ? "decrease" : "stable"
        };
    }

    private static void GenerateInsights(ProgressReportDto report)
    {
        // Kilo analizi
        if (report.WeightChange is { Direction: "decrease" })
            report.Insights.Add($"🔽 Kilo kaybı tespit edildi: {Math.Abs(report.WeightChange.Difference ?? 0)} kg azalma.");
        else if (report.WeightChange is { Direction: "increase" })
            report.Insights.Add($"🔼 Kilo artışı: {report.WeightChange.Difference} kg artış.");

        // Kas artışı analizi
        var bicepUp = report.BicepChange is { Direction: "increase" };
        var chestUp = report.ChestChange is { Direction: "increase" };
        var waistDown = report.WaistChange is { Direction: "decrease" };

        if (bicepUp && chestUp && waistDown)
            report.Insights.Add("💪 Net kas artışı tespit edildi: Bicep ve göğüs artarken bel çevresi azalmış.");
        else if (bicepUp || chestUp)
            report.Insights.Add("💪 Üst vücut kas gelişimi gözlemleniyor.");

        // Bel/Boy oranı
        if (report.CurrentWaistToHeightRatio.HasValue)
        {
            if (report.CurrentWaistToHeightRatio < 0.5m)
                report.Insights.Add("✅ Bel/Boy oranı ideal aralıkta (< 0.50).");
            else
                report.Insights.Add("⚠️ Bel/Boy oranı riskli bölgede (≥ 0.50). Kardiyo tavsiye edilir.");
        }

        // BMI analizi
        if (report.CurrentBmi.HasValue)
        {
            var bmi = report.CurrentBmi.Value;
            var category = bmi switch
            {
                < 18.5m => "Zayıf",
                < 25m => "Normal",
                < 30m => "Fazla Kilolu",
                _ => "Obez"
            };
            report.Insights.Add($"📊 Güncel BMI: {bmi} ({category}).");
        }
    }

    private static string GenerateSummary(ProgressReportDto report)
    {
        var parts = new List<string>();

        if (report.WeightChange != null)
            parts.Add($"Kilo: {report.WeightChange.Difference:+0.##;-0.##;0} kg");
        if (report.WaistChange != null)
            parts.Add($"Bel: {report.WaistChange.Difference:+0.##;-0.##;0} cm");
        if (report.BicepChange != null)
            parts.Add($"Bicep: {report.BicepChange.Difference:+0.##;-0.##;0} cm");

        return parts.Count > 0
            ? $"Toplam değişim → {string.Join(" | ", parts)}"
            : "Yeterli veri yok.";
    }
}

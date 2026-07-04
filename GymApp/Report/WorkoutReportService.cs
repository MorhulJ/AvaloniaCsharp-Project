using System;
using System.Collections.Generic;
using GymApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Colors = QuestPDF.Helpers.Colors;
using Document = QuestPDF.Fluent.Document;

namespace GymApp.Reports;

public class WorkoutReportService
{
    public void GenerateFullReport(
        User user,
        IEnumerable<Goal> goals,
        IEnumerable<PersonalRecord> personalRecords,
        string filePath)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text("GymApp Report")
                        .SemiBold().FontSize(24).FontColor(Colors.Orange.Medium);
                    col.Item().Text($"User: {user.Name}")
                        .FontSize(13).FontColor(Colors.Grey.Darken2);
                    col.Item().Text($"Generated: {DateTime.Now:dd.MM.yyyy HH:mm}")
                        .FontSize(10).FontColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Orange.Medium);
                });

                page.Content().Column(col =>
                {
                    col.Item().PaddingTop(15).Text("Goals")
                        .SemiBold().FontSize(16).FontColor(Colors.Orange.Medium);

                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Title").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Target").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Current").Bold();
                        });

                        foreach (var goal in goals)
                        {
                            table.Cell().Padding(5).Text(goal.Title);
                            table.Cell().Padding(5).Text(goal.TargetValue.ToString());
                            table.Cell().Padding(5).Text(goal.CurrentValue.ToString());
                        }
                    });
                    
                    col.Item().PaddingTop(20).Text("Personal Records")
                        .SemiBold().FontSize(16).FontColor(Colors.Orange.Medium);

                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Exercise").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Value (kg)").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Date").Bold();
                        });

                        foreach (var record in personalRecords)
                        {
                            table.Cell().Padding(5).Text(record.Exercise?.Name ?? "");
                            table.Cell().Padding(5).Text(record.Value.ToString());
                            table.Cell().Padding(5).Text(record.Date.ToString("dd.MM.yyyy"));
                        }
                    });
                });

                page.Footer().AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Generated: ");
                        x.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                    });
            });
        }).GeneratePdf(filePath);
    }
}
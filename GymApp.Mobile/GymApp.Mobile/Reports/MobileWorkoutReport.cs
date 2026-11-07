using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia.Platform;
using GymApp.Models;
using SkiaSharp;

namespace GymApp.Mobile.Reports;

internal static class MobileWorkoutReport
{
    private const float PageWidth = 595;
    private const float PageHeight = 842;
    private const float Margin = 44;

    public static void Write(Stream output, string userName, IReadOnlyList<Goal> goals,
        IReadOnlyList<PersonalRecord> records, IReadOnlyList<Exercise> exercises)
    {
        using var regularAsset = AssetLoader.Open(new Uri("avares://Avalonia.Fonts.Inter/Assets/Inter-Regular.ttf"));
        using var boldAsset = AssetLoader.Open(new Uri("avares://Avalonia.Fonts.Inter/Assets/Inter-SemiBold.ttf"));
        using var regular = SKTypeface.FromStream(regularAsset);
        using var bold = SKTypeface.FromStream(boldAsset);
        using var bodyFont = new SKFont(regular, 11);
        using var headingFont = new SKFont(bold, 16);
        using var titleFont = new SKFont(bold, 24);
        using var paint = new SKPaint { IsAntialias = true };
        using var document = SKDocument.CreatePdf(output) ?? throw new IOException("Couldn't create the PDF.");

        var exerciseNames = exercises.ToDictionary(exercise => exercise.FirebaseId, exercise => exercise.Name);
        var pageNumber = 0;
        SKCanvas canvas = null!;
        float baseline = 0;
        StartPage();
        Draw("GymApp Report", titleFont, SKColor.Parse("#C75D3A"), 30);
        Draw($"User: {userName}", bodyFont, SKColors.Black, 16);
        Draw($"Generated: {DateTime.Now:g}", bodyFont, SKColors.DimGray, 16);

        baseline += 16;
        Draw("Goals", headingFont, SKColors.Black, 22);
        if (goals.Count == 0) Draw("No goals yet.", bodyFont, SKColors.DimGray, 16);
        foreach (var goal in goals)
        {
            Draw(goal.Title, bodyFont, SKColors.Black, 16);
            Draw($"Current: {goal.CurrentValue:g}    Target: {goal.TargetValue:g}", bodyFont, SKColors.DimGray, 16);
            baseline += 8;
        }

        baseline += 16;
        Draw("Personal records", headingFont, SKColors.Black, 22);
        if (records.Count == 0) Draw("No personal records yet.", bodyFont, SKColors.DimGray, 16);
        foreach (var record in records.OrderByDescending(record => record.Date))
        {
            var exercise = exerciseNames.GetValueOrDefault(record.ExerciseFirebaseId) ?? "Exercise unavailable";
            Draw(exercise, bodyFont, SKColors.Black, 16);
            Draw($"{record.Value:g} kg    {record.Date:d}", bodyFont, SKColors.DimGray, 16);
            baseline += 8;
        }
        document.EndPage();
        document.Close();

        void StartPage()
        {
            canvas = document.BeginPage(PageWidth, PageHeight);
            pageNumber++;
            baseline = Margin + 24;
            paint.Color = SKColors.DimGray;
            canvas.DrawText($"GymApp   •   {pageNumber}", Margin, PageHeight - Margin, bodyFont, paint);
        }

        void Draw(string value, SKFont font, SKColor color, float lineHeight)
        {
            foreach (var line in Wrap(value, font, PageWidth - 2 * Margin))
            {
                if (baseline > PageHeight - Margin - 22)
                {
                    document.EndPage();
                    StartPage();
                }
                paint.Color = color;
                canvas.DrawText(line, Margin, baseline, font, paint);
                baseline += lineHeight;
            }
        }
    }

    private static IEnumerable<string> Wrap(string text, SKFont font, float maxWidth)
    {
        foreach (var paragraph in text.ReplaceLineEndings("\n").Split('\n'))
        {
            var line = "";
            var elements = StringInfo.GetTextElementEnumerator(paragraph);
            while (elements.MoveNext())
            {
                var element = elements.GetTextElement();
                if (line.Length > 0 && font.MeasureText(line + element) > maxWidth)
                {
                    var breakAt = line.LastIndexOf(' ');
                    if (breakAt > 0)
                    {
                        yield return line[..breakAt];
                        line = line[(breakAt + 1)..];
                    }
                    else
                    {
                        yield return line;
                        line = "";
                    }
                }
                line += element;
            }
            yield return line;
        }
    }
}

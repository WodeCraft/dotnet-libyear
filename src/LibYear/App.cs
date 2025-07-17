using LibYear.Core;
using Spectre.Console;
using System.Text.Json;

namespace LibYear;

public class App
{
	private readonly IPackageVersionChecker _checker;
	private readonly IProjectFileManager _projectFileManager;
	private readonly IAnsiConsole _console;

	public App(IPackageVersionChecker checker, IProjectFileManager projectFileManager, IAnsiConsole console)
	{
		_checker = checker;
		_projectFileManager = projectFileManager;
		_console = console;
	}

	public async Task<int> Run(Settings settings)
	{
		_console.WriteLine();
		var projects = await _projectFileManager.GetAllProjects(settings.Paths, settings.Recursive);
		if (projects.Count == 0)
		{
			_console.WriteLine("No project files found");
			return 1;
		}

		var result = await _checker.GetPackages(projects);
		DisplayAllResultsTables(result, settings.QuietMode, settings.Output);

		if (settings.Update)
		{
			var updated = await _projectFileManager.Update(result);
			foreach (var projectFile in updated)
			{
				_console.WriteLine($"{projectFile} updated");
			}
		}

		var limitChecker = new LimitChecker(settings);
		return limitChecker.AnyLimitsExceeded(result)
			? 1
			: 0;
	}

	private void DisplayAllResultsTables(SolutionResult allResults, bool quietMode, string? outputFormat = "")
	{
		if (allResults.Details.Count == 0)
			return;

		int MaxLength(Func<Result, int> field)
			=> allResults.Details.Max(results => results.Details.Count > 0 ? results.Details.Max(field) : 0);
		var namePad = Math.Max("Package".Length, MaxLength(r => r.Name.Length));
		var installedPad = Math.Max("Installed".Length, MaxLength(r => r.Installed?.Version.ToString().Length ?? 0));
		var latestPad = Math.Max("Latest".Length, MaxLength(r => r.Latest?.Version.ToString().Length ?? 0));
		var versionBehindPad = Math.Max("Ver behind".Length, MaxLength(r => r.VersionBehind.Length));

		var width = allResults.Details.Max(r => r.ProjectFile.FileName.Length) + 10;
		_console.Profile.Width = 150; // width;
		foreach (var results in allResults.Details)
			GetResultsTable(results, width, namePad, installedPad, latestPad, versionBehindPad, quietMode);

		if (outputFormat is not null
			&& outputFormat?.ToLowerInvariant() == "json")
		{
			var json = JsonSerializer.Serialize(allResults.Details, new JsonSerializerOptions
			{
				WriteIndented = true
			});
			File.WriteAllText("libyear.json", json);
		}

		if (allResults.Details.Count > 1)
		{
			_console.WriteLine($"Total is {allResults.YearsBehind:F1} libyears behind");
		}
	}

	private void GetResultsTable(ProjectResult results,
		int titlePad, int namePad, int installedPad, int latestPad,
		int versionBehindPad, bool quietMode)
	{
		if (results.Details.Count == 0)
			return;

		var width = Math.Max(titlePad + 2, namePad + installedPad + latestPad + versionBehindPad + 48) + 2;
		var table = new Table
		{
			Title = new TableTitle($"  {results.ProjectFile.FileName}".PadRight(width)),
			Caption = new TableTitle(($"Project is {results.YearsBehind:F1} libyears behind. Average of {results.YearsBehind / results.Details.Count:F1} libyears").PadRight(width)),
			Width = width
		};
		table.AddColumn(new TableColumn("Package").Width(namePad));
		table.AddColumn(new TableColumn("Installed").Width(installedPad));
		table.AddColumn(new TableColumn("Released"));
		table.AddColumn(new TableColumn("Latest").Width(latestPad));
		table.AddColumn(new TableColumn("Released"));
		table.AddColumn(new TableColumn("Age (y)"));
		table.AddColumn(new TableColumn("Pulse (y)").NoWrap());
		table.AddColumn(new TableColumn("Ver behind").NoWrap());

		Result? latestResult;
		try
		{
			foreach (var result in results.Details.Where(r => !quietMode || r.YearsBehind > 0))
			{
				latestResult = result;
				table.AddRow(
					result.Name,
					result.Installed?.Version.ToString() ?? "N/A",
					result.Installed?.Date.ToString("yyyy-MM-dd") ?? "N/A",
					result.Latest?.Version.ToString() ?? "N/A",
					result.Latest?.Date.ToString("yyyy-MM-dd") ?? "N/A",
					result.YearsBehind.ToString("F1"),
					result.Latest?.Pulse.ToString("F1") ?? "N/A",
					string.IsNullOrWhiteSpace(result.VersionBehind) ? "N/A" : result.VersionBehind
				);
			}
		}
		catch (Exception ex)
		{
			var error = ex.Message;
			throw;
		}


		if (quietMode && Math.Abs(results.YearsBehind) < double.Epsilon)
		{
			table.ShowHeaders = false;
		}

		_console.Write(table);
		_console.WriteLine();
	}
}
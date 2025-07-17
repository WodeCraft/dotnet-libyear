namespace LibYear.Core;

public class Result : HasAgeMeasurements
{
	public string Name { get; }
	public Release? Installed { get; }
	public Release? Latest { get; }

	public Result(string name, Release? installed, Release? latest)
	{
		Name = name;
		Installed = installed;
		Latest = latest;
	}

	public override double DaysBehind
		=> (Latest?.Date - Installed?.Date ?? TimeSpan.Zero).TotalDays;

	public string VersionBehind
	{
		get
		{
			if (Installed == null || Latest == null)
			{
				return string.Empty;
			}

			if (Installed.Version.Major == Latest.Version.Major
				&& Installed.Version.Minor == Latest.Version.Minor)
			{
				return new PackageVersion(0, 0, Latest.Version.Patch - Installed.Version.Patch).ToString();
			}

			if (Installed.Version.Major == Latest.Version.Major)
			{
				return new PackageVersion(0, Latest.Version.Minor - Installed.Version.Minor, 0).ToString();
			}

			return new PackageVersion(Latest.Version.Major - Installed.Version.Major, 0, 0).ToString(); ;
		}
	}
}
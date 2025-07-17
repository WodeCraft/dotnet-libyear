using NuGet.Protocol.Core.Types;

namespace LibYear.Core;

public class Release
{
	public PackageVersion Version { get; }
	public DateTime Date { get; }
	public bool IsPublished { get; }

	public double Pulse => _pulse > 0 ? _pulse / 365 : 0;

	private double _pulse => (DateTime.UtcNow - Date).TotalDays;

	public Release(IPackageSearchMetadata metadata) : this(new PackageVersion(metadata.Identity.Version), metadata.Published.GetValueOrDefault().Date, metadata.IsListed)
	{
	}

	public Release(PackageVersion version, DateTime released, bool isPublished = true)
	{
		Version = version;
		Date = released;
		IsPublished = isPublished;
	}
}
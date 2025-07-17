using Spectre.Console.Cli;
using System.ComponentModel;

namespace LibYear;

public class Settings : CommandSettings
{
	[CommandArgument(0, "[target]")]
	[Description("the project file(s) or directory paths to analyze")]
	public string[] Paths { get; set; } = ["."];

	[CommandOption("-u|--update")]
	[Description("update any outdated packages")]
	public bool Update { get; set; }

	[CommandOption("-o|--output")]
	[Description("the output format. Either json or mkd")]
	public string? Output { get; set; }

	[CommandOption("-q|--quiet")]
	[Description("only output outdated packages")]
	public bool QuietMode { get; set; }

	[CommandOption("-l|--limit")]
	[Description("fails if total libyears behind is greater than this value")]
	public double? LimitTotal { get; set; }

	[CommandOption("-p|--limit-project")]
	[Description("fails if any project's total libyears behind is greater than this value")]
	public double? LimitProject { get; set; }

	[CommandOption("-a|--limit-any")]
	[Description("fails if any dependency is more libyears behind than this value")]
	public double? LimitAny { get; set; }

	[CommandOption("-r|--recursive")]
	[Description("search recursively for all compatible files, even if one is found in a directory passed as an argument")]
	public bool Recursive { get; set; }
}
using Mustache;

namespace Github.Msbuild.Core
{
	public class Tweaker
	{
		public static void Tweak(GithubModel model)
		{
			var compiler = new FormatCompiler();
			var format = ResourceFileLoader.GithubMsbuildCoreDefaultDescriptiontxt;
			var generator = compiler.Compile(format);
			var result = generator.Render(model);
			model.Nuget.ReleaseNotes = result;
			model.Nuget.ReleaseNotesPowershell = result.Replace("\n", "`n").Replace("\r", "`r");
		}
	}
}
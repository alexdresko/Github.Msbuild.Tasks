using Github.Msbuild.Core;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Github.Msbuild.Tasks
{
	public class GetGithubModel : Task
	{
		public override bool Execute()
		{
			var gitIt = new GitIt(ProductHeaderValue, Owner, Repository, Milestone);
			this.GithubModel = gitIt.GetValue();
			return true;

		}

		[Output]
		public GithubModel GithubModel { get; set; }

		[Required]
		public int Milestone { get; set; }

		[Required]
		public string Repository { get; set; }

		[Required]
		public string Owner { get; private set; }

		[Required]
		public string ProductHeaderValue { get; set; }
	}
}
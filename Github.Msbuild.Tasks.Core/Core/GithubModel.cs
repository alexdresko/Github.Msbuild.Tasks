using System.Collections.Generic;
using Octokit;

namespace Github.Msbuild.Core
{
	public class GithubModel
	{
		public GithubModel()
		{
			Repository = new Repository();
			Issues = new List<Issue>();
			Nuget = new Nuget();
			Milestone = new Milestone();
		}

		public Repository Repository { get; set; }
		public IReadOnlyList<Issue> Issues { get; set; }
		public Milestone Milestone { get; set; }
		public Nuget Nuget { get; set; }
	}
}
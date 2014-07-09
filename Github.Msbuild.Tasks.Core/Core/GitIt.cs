using System.Threading.Tasks;
using Octokit;
using RazorEngine;

namespace Github.Msbuild.Core
{
	public class GitIt
	{
		public GitIt(string productHeaderValue, string owner, string repository, int milestone)
		{
			ProductHeaderValue = productHeaderValue;
			Owner = owner;
			Repository = repository;
			this.MileStone = milestone;
		}

		public string ProductHeaderValue { get; set; }
		public int MileStone { get; set; }
		public string Owner { get; set; }
		public string Repository { get; set; }
		public SortDirection SortDirection { get; set; }

		public GithubModel GetValue()
		{
			var model = new GithubModel();
			var github = new GitHubClient(new ProductHeaderValue(ProductHeaderValue));
			var repo = github.Repository.Get(Owner, Repository);

			var issues = github.Issue.GetForRepository(Owner, Repository,
				new RepositoryIssueRequest {SortDirection = SortDirection, Milestone = MileStone.ToString()});

			var milestone = github.Issue.Milestone.Get(Owner, this.Repository, MileStone);
			
			Task.WaitAll(repo, issues, milestone);

			model.Milestone = milestone.Result;


			model.Repository = repo.Result;

			model.Issues = issues.Result;

			model.Nuget.Description = Razor.Parse(ResourceFileLoader.GithubMsbuildCoreDefaultDescriptiontxt, model);


			return model;
		}
	}
}
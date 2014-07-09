using System.Collections.Generic;
using Github.Msbuild.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Octokit;
using RazorEngine;

namespace Github.Msbuild.Tasks.Tests
{
	[TestClass]
	public class ProofOfConceptTests
	{
		[TestMethod]
		public void CanPullDataFromGithub()
		{
			const string githubMsbuildTasks = "Github.Msbuild.Tasks";
			var g = new GitIt("Github.Msbuild.Tasks.Tests", "alexdresko", githubMsbuildTasks, 1);
			g.SortDirection = SortDirection.Ascending;
			//g.MileStone = "1";
			var result = g.GetValue();

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Repository.Name == githubMsbuildTasks);
		}

		[TestMethod]
		public void CanUserRazorWithEmbeddedResource()
		{
			var model = new GithubModel();
			model.Repository.Name = "asdf";
			model.Repository.Description = "This is so cool";
			model.Milestone.Title = "Whoooo";
			model.Milestone.Description = "Niceeeeee";

			model.Issues = new List<Issue>
			{
				new Issue { Title = "Some issue", State = ItemState.Open, Milestone = model.Milestone },
				new Issue { Title = "Another issue", State = ItemState.Closed, Milestone = model.Milestone }
			};

			var result = Razor.Parse(ResourceFileLoader.GithubMsbuildCoreDefaultDescriptiontxt, model);

			Assert.IsTrue(result.Contains("Niceeeeee"));
		}
	}
}
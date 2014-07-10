using System.Collections.Generic;
using Github.Msbuild.Core;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Octokit;

namespace Github.Msbuild.Tasks
{
	public class GetGithubModel : Task
	{
		public override bool Execute()
		{
			var gitIt = new GitIt(ProductHeaderValue, Owner, Repository, Milestone, AuthenticationToken);

			var model = gitIt.GetValue();

			GithubDescription = model.Repository.Description;
			NugetReleaseNotes = model.Nuget.ReleaseNotes;
			NugetReleaseNotesPowershell = model.Nuget.ReleaseNotesPowershell;
			
			return true;

		}

		[Output]
		public string NugetReleaseNotesPowershell { get; set; }

		[Output]
		public string GithubDescription { get; set; }

		[Output]
		public string NugetReleaseNotes { get; set; }

		[Required]
		public int Milestone { get; set; }
	
		[Required]
		public string AuthenticationToken { get; set; }

		[Required]
		public string Repository { get; set; }

		[Required]
		public string Owner { get; set; }

		[Required]
		public string ProductHeaderValue { get; set; }
	}
}
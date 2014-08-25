//-----------------------------------------------------------------------
// <copyright file="Packager.cs">(c) http://www.codeplex.com/MSBuildExtensionPack. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
// 7/11/2014 7:35:21 AM by AD:   This class has been modified to suit my needs. 
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Github.Msbuild.Tasks
{
	public class Packager : Task
	{
		public Packager()
		{
			RequiresExplicitLicensing = false;
		}

		[Required]
		public string Id { get; set; }

		[Required]
		public string Version { get; set; }

		public string Title { get; set; }

		public bool RequiresExplicitLicensing { get; set; }

		public string Authors { get; set; }

		public string Owners { get; set; }

		[Required]
		public string LicenseUrl { get; set; }

		public string IconUrl { get; set; }

		[Required]
		public string ProjectUrl { get; set; }

		[Required]
		public string Description { get; set; }

		public string ReleaseNotes { get; set; }

		public string CopyrightText { get; set; }

		public string Tags { get; set; }

		[Required]
		public string OutputFile { get; set; }

		public ITaskItem[] Dependencies { get; set; }

		public ITaskItem[] References { get; set; }

		public ITaskItem[] LibraryFiles { get; set; }

		public ITaskItem[] ContentFiles { get; set; }

		public ITaskItem[] ToolsFiles { get; set; }

		public ITaskItem[] FrameworkAssemblies { get; set; }


		private static bool IsValidVersionNumber(string version)
		{
			var regex = new Regex(@"\d+(?:\.\d+)+");
			return regex.Match(version).Success;
		}

		private static XElement GenerateDependencyElement(XNamespace defaultNamespace, ITaskItem taskItem)
		{
			var dependency = new XElement(defaultNamespace + "dependency");
			dependency.Add(new XAttribute("id", taskItem.ItemSpec));
			var version = taskItem.GetMetadata("version");
			if (!string.IsNullOrWhiteSpace(version))
			{
				if (!IsValidVersionNumber(version))
				{
					throw new Exception(string.Format(CultureInfo.CurrentCulture, "Invalid version {0} specified for dependency {1}",
						version, taskItem.ItemSpec));
				}

				dependency.Add(new XAttribute("version", version));
			}

			return dependency;
		}

		private static XElement GenerateFrameworkAssemblyXElement(XNamespace defaultNamespace, ITaskItem taskItem)
		{
			var frameworkAssembly = new XElement(defaultNamespace + "frameworkAssembly ");
			frameworkAssembly.Add(new XAttribute("assemblyName", taskItem.ItemSpec));
			if (!string.IsNullOrWhiteSpace(taskItem.GetMetadata("version")))
			{
				frameworkAssembly.Add(new XAttribute("version", taskItem.GetMetadata("version")));
			}

			return frameworkAssembly;
		}

		private static void PopulateFolder(string folderName, string packageDirectoryPath, IEnumerable<ITaskItem> items)
		{
			if (items == null)
			{
				return;
			}

			var libDirectory = Directory.CreateDirectory(Path.Combine(packageDirectoryPath, folderName));
			foreach (var item in items)
			{
				var framework = item.GetMetadata("framework");
				if (!string.IsNullOrWhiteSpace(framework))
				{
					framework = Path.Combine(libDirectory.FullName, framework);
					if (!Directory.Exists(framework))
					{
						Directory.CreateDirectory(framework);
					}

					File.Copy(item.ItemSpec, Path.Combine(framework, Path.GetFileName(item.ItemSpec)));
				}
				else
				{
					File.Copy(item.ItemSpec, Path.Combine(libDirectory.FullName, Path.GetFileName(item.ItemSpec)));
				}
			}
		}

		private void Pack()
		{
			if (!IsValidVersionNumber(Version))
			{
				Log.LogError(string.Format(CultureInfo.CurrentCulture,
					"Invalid version number {0}. Examples of valid version numbers are 1.0, 1.2.3. 2.3.1909.7", Version));
				return;
			}

			var nugetDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(nugetDirectory);
			try
			{
				var nugetspecification = GenerateSpecification(nugetDirectory);
				PopulateFolder("lib", nugetDirectory, LibraryFiles);
				PopulateFolder("content", nugetDirectory, ContentFiles);
				PopulateFolder("tools", nugetDirectory, LibraryFiles);
				PreparePackage(nugetspecification);
			}
			finally
			{
				Directory.Delete(nugetDirectory, true);
			}
		}

		private void PreparePackage(string nugetSpecificationFile)
		{
			var executionDirectory = Path.GetDirectoryName(nugetSpecificationFile);
			var nugetFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
				@"Resources\Nuget.exe"); // Path.Combine(executionDirectory, "nuget.exe");

			var processStartInfo = new ProcessStartInfo
			{
				Arguments = "pack " + nugetSpecificationFile,
				WorkingDirectory = executionDirectory,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				FileName = nugetFilePath
			};

			using (var process = Process.Start(processStartInfo))
			{
				process.WaitForExit(20000);
				if (process.ExitCode != 0)
				{
					Log.LogError(string.Format(CultureInfo.CurrentCulture, "Nuget Package could not be created. Exit Code: {0}.",
						process.ExitCode));
				}
				else
				{
					var files = Directory.GetFiles(executionDirectory, Id + "." + Version + "*.nupkg");
					if (files.Any())
					{
						File.Copy(files[0], OutputFile, true);
						Log.LogMessage(MessageImportance.Normal,
							string.Format(CultureInfo.CurrentCulture, "NuGet Package {0} created successfully.", OutputFile));
					}
				}
			}
		}

		private string GenerateSpecification(string directoryPath)
		{
			var specFileName = Path.Combine(directoryPath, Id + ".nuspec");
			Log.LogMessage(MessageImportance.Low,
				string.Format(CultureInfo.CurrentCulture, "Generating NuGet specification file {0}", specFileName));

			XNamespace defaultNamespace = "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd";
			var specXml = new XElement(defaultNamespace + "package");
			specXml.Add(
				new XElement(
					defaultNamespace + "metadata",
					new XElement(defaultNamespace + "id", Id),
					new XElement(defaultNamespace + "version", Version),
					new XElement(defaultNamespace + "authors", Authors),
					new XElement(defaultNamespace + "owners", Owners),
					new XElement(defaultNamespace + "licenseUrl", LicenseUrl),
					new XElement(defaultNamespace + "projectUrl", ProjectUrl),
					new XElement(defaultNamespace + "requireLicenseAcceptance",
						RequiresExplicitLicensing.ToString().ToLower(CultureInfo.CurrentCulture)),
					new XElement(defaultNamespace + "description", Description),
					new XElement(defaultNamespace + "tags", Tags)));

			if (Dependencies != null)
			{
				specXml.Element(defaultNamespace + "metadata").Add(new XElement(
					defaultNamespace + "dependencies",
					from dependency in Dependencies.Select(e => GenerateDependencyElement(defaultNamespace, e)) select dependency));
			}

			if (References != null)
			{
				specXml.Element(defaultNamespace + "metadata").Add(new XElement(
					defaultNamespace + "references",
					from reference in References.Select(e =>
					{
						var reference = new XElement(defaultNamespace + "reference");
						reference.Add(new XAttribute("file", e.ItemSpec));
						return reference;
					})
					select reference));
			}

			if (FrameworkAssemblies != null)
			{
				specXml.Element(defaultNamespace + "metadata").Add(new XElement(
					defaultNamespace + "frameworkAssemblies",
					from frameworkAssemblies in FrameworkAssemblies.Select(e => GenerateFrameworkAssemblyXElement(defaultNamespace, e))
					select frameworkAssemblies));
			}

			specXml.Save(specFileName);
			return specFileName;
		}

		public override bool Execute()
		{
			Pack();
			return true;
		}
	}
}
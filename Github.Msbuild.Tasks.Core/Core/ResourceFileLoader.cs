using System.IO;

namespace Github.Msbuild.Core
{
	public static class ResourceFileLoader
	{
		public static string Get(string fileName)
		{
			var assembly = typeof (GithubModel).Assembly;
			var resourceName = fileName;


			string file = null;
			using (var stream = assembly.GetManifestResourceStream(resourceName))
				if (stream != null)
					using (var reader = new StreamReader(stream))
					{
						file = reader.ReadToEnd();
					}
			return file;
		}

		public static string GithubMsbuildCoreDefaultDescriptiontxt
		{
			get { return Get("Github.Msbuild.Core.DefaultDescription.txt"); }
		}
	}
}
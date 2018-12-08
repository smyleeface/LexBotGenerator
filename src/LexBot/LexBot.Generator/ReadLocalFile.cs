using System.IO;
using System.Reflection;

namespace LexBot.Generator {
    public static class ReadLocalFile {
        public static ParseLexYaml Run(string resourceName) {
            var assembly = Assembly.GetExecutingAssembly();
            var data = GetEmbeddedResource(resourceName, assembly);
            var yamlString = new StringReader(data);
            return new ParseLexYaml(yamlString);
        }
        private static string FormatResourceName(Assembly assembly, string resourceName) {
            return assembly.GetName().Name + "." + resourceName.Replace(" ", "_")
                       .Replace("\\", ".")
                       .Replace("/", ".");
        }
        
        private static string GetEmbeddedResource(string resourceName, Assembly assembly) {
            resourceName = FormatResourceName(assembly, resourceName);
            using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName)) {
                if (resourceStream == null) {
                    return null;
                }
                using (StreamReader reader = new StreamReader(resourceStream)) {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
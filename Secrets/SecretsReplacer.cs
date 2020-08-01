using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Collections.Generic;
using System.IO;
#if NETCOREAPP
using System.Text.Json;
using System.Text.Json.Serialization;
#else
using Newtonsoft.Json;
#endif
namespace Secrets
{
    public class SecretReplacer : Task
    {
        [Required]
        public string SecretsClass { get; set; }
        public override bool Execute()
        {
            var buildDir = Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode);
            var jsonPath = Path.Combine(buildDir, "Secrets.json");
            var json = File.ReadAllText(jsonPath);
#if NETCOREAPP
            var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(json, new JsonSerializerOptions());
#else
            var secrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
#endif

            var codeFile = Path.Combine(buildDir, SecretsClass);
            var code = File.ReadAllText(codeFile);
            var backupFile = codeFile + ".bak";
            File.WriteAllText(backupFile, code);
            Log.LogMessage(MessageImportance.High, $"Backup file created at {backupFile}");
            foreach (var secret in secrets)
            {
                code = code.Replace($"%{secret.Key}%", secret.Value);
            }
            File.WriteAllText(codeFile, code);
            Log.LogMessage(MessageImportance.High, $"Secrets  written to {codeFile}");
            return true;
        }
    }
    public class SecretHider : Task
    {
        [Required]
        public string SecretsClass { get; set; }

        public override bool Execute()
        {
            var buildDir = Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode);
            var codeFile = Path.Combine(buildDir, SecretsClass);
            var backupFile = codeFile + ".bak";
            var code = File.ReadAllText(backupFile);
            Log.LogMessage(MessageImportance.High, $"Backup file read from {backupFile}");
            File.WriteAllText(codeFile, code);
            Log.LogMessage(MessageImportance.High, $"Backup restored to {codeFile}");
            File.Delete(backupFile);
            Log.LogMessage(MessageImportance.High, $"Backup file {backupFile} deleetd");
            return true;
        }
    }
}

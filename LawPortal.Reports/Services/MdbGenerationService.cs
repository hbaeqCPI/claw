using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LawPortal.Reports.Services
{
    public class MdbGenerationService
    {
        private readonly string _sqlConnectionString;
        private readonly string _templatePath;
        private readonly string _generatorExePath;
        private readonly ILogger<MdbGenerationService> _logger;

        public MdbGenerationService(IConfiguration configuration, ILogger<MdbGenerationService> logger, string webRootPath)
        {
            _sqlConnectionString = configuration.GetConnectionString("DefaultConnection");
            _templatePath = Path.Combine(webRootPath, "templates", "blank_template.mdb");
            _logger = logger;

            // Find the merged LawPortal.Mdb.exe in solution's build output (we invoke
            // with the "generate" subcommand below).
            var webRoot = new DirectoryInfo(webRootPath);
            var solutionRoot = webRoot.Parent; // LawPortal.Web
            if (solutionRoot?.Parent != null)
                solutionRoot = solutionRoot.Parent; // solution root

            _generatorExePath = Path.Combine(solutionRoot?.FullName ?? "", "LawPortal.Mdb", "bin", "Debug", "net8.0", "LawPortal.Mdb.exe");
        }

        public async Task<List<string>> GenerateMdbFiles(string systems, bool generatePatent, bool generateTrademark, string outputFolder, string releaseName = "")
        {
            Directory.CreateDirectory(outputFolder);

            // Create config JSON for the generator process
            var config = new
            {
                SqlConnectionString = _sqlConnectionString,
                TemplatePath = _templatePath,
                OutputFolder = outputFolder,
                Systems = systems ?? "",
                GeneratePatent = generatePatent,
                GenerateTrademark = generateTrademark,
                ReleaseName = releaseName ?? ""
            };

            var configPath = Path.Combine(Path.GetTempPath(), $"mdbgen_{Guid.NewGuid():N}.json");
            try
            {
                await File.WriteAllTextAsync(configPath, JsonSerializer.Serialize(config));

                if (!File.Exists(_generatorExePath))
                    throw new FileNotFoundException($"LawPortal.Mdb.exe not found at: {_generatorExePath}. Please build the LawPortal.Mdb project.");

                _logger.LogInformation("MdbGeneration: Starting generator at {Path}", _generatorExePath);

                var psi = new ProcessStartInfo
                {
                    FileName = _generatorExePath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                psi.ArgumentList.Add("generate");
                psi.ArgumentList.Add(configPath);

                using var process = Process.Start(psi);
                if (process == null)
                    throw new Exception("Failed to start MDB Generator process.");

                var stdout = await process.StandardOutput.ReadToEndAsync();
                var stderr = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (!string.IsNullOrWhiteSpace(stderr))
                    _logger.LogInformation("MdbGenerator output: {Output}", stderr);

                // Check for success marker (ACE OleDb provider may crash during cleanup causing non-zero exit code
                // even when all work completed successfully)
                bool hasSuccessMarker = stdout.Contains("__MDB_SUCCESS__");

                if (!hasSuccessMarker && process.ExitCode != 0)
                    throw new Exception($"MDB Generator failed (exit code {process.ExitCode}): {stderr}");

                // Parse output - generator writes file paths to stdout, one per line
                var generatedFiles = stdout.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s) && s != "__MDB_SUCCESS__")
                    .ToList();

                _logger.LogInformation("MdbGeneration: Generated {Count} files.", generatedFiles.Count);
                return generatedFiles;
            }
            finally
            {
                // Clean up temp config file
                if (File.Exists(configPath))
                    File.Delete(configPath);
            }
        }
    }
}

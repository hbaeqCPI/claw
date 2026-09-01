using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LawPortal.Web.Services
{
    /// <summary>
    /// Thin client for the MOVEit Transfer REST API (v1). Used by the Deploy
    /// screen's Push button to publish the selected LawDocs/Mdb files to the
    /// company MFT server.
    ///
    /// Flow (see <see cref="PushFilesAsync"/>): log in (POST /token) → upload each
    /// file into the login's default folder keeping its generated name (nothing is
    /// deleted) → log out (POST /token/revoke).
    ///
    /// When two selections share the same name (compared case-insensitively — the
    /// R4 and R5 "law9" MDBs of each side, e.g. "..._Patlaw9.mdb"/"..._patlaw9.mdb"
    /// or "..._TmkLaw9.mdb"), they would overwrite each other in one folder, so each
    /// is routed into a generation subfolder ("Ver9and10", "R5", …) taken from its
    /// slot label — created if missing — so both files are preserved.
    ///
    /// Registered as a typed HttpClient (services.AddHttpClient&lt;MoveItTransferClient&gt;()).
    /// </summary>
    public class MoveItTransferClient
    {
        private readonly HttpClient _http;
        private readonly MoveItMftSettings _settings;
        private readonly ILogger<MoveItTransferClient> _logger;

        // Cache of resolved subfolder ids under the default folder, keyed by name.
        private readonly Dictionary<string, int> _subfolderCache =
            new(StringComparer.OrdinalIgnoreCase);

        private string _accessToken;

        public MoveItTransferClient(
            HttpClient http,
            IOptions<MoveItMftSettings> settings,
            ILogger<MoveItTransferClient> logger)
        {
            _http = http;
            _settings = settings.Value;
            _logger = logger;

            if (!string.IsNullOrWhiteSpace(_settings?.BaseUrl))
                _http.BaseAddress = new Uri(_settings.BaseUrl.TrimEnd('/') + "/");
        }

        /// <summary>
        /// Logs in, uploads each job, and logs out. Never throws for a single failed
        /// file — per-file errors are collected and returned so the caller can report
        /// a partial result. Throws only for a failure that prevents the whole push
        /// (bad config, login failure).
        /// </summary>
        /// <param name="jobs">(LocalPath = file on disk, FileName = name to store as,
        /// Label = slot label whose last segment names the disambiguation subfolder,
        /// e.g. "Mdbs\Tmk\Ver9and10").</param>
        public async Task<(int Uploaded, List<string> Errors)> PushFilesAsync(
            IEnumerable<(string LocalPath, string FileName, string Label)> jobs)
        {
            if (string.IsNullOrWhiteSpace(_settings?.BaseUrl) ||
                string.IsNullOrWhiteSpace(_settings.UserName) ||
                string.IsNullOrWhiteSpace(_settings.Password))
                throw new InvalidOperationException("MoveItMft credentials are not configured in appsettings.");

            var jobList = jobs.ToList();

            // Files whose names match (case-insensitively — MOVEit folders are not
            // case-sensitive) would clash in one folder, so each is routed into its
            // generation subfolder ("Ver9and10", "R5", …); everything else goes flat.
            var collidingNames = jobList
                .GroupBy(j => j.FileName, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var errors = new List<string>();
            int uploaded = 0;

            await AuthenticateAsync();
            try
            {
                int defaultFolderId = await GetDefaultFolderIdAsync();

                foreach (var (localPath, fileName, label) in jobList)
                {
                    var who = string.IsNullOrEmpty(label) ? fileName : $"{label} ({fileName})";
                    try
                    {
                        if (!File.Exists(localPath))
                            throw new FileNotFoundException($"Local file not found: {localPath}");

                        int targetFolderId = defaultFolderId;

                        // Same exact name as another file this push → keep both by
                        // dropping each into a subfolder named for its generation.
                        if (collidingNames.Contains(fileName))
                        {
                            var bucket = LastSegment(label);
                            if (!string.IsNullOrEmpty(bucket))
                                targetFolderId = await EnsureSubfolderAsync(defaultFolderId, bucket);
                            else
                                _logger.LogWarning("MOVEit: {Who} collides on name but has no subfolder label; uploading flat.", who);
                        }

                        await UploadAsync(targetFolderId, localPath, fileName);
                        uploaded++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "MOVEit push failed for {Who}", who);
                        errors.Add($"{who}: {ex.Message}");
                    }
                }
            }
            finally
            {
                await RevokeAsync();
            }

            return (uploaded, errors);
        }

        // ── Auth ────────────────────────────────────────────────────────────

        private async Task AuthenticateAsync()
        {
            using var body = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", _settings.UserName),
                new KeyValuePair<string, string>("password", _settings.Password),
            });

            using var resp = await _http.PostAsync("api/v1/token", body);
            var json = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new Exception($"MOVEit login failed (HTTP {(int)resp.StatusCode}). {Trim(json)}");

            using var doc = JsonDocument.Parse(json);
            _accessToken = doc.RootElement.TryGetProperty("access_token", out var t)
                ? t.GetString()
                : null;
            if (string.IsNullOrEmpty(_accessToken))
                throw new Exception("MOVEit login returned no access token.");

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _accessToken);
            _logger.LogInformation("MOVEit: authenticated as {User}", _settings.UserName);
        }

        private async Task RevokeAsync()
        {
            if (string.IsNullOrEmpty(_accessToken)) return;
            try
            {
                using var body = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("token", _accessToken),
                });
                using var resp = await _http.PostAsync("api/v1/token/revoke", body);
                _logger.LogInformation("MOVEit: logged out (revoke HTTP {Code})", (int)resp.StatusCode);
            }
            catch (Exception ex)
            {
                // Best effort — the token will expire on its own.
                _logger.LogWarning(ex, "MOVEit: token revoke failed (ignored).");
            }
            finally
            {
                _accessToken = null;
                _http.DefaultRequestHeaders.Authorization = null;
            }
        }

        // ── Folder / upload ─────────────────────────────────────────────────

        // The login's default (upload) folder. Falls back to the home folder.
        private async Task<int> GetDefaultFolderIdAsync()
        {
            using var resp = await _http.GetAsync("api/v1/users/self");
            var json = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new Exception($"MOVEit: could not read account info (HTTP {(int)resp.StatusCode}). {Trim(json)}");

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("defaultFolderID", out var d) && d.TryGetInt32(out var defId) && defId > 0)
                return defId;
            if (root.TryGetProperty("homeFolderID", out var h) && h.TryGetInt32(out var homeId) && homeId > 0)
                return homeId;

            throw new Exception("MOVEit: account has no default/home folder id.");
        }

        // Finds (or creates) a direct subfolder of <parentId> with this name.
        private async Task<int> EnsureSubfolderAsync(int parentId, string name)
        {
            if (_subfolderCache.TryGetValue(name, out var cached))
                return cached;

            using (var resp = await _http.GetAsync(
                $"api/v1/folders/{parentId}/subfolders?name={Uri.EscapeDataString(name)}"))
            {
                var json = await resp.Content.ReadAsStringAsync();
                if (resp.IsSuccessStatusCode)
                {
                    var match = FindItemIdByName(json, name);
                    if (match.HasValue)
                    {
                        _subfolderCache[name] = match.Value;
                        return match.Value;
                    }
                }
                else
                {
                    throw new Exception($"MOVEit: listing subfolders of {parentId} failed (HTTP {(int)resp.StatusCode}). {Trim(json)}");
                }
            }

            using var createBody = JsonContent(new { name, inheritPermissions = "Always" });
            using var createResp = await _http.PostAsync($"api/v1/folders/{parentId}/subfolders", createBody);
            var createJson = await createResp.Content.ReadAsStringAsync();
            if (!createResp.IsSuccessStatusCode)
                throw new Exception($"MOVEit: creating subfolder '{name}' failed (HTTP {(int)createResp.StatusCode}). {Trim(createJson)}");

            using var doc = JsonDocument.Parse(createJson);
            if (doc.RootElement.TryGetProperty("id", out var idEl) && idEl.TryGetInt32(out var newId))
            {
                _logger.LogInformation("MOVEit: created subfolder '{Name}' ({Id}) under {Parent}", name, newId, parentId);
                _subfolderCache[name] = newId;
                return newId;
            }
            throw new Exception($"MOVEit: created subfolder '{name}' but response had no id.");
        }

        private async Task UploadAsync(int folderId, string localPath, string fileName)
        {
            using var form = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(localPath);
            using var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            form.Add(streamContent, "file", fileName);

            using var resp = await _http.PostAsync($"api/v1/folders/{folderId}/files", form);
            if (!resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                throw new Exception($"MOVEit: uploading '{fileName}' failed (HTTP {(int)resp.StatusCode}). {Trim(json)}");
            }
            _logger.LogInformation("MOVEit: uploaded {Name} to folder {Folder}", fileName, folderId);
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        // Returns the int id of the first item whose "name" equals <name> (exact).
        private static int? FindItemIdByName(string listJson, string name)
        {
            using var doc = JsonDocument.Parse(listJson);
            if (!doc.RootElement.TryGetProperty("items", out var items) ||
                items.ValueKind != JsonValueKind.Array)
                return null;

            foreach (var item in items.EnumerateArray())
            {
                if (item.TryGetProperty("name", out var nm) &&
                    string.Equals(nm.GetString(), name, StringComparison.OrdinalIgnoreCase) &&
                    item.TryGetProperty("id", out var idEl) && idEl.TryGetInt32(out var id))
                    return id;
            }
            return null;
        }

        // Last path segment of a "Mdbs\Tmk\Ver9and10"-style label → "Ver9and10".
        private static string LastSegment(string label)
        {
            if (string.IsNullOrWhiteSpace(label)) return "";
            var parts = label.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 0 ? "" : parts[^1].Trim();
        }

        private static StringContent JsonContent(object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }

        // Keep error snippets short and free of noise; never contains our credentials.
        private static string Trim(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return "";
            body = body.Trim();
            return body.Length > 300 ? body.Substring(0, 300) + "…" : body;
        }
    }
}

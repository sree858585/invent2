using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Server.Models;
using HIVTraining_Vue.Data;
using System.Globalization;

namespace HIVTraining_Vue.Server.Controllers;

[ApiController]
[Route("api/scorm/runtime")]
public class ScormRuntimeController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public ScormRuntimeController(ApplicationDbContext context) => _context = context;

    public record InitRequest(string? userId, int scormId, int? scoId, bool forceNewAttempt = false);
    public record InitResponse(string registrationId, string scoId, int attempt, Dictionary<string, string> preloadCmi);

    public record CommitRequest(string scoId, List<CommitItem> data);
    public record CommitItem(string element, string value);

    public record FinishRequest(string scoId, string session_time, bool client_completed = false);

    private static int UnixNow() => (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    private static int? ParseNullableInt(string? s) => int.TryParse(s, out var x) ? x : (int?)null;

    private static bool IsCompletedLike(string? v)
    {
        var s = v?.Trim().ToLowerInvariant();
        return s is "completed" or "passed" or "failed";
    }

    private static string? Normalize(string? v) => v?.Trim();

    private static string LessonValue(string? v12, string? success2004, string? completion2004)
    {
        var a = v12?.Trim().ToLowerInvariant();
        var b = success2004?.Trim().ToLowerInvariant();
        var c = completion2004?.Trim().ToLowerInvariant();

        if (a is "completed" or "passed" or "failed") return a!;
        if (b is "passed" or "failed") return b!;
        if (c is "completed") return "completed";
        return "incomplete";
    }

    private static double? ParseDouble(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (double.TryParse(s.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
            return d;
        return null;
    }

    private static bool ProgressMeansCompleted(double? progress, double? threshold)
    {
        if (progress is null) return false;
        if (threshold is not null && threshold > 0) return progress >= threshold;
        return progress >= 0.999;
    }

    [HttpPost("init")]
    public async Task<ActionResult<InitResponse>> Init([FromBody] InitRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.userId) || req.scormId <= 0)
            return BadRequest("userId and scormId are required.");

        if (!Guid.TryParse(req.userId, out var userGuid))
            return BadRequest("userId must be a valid GUID string.");

        var userSysId = await _context.Users
            .Where(u => u.UserId == userGuid)
            .Select(u => (int?)u.UserSysId)
            .FirstOrDefaultAsync() ?? 0;

        if (userSysId <= 0) return NotFound("User not found for given userId.");

        // ✅ IMPORTANT: Do NOT filter by req.scoId for session lookup (often unstable / null on relaunch)
        var lastSession = await _context.ScormAiccSessions
            .Where(s => s.Userid == userSysId && s.Scormid == req.scormId)
            .OrderByDescending(s => s.Attempt)
            .FirstOrDefaultAsync();

        int lastAttempt = lastSession?.Attempt ?? 0;

        bool reuseLastIncomplete =
            !req.forceNewAttempt &&
            lastSession != null &&
            (string.Equals(lastSession.Scormstatus, "incomplete", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(lastSession.Lessonstatus, "incomplete", StringComparison.OrdinalIgnoreCase));

        var nowDt = DateTime.UtcNow;

        ScormAiccSession sessionToUse;
        int attemptToUse;

        if (reuseLastIncomplete)
        {
            sessionToUse = lastSession!;
            attemptToUse = lastAttempt;

            // keep SAME attempt, new registrationId
            sessionToUse.Hacpsession = Guid.NewGuid().ToString("N");
            sessionToUse.Timemodified = nowDt;
            await _context.SaveChangesAsync();
        }
        else
        {
            attemptToUse = lastAttempt + 1;

            // ✅ choose a stable Scoid:
            // 1) if request sends it, use it
            // 2) else reuse lastSession.Scoid
            var stableScoId = req.scoId ?? lastSession?.Scoid;

            sessionToUse = new ScormAiccSession
            {
                Userid = userSysId,
                Scormid = req.scormId,
                Scoid = stableScoId,
                Attempt = attemptToUse,
                Scormmode = "normal",
                Scormstatus = "incomplete",
                Lessonstatus = "incomplete",
                Hacpsession = Guid.NewGuid().ToString("N"),
                Timecreated = nowDt,
                Timemodified = nowDt
            };

            _context.ScormAiccSessions.Add(sessionToUse);
            await _context.SaveChangesAsync();
        }

        // ✅ preload MUST be from the SAME Scoid + SAME Attempt you are resuming
        int? trackScoId = sessionToUse.Scoid;

        var tracksQuery = _context.ScormScoesTracks
            .Where(t => t.Userid == userSysId
                     && t.Scormid == req.scormId
                     && t.Attempt == attemptToUse);

        if (trackScoId.HasValue)
            tracksQuery = tracksQuery.Where(t => t.Scoid == trackScoId);

        var tracks = await tracksQuery.ToListAsync();

        var preload = tracks
            .Where(t => !string.IsNullOrWhiteSpace(t.Element))
            .GroupBy(t => t.Element!)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.Timemodified).First().Value ?? ""
            );

        // defaults
        preload.TryAdd("cmi.core.lesson_status", "incomplete");
        preload.TryAdd("cmi.completion_status", "incomplete");
        preload.TryAdd("cmi.success_status", "unknown");
        preload.TryAdd("cmi.core.score.raw", "");
        preload.TryAdd("cmi.core.total_time", "0000:00:00.00");

        // ✅ resume keys
        preload.TryAdd("cmi.core.lesson_location", "");
        preload.TryAdd("cmi.location", "");
        preload.TryAdd("cmi.suspend_data", "");

        return Ok(new InitResponse(
            sessionToUse.Hacpsession,
            (trackScoId?.ToString() ?? ""),
            attemptToUse,
            preload
        ));
    }

    [HttpPost("{registrationId}/commit")]
    public async Task<IActionResult> Commit(string registrationId, [FromBody] CommitRequest req)
    {
        var session = await _context.ScormAiccSessions.FirstOrDefaultAsync(s => s.Hacpsession == registrationId);
        if (session == null) return NotFound("Invalid registrationId.");
        if (req.data == null || req.data.Count == 0) return Ok();

        int? reqScoId = ParseNullableInt(req.scoId) ?? session.Scoid;
        var nowUnix = UnixNow();

        var normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in req.data)
        {
            var element = (item.element ?? "").Trim();
            if (string.IsNullOrWhiteSpace(element)) continue;

            var value = item.value ?? "";
            normalized[element] = value;

            var existing = await _context.ScormScoesTracks.FirstOrDefaultAsync(t =>
                t.Userid == session.Userid &&
                t.Scormid == session.Scormid &&
                t.Scoid == reqScoId &&
                t.Attempt == session.Attempt &&
                t.Element == element);

            if (existing == null)
            {
                _context.ScormScoesTracks.Add(new ScormScoesTrack
                {
                    Userid = session.Userid,
                    Scormid = session.Scormid,
                    Scoid = reqScoId,
                    Attempt = session.Attempt,
                    Element = element,
                    Value = value,
                    Timemodified = nowUnix
                });
            }
            else
            {
                existing.Value = value;
                existing.Timemodified = nowUnix;
            }
        }

        normalized.TryGetValue("cmi.core.lesson_status", out var v12);
        normalized.TryGetValue("cmi.completion_status", out var completion2004);
        normalized.TryGetValue("cmi.success_status", out var success2004);

        var lessonValue = LessonValue(v12, success2004, completion2004);
        session.Lessonstatus = lessonValue;

        bool completedFromCommit =
            IsCompletedLike(v12) ||
            (completion2004?.Trim().ToLowerInvariant() == "completed") ||
            IsCompletedLike(success2004);

        session.Scormstatus = completedFromCommit ? "completed" : "incomplete";

        session.Timemodified = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("{registrationId}/finish")]
    public async Task<IActionResult> Finish(string registrationId, [FromBody] FinishRequest req)
    {
        var session = await _context.ScormAiccSessions.FirstOrDefaultAsync(s => s.Hacpsession == registrationId);
        if (session == null) return NotFound("Invalid registrationId.");

        int? trackScoId = session.Scoid;

        async Task<string?> Latest(string element) =>
            await _context.ScormScoesTracks
                .Where(t => t.Userid == session.Userid
                         && t.Scormid == session.Scormid
                         && t.Scoid == trackScoId
                         && t.Attempt == session.Attempt
                         && t.Element == element)
                .OrderByDescending(t => t.Timemodified)
                .Select(t => t.Value)
                .FirstOrDefaultAsync();

        var lessonStatus12 = Normalize(await Latest("cmi.core.lesson_status"));
        var completion2004 = Normalize(await Latest("cmi.completion_status"));
        var success2004 = Normalize(await Latest("cmi.success_status"));

        var progress = ParseDouble(await Latest("cmi.progress_measure"));
        var threshold = ParseDouble(await Latest("cmi.completion_threshold"));

        bool completedByScormStatus =
            IsCompletedLike(lessonStatus12) ||
            (completion2004?.Trim().ToLowerInvariant() == "completed") ||
            IsCompletedLike(success2004);

        bool completedByProgress = ProgressMeansCompleted(progress, threshold);

        bool finalCompleted = completedByScormStatus || completedByProgress || req.client_completed;

        session.Scormstatus = finalCompleted ? "completed" : "incomplete";

        if (finalCompleted)
        {
            var lv = LessonValue(lessonStatus12, success2004, completion2004);
            session.Lessonstatus = (lv == "incomplete") ? "completed" : lv;
        }
        else
        {
            session.Lessonstatus = "incomplete";
        }

        session.Sessiontime = req.session_time;
        session.Timemodified = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok();
    }
}
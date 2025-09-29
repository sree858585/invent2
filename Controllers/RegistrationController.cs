using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Data;
using HIVTraining_Vue.Server.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace HIVTraining_Vue.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RegistrationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        [HttpGet("lookups")]
        public async Task<IActionResult> GetLookups()
        {
            // Each fetch is isolated; one bad lookup won’t 500 the whole endpoint.
            var workSettings = await SafeList(_context.LkWorkSettings);             // List<(int code, string value)>
            var educationLevels = await SafeList(_context.LkEducations);
            var ethnicities = await SafeList(_context.LkEthnicities);
            var races = await SafeList(_context.LkRaces);
            var occupations = await SafeList(_context.LkOccupations);
            var yearsCurrentOcc = await SafeList(_context.LkYearsCurrentOccupations);

            // New tables with specific UI shape (already objects with properties)
            var pronouns = await SafeListSpecific(_context.LkPronouns, keyName: null, labelName: null);
            var workLocations = await SafeListSpecific(_context.LkWorkLocations, keyName: null, labelName: null);

            // Project tuples -> anonymous objects so System.Text.Json has real properties
            // and sort by value/label respectively.
            var ws = workSettings.Select(x => new { code = x.code, value = x.value }).OrderBy(x => x.value).ToList();
            var el = educationLevels.Select(x => new { code = x.code, value = x.value }).OrderBy(x => x.value).ToList();
            var eth = ethnicities.Select(x => new { code = x.code, value = x.value }).OrderBy(x => x.value).ToList();
            var rc = races.Select(x => new { code = x.code, value = x.value }).OrderBy(x => x.value).ToList();
            var oc = occupations.Select(x => new { code = x.code, value = x.value }).OrderBy(x => x.value).ToList();
            var yo = yearsCurrentOcc.Select(x => new { code = x.code, value = x.value }).OrderBy(x => x.value).ToList();

            var pr = pronouns.OrderBy(x => (string)(x.label ?? string.Empty)).ToList();
            var wl = workLocations.OrderBy(x => (string)(x.label ?? string.Empty)).ToList();

            // quick diagnostics in server logs
            Console.WriteLine($"[LOOKUPS] counts → WS:{ws.Count} EL:{el.Count} ETH:{eth.Count} R:{rc.Count} OCC:{oc.Count} YCO:{yo.Count} PR:{pr.Count} WL:{wl.Count}");
            try
            {
                var rawOcc = await _context.LkOccupations.AsNoTracking().ToListAsync();
                Console.WriteLine($"[LOOKUPS] LkOccupations raw count: {rawOcc.Count}");
                if (rawOcc.Count > 0)
                {
                    var sample = rawOcc[0];
                    Console.WriteLine($"[LOOKUPS] LkOccupations first row: {System.Text.Json.JsonSerializer.Serialize(sample)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOOKUPS] LkOccupations sanity read failed: {ex.Message} {ex.InnerException?.Message}");
            }
            return Ok(new
            {
                WorkSettings = ws,
                EducationLevels = el,
                Ethnicities = eth,
                Races = rc,
                Occupations = oc,
                YearsCurrentOccupation = yo,
                Pronouns = pr,
                WorkLocations = wl
            });

            // ---------- helpers (unchanged) ----------
            async Task<List<(int code, string value)>> SafeList<T>(DbSet<T> set) where T : class
            {
                try
                {
                    var list = await set.AsNoTracking().ToListAsync();
                    return ToCodeValueListEF(list);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LOOKUPS] {typeof(T).Name} failed: {ex.Message}");
                    return new List<(int code, string value)>();
                }
            }

            async Task<List<dynamic>> SafeListSpecific<T>(DbSet<T> set, string? keyName, string? labelName) where T : class
            {
                try
                {
                    var list = await set.AsNoTracking().ToListAsync();
                    var keyProp = GetPkProp(typeof(T));
                    var labelProp = PickLabelProp(typeof(T), labelName);

                    return list.Select(item => new
                    {
                        id = Convert.ToInt32(keyProp.GetValue(item) ?? 0),
                        label = labelProp?.GetValue(item)?.ToString() ?? keyProp.Name
                    })
                    .Select(x =>
                    {
                        var typeName = typeof(T).Name; // e.g. LkPronoun, LkWorkLocation
                        if (typeName.Equals("LkPronoun", StringComparison.OrdinalIgnoreCase))
                            return new { pronounId = x.id, label = x.label } as dynamic;
                        if (typeName.Equals("LkWorkLocation", StringComparison.OrdinalIgnoreCase))
                            return new { workLocationId = x.id, label = x.label } as dynamic;
                        return new { id = x.id, label = x.label } as dynamic; // fallback
                    })
                    .ToList<dynamic>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LOOKUPS] {typeof(T).Name} (specific) failed: {ex.Message}");
                    return new List<dynamic>();
                }
            }

            PropertyInfo GetPkProp(Type t)
            {
                // Try EF metadata first
                var et = _context.Model.FindEntityType(t);
                var efPkProp = et?.FindPrimaryKey()?.Properties.FirstOrDefault()?.PropertyInfo;
                if (efPkProp != null)
                    return efPkProp;

                // Fallbacks for keyless entities / unusual mappings
                // Try common names
                var byName =
                    t.GetProperty("Id") ??
                    t.GetProperty($"{t.Name}Id") ??
                    t.GetProperty("Code") ??                        // <— matches your LkOccupations
                    t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                     .FirstOrDefault(p => p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase)) ??
                    // Finally: first integral numeric property
                    t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                     .FirstOrDefault(p =>
                         p.PropertyType == typeof(int) ||
                         p.PropertyType == typeof(long) ||
                         p.PropertyType == typeof(short));

                if (byName == null)
                    throw new InvalidOperationException($"No key-like property found for type {t.Name}.");

                return byName;
            }


            PropertyInfo? PickLabelProp(Type t, string? preferred)
            {
                if (!string.IsNullOrEmpty(preferred))
                    return t.GetProperty(preferred);

                return t.GetProperty("Value")
                    ?? t.GetProperty("Name")
                    ?? t.GetProperty("Label")
                    ?? t.GetProperty("Description")
                    ?? t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                         .FirstOrDefault(p => p.PropertyType == typeof(string));
            }

            List<(int code, string value)> ToCodeValueListEF<T>(IEnumerable<T> items)
            {
                var t = typeof(T);
                var pkProp = GetPkProp(t);                 // now resilient for keyless entities
                var labelProp = PickLabelProp(t, null);    // Value -> Name -> Label -> Description -> first string

                var list = new List<(int code, string value)>();
                foreach (var item in items)
                {
                    // code
                    var codeObj = pkProp.GetValue(item);
                    int code;
                    try
                    {
                        code = codeObj == null ? 0 : Convert.ToInt32(codeObj);
                    }
                    catch
                    {
                        // if conversion fails, skip this row
                        continue;
                    }

                    // label
                    var valueStr = labelProp?.GetValue(item)?.ToString();
                    if (string.IsNullOrWhiteSpace(valueStr))
                    {
                        // last-ditch: first string property
                        var anyStringProp = t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                             .FirstOrDefault(p => p.PropertyType == typeof(string));
                        valueStr = anyStringProp?.GetValue(item)?.ToString() ?? code.ToString();
                    }

                    list.Add((code, valueStr));
                }

                return list;
            }
        }
        private static List<(int code, string value)> ToCodeValueList<T>(IEnumerable<T> items)
        {
            var t = typeof(T);
            var keyProp = t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                           .FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null)
                       ?? t.GetProperty("Id")
                       ?? t.GetProperties().FirstOrDefault(p => p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                       ?? t.GetProperties().FirstOrDefault(p => p.Name.EndsWith("ID", StringComparison.OrdinalIgnoreCase));

            if (keyProp == null)
                throw new InvalidOperationException($"No key property found for type {t.Name}.");

            var valueProp = t.GetProperty("Value")
                         ?? t.GetProperty("Name")
                         ?? t.GetProperty("Label")
                         ?? t.GetProperty("Description")
                         ?? t.GetProperty("Text");

            return items.Select(item =>
            {
                var codeObj = keyProp.GetValue(item);
                // convert any numeric to int (safe for typical lookup PKs)
                var code = codeObj == null ? 0 : Convert.ToInt32(codeObj);

                string value = "";
                if (valueProp != null)
                {
                    value = valueProp.GetValue(item)?.ToString() ?? "";
                }
                else
                {
                    // fallback: first string property we can find
                    var anyStringProp = t.GetProperties().FirstOrDefault(p => p.PropertyType == typeof(string));
                    value = anyStringProp?.GetValue(item)?.ToString() ?? code.ToString();
                }

                return (code, value);
            })
                   .ToList();
        }

        // ✅ POST Register a New User
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] JsonElement userData)
        {
            try
            {
                // ===== Required strings =====
                string email = GetJsonString(userData, "email");
                string password = GetJsonString(userData, "password");
                string passwordRecoveryQuestion = GetJsonString(userData, "passwordRecoveryQuestion");
                string passwordRecoveryAnswer = GetJsonString(userData, "passwordRecoveryAnswer");
                string firstName = GetJsonString(userData, "firstName");
                string lastName = GetJsonString(userData, "lastName");
                string workPhone = GetJsonString(userData, "workPhone");

                // ===== Required lookup/flag =====
                int? pronounId = GetJsonInt(userData, "pronounId");
                bool? primaryCanText = GetJsonBool(userData, "primaryCanText");
                //bool? primaryCanText = null;
                //if (userData.TryGetProperty("primaryCanText", out var pctProp))
                //{
                //    if (pctProp.ValueKind is JsonValueKind.True or JsonValueKind.False)
                //        primaryCanText = pctProp.GetBoolean();
                //}

                // ===== Optional =====
                string mi = GetJsonString(userData, "mi");
                string altPhone = GetJsonString(userData, "altPhone");
                bool? altCanText = GetJsonBool(userData, "altCanText");
                int? workLocationId = GetJsonInt(userData, "workLocationId");
                int? workSetting = GetJsonInt(userData, "workSetting");
                int? ethnicity = GetJsonInt(userData, "ethnicity");
                int? race = GetJsonInt(userData, "race");
                int? occupation = GetJsonInt(userData, "occupation");

                // ===== Backend validations (ONLY required fields) =====
                if (string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(password) ||
                    string.IsNullOrWhiteSpace(passwordRecoveryQuestion) ||
                    string.IsNullOrWhiteSpace(passwordRecoveryAnswer) ||
                    string.IsNullOrWhiteSpace(firstName) ||
                    string.IsNullOrWhiteSpace(lastName) ||
                    string.IsNullOrWhiteSpace(workPhone) ||
                    pronounId is null ||
                    primaryCanText is null)
                {
                    return BadRequest(new
                    {
                        message = "Missing one or more required fields.",
                        required = new[] {
                    "firstName","lastName","pronounId","email","password",
                    "passwordRecoveryQuestion","passwordRecoveryAnswer",
                    "workPhone","primaryCanText"
                }
                    });
                }

                // ===== Identity: ensure unique, then create =====
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                    return BadRequest(new { message = "User already exists with this email." });

                var applicationUser = new ApplicationUser
                {
                    UserName = email,
                    NormalizedUserName = email.ToUpperInvariant(),
                    Email = email,
                    NormalizedEmail = email.ToUpperInvariant(),
                    SecurityStamp = Guid.NewGuid().ToString(),
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(applicationUser, password);
                if (!result.Succeeded)
                    return BadRequest(new { message = "Failed to create user in Identity", errors = result.Errors });

                try { await _userManager.AddToRoleAsync(applicationUser, "User"); }
                catch (Exception ex) { return StatusCode(500, $"Error saving user-role mapping: {ex.Message} {ex.InnerException?.Message}"); }

                // Store security Q/A
                await _userManager.AddClaimAsync(applicationUser, new Claim("PasswordRecoveryQuestion", passwordRecoveryQuestion));
                await _userManager.AddClaimAsync(applicationUser, new Claim("PasswordRecoveryAnswer", EncryptData(passwordRecoveryAnswer)));

                // ===== Optional fields normalization =====
                // If your DB columns are NULLABLE (recommended), keep these as null when not provided.
                // If they are NOT NULL ints in the DB, switch the coalescing to "?? 0".
                int? nzInt(int? v) => (v.HasValue && v.Value > 0) ? v : null;
                string? nzStr(string s) => string.IsNullOrWhiteSpace(s) ? null : s;

                var newUser = new User
                {
                    UserId = Guid.Parse(applicationUser.Id),

                    // Names
                    FirstName = firstName,
                    Mi = nzStr(mi),
                    LastName = lastName,

                    // Contact
                    Email = email,
                    WorkPhone = workPhone,
                    Phone = null,
                    CellPhone = nzStr(altPhone),

                    // Textable flags
                    PrimaryCanText = primaryCanText,   // REQUIRED, already validated
                    AltCanText = altCanText,           // OPTIONAL

                    // Lookups (OPTIONAL)
                    PronounId = pronounId,             // REQUIRED
                    WorkLocationId = nzInt(workLocationId), // or: (workLocationId ?? 0) if NOT NULL in DB
                    WorkSetting = nzInt(workSetting),    // or: (workSetting ?? 0)
                    Ethnicity = nzInt(ethnicity),      // or: (ethnicity ?? 0)
                    Race = nzInt(race),           // or: (race ?? 0)
                    Occupation = nzInt(occupation),     // or: (occupation ?? 0)

                    // Legacy/unused
                    Address = null,
                    City = null,
                    State = null,
                    Zip = null,
                    Country = null,
                    Organization = null,
                    Education = null,
                    YearsCurrentOccupation = null,

                    // Admin
                    DateEntered = DateTime.UtcNow,
                    DateModified = DateTime.UtcNow,
                    Active = true,
                    Role = Guid.NewGuid()
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                return Ok(new { message = "User registered successfully!" });
            }
            catch (DbUpdateException ex)
            {
                // If something optional still violates a NOT NULL constraint, surface a clear message.
                return StatusCode(500, $"Failed to save user details. An optional field may be NOT NULL in the database. Details: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        // ✅ Get JSON String Property (Handles Missing Keys)
        private string GetJsonString(JsonElement json, string name)
        {
            if (!json.TryGetProperty(name, out var v)) return string.Empty;
            if (v.ValueKind == JsonValueKind.Null || v.ValueKind == JsonValueKind.Undefined) return string.Empty;
            return v.ValueKind == JsonValueKind.String ? (v.GetString() ?? string.Empty) : v.ToString();
        }

        // ✅ Get JSON Integer Property (Handles Missing Keys)
        private int? GetJsonInt(JsonElement json, string name)
        {
            if (!json.TryGetProperty(name, out var v)) return null;

            switch (v.ValueKind)
            {
                case JsonValueKind.Number:
                    if (v.TryGetInt32(out var i)) return i;
                    if (v.TryGetInt64(out var l)) return unchecked((int)l);
                    if (v.TryGetDouble(out var d)) return (int)Math.Truncate(d);
                    return null;
                case JsonValueKind.String:
                    return int.TryParse(v.GetString(), out var si) ? si : (int?)null;
                case JsonValueKind.True:
                    return 1;
                case JsonValueKind.False:
                    return 0;
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                default:
                    return null;
            }
        }

        private bool? GetJsonBool(JsonElement json, string name)
        {
            if (!json.TryGetProperty(name, out var v)) return null;

            switch (v.ValueKind)
            {
                case JsonValueKind.True: return true;
                case JsonValueKind.False: return false;
                case JsonValueKind.String:
                    var s = v.GetString();
                    if (bool.TryParse(s, out var b)) return b;
                    if (int.TryParse(s, out var ib)) return ib != 0;
                    return null;
                case JsonValueKind.Number:
                    if (v.TryGetInt32(out var i)) return i != 0;
                    if (v.TryGetInt64(out var l)) return l != 0;
                    if (v.TryGetDouble(out var d)) return Math.Abs(d) > double.Epsilon;
                    return null;
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                default:
                    return null;
            }
        }

        // ✅ Encryption Method for Password Answer
        private string EncryptData(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}

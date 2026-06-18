using System.Text.Json;
using System.Text.Json.Serialization;
using CheckListTracker.Models;
using Microsoft.JSInterop;

namespace CheckListTracker.Services;

public class ChecklistService(IJSRuntime js)
{
    private readonly IJSRuntime _js = js;
    private List<ChecklistTemplate>? _templates;
    private List<ChecklistExecution>? _executions;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<List<ChecklistTemplate>> GetTemplatesAsync()
    {
        if (_templates != null) return _templates;
        var json = await _js.InvokeAsync<string?>("lsGet", "fatsat_templates");
        _templates = Deserialize<List<ChecklistTemplate>>(json) ?? [];
        if (_templates.Count == 0)
            await SeedDefaultTemplateAsync();
        return _templates;
    }

    public async Task<ChecklistTemplate?> GetTemplateAsync(string id)
    {
        var list = await GetTemplatesAsync();
        return list.FirstOrDefault(t => t.Id == id);
    }

    public async Task SaveTemplateAsync(ChecklistTemplate tpl)
    {
        var list = await GetTemplatesAsync();
        var idx = list.FindIndex(t => t.Id == tpl.Id);
        if (idx >= 0) list[idx] = tpl; else list.Add(tpl);
        await PersistAsync("fatsat_templates", list);
    }

    public async Task DeleteTemplateAsync(string id)
    {
        var list = await GetTemplatesAsync();
        list.RemoveAll(t => t.Id == id);
        await PersistAsync("fatsat_templates", list);
    }

    public async Task<List<ChecklistExecution>> GetExecutionsAsync()
    {
        if (_executions != null) return _executions;
        var json = await _js.InvokeAsync<string?>("lsGet", "fatsat_executions");
        _executions = Deserialize<List<ChecklistExecution>>(json) ?? [];
        return _executions;
    }

    public async Task<ChecklistExecution?> GetExecutionAsync(string id)
    {
        var list = await GetExecutionsAsync();
        return list.FirstOrDefault(e => e.Id == id);
    }

    public async Task SaveExecutionAsync(ChecklistExecution exec)
    {
        var list = await GetExecutionsAsync();
        var idx = list.FindIndex(e => e.Id == exec.Id);
        if (idx >= 0) list[idx] = exec; else list.Add(exec);
        await PersistAsync("fatsat_executions", list);
    }

    public async Task DeleteExecutionAsync(string id)
    {
        var list = await GetExecutionsAsync();
        list.RemoveAll(e => e.Id == id);
        await PersistAsync("fatsat_executions", list);
    }

    public async Task<ChecklistExecution> CreateFromTemplateAsync(ChecklistTemplate tpl, ChecklistType type)
    {
        var exec = new ChecklistExecution
        {
            TemplateId = tpl.Id,
            TemplateName = tpl.Name,
            Technology = tpl.Technology,
            Type = type,
            Sections = tpl.Sections.Select(CloneSection).ToList(),
            Results = tpl.Sections.SelectMany(s => s.Items)
                        .Select(i => new ItemResult { ItemId = i.Id }).ToList()
        };
        await SaveExecutionAsync(exec);
        return exec;
    }

    public static ExecutionStats GetStats(ChecklistExecution exec)
    {
        var r = exec.Results;
        return new ExecutionStats(
            r.Count,
            r.Count(x => x.Status == ItemStatus.Pass),
            r.Count(x => x.Status == ItemStatus.Fail),
            r.Count(x => x.Status == ItemStatus.NA),
            r.Count(x => x.Status == ItemStatus.Observation),
            r.Count(x => x.Status == ItemStatus.Pending));
    }

    private static TemplateSection CloneSection(TemplateSection s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        Order = s.Order,
        Items = s.Items.Select(i => new TemplateItem
        {
            Id = i.Id,
            Description = i.Description,
            Reference = i.Reference,
            IsRequired = i.IsRequired,
            Order = i.Order
        }).ToList()
    };

    private async Task PersistAsync<T>(string key, T value)
    {
        await _js.InvokeVoidAsync("lsSet", key, JsonSerializer.Serialize(value, JsonOpts));
    }

    private static T? Deserialize<T>(string? json)
    {
        if (string.IsNullOrEmpty(json)) return default;
        try { return JsonSerializer.Deserialize<T>(json, JsonOpts); }
        catch { return default; }
    }

    private async Task SeedDefaultTemplateAsync()
    {
        var tpl = new ChecklistTemplate
        {
            Name = "Všeobecný stroj / zariadenie",
            Technology = "Generický",
            Description = "Štandardná šablóna FAT/SAT pre všeobecné stroje a zariadenia",
            Sections =
            [
                new TemplateSection
                {
                    Title = "1. Dokumentácia",
                    Order = 0,
                    Items =
                    [
                        new TemplateItem { Description = "Technická dokumentácia je kompletná a schválená", Reference = "ISO 12100", Order = 0 },
                        new TemplateItem { Description = "Návod na obsluhu je k dispozícii v požadovanom jazyku", Order = 1 },
                        new TemplateItem { Description = "Prehlásenie o zhode (CE) je vydané", Reference = "2006/42/EC", Order = 2 },
                        new TemplateItem { Description = "Schémy zapojenia (elektrické, pneumatické, hydraulické) sú priložené", Order = 3 },
                        new TemplateItem { Description = "Zoznam náhradných dielov je k dispozícii", Order = 4, IsRequired = false }
                    ]
                },
                new TemplateSection
                {
                    Title = "2. Mechanická inšpekcia",
                    Order = 1,
                    Items =
                    [
                        new TemplateItem { Description = "Rozmery stroja zodpovedajú výkresovej dokumentácii", Order = 0 },
                        new TemplateItem { Description = "Povrchová úprava je bez defektov (lak, anodizácia, pozink)", Order = 1 },
                        new TemplateItem { Description = "Všetky skrutky a upevňovacie prvky sú správne utiahnuté", Order = 2 },
                        new TemplateItem { Description = "Pohyblivé časti sa pohybujú plynulo bez zasekávania", Order = 3 },
                        new TemplateItem { Description = "Tesnosť hydraulickych/pneumatických okruhov (bez únikov)", Order = 4 },
                        new TemplateItem { Description = "Identifikačné štítky a typový štítok sú čitateľné", Order = 5 }
                    ]
                },
                new TemplateSection
                {
                    Title = "3. Elektrická inšpekcia",
                    Order = 2,
                    Items =
                    [
                        new TemplateItem { Description = "Napájacie napätie zodpovedá špecifikácii", Reference = "IEC 60204", Order = 0 },
                        new TemplateItem { Description = "Uzemenie je správne prevedené a otestované", Order = 1 },
                        new TemplateItem { Description = "Kabeláž zodpovedá elektrickým schémam", Order = 2 },
                        new TemplateItem { Description = "Ochrana motorov a pohonov (ističe, relé) je správne nastavená", Order = 3 },
                        new TemplateItem { Description = "Núdzové zastavenie (E-STOP) funguje správne", Reference = "ISO 13850", Order = 4 },
                        new TemplateItem { Description = "Bezpečnostné PLC/relé sú správne nakonfigurované", Order = 5 }
                    ]
                },
                new TemplateSection
                {
                    Title = "4. Bezpečnostné funkcie",
                    Order = 3,
                    Items =
                    [
                        new TemplateItem { Description = "Ochranné kryty a bariéry sú na mieste a funkčné", Reference = "ISO 13857", Order = 0 },
                        new TemplateItem { Description = "Bezpečnostné dvere/zámky fungujú správne (interlock)", Order = 1 },
                        new TemplateItem { Description = "Svetelné závesy / laserové skenery (ak sú) sú nakalibrované", Order = 2, IsRequired = false },
                        new TemplateItem { Description = "Bezpečnostné označenia (výstražné štítky) sú na mieste", Order = 3 },
                        new TemplateItem { Description = "Hodnotenie rizika zodpovedá skutočnému stavu stroja", Reference = "ISO 12100", Order = 4 }
                    ]
                },
                new TemplateSection
                {
                    Title = "5. Funkčné testy",
                    Order = 4,
                    Items =
                    [
                        new TemplateItem { Description = "Spustenie a zastavenie stroja bez chýb", Order = 0 },
                        new TemplateItem { Description = "Ručný režim (JOG) funguje správne", Order = 1 },
                        new TemplateItem { Description = "Automatický cyklus funguje podľa špecifikácie", Order = 2 },
                        new TemplateItem { Description = "Presnosť / opakovateľnosť (meranie vzorky)", Order = 3 },
                        new TemplateItem { Description = "Rýchlosť / výkon zodpovedá požiadavkám", Order = 4 },
                        new TemplateItem { Description = "Systém hlásenia chýb a alarmy fungujú správne", Order = 5 },
                        new TemplateItem { Description = "Dlhodobý cyklický test (min. 30 min bez chýb)", Order = 6, IsRequired = false }
                    ]
                },
                new TemplateSection
                {
                    Title = "6. Prevzatie a záver",
                    Order = 5,
                    Items =
                    [
                        new TemplateItem { Description = "Všetky zistené odchýlky boli odstránené alebo sú akceptované", Order = 0 },
                        new TemplateItem { Description = "Školenie obsluhy bolo vykonané", Order = 1, IsRequired = false },
                        new TemplateItem { Description = "Stroj je čistý a pripravený na expedíciu/inštaláciu", Order = 2 },
                        new TemplateItem { Description = "Zákazník súhlasí s výsledkami testu", Order = 3 }
                    ]
                }
            ]
        };
        _templates!.Add(tpl);
        await PersistAsync("fatsat_templates", _templates);
    }
}

public record ExecutionStats(int Total, int Pass, int Fail, int NA, int Observation, int Pending)
{
    public int Done => Total - Pending;
    public double ProgressPct => Total == 0 ? 0 : Math.Round((double)Done / Total * 100, 0);
    public double PassPct => Total == 0 ? 0 : Math.Round((double)Pass / Total * 100, 1);
}

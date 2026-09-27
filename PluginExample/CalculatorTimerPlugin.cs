using System.Globalization;
using System.Text;
using Kodinn.Sdk;

namespace PluginExample;

/// <summary>
/// Calculator &amp; Timer: an example Kodinn plugin that is also useful.
/// <list type="bullet">
/// <item>calculate - a tool for the agent: models get arithmetic wrong, this does it exactly;</item>
/// <item>/calc expression - the same for the user, in the chat;</item>
/// <item>/timer minutes [label] - a focus timer, with a Kodinn notification when it ends;</item>
/// <item>a dock panel with the history of calculations and the running timers;</item>
/// <item>the history kept in the plugin's own database (plugin.json, "database" section): Kodinn
/// creates it at install time, encrypted with the user's PIN, and deletes it with the plugin.</item>
/// </list>
/// It declares only the capabilities it uses (plugin.json): no chat reading, no model.
/// </summary>
public sealed class CalculatorTimerPlugin : IKodinnPlugin
{
    private const int HistoryMax = 50;         // shown in the panel
    private const int HistoryKeep = 1000;      // kept in the database

    private IPluginCollection<HistoryEntry> _history = null!;

    private IKodinnHost _host = null!;
    private Texts _t = Texts.English;
    private readonly CalculatorState _state = new();

    public async Task ActivateAsync(IKodinnHost host, CancellationToken cancellationToken)
    {
        _host = host;
        _t = Texts.For(host.Language);
        _history = host.Database.Collection<HistoryEntry>("history");
        _state.Cleared += OnHistoryCleared;
        await LoadHistoryAsync();

        host.RegisterTool(new PluginTool
        {
            Name = "calculate",
            Description =
                "Evaluates an arithmetic expression exactly (decimal arithmetic, 28 significant digits). " +
                "Use it for ANY calculation instead of computing in your head: sums, percentages, money, " +
                "unit conversions written as arithmetic. Supports + - * / % ^, parentheses, pi, e and " +
                "sqrt, abs, round(x, digits), floor, ceil, min, max.",
            Parameters = [new PluginToolParameter("expression", "string", "The expression, e.g. 1250 * 1.22 - 15%", Required: true)],
            RequiresApproval = false,     // it cannot change anything
            ExecuteAsync = async (args, ct) =>
            {
                string expression = args.TryGetValue("expression", out var v) ? v?.ToString() ?? "" : "";
                return await CalculateAsync(expression, byAgent: true);
            },
        });

        host.RegisterCommand(new PluginCommand
        {
            Name = "calc",
            Description = _t.CalcCommand,
            ExecuteAsync = async (args, ct) => string.IsNullOrWhiteSpace(args)
                ? _t.CalcUsage
                : await CalculateAsync(args, byAgent: false),
        });

        host.RegisterCommand(new PluginCommand
        {
            Name = "timer",
            Description = _t.TimerCommand,
            ExecuteAsync = (args, ct) => Task.FromResult(StartTimer(args)),
        });

        host.RegisterPanel(new PluginPanel
        {
            Id = "calculator",
            Title = _t.PanelTitle,
            Icon = "🧮",
            ComponentType = typeof(CalculatorPanel),
            Parameters = new Dictionary<string, object?> { ["State"] = _state, ["Texts"] = _t },
        });

        host.Log.Info("Calculator & Timer active.");
    }

    public Task DeactivateAsync()
    {
        _state.CancelTimers();                        // no notification from a disabled plugin
        _state.Cleared -= OnHistoryCleared;
        return Task.CompletedTask;
    }

    private async Task<string> CalculateAsync(string expression, bool byAgent)
    {
        string text = expression.Trim().TrimEnd('=');
        try
        {
            string result = Calculator.Format(Calculator.Evaluate(text));
            var entry = new HistoryEntry { Expression = text, Result = result, ByAgent = byAgent, At = DateTimeOffset.UtcNow };
            _state.AddHistory(entry, HistoryMax);
            await SaveAsync(entry);
            return byAgent ? result : $"`{text}` = **{result}**";
        }
        catch (Exception ex) when (ex is FormatException or ArithmeticException or ArgumentException or OverflowException)
        {
            return byAgent ? $"Error: {ex.Message}" : $"{_t.CalcError}: {ex.Message}";
        }
    }

    private string StartTimer(string args)
    {
        var parts = args.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0 || !double.TryParse(parts[0].Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double minutes)
            || minutes <= 0 || minutes > 24 * 60)
            return _t.TimerUsage;

        string label = parts.Length > 1 ? parts[1].Trim() : _t.TimerDefaultLabel;
        var timer = _state.StartTimer(label, TimeSpan.FromMinutes(minutes), t =>
            _host.Notify(_t.TimerDoneTitle, string.Format(_t.TimerDoneBody, t.Label)));
        return string.Format(_t.TimerStarted, label, timer.EndsAt.ToLocalTime().ToString("HH:mm"));
    }

    private async Task LoadHistoryAsync()
    {
        try
        {
            // "at" is declared as an index in plugin.json: newest first, straight from the database.
            var latest = await _history.RangeAsync("at", from: null, to: null, descending: true, limit: HistoryMax);
            _state.SetHistory(latest.ToList());
        }
        catch (Exception ex) { _host.Log.Warning("History not readable, starting empty: " + ex.Message); }
    }

    private async Task SaveAsync(HistoryEntry entry)
    {
        try
        {
            await _history.InsertAsync(entry);
            int extra = await _history.CountAsync() - HistoryKeep;
            if (extra > 0)
                foreach (var old in await _history.RangeAsync("at", null, null, descending: false, limit: extra))
                    await _history.DeleteAsync(old.Id);
        }
        catch (Exception ex) { _host.Log.Warning("History not saved: " + ex.Message); }
    }

    private async void OnHistoryCleared()
    {
        try { await _history.DeleteAllAsync(); }
        catch (Exception ex) { _host.Log.Warning("History not cleared: " + ex.Message); }
    }
}

/// <summary>One calculation, as stored in the plugin's "history" collection.</summary>
public sealed class HistoryEntry : PluginEntity
{
    public string Expression { get; set; } = "";
    public string Result { get; set; } = "";
    public bool ByAgent { get; set; }
    /// <summary>UTC, so that the "at" index sorts chronologically.</summary>
    public DateTimeOffset At { get; set; }
}

public sealed class RunningTimer
{
    public required string Label { get; init; }
    public required DateTimeOffset EndsAt { get; init; }
    internal CancellationTokenSource Cancel { get; } = new();
}

/// <summary>State shared by the plugin and its dock panel; the panel re-renders on <see cref="Changed"/>.</summary>
public sealed class CalculatorState
{
    private readonly object _lock = new();
    private List<HistoryEntry> _history = [];
    private readonly List<RunningTimer> _timers = [];

    public event Action? Changed;
    /// <summary>The user cleared the history from the panel.</summary>
    public event Action? Cleared;

    public IReadOnlyList<HistoryEntry> History { get { lock (_lock) return _history.ToList(); } }
    public IReadOnlyList<RunningTimer> Timers { get { lock (_lock) return _timers.ToList(); } }

    public void SetHistory(List<HistoryEntry> history)
    {
        lock (_lock) _history = history;
        Changed?.Invoke();
    }

    public void AddHistory(HistoryEntry entry, int max)
    {
        lock (_lock)
        {
            _history.Insert(0, entry);
            if (_history.Count > max) _history.RemoveRange(max, _history.Count - max);
        }
        Changed?.Invoke();
    }

    public void ClearHistory()
    {
        lock (_lock) _history.Clear();
        Changed?.Invoke();
        Cleared?.Invoke();
    }

    public RunningTimer StartTimer(string label, TimeSpan duration, Action<RunningTimer> onDone)
    {
        var timer = new RunningTimer { Label = label, EndsAt = DateTimeOffset.Now + duration };
        lock (_lock) _timers.Add(timer);
        Changed?.Invoke();
        _ = Task.Delay(duration, timer.Cancel.Token).ContinueWith(t =>
        {
            lock (_lock) _timers.Remove(timer);
            Changed?.Invoke();
            if (!t.IsCanceled) onDone(timer);
        }, TaskScheduler.Default);
        return timer;
    }

    public void StopTimer(RunningTimer timer) => timer.Cancel.Cancel();

    public void CancelTimers()
    {
        foreach (var t in Timers) t.Cancel.Cancel();
    }
}

/// <summary>The plugin's own texts, in Kodinn's interface language (Italian or English).</summary>
public sealed class Texts
{
    public required string PanelTitle { get; init; }
    public required string CalcCommand { get; init; }
    public required string CalcUsage { get; init; }
    public required string CalcError { get; init; }
    public required string TimerCommand { get; init; }
    public required string TimerUsage { get; init; }
    public required string TimerDefaultLabel { get; init; }
    public required string TimerStarted { get; init; }
    public required string TimerDoneTitle { get; init; }
    public required string TimerDoneBody { get; init; }
    public required string History { get; init; }
    public required string HistoryEmpty { get; init; }
    public required string Clear { get; init; }
    public required string ByAgent { get; init; }
    public required string Timers { get; init; }
    public required string Stop { get; init; }

    public static Texts For(string language) =>
        language.StartsWith("it", StringComparison.OrdinalIgnoreCase) ? Italian : English;

    public static readonly Texts English = new()
    {
        PanelTitle = "Calculator & Timer",
        CalcCommand = "Exact calculation: /calc 1250 * 1.22",
        CalcUsage = "Write an expression after the command, e.g. `/calc (1250 + 80) * 1.22`.",
        CalcError = "Cannot calculate",
        TimerCommand = "Focus timer: /timer 25 review",
        TimerUsage = "Write the minutes and an optional label, e.g. `/timer 25 review`.",
        TimerDefaultLabel = "Timer",
        TimerStarted = "⏱️ **{0}** started: it ends at {1}.",
        TimerDoneTitle = "Time is up",
        TimerDoneBody = "{0} is over.",
        History = "History",
        HistoryEmpty = "No calculations yet. Try /calc 2^10.",
        Clear = "Clear",
        ByAgent = "agent",
        Timers = "Running timers",
        Stop = "Stop",
    };

    public static readonly Texts Italian = new()
    {
        PanelTitle = "Calcolatrice e timer",
        CalcCommand = "Calcolo esatto: /calc 1250 * 1,22",
        CalcUsage = "Scrivi un'espressione dopo il comando, per esempio `/calc (1250 + 80) * 1.22`.",
        CalcError = "Impossibile calcolare",
        TimerCommand = "Timer di concentrazione: /timer 25 revisione",
        TimerUsage = "Scrivi i minuti e un'etichetta facoltativa, per esempio `/timer 25 revisione`.",
        TimerDefaultLabel = "Timer",
        TimerStarted = "⏱️ **{0}** avviato: finisce alle {1}.",
        TimerDoneTitle = "Tempo scaduto",
        TimerDoneBody = "{0} è terminato.",
        History = "Storico",
        HistoryEmpty = "Ancora nessun calcolo. Prova /calc 2^10.",
        Clear = "Svuota",
        ByAgent = "agente",
        Timers = "Timer in corso",
        Stop = "Ferma",
    };
}

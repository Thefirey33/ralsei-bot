using System.ComponentModel;
using System.Runtime.CompilerServices;
using ralsei_bot_discord.Types;

namespace RalseiBot.Web.Components;

public sealed class GeneralSharedState : INotifyPropertyChanged
{
    /// <summary>
    ///     The state of the question box.
    /// </summary>
    public enum QuestionBoxState
    {
        NoAnswer,
        Yes,
        No
    }

    /// <summary>
    ///     The current state of the question box.
    /// </summary>
    public TaskCompletionSource<QuestionBoxState> QuestionBox = new(QuestionBoxState.NoAnswer);

    /// <summary>
    ///     The current guilds that the bot contains.
    /// </summary>
    public List<GuildData> CurrentGuilds { get; private set; } = [];

    /// <summary>
    ///     Is the question box open?
    /// </summary>
    public bool IsQuestionBoxOpen
    {
        get => field;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = false;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Open the question box for questions.
    /// </summary>
    public void OpenQuestionBox()
    {
        QuestionBox = new TaskCompletionSource<QuestionBoxState>(QuestionBoxState.NoAnswer);
        IsQuestionBoxOpen = true;
    }

    /// <summary>
    ///     This updates the value of the guild state in the API.
    /// </summary>
    /// <param name="guilds">The new list of guilds.</param>
    public void UpdateValue(List<GuildData>? guilds)
    {
        if (guilds != null) CurrentGuilds = guilds;
    }

    public async Task CheckIfImported(HttpClient httpClient)
    {
        if (CurrentGuilds.Count > 0)
            return;
        CurrentGuilds = await httpClient.GetFromJsonAsync<List<GuildData>>("Guild/all") ?? [];
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
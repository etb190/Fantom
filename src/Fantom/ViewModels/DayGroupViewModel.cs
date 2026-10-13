namespace Fantom.ViewModels;

/// <summary>A group of sessions sharing one local date ("Today", "Yesterday", ...).</summary>
public class DayGroupViewModel
{
    public string Header { get; }
    public string TotalText { get; }
    public IReadOnlyList<SessionRowViewModel> Sessions { get; }

    public DayGroupViewModel(string header, string totalText, IReadOnlyList<SessionRowViewModel> sessions)
    {
        Header = header;
        TotalText = totalText;
        Sessions = sessions;
    }
}

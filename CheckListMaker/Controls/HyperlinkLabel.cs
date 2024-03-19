namespace CheckListMaker.Controls;

/// <summary> HyperlinkLabel コントロール </summary>
public class HyperlinkLabel : Label
{
    /// <summary> UrlProperty </summary>
    public static readonly BindableProperty UrlProperty = BindableProperty.Create(nameof(Url), typeof(string), typeof(HyperlinkLabel), null);

    /// <summary> Constructor </summary>
    public HyperlinkLabel()
    {
        TextDecorations = TextDecorations.Underline;
        GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await Launcher.OpenAsync(Url)),
        });
    }

    /// <summary> Url プロパティ </summary>
    public string Url
    {
        get => (string)GetValue(UrlProperty);
        set => SetValue(UrlProperty, value);
    }
}

using System;

public class InputEvent
{
    public DateTime Date { get; set; }
    public string Text { get; set; }

    public InputEvent(DateTime date, string text)
    {
        Date = date;
        Text = text;
    }
}

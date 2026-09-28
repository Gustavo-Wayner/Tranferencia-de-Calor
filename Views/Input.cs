using Raylib_cs;
using RayGUI_cs;

namespace Grid.Views;

public class Input
{
    private Textbox box;
    public Textbox Box { get => box; set => box = value; }
    
    public string Text { get => Box.Text; set => Box.Text = value; }

    public Input()
    {
        Box = new Textbox(0, 0, 180, 30, "A");
        Box.Filter = Textbox.TextFilter.None;
    }
    
    public Input(int _x, int _y, string _text, int _width = 180, int _height = 30, Textbox.TextFilter _filter = Textbox.TextFilter.None)
    {
        Box = new Textbox(_x, _y, _width, _height, _text);
        Box.Filter = _filter;
    }
}
using System.Numerics;
using Raylib_cs;

namespace Grid.Views;

// funcoes de interface usadas pelo Programa e pelo Popup
public static class Ui
{
    // o mouse clicou dentro do retangulo?
    public static bool Clicked(Rectangle rect)
    {
        return Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rect)
            && Raylib.IsMouseButtonPressed(MouseButton.Left);
    }

    // botao desenhado na mao: retangulo com borda e texto centralizado
    public static void DrawButton(string text, Rectangle rect, Color color)
    {
        Raylib.DrawRectangleRec(rect, color);
        Raylib.DrawRectangleLinesEx(rect, 1, Color.White);
        Raylib.DrawText(
            text,
            (int)(rect.X + rect.Width / 2 - Raylib.MeasureText(text, 16) / 2),
            (int)(rect.Y + rect.Height / 2 - 8),
            16,
            Color.White
        );
    }
}

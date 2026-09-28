using System.Numerics;
using Raylib_cs;
using RayGUI_cs;

using Grid.Models;
using Grid.Views;
using Material = Grid.Models.Material;

namespace Grid.Controllers;

// menu de edicao de uma celula. so existe um, estatico: ou esta aberto ou fechado
public static class Popup
{
    public const int WIDTH = 200;
    public const int HEIGHT = 360;

    private static bool visible;
    public static bool Visible { get => visible; set => visible = value; }

    // celula que o popup esta editando: ele muda a celula direto pela referencia
    private static Material? cell;

    private static bool justOpened;
    private static int selectedMaterial;
    private static Rectangle rect;

    // o campo de texto fica num container proprio do popup
    private static readonly GuiContainer container = new();
    private static readonly Input tempInput = new(0, 0, "", 170, 28, Textbox.TextFilter.Naturals);

    static Popup()
    {
        container.Add("tempInput", tempInput.Box);
    }

    // abre o popup na celula clicada, ja preenchido com os dados dela
    public static void Open(Material clicked)
    {
        if (visible) return;

        visible = true;
        justOpened = true;
        cell = clicked;
        selectedMaterial = Material.IndexOf(clicked);
        tempInput.Text = $"{(int)clicked.Temperature}";

        rect = new Rectangle(
            (Raylib.GetScreenWidth() - WIDTH) / 2,
            (Raylib.GetScreenHeight() - HEIGHT) / 2,
            WIDTH,
            HEIGHT
        );
        tempInput.Box.X = (int)rect.X + 15;
        tempInput.Box.Y = (int)rect.Y + 58;
    }

    // ouve os cliques; escondido, nao faz nada
    public static void Act()
    {
        if (!visible) return;

        // ignora o clique que abriu o popup, senao ele fechava na hora
        if (justOpened)
        {
            justOpened = false;
            return;
        }

        if (!Raylib.IsMouseButtonPressed(MouseButton.Left)) return;

        // clique fora do popup cancela
        if (!Ui.Clicked(rect))
        {
            visible = false;
            return;
        }

        // botoes de material
        for (int k = 0; k < Material.Count; k++)
        {
            if (Ui.Clicked(MaterialRect(k)))
            {
                selectedMaterial = k;
            }
        }

        if (Ui.Clicked(OkRect()))
        {
            Apply();
        }
    }

    // desenha o popup; escondido, nao faz nada
    public static void Draw()
    {
        if (!visible) return;

        Raylib.DrawRectangleRec(rect, new Color(35, 35, 35, 255));
        Raylib.DrawRectangleLinesEx(rect, 2, Color.White);
        Raylib.DrawText("Editar celula", (int)rect.X + 15, (int)rect.Y + 12, 16, Color.White);
        Raylib.DrawText("Temperatura", (int)rect.X + 15, (int)rect.Y + 40, 12, Color.White);
        Raylib.DrawText("Material", (int)rect.X + 15, (int)rect.Y + 92, 12, Color.White);

        // botoes de material, o escolhido fica laranja
        for (int k = 0; k < Material.Count; k++)
        {
            Color color = k == selectedMaterial ? new Color(200, 120, 40, 255) : new Color(70, 70, 70, 255);
            Ui.DrawButton(Material.Labels[k], MaterialRect(k), color);
        }

        Ui.DrawButton("OK", OkRect(), new Color(70, 70, 70, 255));

        // caixa selecionada ganha fundo branco, entao o texto vai pra preto
        tempInput.Box.TextColor = tempInput.Box.Focus ? Color.Black : Color.White;
        container.Draw();
    }

    // salva temperatura e material na celula e fecha
    private static void Apply()
    {
        if (cell == null) return;

        if (int.TryParse(tempInput.Text, out int temp) && temp >= 0)
        {
            cell.Temperature = temp;
        }
        cell.Become(Material.FromIndex(selectedMaterial));
        visible = false;
    }

    private static Rectangle MaterialRect(int k)
    {
        return new Rectangle(rect.X + 15, rect.Y + 116 + k * 24, 170, 22);
    }

    private static Rectangle OkRect()
    {
        return new Rectangle(rect.X + 15, rect.Y + 116 + Material.Count * 24 + 10, 170, 28);
    }
}

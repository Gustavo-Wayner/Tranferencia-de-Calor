using System.Numerics;
using Raylib_cs;
using RayGUI_cs;
using Grid.Models;
using Grid.Views;
using Material = Grid.Models.Material;

namespace Grid.Controllers
{
    public static class Program
    {
        private const int DEFAULT_SIZE = 10;
        private const int DEFAULT_TEMP = 20;
        private const int PANEL_WIDTH = 220;

        public static void Main(string[] args)
        {
            Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
            Grid<Material> grid = BuildGrid($"{DEFAULT_SIZE}", $"{DEFAULT_TEMP}");

            // janela fixa: grade encostada na esquerda, painel na direita
            int gridPx = DEFAULT_SIZE * (Globals.CELL_SIZE + (int)Globals.GAP);
            Raylib.InitWindow(gridPx + PANEL_WIDTH, gridPx, "Transferencia de Calor");

            // sem isso os componentes usam um Font vazio e o texto nao aparece por algum motivo
            RayGUI.LoadGUI(new Dictionary<int, Font> { { 15, Raylib.GetFontDefault() } });
            Raylib.SetTargetFPS(60);

            GuiContainer container = new();

            Input SizeField = new(0, 45, "10", 180, 30, Textbox.TextFilter.Naturals);
            Input TempField = new(0, 110, "20", 180, 30, Textbox.TextFilter.Naturals);
            Input TimeField = new(0, 175, "0.1", 180, 30, Textbox.TextFilter.Decimals);

            container.Add("sizeBox", SizeField.Box);
            container.Add("tempBox", TempField.Box);
            container.Add("timeBox", TimeField.Box);

            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);
                grid.Step();
                grid.Draw();

                // clicar numa celula abre o popup nela
                foreach (Material material in grid.Plane)
                {
                    if (material.IsClicked())
                    {
                        Popup.Open(material);
                        break;
                    }
                }

                // cola na direita
                float panelX = Raylib.GetScreenWidth() - PANEL_WIDTH + 20f;
                SizeField.Box.X = (int)panelX;
                TempField.Box.X = (int)panelX;
                TimeField.Box.X = (int)panelX;

                Raylib.DrawText("Tamanho", (int)panelX, 25, 16, Color.White);
                Raylib.DrawText("Temperatura inicial", (int)panelX, 90, 16, Color.White);
                Raylib.DrawText("Velocidade", (int)panelX, 155, 16, Color.White);

                // Recarregar: recria a grade com o tamanho e temperatura dos campos
                Rectangle reloadButton = new(panelX - 20f, 235, 180, 30);
                Ui.DrawButton("Recarregar", reloadButton, new Color(70, 70, 70, 255));
                if (Ui.Clicked(reloadButton))
                {
                    Popup.Visible = false;
                    grid = BuildGrid(SizeField.Text, TempField.Text);
                    Globals.TIME_STEP = Parse<float>(TimeField.Text, 0.1f);
                }

                // caixa selecionada ganha fundo branco, entao o texto vai pra preto
                SizeField.Box.TextColor = SizeField.Box.Focus ? Color.Black : Color.White;
                TempField.Box.TextColor = TempField.Box.Focus ? Color.Black : Color.White;
                TimeField.Box.TextColor = TimeField.Box.Focus ? Color.Black : Color.White;

                Popup.Act();
                Popup.Draw();

                container.Draw();
                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }

        private static readonly Random random = new();

        // Pega uma random entre as 8 classes pra cada quadradinho
        private static Material RandomMaterial()
        {
            return Material.FromIndex(random.Next(Material.Count));
        }

        private static Grid<Material> BuildGrid(string sizeText, string tempText)
        {
            int size = Parse<int>(sizeText, DEFAULT_SIZE);
            Grid<Material> grid = new(size, size, RandomMaterial);
            grid.Fill(Parse<int>(tempText, DEFAULT_TEMP));
            return grid;
        }

        private static T Parse<T>(string text, T fallback)
        where T: IParsable<T>, INumber<T>
        {
            return T.TryParse(text, null, out T value) && value > T.Zero ? value : fallback;
        }
    }
}

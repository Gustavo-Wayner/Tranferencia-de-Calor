using System.Numerics;
using Raylib_cs;
using RayGUI_cs;

namespace Grid
{
    public static class Program
    {
        private const int DEFAULT_SIZE = 10;
        private const int DEFAULT_TEMP = 20;
        private const int PANEL_WIDTH = 220;

        // dimensões do popup de edição pra n cortar se a grade for pequena
        private const int POPUP_WIDTH = 200;
        private const int POPUP_HEIGHT = 360;
        private const int MIN_WINDOW_HEIGHT = POPUP_HEIGHT + 20;
        private const int MIN_WINDOW_WIDTH = PANEL_WIDTH + 20;

        public static void Main(string[] args)
        {
            string sizeText = $"{DEFAULT_SIZE}";
            string tempText = $"{DEFAULT_TEMP}";
            Grid<Material> grid = BuildGrid(sizeText, tempText);

            FitWindow(grid);
            // sem isso os componentes usam um Font vazio e o texto nao aparece
            RayGUI.LoadGUI(new Dictionary<int, Font> { { 15, Raylib.GetFontDefault() } });
            Raylib.SetTargetFPS(60);

            GuiContainer container = new GuiContainer();

            Input SizeField = new(0, 45, "10", 180, 30, Textbox.TextFilter.Naturals);
            Input TempField = new(0, 110, "20", 180, 30, Textbox.TextFilter.Naturals);
            Input TimeField = new(0, 175, "0.1", 180, 30, Textbox.TextFilter.Decimals);

            // popup de edicao: clicar numa celula abre temperatura + material
            bool popupOpen = false;
            bool popupJustOpened = false;
            int popupI = 0;
            int popupJ = 0;
            int selectedMaterial = 0;
            Rectangle popupRect = new(0, 0, 0, 0);

            Input PopupTempInput = new(0, 0, "", 170, 28, Textbox.TextFilter.Naturals);

            Button[] materialButtons = [.. MaterialLabels.Select((name, k) =>
            {
                Button button = new Button(0, 0, 170, 22, name);
                button.Event += () => selectedMaterial = k;
                return button;
            })];

            foreach (Button button in materialButtons)
            {
                button.HoverColor = new Color(110, 110, 110, 255);
                button.BorderColor = Color.White;
            }

            Button okButton = new Button(0, 0, 170, 28, "OK");
            okButton.Event += () =>
            {
                Material cell = grid.Plane[popupI, popupJ];
                if (int.TryParse(PopupTempInput.Text, out int newTemp) && newTemp >= 0)
                {
                    cell.Temperature = newTemp;
                }
                if (IndexOfMaterial(cell) != selectedMaterial)
                {
                    grid.Replace(popupI, popupJ, MaterialFromIndex(selectedMaterial));
                }
                ClosePopup();
            };

            void ClosePopup()
            {
                container.Remove("popupTempInput");
                for (int k = 0; k < materialButtons.Length; k++)
                {
                    container.Remove($"popupMaterial{k}");
                }
                container.Remove("popupOkButton");
                popupOpen = false;
            }

            void OpenPopup(int i, int j)
            {
                if (popupOpen) return;

                popupOpen = true;
                popupJustOpened = true;
                popupI = i;
                popupJ = j;
                selectedMaterial = IndexOfMaterial(grid.Plane[i, j]);
                PopupTempInput.Text = $"{(int)grid.Plane[i, j].Temperature}";
                popupRect = LayoutPopup(PopupTempInput.Box, materialButtons, okButton);

                container.Add("popupTempInput", PopupTempInput.Box);
                for (int k = 0; k < materialButtons.Length; k++)
                {
                    container.Add($"popupMaterial{k}", materialButtons[k]);
                }
                container.Add("popupOkButton", okButton);
            }

            grid.OnCellClick = OpenPopup;

            Button reloadButton = new Button(0, 235, 180, 30, "Recarregar");
            reloadButton.Event += () =>
            {
                grid = BuildGrid(SizeField.Text, TempField.Text);
                FitWindow(grid);
                MovePanel(grid, SizeField.Box, TempField.Box, TimeField.Box, reloadButton);
                Globals.TIME_STEP = Parse<float>(TimeField.Text, 0.1f);
                // a grade nova nasce sem handler, reconecta o popup nela
                grid.OnCellClick = OpenPopup;
            };

            container.Add("sizeBox", SizeField.Box);
            container.Add("tempBox", TempField.Box);
            container.Add("timeBox", TimeField.Box);
            container.Add("reloadButton", reloadButton);
            MovePanel(grid, SizeField.Box, TempField.Box, TimeField.Box, reloadButton);

            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);
                grid.Step();
                grid.Draw();

                float panelX = PanelX(grid);
                Raylib.DrawText("Tamanho", (int)panelX, 25, 16, Color.White);
                Raylib.DrawText("Temperatura inicial", (int)panelX, 90, 16, Color.White);
                Raylib.DrawText("Velocidade", (int)panelX, 155, 16, Color.White);

                if (popupOpen)
                {
                    Raylib.DrawRectangleRec(popupRect, new Color(35, 35, 35, 255));
                    Raylib.DrawRectangleLinesEx(popupRect, 2, Color.White);
                    Raylib.DrawText("Editar celula", (int)popupRect.X + 15, (int)popupRect.Y + 12, 16, Color.White);
                    Raylib.DrawText("Temperatura", (int)popupRect.X + 15, (int)popupRect.Y + 40, 12, Color.White);
                    Raylib.DrawText("Material", (int)popupRect.X + 15, (int)popupRect.Y + 92, 12, Color.White);
                }

                // caixa selecionada ganha fundo branco, entao o texto vai pra preto
                SizeField.Box.TextColor = SizeField.Box.Focus ? Color.Black : Color.White;
                TempField.Box.TextColor = TempField.Box.Focus ? Color.Black : Color.White;
                TimeField.Box.TextColor = TimeField.Box.Focus ? Color.Black : Color.White;
                PopupTempInput.Box.TextColor = PopupTempInput.Box.Focus ? Color.Black : Color.White;

                // material escolhido fica destacado na listinha
                for (int k = 0; k < materialButtons.Length; k++)
                {
                    materialButtons[k].BaseColor = k == selectedMaterial ? ChosenMaterial : IdleMaterial;
                }

                // clicar fora do popup fecha ele
                if (popupOpen && !popupJustOpened
                    && Raylib.IsMouseButtonPressed(MouseButton.Left)
                    && !Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), popupRect))
                {
                    ClosePopup();
                }
                popupJustOpened = false;

                container.Draw();
                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }

        private static readonly Random random = new();

        private static Material MaterialFromIndex(int index) => index switch
        {
            0 => new Copper(),
            1 => new Iron(),
            2 => new Aluminum(),
            3 => new Granite(),
            4 => new Glass(),
            5 => new Brick(),
            6 => new Wood(),
            _ => new Rubber(),
        };

        private static int IndexOfMaterial(Material cell) => cell switch
        {
            Copper => 0,
            Iron => 1,
            Aluminum => 2,
            Granite => 3,
            Glass => 4,
            Brick => 5,
            Wood => 6,
            _ => 7,
        };

        // nomes na ordem do seletor, reaproveitando o Label() de cada classe
        private static readonly List<string> MaterialLabels =
            [.. Enumerable.Range(0, 8).Select(i => MaterialFromIndex(i).Label())];

        // sorteia uma das 8 classes de material pra cada celula da grade
        private static Material RandomMaterial()
        {
            return MaterialFromIndex(random.Next(8));
        }

        private static Grid<Material> BuildGrid(string sizeText, string tempText)
        {
            int size = Parse<int>(sizeText, DEFAULT_SIZE);
            Grid<Material> grid = new(size, size, RandomMaterial);
            grid.Fill(Parse<int>(tempText, DEFAULT_TEMP));
            return grid;
        }

        // janela acompanha o tamanho da grade + o painel de controle (Vibecodado)
        private static void FitWindow(Grid<Material> grid)
        {
            int gridPx = (int)(grid.Width * (Globals.CELL_SIZE + Globals.GAP));
            int width = Math.Max(gridPx + PANEL_WIDTH, MIN_WINDOW_WIDTH);
            int height = Math.Max(gridPx, MIN_WINDOW_HEIGHT);

            if (Raylib.IsWindowReady())
            {
                Raylib.SetWindowSize(width, height);
            }
            else
            {
                Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
                Raylib.InitWindow(width, height, "Transferencia de Calor");
            }

            // impede o usuario de arrastar a janela pra um tamanho que corta a UI
            Raylib.SetWindowMinSize(MIN_WINDOW_WIDTH, MIN_WINDOW_HEIGHT);
        }

        private static void MovePanel(Grid<Material> grid, Textbox sizeBox, Textbox tempBox, Textbox timeBox, Button reloadButton)
        {
            float x = PanelX(grid);
            sizeBox.X = (int)x;
            tempBox.X = (int)x;
            timeBox.X = (int)x;
            reloadButton.X = (int)x;
        }

        // cores do seletor de material no popup
        private static readonly Color IdleMaterial = new(70, 70, 70, 255);
        private static readonly Color ChosenMaterial = new(200, 120, 40, 255);

        // centraliza o popup e posiciona seus componentes
        private static Rectangle LayoutPopup(Textbox tempInput, Button[] materials, Button ok)
        {
            int x = (Raylib.GetScreenWidth() - POPUP_WIDTH) / 2;
            int y = (Raylib.GetScreenHeight() - POPUP_HEIGHT) / 2;

            tempInput.X = x + 15;
            tempInput.Y = y + 58;
            for (int k = 0; k < materials.Length; k++)
            {
                materials[k].X = x + 15;
                materials[k].Y = y + 116 + k * 24;
            }
            ok.X = x + 15;
            ok.Y = y + 116 + materials.Length * 24 + 10;

            return new Rectangle(x, y, POPUP_WIDTH, POPUP_HEIGHT);
        }

        private static float PanelX(Grid<Material> grid)
        {
            return grid.Width * (Globals.CELL_SIZE + Globals.GAP) + 20f;
        }

        private static T Parse<T>(string text, T fallback)
        where T: IParsable<T>, INumber<T>
        {
            return T.TryParse(text, null, out T value) && value > T.Zero ? value : fallback;
        }
    }
}

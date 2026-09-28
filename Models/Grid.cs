using Raylib_cs;
using RayGUI_cs;

namespace Grid.Models;
public class Grid<T>
    where T: Material
{
    private Func<T> newMaterial;

    private float CellSize { get; set; } = Globals.CELL_SIZE;
    private float Gap { get; set; } = Globals.GAP;

    private int width;
    public int Width { get => width; set => width = value; }

    private int height;
    public int Height { get => height; set => height = value; }

    private T[,] plane;
    public T[,] Plane { get => plane; set => plane = value; }

    public Grid(Func<T> newMaterial)
    {
        width = 10;
        height = 10;
        Plane = new T[10, 10];
        this.newMaterial = newMaterial;
    }

    public Grid(int _width, int _height, Func<T> newMaterial)
    {
        Width = _width;
        Height = _height;
        Plane = new T[_width, _height];
        this.newMaterial = newMaterial;
    }

    // (re)cria as celulas com a temperatura inicial global
    public void Fill(float initialTemp)
    {
        float step = CellSize + Gap;

        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                Plane[i, j] = newMaterial();
                Plane[i, j].XY = [ j * step, i * step ];
                Plane[i, j].Temperature = initialTemp;
            }
        }

        // o canto {0, 0} sempre comeca com calor pra carai
        // Plane[0, 0].Temperature = Globals.HOTSPOT_TEMP;
    }

    // uma passada de troca de calor: pra vizinhos não trocarem calor entre si mais de uma vez
    public void Step()
    {
        for(int i = 0; i < Height; i++)
        {
            for(int j = 0; j < Width; j++)
            {
                if (j + 1 < Width)  Plane[i, j].ExchangeHeat(Plane[i, j + 1], Globals.TIME_STEP);
                if (i + 1 < Height) Plane[i, j].ExchangeHeat(Plane[i + 1, j], Globals.TIME_STEP);
            }
        }
    }

    public void Draw()
    {
        for(int i = 0; i < Height; i++)
        {
            for(int j = 0; j < Width; j++)
            {
                Plane[i, j].Draw();
            }
        }
    }
}

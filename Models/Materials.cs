using System.Numerics;
using Raylib_cs;
using RayGUI_cs;
namespace Grid.Models;

public abstract class Material
{
    // catalogo de materiais, na ordem do seletor do popup
    public static readonly int Count = 8;
    public static readonly string[] Labels = [.. Enumerable.Range(0, Count).Select(i => FromIndex(i).Label())];

    public static Material FromIndex(int index) => index switch
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

    public static int IndexOf(Material material)
    {
        for (int i = 0; i < Count; i++)
        {
            if (FromIndex(i).Label() == material.Label())
            {
                return i;
            }
        }
        return 0;
    }

    private Rectangle bounds;
    public Rectangle Bounds { get => bounds; set => bounds = value; }

    private string text = "";
    public string Text { get => text; set => text = value; }

    private float temperature;
    public float Temperature { get => temperature; set => temperature = value; }

    private float mass;
    public float Mass { get => mass; set => mass = value; }

    private float specificHeat;
    public float SpecificHeat { get => specificHeat; set => specificHeat = value; }

    private float conductivity;
    public float Conductivity { get => conductivity; set => conductivity = value; }

    private string name;

    // o que varia de material pra material entra pelo construtor
    protected Material(float conductivity, float specificHeat, float mass, string name)
    {
        Bounds = new Rectangle(0, 0, Globals.CELL_SIZE, Globals.CELL_SIZE);

        this.conductivity = conductivity;
        this.specificHeat = specificHeat;
        this.mass = mass;
        this.name = name;
    }

    // vira outro material copiando o que nele varia; o popup usa isso pra trocar
    public void Become(Material other)
    {
        conductivity = other.conductivity;
        specificHeat = other.specificHeat;
        mass = other.mass;
        name = other.name;
    }

    // nome do material pra exibicao
    public string Label()
    {
        return name;
    }

    // calor sensivel: Q = m . c . Delta T (de 0 K ate a temperatura atual)
    public float Heat()
    {
        return mass * specificHeat * temperature;
    }

    public void ExchangeHeat(Material other, float dt)
    {
        // o sentido do fluxo e sempre do mais quente pro mais frio
        Material hot = Temperature >= other.Temperature ? this : other;
        Material cold = hot == this ? other : this;

        // entre materiais diferentes vale o menor k
        float k = MathF.Min(hot.Conductivity, cold.Conductivity);
        float deltaT = hot.Temperature - cold.Temperature;

        // q = k . A . Delta T (Delta x desconsiderado, corpos se tocando perfeitamente)
        float energy = k * Globals.CONTACT_AREA * deltaT * dt;

        // nunca transfere mais que o necessario pra inverter as temperaturas
        float capacities = 1f / (hot.mass * hot.specificHeat) + 1f / (cold.mass * cold.specificHeat);
        energy = MathF.Min(energy, 0.5f * deltaT / capacities);

        // Q = m . c . Delta T rearranjado: Delta T = Q / (m . c)
        hot.temperature -= energy / (hot.mass * hot.specificHeat);
        cold.temperature += energy / (cold.mass * cold.specificHeat);
    }

    public void Draw()
    {
        Text = $"{Label()} {Temperature:F0}";
        Raylib.DrawRectangleRec(Bounds, TemperatureColor());
        Raylib.DrawText(Label(), (int)Bounds.X + 3, (int)Bounds.Y + 3, 10, Color.White);
        Raylib.DrawText($"{Temperature:F0}", (int)Bounds.X + 3, (int)Bounds.Y + 19, 10, Color.White);
    }

    private Color TemperatureColor()
    {
        // azul (frio) -> vermelho (quente) na faixa 0..100 graus
        float t = Math.Clamp(Temperature / 100f, 0f, 1f);
        byte r = (byte)(30 + (235 - 30) * t);
        byte g = (byte)(60 + (45 - 60) * t);
        byte b = (byte)(220 + (25 - 220) * t);
        return new Color(r, g, b, (byte)255);
    }

    public bool IsClicked()
    {
        return Raylib.CheckCollisionPointRec(
            Raylib.GetMousePosition(),
            Bounds
        ) && Raylib.IsMouseButtonPressed(MouseButton.Left);
    }


    public float[] XY
    {
        get => new float[] { Bounds.X, Bounds.Y };
        set
        {
            // Bounds da erro pois retorna uma propriedade temporária???
            bounds.X = value[0];
            bounds.Y = value[1];
        }
    }
}

// k adimensional, c em kJ/(kg.K) escalado, m = 1
// (cada subclasse so passa pro construtor o que varia dela pras outras)

public class Copper : Material
{
    public Copper() : base(0.90f, 0.385f, 1f, "cobre") { }
}

public class Iron : Material
{
    public Iron() : base(0.50f, 0.450f, 1f, "ferro") { }
}

public class Aluminum : Material
{
    public Aluminum() : base(0.70f, 0.900f, 1f, "alumínio") { }
}

public class Granite : Material
{
    public Granite() : base(0.20f, 0.790f, 1f, "granito") { }
}

public class Glass : Material
{
    public Glass() : base(0.15f, 0.840f, 1f, "vidro") { }
}

public class Brick : Material
{
    public Brick() : base(0.10f, 0.840f, 1f, "tijolo") { }
}

public class Wood : Material
{
    public Wood() : base(0.05f, 1.700f, 1f, "madeira") { }
}

public class Rubber : Material
{
    public Rubber() : base(0.01f, 2.000f, 1f, "borracha") { }
}

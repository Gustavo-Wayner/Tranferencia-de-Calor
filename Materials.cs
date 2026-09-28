using System.Numerics;
using Raylib_cs;
using RayGUI_cs;
namespace Grid;

public abstract class Material
{
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

    // o que varia de material pra material entra pelo construtor base
    protected Material(float conductivity, float specificHeat, float mass)
    {
        Bounds = new Rectangle(0, 0, Globals.CELL_SIZE, Globals.CELL_SIZE);

        this.conductivity = conductivity;
        this.specificHeat = specificHeat;
        this.mass = mass;
    }

    // nome do material pra exibicao, cada subclasse identifica a si mesma
    public abstract string Label();

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
        Raylib.DrawText($"{Temperature:F0} °K", (int)Bounds.X + 3, (int)Bounds.Y + 19, 10, Color.White);
    }

    private Color TemperatureColor()
    {
        // azul (frio) -> vermelho (quente) na faixa 0..100 graus (Vibecodado)
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
// (cada subclasse so passa pro base o que varia de material pra material)

public class Copper : Material
{
    public Copper() : base(0.90f, 0.385f, 1f) { }

    public override string Label()
    {
        return "cobre";
    }
}

public class Iron : Material
{
    public Iron() : base(0.50f, 0.450f, 1f) { }

    public override string Label()
    {
        return "ferro";
    }
}

public class Aluminum : Material
{
    public Aluminum() : base(0.70f, 0.900f, 1f) { }

    public override string Label()
    {
        return "alumínio";
    }
}

public class Granite : Material
{
    public Granite() : base(0.20f, 0.790f, 1f) { }

    public override string Label()
    {
        return "granito";
    }
}

public class Glass : Material
{
    public Glass() : base(0.15f, 0.840f, 1f) { }

    public override string Label()
    {
        return "vidro";
    }
}

public class Brick : Material
{
    public Brick() : base(0.10f, 0.840f, 1f) { }

    public override string Label()
    {
        return "tijolo";
    }
}

public class Wood : Material
{
    public Wood() : base(0.05f, 1.700f, 1f) { }

    public override string Label()
    {
        return "madeira";
    }
}

public class Rubber : Material
{
    public Rubber() : base(0.01f, 2.000f, 1f) { }

    public override string Label()
    {
        return "borracha";
    }
}

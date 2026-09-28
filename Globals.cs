namespace Grid;

public static class Globals
{
    public const int CELL_SIZE = 40;
    public const float GAP = 5f;

    // "A" e a area da secao transversal por onde o calor flui entre os corpos.
    public const float CONTACT_AREA = 1f;

    // segundos simulados a cada frame
    public static float TIME_STEP = 0.1f;

    // temperatura inicial do canto {0, 0}, fonte de calor da simulacao
    public const float HOTSPOT_TEMP = 1000f;
}

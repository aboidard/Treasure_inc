using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The skins of the equipment that are not in the sprite sheet of the items: they are drawn pixel by pixel, here,
/// and added after the sprites of the sheet (ItemManager.sprites), so the first of them has the id FirstId.
/// Each kind of equipment has a few shapes, and each shape is painted with each set of colors of its kind:
///   id = FirstId + (index of the kind * SkinsPerKind) + (index of the shape * number of sets) + index of the set
/// The engine gives these ids to the equipment it generates (Equipment.Kind, engine.items.graphics-count):
/// never reorder the kinds, the shapes or the sets of colors, an item in the database keeps its id. Add new
/// kinds at the end.
///
/// A shape is 16 rows of 16 characters, from the top:
///   '.' nothing             '#' black
///   '1' '2' '3' '4'         the main material, from its darkest color to its brightest
///   'a' 'b' 'c'             the second material, from its darkest color to its brightest
///   'o' 'O'                 the gem, dark and bright
/// The black outline is added around the shape: the first and last rows and columns must stay empty.
/// </summary>
public static class ItemArt
{
    /// <summary>The number of sprites of the sheet: the id of the first skin drawn here.</summary>
    public const int FirstId = 181;
    public const int SkinsPerKind = 16;

    private const int size = 16;

    // the materials, from dark to bright
    private static readonly Dictionary<string, string> ramps = new Dictionary<string, string>
    {
        { "iron", "4a4a4a 727272 9b9b9b c8c8c8" },
        { "steel", "4b5568 7c8aa3 b7c2d6 e6ecf5" },
        { "gold", "886c0f caa217 eac64a fff0a0" },
        { "bronze", "6b3f1d a5642c d08f4a f0c080" },
        { "obsidian", "1e1b2e 3a3558 5d568a 9088c8" },
        { "mithril", "3b6e8c 6fb3d0 b0e4f2 ffffff" },
        { "jade", "14532d 1f8a4c 4cc47a a8f0c0" },
        { "crimson", "5c1010 a11d1d d94a3a ff9a80" },
        { "leather", "4a2f18 7a5230 a87848 cfa26e" },
        { "hide", "241a12 45301f 6b4c33 8f6c4d" },
        { "bone", "8a7f66 c9bfa0 ece5cc ffffff" },
        { "wood", "5e4022 885c31 b07840 c28d58" },
        { "azure", "1f2f6b 3550b0 5f7fe0 a0b8ff" },
        { "violet", "3d1a5c 6b2fa0 9a5cd0 c8a0f0" },
        // the gems: dark, bright
        { "ruby", "a11d1d ff5a4a" },
        { "sapphire", "2040c0 60a0ff" },
        { "emerald", "1f8a4c 60e090" },
        { "amethyst", "7030b0 c080ff" },
        { "topaz", "c08010 ffe060" },
        { "diamond", "a0c8e0 ffffff" },
    };

    private class Kind
    {
        // "main second gem": three names of ramps
        public readonly string[] colors;
        public readonly string[][] shapes;

        public Kind(string[] colors, params string[][] shapes)
        {
            this.colors = colors;
            this.shapes = shapes;
        }
    }

    // in the order of Equipment.Kind in the engine
    private static readonly Kind[] kinds =
    {
        // ---- swords
        new Kind(new[] { "steel gold ruby", "obsidian bone amethyst", "mithril bronze sapphire", "crimson obsidian topaz" },
            new[] {
                "................",
                ".............43.",
                "...........342..",
                "..........342...",
                ".........342....",
                "........342.....",
                ".......342......",
                "...c..342.......",
                "...bc342........",
                "....bc2.........",
                "....abc.........",
                "...ab.bc........",
                "..ab...b........",
                ".oO.............",
                ".oo.............",
                "................" },
            new[] {
                "................",
                "...........33...",
                "..........3442..",
                ".........34442..",
                ".........3442...",
                "........3442....",
                "........342.....",
                ".......342......",
                "..c...342.......",
                "..bc.342........",
                "...bc32.........",
                "....bc..........",
                "...ab.c.........",
                "..ab............",
                ".oO.............",
                "................" },
            new[] {
                "................",
                "..............4.",
                ".............42.",
                "............42..",
                "...........42...",
                "..........42....",
                ".........42.....",
                "........42......",
                "...cc..42.......",
                "..c.bc42........",
                "..b..bc.........",
                "..bc.ab.........",
                "...bab..........",
                "..ab............",
                ".oO.............",
                "................" },
            new[] {
                "................",
                "............443.",
                "...........4432.",
                "..........4432..",
                ".........4432...",
                "........4432....",
                ".......4432.....",
                ".c....4432......",
                ".bc..4432.......",
                "..bc.432........",
                "...bOc2.........",
                "....cbc.........",
                "...ab.bc........",
                "..ab...bc.......",
                ".ab.............",
                "................" }),

        // ---- shields
        new Kind(new[] { "iron wood ruby", "gold crimson topaz", "steel azure diamond", "obsidian jade emerald" },
            new[] {
                "................",
                ".....222222.....",
                "...2233333322...",
                "..23bbbbbbbb32..",
                "..3bbccccccbb2..",
                ".23bcccccccbb21.",
                ".23bccc34ccab21.",
                ".23bccc42ccab21.",
                ".23bccc22ccab21.",
                ".23bbcccccaab21.",
                "..2bbbccaaab21..",
                "..12bbaaaaab1...",
                "...1122222211...",
                ".....111111.....",
                "................",
                "................" },
            new[] {
                "................",
                "..222222222222..",
                "..2ccccOOcccc1..",
                "..2ccccOOcccb1..",
                "..2cOOOOOOOOb1..",
                "..2cOOOOOOOOb1..",
                "..2ccccOObbbb1..",
                "..2bcccOObbbb1..",
                "...2bccOObbb1...",
                "...2bbcOobba1...",
                "....2bbOoba1....",
                "....12bboa11....",
                ".....12ba1......",
                "......1211......",
                ".......11.......",
                "................" },
            new[] {
                "................",
                "...2222222222...",
                "..233333333332..",
                "..23bbbbbbbb21..",
                "..23b4cccc4b21..",
                "..23bccccccb21..",
                "..23bcc33ccb21..",
                "..23bc3443cb21..",
                "..23bc3443cb21..",
                "..23bcc33ccb21..",
                "..23bccccccb21..",
                "..23b4cccc4b21..",
                "..22bbbbbbbb21..",
                "..122222222211..",
                "...1111111111...",
                "................" },
            new[] {
                "................",
                ".......22.......",
                "..2...2332...2..",
                "..22.233332.22..",
                "..2322bccb2232..",
                "..23bbccccbb32..",
                "..23bccOOccb21..",
                "..23bcOOOocb21..",
                "..23bcOOoocb21..",
                "..23bccooccb21..",
                "...2bbccccbb1...",
                "...22bbccbb11...",
                "....12bbbb11....",
                ".....122211.....",
                ".......11.......",
                "................" }),

        // ---- armors
        new Kind(new[] { "steel gold ruby", "bronze leather emerald", "obsidian crimson amethyst", "mithril azure sapphire" },
            new[] {
                "................",
                "..3333....3333..",
                ".34433222233221.",
                ".34333333333321.",
                ".33323444332321.",
                "..222344332221..",
                "...2334333221...",
                "...2334332221...",
                "...2333332211...",
                "...2233322211...",
                "...aabbccbbaa...",
                "...2233332211...",
                "..223333322211..",
                "..22332..22211..",
                "..1221....1111..",
                "................" },
            new[] {
                "................",
                "....332aa233....",
                ".33232323232321.",
                ".32323232323211.",
                ".aa.23232323.aa.",
                "....32323231....",
                "....23232321....",
                "....32323231....",
                "....23232321....",
                "....abbccbba....",
                "....32323231....",
                "....23232321....",
                "...3232323231...",
                "...2121212121...",
                "................",
                "................" },
            new[] {
                "................",
                ".....33..33.....",
                "....3c3223c3....",
                "...33c3333c33...",
                "..3323c33c3233..",
                ".33323ccOc32321.",
                ".3322.3cc3.2221.",
                ".222.33cc32.211.",
                ".....33cc32.....",
                "....aabccbaa....",
                "....33cc3322....",
                "...333cc33221...",
                "...333cc33221...",
                "..3333cc332221..",
                "..ccccccccccc1..",
                "................" },
            new[] {
                "................",
                "..333......333..",
                ".3443a....a3432.",
                ".3333ba..ab3321.",
                ".2332bbaabb2321.",
                "..22cbbbbbbc21..",
                "...3cbb3bbbc2...",
                "...3bb343bbb2...",
                "...3cbb3bbbc2...",
                "...3bbbbbbbb2...",
                "...3cb3bb3bc2...",
                "...2bbbbbbbb1...",
                "...oOooooooo1...",
                "...2bbb..bbb1...",
                "...111....111...",
                "................" }),

        // ---- helmets
        new Kind(new[] { "iron leather ruby", "gold crimson sapphire", "obsidian bone amethyst", "mithril gold emerald" },
            new[] {
                "................",
                ".....333333.....",
                "...3344433322...",
                "..334433333221..",
                "..343333333221..",
                ".33433333333221.",
                ".3aaaaaaaaaaaa1.",
                ".33####3####221.",
                ".32####3####211.",
                ".32####2####211.",
                "..22##.2.##11...",
                "..21.......11...",
                "................",
                "................",
                "................",
                "................" },
            new[] {
                "................",
                ".c............c.",
                ".cc..........cb.",
                ".bcc.333333.cba.",
                "..bc33444332ba..",
                "..ab344333322a..",
                "...3443333322...",
                "..334333333221..",
                "..3aaaaaaaaaa1..",
                "..33233##33221..",
                "..3323####3221..",
                "..322######221..",
                "..22.######.11..",
                "................",
                "................",
                "................" },
            new[] {
                "................",
                ".......oO.......",
                "......oOOo......",
                "....3333oo33....",
                "...3344333322...",
                "...3443333322...",
                "..334333333221..",
                "..3####33####1..",
                "..334333333221..",
                "..33a3a33a3a21..",
                "..333333333221..",
                "..33a3a33a3a21..",
                "..233333332221..",
                "...2222222211...",
                "................",
                "................" },
            new[] {
                "................",
                ".c....3333....c.",
                ".cc..344332..cc.",
                ".ccc34433322ccb.",
                ".bcc34333322cbb.",
                "..bb3333O3322b..",
                "..333333o33221..",
                "..3aaaaaaaaaa1..",
                "..332##33##221..",
                "..332##33##221..",
                "..3322#33#2211..",
                "...322.22.211...",
                "...22......11...",
                "................",
                "................",
                "................" }),

        // ---- boots
        new Kind(new[] { "leather hide topaz", "hide iron ruby", "steel gold sapphire", "crimson gold diamond" },
            new[] {
                "................",
                "................",
                "..cccc...cccc...",
                "..bbbb...bbbb...",
                "..3432...3432...",
                "..3432...3432...",
                "..3332...3332...",
                "..3332...3332...",
                "..33322..33322..",
                "..344332.344332.",
                "..333322.333322.",
                "..aaaaaa.aaaaaa.",
                "................",
                "................",
                "................",
                "................" },
            new[] {
                "................",
                "..333....333....",
                ".34432..34432...",
                ".34332..34332...",
                "..332....332....",
                "..342....342....",
                "..342....342....",
                "..332....332....",
                "..aaa....aaa....",
                "..3322...3322...",
                "..34432..34432..",
                "..33322..33322..",
                "..11111..11111..",
                "................",
                "................",
                "................" },
            new[] {
                "................",
                "................",
                ".cccccc.cccccc..",
                ".cbcbcb.cbcbcb..",
                "..3432...3432...",
                "..3432...3432...",
                "..oOo2...oOo2...",
                "..3332...3332...",
                "..33322..33322..",
                "..344332.344332.",
                "..333322.333322.",
                "..aaaaaa.aaaaaa.",
                "................",
                "................",
                "................",
                "................" },
            new[] {
                "................",
                "................",
                "..3333...3333...",
                "..oOoo...oOoo...",
                "..3432...3432...",
                "..3432...3432...",
                "..oOoo...oOoo...",
                "..3332...3332...",
                "..333cc..333cc..",
                "..3443cc.3443cc.",
                "..3333bb.3333bb.",
                "..aaaaaa.aaaaaa.",
                "................",
                "................",
                "................",
                "................" }),

        // ---- gloves
        new Kind(new[] { "leather hide topaz", "steel leather ruby", "obsidian crimson amethyst", "gold azure sapphire" },
            new[] {
                "................",
                "...33.33.33.....",
                "...34.34.32.....",
                "...34.34.32.....",
                "...34.34.32.33..",
                "...34334332342..",
                "...34333333332..",
                "...3333333332...",
                "...333333332....",
                "....3333332.....",
                "....aaaaaaa.....",
                "...cccccccc1....",
                "...cbbbbbbb1....",
                "...cbbbbbbb1....",
                "....1111111.....",
                "................" },
            new[] {
                "................",
                "................",
                "...oo.oo.oo.....",
                "...33.33.33.....",
                "...34.34.32.oo..",
                "...34334332332..",
                "...3bbbbbbbb32..",
                "...3bcOcbcOb2...",
                "...3bbbbbbb2....",
                "....3333332.....",
                "....aaaaaaa.....",
                "...cccccccc1....",
                "...cbObObOb1....",
                "...cbbbbbbb1....",
                "....1111111.....",
                "................" },
            new[] {
                "................",
                "...4..4..4......",
                "...34.34.34.....",
                "...33.33.33.....",
                "...32.32.32..4..",
                "...3233233234...",
                "...34333333332..",
                "...3443333332...",
                "...333333332....",
                "....3333332.....",
                "....aaaaaaa.....",
                "..cccccccccc1...",
                "...cbbbbbbb1....",
                "..ccbbbbbbb11...",
                "....1111111.....",
                "................" },
            new[] {
                "................",
                "...33.33.33.....",
                "...34.34.32.....",
                "...34.34.32.....",
                "...34.34.32.33..",
                "...34334332342..",
                "...343oOo33332..",
                "...333OOo3332...",
                "...333ooo332....",
                "....3333332.....",
                "....ccccccc.....",
                "...cbbbbbbb1....",
                "..cbbcbcbcbb1...",
                ".ccbbbbbbbbb11..",
                "..11111111111...",
                "................" }),

        // ---- rings
        new Kind(new[] { "gold gold ruby", "steel steel sapphire", "bronze bronze emerald", "obsidian obsidian amethyst" },
            new[] {
                "................",
                "................",
                "......OOo.......",
                ".....OOOoo......",
                ".....3ooo2......",
                "....343.332.....",
                "...343...332....",
                "...33.....32....",
                "...33.....32....",
                "...33.....32....",
                "...332...322....",
                "....3322221.....",
                ".....22211......",
                "................",
                "................",
                "................" },
            new[] {
                "................",
                "................",
                ".....33333......",
                "....34OoO32.....",
                "....34oOo32.....",
                "....3333322.....",
                "...342...332....",
                "...33.....32....",
                "...33.....32....",
                "...33.....32....",
                "...332...322....",
                "....3322221.....",
                ".....22211......",
                "................",
                "................",
                "................" },
            new[] {
                "................",
                ".......O........",
                "......OOo.......",
                ".....OOooo......",
                "......ooo.......",
                "....3a3o3a3.....",
                "...3a3...3a2....",
                "...a3.....a2....",
                "...3a.....32....",
                "...a3.....a2....",
                "...3a3...3a2....",
                "....3a3a3a2.....",
                ".....22211......",
                "................",
                "................",
                "................" },
            new[] {
                "................",
                "................",
                "....O..O..O.....",
                "....3.343.3.....",
                "....3334333.....",
                "....3443332.....",
                "...343...332....",
                "...33.....32....",
                "...33.....32....",
                "...33.....32....",
                "...332...322....",
                "....3322221.....",
                ".....22211......",
                "................",
                "................",
                "................" }),

        // ---- amulets
        new Kind(new[] { "gold gold ruby", "steel steel sapphire", "bone leather emerald", "obsidian iron amethyst" },
            new[] {
                "................",
                ".....bcccb......",
                "....b.....b.....",
                "...b.......b....",
                "...b.......b....",
                "....b.....b.....",
                ".....b...b......",
                "......b.b.......",
                "......232.......",
                ".....23O32......",
                "....23OOo21.....",
                "....23Ooo21.....",
                ".....23o21......",
                "......211.......",
                "................",
                "................" },
            new[] {
                "................",
                ".....bcccb......",
                "....b.....b.....",
                "...b.......b....",
                "...b.......b....",
                "....b.....b.....",
                ".....b...b......",
                "......b.b.......",
                ".......3........",
                "......oOo.......",
                ".....oOOoo......",
                ".....oOooo......",
                ".....ooooo......",
                "......ooo.......",
                "................",
                "................" },
            new[] {
                "................",
                ".....bcccb......",
                "....b.....b.....",
                "...b.......b....",
                "...b.......b....",
                "....b.....b.....",
                ".....b...b......",
                "......b.b.......",
                ".......3........",
                ".......3........",
                ".....33O33......",
                ".......3........",
                ".......2........",
                ".......2........",
                "................",
                "................" },
            new[] {
                "................",
                ".....bcccb......",
                "....b.....b.....",
                "...b.......b....",
                "...b.......b....",
                "....b.....b.....",
                ".....b...b......",
                "......b.b.......",
                ".....aaaaa......",
                ".....34332......",
                ".....34332......",
                "......342.......",
                "......332.......",
                ".......2........",
                "................",
                "................" }),

        // ---- talismans
        new Kind(new[] { "iron leather sapphire", "jade gold topaz", "bone leather ruby", "gold crimson amethyst" },
            new[] {
                "................",
                "................",
                ".....33333......",
                "....3443332.....",
                "...344333322....",
                "...343O33O22....",
                "...333OO3O221...",
                "...333O3OO221...",
                "...333O33O221...",
                "...3333333221...",
                "...2333332221...",
                "....22222221....",
                ".....111111.....",
                "................",
                "................",
                "................" },
            new[] {
                "................",
                ".......b........",
                "......b.b.......",
                ".......b........",
                ".....33333......",
                "...3344333322...",
                "..334ccccc3221..",
                "..34ccoOocc321..",
                "..33ccoOocc221..",
                "..333ccccc2221..",
                "...3333322221...",
                ".....22211......",
                "................",
                "................",
                "................",
                "................" },
            new[] {
                "................",
                "................",
                ".....333333.....",
                "....34433332....",
                "...3443..3322...",
                "...343....322...",
                "...343....322...",
                "...333....322...",
                "...3o3....2o2...",
                "...333....222...",
                "...333....222...",
                "...aaa....aaa...",
                "................",
                "................",
                "................",
                "................" },
            new[] {
                "................",
                ".......b........",
                "......b.b.......",
                ".......b........",
                ".......3........",
                "......343.......",
                "......343.......",
                "..33334443333...",
                "...334OOO332....",
                "....3443332.....",
                "....3433332.....",
                "...3432.2332....",
                "...332...222....",
                "..32.......21...",
                "................",
                "................" }),
    };

    private static List<Sprite> sprites;

    /// <summary>The skins, in the order of their ids: the first one is the sprite FirstId.</summary>
    public static List<Sprite> Sprites
    {
        get
        {
            if (sprites == null || sprites.Count == 0 || sprites[0] == null) sprites = Build();
            return sprites;
        }
    }

    // every skin is drawn in one texture: one row per kind
    private static List<Sprite> Build()
    {
        Texture2D texture = new Texture2D(SkinsPerKind * size, kinds.Length * size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        Color32[] pixels = new Color32[texture.width * texture.height];

        List<Sprite> built = new List<Sprite>();
        List<Rect> areas = new List<Rect>();
        for (int k = 0; k < kinds.Length; k++)
        {
            Kind kind = kinds[k];
            int index = 0;
            foreach (string[] shape in kind.shapes)
            {
                foreach (string colors in kind.colors)
                {
                    int left = index * size;
                    int bottom = k * size;
                    Draw(shape, colors.Split(' '), pixels, texture.width, left, bottom);
                    areas.Add(new Rect(left, bottom, size, size));
                    index++;
                }
            }
            if (index != SkinsPerKind) Debug.LogError("ItemArt: a kind has " + index + " skins instead of " + SkinsPerKind);
        }
        texture.SetPixels32(pixels);
        texture.Apply();
        foreach (Rect area in areas)
        {
            built.Add(Sprite.Create(texture, area, new Vector2(0.5f, 0.5f), 100f));
        }
        return built;
    }

    private static void Draw(string[] shape, string[] colors, Color32[] pixels, int width, int left, int bottom)
    {
        Color32[] main = Ramp(colors[0]);
        Color32[] second = Ramp(colors[1]);
        Color32[] gem = Ramp(colors[2]);
        Color32 black = new Color32(0, 0, 0, 255);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                char c = shape[y][x];
                Color32 color;
                if (c == '.')
                {
                    // the outline: an empty pixel touching the shape
                    bool edge = Filled(shape, x - 1, y) || Filled(shape, x + 1, y) || Filled(shape, x, y - 1) || Filled(shape, x, y + 1);
                    color = edge ? black : new Color32(0, 0, 0, 0);
                }
                else if (c >= '1' && c <= '4') color = main[c - '1'];
                else if (c >= 'a' && c <= 'c') color = second[c - 'a'];
                else if (c == 'o') color = gem[0];
                else if (c == 'O') color = gem[1];
                else color = black;
                // the first row of a texture is its bottom
                pixels[(bottom + size - 1 - y) * width + left + x] = color;
            }
        }
    }

    private static bool Filled(string[] shape, int x, int y)
    {
        return x >= 0 && x < size && y >= 0 && y < size && shape[y][x] != '.';
    }

    private static Color32[] Ramp(string name)
    {
        string[] hex = ramps[name].Split(' ');
        Color32[] colors = new Color32[hex.Length];
        for (int i = 0; i < hex.Length; i++)
        {
            ColorUtility.TryParseHtmlString("#" + hex[i], out Color color);
            colors[i] = color;
        }
        return colors;
    }
}

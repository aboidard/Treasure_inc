using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The look of what stands in the way of an expedition: a small white shape drawn pixel by pixel for each kind
/// of monster of the server (see Monster.KINDS in the engine) and for the barricade, and the color it is tinted
/// with. Giving a monster its real art: return its sprite from Sprite instead, and white from Color.
/// </summary>
public static class MonsterArt
{
    public const string BARRICADE = "barricade";

    private static readonly Dictionary<string, string[]> shapes = new Dictionary<string, string[]>
    {
        { "rat", new[] {
            "............",
            "............",
            ".##......##.",
            ".##......##.",
            "..########..",
            ".##.####.##.",
            ".##########.",
            "..########..",
            "...##..##...",
            "....####....",
            ".....##.....",
            "............" } },
        { "bat", new[] {
            "............",
            "#..........#",
            "##...##...##",
            "###.####.###",
            "############",
            "####.##.####",
            ".##########.",
            "..##.##.##..",
            "...#.##.#...",
            ".....##.....",
            "............",
            "............" } },
        { "goblin", new[] {
            "............",
            "#...####...#",
            "##.######.##",
            ".##########.",
            "..#.####.#..",
            "..########..",
            "...#....#...",
            "...######...",
            "..########..",
            ".##.####.##.",
            "....#..#....",
            "...##..##..." } },
        { "spider", new[] {
            "............",
            "#..#....#..#",
            ".#..#..#..#.",
            "..#.####.#..",
            "...######...",
            "####.##.####",
            "...######...",
            "..#.####.#..",
            ".#..#..#..#.",
            "#..#....#..#",
            "............",
            "............" } },
        { "slime", new[] {
            "............",
            "............",
            "............",
            "....####....",
            "..########..",
            ".##########.",
            ".##.####.##.",
            "############",
            "############",
            "############",
            ".##########.",
            "............" } },
        { "skeleton", new[] {
            "...######...",
            "..########..",
            "..#..##..#..",
            "..#..##..#..",
            "..########..",
            "...#.##.#...",
            "...######...",
            ".....##.....",
            "..########..",
            ".#...##...#.",
            ".....##.....",
            "....#..#...." } },
        { "mummy", new[] {
            "...######...",
            "..########..",
            "..##....##..",
            "..########..",
            "..#.####.#..",
            "...######...",
            ".##########.",
            "#..######..#",
            "...######...",
            "...##..##...",
            "...##..##...",
            "..###..###.." } },
        { "ghost", new[] {
            "............",
            "...######...",
            "..########..",
            ".##########.",
            ".##..##..##.",
            ".##..##..##.",
            ".##########.",
            ".####..####.",
            ".##########.",
            ".##########.",
            ".#.##..##.#.",
            "............" } },
        { "troll", new[] {
            "..########..",
            ".##########.",
            ".#..####..#.",
            ".##########.",
            ".###....###.",
            "..########..",
            "############",
            "############",
            "##.######.##",
            "##.######.##",
            "...##..##...",
            "..###..###.." } },
        { "golem", new[] {
            "...######...",
            "...#.##.#...",
            "...######...",
            ".##########.",
            "############",
            "###.####.###",
            "###.####.###",
            "###.####.###",
            "....####....",
            "...##..##...",
            "..###..###..",
            "..###..###.." } },
        { BARRICADE, new[] {
            "............",
            "#..#....#..#",
            "############",
            "############",
            "#.##....##.#",
            "#..##..##..#",
            "#...####...#",
            "#..##..##..#",
            "#.##....##.#",
            "############",
            "############",
            "#..#....#..#" } },
    };

    private static readonly Dictionary<string, Color> colors = new Dictionary<string, Color>
    {
        { "rat", new Color(0.62f, 0.55f, 0.5f) },
        { "bat", new Color(0.55f, 0.4f, 0.75f) },
        { "goblin", new Color(0.45f, 0.8f, 0.35f) },
        { "spider", new Color(0.75f, 0.25f, 0.3f) },
        { "slime", new Color(0.35f, 0.85f, 0.75f) },
        { "skeleton", new Color(0.92f, 0.9f, 0.82f) },
        { "mummy", new Color(0.85f, 0.75f, 0.5f) },
        { "ghost", new Color(0.75f, 0.85f, 1f) },
        { "troll", new Color(0.4f, 0.6f, 0.45f) },
        { "golem", new Color(0.6f, 0.62f, 0.68f) },
        { BARRICADE, new Color(0.65f, 0.45f, 0.25f) },
    };

    private const string unknown = "slime";
    private static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

    /// <summary>The white shape of a kind of monster, or of the barricade; a monster the client does not know is a blob.</summary>
    public static Sprite Sprite(string kind)
    {
        if (kind == null || !shapes.ContainsKey(kind)) kind = unknown;
        if (!sprites.TryGetValue(kind, out Sprite sprite) || sprite == null)
        {
            sprite = UIBuilder.PixelSprite(shapes[kind]);
            sprites[kind] = sprite;
        }
        return sprite;
    }

    public static Color Color(string kind)
    {
        return kind != null && colors.TryGetValue(kind, out Color color) ? color : new Color(0.7f, 0.7f, 0.7f);
    }
}

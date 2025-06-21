using UnityEngine;

public static class StringExtensions
{
    public static string Bold(this string str) => $"<b>{str}</b>";
    public static string Italic(this string str) => $"<i>{str}</i>";
    public static string Underline(this string str) => $"<u>{str}</u>";
    public static string Strikethrough(this string str) => $"<s>{str}</s>";
    public static string Size(this string str, float size) => $"<size={size}>{str}</size>";

    public static string Color(this string str, Color color)
    => $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{str}</color>";
}
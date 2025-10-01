using PdfSharp.Fonts;
using System.Reflection;

public class MauiFontResolver : IFontResolver
{
    public byte[] GetFont(string faceName)
    {
        if (faceName == "OpenSans")
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("PharmacistRecommendation.Resources.Fonts.OpenSans-Regular2.ttf");
            byte[] fontData = new byte[stream!.Length];
            stream.Read(fontData, 0, fontData.Length);
            return fontData;
        }
        throw new InvalidOperationException("Font not found: " + faceName);
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        if (familyName == "OpenSans")
            return new FontResolverInfo("OpenSans");
        return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
    }
}

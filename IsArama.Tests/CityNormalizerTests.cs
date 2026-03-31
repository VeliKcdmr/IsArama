using IsArama.Scraper.Helpers;
using Xunit;

namespace IsArama.Tests;

public class CityNormalizerTests
{
    [Theory]
    [InlineData("Satış Danışmanı - İstanbul", "Satış Danışmanı")]
    [InlineData("Yazılım Geliştirici - Ankara", "Yazılım Geliştirici")]
    [InlineData("Muhasebe Uzmanı - İzmir Merkez", "Muhasebe Uzmanı")]
    public void Strip_DashCity_Removed(string input, string expected)
    {
        Assert.Equal(expected, CityNormalizer.StripLocationSuffix(input));
    }

    [Theory]
    [InlineData("Satış Danışmanı (Acıbadem)", "Satış Danışmanı")]
    [InlineData("Yazılım Uzmanı (Remote)", "Yazılım Uzmanı")]
    public void Strip_Parenthesis_Removed(string input, string expected)
    {
        Assert.Equal(expected, CityNormalizer.StripLocationSuffix(input));
    }

    [Theory]
    [InlineData("Satış Danışmanı / Cevahir Mağazası", "Satış Danışmanı")]
    public void Strip_Slash_Removed(string input, string expected)
    {
        Assert.Equal(expected, CityNormalizer.StripLocationSuffix(input));
    }

    [Theory]
    [InlineData("Satış Danışmanı İstanbul", "Satış Danışmanı")]
    [InlineData("Yazılım Geliştirici Ankara", "Yazılım Geliştirici")]
    [InlineData("Muhasebeci Bursa", "Muhasebeci")]
    public void Strip_InlineCity_Removed(string input, string expected)
    {
        Assert.Equal(expected, CityNormalizer.StripLocationSuffix(input));
    }

    [Theory]
    [InlineData("Satış Temsilcisi-İstanbul", "Satış Temsilcisi")]
    public void Strip_DashNoSpace_Removed(string input, string expected)
    {
        Assert.Equal(expected, CityNormalizer.StripLocationSuffix(input));
    }

    [Theory]
    [InlineData("Yazılım Geliştirici", "Yazılım Geliştirici")]
    [InlineData("İnsan Kaynakları Uzmanı", "İnsan Kaynakları Uzmanı")]
    [InlineData("Proje Yöneticisi", "Proje Yöneticisi")]
    public void Strip_NoLocation_Unchanged(string input, string expected)
    {
        Assert.Equal(expected, CityNormalizer.StripLocationSuffix(input));
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    public void Strip_NullOrEmpty_ReturnsEmpty(string? input, string expected)
    {
        Assert.Equal(expected, CityNormalizer.StripLocationSuffix(input));
    }

    [Theory]
    [InlineData("Mağaza Müdür Yardımcısı-Avrupa Yakası", "Mağaza Müdür Yardımcısı")]
    public void Strip_DashLocationKeyword_Removed(string input, string expected)
    {
        Assert.Equal(expected, CityNormalizer.StripLocationSuffix(input));
    }

    [Theory]
    [InlineData("Satış Danışmanı Akasya AVM", "Satış Danışmanı")]
    public void Strip_NameBeforeKeyword_Removed(string input, string expected)
    {
        Assert.Equal(expected, CityNormalizer.StripLocationSuffix(input));
    }

    [Theory]
    [InlineData("Mağaza Müdür Yardımcısı-", "Mağaza Müdür Yardımcısı")]
    public void Strip_TrailingDash_Removed(string input, string expected)
    {
        Assert.Equal(expected, CityNormalizer.StripLocationSuffix(input));
    }

    [Theory]
    [InlineData("Mağaza Müdür Yardımcısı- Avrupa", "Mağaza Müdür Yardımcısı")]
    public void Strip_DashSpaceKeyword_Removed(string input, string expected)
    {
        Assert.Equal(expected, CityNormalizer.StripLocationSuffix(input));
    }

    [Theory]
    [InlineData("Mağaza Satış Danışmanı(İzmit Outlet)", "Mağaza Satış Danışmanı")]
    public void Strip_ParenNoSpace_Removed(string input, string expected)
    {
        Assert.Equal(expected, CityNormalizer.StripLocationSuffix(input));
    }

    [Theory]
    [InlineData("mağaza elemanı", "Mağaza Elemanı")]
    [InlineData("müşteri temsilcisi", "Müşteri Temsilcisi")]
    [InlineData("ÇAPAK ALMA", "Çapak Alma")]
    [InlineData("Satış Danışmanı", "Satış Danışmanı")]
    public void Strip_CaseNormalize_TitleCase(string input, string expected)
    {
        Assert.Equal(expected, CityNormalizer.StripLocationSuffix(input));
    }

    [Theory]
    [InlineData("Mağaza Müdürü-Fethiye İpekyol", "Mağaza Müdürü")]
    [InlineData("Mağaza Müdürü-İyaş", "Mağaza Müdürü")]
    [InlineData("Mağaza Müdür Yardımcısı- Edremit", "Mağaza Müdür Yardımcısı")]
    public void Strip_DashAnything_Removed(string input, string expected)
    {
        Assert.Equal(expected, CityNormalizer.StripLocationSuffix(input));
    }

    [Theory]
    [InlineData("E-Ticaret Uzmanı-İstanbul", "E-Ticaret Uzmanı")]
    [InlineData("E-Ticaret Müdürü", "E-Ticaret Müdürü")]
    [InlineData("E-Market Personeli", "E-Market Personeli")]
    [InlineData("E -Market Personeli", "E -Market Personeli")]
    [InlineData("E- Market Personeli", "E- Market Personeli")]
    [InlineData("E - Market Personeli", "E - Market Personeli")]
    [InlineData("( Bursa Bölgesi) Yapı Sektöründe İş Geliştirme Sorumlusu", "Yapı Sektöründe İş Geliştirme Sorumlusu")]
    public void Strip_EPrefix_Preserved(string input, string expected)
    {
        Assert.Equal(expected, CityNormalizer.StripLocationSuffix(input));
    }




}

namespace IsArama.Scraper.Helpers;

public static class CityNormalizer
{
    // Türkiye'nin 81 ili
    private static readonly HashSet<string> TurkishCities = new(StringComparer.OrdinalIgnoreCase)
    {
        "Adana", "Adıyaman", "Afyonkarahisar", "Ağrı", "Amasya", "Ankara", "Antalya",
        "Artvin", "Aydın", "Balıkesir", "Bilecik", "Bingöl", "Bitlis", "Bolu",
        "Burdur", "Bursa", "Çanakkale", "Çankırı", "Çorum", "Denizli", "Diyarbakır",
        "Edirne", "Elazığ", "Erzincan", "Erzurum", "Eskişehir", "Gaziantep", "Giresun",
        "Gümüşhane", "Hakkari", "Hatay", "Isparta", "Mersin", "İstanbul", "İzmir",
        "Kars", "Kastamonu", "Kayseri", "Kırklareli", "Kırşehir", "Kocaeli", "Konya",
        "Kütahya", "Malatya", "Manisa", "Kahramanmaraş", "Mardin", "Muğla", "Muş",
        "Nevşehir", "Niğde", "Ordu", "Rize", "Sakarya", "Samsun", "Siirt", "Sinop",
        "Sivas", "Tekirdağ", "Tokat", "Trabzon", "Tunceli", "Şanlıurfa", "Uşak",
        "Van", "Yozgat", "Zonguldak", "Aksaray", "Bayburt", "Karaman", "Kırıkkale",
        "Batman", "Şırnak", "Bartın", "Ardahan", "Iğdır", "Yalova", "Karabük",
        "Kilis", "Osmaniye", "Düzce"
    };

    public static string Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "Belirtilmemiş";

        // 1) Parantez içini at: "İstanbul (Avr.)" → "İstanbul"
        var paren = raw.IndexOf('(');
        if (paren > 0) raw = raw[..paren];

        // 2) "+" sonrasını at: "İstanbul +2 il daha" → "İstanbul"
        var plus = raw.IndexOf('+');
        if (plus > 0) raw = raw[..plus];

        // 3) Virgülle ayrılmışsa ilk parçayı al
        var comma = raw.IndexOf(',');
        if (comma > 0) raw = raw[..comma];

        // 4) " - " ile ayrılmışsa ilk parçayı al: "İstanbul Avrupa - Arnavutköy"
        var dash = raw.IndexOf(" - ");
        if (dash > 0) raw = raw[..dash];

        // 5) "Greater " prefix'ini temizle
        raw = raw.Trim();
        if (raw.StartsWith("Greater ", StringComparison.OrdinalIgnoreCase))
            raw = raw["Greater ".Length..].Trim();

        // 6) 81 ilde direkt eşleşme var mı?
        if (TurkishCities.Contains(raw))
            return CanonicalName(raw);

        // 7) İçinde geçen ili bul: "İstanbul Anadolu Yakası" → "İstanbul"
        foreach (var city in TurkishCities)
        {
            if (raw.StartsWith(city, StringComparison.OrdinalIgnoreCase))
                return CanonicalName(city);
        }

        // 8) Hiçbir ile uymuyorsa → Belirtilmemiş
        return "Belirtilmemiş";
    }

    // Canonical büyük harf düzeltmesi (set'teki yazımı döndür)
    private static string CanonicalName(string city)
    {
        foreach (var c in TurkishCities)
            if (string.Equals(c, city, StringComparison.OrdinalIgnoreCase))
                return c;
        return city;
    }
}

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

    // AVM, cadde, vs. gibi lokasyon ipucu olan kelimeler
    private static readonly HashSet<string> LocationKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "AVM", "Mağazası", "Caddesi", "Sokak", "Sokağı", "Bulvarı",
        "Sitesi", "Plaza", "Mahallesi", "Çarşısı", "Yakası", "Tersane"
    };

    /// <summary>
    /// "Satış Danışmanı - Erzurum"        → "Satış Danışmanı"
    /// "Satış Danışmanı (Acıbadem)"        → "Satış Danışmanı"
    /// "Satış Danışmanı / Cevahir Mağazası"→ "Satış Danışmanı"
    /// "Satış Danışmanı Akasya AVM"        → "Satış Danışmanı"
    /// "Satış Danışmanı İstanbul 5. Levent"→ "Satış Danışmanı"
    /// </summary>
    public static string StripLocationSuffix(string? title)
    {
        if (string.IsNullOrWhiteSpace(title)) return "";
        var t = title.Trim();

        // 1) " - ..." → at
        var idx = t.IndexOf(" - ", StringComparison.Ordinal);
        if (idx > 0) return t[..idx].Trim();

        // 2) " (" → at (parantez içi lokasyon)
        idx = t.IndexOf(" (", StringComparison.Ordinal);
        if (idx > 0) return t[..idx].Trim();

        // 3) " / " → at (slash ile lokasyon)
        idx = t.IndexOf(" / ", StringComparison.Ordinal);
        if (idx > 0) return t[..idx].Trim();

        // 4) "-Şehir" boşuksuz kalıp
        idx = t.IndexOf('-');
        if (idx > 2)
        {
            var afterDash = t[(idx + 1)..].TrimStart();
            foreach (var city in TurkishCities)
                if (afterDash.StartsWith(city, StringComparison.OrdinalIgnoreCase))
                    return t[..idx].Trim();
        }

        // 5) Kelime kelime tara: bilinen şehir veya lokasyon anahtar kelimesi bulununca at
        var words = t.Split(' ');
        for (int i = 1; i < words.Length; i++)
        {
            var w = words[i];
            if (LocationKeywords.Contains(w))
                return string.Join(" ", words[..i]).Trim();

            foreach (var city in TurkishCities)
                if (string.Equals(w, city, StringComparison.OrdinalIgnoreCase))
                    return string.Join(" ", words[..i]).Trim();
        }

        return t;
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

namespace IsArama.Scraper.Helpers;

public static class CategoryClassifier
{
    private static readonly (string Category, string[] Keywords)[] Rules =
    [
        ("Yazılım Geliştirme",
            ["yazılım", "software", "developer", "geliştirici", "programcı", "programmer",
             "frontend", "backend", "fullstack", "full stack", "full-stack", "react", "angular",
             "vue", ".net", "java ", "python", "php ", "nodejs", "node.js", "kotlin", "swift",
             "android", "ios ", "mobil geliştir", "web geliştir", "uygulama geliştir"]),

        ("Veri Bilimi & Yapay Zeka",
            ["veri bilimi", "data science", "machine learning", "makine öğren", "yapay zeka",
             "artificial intelligence", "deep learning", "nlp", "data analyst", "veri analiz",
             "bi ", "business intelligence", "power bi", "tableau", "data engineer"]),

        ("DevOps & Cloud",
            ["devops", "cloud", "aws", "azure", "gcp", "kubernetes", "docker", "ci/cd",
             "site reliability", "sre ", "altyapı", "infrastructure", "sistem yönetici",
             "linux yönetici", "network yönetici"]),

        ("Siber Güvenlik",
            ["siber güvenlik", "cybersecurity", "güvenlik uzmanı", "pentest", "soc analist",
             "bilgi güvenliği", "network security", "information security"]),

        ("Sistem & Ağ Yönetimi",
            ["sistem uzmanı", "sistem yönetici", "network admin", "ağ yönetici", "it uzman",
             "bilişim uzman", "teknik altyapı", "server", "helpdesk", "it destek"]),

        ("Teknik Destek",
            ["teknik destek", "technical support", "çağrı merkezi teknik", "it support",
             "servis teknisyen", "teknik servis", "field support"]),

        ("UI/UX & Tasarım",
            ["ui/ux", "ux designer", "ui designer", "kullanıcı deneyim", "arayüz tasarım",
             "grafik tasarım", "graphic design", "creative", "illüstrasyon", "motion"]),

        ("Muhasebe",
            ["muhasebe", "accountant", "mali müşavir", "finansal kontrolör", "ön muhasebe",
             "muhasebeci", "accounting", "vergi", "bütçe uzman"]),

        ("Finans & Bankacılık",
            ["finans", "finance", "bankacılık", "banking", "kredi analiz", "risk analiz",
             "portföy", "yatırım", "investment", "hazine", "sigortacılık"]),

        ("İnsan Kaynakları",
            ["insan kaynakları", "human resources", "hr ", "işe alım", "recruitment",
             "talent acquisition", "ik uzman", "organizasyon gelişim", "özlük"]),

        ("Satış & Satış Temsilcisi",
            ["satış danışman", "satış temsilci", "satış müdür", "satış uzman", "account manager",
             "account executive", "business development", "satış geliştir", "saha satış",
             "kurumsal satış", "perakende satış"]),

        ("Müşteri Hizmetleri",
            ["müşteri hizmet", "müşteri temsilci", "customer service", "customer success",
             "customer support", "çağrı merkezi", "call center", "müşteri ilişki"]),

        ("Pazarlama & Marka",
            ["pazarlama", "marketing", "marka", "brand", "reklam", "reklam", "pr ", "halkla ilişki",
             "campaign", "kampanya", "growth", "crm uzman"]),

        ("Dijital Pazarlama",
            ["dijital pazarlama", "digital marketing", "seo", "sem", "sosyal medya uzman",
             "social media", "content", "içerik uzman", "email marketing", "performance market",
             "google ads", "facebook ads", "influencer"]),

        ("E-ticaret",
            ["e-ticaret", "eticaret", "e-commerce", "ecommerce", "marketplace", "amazon",
             "trendyol", "hepsiburada", "n11", "online satış"]),

        ("Lojistik & Tedarik Zinciri",
            ["lojistik", "logistics", "tedarik zinciri", "supply chain", "satın alma", "purchasing",
             "depo", "warehouse", "ambar", "forklift", "sevkiyat", "gümrük", "ihracat", "ithalat",
             "nakliye", "kurye", "dağıtım"]),

        ("Üretim & İmalat",
            ["üretim", "imalat", "production", "manufacturing", "fabrika", "kalite kontrol",
             "quality", "operatör", "işçi", "kaynakçı", "tornacı", "operasyon"]),

        ("İnşaat & İnşaat Mühendisliği",
            ["inşaat", "construction", "şantiye", "proje yönetici", "mimar", "statik",
             "yapı denetim", "tesisat", "elektrik tesisat"]),

        ("Makine Mühendisliği",
            ["makine mühendis", "mechanical engineer", "mekanik", "otomotiv mühendis",
             "imalat mühendis", "endüstriyel tasarım", "cam", "solidworks", "autocad"]),

        ("Elektrik & Elektronik Mühendisliği",
            ["elektrik mühendis", "elektronik mühendis", "electrical engineer", "elektronik",
             "otomasyon", "plc", "scada", "gömülü sistem", "embedded"]),

        ("Endüstri Mühendisliği",
            ["endüstri mühendis", "industrial engineer", "süreç iyileştirme", "lean",
             "six sigma", "operasyon yönetici", "verimlilik"]),

        ("Kimya Mühendisliği",
            ["kimya mühendis", "chemical engineer", "kimyager", "ar-ge kimya", "boya",
             "petrokimya", "polimer"]),

        ("Biyomedikal Mühendisliği",
            ["biyomedikal", "biomedical", "tıbbi cihaz", "medikal mühendis"]),

        ("Öğretmenlik",
            ["öğretmen", "teacher", "eğitmen", "akademisyen", "instructor",
             "okul", "anaokulu", "ilkokul", "lise öğretmen"]),

        ("Özel Ders & Eğitmenlik",
            ["özel ders", "kurs", "eğitim uzman", "kursiyer", "koçluk", "coaching"]),

        ("Doktor & Uzman Hekim",
            ["doktor", "hekim", "uzman hekim", "pratisyen", "cerrah", "psikiyatr",
             "nörolog", "kardiyolog", "ortopedi", "dahiliye"]),

        ("Hemşire & Sağlık Personeli",
            ["hemşire", "nurse", "ebe", "sağlık personel", "paramedik", "acil tıp teknisyen",
             "laborant", "radyoloji teknisyen", "fizyoterapist", "diyetisyen"]),

        ("Eczacılık",
            ["eczacı", "pharmacist", "eczane", "pharmacy"]),

        ("Diş Hekimliği",
            ["diş hekimi", "dentist", "dişçi", "diş teknisyen", "ortodonti"]),

        ("Psikoloji & Danışmanlık",
            ["psikolog", "psychologist", "danışman", "counselor", "terapi",
             "sosyal hizmet uzmanı"]),

        ("Hukuk & Avukatlık",
            ["avukat", "lawyer", "hukuk müşavir", "hukuk uzman", "hakim", "savcı",
             "patent uzman", "compliance"]),

        ("Turizm & Otelcilik",
            ["turizm", "otel", "hotel", "resepsiyon", "reservation", "konaklama",
             "misafir", "concierge", "bellboy", "tur rehber"]),

        ("Aşçılık & Mutfak",
            ["aşçı", "chef", "cook", "mutfak", "pastane", "pasta", "fırın", "barista",
             "garson", "waiter", "servis eleman"]),

        ("Garsonluk & Servis",
            ["garson", "waiter", "servis eleman", "hostess", "kasiyer", "bar"]),

        ("Perakende & Mağazacılık",
            ["mağaza müdür", "mağaza yönetici", "satış asistan", "kasiyer", "vitrin",
             "perakende", "retail", "mağaza eleman"]),

        ("Sosyal Medya Yönetimi",
            ["sosyal medya yönetici", "social media manager", "community manager",
             "instagram", "tiktok içerik"]),

        ("Grafik Tasarım",
            ["grafik tasarımcı", "graphic designer", "illüstratör", "görsel tasarım"]),

        ("Fotoğrafçılık & Video",
            ["fotoğrafçı", "photographer", "video editör", "kameraman", "video prodüksiyon"]),

        ("Gazetecilik & İçerik",
            ["gazeteci", "journalist", "muhabir", "editör", "içerik yazarı", "copywriter",
             "redaktör", "yazar"]),

        ("Moda & Tekstil",
            ["moda", "fashion", "tekstil", "giyim", "konfeksiyon", "dikiş", "tasarımcı moda"]),

        ("Mimarlık & İç Mimarlık",
            ["mimar", "architect", "iç mimar", "interior", "peyzaj"]),

        ("Staj & Yeni Mezun",
            ["staj", "intern", "stajyer", "yeni mezun", "fresh graduate", "junior"]),

        ("Part-time & Yarı Zamanlı",
            ["part-time", "part time", "yarı zamanlı", "yarı-zamanlı", "esnek çalışma"]),

        ("Uzaktan Çalışma",
            ["uzaktan", "remote", "home office", "evden çalışma"]),

        ("Şoför & Kurye",
            ["şoför", "driver", "sürücü", "kurye", "dağıtıcı", "tır", "kamyon"]),

        ("Çocuk Gelişimi & Bakım",
            ["çocuk gelişim", "bebek bakım", "kreş", "bakıcı", "nanny"]),

        ("Kamu & Devlet",
            ["kamu", "devlet", "belediye", "bakanlık", "personel alım", "memur",
             "kadrolu", "engelli personel", "sözleşmeli personel"]),

        ("Askerlik & Güvenlik",
            ["güvenlik görevli", "security", "muhafız", "jandarma", "polis"]),

        ("Çağrı Merkezi",
            ["çağrı merkezi", "call center", "operatör", "müşteri temsilci çağrı"]),

        ("İdari İşler & Sekreterlik",
            ["sekreter", "secretary", "asistan", "ofis", "idari", "administrative",
             "yönetici asistanı", "büro"]),

        ("Genel Müdürlük & Üst Yönetim",
            ["genel müdür", "ceo", "cto", "cfo", "direktör", "vp ", "vice president",
             "üst yönetim", "yönetim kurulu"]),

        ("Akademik & Araştırma",
            ["araştırmacı", "researcher", "ar-ge", "r&d", "akademik", "doktora",
             "proje araştırma"]),

        ("Denetim & Vergi",
            ["denetçi", "auditor", "iç denetim", "internal audit", "vergi danışman",
             "bağımsız denetim"]),

        ("Depo & Ambar",
            ["depo eleman", "ambar", "stok", "forklift operatör", "depo görevli"]),

        ("Freelance",
            ["freelance", "serbest", "proje bazlı"]),
    ];

    public static string Classify(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return "Diğer";

        var lower = title.ToLower(System.Globalization.CultureInfo.GetCultureInfo("tr-TR"));

        foreach (var (category, keywords) in Rules)
            foreach (var kw in keywords)
                if (lower.Contains(kw))
                    return category;

        return "Diğer";
    }
}

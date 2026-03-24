namespace IsArama.Web.Helpers;

public static class DateHelper
{
    public static string ToRelative(DateTime date)
    {
        var diff = DateTime.UtcNow - date;

        if (diff.TotalMinutes < 60)
            return $"{(int)diff.TotalMinutes} dakika önce";
        if (diff.TotalHours < 24)
            return $"{(int)diff.TotalHours} saat önce";
        if (diff.TotalDays < 2)
            return "Dün";
        if (diff.TotalDays < 7)
            return $"{(int)diff.TotalDays} gün önce";
        if (diff.TotalDays < 14)
            return "1 hafta önce";
        if (diff.TotalDays < 30)
            return $"{(int)(diff.TotalDays / 7)} hafta önce";
        if (diff.TotalDays < 60)
            return "1 ay önce";
        if (diff.TotalDays < 365)
            return $"{(int)(diff.TotalDays / 30)} ay önce";

        return $"{(int)(diff.TotalDays / 365)} yıl önce";
    }
}

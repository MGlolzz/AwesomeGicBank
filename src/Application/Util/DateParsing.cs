namespace Application.Util;

public static class DateParsing
{
    public static bool TryParseYyyyMmDd(string s, out DateOnly date) =>
        DateOnly.TryParseExact(s, "yyyyMMdd", null,
            System.Globalization.DateTimeStyles.None, out date);

    public static bool TryParseYyyyMm(string s, out int year, out int month)
    {
        year = month = 0;
        if (s is { Length: 6 } && int.TryParse(s[..4], out year) && int.TryParse(s[4..], out month))
            return month is >= 1 and <= 12;
        return false;
    }
}

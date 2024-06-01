namespace FindMyVolunteer.Extensions {
  public static class TimeExtension {
    public static uint SecondsSinceMillenium(this DateTime dt) {
      return (uint)(dt - new DateTime(2000, 1, 1)).TotalSeconds;
    }
    public static string ToHex(this long value, int length) {
      return value.ToString("X").PadLeft(length, '0');
    }
    public static string ToHex(this int value, int length) {
      return value.ToString("X").PadLeft(length, '0');
    }
    public static string ToHex(this uint value, int length) {
      return value.ToString("X").PadLeft(length, '0');
    }
    public static string ToHex(this byte value, int length) {
      return value.ToString("X").PadLeft(length, '0');
    }
  }
}

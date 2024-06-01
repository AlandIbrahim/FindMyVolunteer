namespace FindMyVolunteer.Data.Types {
  [Flags]
  public enum Languages: byte {
    Kurdish = 1,
    Arabic = 2,
    English = 4,
    Persian = 8,
    Turkish = 16,
    Other = 32
  }
  public enum TokenType: byte {
    EmailConfirmation = 1,
    PasswordReset = 2
  }
  public enum City: byte {
    Sulaymaniyah = 1,
    Ranya = 2,
    Chamchamal = 3,
    Kalar = 4,
    Penjwin = 5,
    Qaladze = 6,
    Darbandikhan = 7,
    Koya = 8,
    SaidSadiq = 9,
    Takiya = 10,
    Arbat = 11,
    Kifri = 12,
    Khanaqin = 13,
    Mandali = 14,
    Jalawla = 15,
    Saadiya = 16,
    Zakho = 17,
    Piramagrun = 18,
    Amedi = 19,
    Dohuk = 20,
    Erbil = 21,
    Halabja = 22,
    Kirkuk = 23,
    TuzKhurmatu = 24,
    Ronkaw = 25, //"ڕۆنکاو" by Bawan
  }
  public enum EventStatus: byte {
    Pending = 1,
    Ongoing = 2,
    Finished = 3,
    Cancelled = 4
  }
}
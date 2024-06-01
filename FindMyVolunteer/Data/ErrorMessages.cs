namespace FindMyVolunteer.Data {
  public static class ErrorMessages {
    public static readonly Dictionary<string, string[]> UserNotFound= new() {{ "UID", new string[] { "User not found" } }};
    public static readonly Dictionary<string, string[]> InvalidPassword = new() { { "Password", new string[] { "Invalid password" } }};
  }
}

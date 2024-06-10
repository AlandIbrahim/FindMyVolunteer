using FindMyVolunteer.Data.Types;

namespace FindMyVolunteer.Engines {
  public static class Translator {
    public static string EventStatus(DateTime enrollmentDeadline, DateTime startTime, DateTime endTime, bool enrolled = false, bool attended = false, bool cancelled = false, bool attendeesMaxed = false) {
      if(cancelled) return "Cancelled"; // check if cancelled
      if(startTime < DateTime.Now) // check if event has started
        if(endTime < DateTime.Now) { // check if event has ended
          return enrolled ? attended ? "Ended(Attended)" : "Ended(Missed)" : "Ended"; // check if the volunteer enrolled, and if they attended
        } else return "Live";
      if(enrolled) return "Upcoming(Enrolled)"; // check if the volunteer enrolled
      return enrollmentDeadline < DateTime.Now ? "Upcoming(Enrollment deadline Passed)" : attendeesMaxed ? "Upcoming(Full)" : "Upcoming"; //check if the enrollment deadline has passed, and if the event is full
    }
    public static string EventStatus(DateTime enrollmentDeadline, DateTime startTime, DateTime endTime, bool cancelled = false, bool attendeesMaxed = false) {
      if(cancelled) return "Cancelled";
      if(startTime < DateTime.Now)
        return endTime < DateTime.Now ? "Ended" : "Live";
      if(enrollmentDeadline < DateTime.Now) return "Upcoming(Enrollment deadline Passed)";
      return attendeesMaxed ? "Upcoming(Full)" : "Upcoming";
    }
    public static string EventStatus(DateTime startTime, DateTime endTime, bool attended = false, bool cancelled = false) {
      if(cancelled) return "Cancelled";
      return startTime < DateTime.Now ? endTime < DateTime.Now ? attended ? "Ended(Attended)" : "Ended(Missed)" : "Live" : "Upcoming";
    }
    public static string EventStatus(DateTime startTime, DateTime endTime, bool cancelled = false) {
      if(cancelled) return "Cancelled";
      return startTime < DateTime.Now ? endTime < DateTime.Now ? "Live" : "Ended" : "Upcoming";
    }
    /// <summary>
    /// Translates a comma separated string of languages to a Languages flag enum.
    /// </summary>
    /// <param name="langStr"></param>
    /// <returns></returns>
    public static Languages FromString(string langStr) {
      string[] langs = langStr.Split(',');
      Languages languages = 0;
      foreach(string lang in langs) {
        if(Enum.TryParse(lang, out Languages language)) languages |= language;
      }
      return languages;
    }
  }
}

namespace FindMyVolunteer.Data.DataTransfer
{
    public class LoginResultDTO
    {
        public int Uid { get; set; }
        public bool IsOrg { get; set; }
        public Dictionary<string, string[]> Errors { get; set; } = new();
    }
}

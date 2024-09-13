namespace SporttiporssiAPI.Models
{
    public class GroupRegisterRequest
    {
        public string Password { get; set; }
        public string Email { get; set; }
        public string Serie { get; set; }
        public FantasyGroup FantasyGroup { get; set; }
    }
}

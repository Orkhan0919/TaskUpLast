namespace TaskUp.ViewModels.Admin
{
    public class BoardAdminVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string BoardType { get; set; }
        public string OwnerEmail { get; set; }
        public int MemberCount { get; set; }
        public int TaskCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string JoinCode { get; set; }
        public bool IsPrivate { get; set; }
    }
}
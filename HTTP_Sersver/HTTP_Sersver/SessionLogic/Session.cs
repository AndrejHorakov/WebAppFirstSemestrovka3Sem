namespace NetConsoleApp.SessionLogic;

public class Session
{
    public readonly Guid Id;
    public readonly int UserId;
    public readonly string Nickname;
    public readonly DateTime CreationTime;

    public Session(Guid id, int accountId, string nickname, DateTime created)
    {
        Id = id;
        UserId = accountId;
        Nickname = nickname;
        CreationTime = created;
    }
}
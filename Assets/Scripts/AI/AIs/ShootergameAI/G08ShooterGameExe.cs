public class G08ShooterGameExe : AIPlayer
{
    public override string PlayerName => "G08ShooterGameExe";
    public override IThinker Thinker => thinker;

    private IThinker thinker;
    public override void Setup()
    {
        base.Awake();
        thinker = new G08ShooterGameExeThinker();
    }
}

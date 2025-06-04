namespace SystemHooksPOC;

public delegate void GameReadyForRegistrationHandler();

public class Bus
{
    public event GameReadyForRegistrationHandler GameReadyForRegistration;

    public void TriggerGameReadyForRegistration()
    {
        GameReadyForRegistration?.Invoke();
    }
    
}

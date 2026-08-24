namespace SafehouseDesk
{
    // Anything you can look at and press E on.
    public interface IInteractable
    {
        string Prompt(PlayerInteractor p);  // text shown when looked at ("" = none)
        void Interact(PlayerInteractor p);  // called on E
    }
}

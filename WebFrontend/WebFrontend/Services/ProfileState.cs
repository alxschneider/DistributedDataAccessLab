using WebFrontend.Models;

namespace WebFrontend.Services;

public class ProfileState
{
    public Customer? CurrentCustomer { get; private set; }

    public bool IsAdmin => CurrentCustomer == null;
    public bool IsBuyer => CurrentCustomer?.Role == "Buyer";
    public bool IsSeller => CurrentCustomer?.Role == "Seller";
    public bool HasProfile => CurrentCustomer != null;
    public int CurrentId => CurrentCustomer?.Id ?? 0;

    public event Action? OnChange;
    public event Action? OnCustomersChanged;

    public void SetCustomer(Customer? customer)
    {
        CurrentCustomer = customer;
        OnChange?.Invoke();
    }

    public void GoAdmin()
    {
        CurrentCustomer = null;
        OnChange?.Invoke();
    }

    public void NotifyCustomersChanged()
    {
        OnCustomersChanged?.Invoke();
    }
}

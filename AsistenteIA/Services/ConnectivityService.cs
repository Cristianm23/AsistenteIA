namespace AsistenteIA.Services;

public class ConnectivityService
{
    public bool HasInternet() =>
        Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
}
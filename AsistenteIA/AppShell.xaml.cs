using AsistenteIA.Views;

namespace AsistenteIA;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Ruta para la pantalla de detalle (no está en el TabBar)
        Routing.RegisterRoute("detalle", typeof(DetailPage));
    }
}
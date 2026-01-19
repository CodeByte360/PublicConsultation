using MudBlazor;

namespace PublicConsultation.BlazorServer.Theme;

public static class AppTheme
{
    public static MudTheme DefaultTheme = new MudTheme()
    {
        PaletteLight = new PaletteLight()
        {
            Primary = "#1E3C72",       // Deep Royal Blue
            Secondary = "#2A5298",     // Lighter Blue/Teal mix
            Success = "#00C853",       // Material Green A700
            Info = "#2196F3",
            Warning = "#FF9800",
            Error = "#F44336",
            AppbarBackground = "#1E3C72",
            Background = "#F5F5F5",
            Surface = "#FFFFFF",
            DrawerBackground = "#FFFFFF",
            DrawerText = "rgba(0,0,0, 0.7)",
            TextPrimary = "rgba(0,0,0, 0.87)",
            TextSecondary = "rgba(0,0,0, 0.54)"
        },
        PaletteDark = new PaletteDark()
        {
            Primary = "#4D8AF0",       // Bright Professional Blue
            Secondary = "#26A69A",     // Professional Teal
            Success = "#00C853",
            Info = "#2196F3",
            Warning = "#FF9800",
            Error = "#F44336",
            AppbarBackground = "#1A2634",
            Background = "#0D1117",
            Surface = "#161B22",
            DrawerBackground = "#161B22",
            DrawerText = "rgba(255,255,255, 0.7)",
            TextPrimary = "rgba(255,255,255, 0.9)",
            TextSecondary = "rgba(255,255,255, 0.6)"
        },
        Typography = new Typography()
        {
            Default = new Default()
            {
                FontFamily = new[] { "Segoe UI", "Helvetica", "Arial", "sans-serif" }
            },
            H1 = new H1() { FontWeight = 300, FontSize = "6rem", LineHeight = 1.167 },
            H2 = new H2() { FontWeight = 300, FontSize = "3.75rem", LineHeight = 1.2 },
            H3 = new H3() { FontWeight = 400, FontSize = "3rem", LineHeight = 1.167 },
            H4 = new H4() { FontWeight = 400, FontSize = "2.125rem", LineHeight = 1.235 },
            H5 = new H5() { FontWeight = 400, FontSize = "1.5rem", LineHeight = 1.334 },
            H6 = new H6() { FontWeight = 500, FontSize = "1.25rem", LineHeight = 1.6 },
        },
        LayoutProperties = new LayoutProperties()
        {
            DrawerWidthLeft = "260px",
            DrawerWidthRight = "300px"
        }
    };
}

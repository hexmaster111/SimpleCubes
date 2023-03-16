using static SDL2.SDL;

public static class SDLExtentions
{
    public static SDL_Color ToSdlColor(this System.Drawing.Color color)
    {
        return new SDL_Color()
        {
            r = color.R,
            g = color.G,
            b = color.B,
            a = color.A
        };
    }
}



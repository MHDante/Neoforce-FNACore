using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomShane.Neoforce.Controls
{
    /// <summary>
    /// Provides a basic Software cursor
    /// </summary>
    public class Cursor
    {
        private Texture2D cursorTexture;

        public Texture2D CursorTexture
        {
            get { return cursorTexture; }
            set { cursorTexture = value; }
        }

        internal Color[] pixels;

        private Vector2 hotspot;
        private int width;
        private int height;

        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        public Vector2 HotSpot
        {
            get { return hotspot; }
            set { hotspot = value; }
        }

        public Cursor(Color[] pixels, int width, int height, int hotspotX, int hotspotY)
        {
            this.pixels = pixels;
            this.hotspot = new Vector2((float)hotspotX / width, (float)hotspotY / height);
            this.width = width;
            this.height = height;
        }

        public void LoadTextureIfNeeded(GraphicsDevice gDevice)
        {
            if (CursorTexture != null) return;
            var tex = new Texture2D(gDevice, Width, Height);
            tex.SetData(pixels);
            CursorTexture = tex;
        }
    }
}

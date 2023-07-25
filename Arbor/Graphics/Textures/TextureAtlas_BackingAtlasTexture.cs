using System.Runtime.CompilerServices;
using Arbor.Utils;
using Veldrid;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace Arbor.Graphics.Textures;

public partial class TextureAtlas
{
    private class BackingAtlasTexture : Texture
    {
        private readonly int padding;

        private readonly Rectangle atlasBounds;

        private readonly Texture parent;

        private static readonly RgbaFloat initialisation_colour = RgbaFloat.White;

        public BackingAtlasTexture(DevicePipeline pipeline, int width, int height, int padding = 0)
            : this(pipeline.CreateTexture(width, height, initialisation_colour)!)
        {
            this.padding = padding;
            atlasBounds = new Rectangle(0, 0, (int)Width, (int)Height);
        }

        private BackingAtlasTexture(Texture parent)
            : base(parent)
        {
            this.parent = parent;
            IsAtlasTexture = parent.IsAtlasTexture = true;
        }

        internal override void SetData(ITextureUpload upload)
        {
            var middleBounds = upload.Bounds;

            if (middleBounds.IsEmpty || middleBounds.Width * middleBounds.Height > upload.Data.Length)
            {
                base.SetData(upload);
                return;
            }

            var actualPadding = padding / (1 << 1);
            var data = upload.Data;
            
            uploadCornerPadding(data, middleBounds, actualPadding, false);
            uploadHorizontalPadding(data, middleBounds, actualPadding, false);
            uploadVerticalPadding(data, middleBounds, actualPadding, false);
            
            base.SetData(upload);
        }

        private void uploadVerticalPadding(ReadOnlySpan<Rgba32> upload, Rectangle middleBounds, int actualPadding, bool fillOpaque)
        {
            var sideBoundsArray = new[]
            {
                RectangleExtensions.Intersect(new Rectangle(middleBounds.X, middleBounds.Y - actualPadding, middleBounds.Width, actualPadding), atlasBounds),
                RectangleExtensions.Intersect(new Rectangle(middleBounds.X, middleBounds.Y + middleBounds.Height, middleBounds.Width, actualPadding), atlasBounds),
            };
            
            var sideIndices = new[]
            {
                0,
                (middleBounds.Height - 1) * middleBounds.Width
            };

            for (int i = 0; i < 2; ++i)
            {
                var sideBounds = sideBoundsArray[i];
                
                if (sideBounds.IsEmpty)
                    continue;
                
                var allTransparent = true;
                var index = sideIndices[i];
                
                var sideUpload = new MemoryAllocatorTextureUpload(sideBounds.Width, sideBounds.Height) { Bounds = sideBounds };
                var data = sideUpload.RawData;

                for (int y = 0; y < sideBounds.Height; ++y)
                {
                    for (int x = 0; x < sideBounds.Width; ++x)
                    {
                        var pixel = upload[index + x];
                        allTransparent &= checkEdgeRgb(pixel);
                        
                        transferBorderPixel(ref data[y * sideBounds.Width + x], pixel, fillOpaque);
                    }
                }

                if (!allTransparent)
                {
                    base.SetData(sideUpload);
                }
            }
        }
        
        private void uploadHorizontalPadding(ReadOnlySpan<Rgba32> upload, Rectangle middleBounds, int actualPadding, bool fillOpaque)
        {
            var sideBoundsArray = new[]
            {
                RectangleExtensions.Intersect(new Rectangle(middleBounds.X - actualPadding, middleBounds.Y, actualPadding, middleBounds.Height), atlasBounds),
                RectangleExtensions.Intersect(new Rectangle(middleBounds.X + middleBounds.Width, middleBounds.Y, actualPadding, middleBounds.Height), atlasBounds),
            };

            var sideIndices = new[]
            {
                0,
                middleBounds.Width - 1
            };

            for (int i = 0; i < 2; ++i)
            {
                var sideBounds = sideBoundsArray[i];
                
                if (sideBounds.IsEmpty)
                    continue;

                var allTransparent = true;
                var index = sideIndices[i];
                
                var sideUpload = new MemoryAllocatorTextureUpload(sideBounds.Width, sideBounds.Height) { Bounds = sideBounds };
                var data = sideUpload.RawData;

                var stride = middleBounds.Width;

                for (int y = 0; y < sideBounds.Height; ++y)
                {
                    for (int x = 0; x < sideBounds.Width; ++x)
                    {
                        var pixel = upload[index + y * stride];
                        allTransparent &= checkEdgeRgb(pixel);
                        transferBorderPixel(ref data[y * sideBounds.Width + x], pixel, fillOpaque);
                    }
                }

                // Only upload border padding if the border isn't completely transparent.
                if (!allTransparent)
                {
                    base.SetData(sideUpload);
                }
            }
        }

        private void uploadCornerPadding(ReadOnlySpan<Rgba32> upload, Rectangle middleBounds, int actualPadding, bool fillOpaque)
        {
            var cornerBoundsArray = new[]
            {
                RectangleExtensions.Intersect(new Rectangle(middleBounds.X - actualPadding, middleBounds.Y - actualPadding, actualPadding, actualPadding), atlasBounds),
                RectangleExtensions.Intersect(new Rectangle(middleBounds.X + middleBounds.Width, middleBounds.Y - actualPadding, actualPadding, actualPadding), atlasBounds),
                RectangleExtensions.Intersect(new Rectangle(middleBounds.X - actualPadding, middleBounds.Y + middleBounds.Height, actualPadding, actualPadding), atlasBounds),
                RectangleExtensions.Intersect(new Rectangle(middleBounds.X + middleBounds.Width, middleBounds.Y + middleBounds.Height, actualPadding, actualPadding), atlasBounds),
            };

            var cornerIndices = new[]
            {
                0, // top left
                middleBounds.Width - 1, // top right
                (middleBounds.Height - 1) * middleBounds.Width, // bottom left
                (middleBounds.Height - 1) * middleBounds.Width + middleBounds.Width - 1, // bottom right
            };

            for (var i = 0; i < 4; ++i)
            {
                var cornerBounds = cornerBoundsArray[i];
                var nCornerPixels = cornerBounds.Width * cornerBounds.Height;
                var pixel = upload[cornerIndices[i]];
                
                // only upload if we have a non-zero size and if the colour isn't already transparent white
                if (nCornerPixels > 0 && !checkEdgeRgb(pixel))
                {
                    var cornerUpload = new MemoryAllocatorTextureUpload(cornerBounds.Width, cornerBounds.Height) { Bounds = cornerBounds };
                    var data = cornerUpload.RawData;

                    for (int j = 0; j < nCornerPixels; j++)
                        transferBorderPixel(ref data[j], pixel, fillOpaque);
                    
                    base.SetData(cornerUpload);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void transferBorderPixel(ref Rgba32 dest, Rgba32 source, bool fillOpaque)
        {
            dest.R = source.R;
            dest.G = source.G;
            dest.B = source.B;
            dest.A = fillOpaque ? source.A : (byte) 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool checkEdgeRgb(Rgba32 cornerPixel)
            => cornerPixel.R == initialisation_colour.R
               && cornerPixel.G == initialisation_colour.G
               && cornerPixel.B == initialisation_colour.B;
    }
}

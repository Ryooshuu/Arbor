using System.Numerics;
using Arbor.Elements;
using Arbor.Graphics;
using Arbor.Graphics.Textures;
using Arbor.IO.Stores;
using Arbor.Platform;
using Arbor.Resources;
using ImGuiNET;

namespace Arbor;

public abstract class Game : Scene
{
    public static ResourceStore<byte[]> Resources { get; private set; } = null!;
    public static TextureStore Textures { get; private set; } = null!;
    public static NativeStorage Storage { get; internal set; } = null!;
    public static FontStore Fonts { get; internal set; } = null!;

    private FontStore localFonts = null!;


    internal override void LoadInternal()
    {
        Storage = new NativeStorage(Path.GetFullPath(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Window.Title)));

        Resources = new ResourceStore<byte[]>();
        Resources.AddStore(new DllResourceStore(ArborResources.ResourcesAssembly));

        Textures = new TextureStore(Pipeline, new TextureLoaderStore(new NamespacedResourceStore<byte[]>(Resources, @"Textures")));

        var cacheStorage = Storage.GetStorageForDirectory("cached");
        var fontStorage = cacheStorage.GetStorageForDirectory("fonts");

        Fonts = new FontStore(Pipeline, useAtlas: true, cacheStorage: fontStorage);

        Fonts.AddStore(localFonts = new FontStore(Pipeline, useAtlas: false));

        // Roboto
        addFont(localFonts, Resources, @"Fonts/Roboto/Roboto-Regular");
        addFont(localFonts, Resources, @"Fonts/Roboto/Roboto-RegularItalic");
        addFont(localFonts, Resources, @"Fonts/Roboto/Roboto-Bold");
        addFont(localFonts, Resources, @"Fonts/Roboto/Roboto-BoldItalic");

        base.LoadInternal();
    }

    public void AddFont(ResourceStore<byte[]> store, string? assetName = null, FontStore? target = null)
        => addFont(target ?? Fonts, store, assetName);

    private void addFont(FontStore target, ResourceStore<byte[]> store, string? assetName = null)
        => target.AddTextureSource(new RawCachingGlyphStore(store, assetName, new TextureLoaderStore(store)));

    protected override void Draw(DrawPipeline pipeline)
    {
        if (ImGui.IsKeyDown(ImGuiKey.F1) && ImGui.Begin("Texture Visualiser"))
        {
            ImGui.Text("Atlases textures");
            if (ImGui.BeginTable("Atlases", 3))
            {
                ImGui.TableNextRow();
                var column = 0;

                var textures = pipeline.DevicePipeline.GetAllTextures();

                foreach (var texture in textures)
                {
                    if (!texture.IsAtlasTexture)
                        continue;

                    if (column == 3)
                    {
                        ImGui.TableNextRow();
                        column = 0;
                    }

                    ImGui.TableSetColumnIndex(column++);

                    var binding = Window.Igr.GetOrCreateImGuiBinding(pipeline.DevicePipeline.Factory, texture.NativeTexture);
                    ImGui.Image(binding, new Vector2(128));

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text($"{texture.AssetName} | {texture.LookupKey}");
                        ImGui.Text("");
                        ImGui.Text($"Size: {texture.Width}x{texture.Height}");
                        ImGui.Text($"Displayable size: {texture.DisplayWidth}x{texture.DisplayHeight}");
                        ImGui.Image(binding, new Vector2(texture.Width / 2f, texture.Height / 2f));
                        ImGui.EndTooltip();
                    }
                }

                ImGui.EndTable();
            }

            ImGui.Text("Textures");
            if (ImGui.BeginTable("Textures", 3))
            {
                ImGui.TableNextRow();
                var column = 0;

                var textures = pipeline.DevicePipeline.GetAllTextures();

                foreach (var texture in textures)
                {
                    if (texture.IsAtlasTexture)
                        continue;
                
                    if (column == 3)
                    {
                        ImGui.TableNextRow();
                        column = 0;
                    }
                    
                    ImGui.TableSetColumnIndex(column++);
                    
                    var binding = Window.Igr.GetOrCreateImGuiBinding(pipeline.DevicePipeline.Factory, texture.NativeTexture);
                    ImGui.Image(binding, new Vector2(128));
                
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text($"{texture.AssetName} | {texture.LookupKey}");
                        ImGui.Text("");
                        ImGui.Text($"Size: {texture.Width}x{texture.Height}");
                        ImGui.Text($"Displayable size: {texture.DisplayWidth}x{texture.DisplayHeight}");
                        ImGui.Image(binding, new Vector2(texture.Width / 2f, texture.Height / 2f));
                        ImGui.EndTooltip();
                    }
                }

                ImGui.EndTable();
            }

            ImGui.End();
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        Fonts.Dispose();
        localFonts.Dispose();
    }
}

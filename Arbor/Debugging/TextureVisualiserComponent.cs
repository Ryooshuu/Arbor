using System.Numerics;
using Arbor.Elements;
using Arbor.Graphics;
using ImGuiNET;

namespace Arbor.Debugging;

public class TextureVisualiserComponent : IDebugComponent
{
    public Entity Entity { get; set; } = null!;

    public void Draw(DrawPipeline pipeline)
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
}

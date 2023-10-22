using Arbor.Elements;
using Arbor.Graphics;
using Arbor.Statistics;
using Arbor.Timing;
using ImGuiNET;

namespace Arbor.Debugging;

public class StatisticsComponent : IDebugComponent
{
    public Entity Entity { get; set; } = null!;

    public void Draw(DrawPipeline pipeline)
    {
        if (ImGui.IsKeyDown(ImGuiKey.F2) && ImGui.Begin("Frame Statistics"))
        {
            var drawCalls = FrameStatistics.COUNTERS[(int) StatisticsCounterType.DrawCalls];
            var vertices = FrameStatistics.COUNTERS[(int) StatisticsCounterType.Vertices];
            var indices = FrameStatistics.COUNTERS[(int) StatisticsCounterType.Indices];
            var buffers = FrameStatistics.COUNTERS[(int) StatisticsCounterType.Buffers];
            var vertexMemUsage = FrameStatistics.COUNTERS[(int) StatisticsCounterType.VertexMemUsage];
            var indexMemUsage = FrameStatistics.COUNTERS[(int) StatisticsCounterType.IndexMemUsage];
            
            ImGui.Text($"Draw calls: {drawCalls}");
            ImGui.Text($"Vertices: {vertices}");
            ImGui.Text($"Indices: {indices}");
            ImGui.Text($"Buffers: {buffers}");
            ImGui.Text($"Vertex Memory Usage: {vertexMemUsage}");
            ImGui.Text($"Index Memory Usage: {indexMemUsage}");
            ImGui.Text($"Time: {Entity.Clock.CurrentTime:n}ms");

            var framedClock = Entity.Clock as FramedClock;
            ImGui.Text($"Delta Time: {framedClock?.TimeInfo.Elapsed:n}ms");
            ImGui.Text("");
            ImGui.Text($"{framedClock}");
            
            ImGui.End();
        }
    }
}

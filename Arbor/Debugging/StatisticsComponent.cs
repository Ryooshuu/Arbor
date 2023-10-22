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
            ImGui.Text($"Vertex Memory Usage: {formatSizes(vertexMemUsage)}");
            ImGui.Text($"Index Memory Usage: {formatSizes(indexMemUsage)}");
            ImGui.Text($"Time: {Entity.Clock.CurrentTime:n}ms");

            var framedClock = Entity.Clock as FramedClock;
            ImGui.Text($"Delta Time: {framedClock?.TimeInfo.Elapsed:n}ms");
            ImGui.Text("");
            ImGui.Text($"{framedClock}");
            
            ImGui.End();
        }
    }

    private readonly string[] sizeSuffixes = { "B", "KB", "MB", "GB" };
    
    private string formatSizes(long value, int decimalPlaces = 2)
    {
        if (value < 0)
            return "-" + formatSizes(-value, decimalPlaces);

        var i = 0;
        var dValue = (decimal) value;

        while (Math.Round(dValue, decimalPlaces) >= 1000)
        {
            dValue /= 1024;
            i++;
        }

        return string.Format("{0:n" + decimalPlaces + "} {1}", dValue, sizeSuffixes[i]);
    }
}

namespace Arbor.Graphics;

public class DrawStack
{
    private readonly LinkedList<DrawCommand> stack = new LinkedList<DrawCommand>();

    public void Push(DrawCommand command)
    {
        stack.AddLast(command);
    }

    public DrawCommand Pop()
    {
        var first = stack.First();
        stack.RemoveFirst();

        return first;
    }

    public bool TryPop(out DrawCommand command)
    {
        if (!stack.Any())
        {
            command = null!;
            return false;
        }

        command = Pop();
        return true;
    }
}

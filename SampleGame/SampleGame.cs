using Arbor;
using Arbor.Elements;
using Arbor.Elements.Components;

namespace SampleGame;

public class SampleGame : Game
{
    private Entity? entity;
    
    protected override void Load()
    {
        entity = CreateEntity();
        entity.AddComponent<Sprite>();
        entity.AddComponent<Transform>();
        
        var sprite = entity.GetComponent<Sprite>()!;
        sprite.Texture = new(@"D:\Projects\Projects\Arbor\SampleGame\Textures\10-wKGO250UVi.png");
        sprite.Colour = new(1, 0.5f, 0.5f, 1);
        
        var transform = entity.GetComponent<Transform>()!;
        transform.Position = new(20);
        transform.Rotation -= 10;
    }
}

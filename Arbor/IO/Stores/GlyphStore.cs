using System.Collections.Concurrent;
using System.Diagnostics;
using Arbor.Graphics.Textures;
using Arbor.Text;
using Arbor.Utils;
using SharpFNT;
using SixLabors.ImageSharp.Advanced;

namespace Arbor.IO.Stores;

public class GlyphStore : IResourceStore<TextureUpload>, IGlyphStore
{
    protected readonly string? AssetName;

    protected readonly IResourceStore<TextureUpload>? TextureLoader;
    
    public string? FontName { get; }

    public float? Baseline => Font?.Common.Base;

    protected readonly ResourceStore<byte[]> Store;

    protected BitmapFont? Font => completionSource.Task.GetResultSafely();

    private readonly TaskCompletionSource<BitmapFont?> completionSource = new TaskCompletionSource<BitmapFont?>();

    private static readonly ConcurrentDictionary<string, BitmapFont> font_cache = new ConcurrentDictionary<string, BitmapFont>();

    public GlyphStore(ResourceStore<byte[]> store, string? assetName = null, IResourceStore<TextureUpload>? textureLoader = null)
    {
        Store = new ResourceStore<byte[]>(store);
        
        Store.AddExtension("fnt");
        Store.AddExtension("bin");

        AssetName = assetName;
        TextureLoader = textureLoader;

        FontName = assetName?.Split('/').Last();
    }

    private Task? fontLoadTask;
    
    public Task LoadFontAsync()
        => fontLoadTask ??= Task.Factory.StartNew(() =>
        {
            try
            {
                BitmapFont font;

                using (var s = Store.GetStream($@"{AssetName}"))
                {
                    var hash = s.ComputeMD5Hash();

                    if (font_cache.TryGetValue(hash, out font))
                    {
                        Console.WriteLine($"Cached font load for {AssetName}");
                    }
                    else
                    {
                        font_cache.TryAdd(hash, font = BitmapFont.FromStream(s, FormatHint.Binary, false));
                    }
                }
                
                completionSource.SetResult(font);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Couldn't load font asset from {AssetName}.");
                Console.WriteLine(e);
                completionSource.SetResult(null);
                throw;
            }
        }, TaskCreationOptions.PreferFairness);

    public bool HasGlyph(char c)
        => Font?.Characters.ContainsKey(c) == true;

    protected virtual TextureUpload? GetPageImage(int page)
    {
        if (TextureLoader != null)
            return TextureLoader.Get(GetFilenameForPage(page));

        using (var stream = Store.GetStream(GetFilenameForPage(page)))
            return stream != null ? new TextureUpload(stream) : null;
    }

    protected string GetFilenameForPage(int page)
    {
        Debug.Assert(Font != null);
        return $@"{AssetName}_{page.ToString().PadLeft((Font.Pages.Count - 1).ToString().Length, '0')}.png";
    }

    public CharacterGlyph? Get(char character)
    {
        if (Font == null)
            return null;
        
        Debug.Assert(Baseline != null);

        var bmCharacter = Font.GetCharacter(character);
        return new CharacterGlyph(character, bmCharacter.XOffset, bmCharacter.YOffset, bmCharacter.XAdvance, Baseline.Value, this);
    }

    public int GetKerning(char left, char right)
        => Font?.GetKerningAmount(left, right) ?? 0;

    Task<CharacterGlyph?> IResourceStore<CharacterGlyph>.GetAsync(string name, CancellationToken cancellationToken)
        => Task.Run(() => ((IGlyphStore) this).Get(name[0]), cancellationToken);

    CharacterGlyph? IResourceStore<CharacterGlyph>.Get(string name)
        => Get(name[0]);

    public TextureUpload? Get(string name)
    {
        if (Font == null)
            return null;

        if (name.Length > 1 && !name.StartsWith($@"{FontName}/", StringComparison.Ordinal))
            return null;

        return Font.Characters.TryGetValue(name.Last(), out var c) ? LoadCharacter(c) : null;
    }

    public virtual async Task<TextureUpload?> GetAsync(string name, CancellationToken cancellationToken = default)
    {
        if (name.Length > 1 && !name.StartsWith($@"{FontName}/", StringComparison.Ordinal))
            return null;

        var bmFont = await completionSource.Task.ConfigureAwait(false)!;

        return bmFont.Characters.TryGetValue(name.Last(), out var c)
                   ? LoadCharacter(c)
                   : null;
    }

    protected int LoadedGlyphCount;

    protected virtual TextureUpload LoadCharacter(Character character)
    {
        var page = GetPageImage(character.Page);
        LoadedGlyphCount++;

        var image = new Image<Rgba32>(SixLabors.ImageSharp.Configuration.Default, character.Width, character.Height);
        var source = page.Data;

        var readableHeight = (int)Math.Min(character.Height, page.Height - character.Y);
        var readableWidth = (int)Math.Min(character.Width, page.Width - character.X);

        for (var y = 0; y < character.Height; y++)
        {
            var pixelRowMemory = image.DangerousGetPixelRowMemory(y);
            var readOffset = (int)((character.Y + y) * page.Width + character.X);

            for (var x = 0; x < character.Width; x++)
                pixelRowMemory.Span[x] = x < readableWidth && y < readableHeight ? source[readOffset + x] : new Rgba32(255, 255, 255, 0);
        }

        return new TextureUpload(image);
    }

    public Stream GetStream(string name)
        => throw new NotSupportedException();

    public IEnumerable<string> GetAvailableResources()
        => Font?.Characters.Keys.Select(k => $"{FontName}/{(char) k}") ?? Enumerable.Empty<string>();

    #region IDisposable Support

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    #endregion
}

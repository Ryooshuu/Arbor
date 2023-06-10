layout (location = 0) in vec2 in_Uv;
layout (location = 1) in vec4 in_Colour;

layout (location = 0) out vec4 out_Colour;

layout (set = 1, binding = 1) uniform texture2D in_Texture;
layout (set = 1, binding = 2) uniform sampler in_TextureSampler;

void main() {
    out_Colour = texture(sampler2D(in_Texture, in_TextureSampler), in_Uv) * in_Colour;
}
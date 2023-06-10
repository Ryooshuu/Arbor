layout (location = 0) in vec2 in_Position;
layout (location = 1) in vec4 in_Uv;
layout (location = 2) in vec4 in_Colour;

layout (location = 0) out vec4 fs_Uv;
layout (location = 1) out vec4 fs_Colour;

void main() {
    gl_Position = g_PixelMatrix * vec4(in_Position, 0.0, 1.0);
    fs_Uv = in_Uv;
    fs_Colour = in_Colour;
}
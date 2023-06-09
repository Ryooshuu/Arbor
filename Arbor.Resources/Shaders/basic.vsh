#version 450

layout (location = 0) in vec2 in_Position;
layout (location = 1) in vec4 in_Colour;

layout (location = 0) out vec4 fs_Colour;

void main() {
    gl_Position = vec4(in_Position, 0.0, 1.0);
    fs_Colour = in_Colour;
}
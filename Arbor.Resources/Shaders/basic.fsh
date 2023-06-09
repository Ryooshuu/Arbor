#version 450

layout(location = 0) in vec4 colour;
layout(location = 0) out vec4 out_Colour;

void main() {
    out_Colour = colour;
}
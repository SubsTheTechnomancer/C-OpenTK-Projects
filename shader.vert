#version 330 core
in vec3 aPosition;
out vec3 vertColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    vertColor = (aPosition/2.0f)+0.5f;
    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
}
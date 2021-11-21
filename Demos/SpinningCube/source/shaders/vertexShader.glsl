precision mediump float;

attribute vec3 position;
attribute vec3 color;

varying vec3 vColor;
uniform mat4 matrix;

void main() {
    vColor = color;
    gl_Position = matrix * vec4(position, 1);
}
